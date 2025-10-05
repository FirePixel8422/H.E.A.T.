Shader "Custom/ReticleMask_URP_LitHDR"
{
    Properties
    {
        [HDR]_BaseColor("Base Color", Color) = (1,1,1,1)
        _MainTex("Base Map", 2D) = "white" {}
        _UseTextureAlpha("Use Texture Alpha", Float) = 1
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "DisableBatching"="False" }
        LOD 200

        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode"="UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest LEqual

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
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHTS _ADDITIONAL_LIGHTS_VERTEX
            #pragma multi_compile_fragment _ _LIGHT_LAYERS
            #pragma multi_compile_fragment _ _RECEIVE_SHADOWS_OFF _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile_fragment _ _SURFACE_TYPE_TRANSPARENT _ALPHATEST_ON _ALPHAPREMULTIPLY_ON _ALPHAMODULATE_ON _EMISSION

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

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

            inline half Brightness(half3 c)
            {
                return dot(c, half3(0.3333,0.3333,0.3333));
            }

            float4 frag(Varyings IN) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);
                half4 texCol = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                half3 baseColor = texCol.rgb * _BaseColor.rgb;

                half alpha = (_UseTextureAlpha > 0.5) ? texCol.a * _BaseColor.a : Brightness(texCol.rgb) * _BaseColor.a;
                clip(alpha - 0.001);

                Light mainLight = GetMainLight();
                half3 lightDir = -mainLight.direction;
                half NdotL = max(dot(IN.normalWS, lightDir), 0.0);

                half3 lit = baseColor * (mainLight.color * NdotL + 0.18);

                return float4(lit, alpha);
            }

            ENDHLSL
        }
    }

    FallBack Off
}
