Shader "Custom/ReticleMask_URP_LitHDR"
{
    Properties
    {
        [HDR]_BaseColor("Base Color", Color) = (1,1,1,1)
        _MainTex("Base Map", 2D) = "white" {}
        _UseTextureAlpha("Use Texture Alpha", Float) = 1 // 1 = use texture alpha, 0 = derive alpha from color (black -> transparent)
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "DisableBatching"="False" }
        LOD 200

        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode"="UniversalForward" }

            // Transparent settings
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest LEqual

            // Stencil: only render where mask wrote 1
            Stencil
            {
                Ref 1
                Comp Equal
                Pass Keep
                Fail Keep
                ZFail Keep
            }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHTS

            // URP includes - these are the standard package includes for URP
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            // Texture / sampler
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float4 _BaseColor;
            float _UseTextureAlpha;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS   : TEXCOORD0;
                float2 uv         : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                float3 worldPos = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.positionWS = worldPos;
                OUT.normalWS = normalize(TransformObjectToWorldNormal(IN.normalOS));
                OUT.uv = IN.uv;
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
                return OUT;
            }

            // Simple utility: convert tex.rgb -> perceived brightness
            inline half Brightness(half3 c)
            {
                // a neutral average; you can swap to luminance weights if wanted
                return dot(c, half3(0.3333, 0.3333, 0.3333));
            }

            float4 frag(Varyings IN) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);
                // sample texture
                half4 texCol = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);

                // base HDR color (texture * base color). _BaseColor can be >1 (HDR)
                half3 baseColor = texCol.rgb * _BaseColor.rgb;

                // alpha logic
                half alpha;
                if (_UseTextureAlpha > 0.5)
                {
                    alpha = texCol.a * _BaseColor.a;
                }
                else
                {
                    alpha = Brightness(texCol.rgb) * _BaseColor.a;
                }

                // If fully transparent early-out (optional small optimization)
                clip(alpha - 0.001);

                // Simple lit shading so URP volume affects result.
                // We'll sample URP's main directional light and apply a lambert term + small ambient.
                Light mainLight = GetMainLight(); // URP Lighting.hlsl function
                half3 lightDir = -mainLight.direction; // direction points from light -> surface, so negate for L
                half NdotL = max(dot(IN.normalWS, lightDir), 0.0);

                // Main light color is already linear HDR; include it
                half3 lit = baseColor * (mainLight.color * NdotL + 0.18); // 0.18 small ambient term

                // Let URP's postprocessing / exposure / color grading affect the final color:
                // return color with alpha.
                return float4(lit, alpha);
            }

            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Lit"
}
