﻿// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "framefield/UnlitTextureZWriteFront" {

Properties {
    _MainTex ("Base (RGB)", 2D) = "white" {}
    _tintAmount ("Tint Amount", Range (0, 1)) = 0
    _tintColor ("Tint Color", Color) = (0.5, 0.5, 0.5 ,0.5)
}
    
SubShader {
    Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
    Fog {Mode Off} 
    LOD 100
        
    Lighting Off
    ZWrite On
    AlphaTest Off
    ZTest LEqual
    Offset -0.01, -0.01
    //Cull Off
    Blend SrcAlpha OneMinusSrcAlpha 
    
    Pass {
        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
        #pragma fragmentoption ARB_precision_hint_fastest
        

        sampler2D _MainTex;
        float4 _MainTex_ST;
        float4 _tintColor;
        float _tintAmount;

        #include "UnityCG.cginc"


        struct appdata_t
        {
            float4 vertex : POSITION;
            float2 uv : TEXCOORD0;
            fixed4 color : COLOR;
        };

        struct v2f
        {
            float2 uv : TEXCOORD0;
            //UNITY_FOG_COORDS(1)
            float4 vertex : SV_POSITION;
            float4 color : COLOR;
        };

        v2f vert (appdata_t v)
        {
            v2f o;
            o.vertex = UnityObjectToClipPos(v.vertex);
            o.uv = TRANSFORM_TEX(v.uv, _MainTex);
            o.color = v.color;
            return o;
        }

        float4 frag(v2f i) : COLOR 
        
        {			
            float u = i.uv.x;
            float v = i.uv.y;
            fixed4 c =tex2D(_MainTex, float2(u,v)).rgba;
            fixed4 tintedColor = lerp( c * i.color, _tintColor, _tintAmount);
            tintedColor.a *= c.a;
            return tintedColor;            
        } 
              
        ENDCG
    }
} 
    
FallBack "Unlit/Texture"
}
