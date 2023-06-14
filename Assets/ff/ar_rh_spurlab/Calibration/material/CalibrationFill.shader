Shader "framefield/CalibrationFill" {
    Properties {
        _MainColor ("Color", Color) = ( 0, 0, 0, 0 )
		_FadeInCameraDistance ("Fade In Camera Distance", Float) = 10
		_FadeOutCameraDistance ("Fade Out Camera Distance", Float) = 15
    }

    Category {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
        Blend SrcAlpha One
        Cull Off 
        Lighting Off 
        ZWrite Off

        SubShader {
            Pass {

                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma target 2.0
                #pragma multi_compile_particles
                #pragma multi_compile_fog

                #include "UnityCG.cginc"

                fixed4 _MainColor;
				float _FadeInCameraDistance;
				float _FadeOutCameraDistance;

                struct appdata_t {
				    float4 vertex : POSITION;
				    float3 normal : NORMAL;
                };

                struct v2f {
                    float4 vertex : SV_POSITION;
				    float3 normal : TEXCOORD1;
				    float4 worldPos : TEXCOORD2;
				    float4 projPos : TEXCOORD3;
                };

                float3 getCameraPosition()
				{
					#ifdef USING_STEREO_MATRICES
						return lerp(unity_StereoWorldSpaceCameraPos[0], unity_StereoWorldSpaceCameraPos[1], 0.5);
					#endif
					return _WorldSpaceCameraPos;
				}

                v2f vert (appdata_t v)
                {
                    v2f o; 
                    o.vertex = UnityObjectToClipPos(v.vertex);
				    o.normal = UnityObjectToWorldNormal(v.normal);
				    o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				    o.projPos = ComputeScreenPos (o.vertex);
				    o.projPos.z = -mul(UNITY_MATRIX_V, o.worldPos).z;
                    return o;
                }

                fixed4 frag (v2f i) : SV_Target
                {

					float3 cameraPos = getCameraPosition();

                	
					// fade based on distance to camera
					float distanceToCamera = distance(cameraPos.xyz, i.worldPos.xyz);
					float cameraDistanceFade = smoothstep(_FadeOutCameraDistance, _FadeInCameraDistance, distanceToCamera);
                	
                    half4 prev = _MainColor;
                    fixed4 col = lerp(half4(0,0,0,0), prev, saturate(prev.a * cameraDistanceFade));
                    return col;
                }
                ENDCG
            }
        }
    }
}
