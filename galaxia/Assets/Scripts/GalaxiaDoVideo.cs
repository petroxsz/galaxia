using System;
using UnityEngine;

/// <summary>
/// Galáxia 3D criada quase inteiramente com Particle Systems (sistema de partículas).
/// Visual de referência: espirais rosadas/alaranjadas, centro luminoso, pontos brilhantes.
/// Os pontos são reconstruídos em Awake porque SetParticles não persiste no arquivo .unity.
/// </summary>
[DisallowMultipleComponent]
public class GalaxiaDoVideo : MonoBehaviour
{
    [Header("Formato da galáxia")]
    [Range(2, 5)] public int bracosEspirais = 3;
    [Range(800, 4500)] public int estrelasPorBraco = 2150;
    [Range(15f, 50f)] public float raio = 28f;
    [Range(0.7f, 2.7f)] public float voltas = 1.68f;
    [Range(0.3f, 2.2f)] public float aberturaBracos = 0.84f;
    [Range(0.1f, 3f)] public float espessura = 0.70f;

    [Header("Brilho e cores da referência")]
    [Range(0.3f, 3f)] public float brilhoEstrelas = 1.30f;
    [Range(0.3f, 2.5f)] public float brilhoNucleo = 0.94f;
    [Range(0.2f, 2.2f)] public float brilhoHalo = 0.84f;
    [Range(0.2f, 3f)] public float brilhoAneis = 1.48f;

    [Header("Movimento orbital")]
    public bool girar = true;
    [Range(0f, 30f)] public float velocidadeInterior = 11.0f;
    [Range(0f, 20f)] public float velocidadeDisco = 7.3f;
    [Range(0f, 15f)] public float velocidadeExterior = 4.6f;
    [Range(0f, 30f)] public float velocidadeFios = 13.0f;

    [Header("Detalhes da referência")]
    public bool fiosLuminosos = true;
    public bool raiosAzuis = true;
    public int semente = 30112026;

    private Transform interno, disco, externo, halo, eixoFios, fundo;
    private Material materialEstrelas, materialNucleo, materialHalo, materialAneis, materialFundo;

    private void Awake() { Recriar(); }

    private void Update()
    {
        if (!girar) return;
        float dt = Time.deltaTime;
        if (interno != null) interno.Rotate(Vector3.up, velocidadeInterior * dt, Space.Self);
        if (disco != null) disco.Rotate(Vector3.up, velocidadeDisco * dt, Space.Self);
        if (externo != null) externo.Rotate(Vector3.up, velocidadeExterior * dt, Space.Self);
        if (halo != null) halo.Rotate(Vector3.up, velocidadeInterior * 0.4f * dt, Space.Self);
        if (eixoFios != null) eixoFios.Rotate(Vector3.up, velocidadeFios * dt, Space.Self);
    }

    [ContextMenu("Recriar galaxia com a referencia")]
    public void Recriar()
    {
        Transform anterior = transform.Find("GALAXIA GERADA (Particle Systems)");
        if (anterior != null)
        {
            anterior.gameObject.SetActive(false);
            if (Application.isPlaying) Destroy(anterior.gameObject);
            else DestroyImmediate(anterior.gameObject);
        }

        Transform raiz = Novo("GALAXIA GERADA (Particle Systems)", transform);
        fundo = Novo("01 - Fundo de estrelas distantes", raiz);
        halo = Novo("02 - Halo rosa do centro", raiz);
        externo = Novo("03 - Estrelas e anéis externos", raiz);
        disco = Novo("04 - Braços espirais", raiz);
        interno = Novo("05 - Núcleo e órbitas internas", raiz);
        eixoFios = Novo("06 - Fios de luz serpenteantes", raiz);

        Shader shader = Shader.Find("GalaxiaReferencia/Estrela Suave");
        if (shader == null) shader = Shader.Find("Particles/Standard Unlit");
        if (shader == null) shader = Shader.Find("Sprites/Default");
        materialEstrelas = MaterialEstelar(shader, "Estrelas de cor salmao", 2.9f * brilhoEstrelas, 0.78f);
        materialNucleo = MaterialEstelar(shader, "Estrelas do nucleo", 1.5f * brilhoNucleo, 0.76f);
        materialHalo = MaterialEstelar(shader, "Halo rosado difuso", 0.95f * brilhoHalo, 1.38f);
        materialAneis = MaterialEstelar(shader, "Aneis de pontos brilhantes", 3.0f * brilhoAneis, 0.65f);
        materialFundo = MaterialEstelar(shader, "Estrelas azuis ao fundo", 1.35f, 0.75f);

        System.Random rng = new System.Random(semente);
        CriarFundo(rng);
        CriarHalo(rng);
        CriarEspirais(rng);
        CriarAneis(rng);
        CriarPoeira(rng);
        CriarNucleo(rng);
        if (fiosLuminosos) CriarFios();
        if (raiosAzuis) CriarRaios();
    }

