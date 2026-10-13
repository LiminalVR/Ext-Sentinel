Shader "Liminal/311/Source/Ghost" {
Properties{
        // Main Properties
        _Color("Main Color", Color) = (1,1,1,1)
        _MainTex("Base (RGB) Trans (A)", 2D) = "white" {}
        _BumpMap("Normal Map", 2D) = "bump" {}

        // Fresnel Properties
        _FresnelColor("Fresnel Color", Color) = (1,1,1,1)
        [PowerSlider(4)] _FresnelExponent("Fresnel Exponent", Range(0.25, 4)) = 1

            // Intensity Controls
            _EmissionIntensity("Emission Intensity", Range(0, 5)) = 1
            _ColorIntensity("Color Intensity", Range(0, 3)) = 1

            // Transparency Control
            _AlphaIntensity("Alpha Intensity", Range(0, 2)) = 1
            _BaseAlpha("Base Alpha", Range(0, 1)) = 0.5

            // Noise Properties
            _NoiseTex("Noise Texture", 2D) = "white" {}
            _NoiseMask("Noise Mask", 2D) = "white" {}
            _NoiseIntensity("Noise Intensity", Range(0, 1)) = 0.5
            _NoiseScale("Noise Scale", Range(0.1, 10)) = 1
            _NoiseSpeedX("Noise Speed X", Range(-2, 2)) = 0.1
            _NoiseSpeedY("Noise Speed Y", Range(-2, 2)) = 0.1

                // Distortion Properties
                _DistortionIntensity("Distortion Intensity", Range(0, 0.5)) = 0.1
                _DistortionScale("Distortion Scale", Range(0.1, 5)) = 1
                _DistortionSpeedX("Distortion Speed X", Range(-2, 2)) = 0.05
                _DistortionSpeedY("Distortion Speed Y", Range(-2, 2)) = 0.1
    }
SubShader { Tags { "RenderType"="Transparent" "Queue"="Transparent" "IgnoreProjector"="True" "RenderPipeline"="UniversalPipeline" }
Pass { Name "<Unnamed Pass 0>" Tags { "LightMode"="SRPDefaultUnlit" } ZWrite On ColorMask 0
HLSLPROGRAM
#include "HLSLSupport.cginc"
#include "Assets/URP311/Source/Source311ShaderVariables.hlsl"
#define KEYWORD_TYPE_FLAG_SPECIALIZED_VARIANTS (1 << 0)
#define KEYWORD_TYPE_FLAG_RUNTIME_BRANCHING (1 << 1)
#line 42 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif

#include "HLSLSupport.cginc"
#define UNITY_INSTANCED_LOD_FADE
#define UNITY_INSTANCED_SH
#define UNITY_INSTANCED_LIGHTMAPSTS
#define UNITY_INSTANCED_RENDERER_BOUNDS
#include "UnityShaderVariables.cginc"
#include "UnityShaderUtilities.cginc"
#line 42 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
                    #pragma vertex vert
                    #pragma fragment frag
                    #include "UnityCG.cginc"

                    struct v2f {
                        float4 pos : SV_POSITION;
                    };

                    v2f vert(appdata_base v)
                    {
                        v2f o;
                        o.pos = UnityObjectToClipPos(v.vertex);
                        return o;
                    }

                    half4 frag(v2f i) : COLOR
                    {
                        return half4(0,0,0,0);
                    }
                    
ENDHLSL
}
Pass { Name "FORWARD" Tags { "LightMode"="UniversalForward" } Blend SrcAlpha OneMinusSrcAlpha ZWrite Off ColorMask RGB
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
#define INSTANCING_ON_KEYWORD_DECLARED (1 << 0)
#define FOG_LINEAR_KEYWORD_DECLARED (1 << 0)
#define FOG_EXP_KEYWORD_DECLARED (1 << 0)
#define FOG_EXP2_KEYWORD_DECLARED (1 << 0)
#endif
#if defined(SHADER_STAGE_FRAGMENT)
#define INSTANCING_ON_KEYWORD_DECLARED (1 << 0)
#define FOG_LINEAR_KEYWORD_DECLARED (1 << 0)
#define FOG_EXP_KEYWORD_DECLARED (1 << 0)
#define FOG_EXP2_KEYWORD_DECLARED (1 << 0)
#endif
#line 78 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif

// compile directives
#pragma vertex vert_surf
#pragma fragment frag_surf
#pragma target 3.0
#pragma multi_compile_instancing
#pragma multi_compile_fog
#pragma multi_compile_fwdbasealpha noshadow
#include "HLSLSupport.cginc"
#define UNITY_INSTANCED_LOD_FADE
#define UNITY_INSTANCED_SH
#define UNITY_INSTANCED_LIGHTMAPSTS
#define UNITY_INSTANCED_RENDERER_BOUNDS
#include "UnityShaderVariables.cginc"
#include "UnityShaderUtilities.cginc"
// -------- variant for: <when no other keywords are defined>
#if !defined(INSTANCING_ON)
// Surface shader code generated based on:
// writes to per-pixel normal: YES
// writes to emission: YES
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: no
// needs screen space position: no
// needs world space position: no
// needs view direction: YES
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: no
// needs world space view direction for lightmaps: no
// needs vertex color: no
// needs VFACE: no
// needs SV_IsFrontFace: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: no
// 4 texcoords actually used
//   float2 _MainTex
//   float2 _BumpMap
//   float2 _NoiseTex
//   float2 _NoiseMask
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "AutoLight.cginc"

#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

// Original surface shader snippet:
#line 66 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
                //#pragma surface surf Lambert alpha
                //#pragma target 3.0

                struct Input {
                    float2 uv_MainTex;
                    float3 worldNormal;
                    float3 viewDir;
                    float2 uv_BumpMap;
                    float2 uv_NoiseTex;
                    float2 uv_NoiseMask;
                    INTERNAL_DATA
                };

                    // Texture samplers
                    sampler2D _MainTex;
                    sampler2D _BumpMap;
                    sampler2D _NoiseTex;
                    sampler2D _NoiseMask;

                    // Color properties
                    fixed4 _Color;
                    float3 _FresnelColor;

                    // Intensity properties
                    float _FresnelExponent;
                    float _EmissionIntensity;
                    float _ColorIntensity;
                    float _AlphaIntensity;
                    float _BaseAlpha;

                    // Noise properties
                    float _NoiseIntensity;
                    float _NoiseScale;
                    float _NoiseSpeedX;
                    float _NoiseSpeedY;

                    // Distortion properties
                    float _DistortionIntensity;
                    float _DistortionScale;
                    float _DistortionSpeedX;
                    float _DistortionSpeedY;

                    void surf(Input IN, inout SurfaceOutput o) {
                        // Calculate animated UV offsets for noise
                        float2 noiseUV = IN.uv_NoiseTex * _NoiseScale;
                        noiseUV.x += _Time.y * _NoiseSpeedX;
                        noiseUV.y += _Time.y * _NoiseSpeedY;

                        // Calculate distortion UV
                        float2 distortUV = IN.uv_MainTex * _DistortionScale;
                        distortUV.x += _Time.y * _DistortionSpeedX;
                        distortUV.y += _Time.y * _DistortionSpeedY;

                        // Sample noise textures
                        float4 noise = tex2D(_NoiseTex, noiseUV);
                        float4 noiseMask = tex2D(_NoiseMask, IN.uv_NoiseMask);

                        // Create distortion from noise
                        float2 distortion = (noise.rg - 0.5) * 2.0 * _DistortionIntensity * noiseMask.r;

                        // Apply distortion to main texture UV
                        float2 distortedUV = IN.uv_MainTex + distortion;

                        // Sample main texture with distorted UVs
                        half4 mainTex = tex2D(_MainTex, distortedUV);

                        // Unpack normal map
                        o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap + distortion * 0.5));

                        // Calculate fresnel effect
                        float fresnel = dot(normalize(IN.viewDir), o.Normal);
                        fresnel = saturate(1 - fresnel);
                        fresnel = pow(fresnel, _FresnelExponent);

                        // Apply main color to texture (proper colorization)
                        float3 baseColor = mainTex.rgb * _Color.rgb * _ColorIntensity;

                        // Combine fresnel with its color
                        float3 fresnelColor = fresnel * _FresnelColor;

                        // Add noise effect to emission
                        float noiseEffect = noise.r * noiseMask.g * _NoiseIntensity;

                        // Set albedo with the properly colored texture
                        o.Albedo = baseColor;

                        // Set emission with intensity control and noise
                        o.Emission = (baseColor + fresnelColor + noiseEffect * _Color.rgb) * _EmissionIntensity;

                        // Calculate alpha separately without being affected by color
                        // Use base alpha plus fresnel for edge glow, multiplied by texture alpha
                        float finalAlpha = (_BaseAlpha + fresnel * _AlphaIntensity) * mainTex.a * _Color.a;

                        // Add subtle noise variation to alpha if desired
                        finalAlpha = saturate(finalAlpha + noiseEffect * 0.1);

                        o.Alpha = finalAlpha;
                    }
                    

// vertex-to-fragment interpolation data
// no lightmaps:
#ifndef LIGHTMAP_ON
// half-precision fragment shader registers:
#ifdef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
#define FOG_COMBINED_WITH_TSPACE
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 pack0 : TEXCOORD0; // _MainTex _BumpMap
  float4 pack1 : TEXCOORD1; // _NoiseTex _NoiseMask
  float4 tSpace0 : TEXCOORD2;
  float4 tSpace1 : TEXCOORD3;
  float4 tSpace2 : TEXCOORD4;
  #if UNITY_SHOULD_SAMPLE_SH
  half3 sh : TEXCOORD5; // SH
  #endif
  DECLARE_LIGHT_COORDS(6)
  #if SHADER_TARGET >= 30
  float4 lmap : TEXCOORD7;
  #endif
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
#endif
// high-precision fragment shader registers:
#ifndef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 pack0 : TEXCOORD0; // _MainTex _BumpMap
  float4 pack1 : TEXCOORD1; // _NoiseTex _NoiseMask
  float4 tSpace0 : TEXCOORD2;
  float4 tSpace1 : TEXCOORD3;
  float4 tSpace2 : TEXCOORD4;
  #if UNITY_SHOULD_SAMPLE_SH
  half3 sh : TEXCOORD5; // SH
  #endif
  UNITY_FOG_COORDS(6)
  DECLARE_LIGHT_COORDS(7)
  #if SHADER_TARGET >= 30
  float4 lmap : TEXCOORD8;
  #endif
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
#endif
#endif
// with lightmaps:
#ifdef LIGHTMAP_ON
// half-precision fragment shader registers:
#ifdef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
#define FOG_COMBINED_WITH_TSPACE
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 pack0 : TEXCOORD0; // _MainTex _BumpMap
  float4 pack1 : TEXCOORD1; // _NoiseTex _NoiseMask
  float4 tSpace0 : TEXCOORD2;
  float4 tSpace1 : TEXCOORD3;
  float4 tSpace2 : TEXCOORD4;
  float4 lmap : TEXCOORD5;
  DECLARE_LIGHT_COORDS(6)
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
#endif
// high-precision fragment shader registers:
#ifndef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 pack0 : TEXCOORD0; // _MainTex _BumpMap
  float4 pack1 : TEXCOORD1; // _NoiseTex _NoiseMask
  float4 tSpace0 : TEXCOORD2;
  float4 tSpace1 : TEXCOORD3;
  float4 tSpace2 : TEXCOORD4;
  float4 lmap : TEXCOORD5;
  UNITY_FOG_COORDS(6)
  DECLARE_LIGHT_COORDS(7)
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
#endif
#endif
float4 _MainTex_ST;
float4 _BumpMap_ST;
float4 _NoiseTex_ST;
float4 _NoiseMask_ST;

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  o.pos = UnityObjectToClipPos(v.vertex);
  o.pack0.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
  o.pack0.zw = TRANSFORM_TEX(v.texcoord, _BumpMap);
  o.pack1.xy = TRANSFORM_TEX(v.texcoord, _NoiseTex);
  o.pack1.zw = TRANSFORM_TEX(v.texcoord, _NoiseMask);
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  float3 worldNormal = UnityObjectToWorldNormal(v.normal);
  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
  o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
  o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
  o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
  #ifdef DYNAMICLIGHTMAP_ON
  o.lmap.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
  #endif
  #ifdef LIGHTMAP_ON
  o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
  #endif

  // SH/ambient and vertex lights
  #ifndef LIGHTMAP_ON
    #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
      o.sh = 0;
      // Approximated illumination from non-important point lights
      #ifdef VERTEXLIGHT_ON
        o.sh += Shade4PointLights (
          unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
          unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
          unity_4LightAtten0, worldPos, worldNormal);
      #endif
      o.sh = ShadeSHPerVertex (worldNormal, o.sh);
    #endif
  #endif // !LIGHTMAP_ON

  COMPUTE_LIGHT_COORDS(o); // pass light cookie coordinates to pixel shader
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_TRANSFER_FOG_COMBINED_WITH_TSPACE(o,o.pos); // pass fog coordinates to pixel shader
  #elif defined (FOG_COMBINED_WITH_WORLD_POS)
    UNITY_TRANSFER_FOG_COMBINED_WITH_WORLD_POS(o,o.pos); // pass fog coordinates to pixel shader
  #else
    UNITY_TRANSFER_FOG(o,o.pos); // pass fog coordinates to pixel shader
  #endif
  return o;
}

// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  UNITY_SETUP_INSTANCE_ID(IN);
  UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
  // prepare and unpack data
  Input surfIN;
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_EXTRACT_FOG_FROM_TSPACE(IN);
  #elif defined (FOG_COMBINED_WITH_WORLD_POS)
    UNITY_EXTRACT_FOG_FROM_WORLD_POS(IN);
  #else
    UNITY_EXTRACT_FOG(IN);
  #endif
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_RECONSTRUCT_TBN(IN);
  #else
    UNITY_EXTRACT_TBN(IN);
  #endif
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.uv_MainTex.x = 1.0;
  surfIN.worldNormal.x = 1.0;
  surfIN.viewDir.x = 1.0;
  surfIN.uv_BumpMap.x = 1.0;
  surfIN.uv_NoiseTex.x = 1.0;
  surfIN.uv_NoiseMask.x = 1.0;
  surfIN.uv_MainTex = IN.pack0.xy;
  surfIN.uv_BumpMap = IN.pack0.zw;
  surfIN.uv_NoiseTex = IN.pack1.xy;
  surfIN.uv_NoiseMask = IN.pack1.zw;
  float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
  float3 viewDir = _unity_tbn_0 * worldViewDir.x + _unity_tbn_1 * worldViewDir.y  + _unity_tbn_2 * worldViewDir.z;
  surfIN.viewDir = viewDir;
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutput o = (SurfaceOutput)0;
  #else
  SurfaceOutput o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Specular = 0.0;
  o.Alpha = 0.0;
  o.Gloss = 0.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);
  o.Normal = fixed3(0,0,1);

  // call surface function
  surf (surfIN, o);

  // compute lighting & shadowing factor
  UNITY_LIGHT_ATTENUATION(atten, IN, worldPos)
  fixed4 c = 0;
  float3 worldN;
  worldN.x = dot(_unity_tbn_0, o.Normal);
  worldN.y = dot(_unity_tbn_1, o.Normal);
  worldN.z = dot(_unity_tbn_2, o.Normal);
  worldN = normalize(worldN);
  o.Normal = worldN;

  // Setup lighting environment
  UnityGI gi;
  UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
  gi.indirect.diffuse = 0;
  gi.indirect.specular = 0;
  gi.light.color = _LightColor0.rgb;
  gi.light.dir = lightDir;
  // Call GI (lightmaps/SH/reflections) lighting function
  UnityGIInput giInput;
  UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
  giInput.light = gi.light;
  giInput.worldPos = worldPos;
  giInput.atten = atten;
  #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
    giInput.lightmapUV = IN.lmap;
  #else
    giInput.lightmapUV = 0.0;
  #endif
  #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
    giInput.ambient = IN.sh;
  #else
    giInput.ambient.rgb = 0.0;
  #endif
  giInput.probeHDR[0] = unity_SpecCube0_HDR;
  giInput.probeHDR[1] = unity_SpecCube1_HDR;
  #if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
    giInput.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
  #endif
  #ifdef UNITY_SPECCUBE_BOX_PROJECTION
    giInput.boxMax[0] = unity_SpecCube0_BoxMax;
    giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
    giInput.boxMax[1] = unity_SpecCube1_BoxMax;
    giInput.boxMin[1] = unity_SpecCube1_BoxMin;
    giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
  #endif
  LightingLambert_GI(o, giInput, gi);

  // realtime lighting: call lighting function
  c += LightingLambert (o, gi);
  c.rgb += o.Emission;
  UNITY_APPLY_FOG(_unity_fogCoord, c); // apply fog
  return c;
}


