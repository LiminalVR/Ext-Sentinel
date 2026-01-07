Shader "Custom/Ghost_Improved" {
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

        SubShader{
            Tags {"RenderType" = "Transparent" "Queue" = "Transparent" "IgnoreProjector" = "True"}
            LOD 200

                // First pass for depth writing
                Pass {
                    ZWrite On
                    ColorMask 0

                    CGPROGRAM
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
                    ENDCG
                }

                // Main surface shader pass
                CGPROGRAM
                #pragma surface surf Lambert alpha
                #pragma target 3.0

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
                    ENDCG
            }

                Fallback "Transparent/Diffuse"
}