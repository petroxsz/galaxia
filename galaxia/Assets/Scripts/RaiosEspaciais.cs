using UnityEngine;

/// <summary>Dois pequenos rastros azulados e rosados atravessam ocasionalmente as bordas.</summary>
public class RaiosEspaciais : MonoBehaviour
{
    private LineRenderer[] rastros;
    private const int Segmentos = 48;
    private Vector3[] pontos = new Vector3[Segmentos];
    private bool criado;

    public void Criar()
    {
        if (criado) return;
        criado = true;
        Shader shader = Shader.Find("GalaxiaReferencia/Fio Aditivo");
        if (shader == null) shader = Shader.Find("Sprites/Default");
        rastros = new LineRenderer[2];
        for (int i = 0; i < rastros.Length; i++)
        {
            GameObject obj = new GameObject(i == 0 ? "Raio eletrico azul" : "Rastro coral de luz");
            obj.transform.SetParent(transform, false);
            LineRenderer linha = obj.AddComponent<LineRenderer>();
            Material mat = new Material(shader) { name = "Rastro de cometa " + i };
            if (mat.HasProperty("_Intensidade")) mat.SetFloat("_Intensidade", 3.1f);
            linha.sharedMaterial = mat;
            linha.positionCount = Segmentos;
            linha.useWorldSpace = false;
            linha.widthMultiplier = i == 0 ? .19f : .12f;
            linha.numCapVertices = 3;
            linha.alignment = LineAlignment.View;
            linha.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            linha.receiveShadows = false;
            rastros[i] = linha;
        }
        Atualizar(0f);
    }

    void Update()
    {
        if (criado) Atualizar(Time.time);
    }

    void Atualizar(float tempo)
    {
        for (int r = 0; r < rastros.Length; r++)
        {
            // 7 segundos visíveis a cada 18 segundos (não ficam obstruindo a galáxia).
            float fase = Mathf.Repeat(tempo + (r == 0 ? 0f : 8f), 18f);
            float janela = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(fase / 1.1f)) *
                           (1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((fase - 5.4f) / 1.1f)));
            Color baseCor = r == 0 ? new Color(.49f, .73f, 1f, 1f) : new Color(1f, .41f, .67f, 1f);
            var g = new Gradient();
            g.SetKeys(new GradientColorKey[] {
                new GradientColorKey(baseCor, 0f),
                new GradientColorKey(Color.white, .90f),
                new GradientColorKey(baseCor, 1f)
            }, new GradientAlphaKey[] {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(janela * .43f, .35f),
                new GradientAlphaKey(janela * .95f, .9f),
                new GradientAlphaKey(0f, 1f)
            });
            rastros[r].colorGradient = g;

            float a = r == 0 ? -.45f : 2.85f;
            a += Mathf.Sin(tempo * .19f + r) * .12f;
            for (int i = 0; i < Segmentos; i++)
            {
                float u = i / (float)(Segmentos - 1);
                float distancia = Mathf.Lerp(21f, 56f, u);
                float lateral = Mathf.Sin(u * 15f - tempo * 5.5f + r) * .65f * (1f - u * .50f);
                float x = Mathf.Cos(a) * distancia + Mathf.Sin(a) * lateral;
                float z = Mathf.Sin(a) * distancia - Mathf.Cos(a) * lateral;
                float y = 1.9f + Mathf.Sin(u * 9f - tempo * 3f) * 1.3f;
                pontos[i] = new Vector3(x, y, z);
            }
            rastros[r].SetPositions(pontos);
        }
    }
}
