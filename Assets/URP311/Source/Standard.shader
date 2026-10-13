Shader "Liminal/311/Source/Standard" {
Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _MainTex("Albedo", 2D) = "white" {}

        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5

        _Glossiness("Smoothness", Range(0.0, 1.0)) = 0.5
        _GlossMapScale("Smoothness Scale", Range(0.0, 1.0)) = 1.0
        [Enum(Metallic Alpha,0,Albedo Alpha,1)] _SmoothnessTextureChannel ("Smoothness texture channel", Float) = 0

        [Gamma] _Metallic("Metallic", Range(0.0, 1.0)) = 0.0
        _MetallicGlossMap("Metallic", 2D) = "white" {}

        [ToggleOff] _SpecularHighlights("Specular Highlights", Float) = 1.0
        [ToggleOff] _GlossyReflections("Glossy Reflections", Float) = 1.0

        _BumpScale("Scale", Float) = 1.0
        [Normal] _BumpMap("Normal Map", 2D) = "bump" {}

        _Parallax ("Height Scale", Range (0.005, 0.08)) = 0.02
        _ParallaxMap ("Height Map", 2D) = "black" {}

        _OcclusionStrength("Strength", Range(0.0, 1.0)) = 1.0
        _OcclusionMap("Occlusion", 2D) = "white" {}

        _EmissionColor("Color", Color) = (0,0,0)
        _EmissionMap("Emission", 2D) = "white" {}

        _DetailMask("Detail Mask", 2D) = "white" {}

        _DetailAlbedoMap("Detail Albedo x2", 2D) = "grey" {}
        _DetailNormalMapScale("Scale", Float) = 1.0
        [Normal] _DetailNormalMap("Normal Map", 2D) = "bump" {}

        [Enum(UV0,0,UV1,1)] _UVSec ("UV Set for secondary textures", Float) = 0


        // Blending state
        [HideInInspector] _Mode ("__mode", Float) = 0.0
        [HideInInspector] _SrcBlend ("__src", Float) = 1.0
        [HideInInspector] _DstBlend ("__dst", Float) = 0.0
        [HideInInspector] _ZWrite ("__zw", Float) = 1.0
    }
