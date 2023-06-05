Shader "framefield/SpurlabCameraBackground"
{
    Properties
    {
        // ios
        _textureY ("TextureY", 2D) = "white" {}
        _textureCbCr ("TextureCbCr", 2D) = "black" {}
        
        // simulation
        _textureSingle ("TextureSingle", 2D) = "white" {}
        
        _HumanStencil ("HumanStencil", 2D) = "black" {}
        _HumanDepth ("HumanDepth", 2D) = "black" {}
        _EnvironmentDepth ("EnvironmentDepth", 2D) = "black" {}
        
        _portalMask ("PortalMask", 2D) = "black" {}
        
        _fadeoutGradient ("FadeOutGradient", 2D) = "black" {}
        
        sphereRadius("sphereRadius", Float) = 10
        degreesPerUv("degreesPerUv", Float) = 90

        [KeywordEnum(NoPortal, GuideToPortal, InPortal)] _Mode ("Mode", Float) = 0 
        
        [KeywordEnum(None, HumanSegmentation, Depth)] _ForceArKitFeature ("Force", Float) = 0 
        [KeywordEnum(None, AngularDifference, PortalMask, Sphere)] _Debug ("Debug", Float) = 0 
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
            #pragma multi_compile_local __ XR_SIMULATION
            #pragma multi_compile_local __ _DEBUG_NONE _DEBUG_ANGULARDIFFERENCE _DEBUG_PORTALMASK _DEBUG_SPHERE
            #pragma multi_compile_local __ _MODE_GUIDETOPORTAL _MODE_INPORTAL _MODE_NOPORTAL
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


            static const float Exponent = 1;
            static const float Brightness = 0;

                
            float hash12(float2 p)
            {
	            float3 p3  = frac(float3(p.xyx) * .1031);
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.x + p3.y) * p3.z);
            }

            float2 hash21(float p)
            {
	            float3 p3 = frac(float3(p,p,p) * float3(.1031, .1030, .0973));
	            p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.xx+p3.yz)*p3.zy);

            }
                
            float4 GetNoiseFromUv(float2 uv) 
            {
                // Animation
                float pxHash = hash12( uv * 431 + 111);

                float sawTime = abs(_Time.x % 100 - 50);    // avoid large numbers because floating point precision
                float t = sawTime * 80 + pxHash;

                
                const float hash1 = hash12(( uv * 431 + (int)t));
                const float hash2 = hash12(( uv * 431 + (int)t+1));
                float4 hash = lerp(hash1,hash2, t % 1);
                float4 noise = float4(hash.xxx,1);

                noise = noise < 0 
                        ? -pow(-noise, Exponent)
                        : pow(noise, Exponent);
                
                noise += Brightness ;
                return noise;
            }

            float4 GetGrainyColor(float2 uv, float4 color, float strength )
            {
                const float4 noise = GetNoiseFromUv(uv) - 0.5;
                const half4 gray = dot(color.rgb, float3(0.3, 0.59, 0.11));                
                //const float gray = (color.r + color.g + color.b)/3;
                const float grainyGray = 1-smoothstep(1,0,gray) + noise * 0.6;
                return  lerp(color,  grainyGray, strength);
            }
            
            struct appdata
            {
                float3 position : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 position : SV_POSITION;
                float2 texcoord : TEXCOORD0;
                float2 rawTexcoord : TEXCOORD1;
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
                
#if XR_SIMULATION
                o.rawTexcoord = texcoord;
#else
                o.rawTexcoord = v.texcoord;
#endif
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

#if XR_SIMULATION
            ARKIT_TEXTURE2D_HALF(_textureSingle);
            ARKIT_SAMPLER_HALF(sampler_textureSingle);
#else
            ARKIT_TEXTURE2D_HALF(_textureY);
            ARKIT_SAMPLER_HALF(sampler_textureY);
            
            ARKIT_TEXTURE2D_HALF(_textureCbCr);
            ARKIT_SAMPLER_HALF(sampler_textureCbCr);
#endif
            
            
#if ARKIT_ENVIRONMENT_DEPTH_ENABLED || _FORCEARKITFEATURE_DEPTH
            ARKIT_TEXTURE2D_FLOAT(_EnvironmentDepth);
            ARKIT_SAMPLER_FLOAT(sampler_EnvironmentDepth);
#elif ARKIT_HUMAN_SEGMENTATION_ENABLED || _FORCEARKITFEATURE_HUMANSEGMENTATION
            ARKIT_TEXTURE2D_HALF(_HumanStencil);
            ARKIT_SAMPLER_HALF(sampler_HumanStencil);
            
            ARKIT_TEXTURE2D_FLOAT(_HumanDepth);
            ARKIT_SAMPLER_FLOAT(sampler_HumanDepth);
#endif // ARKIT_HUMAN_SEGMENTATION_ENABLED
            
            
            ARKIT_TEXTURE2D_FLOAT(_fadeoutGradient);
            ARKIT_SAMPLER_FLOAT(sampler_fadeoutGradient);
            
            ARKIT_TEXTURE2D_HALF(_portalMask);
            ARKIT_SAMPLER_HALF(sampler_portalMask);
            
            float4x4 _cameraTransformMatrix;
            float4x4 _PointsOfInterest;

            half degreesPerUv = 170;
            half sphereRadius = 30;

            fragment_output frag (v2f i)
            {
#if XR_SIMULATION
                real4 videoColor = ARKIT_SAMPLE_TEXTURE2D(_textureSingle, sampler_textureSingle, i.texcoord);
#else
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
#endif                
                
                // Assume the background depth is the back of the depth clipping volume.
                float depthValue = 0.0f;
                float humanMask = 0.0f;
                
                
#if ARKIT_ENVIRONMENT_DEPTH_ENABLED || _FORCEARKITFEATURE_DEPTH
                // Sample the environment depth (in meters).
                float envDistance = ARKIT_SAMPLE_TEXTURE2D(_EnvironmentDepth, sampler_EnvironmentDepth, i.texcoord).r;

                // Convert the distance to depth. 
                depthValue = ConvertDistanceToDepth(envDistance);
#elif ARKIT_HUMAN_SEGMENTATION_ENABLED || _FORCEARKITFEATURE_HUMANSEGMENTATION
                // Check the human stencil, and skip non-human pixels.
                if (ARKIT_SAMPLE_TEXTURE2D(_HumanStencil, sampler_HumanStencil, i.texcoord).r > 0.5h)
                {
                    humanMask = 1.0f;
                    // Sample the human depth (in meters).
                    float humanDistance = ARKIT_SAMPLE_TEXTURE2D(_HumanDepth, sampler_HumanDepth, i.texcoord).r;

                    // Convert the distance to depth.
                    depthValue = ConvertDistanceToDepth(humanDistance);
                }
#endif // ARKIT_HUMAN_SEGMENTATION_ENABLED

                fragment_output o;

#if _MODE_NOPORTAL
                const half4 grayScaleVideo = videoColor;
#else
                const half4 grayScaleVideo = GetGrainyColor(i.texcoord, videoColor, 1);
#endif
                

#if _MODE_GUIDETOPORTAL
                half portalMask = 1 - ARKIT_SAMPLE_TEXTURE2D(_portalMask, sampler_portalMask, i.rawTexcoord).b;
#else
                half portalMask = ARKIT_SAMPLE_TEXTURE2D(_portalMask, sampler_portalMask, i.rawTexcoord).r;
#endif
                
                const half grayScaleAmount = max(0, min(1, 1 - max(humanMask, portalMask)));
                
                half4 mixedVideo = lerp(videoColor, grayScaleVideo, grayScaleAmount);
                
                half3 poiToCameraDir;
                half3 pixelViewDir;
                float inverseFadeOutStrength = 1;
#if _MODE_GUIDETOPORTAL
                if (portalMask > 0.5)
                {
                    const half aspectX = min(1, _ScreenParams.x / _ScreenParams.y);
                    const half aspectY = min(1, _ScreenParams.y / _ScreenParams.x);

                    float2 uvInView = (i.texcoord -0.5 ) * 2;
                    float4 posInView = float4(-uvInView, 0.1,1);
                    
                    float4 posInCam = mul(unity_CameraInvProjection, posInView);
                    posInCam.xyz /= posInCam.w;  // <--- !
                    float4 posInWorld = mul(unity_CameraToWorld, posInCam);
                    posInWorld.xyz /= posInWorld.w;  // <--- !
                    pixelViewDir = normalize( posInWorld.xyz - _WorldSpaceCameraPos.xyz);
                    
                    half3 poiPos =0;
                    if (_PointsOfInterest._m03 > 0)
                    {
                         poiPos = half3(_PointsOfInterest._m00, _PointsOfInterest._m01, _PointsOfInterest._m02);
                    }
                    if (_PointsOfInterest._m13 > 0)
                    {
                         poiPos = half3(_PointsOfInterest._m10, _PointsOfInterest._m11, _PointsOfInterest._m12);
                    }
                    if (_PointsOfInterest._m23 > 0)
                    {
                         poiPos = half3(_PointsOfInterest._m20, _PointsOfInterest._m21, _PointsOfInterest._m22);
                    }
                    if (_PointsOfInterest._m33 > 0)
                    {
                         poiPos = half3(_PointsOfInterest._m30, _PointsOfInterest._m31, _PointsOfInterest._m32);
                    }

                    float distanceToPoi = length(_WorldSpaceCameraPos.xyz - poiPos);
                    float focusSphereFactor = smoothstep(1, 7, distanceToPoi) + 0.5; 
                    
                    poiToCameraDir = normalize(poiPos.xyz - _WorldSpaceCameraPos.xyz);
                    float normalized = (1 + dot(poiToCameraDir, pixelViewDir)) / 2.0f;
                    const float angleDistance = saturate( normalized  * focusSphereFactor );

                    // Animation experiment (too distracting)
                    // float wave = pow(sin(normalized * 30 + _Time.x * 30) * 0.5 + 0.5, 5) * 0.1 * saturate(normalized );
                    const half4 fadeOutColor = ARKIT_SAMPLE_TEXTURE2D(_fadeoutGradient, sampler_fadeoutGradient, half2(angleDistance, 0.5));
                    mixedVideo = lerp(half4(fadeOutColor.rgb, 1), mixedVideo, max(1 - fadeOutColor.a , humanMask));
                } 
#endif


#if _DEBUG_ANGULARDIFFERENCE
               if (i.texcoord.x % 0.1 < 0.05 ? i.texcoord.y % 0.1 < 0.05 : i.texcoord.y % 0.1 > 0.05 )
               {
                   o.color = half4(inverseFadeOutStrength, inverseFadeOutStrength, inverseFadeOutStrength, 1);
               }
               else  if (i.texcoord.x % 0.1 < 0.075 ? i.texcoord.y % 0.1 < 0.075 : i.texcoord.y % 0.1 > 0.075 )
               {
                   o.color = half4((pixelViewDir + float3(1,1,1))/2, 1);
               }
               else {
                   o.color = half4((poiToCameraDir + float3(1,1,1))/2, 1);
               }
#elif _DEBUG_PORTALMASK
                o.color = portalMask;
#else 
                o.color = mixedVideo;
#endif
                o.depth = depthValue;
                return o;
            }

            ENDHLSL
        }
    }
}