    static Transform Novo(string nome, Transform pai)
    {
        GameObject obj = new GameObject(nome);
        obj.transform.SetParent(pai, false);
        return obj.transform;
    }

    static Material MaterialEstelar(Shader shader, string nome, float intensidade, float maciez)
    {
        Material mat = new Material(shader) { name = nome };
        if (mat.HasProperty("_Intensidade")) mat.SetFloat("_Intensidade", intensidade);
        if (mat.HasProperty("_Maciez")) mat.SetFloat("_Maciez", maciez);
        return mat;
    }

    void CriarFundo(System.Random rng)
    {
        ParticleSystem.Particle[] pontos = new ParticleSystem.Particle[1900];
        for (int i = 0; i < pontos.Length; i++)
        {
            Vector3 p = new Vector3(Aleatorio(rng, -180, 180), Aleatorio(rng, -96, 96), Aleatorio(rng, -180, 180));
            if (p.magnitude < 65f) p = p.normalized * Aleatorio(rng, 78f, 150f);
            Color cor = rng.NextDouble() < .28
                ? new Color(.78f, .88f, 1f, Aleatorio(rng, .30f, .8f))
                : new Color(.40f, .62f, 1f, Aleatorio(rng, .15f, .60f));
            pontos[i] = Ponto(p, cor, Aleatorio(rng, .06f, .30f), rng);
        }
        Sistema("Estrelas frias distantes", pontos, fundo, materialFundo);
    }

    void CriarHalo(System.Random rng)
    {
        // Bolhas rosadas grandes e translucidas: brilho difuso, sem disco branco chapado.
        ParticleSystem.Particle[] nuvem = new ParticleSystem.Particle[400];
        for (int i = 0; i < nuvem.Length; i++)
        {
            float r = Mathf.Abs(Gauss(rng)) * 2.20f;
            float theta = Aleatorio(rng, 0f, Mathf.PI * 2f);
            Vector3 p = new Vector3(Mathf.Cos(theta) * r, Gauss(rng) * 0.40f, Mathf.Sin(theta) * r);
            Color c = Color.Lerp(new Color(1f, .22f, .58f, .07f), new Color(1f, .64f, .73f, .11f), (float)rng.NextDouble());
            nuvem[i] = Ponto(p, c, Aleatorio(rng, 3.5f, 8.7f), rng);
        }
        Sistema("Halo nebuloso rosa", nuvem, halo, materialHalo);

        ParticleSystem.Particle[] entorno = new ParticleSystem.Particle[1300];
        for (int i = 0; i < entorno.Length; i++)
        {
            float r = Mathf.Pow((float)rng.NextDouble(), .55f) * 11.5f;
            float a = Aleatorio(rng, 0, 2 * Mathf.PI);
            Vector3 p = new Vector3(Mathf.Cos(a) * r, Gauss(rng) * .28f, Mathf.Sin(a) * r);
            Color c = Color.Lerp(new Color(.85f, .32f, .90f, .20f), new Color(1f, .62f, .80f, .30f), (float)rng.NextDouble());
            entorno[i] = Ponto(p, c, Aleatorio(rng, .16f, .66f), rng);
        }
        Sistema("Nuvem rosada de estrelas", entorno, halo, materialEstrelas);
    }

