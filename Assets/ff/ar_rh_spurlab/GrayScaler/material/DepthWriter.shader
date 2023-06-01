Shader "framefield/DepthOnly" 
{
 
	SubShader { 
		Tags {"Queue" = "Geometry-500"  "ForceNoShadowCasting"="True"  }
            
		Lighting Off
		ColorMask 0
		ZWrite On
 
		Pass {}
	}
}
