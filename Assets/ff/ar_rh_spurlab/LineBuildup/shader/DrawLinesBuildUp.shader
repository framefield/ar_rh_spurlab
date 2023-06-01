Shader "framefield/DrawLinesBuildup" 
{
    Properties
    {
        MainColor("MainColor", Color)  = (1,1,1,1)
        MainTex("MainTex", 2D) = "white" {}
        LineWidth("LineWidth", float) = 0.039
        ShrinkWithDistance("ShrinkWithDistance", float) = 0.550
        TransitionProgress("TransitionProgress", float) = 1.0
        VisibleRange("VisibleRange", float) = 1.0
        FogDistance("FogDistance", float) = 117.0
        FogBias("FogBias", float) = 4.700
        FogColor("FogColor", Color) = (0.17, 0.17, 0.17, 1.0)
        
        NoiseAmount("NoiseAmount", float) = 10
        NoiseVariation("NoiseVariation", float) = 0.01
        NoiseFrequency("NoiseFrequency", float) = 3
        NoisePhase("NoisePhase", float) = 3
    }
    
    SubShader
    {
        Tags {"RenderType"="Transparent"}

        Lighting Off
	    ZWrite Off
        ZTest LEqual
	    AlphaTest Off
	    Cull Off
	    Blend SrcAlpha OneMinusSrcAlpha 
        
        Pass
        {
            CGPROGRAM
 
            #pragma vertex vsMain
            #pragma fragment psMain
            #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
            #pragma target 4.5
 
            #include "Assets/ff/ar_rh_spurlab/LineBuildup/shader/point.hlsl"
            #include "Assets/ff/ar_rh_spurlab/LineBuildup/shader/noise.hlsl"
            

            static const float3 Corners[] = 
            {
              float3(0, -1, 0),
              float3(1, -1, 0), 
              float3(1,  1, 0), 
              float3(1,  1, 0), 
              float3(0,  1, 0), 
              float3(0, -1, 0),  
            };
            

            struct psInput
            {
                float4 position : SV_POSITION;
                float4 color : COLOR;
                float2 texCoord : TEXCOORD;
                float fog: FOG;
            };

            struct fragOutput {
                fixed4 color: SV_Target;
            };
            
            float4 MainColor;
            sampler2D MainTex;
            float4x4 _ObjectToWorld;
            
            float LineWidth;
            float ShrinkWithDistance;
            float TransitionProgress;
            float VisibleRange;
            float4 FogColor;
            float FogDistance;
            float FogBias;

            float NoiseAmount;
            float NoiseVariation;
            float NoiseFrequency;
            float NoisePhase;
            


            uint SegmentCount;
            StructuredBuffer<Point> Points;

            // we have to build our own mvp in the shader, draw procedural does not care 
            // https://forum.unity.com/threads/how-to-get-model-matrix-into-graphics-drawprocedural-custom-shader.489304/#post-3535125
            inline float4 PointToClipPos(in float3 pos)
            {
                return mul(UNITY_MATRIX_VP, mul(_ObjectToWorld, float4(pos, 1.0)));
            }
            
            psInput vsMain(uint id: SV_VertexID)
            {                
                psInput output;
                float discardFactor = 1;
                
                float4 aspect = float4(_ScreenParams.x / _ScreenParams.y,1,1,1);
                int quadIndex = id % 6;
                uint particleId = id / 6;
                float3 cornerFactors = Corners[quadIndex];
                
                Point pointAA = Points[ particleId<1 ? 0: particleId-1];
                Point pointA = Points[particleId];
                Point pointB = Points[particleId+1];
                Point pointBB = Points[particleId > SegmentCount-2 ? SegmentCount-2: particleId+2];

                float phase = NoisePhase + _Time.x;
                
                float3 posAA = AddNoise(particleId-1, pointAA.position,   NoiseAmount, NoiseVariation, phase, NoiseFrequency);
                float3 posA  = AddNoise(particleId, pointA.position,    NoiseAmount, NoiseVariation, phase, NoiseFrequency);
                float3 posB  = AddNoise(particleId+1, pointB.position,  NoiseAmount, NoiseVariation, phase, NoiseFrequency);
                float3 posBB = AddNoise(particleId+2, pointBB.position, NoiseAmount, NoiseVariation, phase, NoiseFrequency);
                
                float3 posInObject = cornerFactors.x < 0.5
                    ? posA
                    : posB;

                float tz= 0;
                float4 aaInScreen  = PointToClipPos(float4(posAA,1)) * aspect;
                aaInScreen /= aaInScreen.w;
                if(aaInScreen.z < tz)
                    discardFactor = 0;
                
                
                float4 aInScreen  = PointToClipPos(float4(posA,1)) * aspect;
                aInScreen /= aInScreen.w;
                if(aInScreen.z < tz)
                    discardFactor = 0;
                

                
                float4 bInScreen  = PointToClipPos(float4(posB,1)) * aspect;
                bInScreen /= bInScreen.w;
                if(bInScreen.z < tz)
                    discardFactor = 0;
                
                
                float4 bbInScreen  = PointToClipPos(float4(posBB,1)) * aspect;
                bbInScreen /= bbInScreen.w;
                if(bbInScreen.z < tz)
                    discardFactor = 0;
                

                float3 direction = (aInScreen - bInScreen).xyz;
                float3 directionA = particleId > 0 
                                        ? (aaInScreen - aInScreen).xyz
                                        : direction;
                float3 directionB = particleId < SegmentCount- 1
                                        ? (bInScreen - bbInScreen).xyz
                                        : direction;

                float3 normal =  normalize( cross(direction, float3(0,0,1))); 
                float3 normalA =  normalize( cross(directionA, float3(0,0,1))); 
                float3 normalB =  normalize( cross(directionB, float3(0,0,1))); 
                if(isnan(pointAA.w) || pointAA.w < 0.01) {
                    normalA =normal;
                }
                if(isnan(pointBB.w) || pointAA.w < 0.01) {
                    normalB =normal;
                }

                float3 neighbourNormal = lerp(normalA, normalB, cornerFactors.x);
                float3 meterNormal = (normal + neighbourNormal) / 2;
                float4 pos = lerp(aInScreen, bInScreen, cornerFactors.x);


                float4 posInCamSpace = mul(mul(float4(posInObject,1), _ObjectToWorld), unity_CameraProjection);
                posInCamSpace.xyz /= posInCamSpace.w;
                posInCamSpace.w = 1;


                float wAtPoint = lerp( pointA.w  , pointB.w , cornerFactors.x);

                // Buildup transition

                if(!isnan(wAtPoint)) 
                {        
                    output.texCoord = float2(wAtPoint - TransitionProgress, cornerFactors.y /2 +0.5);
                }

                float thickness = !isnan(wAtPoint) ? LineWidth * discardFactor * lerp(1, 1/(posInCamSpace.z), ShrinkWithDistance) : wAtPoint;
                float miter = dot(-meterNormal, normal);
                pos+= cornerFactors.y * 0.1f * thickness * float4(meterNormal,0) / clamp(miter, -2.0,-0.13) ;   

                output.position = pos / aspect;
                
                float3 n = cornerFactors.x < 0.5 
                    ? cross(posA - posAA, posA - posB)
                    : cross(posB - posA, posB - posBB);
                n =normalize(n);

                output.fog = pow(saturate(-posInCamSpace.z/FogDistance), FogBias);
                output.color.rgb =  MainColor.rgb;

                output.color.a = MainColor.a;
                return output;    
            }

            fragOutput psMain(psInput input)
            {
                fragOutput output;
                //float u = (input.texCoord.x + VisibleRange);
                //float4 imgColor = tex2D(MainTex, float2(u, input.texCoord.y)) * MainColor;

                float4 imgColor =1;
                float f1 = saturate((input.texCoord.x + VisibleRange) * 1000  );
                float f2 = 1-saturate( input.texCoord.x * 1000);
                float t = f1*f2;
                // if(t < 0.01)
                //     discard;
                
                //output.color = float4(t,0,0,1);
                //return output;

                output.color = float4(lerp(imgColor.rgb, FogColor.rgb, input.fog * FogColor.a), imgColor.a * t);

                return output;
            }

 
            ENDCG
        }
    }
}
