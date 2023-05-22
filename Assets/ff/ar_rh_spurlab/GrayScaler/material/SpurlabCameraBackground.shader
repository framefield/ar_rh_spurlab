Shader "framefield/SpurlabCameraBackground"
{
    Properties
    {
        _textureY ("TextureY", 2D) = "white" {}
        _textureCbCr ("TextureCbCr", 2D) = "black" {}
        _HumanStencil ("HumanStencil", 2D) = "black" {}
        _HumanDepth ("HumanDepth", 2D) = "black" {}
        _EnvironmentDepth ("EnvironmentDepth", 2D) = "black" {}

        _baseGrayScaleStrength ("BaseGrayScaleStrength", range(0, 1)) = 1
        _cameraViewportScale ("CameraViewportWidthInRadians", range(0.1, 6)) = 4

        _fadeOutColor ("FadeOutColor", Color) = (0,0,0,1)
        _maxFadeOut ("MaxFadeout", range(0, 1)) = 0.2

        [KeywordEnum(GuideToPortal, InPortal)] _Mode ("Mode", Float) = 0 
        
        [KeywordEnum(None, HumanSegmentation, Depth)] _ForceArKitFeature ("Force", Float) = 0 
        [KeywordEnum(None, AngularDifference)] _Debug ("Debug", Float) = 0 
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Background"
            "RenderType" = "Background"
            "ForceNoShadowCasting" = "True"
        }

        Pass
        {
            Cull Off
            ZTest Always
            ZWrite On
            Lighting Off
            LOD 100
            Tags
            {
                "LightMode" = "Always"
            }


            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile_local __ ARKIT_BACKGROUND_URP
            #pragma multi_compile_local __ _DEBUG_NONE _DEBUG_ANGULARDIFFERENCE
            #pragma multi_compile_local __ _MODE_GUIDETOPORTAL _MODE_INPORTAL
            #pragma multi_compile_local __ ARKIT_HUMAN_SEGMENTATION_ENABLED ARKIT_ENVIRONMENT_DEPTH_ENABLED
            #pragma multi_compile_local __ _FORCEARKITFEATURE_NONE _FORCEARKITFEATURE_HUMANSEGMENTATION _FORCEARKITFEATURE_DEPTH


#if ARKIT_BACKGROUND_URP

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

            #define ARKIT_TEXTURE2D_HALF(texture) TEXTURE2D(texture)
            #define ARKIT_SAMPLER_HALF(sampler) SAMPLER(sampler)
            #define ARKIT_TEXTURE2D_FLOAT(texture) TEXTURE2D(texture)
            #define ARKIT_SAMPLER_FLOAT(sampler) SAMPLER(sampler)
            #define ARKIT_SAMPLE_TEXTURE2D(texture,sampler,texcoord) SAMPLE_TEXTURE2D(texture,sampler,texcoord)

#else // Legacy RP

            #include "UnityCG.cginc"

            #define real4 half4
            #define real4x4 half4x4
            #define TransformObjectToHClip UnityObjectToClipPos
            #define FastSRGBToLinear GammaToLinearSpace

            #define ARKIT_TEXTURE2D_HALF(texture) UNITY_DECLARE_TEX2D_HALF(texture)
            #define ARKIT_SAMPLER_HALF(sampler)
            #define ARKIT_TEXTURE2D_FLOAT(texture) UNITY_DECLARE_TEX2D_FLOAT(texture)
            #define ARKIT_SAMPLER_FLOAT(sampler)
            #define ARKIT_SAMPLE_TEXTURE2D(texture,sampler,texcoord) UNITY_SAMPLE_TEX2D(texture,texcoord)

#endif


            struct appdata
            {
                float3 position : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 position : SV_POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct fragment_output
            {
                real4 color : SV_Target;
                float depth : SV_Depth;
            };


            CBUFFER_START(UnityARFoundationPerFrame)
            // Device display transform is provided by the AR Foundation camera background renderer.
            float4x4 _UnityDisplayTransform;
            float _UnityCameraForwardScale;
            CBUFFER_END


            v2f vert (appdata v)
            {
                // Transform the position from object space to clip space.
                float4 position = TransformObjectToHClip(v.position);

                // Remap the texture coordinates based on the device rotation.
                float2 texcoord = mul(float3(v.texcoord, 1.0f), _UnityDisplayTransform).xy;

                v2f o;
                o.position = position;
                o.texcoord = texcoord;
                return o;
            }


            CBUFFER_START(ARKitColorTransformations)
            static const real4x4 s_YCbCrToSRGB = real4x4(
                real4(1.0h,  0.0000h,  1.4020h, -0.7010h),
                real4(1.0h, -0.3441h, -0.7141h,  0.5291h),
                real4(1.0h,  1.7720h,  0.0000h, -0.8860h),
                real4(0.0h,  0.0000h,  0.0000h,  1.0000h)
            );
            CBUFFER_END


            inline float ConvertDistanceToDepth(float d)
            {
                // Account for scale
                d = _UnityCameraForwardScale > 0.0 ? _UnityCameraForwardScale * d : d;

                // Clip any distances smaller than the near clip plane, and compute the depth value from the distance.
                return (d < _ProjectionParams.y) ? 0.0f : ((1.0f / _ZBufferParams.z) * ((1.0f / d) - _ZBufferParams.w));
            }


            ARKIT_TEXTURE2D_HALF(_textureY);
            ARKIT_SAMPLER_HALF(sampler_textureY);
            ARKIT_TEXTURE2D_HALF(_textureCbCr);
            ARKIT_SAMPLER_HALF(sampler_textureCbCr);
#if ARKIT_ENVIRONMENT_DEPTH_ENABLED || _FORCEARKITFEATURE_DEPTH
            ARKIT_TEXTURE2D_FLOAT(_EnvironmentDepth);
            ARKIT_SAMPLER_FLOAT(sampler_EnvironmentDepth);
#elif ARKIT_HUMAN_SEGMENTATION_ENABLED || _FORCEARKITFEATURE_HUMANSEGMENTATION
            ARKIT_TEXTURE2D_HALF(_HumanStencil);
            ARKIT_SAMPLER_HALF(sampler_HumanStencil);
            ARKIT_TEXTURE2D_FLOAT(_HumanDepth);
            ARKIT_SAMPLER_FLOAT(sampler_HumanDepth);
#endif // ARKIT_HUMAN_SEGMENTATION_ENABLED


            half _baseGrayScaleStrength;
            
            half _maxFadeOut;
            half _fadeOutColor;
            
            half _cameraViewportWidthInRadians;
            half _cameraViewportHeightInRadians;
            half _cameraViewportRadius;
            half4 _CameraForward;
            half4 _CameraRight;
            half4 _CameraUp;
            float4x4 _PointsOfInterest;
            

            fragment_output frag (v2f i)
            {
                // Sample the video textures (in YCbCr).
                real4 ycbcr = real4(ARKIT_SAMPLE_TEXTURE2D(_textureY, sampler_textureY, i.texcoord).r,
                                    ARKIT_SAMPLE_TEXTURE2D(_textureCbCr, sampler_textureCbCr, i.texcoord).rg,
                                    1.0h);

                // Convert from YCbCr to sRGB.
                real4 videoColor = mul(s_YCbCrToSRGB, ycbcr);

#if !UNITY_COLORSPACE_GAMMA
                // If rendering in linear color space, convert from sRGB to RGB.
                videoColor.xyz = FastSRGBToLinear(videoColor.xyz);
#endif // !UNITY_COLORSPACE_GAMMA

                // Assume the background depth is the back of the depth clipping volume.
                float depthValue = 0.0f;
                float humanFound = 0.0f;
                
#if ARKIT_ENVIRONMENT_DEPTH_ENABLED || _FORCEARKITFEATURE_DEPTH
                // Sample the environment depth (in meters).
                float envDistance = ARKIT_SAMPLE_TEXTURE2D(_EnvironmentDepth, sampler_EnvironmentDepth, i.texcoord).r;

                // Convert the distance to depth. 
                depthValue = ConvertDistanceToDepth(envDistance);
#elif ARKIT_HUMAN_SEGMENTATION_ENABLED || _FORCEARKITFEATURE_HUMANSEGMENTATION
                // Check the human stencil, and skip non-human pixels.
                if (ARKIT_SAMPLE_TEXTURE2D(_HumanStencil, sampler_HumanStencil, i.texcoord).r > 0.5h)
                {
                    humanFound = 1.0f;
                    // Sample the human depth (in meters).
                    float humanDistance = ARKIT_SAMPLE_TEXTURE2D(_HumanDepth, sampler_HumanDepth, i.texcoord).r;

                    // Convert the distance to depth.
                    depthValue = ConvertDistanceToDepth(humanDistance);
                }
#endif // ARKIT_HUMAN_SEGMENTATION_ENABLED

                fragment_output o;

                const half4 grayScaleVideo = dot(videoColor.xyz, float3(0.3, 0.59, 0.11));
                const half grayScaleAmount = max(0, min(1, _baseGrayScaleStrength) - humanFound);
                const half4 mixedVideo = lerp(videoColor, grayScaleVideo, grayScaleAmount);

                half inverseFadeOutStrength = 1;

                
#if _MODE_GUIDETOPORTAL
                const half3 viewDir = _CameraForward +
                    (i.texcoord.x - 0.5) * _CameraRight +
                        (i.texcoord.y - 0.5) * _CameraUp;

                
                if (_PointsOfInterest._m03 > 0)
                {
                    const half3 poiPos = half3(_PointsOfInterest._m00, _PointsOfInterest._m01, _PointsOfInterest._m02);
                    const half3 poiToCameraDir = normalize(poiPos.xyz - _WorldSpaceCameraPos.xyz);
                    const half angleDistance = (1 + dot(poiToCameraDir, viewDir)) / 2.0f;
                    inverseFadeOutStrength *= angleDistance;
                }
                if (_PointsOfInterest._m13 > 0)
                {
                    const half3 poiPos = half3(_PointsOfInterest._m10, _PointsOfInterest._m11, _PointsOfInterest._m12);
                    const half3 poiToCameraDir = normalize(poiPos.xyz - _WorldSpaceCameraPos.xyz);
                    const half angleDistance = (1 + dot(poiToCameraDir, viewDir)) / 2.0f;
                    inverseFadeOutStrength *= angleDistance;
                }
                if (_PointsOfInterest._m23 > 0)
                {
                    const half3 poiPos = half3(_PointsOfInterest._m20, _PointsOfInterest._m21, _PointsOfInterest._m22);
                    const half3 poiToCameraDir = normalize(poiPos.xyz - _WorldSpaceCameraPos.xyz);
                    const half angleDistance = (1 + dot(poiToCameraDir, viewDir)) / 2.0f;
                    inverseFadeOutStrength *= angleDistance;
                }
                if (_PointsOfInterest._m33 > 0)
                {
                    const half3 poiPos = half3(_PointsOfInterest._m30, _PointsOfInterest._m31, _PointsOfInterest._m32);
                    const half3 poiToCameraDir = normalize(poiPos.xyz - _WorldSpaceCameraPos.xyz);
                    const half angleDistance = (1 + dot(poiToCameraDir, viewDir)) / 2.0f;
                    inverseFadeOutStrength *= angleDistance;
                }
#endif
                
                const half4 darkenedMixedVideo = lerp(_fadeOutColor, mixedVideo, max(max(inverseFadeOutStrength, _maxFadeOut), humanFound));
#if _DEBUG_ANGULAR_DIFFERENCE
                if (i.texcoord.x % 0.1 < 0.05 ? i.texcoord.y % 0.1 < 0.05 : i.texcoord.y % 0.1 > 0.05 )
                {
                    o.color = half4(angleDistance, angleDistance, angleDistance, 1);
                }
                else  if (i.texcoord.x % 0.1 < 0.075 ? i.texcoord.y % 0.1 < 0.075 : i.texcoord.y % 0.1 > 0.075 )
                {
                    o.color = half4((viewDir + float3(1,1,1))/2, 1);
                }
                else {
                    o.color = half4((poiToCameraDir + float3(1,1,1))/2, 1);
                }
#else 
                o.color = darkenedMixedVideo;
#endif
                o.depth = depthValue;
                return o;
            }

            ENDHLSL
        }
    }
}