#endif

// -------- variant for: INSTANCING_ON 
#if defined(INSTANCING_ON)
// Surface shader code generated based on:
// writes to per-pixel normal: YES
// writes to emission: YES
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: no
// needs screen space position: no
// needs world space position: no
// needs view direction: YES
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: no
// needs world space view direction for lightmaps: no
// needs vertex color: no
// needs VFACE: no
// needs SV_IsFrontFace: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: no
// 4 texcoords actually used
//   float2 _MainTex
//   float2 _BumpMap
//   float2 _NoiseTex
//   float2 _NoiseMask
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "AutoLight.cginc"

#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

// Original surface shader snippet:
#line 66 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
                //#pragma surface surf Lambert alpha
                //#pragma target 3.0

                struct Input {
                    float2 uv_MainTex;
                    float3 worldNormal;
                    float3 viewDir;
                    float2 uv_BumpMap;
                    float2 uv_NoiseTex;
                    float2 uv_NoiseMask;
                    INTERNAL_DATA
                };

                    // Texture samplers
                    sampler2D _MainTex;
                    sampler2D _BumpMap;
                    sampler2D _NoiseTex;
                    sampler2D _NoiseMask;

                    // Color properties
                    fixed4 _Color;
                    float3 _FresnelColor;

                    // Intensity properties
                    float _FresnelExponent;
                    float _EmissionIntensity;
                    float _ColorIntensity;
                    float _AlphaIntensity;
                    float _BaseAlpha;

                    // Noise properties
                    float _NoiseIntensity;
                    float _NoiseScale;
                    float _NoiseSpeedX;
                    float _NoiseSpeedY;

                    // Distortion properties
                    float _DistortionIntensity;
                    float _DistortionScale;
                    float _DistortionSpeedX;
                    float _DistortionSpeedY;

                    void surf(Input IN, inout SurfaceOutput o) {
                        // Calculate animated UV offsets for noise
                        float2 noiseUV = IN.uv_NoiseTex * _NoiseScale;
                        noiseUV.x += _Time.y * _NoiseSpeedX;
                        noiseUV.y += _Time.y * _NoiseSpeedY;

                        // Calculate distortion UV
                        float2 distortUV = IN.uv_MainTex * _DistortionScale;
                        distortUV.x += _Time.y * _DistortionSpeedX;
                        distortUV.y += _Time.y * _DistortionSpeedY;

                        // Sample noise textures
                        float4 noise = tex2D(_NoiseTex, noiseUV);
                        float4 noiseMask = tex2D(_NoiseMask, IN.uv_NoiseMask);

                        // Create distortion from noise
                        float2 distortion = (noise.rg - 0.5) * 2.0 * _DistortionIntensity * noiseMask.r;

                        // Apply distortion to main texture UV
                        float2 distortedUV = IN.uv_MainTex + distortion;

                        // Sample main texture with distorted UVs
                        half4 mainTex = tex2D(_MainTex, distortedUV);

                        // Unpack normal map
                        o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap + distortion * 0.5));

                        // Calculate fresnel effect
                        float fresnel = dot(normalize(IN.viewDir), o.Normal);
                        fresnel = saturate(1 - fresnel);
                        fresnel = pow(fresnel, _FresnelExponent);

                        // Apply main color to texture (proper colorization)
                        float3 baseColor = mainTex.rgb * _Color.rgb * _ColorIntensity;

                        // Combine fresnel with its color
                        float3 fresnelColor = fresnel * _FresnelColor;

                        // Add noise effect to emission
                        float noiseEffect = noise.r * noiseMask.g * _NoiseIntensity;

                        // Set albedo with the properly colored texture
                        o.Albedo = baseColor;

                        // Set emission with intensity control and noise
                        o.Emission = (baseColor + fresnelColor + noiseEffect * _Color.rgb) * _EmissionIntensity;

                        // Calculate alpha separately without being affected by color
                        // Use base alpha plus fresnel for edge glow, multiplied by texture alpha
                        float finalAlpha = (_BaseAlpha + fresnel * _AlphaIntensity) * mainTex.a * _Color.a;

                        // Add subtle noise variation to alpha if desired
                        finalAlpha = saturate(finalAlpha + noiseEffect * 0.1);

                        o.Alpha = finalAlpha;
                    }
                    

