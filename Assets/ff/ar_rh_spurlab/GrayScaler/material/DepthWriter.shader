Shader "framefield/DepthOnly" 
{
 
	SubShader { 
		Tags {"Queue" = "Geometry"  "ForceNoShadowCasting"="True"  }
            
		Lighting Off
		ColorMask 0
		ZWrite On
 
		Pass {}
	}
}
