Shader "Universal Render Pipeline/DissolveShader"
{
    Properties
    {
        [MainColor] _Color("Color", Color) = (1,1,1,1)
        [MainTexture] _MainTex("Albedo (RGB)", 2D) = "white" {}
        _Glossiness("Smoothness", Range(0,1)) = 0.5
        _Metallic("Metallic", Range(0,1)) = 0.0
        
        //Dissolve properties
        _DissolveTexture("Dissolve Texture", 2D) = "white" {} 
        _Amount("Amount", Range(0,1)) = 0
    }
    
    SubShader
    {
        Tags { 
            "RenderType" = "Opaque" 
            "RenderPipeline" = "UniversalPipeline"
        }
        LOD 200
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_DissolveTexture);
            SAMPLER(sampler_DissolveTexture);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float _Metallic;
                float _Glossiness;
                float _Amount;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz); // Proper world position
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // Sample dissolve texture
                half dissolve_value = SAMPLE_TEXTURE2D(_DissolveTexture, sampler_DissolveTexture, IN.uv).r;
                clip(dissolve_value - _Amount);

                // Sample main texture
                half4 baseColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv) * _Color;

                // Prepare surface data for lighting
                InputData inputData = (InputData)0;
                inputData.positionWS = IN.positionWS; // Use actual world position
                inputData.normalWS = half3(0, 0, 1); // Default normal
                inputData.viewDirectionWS = SafeNormalize(GetCameraPositionWS() - inputData.positionWS);
                inputData.shadowCoord = TransformWorldToShadowCoord(inputData.positionWS);

                SurfaceData surfaceData;
                surfaceData.albedo = baseColor.rgb;
                surfaceData.alpha = baseColor.a;
                surfaceData.metallic = _Metallic;
                surfaceData.specular = half3(0.04, 0.04, 0.04); // Standard specular value
                surfaceData.smoothness = _Glossiness;
                surfaceData.normalTS = half3(0, 0, 1);
                surfaceData.occlusion = 1;
                surfaceData.emission = half3(0, 0, 0);

                // Apply lighting
                return UniversalFragmentPBR(inputData, surfaceData);
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Lit"
}