// vertex-to-fragment interpolation data
// no lightmaps:
#ifndef LIGHTMAP_ON
// half-precision fragment shader registers:
#ifdef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
#define FOG_COMBINED_WITH_TSPACE
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 pack0 : TEXCOORD0; // _MainTex _BumpMap
  float4 pack1 : TEXCOORD1; // _NoiseTex _NoiseMask
  float4 tSpace0 : TEXCOORD2;
  float4 tSpace1 : TEXCOORD3;
  float4 tSpace2 : TEXCOORD4;
  #if UNITY_SHOULD_SAMPLE_SH
  half3 sh : TEXCOORD5; // SH
  #endif
  DECLARE_LIGHT_COORDS(6)
  #if SHADER_TARGET >= 30
  float4 lmap : TEXCOORD7;
  #endif
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
#endif
// high-precision fragment shader registers:
#ifndef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 pack0 : TEXCOORD0; // _MainTex _BumpMap
  float4 pack1 : TEXCOORD1; // _NoiseTex _NoiseMask
  float4 tSpace0 : TEXCOORD2;
  float4 tSpace1 : TEXCOORD3;
  float4 tSpace2 : TEXCOORD4;
  #if UNITY_SHOULD_SAMPLE_SH
  half3 sh : TEXCOORD5; // SH
  #endif
  UNITY_FOG_COORDS(6)
  DECLARE_LIGHT_COORDS(7)
  #if SHADER_TARGET >= 30
  float4 lmap : TEXCOORD8;
  #endif
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
#endif
#endif
// with lightmaps:
#ifdef LIGHTMAP_ON
// half-precision fragment shader registers:
#ifdef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
#define FOG_COMBINED_WITH_TSPACE
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 pack0 : TEXCOORD0; // _MainTex _BumpMap
  float4 pack1 : TEXCOORD1; // _NoiseTex _NoiseMask
  float4 tSpace0 : TEXCOORD2;
  float4 tSpace1 : TEXCOORD3;
  float4 tSpace2 : TEXCOORD4;
  float4 lmap : TEXCOORD5;
  DECLARE_LIGHT_COORDS(6)
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
#endif
// high-precision fragment shader registers:
#ifndef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 pack0 : TEXCOORD0; // _MainTex _BumpMap
  float4 pack1 : TEXCOORD1; // _NoiseTex _NoiseMask
  float4 tSpace0 : TEXCOORD2;
  float4 tSpace1 : TEXCOORD3;
  float4 tSpace2 : TEXCOORD4;
  float4 lmap : TEXCOORD5;
  UNITY_FOG_COORDS(6)
  DECLARE_LIGHT_COORDS(7)
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
#endif
#endif
float4 _MainTex_ST;
float4 _BumpMap_ST;
float4 _NoiseTex_ST;
float4 _NoiseMask_ST;

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  o.pos = UnityObjectToClipPos(v.vertex);
  o.pack0.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
  o.pack0.zw = TRANSFORM_TEX(v.texcoord, _BumpMap);
  o.pack1.xy = TRANSFORM_TEX(v.texcoord, _NoiseTex);
  o.pack1.zw = TRANSFORM_TEX(v.texcoord, _NoiseMask);
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  float3 worldNormal = UnityObjectToWorldNormal(v.normal);
  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
  o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
  o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
  o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
  #ifdef DYNAMICLIGHTMAP_ON
  o.lmap.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
  #endif
  #ifdef LIGHTMAP_ON
  o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
  #endif

  // SH/ambient and vertex lights
  #ifndef LIGHTMAP_ON
    #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
      o.sh = 0;
      // Approximated illumination from non-important point lights
      #ifdef VERTEXLIGHT_ON
        o.sh += Shade4PointLights (
          unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
          unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
          unity_4LightAtten0, worldPos, worldNormal);
      #endif
      o.sh = ShadeSHPerVertex (worldNormal, o.sh);
    #endif
  #endif // !LIGHTMAP_ON

  COMPUTE_LIGHT_COORDS(o); // pass light cookie coordinates to pixel shader
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_TRANSFER_FOG_COMBINED_WITH_TSPACE(o,o.pos); // pass fog coordinates to pixel shader
  #elif defined (FOG_COMBINED_WITH_WORLD_POS)
    UNITY_TRANSFER_FOG_COMBINED_WITH_WORLD_POS(o,o.pos); // pass fog coordinates to pixel shader
  #else
    UNITY_TRANSFER_FOG(o,o.pos); // pass fog coordinates to pixel shader
  #endif
  return o;
}

// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  UNITY_SETUP_INSTANCE_ID(IN);
  UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
  // prepare and unpack data
  Input surfIN;
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_EXTRACT_FOG_FROM_TSPACE(IN);
  #elif defined (FOG_COMBINED_WITH_WORLD_POS)
    UNITY_EXTRACT_FOG_FROM_WORLD_POS(IN);
  #else
    UNITY_EXTRACT_FOG(IN);
  #endif
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_RECONSTRUCT_TBN(IN);
  #else
    UNITY_EXTRACT_TBN(IN);
  #endif
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.uv_MainTex.x = 1.0;
  surfIN.worldNormal.x = 1.0;
  surfIN.viewDir.x = 1.0;
  surfIN.uv_BumpMap.x = 1.0;
  surfIN.uv_NoiseTex.x = 1.0;
  surfIN.uv_NoiseMask.x = 1.0;
  surfIN.uv_MainTex = IN.pack0.xy;
  surfIN.uv_BumpMap = IN.pack0.zw;
  surfIN.uv_NoiseTex = IN.pack1.xy;
  surfIN.uv_NoiseMask = IN.pack1.zw;
  float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
  float3 viewDir = _unity_tbn_0 * worldViewDir.x + _unity_tbn_1 * worldViewDir.y  + _unity_tbn_2 * worldViewDir.z;
  surfIN.viewDir = viewDir;
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutput o = (SurfaceOutput)0;
  #else
  SurfaceOutput o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Specular = 0.0;
  o.Alpha = 0.0;
  o.Gloss = 0.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);
  o.Normal = fixed3(0,0,1);

  // call surface function
  surf (surfIN, o);

  // compute lighting & shadowing factor
  UNITY_LIGHT_ATTENUATION(atten, IN, worldPos)
  fixed4 c = 0;
  float3 worldN;
  worldN.x = dot(_unity_tbn_0, o.Normal);
  worldN.y = dot(_unity_tbn_1, o.Normal);
  worldN.z = dot(_unity_tbn_2, o.Normal);
  worldN = normalize(worldN);
  o.Normal = worldN;

  // Setup lighting environment
  UnityGI gi;
  UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
  gi.indirect.diffuse = 0;
  gi.indirect.specular = 0;
  gi.light.color = _LightColor0.rgb;
  gi.light.dir = lightDir;
  // Call GI (lightmaps/SH/reflections) lighting function
  UnityGIInput giInput;
  UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
  giInput.light = gi.light;
  giInput.worldPos = worldPos;
  giInput.atten = atten;
  #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
    giInput.lightmapUV = IN.lmap;
  #else
    giInput.lightmapUV = 0.0;
  #endif
  #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
    giInput.ambient = IN.sh;
  #else
    giInput.ambient.rgb = 0.0;
  #endif
  giInput.probeHDR[0] = unity_SpecCube0_HDR;
  giInput.probeHDR[1] = unity_SpecCube1_HDR;
  #if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
    giInput.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
  #endif
  #ifdef UNITY_SPECCUBE_BOX_PROJECTION
    giInput.boxMax[0] = unity_SpecCube0_BoxMax;
    giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
    giInput.boxMax[1] = unity_SpecCube1_BoxMax;
    giInput.boxMin[1] = unity_SpecCube1_BoxMin;
    giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
  #endif
  LightingLambert_GI(o, giInput, gi);

  // realtime lighting: call lighting function
  c += LightingLambert (o, gi);
  c.rgb += o.Emission;
  UNITY_APPLY_FOG(_unity_fogCoord, c); // apply fog
  return c;
}


