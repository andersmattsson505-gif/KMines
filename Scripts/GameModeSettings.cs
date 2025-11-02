using UnityEngine;
using UnityEngine.SceneManagement;

namespace KMines
{
    public enum GameModeType
    {
        Campaign,
        Arcade,
        Boss
    }

    /// <summary>
    /// Beskriver hur en spel-session ska konfigureras.
    /// Boot.cs läser detta och bygger brädet efter reglerna.
    /// </summary>
    [System.Serializable]
    public struct GameSessionConfig
    {
        public GameModeType mode;
        public int levelIndex;

        // Missiler tillåtna eller inte
        public bool allowMissiles;

        // Timer
        public bool timed;
        public float timeLimitSeconds;

        // Arcade panic: +X sek varje gång vi öppnar en säker ruta
        public float timeBonusPerSafeReveal;

        // Anpassad brädstorlek (Arcade Custom Run)
        public bool useCustomBoardSize;
        public int customWidth;
        public int customHeight;
        public float customMineDensity;

        // Vilket tileset / tema Board ska använda.
        // "grass", "desert", "ice", "toxic", "boss_scorpion", eller "random" (Arcade Quick Play)
        public string tilesetTheme;
    }

    /// <summary>
    /// Global state "hur nästa runda ska startas".
    /// Menyknapparna skriver hit, Boot.cs läser och bygger scene Gameplay.
    /// </summary>
    public static class GameModeSettings
    {
        const int MAX_WIDTH = 9;
        const int MAX_HEIGHT = 15;

        public static bool hasConfig;
        public static GameSessionConfig current;

        // -------------------------------------------------
        // Kampanj / Story-bana: spelaren väljer en specifik bana
        // -------------------------------------------------
        public static void LaunchCampaignLevel(int levelIndex)
        {
            current = new GameSessionConfig
            {
                mode = GameModeType.Campaign,
                levelIndex = levelIndex,

                allowMissiles = true,

                timed = false,
                timeLimitSeconds = 0f,

                timeBonusPerSafeReveal = 0f,

                // ingen custom storlek här, LevelLoader bestämmer
                useCustomBoardSize = false,
                customWidth = 0,
                customHeight = 0,
                customMineDensity = 0f,

                // standardtema
                tilesetTheme = "grass"
            };

            hasConfig = true;
            SceneManager.LoadScene("Gameplay");
        }

        // -------------------------------------------------
        // Arcade Quick Play
        // -------------------------------------------------
        public static void LaunchArcadeQuick()
        {
            current = new GameSessionConfig
            {
                mode = GameModeType.Arcade,
                levelIndex = 0,

                allowMissiles = true,

                timed = false,
                timeLimitSeconds = 0f,

                timeBonusPerSafeReveal = 0f,

                useCustomBoardSize = false,
                customWidth = 0,
                customHeight = 0,
                customMineDensity = 0f,

                tilesetTheme = "random"
            };

            hasConfig = true;
            SceneManager.LoadScene("Gameplay");
        }

        public static void BackToArcadeMenu()
        {
            // rensa aktiv konfiguration så nästa runda inte startar automatiskt
            hasConfig = false;

            // hoppa till Arcade-menyn
            SceneManager.LoadScene("ArcadeMenu");
        }

        // Behåll gamla LaunchArcade() för bakåtkompabilitet
        public static void LaunchArcade()
        {
            LaunchArcadeQuick();
        }

        // -------------------------------------------------
        // Arcade Custom Run
        // -------------------------------------------------
        public static void LaunchArcadeCustom(
            int width,
            int height,
            float mineDensity,
            bool allowMissiles,
            bool timed,
            float startTimeSeconds,
            float timeBonusPerSafeRevealSeconds,
            string themeName
        )
        {
            // sanity clamp
            if (width < 1) width = 1;
            if (height < 1) height = 1;
            if (mineDensity < 0f) mineDensity = 0f;

            if (startTimeSeconds < 0f) startTimeSeconds = 0f;
            if (timeBonusPerSafeRevealSeconds < 0f) timeBonusPerSafeRevealSeconds = 0f;

            // mobil-klamp
            if (width > MAX_WIDTH) width = MAX_WIDTH;
            if (height > MAX_HEIGHT) height = MAX_HEIGHT;

            current = new GameSessionConfig
            {
                mode = GameModeType.Arcade,
                levelIndex = 0,

                allowMissiles = allowMissiles,

                timed = timed,
                timeLimitSeconds = startTimeSeconds,

                timeBonusPerSafeReveal = timeBonusPerSafeRevealSeconds,

                useCustomBoardSize = true,
                customWidth = width,
                customHeight = height,
                customMineDensity = mineDensity,

                // kan vara "random" eller "grass"/"desert"/osv
                tilesetTheme = string.IsNullOrEmpty(themeName) ? "random" : themeName
            };

            hasConfig = true;
            SceneManager.LoadScene("Gameplay");
        }

        // -------------------------------------------------
        // Boss
        // -------------------------------------------------
        public static void LaunchBoss()
        {
            current = new GameSessionConfig
            {
                mode = GameModeType.Boss,
                levelIndex = 0,

                allowMissiles = false,

                timed = true,
                timeLimitSeconds = 30f,

                // boss = stress, vi vill kunna få andrum genom att öppna säkra rutor
                timeBonusPerSafeReveal = 5f,

                // boss storlek sätts i Boot (bossWidth/bossHeight), så ingen custom här
                useCustomBoardSize = false,
                customWidth = 0,
                customHeight = 0,
                customMineDensity = 0f,

                // Viktigt byte: vi kör "boss_scorpion" nu,
                // inte bara "boss", så vi får röd accent, bakgrund etc.
                tilesetTheme = "boss_scorpion"
            };

            hasConfig = true;
            SceneManager.LoadScene("Gameplay");
        }

        // Tillbaka till huvudmenyn
        public static void BackToMenu()
        {
            hasConfig = false;
            SceneManager.LoadScene("MainMenu");
        }
    }
}
