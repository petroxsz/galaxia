Shader "Hidden/GalaxiaReferencia/Bloom"
{
    Properties { _MainTex ("Imagem", 2D) = "black" {} }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Cull Off ZWrite Off ZTest Always

        // Pass 0: isola os pontos suficientemente brilhantes.
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            float _Limiar;
            float4 frag(v2f_img i) : SV_Target
            {
                float3 cor = tex2D(_MainTex, i.uv).rgb;
                float luz = dot(cor, float3(.2126, .7152, .0722));
                float peso = saturate((luz - _Limiar) / max(.4, luz + .0001));
                return float4(cor * peso, 1);
            }
            ENDCG
        }

        // Pass 1: borrão horizontal.
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float4 frag(v2f_img i) : SV_Target
            {
                float2 d = float2(_MainTex_TexelSize.x * 2.0, 0);
                float3 c = tex2D(_MainTex, i.uv).rgb * .227027;
                c += tex2D(_MainTex, i.uv + d).rgb * .1945946;
                c += tex2D(_MainTex, i.uv - d).rgb * .1945946;
                c += tex2D(_MainTex, i.uv + 2*d).rgb * .1216216;
                c += tex2D(_MainTex, i.uv - 2*d).rgb * .1216216;
                c += tex2D(_MainTex, i.uv + 3*d).rgb * .0702702;
                c += tex2D(_MainTex, i.uv - 3*d).rgb * .0702702;
                return float4(c, 1);
            }
            ENDCG
        }
        // Pass 2: borrão vertical.
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float4 frag(v2f_img i) : SV_Target
            {
                float2 d = float2(0, _MainTex_TexelSize.y * 2.0);
                float3 c = tex2D(_MainTex, i.uv).rgb * .227027;
                c += tex2D(_MainTex, i.uv + d).rgb * .1945946;
                c += tex2D(_MainTex, i.uv - d).rgb * .1945946;
                c += tex2D(_MainTex, i.uv + 2*d).rgb * .1216216;
                c += tex2D(_MainTex, i.uv - 2*d).rgb * .1216216;
                c += tex2D(_MainTex, i.uv + 3*d).rgb * .0702702;
                c += tex2D(_MainTex, i.uv - 3*d).rgb * .0702702;
                return float4(c, 1);
            }
            ENDCG
        }
        // Pass 3: combina cena original com os brilhos e aplica leve compressão HDR.
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            sampler2D _BloomTex;
            sampler2D _BloomTex2;
            float _Forca;
            float4 frag(v2f_img i) : SV_Target
            {
                float3 original = tex2D(_MainTex, i.uv).rgb;
                float3 aura = tex2D(_BloomTex, i.uv).rgb;
                float3 auraLarga = tex2D(_BloomTex2, i.uv).rgb;
                float3 cor = original + (aura * .67 + auraLarga * 1.05) * _Forca;
                cor = cor / (1.0 + cor * .22);
                return float4(cor, 1);
            }
            ENDCG
        }
    }
    FallBack Off
}
