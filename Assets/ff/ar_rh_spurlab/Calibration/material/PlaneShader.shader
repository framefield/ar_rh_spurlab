Shader "framefield/PlaneShader"
{
    Properties
    {
        _FloorPattern ("Texture", 2D) = "white" {}
        _WallPattern ("Texture", 2D) = "white" {}
        _TexTintColor("Texture Tint Color", Color) = (1,1,1,1)
        _PlaneColor("Plane Color", Color) = (1,1,1,1)
        
        _isWall("Is Wall", float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 uv2 : TEXCOORD1;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 uv2 : TEXCOORD1;

                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _FloorPattern;
            float4 _FloorPattern_ST;
            
            sampler2D _WallPattern;
            float4 _WallPattern_ST;
            
            fixed4 _TexTintColor;
            fixed4 _PlaneColor;
            
            float _ShortestUVMapping;
            float _isWall = 0;

            v2f vert (appdata v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = _isWall > 0.5 ? TRANSFORM_TEX(v.uv, _WallPattern) : TRANSFORM_TEX(v.uv, _FloorPattern);
                o.uv2 = v.uv2;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                fixed4 col = (_isWall > 0.5 ? tex2D(_WallPattern, i.uv) : tex2D(_FloorPattern, i.uv)) * _TexTintColor;
                col = lerp( _PlaneColor, col, col.a);
                // Fade out from as we pass the edge.
                // uv2.x stores a mapped UV that will be "1" at the beginning of the feathering.
                // We fade until we reach at the edge of the shortest UV mapping.
                // This is the remapped UV value at the vertex.
                // We choose the shorted one so that ll edges will fade out completely.
                // See PlaneMeshVisualizer.cs for more details.
                col.a *=  1-smoothstep(1, _ShortestUVMapping, i.uv2.x);
                return col;
            }
            ENDCG
        }
    }
}
