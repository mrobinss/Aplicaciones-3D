using UnityEngine;

// Orden de los niveles del mapa y nivel mas alto desbloqueado (se guarda en PlayerPrefs).
// Ganar un nivel desbloquea el siguiente.
public static class LevelProgress
{
    private const string UnlockedKey = "NivelDesbloqueado";

    // Escena de cada nivel y escena con la que arranca (instrucciones), en el orden del mapa
    private static readonly string[] LevelScenes = { "SampleScene", "Nivel2" };
    // Nivel 2: primero la cuajada (Lvl2_02), luego el melao (Lvl2_01) y al final la combinacion (Lvl2_03)
    private static readonly string[] IntroScenes = { "InstructionsScene", "InstructionsLvlTwo02" };

    // Ultimo nivel jugado (lo fija el GameManager del nivel)
    public static int CurrentLevel = 1;

    public static int LevelCount => LevelScenes.Length;
    public static int Unlocked => Mathf.Clamp(PlayerPrefs.GetInt(UnlockedKey, 1), 1, LevelCount);

    public static bool HasLevel(int level) => level >= 1 && level <= LevelCount;
    public static bool IsUnlocked(int level) => HasLevel(level) && level <= Unlocked;
    public static string LevelScene(int level) => LevelScenes[level - 1];
    public static string IntroScene(int level) => IntroScenes[level - 1];

    public static void Unlock(int level)
    {
        if (level <= PlayerPrefs.GetInt(UnlockedKey, 1)) return;

        PlayerPrefs.SetInt(UnlockedKey, level);
        PlayerPrefs.Save();
    }
}
