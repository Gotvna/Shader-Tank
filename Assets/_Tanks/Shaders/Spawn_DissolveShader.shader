Shader "Unlit/Spawn_DissolveShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _DissolveTexture ("Dissolve Noise", 2D) = "white" {}
        _DissolveAmount ("Dissolve Amount", Range(0, 1)) = 0
        _DissolveEdgeWidth ("Edge Width", Range(0, 0.2)) = 0.1
        _DissolveEdgeColor ("Edge Color", Color) = (1, 1, 0, 1)
        [HDR] _EmissionColor("Emission Color", Color) = (0,0,0,0)
        _EmissionMap("Emission Map", 2D) = "white" {}
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };
            
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_DissolveTexture);
            SAMPLER(sampler_DissolveTexture);
            
            CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            float4 _DissolveTexture_ST;
            float _DissolveAmount;
            float _DissolveEdgeWidth;
            float4 _DissolveEdgeColor;
            CBUFFER_END
            
            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                return OUT;
            }
            
            half4 frag(Varyings IN) : SV_Target
            {
                // Sample main texture with UV scaling
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
    
                // Sample emission if needed
                half4 emission = SAMPLE_TEXTURE2D(_EmissionMap, sampler_MainTex, IN.uv) * _EmissionColor;
    
                // Sample dissolve noise texture
                float dissolveValue = SAMPLE_TEXTURE2D(_DissolveTexture, sampler_DissolveTexture, IN.uv).r;
    
                // Clip pixels based on dissolve amount
                clip(dissolveValue - _DissolveAmount);
    
                // Edge effect
                if (dissolveValue < _DissolveAmount + _DissolveEdgeWidth)
                {
                    col.rgb = _DissolveEdgeColor.rgb;
                    emission.rgb += _DissolveEdgeColor.rgb * 2.0; // Boost emission at edges
                }
    
                return col + emission;
            }
            ENDHLSL
        }
    }
}