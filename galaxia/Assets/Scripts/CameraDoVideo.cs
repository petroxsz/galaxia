using UnityEngine;

/// <summary>Reproduz o movimento do vídeo: aproxima, afasta e orbita lentamente.</summary>
public class CameraDoVideo : MonoBehaviour
{
    public Transform alvo;
    public bool cinematica = true;
    [Range(0f, 20f)] public float velocidadeOrbita = 2.7f;
    [Range(1f, 14f)] public float sensibilidade = 3.6f;
    [Range(2f, 20f)] public float velocidadeZoom = 8f;

    private float tempo;
    private float distancia = 65f;
    private float horizontal = 30f;
    private float vertical = 58f;
    private bool pausa;

    // Ciclo de 32 segundos, modelado conforme os enquadramentos observados no vídeo.
    static readonly float[] tempos = { 0f, 2f, 4f, 6f, 8f, 11f, 14f, 17f, 20f, 23f, 27f, 32f };
    static readonly float[] distancias = { 66f, 80f, 112f, 110f, 91f, 72f, 62f, 62f, 43f, 27f, 24f, 66f };
    static readonly float[] inclinacoes = { 61f, 60f, 56f, 55f, 58f, 63f, 61f, 62f, 67f, 71f, 66f, 61f };

    private void LateUpdate()
    {
        if (alvo == null) return;
        if (Input.GetKeyDown(KeyCode.C)) { cinematica = !cinematica; tempo = 0; }
        if (Input.GetKeyDown(KeyCode.Space)) pausa = !pausa;
        if (Input.GetKeyDown(KeyCode.R)) { cinematica = true; pausa = false; tempo = 0; horizontal = 30f; vertical = 58f; distancia = 66f; }

        bool controle = Input.GetMouseButton(0);
        float scroll = Input.mouseScrollDelta.y;
        if (controle || Mathf.Abs(scroll) > .001f || Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.E))
            cinematica = false;

        if (cinematica)
        {
            if (!pausa) tempo += Time.deltaTime;
            float t = Mathf.Repeat(tempo, 32f);
            distancia = Interpolar(t, tempos, distancias);
            vertical = Interpolar(t, tempos, inclinacoes);
            horizontal += (pausa ? 0f : velocidadeOrbita * Time.deltaTime);
        }
        else
        {
            if (controle)
            {
                horizontal += Input.GetAxis("Mouse X") * sensibilidade;
                vertical -= Input.GetAxis("Mouse Y") * sensibilidade;
            }
            if (Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.A)) horizontal -= 35f * Time.deltaTime;
            if (Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.D)) horizontal += 35f * Time.deltaTime;
            if (Input.GetKey(KeyCode.W)) vertical -= 25f * Time.deltaTime;
            if (Input.GetKey(KeyCode.S)) vertical += 25f * Time.deltaTime;
            distancia = Mathf.Clamp(distancia - scroll * velocidadeZoom, 8f, 180f);
        }

        vertical = Mathf.Clamp(vertical, 8f, 172f);
        Quaternion rot = Quaternion.Euler(vertical, horizontal, 0f);
        transform.position = alvo.position + rot * new Vector3(0f, 0f, -distancia);
        transform.LookAt(alvo.position);
    }

    static float Interpolar(float t, float[] ts, float[] vs)
    {
        for (int i = 0; i < ts.Length - 1; i++)
        {
            if (t > ts[i + 1]) continue;
            float u = Mathf.Clamp01((t - ts[i]) / (ts[i + 1] - ts[i]));
            u = u * u * (3f - 2f * u);
            return Mathf.Lerp(vs[i], vs[i + 1], u);
        }
        return vs[vs.Length - 1];
    }
}
