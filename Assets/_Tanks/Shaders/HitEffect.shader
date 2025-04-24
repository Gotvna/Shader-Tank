Shader "Custom/HitEffect"
{
    Properties
    {
        _Intensity("Distortion Intensity", Range(0, 0.3)) = 0.02
        _DirectionX("X Direction", Range(-1, 1)) = 1
        _DirectionY("Y Direction", Range(-1, 1)) = 0
    }

    HLSLINCLUDE
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

    TEXTURE2D_X(_BlitTexture);
    SAMPLER(sampler_BlitTexture);
    
    float _Intensity;
    float _DirectionX;
    float _DirectionY;

    struct Attributes { uint vertexID : SV_VertexID; };
    struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; };

    Varyings Vert(Attributes input)
    {
        Varyings output;
        output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
        output.uv = GetFullScreenTriangleTexCoord(input.vertexID);
        return output;
    }

    half4 Frag(Varyings input) : SV_Target
    {
        float2 uv = input.uv;
        float2 offset = float2(_DirectionX, _DirectionY) * _Intensity;
        
        half r = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, uv + offset).r;
        half g = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, uv).g;
        half b = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, uv - offset).b;
        
        return half4(r, g, b, 1.0);
    }
    ENDHLSL

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        ZTest Always ZWrite Off Cull Off

        Pass
        {
            Name "ChromaticAberrationPass"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            ENDHLSL
        }
    }
}