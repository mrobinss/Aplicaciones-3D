using UnityEngine.SceneManagement;

public static class SceneHistory
{
    public static string LastScene = "MainMenu";

    // Carga una escena recordando desde cual se llego (para botones de "Salir" / "Volver")
    public static void Load(string sceneName)
    {
        LastScene = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(sceneName);
    }
}
