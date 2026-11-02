Shader "GalaxiaReferencia/Fio Aditivo"
{
    Properties { _Intensidade ("Intensidade", Range(0, 8)) = 2 }
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
            struct entrada { float4 vertex : POSITION; float4 color : COLOR; float2 uv : TEXCOORD0; };
            struct saida { float4 pos : SV_POSITION; float4 cor : COLOR; float2 uv : TEXCOORD0; };
            saida vert(entrada v)
            {
                saida o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.cor = v.color;
                o.uv = v.uv;
                return o;
            }
            float4 frag(saida i) : SV_Target
            {
                float transversal = abs(i.uv.y * 2.0 - 1.0);
                float brilho = exp(-transversal * transversal * 3.5);
                return float4(i.cor.rgb * _Intensidade * brilho, i.cor.a * brilho);
            }
            ENDCG
        }
    }
    FallBack Off
}
