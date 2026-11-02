using UnityEngine;

/// <summary>Traçados brancos finos e dinâmicos como no vídeo: alguns orbitam o núcleo,
/// outros se elevam acima do plano galáctico.</summary>
public class FiosLuminosos : MonoBehaviour
{
    [Range(0.5f, 2f)] public float intensidade = 1f;
    [Range(0.5f, 3f)] public float velocidade = 1f;

    const int Fios = 4;
    const int Segmentos = 130;
    private LineRenderer[] finos, halos;
    private Vector3[] pontos;
    private bool criado;

    public void Criar()
    {
        if (criado) return;
        criado = true;
        finos = new LineRenderer[Fios];
        halos = new LineRenderer[Fios];
        pontos = new Vector3[Segmentos];
        Shader shader = Shader.Find("GalaxiaReferencia/Fio Aditivo");
        if (shader == null) shader = Shader.Find("Sprites/Default");
        Material miolo = new Material(shader) { name = "Fio branco dourado" };
        Material brilho = new Material(shader) { name = "Aurora suave dos fios" };
        if (miolo.HasProperty("_Intensidade")) miolo.SetFloat("_Intensidade", 2.6f);
        if (brilho.HasProperty("_Intensidade")) brilho.SetFloat("_Intensidade", .8f);

        for (int i = 0; i < Fios; i++)
        {
            Transform par = new GameObject("Espiral de energia " + (i + 1)).transform;
            par.SetParent(transform, false);
            halos[i] = Linha("Halo suave", par, brilho, .33f, new Color(1f, .54f, .72f, .18f));
            finos[i] = Linha("Fio branco-dourado", par, miolo, .075f, new Color(1f, .92f, .79f, .70f));
        }
        Atualizar(0f);
    }

    static LineRenderer Linha(string nome, Transform pai, Material material, float largura, Color cor)
    {
        GameObject obj = new GameObject(nome);
        obj.transform.SetParent(pai, false);
        LineRenderer l = obj.AddComponent<LineRenderer>();
        l.sharedMaterial = material;
        l.useWorldSpace = false;
        l.loop = false;
        l.positionCount = Segmentos;
        l.widthMultiplier = largura;
        l.numCapVertices = 3;
        l.numCornerVertices = 2;
        l.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        l.receiveShadows = false;
        l.alignment = LineAlignment.View;
        var grad = new Gradient();
        grad.SetKeys(new GradientColorKey[] {
            new GradientColorKey(new Color(cor.r, cor.g, cor.b, 1f), 0f),
            new GradientColorKey(new Color(1f, .97f, .91f, 1f), .4f),
            new GradientColorKey(new Color(cor.r, cor.g, cor.b, 1f), 1f)
        }, new GradientAlphaKey[] {
            new GradientAlphaKey(.0f, 0f),
            new GradientAlphaKey(cor.a * .78f, .12f),
            new GradientAlphaKey(cor.a, .48f),
            new GradientAlphaKey(cor.a * .80f, .8f),
            new GradientAlphaKey(.0f, 1f)
        });
        l.colorGradient = grad;
        return l;
    }

    void Update()
    {
        if (!criado) return;
        Atualizar(Time.time * velocidade);
    }

    void Atualizar(float tempo)
    {
        for (int f = 0; f < Fios; f++)
        {
            for (int i = 0; i < Segmentos; i++)
            {
                float t = i / (float)(Segmentos - 1);
                float a, r, h;
                switch (f)
                {
                    case 0: // Fio central circular que se fecha formando laços
                        a = t * Mathf.PI * 5.35f + tempo * .15f;
                        r = Mathf.Lerp(.6f, 7.6f, t) + Mathf.Sin(13f * t + tempo * .48f) * .40f;
                        h = Mathf.Sin(11f * t + tempo * .65f) * .62f + t * .15f;
                        break;
                    case 1: // Fio saindo verticalmente do centro (muito característico do vídeo)
                        a = t * Mathf.PI * 6.0f + 2f + tempo * .11f;
                        r = Mathf.Lerp(.3f, 3.8f, t) + Mathf.Sin(t * 18f + tempo * .35f) * .38f;
                        h = Mathf.Lerp(-1.0f, 15.0f, t);
                        break;
                    case 2: // Arco inclinando para fora, ao redor do halo
                        a = t * Mathf.PI * 4.4f + 3f - tempo * .13f;
                        r = Mathf.Lerp(.8f, 8.5f, t) + Mathf.Sin(t * 7f) * .7f;
                        h = Mathf.Sin(t * Mathf.PI * 2.6f + tempo * .3f) * 1.2f + 1.1f;
                        break;
                    default: // Outro fio curvo vertical, delicado
                        a = t * Mathf.PI * 4.2f + 1f + tempo * .13f;
                        r = Mathf.Lerp(1.0f, 2.6f, t) + Mathf.Sin(t * Mathf.PI * 3f) * .7f;
                        h = Mathf.Lerp(-6.0f, 10.0f, t);
                        break;
                }
                pontos[i] = new Vector3(Mathf.Cos(a) * r, h, Mathf.Sin(a) * r);
            }
            halos[f].SetPositions(pontos);
            finos[f].SetPositions(pontos);
        }
    }
}