    void CriarEspirais(System.Random rng)
    {
        // Espirais bem separadas, em tons de coral/rosa, como na referência.
        Color[] cores = {
            new Color(1.0f, .49f, .40f, .86f),
            new Color(1.0f, .57f, .65f, .85f),
            new Color(1.0f, .69f, .42f, .90f),
            new Color(.78f, .53f, 1.0f, .72f),
            new Color(1.0f, .45f, .72f, .86f)
        };

        for (int b = 0; b < bracosEspirais; b++)
        {
            ParticleSystem.Particle[] pontos = new ParticleSystem.Particle[estrelasPorBraco];
            for (int i = 0; i < pontos.Length; i++)
            {
                float t = Mathf.Pow((float)rng.NextDouble(), .68f);
                float r = Mathf.Lerp(2.4f, raio, t);
                float theta = (2f * Mathf.PI * b / bracosEspirais) + (t * voltas * 2f * Mathf.PI);
                theta += Gauss(rng) * aberturaBracos / Mathf.Max(2.5f, .47f * r);
                r += Gauss(rng) * (.15f + .32f * t);
                float y = Gauss(rng) * espessura * Mathf.Lerp(.86f, .22f, t);
                Color cor = Color.Lerp(cores[b % cores.Length], Color.white, Aleatorio(rng, 0, .38f));
                cor.a = Aleatorio(rng, .32f, .95f);
                float tamanho = rng.NextDouble() < .17
                    ? Aleatorio(rng, .35f, .85f)
                    : Aleatorio(rng, .12f, .37f);
                pontos[i] = Ponto(new Vector3(Mathf.Cos(theta) * r, y, Mathf.Sin(theta) * r), cor, tamanho, rng);
            }
            Sistema("Braco luminoso " + (b + 1), pontos, disco, materialEstrelas);
        }
    }

    void CriarAneis(System.Random rng)
    {
        // Trilhas concêntricas de estrelas claras e pontuais, não círculos contínuos.
        float[] raios = { 5.8f, 9.8f, 14.3f, 19.2f, 24.3f };
        for (int anel = 0; anel < raios.Length; anel++)
        {
            int contagem = anel < 2 ? 920 : 1150;
            ParticleSystem.Particle[] pontos = new ParticleSystem.Particle[contagem];
            for (int i = 0; i < pontos.Length; i++)
            {
                float theta = Aleatorio(rng, 0, 2f * Mathf.PI);
                float r = raios[anel] + Mathf.Sin(theta * (bracosEspirais + 1) + anel) * .40f + Gauss(rng) * .34f;
                float y = Gauss(rng) * espessura * .25f;
                Color c;
                float chance = (float)rng.NextDouble();
                if (chance < .64f) c = new Color(1f, Aleatorio(rng, .45f, .80f), Aleatorio(rng, .33f, .59f), Aleatorio(rng, .50f, 1f));
                else if (chance < .92f) c = new Color(1f, Aleatorio(rng, .55f, .80f), Aleatorio(rng, .75f, .95f), Aleatorio(rng, .45f, 1f));
                else c = new Color(.66f, .78f, 1f, Aleatorio(rng, .55f, .92f));
                float tam = rng.NextDouble() < .21 ? Aleatorio(rng, .36f, .78f) : Aleatorio(rng, .14f, .35f);
                pontos[i] = Ponto(new Vector3(r * Mathf.Cos(theta), y, r * Mathf.Sin(theta)), c, tam, rng);
            }
            Sistema("Anel de pontos " + (anel + 1), pontos, anel < 2 ? interno : externo, materialAneis);
        }

        ParticleSystem.Particle[] destaques = new ParticleSystem.Particle[620];
        for (int i = 0; i < destaques.Length; i++)
        {
            float r = Aleatorio(rng, 6f, raio * 1.1f);
            float a = Aleatorio(rng, 0f, 2f * Mathf.PI);
            Color c = rng.NextDouble() < .10f
                ? new Color(.60f, .73f, 1.00f, .88f)
                : new Color(1f, Aleatorio(rng, .56f, 1f), Aleatorio(rng, .48f, .86f), Aleatorio(rng, .65f, 1f));
            destaques[i] = Ponto(new Vector3(Mathf.Cos(a) * r, Gauss(rng) * .34f, Mathf.Sin(a) * r), c,
                Aleatorio(rng, .35f, 1.20f), rng);
        }
        Sistema("Estrelas grandes e cintilantes", destaques, externo, materialAneis);
    }

    void CriarPoeira(System.Random rng)
    {
        ParticleSystem.Particle[] poeira = new ParticleSystem.Particle[5600];
        for (int i = 0; i < poeira.Length; i++)
        {
            float r = Mathf.Sqrt((float)rng.NextDouble()) * (raio + 2f);
            float a = Aleatorio(rng, 0f, 2f * Mathf.PI);
            float y = Gauss(rng) * espessura * .32f;
            Color c = rng.NextDouble() < .65f
                ? new Color(1f, .27f, .44f, Aleatorio(rng, .07f, .20f))
                : new Color(.58f, .37f, 1f, Aleatorio(rng, .07f, .20f));
            poeira[i] = Ponto(new Vector3(Mathf.Cos(a) * r, y, Mathf.Sin(a) * r), c,
                Aleatorio(rng, .09f, .25f), rng);
        }
        Sistema("Poeira discreta entre os aneis", poeira, externo, materialHalo);
    }

