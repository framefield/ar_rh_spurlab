Shader "framefield/FakeGround"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _Ground ("Ground", Color) = (0.5,0.5,0.5,1)
        _FadeDistance ("Fade Object Distance", Range(0, 50)) = 0.1
        _FloorPlanePosition ("Floor Plane Object Position", Vector) = (0,0,0,0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            float invLerp(float from, float to, float value) {
              return (value - from) / (to - from);
            }
            
            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 objectViewDir: TEXCOORD0;
			    float3 objectPos : TEXCOORD1;
			    float3 objectPlaneNormal : TEXCOORD2;
            };

            float4 _Color;
            float4 _Ground;

            float _FadeDistance;
            float4 _FloorPlanePosition;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);

                float3 objectCameraPos = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos, 1.0)).xyz;

			    o.objectViewDir = v.vertex - objectCameraPos;
			    o.objectPos = v.vertex;
                o.objectPlaneNormal = mul(unity_WorldToObject, float3(0, 1, 0)).xyz;
                return o;
            }


            fixed4 frag(v2f i) : SV_Target
            {
                float3 rayDir = normalize(i.objectViewDir);
			    float3 rayOrigin = i.objectPos;
                float3 planeNormal = normalize(i.objectPlaneNormal);

                if (dot(planeNormal, rayDir) > 0)
                {
                    return _Color;
                }

                float denominator = dot(rayDir, planeNormal);
                float distance = (dot(_FloorPlanePosition - rayOrigin, planeNormal) + 0) / denominator;

                float fade = saturate(invLerp(0, _FadeDistance, distance));

                return lerp(_Ground, _Color, fade);
            }
            ENDCG
        }
    }
}