#endif



ENDHLSL
}
Pass { Name "FORWARD" Tags { "LightMode"="Source311ForwardAdd" } Blend SrcAlpha One ZWrite Off ColorMask RGB
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
#define INSTANCING_ON_KEYWORD_DECLARED (1 << 0)
#endif
#if defined(SHADER_STAGE_FRAGMENT)
#define FOG_LINEAR_KEYWORD_DECLARED (1 << 0)
#define FOG_EXP_KEYWORD_DECLARED (1 << 0)
#define FOG_EXP2_KEYWORD_DECLARED (1 << 0)
#define INSTANCING_ON_KEYWORD_DECLARED (1 << 0)
#endif
#line 429 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif

// compile directives
#pragma vertex vert_surf
#pragma fragment frag_surf
#pragma target 3.0
#pragma multi_compile_instancing
#pragma multi_compile_fog
#pragma skip_variants INSTANCING_ON
#pragma multi_compile_fwdadd noshadow
#include "HLSLSupport.cginc"
#define UNITY_INSTANCED_LOD_FADE
#define UNITY_INSTANCED_SH
#define UNITY_INSTANCED_LIGHTMAPSTS
#define UNITY_INSTANCED_RENDERER_BOUNDS
#include "UnityShaderVariables.cginc"
#include "UnityShaderUtilities.cginc"
// -------- variant for: <when no other keywords are defined>
#if !defined(INSTANCING_ON)
// Surface shader code generated based on:
// writes to per-pixel normal: YES
// writes to emission: YES
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: no
// needs screen space position: no
// needs world space position: no
// needs view direction: YES
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: no
// needs world space view direction for lightmaps: no
// needs vertex color: no
// needs VFACE: no
// needs SV_IsFrontFace: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: no
// 4 texcoords actually used
//   float2 _MainTex
//   float2 _BumpMap
//   float2 _NoiseTex
//   float2 _NoiseMask
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "AutoLight.cginc"

#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

// Original surface shader snippet:
#line 66 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
                //#pragma surface surf Lambert alpha
                //#pragma target 3.0

                struct Input {
                    float2 uv_MainTex;
                    float3 worldNormal;
                    float3 viewDir;
                    float2 uv_BumpMap;
                    float2 uv_NoiseTex;
                    float2 uv_NoiseMask;
                    INTERNAL_DATA
                };

                    // Texture samplers
                    sampler2D _MainTex;
                    sampler2D _BumpMap;
                    sampler2D _NoiseTex;
                    sampler2D _NoiseMask;

                    // Color properties
                    fixed4 _Color;
                    float3 _FresnelColor;

                    // Intensity properties
                    float _FresnelExponent;
                    float _EmissionIntensity;
                    float _ColorIntensity;
                    float _AlphaIntensity;
                    float _BaseAlpha;

                    // Noise properties
                    float _NoiseIntensity;
                    float _NoiseScale;
                    float _NoiseSpeedX;
                    float _NoiseSpeedY;

                    // Distortion properties
                    float _DistortionIntensity;
                    float _DistortionScale;
                    float _DistortionSpeedX;
                    float _DistortionSpeedY;

                    void surf(Input IN, inout SurfaceOutput o) {
                        // Calculate animated UV offsets for noise
                        float2 noiseUV = IN.uv_NoiseTex * _NoiseScale;
                        noiseUV.x += _Time.y * _NoiseSpeedX;
                        noiseUV.y += _Time.y * _NoiseSpeedY;

                        // Calculate distortion UV
                        float2 distortUV = IN.uv_MainTex * _DistortionScale;
                        distortUV.x += _Time.y * _DistortionSpeedX;
                        distortUV.y += _Time.y * _DistortionSpeedY;

                        // Sample noise textures
                        float4 noise = tex2D(_NoiseTex, noiseUV);
                        float4 noiseMask = tex2D(_NoiseMask, IN.uv_NoiseMask);

                        // Create distortion from noise
                        float2 distortion = (noise.rg - 0.5) * 2.0 * _DistortionIntensity * noiseMask.r;

                        // Apply distortion to main texture UV
                        float2 distortedUV = IN.uv_MainTex + distortion;

                        // Sample main texture with distorted UVs
                        half4 mainTex = tex2D(_MainTex, distortedUV);

                        // Unpack normal map
                        o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap + distortion * 0.5));

                        // Calculate fresnel effect
                        float fresnel = dot(normalize(IN.viewDir), o.Normal);
                        fresnel = saturate(1 - fresnel);
                        fresnel = pow(fresnel, _FresnelExponent);

                        // Apply main color to texture (proper colorization)
                        float3 baseColor = mainTex.rgb * _Color.rgb * _ColorIntensity;

                        // Combine fresnel with its color
                        float3 fresnelColor = fresnel * _FresnelColor;

                        // Add noise effect to emission
                        float noiseEffect = noise.r * noiseMask.g * _NoiseIntensity;

                        // Set albedo with the properly colored texture
                        o.Albedo = baseColor;

                        // Set emission with intensity control and noise
                        o.Emission = (baseColor + fresnelColor + noiseEffect * _Color.rgb) * _EmissionIntensity;

                        // Calculate alpha separately without being affected by color
                        // Use base alpha plus fresnel for edge glow, multiplied by texture alpha
                        float finalAlpha = (_BaseAlpha + fresnel * _AlphaIntensity) * mainTex.a * _Color.a;

                        // Add subtle noise variation to alpha if desired
                        finalAlpha = saturate(finalAlpha + noiseEffect * 0.1);

                        o.Alpha = finalAlpha;
                    }
                    

// vertex-to-fragment interpolation data
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 pack0 : TEXCOORD0; // _MainTex _BumpMap
  float4 pack1 : TEXCOORD1; // _NoiseTex _NoiseMask
  float3 tSpace0 : TEXCOORD2;
  float3 tSpace1 : TEXCOORD3;
  float3 tSpace2 : TEXCOORD4;
  float3 worldPos : TEXCOORD5;
  DECLARE_LIGHT_COORDS(6)
  UNITY_FOG_COORDS(7)
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
float4 _MainTex_ST;
float4 _BumpMap_ST;
float4 _NoiseTex_ST;
float4 _NoiseMask_ST;

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  o.pos = UnityObjectToClipPos(v.vertex);
  o.pack0.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
  o.pack0.zw = TRANSFORM_TEX(v.texcoord, _BumpMap);
  o.pack1.xy = TRANSFORM_TEX(v.texcoord, _NoiseTex);
  o.pack1.zw = TRANSFORM_TEX(v.texcoord, _NoiseMask);
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  float3 worldNormal = UnityObjectToWorldNormal(v.normal);
  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
  o.tSpace0 = float3(worldTangent.x, worldBinormal.x, worldNormal.x);
  o.tSpace1 = float3(worldTangent.y, worldBinormal.y, worldNormal.y);
  o.tSpace2 = float3(worldTangent.z, worldBinormal.z, worldNormal.z);
  o.worldPos.xyz = worldPos;

  COMPUTE_LIGHT_COORDS(o); // pass light cookie coordinates to pixel shader
  UNITY_TRANSFER_FOG(o,o.pos); // pass fog coordinates to pixel shader
  return o;
}

// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  UNITY_SETUP_INSTANCE_ID(IN);
  UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
  // prepare and unpack data
  Input surfIN;
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_EXTRACT_FOG_FROM_TSPACE(IN);
  #elif defined (FOG_COMBINED_WITH_WORLD_POS)
    UNITY_EXTRACT_FOG_FROM_WORLD_POS(IN);
  #else
    UNITY_EXTRACT_FOG(IN);
  #endif
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_RECONSTRUCT_TBN(IN);
  #else
    UNITY_EXTRACT_TBN(IN);
  #endif
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.uv_MainTex.x = 1.0;
  surfIN.worldNormal.x = 1.0;
  surfIN.viewDir.x = 1.0;
  surfIN.uv_BumpMap.x = 1.0;
  surfIN.uv_NoiseTex.x = 1.0;
  surfIN.uv_NoiseMask.x = 1.0;
  surfIN.uv_MainTex = IN.pack0.xy;
  surfIN.uv_BumpMap = IN.pack0.zw;
  surfIN.uv_NoiseTex = IN.pack1.xy;
  surfIN.uv_NoiseMask = IN.pack1.zw;
  float3 worldPos = IN.worldPos.xyz;
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
  float3 viewDir = _unity_tbn_0 * worldViewDir.x + _unity_tbn_1 * worldViewDir.y  + _unity_tbn_2 * worldViewDir.z;
  surfIN.viewDir = viewDir;
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutput o = (SurfaceOutput)0;
  #else
  SurfaceOutput o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Specular = 0.0;
  o.Alpha = 0.0;
  o.Gloss = 0.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);
  o.Normal = fixed3(0,0,1);

  // call surface function
  surf (surfIN, o);
  UNITY_LIGHT_ATTENUATION(atten, IN, worldPos)
  fixed4 c = 0;
  float3 worldN;
  worldN.x = dot(_unity_tbn_0, o.Normal);
  worldN.y = dot(_unity_tbn_1, o.Normal);
  worldN.z = dot(_unity_tbn_2, o.Normal);
  worldN = normalize(worldN);
  o.Normal = worldN;

  // Setup lighting environment
  UnityGI gi;
  UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
  gi.indirect.diffuse = 0;
  gi.indirect.specular = 0;
  gi.light.color = _LightColor0.rgb;
  gi.light.dir = lightDir;
  gi.light.color *= atten;
  c += LightingLambert (o, gi);
  UNITY_APPLY_FOG(_unity_fogCoord, c); // apply fog
  return c;
}


#endif



ENDHLSL
}
Pass { Name "Meta" Tags { "LightMode"="Meta" } Cull Off
HLSLPROGRAM
#include "HLSLSupport.cginc"
#include "Assets/URP311/Source/Source311ShaderVariables.hlsl"
#define UNITY_PASS_META 1
#define KEYWORD_TYPE_FLAG_SPECIALIZED_VARIANTS (1 << 0)
#define KEYWORD_TYPE_FLAG_RUNTIME_BRANCHING (1 << 1)
#if defined(SHADER_STAGE_VERTEX)
#define EDITOR_VISUALIZATION_KEYWORD_DECLARED (1 << 0)
#define INSTANCING_ON_KEYWORD_DECLARED (1 << 0)
#endif
#if defined(SHADER_STAGE_FRAGMENT)
#define EDITOR_VISUALIZATION_KEYWORD_DECLARED (1 << 0)
#define INSTANCING_ON_KEYWORD_DECLARED (1 << 0)
#endif
#line 302 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif

// compile directives
#pragma vertex vert_surf
#pragma fragment frag_surf
#pragma target 3.0
#pragma multi_compile_instancing
#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
#pragma shader_feature EDITOR_VISUALIZATION

#include "HLSLSupport.cginc"
#define UNITY_INSTANCED_LOD_FADE
#define UNITY_INSTANCED_SH
#define UNITY_INSTANCED_LIGHTMAPSTS
#define UNITY_INSTANCED_RENDERER_BOUNDS
#include "UnityShaderVariables.cginc"
#include "UnityShaderUtilities.cginc"
// -------- variant for: <when no other keywords are defined>
#if !defined(INSTANCING_ON)
// Surface shader code generated based on:
// writes to per-pixel normal: YES
// writes to emission: YES
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: no
// needs screen space position: no
// needs world space position: no
// needs view direction: YES
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: no
// needs world space view direction for lightmaps: no
// needs vertex color: no
// needs VFACE: no
// needs SV_IsFrontFace: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: no
// 4 texcoords actually used
//   float2 _MainTex
//   float2 _BumpMap
//   float2 _NoiseTex
//   float2 _NoiseMask
#include "UnityCG.cginc"
#include "Lighting.cginc"

#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

// Original surface shader snippet:
#line 66 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
                //#pragma surface surf Lambert alpha
                //#pragma target 3.0

                struct Input {
                    float2 uv_MainTex;
                    float3 worldNormal;
                    float3 viewDir;
                    float2 uv_BumpMap;
                    float2 uv_NoiseTex;
                    float2 uv_NoiseMask;
                    INTERNAL_DATA
                };

                    // Texture samplers
                    sampler2D _MainTex;
                    sampler2D _BumpMap;
                    sampler2D _NoiseTex;
                    sampler2D _NoiseMask;

                    // Color properties
                    fixed4 _Color;
                    float3 _FresnelColor;

                    // Intensity properties
                    float _FresnelExponent;
                    float _EmissionIntensity;
                    float _ColorIntensity;
                    float _AlphaIntensity;
                    float _BaseAlpha;

                    // Noise properties
                    float _NoiseIntensity;
                    float _NoiseScale;
                    float _NoiseSpeedX;
                    float _NoiseSpeedY;

                    // Distortion properties
                    float _DistortionIntensity;
                    float _DistortionScale;
                    float _DistortionSpeedX;
                    float _DistortionSpeedY;

                    void surf(Input IN, inout SurfaceOutput o) {
                        // Calculate animated UV offsets for noise
                        float2 noiseUV = IN.uv_NoiseTex * _NoiseScale;
                        noiseUV.x += _Time.y * _NoiseSpeedX;
                        noiseUV.y += _Time.y * _NoiseSpeedY;

                        // Calculate distortion UV
                        float2 distortUV = IN.uv_MainTex * _DistortionScale;
                        distortUV.x += _Time.y * _DistortionSpeedX;
                        distortUV.y += _Time.y * _DistortionSpeedY;

                        // Sample noise textures
                        float4 noise = tex2D(_NoiseTex, noiseUV);
                        float4 noiseMask = tex2D(_NoiseMask, IN.uv_NoiseMask);

                        // Create distortion from noise
                        float2 distortion = (noise.rg - 0.5) * 2.0 * _DistortionIntensity * noiseMask.r;

                        // Apply distortion to main texture UV
                        float2 distortedUV = IN.uv_MainTex + distortion;

                        // Sample main texture with distorted UVs
                        half4 mainTex = tex2D(_MainTex, distortedUV);

                        // Unpack normal map
                        o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap + distortion * 0.5));

                        // Calculate fresnel effect
                        float fresnel = dot(normalize(IN.viewDir), o.Normal);
                        fresnel = saturate(1 - fresnel);
                        fresnel = pow(fresnel, _FresnelExponent);

                        // Apply main color to texture (proper colorization)
                        float3 baseColor = mainTex.rgb * _Color.rgb * _ColorIntensity;

                        // Combine fresnel with its color
                        float3 fresnelColor = fresnel * _FresnelColor;

                        // Add noise effect to emission
                        float noiseEffect = noise.r * noiseMask.g * _NoiseIntensity;

                        // Set albedo with the properly colored texture
                        o.Albedo = baseColor;

                        // Set emission with intensity control and noise
                        o.Emission = (baseColor + fresnelColor + noiseEffect * _Color.rgb) * _EmissionIntensity;

                        // Calculate alpha separately without being affected by color
                        // Use base alpha plus fresnel for edge glow, multiplied by texture alpha
                        float finalAlpha = (_BaseAlpha + fresnel * _AlphaIntensity) * mainTex.a * _Color.a;

                        // Add subtle noise variation to alpha if desired
                        finalAlpha = saturate(finalAlpha + noiseEffect * 0.1);

                        o.Alpha = finalAlpha;
                    }
                    