    void CriarNucleo(System.Random rng)
    {
        ParticleSystem.Particle[] pontos = new ParticleSystem.Particle[1750];
        for (int i = 0; i < pontos.Length; i++)
        {
            Vector3 p = new Vector3(Gauss(rng) * 1.85f, Gauss(rng) * .63f, Gauss(rng) * 1.85f);
            Color c = Color.Lerp(new Color(1f, .48f, .75f, .62f), new Color(1f, .96f, .89f, .92f), (float)rng.NextDouble());
            pontos[i] = Ponto(p, c, Aleatorio(rng, .15f, .50f), rng);
        }
        Sistema("Pontos do nucleo rosa-claro", pontos, interno, materialNucleo);

        // Núcleo esbranquiçado mínimo, mas bordas rosadas intensas.
        ParticleSystem.Particle[] miolo = new ParticleSystem.Particle[120];
        for (int i = 0; i < miolo.Length; i++)
        {
            Vector3 p = new Vector3(Gauss(rng) * .35f, Gauss(rng) * .25f, Gauss(rng) * .35f);
            Color c = new Color(1f, Aleatorio(rng, .80f, 1f), Aleatorio(rng, .78f, .94f), Aleatorio(rng, .24f, .57f));
            miolo[i] = Ponto(p, c, Aleatorio(rng, .37f, 1.25f), rng);
        }
        Sistema("Miolo branco-rosa", miolo, interno, materialNucleo);
    }

    void CriarFios()
    {
        GameObject raiz = new GameObject("Fios orbitais finos");
        raiz.transform.SetParent(eixoFios, false);
        FiosLuminosos fios = raiz.AddComponent<FiosLuminosos>();
        fios.Criar();
    }

    void CriarRaios()
    {
        GameObject raiz = new GameObject("Raios de energia distantes");
        raiz.transform.SetParent(transform, false);
        RaiosEspaciais raios = raiz.AddComponent<RaiosEspaciais>();
        raios.Criar();
    }

    static float Aleatorio(System.Random rng, float min, float max) { return Mathf.Lerp(min, max, (float)rng.NextDouble()); }
    static float Gauss(System.Random rng)
    {
        float u = Mathf.Max(.00001f, (float)rng.NextDouble());
        float v = (float)rng.NextDouble();
        return Mathf.Sqrt(-2f * Mathf.Log(u)) * Mathf.Cos(2f * Mathf.PI * v);
    }

    static ParticleSystem.Particle Ponto(Vector3 p, Color cor, float tamanho, System.Random rng)
    {
        return new ParticleSystem.Particle
        {
            position = p, startColor = cor, startSize = tamanho,
            startLifetime = 100000f, remainingLifetime = 100000f,
            randomSeed = (uint)rng.Next(1, Int32.MaxValue)
        };
    }

    static ParticleSystem Sistema(string nome, ParticleSystem.Particle[] pontos, Transform pai, Material material)
    {
        GameObject obj = new GameObject(nome);
        obj.transform.SetParent(pai, false);
        ParticleSystem sistema = obj.AddComponent<ParticleSystem>();
        sistema.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var main = sistema.main;
        main.loop = false;
        main.playOnAwake = false;
        main.startLifetime = 100000f;
        main.startSpeed = 0f;
        main.maxParticles = pontos.Length + 1;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.cullingMode = ParticleSystemCullingMode.AlwaysSimulate;
        var emission = sistema.emission; emission.enabled = false;
        var shape = sistema.shape; shape.enabled = false;
        var renderer = sistema.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.alignment = ParticleSystemRenderSpace.View;
        renderer.sortMode = ParticleSystemSortMode.Distance;
        renderer.minParticleSize = 0f;
        renderer.maxParticleSize = .25f;
        renderer.sharedMaterial = material;
        // Ordem correta: Play antes de SetParticles.
        sistema.Play(true);
        sistema.SetParticles(pontos, pontos.Length);
        return sistema;
    }
}
