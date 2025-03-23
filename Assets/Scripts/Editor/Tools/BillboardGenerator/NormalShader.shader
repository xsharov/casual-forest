Shader "Hidden/NormalShader"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Name "DrawNormals"
            Tags { "LightMode"="UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS   : TEXCOORD0;
            };

            Varyings vert (Attributes input)
            {
                Varyings output;
                float3 worldPos = TransformObjectToWorld(input.vertex.xyz);
                output.positionCS = TransformWorldToHClip(worldPos);
                output.normalWS = normalize(TransformObjectToWorldNormal(input.normal));
                return output;
            }

            half4 frag (Varyings input) : SV_Target
            {
                float3 color = input.normalWS * 0.5 + 0.5;
                return half4(color, 1.0);
            }
            ENDHLSL
        }
    }
}
