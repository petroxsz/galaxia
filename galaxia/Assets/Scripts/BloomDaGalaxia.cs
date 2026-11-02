using UnityEngine;

/// <summary>
/// Bloom local para a Built-in Render Pipeline, sem baixar pacotes externos.
/// OnRenderImage não funciona com URP/HDRP. Este projeto já vem configurado com Built-in.
/// </summary>
[RequireComponent(typeof(Camera))]
public class BloomDaGalaxia : MonoBehaviour
{
    [Range(0.1f, 5f)] public float intensidade = 1.25f;
    [Range(0.2f, 3f)] public float limiar = 0.85f;
    public bool ativado = true;
    private Material material;

    private void OnRenderImage(RenderTexture origem, RenderTexture destino)
    {
        if (!ativado || intensidade <= 0.01f)
        {
            Graphics.Blit(origem, destino);
            return;
        }

        if (material == null)
        {
            Shader shader = Shader.Find("Hidden/GalaxiaReferencia/Bloom");
            if (shader == null || !shader.isSupported)
            {
                Graphics.Blit(origem, destino);
                return;
            }
            material = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
        }

        int largura = Mathf.Max(4, origem.width / 3);
        int altura = Mathf.Max(4, origem.height / 3);
        int largura2 = Mathf.Max(4, origem.width / 8);
        int altura2 = Mathf.Max(4, origem.height / 8);
        RenderTextureFormat formato = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.DefaultHDR)
            ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default;
        RenderTexture a = null, b = null, c = null, d = null;
        try
        {
            a = RenderTexture.GetTemporary(largura, altura, 0, formato);
            b = RenderTexture.GetTemporary(largura, altura, 0, formato);
            c = RenderTexture.GetTemporary(largura2, altura2, 0, formato);
            d = RenderTexture.GetTemporary(largura2, altura2, 0, formato);
            a.filterMode = b.filterMode = c.filterMode = d.filterMode = FilterMode.Bilinear;
            material.SetFloat("_Limiar", limiar);
            material.SetFloat("_Forca", intensidade);

            Graphics.Blit(origem, a, material, 0); // highlights
            Graphics.Blit(a, b, material, 1);      // horizontal
            Graphics.Blit(b, a, material, 2);      // vertical
            Graphics.Blit(a, c);                   // espalhar brilho
            Graphics.Blit(c, d, material, 1);
            Graphics.Blit(d, c, material, 2);
            material.SetTexture("_BloomTex", a);
            material.SetTexture("_BloomTex2", c);
            Graphics.Blit(origem, destino, material, 3);
        }
        finally
        {
            if (a != null) RenderTexture.ReleaseTemporary(a);
            if (b != null) RenderTexture.ReleaseTemporary(b);
            if (c != null) RenderTexture.ReleaseTemporary(c);
            if (d != null) RenderTexture.ReleaseTemporary(d);
        }
    }

    private void OnDisable()
    {
        if (material != null)
        {
            if (Application.isPlaying) Destroy(material);
            else DestroyImmediate(material);
            material = null;
        }
    }
}