#include "UnityMetaPass.cginc"

// vertex-to-fragment interpolation data
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 pack0 : TEXCOORD0; // _MainTex _BumpMap
  float4 pack1 : TEXCOORD1; // _NoiseTex _NoiseMask
  float4 tSpace0 : TEXCOORD2;
  float4 tSpace1 : TEXCOORD3;
  float4 tSpace2 : TEXCOORD4;
#ifdef EDITOR_VISUALIZATION
  float2 vizUV : TEXCOORD5;
  float4 lightCoord : TEXCOORD6;
#endif
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
float4 _MainTex_ST;
float4 _BumpMap_ST;
float4 _NoiseTex_ST;
float4 _NoiseMask_ST;

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  o.pos = UnityMetaVertexPosition(v.vertex, v.texcoord1.xy, v.texcoord2.xy, unity_LightmapST, unity_DynamicLightmapST);
#ifdef EDITOR_VISUALIZATION
  o.vizUV = 0;
  o.lightCoord = 0;
  if (unity_VisualizationMode == EDITORVIZ_TEXTURE)
    o.vizUV = UnityMetaVizUV(unity_EditorViz_UVIndex, v.texcoord.xy, v.texcoord1.xy, v.texcoord2.xy, unity_EditorViz_Texture_ST);
  else if (unity_VisualizationMode == EDITORVIZ_SHOWLIGHTMASK)
  {
    o.vizUV = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
    o.lightCoord = mul(unity_EditorViz_WorldToLight, mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1)));
  }
#endif
  o.pack0.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
  o.pack0.zw = TRANSFORM_TEX(v.texcoord, _BumpMap);
  o.pack1.xy = TRANSFORM_TEX(v.texcoord, _NoiseTex);
  o.pack1.zw = TRANSFORM_TEX(v.texcoord, _NoiseMask);
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  float3 worldNormal = UnityObjectToWorldNormal(v.normal);
  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
  o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
  o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
  o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
  return o;
}

// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  UNITY_SETUP_INSTANCE_ID(IN);
  UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
  // prepare and unpack data
  Input surfIN;
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_EXTRACT_FOG_FROM_TSPACE(IN);
  #elif defined (FOG_COMBINED_WITH_WORLD_POS)
    UNITY_EXTRACT_FOG_FROM_WORLD_POS(IN);
  #else
    UNITY_EXTRACT_FOG(IN);
  #endif
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_RECONSTRUCT_TBN(IN);
  #else
    UNITY_EXTRACT_TBN(IN);
  #endif
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.uv_MainTex.x = 1.0;
  surfIN.worldNormal.x = 1.0;
  surfIN.viewDir.x = 1.0;
  surfIN.uv_BumpMap.x = 1.0;
  surfIN.uv_NoiseTex.x = 1.0;
  surfIN.uv_NoiseMask.x = 1.0;
  surfIN.uv_MainTex = IN.pack0.xy;
  surfIN.uv_BumpMap = IN.pack0.zw;
  surfIN.uv_NoiseTex = IN.pack1.xy;
  surfIN.uv_NoiseMask = IN.pack1.zw;
  float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
  float3 viewDir = _unity_tbn_0 * worldViewDir.x + _unity_tbn_1 * worldViewDir.y  + _unity_tbn_2 * worldViewDir.z;
  surfIN.viewDir = viewDir;
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutput o = (SurfaceOutput)0;
  #else
  SurfaceOutput o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Specular = 0.0;
  o.Alpha = 0.0;
  o.Gloss = 0.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);

  // call surface function
  surf (surfIN, o);
  UnityMetaInput metaIN;
  UNITY_INITIALIZE_OUTPUT(UnityMetaInput, metaIN);
  metaIN.Albedo = o.Albedo;
  metaIN.Emission = o.Emission;
  metaIN.SpecularColor = o.Specular;
#ifdef EDITOR_VISUALIZATION
  metaIN.VizUV = IN.vizUV;
  metaIN.LightCoord = IN.lightCoord;
#endif
  return UnityMetaFragment(metaIN);
}


#endif

// -------- variant for: INSTANCING_ON 
#if defined(INSTANCING_ON)
// Surface shader code generated based on:
// writes to per-pixel normal: YES
// writes to emission: YES
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: no
// needs screen space position: no
// needs world space position: no
// needs view direction: YES
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: no
// needs world space view direction for lightmaps: no
// needs vertex color: no
// needs VFACE: no
// needs SV_IsFrontFace: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: no
// 4 texcoords actually used
//   float2 _MainTex
//   float2 _BumpMap
//   float2 _NoiseTex
//   float2 _NoiseMask
#include "UnityCG.cginc"
#include "Lighting.cginc"

#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

