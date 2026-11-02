Shader "GalaxiaReferencia/Estrela Suave"
{
    Properties
    {
        _Intensidade ("Intensidade", Range(0.1, 8)) = 2.0
        _Maciez ("Suavidade", Range(0.15, 2)) = 0.8
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Cull Off ZWrite Off ZTest LEqual Blend SrcAlpha One
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float _Intensidade;
            float _Maciez;

            struct dados
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };
            struct saida
            {
                float4 pos : SV_POSITION;
                float4 cor : COLOR;
                float2 uv : TEXCOORD0;
            };
            saida vert(dados v)
            {
                saida o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.cor = v.color;
                o.uv = v.uv;
                return o;
            }
            float4 frag(saida i) : SV_Target
            {
                float2 d = i.uv * 2.0 - 1.0;
                float r2 = dot(d, d);
                // O gradiente radial impede que as partículas sejam quadrados brilhantes.
                float estrela = exp(-r2 * lerp(12.0, 3.0, saturate(_Maciez)));
                float aureola = exp(-r2 * 1.25) * 0.12 * _Maciez;
                float forma = saturate(estrela + aureola);
                float3 cor = i.cor.rgb * _Intensidade;
                return float4(cor * forma, saturate(i.cor.a) * forma);
            }
            ENDCG
        }
    }
    FallBack Off
}
