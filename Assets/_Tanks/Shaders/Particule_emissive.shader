Shader "Universal Render Pipeline/Lit_CustomEmissionFade"
{
    Properties
    {
        _BaseMap("Albedo", 2D) = "white" {}
        _BaseColor("Color", Color) = (1,1,1,1)
        _Metallic("Metallic", Range(0.0, 1.0)) = 0.0
        _Smoothness("Smoothness", Range(0.0, 1.0)) = 0.5
        _EmissionColor("Emission Color", Color) = (0,0,0,1)
        _EmissionMap("Emission Map", 2D) = "white" {}
        _BumpMap("Normal Map", 2D) = "bump" {}
        _BumpScale("Normal Scale", Float) = 1.0
        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5

        _MetallicGlossMap("Metallic", 2D) = "white" {}
        _SpecGlossMap("Specular", 2D) = "white" {}
        _SpecColor("Specular Color", Color) = (0.2, 0.2, 0.2, 1)
        _OcclusionMap("Occlusion", 2D) = "white" {}
        _OcclusionStrength("Occlusion Strength", Range(0.0, 1.0)) = 1.0

        _DetailMask("Detail Mask", 2D) = "white" {}
        _DetailAlbedoMap("Detail Albedo x2", 2D) = "linearGrey" {}
        _DetailAlbedoMapScale("Detail Albedo Scale", Range(0.0, 2.0)) = 1.0
        _DetailNormalMap("Detail Normal Map", 2D) = "bump" {}
        _DetailNormalMapScale("Detail Normal Scale", Range(0.0, 2.0)) = 1.0

        _ParallaxMap("Height Map", 2D) = "black" {}
        _Parallax("Parallax", Range(0.005, 0.08)) = 0.005

        _SpawnPosition("Spawn Position", Vector) = (0, 0, 0, 0)
        _FadeDistance("Fade Distance", Float) = 5.0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "UniversalMaterialType" = "Lit"
        }
        LOD 300

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull[_Cull]
            AlphaToMask[_AlphaToMask]

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex LitPassVertex
            #pragma fragment LitPassFragment

            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local_fragment _EMISSION

            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitForwardPass.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float3 _SpawnPosition;
                float _FadeDistance;
                float _EmissionFade;
            CBUFFER_END


            void InitializeSurfaceData(float2 uv, half4 albedo, half3 normalTS, half metallic, half3 specular,
                                       half smoothness, half occlusion, half3 emission, half alpha,
                                       float3 positionWS,
                                       out SurfaceData outSurfaceData)
            {
                outSurfaceData.albedo = albedo.rgb;
                outSurfaceData.metallic = metallic;
                outSurfaceData.specular = specular;
                outSurfaceData.smoothness = smoothness;
                outSurfaceData.normalTS = normalTS;
                outSurfaceData.occlusion = occlusion;

                outSurfaceData.emission = emission * _EmissionFade;

                float dist = distance(positionWS, _SpawnPosition);
                float fade = saturate(1.0 - dist / _FadeDistance);
                outSurfaceData.alpha *= fade;
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
    CustomEditor "UnityEditor.Rendering.Universal.ShaderGUI.LitShader"
}