// Original surface shader snippet:
#line 66 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
                //#pragma surface surf Lambert alpha
                //#pragma target 3.0

                struct Input {
                    float2 uv_MainTex;
                    float3 worldNormal;
                    float3 viewDir;
                    float2 uv_BumpMap;
                    float2 uv_NoiseTex;
                    float2 uv_NoiseMask;
                    INTERNAL_DATA
                };

                    // Texture samplers
                    sampler2D _MainTex;
                    sampler2D _BumpMap;
                    sampler2D _NoiseTex;
                    sampler2D _NoiseMask;

                    // Color properties
                    fixed4 _Color;
                    float3 _FresnelColor;

                    // Intensity properties
                    float _FresnelExponent;
                    float _EmissionIntensity;
                    float _ColorIntensity;
                    float _AlphaIntensity;
                    float _BaseAlpha;

                    // Noise properties
                    float _NoiseIntensity;
                    float _NoiseScale;
                    float _NoiseSpeedX;
                    float _NoiseSpeedY;

                    // Distortion properties
                    float _DistortionIntensity;
                    float _DistortionScale;
                    float _DistortionSpeedX;
                    float _DistortionSpeedY;

                    void surf(Input IN, inout SurfaceOutput o) {
                        // Calculate animated UV offsets for noise
                        float2 noiseUV = IN.uv_NoiseTex * _NoiseScale;
                        noiseUV.x += _Time.y * _NoiseSpeedX;
                        noiseUV.y += _Time.y * _NoiseSpeedY;

                        // Calculate distortion UV
                        float2 distortUV = IN.uv_MainTex * _DistortionScale;
                        distortUV.x += _Time.y * _DistortionSpeedX;
                        distortUV.y += _Time.y * _DistortionSpeedY;

                        // Sample noise textures
                        float4 noise = tex2D(_NoiseTex, noiseUV);
                        float4 noiseMask = tex2D(_NoiseMask, IN.uv_NoiseMask);

                        // Create distortion from noise
                        float2 distortion = (noise.rg - 0.5) * 2.0 * _DistortionIntensity * noiseMask.r;

                        // Apply distortion to main texture UV
                        float2 distortedUV = IN.uv_MainTex + distortion;

                        // Sample main texture with distorted UVs
                        half4 mainTex = tex2D(_MainTex, distortedUV);

                        // Unpack normal map
                        o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap + distortion * 0.5));

                        // Calculate fresnel effect
                        float fresnel = dot(normalize(IN.viewDir), o.Normal);
                        fresnel = saturate(1 - fresnel);
                        fresnel = pow(fresnel, _FresnelExponent);

                        // Apply main color to texture (proper colorization)
                        float3 baseColor = mainTex.rgb * _Color.rgb * _ColorIntensity;

                        // Combine fresnel with its color
                        float3 fresnelColor = fresnel * _FresnelColor;

                        // Add noise effect to emission
                        float noiseEffect = noise.r * noiseMask.g * _NoiseIntensity;

                        // Set albedo with the properly colored texture
                        o.Albedo = baseColor;

                        // Set emission with intensity control and noise
                        o.Emission = (baseColor + fresnelColor + noiseEffect * _Color.rgb) * _EmissionIntensity;

                        // Calculate alpha separately without being affected by color
                        // Use base alpha plus fresnel for edge glow, multiplied by texture alpha
                        float finalAlpha = (_BaseAlpha + fresnel * _AlphaIntensity) * mainTex.a * _Color.a;

                        // Add subtle noise variation to alpha if desired
                        finalAlpha = saturate(finalAlpha + noiseEffect * 0.1);

                        o.Alpha = finalAlpha;
                    }
                    
#include "UnityMetaPass.cginc"

// vertex-to-fragment interpolation data
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 pack0 : TEXCOORD0; // _MainTex _BumpMap
  float4 pack1 : TEXCOORD1; // _NoiseTex _NoiseMask
  float4 tSpace0 : TEXCOORD2;
  float4 tSpace1 : TEXCOORD3;
  float4 tSpace2 : TEXCOORD4;
#ifdef EDITOR_VISUALIZATION
  float2 vizUV : TEXCOORD5;
  float4 lightCoord : TEXCOORD6;
#endif
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
float4 _MainTex_ST;
float4 _BumpMap_ST;
float4 _NoiseTex_ST;
float4 _NoiseMask_ST;

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  o.pos = UnityMetaVertexPosition(v.vertex, v.texcoord1.xy, v.texcoord2.xy, unity_LightmapST, unity_DynamicLightmapST);
#ifdef EDITOR_VISUALIZATION
  o.vizUV = 0;
  o.lightCoord = 0;
  if (unity_VisualizationMode == EDITORVIZ_TEXTURE)
    o.vizUV = UnityMetaVizUV(unity_EditorViz_UVIndex, v.texcoord.xy, v.texcoord1.xy, v.texcoord2.xy, unity_EditorViz_Texture_ST);
  else if (unity_VisualizationMode == EDITORVIZ_SHOWLIGHTMASK)
  {
    o.vizUV = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
    o.lightCoord = mul(unity_EditorViz_WorldToLight, mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1)));
  }
#endif
  o.pack0.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
  o.pack0.zw = TRANSFORM_TEX(v.texcoord, _BumpMap);
  o.pack1.xy = TRANSFORM_TEX(v.texcoord, _NoiseTex);
  o.pack1.zw = TRANSFORM_TEX(v.texcoord, _NoiseMask);
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  float3 worldNormal = UnityObjectToWorldNormal(v.normal);
  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
  o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
  o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
  o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
  return o;
}

// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  UNITY_SETUP_INSTANCE_ID(IN);
  UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
  // prepare and unpack data
  Input surfIN;
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_EXTRACT_FOG_FROM_TSPACE(IN);
  #elif defined (FOG_COMBINED_WITH_WORLD_POS)
    UNITY_EXTRACT_FOG_FROM_WORLD_POS(IN);
  #else
    UNITY_EXTRACT_FOG(IN);
  #endif
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_RECONSTRUCT_TBN(IN);
  #else
    UNITY_EXTRACT_TBN(IN);
  #endif
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.uv_MainTex.x = 1.0;
  surfIN.worldNormal.x = 1.0;
  surfIN.viewDir.x = 1.0;
  surfIN.uv_BumpMap.x = 1.0;
  surfIN.uv_NoiseTex.x = 1.0;
  surfIN.uv_NoiseMask.x = 1.0;
  surfIN.uv_MainTex = IN.pack0.xy;
  surfIN.uv_BumpMap = IN.pack0.zw;
  surfIN.uv_NoiseTex = IN.pack1.xy;
  surfIN.uv_NoiseMask = IN.pack1.zw;
  float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
  float3 viewDir = _unity_tbn_0 * worldViewDir.x + _unity_tbn_1 * worldViewDir.y  + _unity_tbn_2 * worldViewDir.z;
  surfIN.viewDir = viewDir;
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutput o = (SurfaceOutput)0;
  #else
  SurfaceOutput o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Specular = 0.0;
  o.Alpha = 0.0;
  o.Gloss = 0.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);

  // call surface function
  surf (surfIN, o);
  UnityMetaInput metaIN;
  UNITY_INITIALIZE_OUTPUT(UnityMetaInput, metaIN);
  metaIN.Albedo = o.Albedo;
  metaIN.Emission = o.Emission;
  metaIN.SpecularColor = o.Specular;
#ifdef EDITOR_VISUALIZATION
  metaIN.VizUV = IN.vizUV;
  metaIN.LightCoord = IN.lightCoord;
#endif
  return UnityMetaFragment(metaIN);
}


#endif



ENDHLSL
}
}
Fallback Off
}
