// Solid-colour outline for small props such as coins. It draws a copy of the mesh scaled up around a centre point,
// showing only its back faces, so the copy peeks out as a rim around the real mesh drawn in front of it.
// Used as an extra material on the prop's renderer, which makes Unity draw the mesh a second time with it.
Shader "PetShiftRun/Outline"
{
    Properties
    {
        _OutlineColor ("Outline Color", Color) = (0.12, 0.13, 0.2, 1)
        _OutlineScale ("Outline Scale", Float) = 1.2
        _Center ("Object-Space Centre", Vector) = (0, 0, 0, 0)
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }

        Pass
        {
            Name "Outline"
            Tags { "LightMode" = "SRPDefaultUnlit" }
            Cull Front
            ZWrite On

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _OutlineColor;
                float _OutlineScale;
                float4 _Center;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float fogFactor : TEXCOORD0;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                float3 position = _Center.xyz + (input.positionOS.xyz - _Center.xyz) * _OutlineScale;
                output.positionCS = TransformObjectToHClip(position);
                output.fogFactor = ComputeFogFactor(output.positionCS.z);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                return half4(MixFog(_OutlineColor.rgb, input.fogFactor), 1);
            }
            ENDHLSL
        }
    }
}
