Shader "Force Field" {
    Properties{
        [Header(Main Settings)]
        _MainTex("Main Texture", 2D) = "white" {}
        _Color("Color", Color) = (1, 1, 1, 1)
        _RimIntensity("Rim Intensity", Range(0, 10)) = 1.7

        [Header(Ripple Distortion Effect)]
        _NoiseTex("Ripple Noise Texture (R channel)", 2D) = "gray" {}
        _RippleScale("Ripple Scale (Tiling)", Float) = 10
        _RippleSpeed("Ripple Scroll Speed", Float) = 0.5
        _RippleIntensity("Ripple Distortion Intensity", Float) = 0.05

        [Header(Secondary Mask Effect)]
        _MaskTex("Secondary Mask Texture (R channel)", 2D) = "gray" {}
        _MaskScale("Mask Scale (Tiling)", Float) = 5
        _MaskSpeed("Mask Scroll Speed", Float) = -0.3
        _MaskWarp("Mask Warp Amount", Float) = 0.1
    }
        SubShader{
            Pass {
                // --- CORRECTION: Using pre-multiplied additive blending ---
                // This provides the additive "glow" look while still respecting the source alpha for fading.
                Blend SrcAlpha One
                // --- END CORRECTION ---

                Cull Off
                ZWrite Off

                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag

                #include "UnityCG.cginc"

                struct v2f {
                    float4 pos : SV_POSITION;
                    float3 normal : NORMAL;
                    float2 uv : TEXCOORD0;
                    float3 viewDir : TEXCOORD1;
                };

                sampler2D _MainTex;
                sampler2D _NoiseTex;
                sampler2D _MaskTex;

                float4 _MainTex_ST;

                fixed4 _Color;
                float _RimIntensity;
                float _RippleScale;
                float _RippleSpeed;
                float _RippleIntensity;
                float _MaskScale;
                float _MaskSpeed;
                float _MaskWarp;

                v2f vert(appdata_full v) {
                    v2f o;
                    o.pos = UnityObjectToClipPos(v.vertex);
                    o.normal = UnityObjectToWorldNormal(v.normal);

                    float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                    o.viewDir = normalize(_WorldSpaceCameraPos.xyz - worldPos);

                    o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                    return o;
                }

                fixed4 frag(v2f i) : COLOR {
                    // 1. RIPPLE/DISTORTION EFFECT
                    float2 noiseUV = i.uv * _RippleScale;
                    noiseUV.x += _Time.y * _RippleSpeed;

                    float noiseValue = (tex2D(_NoiseTex, noiseUV).r * 2.0 - 1.0) * _RippleIntensity;

                    // 2. SECONDARY MASK EFFECT
                    float2 maskUV = i.uv * _MaskScale;
                    maskUV.y -= _Time.y * _MaskSpeed;

                    maskUV.x += noiseValue * _MaskWarp;

                    // 3. APPLY DISTORTION
                    float2 mainUV = i.uv + noiseValue;

                    // 4. SAMPLE TEXTURES
                    fixed4 mainTexColor = tex2D(_MainTex, mainUV);
                    fixed maskValue = tex2D(_MaskTex, maskUV).r;

                    // 5. CALCULATE RIM/FRESNEL EFFECT
                    float val = 0.95 - abs(dot(i.viewDir, i.normal.yz));
                    float val2 = 1.5 - abs(dot(i.viewDir, i.normal.zx));
                    fixed rim = val * val2;

                    // 6. COMBINE EVERYTHING
                    // Start with the base color
                    fixed4 finalColor = _Color;

                    // The final alpha is determined by the color's alpha, the textures, and the rim effect.
                    // This value will be used by the 'Blend SrcAlpha One' operation.
                    finalColor.a *= rim * mainTexColor.a * maskValue;

                    // The final RGB is multiplied by the textures and intensity, but not the alpha,
                    // because the alpha is now used as a blending factor, not a color component.
                    finalColor.rgb *= rim * mainTexColor.rgb * maskValue * _RimIntensity;

                    return finalColor;
                }
                ENDCG
            }
        }
}