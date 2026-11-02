using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class CriarCenaDoVideo
{
    private const string Cena = "Assets/Scenes/GalaxiaReferencia.unity";

    static CriarCenaDoVideo()
    {
        EditorApplication.delayCall += CriarCenaSeFaltar;
    }

    static void CriarCenaSeFaltar()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling) return;
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(Cena) != null) return;
        Criar();
    }

    [MenuItem("Galaxia do Video/Criar ou recriar cena fiel ao video")]
    public static void Criar()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        Scene cena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        GameObject raiz = new GameObject("GALAXIA - REFERENCIA DO VIDEO");
        GalaxiaDoVideo galaxia = raiz.AddComponent<GalaxiaDoVideo>();
        galaxia.Recriar();

        GameObject camObj = new GameObject("Camera cinematografica");
        camObj.tag = "MainCamera";
        Camera cam = camObj.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(.002f, .001f, .014f, 1f);
        cam.allowHDR = true;
        cam.fieldOfView = 56f;
        cam.nearClipPlane = .1f;
        cam.farClipPlane = 600f;
        camObj.AddComponent<AudioListener>();

        CameraDoVideo cameraControle = camObj.AddComponent<CameraDoVideo>();
        cameraControle.alvo = raiz.transform;
        cameraControle.cinematica = true;
        cameraControle.velocidadeOrbita = 2.7f;
        BloomDaGalaxia bloom = camObj.AddComponent<BloomDaGalaxia>();
        bloom.ativado = true;
        bloom.intensidade = 1.25f;
        bloom.limiar = .85f;

        camObj.transform.position = new Vector3(23f, 52f, -34f);
        camObj.transform.LookAt(raiz.transform.position);

        RenderSettings.ambientLight = Color.black;
        RenderSettings.fog = false;

        EditorSceneManager.SaveScene(cena, Cena);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(Cena, true) };
        Selection.activeGameObject = raiz;
        Debug.Log("[GALAXIA DO VIDEO] Cena criada! Abra Assets/Scenes/GalaxiaReferencia e aperte Play.");
    }
}