SubShader { Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
Pass { Name "FORWARD" Tags { "LightMode"="UniversalForward" } Blend [_SrcBlend] [_DstBlend] ZWrite [_ZWrite]
HLSLPROGRAM
#include "HLSLSupport.cginc"
#include "Assets/URP311/Source/Source311ShaderVariables.hlsl"
float4 _Source311BasePosition;
#define _LightColor0 _Source311BaseColor
#include "Lighting.cginc"
#undef _LightColor0
#define _LightColor0 (_Source311BaseColor*unity_LightData.z)
#define _WorldSpaceLightPos0 (_Source311BasePosition*unity_LightData.z)
float4 _Source311IndirectSpecColor;
#define unity_IndirectSpecColor _Source311IndirectSpecColor
float4 _Source311Ambient;
#define glstate_lightmodel_ambient _Source311Ambient
float4 _Source311SHAr;
#define unity_SHAr _Source311SHAr
float4 _Source311SHAg;
#define unity_SHAg _Source311SHAg
float4 _Source311SHAb;
#define unity_SHAb _Source311SHAb
float4 _Source311SHBr;
#define unity_SHBr _Source311SHBr
float4 _Source311SHBg;
#define unity_SHBg _Source311SHBg
float4 _Source311SHBb;
#define unity_SHBb _Source311SHBb
float4 _Source311SHC;
#define unity_SHC _Source311SHC
#define UNITY_PASS_FORWARDBASE 1
#define KEYWORD_TYPE_FLAG_SPECIALIZED_VARIANTS (1 << 0)
#define KEYWORD_TYPE_FLAG_RUNTIME_BRANCHING (1 << 1)
#if defined(SHADER_STAGE_VERTEX)
#define FOG_LINEAR_KEYWORD_DECLARED (1 << 0)
#define FOG_EXP_KEYWORD_DECLARED (1 << 0)
#define FOG_EXP2_KEYWORD_DECLARED (1 << 0)
#define INSTANCING_ON_KEYWORD_DECLARED (1 << 0)
#define _NORMALMAP_KEYWORD_DECLARED (1 << 0)
#define _ALPHATEST_ON_KEYWORD_DECLARED (1 << 0)
#define _ALPHABLEND_ON_KEYWORD_DECLARED (1 << 0)
#define _ALPHAPREMULTIPLY_ON_KEYWORD_DECLARED (1 << 0)
#define _METALLICGLOSSMAP_KEYWORD_DECLARED (1 << 0)
#define _PARALLAXMAP_KEYWORD_DECLARED (1 << 0)
#endif
#if defined(SHADER_STAGE_FRAGMENT)
#define _EMISSION_KEYWORD_DECLARED (1 << 0)
#define FOG_LINEAR_KEYWORD_DECLARED (1 << 0)
#define FOG_EXP_KEYWORD_DECLARED (1 << 0)
#define FOG_EXP2_KEYWORD_DECLARED (1 << 0)
#define INSTANCING_ON_KEYWORD_DECLARED (1 << 0)
#define _DETAIL_MULX2_KEYWORD_DECLARED (1 << 0)
#define _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A_KEYWORD_DECLARED (1 << 0)
#define _SPECULARHIGHLIGHTS_OFF_KEYWORD_DECLARED (1 << 0)
#define _GLOSSYREFLECTIONS_OFF_KEYWORD_DECLARED (1 << 0)
#define _NORMALMAP_KEYWORD_DECLARED (1 << 0)
#define _ALPHATEST_ON_KEYWORD_DECLARED (1 << 0)
#define _ALPHABLEND_ON_KEYWORD_DECLARED (1 << 0)
#define _ALPHAPREMULTIPLY_ON_KEYWORD_DECLARED (1 << 0)
#define _METALLICGLOSSMAP_KEYWORD_DECLARED (1 << 0)
#define _PARALLAXMAP_KEYWORD_DECLARED (1 << 0)
#endif
#line 66 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif

#include "HLSLSupport.cginc"
#define UNITY_INSTANCED_LOD_FADE
#define UNITY_INSTANCED_SH
#define UNITY_INSTANCED_LIGHTMAPSTS
#define UNITY_INSTANCED_RENDERER_BOUNDS
#include "UnityShaderVariables.cginc"
#include "UnityShaderUtilities.cginc"
#line 49

        #define UNITY_SETUP_BRDF_INPUT MetallicSetup
    #line 66 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
            #pragma target 3.0

            // -------------------------------------

            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature_fragment _EMISSION
            #pragma shader_feature_local _METALLICGLOSSMAP
            #pragma shader_feature_local_fragment _DETAIL_MULX2
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature_local_fragment _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature_local_fragment _GLOSSYREFLECTIONS_OFF
            #pragma shader_feature_local _PARALLAXMAP

            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            // Uncomment the following line to enable dithering LOD crossfade. Note: there are more in the file to uncomment for other passes.
            //#pragma multi_compile _ LOD_FADE_CROSSFADE

            #pragma vertex vertBase
            #pragma fragment fragBase
            #include "UnityStandardCoreForward.cginc"

            
ENDHLSL
}
Pass { Name "FORWARD_DELTA" Tags { "LightMode"="Source311ForwardAdd" } Blend [_SrcBlend] One ZWrite Off ZTest LEqual
HLSLPROGRAM
#include "HLSLSupport.cginc"
#include "Assets/URP311/Source/Source311ShaderVariables.hlsl"
#define UNITY_PASS_FORWARDADD 1
#define KEYWORD_TYPE_FLAG_SPECIALIZED_VARIANTS (1 << 0)
#define KEYWORD_TYPE_FLAG_RUNTIME_BRANCHING (1 << 1)
#if defined(SHADER_STAGE_VERTEX)
#define FOG_LINEAR_KEYWORD_DECLARED (1 << 0)
#define FOG_EXP_KEYWORD_DECLARED (1 << 0)
#define FOG_EXP2_KEYWORD_DECLARED (1 << 0)
#define _NORMALMAP_KEYWORD_DECLARED (1 << 0)
#define _ALPHATEST_ON_KEYWORD_DECLARED (1 << 0)
#define _ALPHABLEND_ON_KEYWORD_DECLARED (1 << 0)
#define _ALPHAPREMULTIPLY_ON_KEYWORD_DECLARED (1 << 0)
#define _METALLICGLOSSMAP_KEYWORD_DECLARED (1 << 0)
#define _PARALLAXMAP_KEYWORD_DECLARED (1 << 0)
#endif
#if defined(SHADER_STAGE_FRAGMENT)
#define FOG_LINEAR_KEYWORD_DECLARED (1 << 0)
#define FOG_EXP_KEYWORD_DECLARED (1 << 0)
#define FOG_EXP2_KEYWORD_DECLARED (1 << 0)
#define _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A_KEYWORD_DECLARED (1 << 0)
#define _SPECULARHIGHLIGHTS_OFF_KEYWORD_DECLARED (1 << 0)
#define _DETAIL_MULX2_KEYWORD_DECLARED (1 << 0)
#define _NORMALMAP_KEYWORD_DECLARED (1 << 0)
#define _ALPHATEST_ON_KEYWORD_DECLARED (1 << 0)
#define _ALPHABLEND_ON_KEYWORD_DECLARED (1 << 0)
#define _ALPHAPREMULTIPLY_ON_KEYWORD_DECLARED (1 << 0)
#define _METALLICGLOSSMAP_KEYWORD_DECLARED (1 << 0)
#define _PARALLAXMAP_KEYWORD_DECLARED (1 << 0)
#endif
#line 104 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif

#include "HLSLSupport.cginc"
#define UNITY_INSTANCED_LOD_FADE
#define UNITY_INSTANCED_SH
#define UNITY_INSTANCED_LIGHTMAPSTS
#define UNITY_INSTANCED_RENDERER_BOUNDS
#include "UnityShaderVariables.cginc"
#include "UnityShaderUtilities.cginc"
#line 49

        #define UNITY_SETUP_BRDF_INPUT MetallicSetup
    #line 104 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
            #pragma target 3.0

            // -------------------------------------


            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature_local _METALLICGLOSSMAP
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature_local_fragment _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature_local_fragment _DETAIL_MULX2
            #pragma shader_feature_local _PARALLAXMAP

            #pragma multi_compile_fwdadd_fullshadows
            #pragma multi_compile_fog
            // Uncomment the following line to enable dithering LOD crossfade. Note: there are more in the file to uncomment for other passes.
            //#pragma multi_compile _ LOD_FADE_CROSSFADE

            #pragma vertex vertAdd
            #pragma fragment fragAdd
            #include "UnityStandardCoreForward.cginc"

            
ENDHLSL
}
Pass { Name "ShadowCaster" Tags { "LightMode"="ShadowCaster" } ZWrite On ZTest LEqual
HLSLPROGRAM
#include "HLSLSupport.cginc"
#include "Assets/URP311/Source/Source311ShaderVariables.hlsl"
#define UNITY_PASS_SHADOWCASTER 1
#define KEYWORD_TYPE_FLAG_SPECIALIZED_VARIANTS (1 << 0)
#define KEYWORD_TYPE_FLAG_RUNTIME_BRANCHING (1 << 1)
#if defined(SHADER_STAGE_VERTEX)
#define INSTANCING_ON_KEYWORD_DECLARED (1 << 0)
#define _ALPHATEST_ON_KEYWORD_DECLARED (1 << 0)
#define _ALPHABLEND_ON_KEYWORD_DECLARED (1 << 0)
#define _ALPHAPREMULTIPLY_ON_KEYWORD_DECLARED (1 << 0)
#define _METALLICGLOSSMAP_KEYWORD_DECLARED (1 << 0)
#define _PARALLAXMAP_KEYWORD_DECLARED (1 << 0)
#endif
#if defined(SHADER_STAGE_FRAGMENT)
#define INSTANCING_ON_KEYWORD_DECLARED (1 << 0)
#define _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A_KEYWORD_DECLARED (1 << 0)
#define _ALPHATEST_ON_KEYWORD_DECLARED (1 << 0)
#define _ALPHABLEND_ON_KEYWORD_DECLARED (1 << 0)
#define _ALPHAPREMULTIPLY_ON_KEYWORD_DECLARED (1 << 0)
#define _METALLICGLOSSMAP_KEYWORD_DECLARED (1 << 0)
#define _PARALLAXMAP_KEYWORD_DECLARED (1 << 0)
#endif
#line 137 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif

#include "HLSLSupport.cginc"
#define UNITY_INSTANCED_LOD_FADE
#define UNITY_INSTANCED_SH
#define UNITY_INSTANCED_LIGHTMAPSTS
#define UNITY_INSTANCED_RENDERER_BOUNDS
#include "UnityShaderVariables.cginc"
#include "UnityShaderUtilities.cginc"
#line 49

        #define UNITY_SETUP_BRDF_INPUT MetallicSetup
    #line 137 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
            #pragma target 3.0

            // -------------------------------------


            #pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature_local _METALLICGLOSSMAP
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature_local _PARALLAXMAP
            #pragma multi_compile_shadowcaster
            #pragma multi_compile_instancing
            // Uncomment the following line to enable dithering LOD crossfade. Note: there are more in the file to uncomment for other passes.
            //#pragma multi_compile _ LOD_FADE_CROSSFADE

            #pragma vertex vertShadowCaster
            #pragma fragment fragShadowCaster

            #include "UnityStandardShadow.cginc"

            
ENDHLSL
}
Pass { Name "META" Tags { "LightMode"="Meta" } Cull Off
HLSLPROGRAM
#include "HLSLSupport.cginc"
#include "Assets/URP311/Source/Source311ShaderVariables.hlsl"
#define UNITY_PASS_META 1
#define KEYWORD_TYPE_FLAG_SPECIALIZED_VARIANTS (1 << 0)
#define KEYWORD_TYPE_FLAG_RUNTIME_BRANCHING (1 << 1)
#if defined(SHADER_STAGE_VERTEX)
#define EDITOR_VISUALIZATION_KEYWORD_DECLARED (1 << 0)
#define _METALLICGLOSSMAP_KEYWORD_DECLARED (1 << 0)
#endif
#if defined(SHADER_STAGE_FRAGMENT)
#define _EMISSION_KEYWORD_DECLARED (1 << 0)
#define EDITOR_VISUALIZATION_KEYWORD_DECLARED (1 << 0)
#define _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A_KEYWORD_DECLARED (1 << 0)
#define _DETAIL_MULX2_KEYWORD_DECLARED (1 << 0)
#define _METALLICGLOSSMAP_KEYWORD_DECLARED (1 << 0)
#endif
#line 205 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif

#include "HLSLSupport.cginc"
#define UNITY_INSTANCED_LOD_FADE
#define UNITY_INSTANCED_SH
#define UNITY_INSTANCED_LIGHTMAPSTS
#define UNITY_INSTANCED_RENDERER_BOUNDS
#include "UnityShaderVariables.cginc"
#include "UnityShaderUtilities.cginc"
#line 49

        #define UNITY_SETUP_BRDF_INPUT MetallicSetup
    #line 205 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
            #pragma vertex vert_meta
            #pragma fragment frag_meta

            #pragma shader_feature_fragment _EMISSION
            #pragma shader_feature_local _METALLICGLOSSMAP
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature_local_fragment _DETAIL_MULX2
            #pragma shader_feature EDITOR_VISUALIZATION

            #include "UnityStandardMeta.cginc"
            
ENDHLSL
}
}
Fallback Off
}
