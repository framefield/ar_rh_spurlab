using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ff.ar_rh_spurlab.JumpFloodOutline
{
    [ExecuteInEditMode]
    public class JumpFloodOutlineRenderer : MonoBehaviour
    {
        [ColorUsage(true, true)]
        public Color outlineColor = Color.white;

        [Range(0.0f, 1000.0f)]
        public float outlinePixelWidth = 4f;

        // list of all renderer components you want to have outlined as a single silhouette
        public List<Renderer> renderers = new List<Renderer>();

        // hidden reference to ensure shader gets included with builds
        // gets auto-assigned with an OnValidate() function later
        [HideInInspector, SerializeField]
        private Shader _outlineShader;

        // some hidden settings
        private const string ShaderName = "Hidden/JumpFloodOutline";
        private const CameraEvent CameraEvent = UnityEngine.Rendering.CameraEvent.AfterForwardAlpha;

        private const bool UseSeparableAxisMethod = true;

        // shader pass indices
        private const int ShaderPassInteriorStencil = 0;
        private const int ShaderPassSilhouetteBufferFill = 1;
        private const int ShaderPassJfaInit = 2;
        private const int ShaderPassJfaFlood = 3;
        private const int ShaderPassJfaFloodSingleAxis = 4;
        private const int ShaderPassJfaOutline = 5;

        // render texture IDs
        private readonly int _silhouetteBufferID = Shader.PropertyToID("_SilhouetteBuffer");
        private readonly int _nearestPointID = Shader.PropertyToID("_NearestPoint");
        private readonly int _nearestPointPingPongID = Shader.PropertyToID("_NearestPointPingPong");

        // shader properties
        private readonly int _outlineColorID = Shader.PropertyToID("_OutlineColor");
        private readonly int _outlineWidthID = Shader.PropertyToID("_OutlineWidth");
        private readonly int _stepWidthID = Shader.PropertyToID("_StepWidth");
        private readonly int _axisWidthID = Shader.PropertyToID("_AxisWidth");

        // private variables
        private CommandBuffer _cb;
        private Material _outlineMat;
        private Camera _bufferCam;

        private Mesh MeshFromRenderer(Renderer r)
        {
            if (r is SkinnedMeshRenderer)
            {
                return (r as SkinnedMeshRenderer).sharedMesh;
            }
            else if (r is MeshRenderer)
            {
                return r.GetComponent<MeshFilter>().sharedMesh;
            }

            return null;
        }

        private void CreateCommandBuffer(Camera cam)
        {
            if (renderers == null || renderers.Count == 0)
            {
                return;
            }

            if (_cb == null)
            {
                _cb = new CommandBuffer();
                _cb.name = "JumpFloodOutlineRenderer: " + gameObject.name;
            }
            else
            {
                _cb.Clear();
            }

            if (_outlineMat == null)
            {
                _outlineMat = new Material(_outlineShader ? _outlineShader : Shader.Find(ShaderName));
            }

            // do nothing if no outline will be visible
            if (outlineColor.a <= (1f / 255f) || outlinePixelWidth <= 0f)
            {
                _cb.Clear();
                return;
            }

            // support meshes with sub meshes
            // can be from having multiple materials, complex skinning rigs, or a lot of vertices
            var renderersCount = renderers.Count;
            var subMeshCount = new int[renderersCount];

            for (var i = 0; i < renderersCount; i++)
            {
                var mesh = MeshFromRenderer(renderers[i]);
                Debug.Assert(mesh != null, "JumpFloodOutlineRenderer's renderer [" + i + "] is missing a valid mesh.",
                    gameObject);
                if (mesh != null)
                {
                    // assume staticly batched meshes only have one sub mesh
                    if (renderers[i].isPartOfStaticBatch)
                    {
                        subMeshCount[i] = 1; // hack hack hack
                    }
                    else
                    {
                        subMeshCount[i] = mesh.subMeshCount;
                    }
                }
            }

            // render meshes to main buffer for the interior stencil mask
            _cb.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
            for (var i = 0; i < renderersCount; i++)
            {
                for (var m = 0; m < subMeshCount[i]; m++)
                {
                    _cb.DrawRenderer(renderers[i], _outlineMat, m, ShaderPassInteriorStencil);
                }
            }

            // match current quality settings' MSAA settings
            // doesn't check if current camera has MSAA enabled
            // also could just always do MSAA if you so pleased
            var msaa = Mathf.Max(1, QualitySettings.antiAliasing);

            var width = cam.scaledPixelWidth;
            var height = cam.scaledPixelHeight;

            // setup descriptor for silhouette render texture
            var silhouetteRtd = new RenderTextureDescriptor()
            {
                dimension = TextureDimension.Tex2D,
                graphicsFormat = GraphicsFormat.R8_UNorm,

                width = width,
                height = height,

                msaaSamples = msaa,
                depthBufferBits = 0,

                sRGB = false,

                useMipMap = false,
                autoGenerateMips = false
            };

            // create silhouette buffer and assign it as the current render target
            _cb.GetTemporaryRT(_silhouetteBufferID, silhouetteRtd, FilterMode.Point);
            _cb.SetRenderTarget(_silhouetteBufferID);
            _cb.ClearRenderTarget(false, true, Color.clear);

            // render meshes to silhouette buffer
            for (var i = 0; i < renderersCount; i++)
            {
                for (var m = 0; m < subMeshCount[i]; m++)
                {
                    _cb.DrawRenderer(renderers[i], _outlineMat, m, ShaderPassSilhouetteBufferFill);
                }
            }

            // Humus3D wire trick, keep line 1 pixel wide and fade alpha instead of making line smaller
            // slightly nicer looking and no more expensive
            var adjustedOutlineColor = outlineColor;
            adjustedOutlineColor.a *= Mathf.Clamp01(outlinePixelWidth);
            _cb.SetGlobalColor(_outlineColorID, adjustedOutlineColor.linear);
            _cb.SetGlobalFloat(_outlineWidthID, Mathf.Max(1f, outlinePixelWidth));

            // setup descriptor for jump flood render textures
            var jfaRtd = silhouetteRtd;
            jfaRtd.msaaSamples = 1;
            jfaRtd.graphicsFormat = GraphicsFormat.R16G16_SNorm;

            // create jump flood buffers to ping pong between
            _cb.GetTemporaryRT(_nearestPointID, jfaRtd, FilterMode.Point);
            _cb.GetTemporaryRT(_nearestPointPingPongID, jfaRtd, FilterMode.Point);

            // calculate the number of jump flood passes needed for the current outline width
            // + 1.0f to handle half pixel inset of the init pass and antialiasing
            var numMips = Mathf.CeilToInt(Mathf.Log(outlinePixelWidth + 1.0f, 2f));
            var jfaIter = numMips - 1;

            // Alan Wolfe's separable axis JFA - https://www.shadertoy.com/view/Mdy3D3
            if (UseSeparableAxisMethod)
            {
                // jfa init
                _cb.Blit(_silhouetteBufferID, _nearestPointID, _outlineMat, ShaderPassJfaInit);

                // jfa flood passes
                for (var i = jfaIter; i >= 0; i--)
                {
                    // calculate appropriate jump width for each iteration
                    // + 0.5 is just me being cautious to avoid any floating point math rounding errors
                    var stepWidth = Mathf.Pow(2, i) + 0.5f;

                    // the two separable passes, one axis at a time
                    _cb.SetGlobalVector(_axisWidthID, new Vector2(stepWidth, 0f));
                    _cb.Blit(_nearestPointID, _nearestPointPingPongID, _outlineMat, ShaderPassJfaFloodSingleAxis);
                    _cb.SetGlobalVector(_axisWidthID, new Vector2(0f, stepWidth));
                    _cb.Blit(_nearestPointPingPongID, _nearestPointID, _outlineMat, ShaderPassJfaFloodSingleAxis);
                }
            }
            // traditional JFA
            else
            {
                // choose a starting buffer so we always finish on the same buffer
                var startBufferID = (jfaIter % 2 == 0) ? _nearestPointPingPongID : _nearestPointID;

                // jfa init
                _cb.Blit(_silhouetteBufferID, startBufferID, _outlineMat, ShaderPassJfaInit);

                // jfa flood passes
                for (var i = jfaIter; i >= 0; i--)
                {
                    // calculate appropriate jump width for each iteration
                    // + 0.5 is just me being cautious to avoid any floating point math rounding errors
                    _cb.SetGlobalFloat(_stepWidthID, Mathf.Pow(2, i) + 0.5f);

                    // ping pong between buffers
                    if (i % 2 == 1)
                    {
                        _cb.Blit(_nearestPointID, _nearestPointPingPongID, _outlineMat, ShaderPassJfaFlood);
                    }
                    else
                    {
                        _cb.Blit(_nearestPointPingPongID, _nearestPointID, _outlineMat, ShaderPassJfaFlood);
                    }
                }
            }

            // jfa decode & outline render
            _cb.Blit(_nearestPointID, BuiltinRenderTextureType.CameraTarget, _outlineMat, ShaderPassJfaOutline);

            _cb.ReleaseTemporaryRT(_silhouetteBufferID);
            _cb.ReleaseTemporaryRT(_nearestPointID);
            _cb.ReleaseTemporaryRT(_nearestPointPingPongID);
        }

        private void ApplyCommandBuffer(Camera cam)
        {
#if UNITY_EDITOR
            // hack to avoid rendering in the inspector preview window
            if (cam.gameObject.name == "Preview Scene Camera")
            {
                return;
            }
#endif

            if (_bufferCam != null)
            {
                if (_bufferCam == cam)
                {
                    return;
                }
                else
                {
                    RemoveCommandBuffer(cam);
                }
            }

            var planes = GeometryUtility.CalculateFrustumPlanes(cam);

            // skip rendering if none of the renderers are in view
            var visible = false;
            for (var i = 0; i < renderers.Count; i++)
            {
                if (GeometryUtility.TestPlanesAABB(planes, renderers[i].bounds))
                {
                    visible = true;
                    break;
                }
            }

            if (!visible)
            {
                return;
            }

            CreateCommandBuffer(cam);
            if (_cb == null)
            {
                return;
            }

            _bufferCam = cam;
            _bufferCam.AddCommandBuffer(CameraEvent, _cb);
        }

        private void RemoveCommandBuffer(Camera cam)
        {
            if (_bufferCam != null && _cb != null)
            {
                _bufferCam.RemoveCommandBuffer(CameraEvent, _cb);
                _bufferCam = null;
            }
        }

        private void OnEnable()
        {
            Camera.onPreRender += ApplyCommandBuffer;
            Camera.onPostRender += RemoveCommandBuffer;
        }

        private void OnDisable()
        {
            Camera.onPreRender -= ApplyCommandBuffer;
            Camera.onPostRender -= RemoveCommandBuffer;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!_outlineShader)
            {
                _outlineShader = Shader.Find(ShaderName);
            }

            if (renderers != null)
            {
                for (var i = renderers.Count - 1; i > -1; i--)
                {
                    if (renderers[i] == null ||
                        (!(renderers[i] is SkinnedMeshRenderer) && !(renderers[i] is MeshRenderer)))
                    {
                        renderers.RemoveAt(i);
                    }
                    else
                    {
                        var foundDuplicate = false;
                        for (var k = 0; k < i; k++)
                        {
                            if (renderers[i] == renderers[k])
                            {
                                foundDuplicate = true;
                                break;
                            }
                        }

                        if (foundDuplicate)
                        {
                            renderers.RemoveAt(i);
                        }
                    }
                }
            }
        }

        public void FindActiveMeshes()
        {
            Undo.RecordObject(this, "Filling with all active Renderer components");
            var parent = gameObject;
            if (renderers == null)
            {
                return;
            }

            foreach (var r in renderers)
            {
                if (r)
                {
                    parent = r.transform.parent.gameObject;
                    break;
                }
            }


            if (parent != null)
            {
                var skinnedMeshes = parent.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                var meshes = parent.GetComponentsInChildren<MeshRenderer>(true);
                if (skinnedMeshes.Length > 0 || meshes.Length > 0)
                {
                    foreach (var sk in skinnedMeshes)
                    {
                        if (sk.gameObject.activeSelf)
                        {
                            renderers.Add(sk);
                        }
                    }

                    foreach (var mesh in meshes)
                    {
                        if (mesh.gameObject.activeSelf)
                        {
                            renderers.Add(mesh);
                        }
                    }

                    OnValidate();
                }
                else
                {
                    Debug.LogError("No Active Meshes Found");
                }
            }
        }
#endif
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(JumpFloodOutlineRenderer))]
    public class JumpFloodOutlineRendererEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Get Active Children Renderers"))
            {
                var objs = serializedObject.targetObjects;

                foreach (var obj in objs)
                {
                    if (obj is JumpFloodOutlineRenderer jfor)
                    {
                        jfor.FindActiveMeshes();
                    }
                }
            }
        }
    }
#endif
}
