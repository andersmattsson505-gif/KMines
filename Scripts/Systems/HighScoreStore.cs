using UnityEngine;

namespace KMines
{
    /// <summary>
    /// Lagrar highscores persistent via PlayerPrefs.
    /// - Campaign: per levelIndex
    /// - Arcade:   ett globalt värde
    /// - Boss:     ett globalt värde
    /// </summary>
    public static class HighScoreStore
    {
        const string KeyPrefix = "KMines_HighScore_";
        const string ArcadeKey = "KMines_HighScore_Arcade";
        const string BossKey = "KMines_HighScore_Boss";

        static string KeyFor(int levelIndex)
        {
            return KeyPrefix + levelIndex.ToString();
        }

        // -------- Campaign per level --------

        public static int GetHighScore(int levelIndex)
        {
            return PlayerPrefs.GetInt(KeyFor(levelIndex), 0);
        }

        public static bool TrySetHighScore(int levelIndex, int newScore)
        {
            int old = GetHighScore(levelIndex);
            if (newScore > old)
            {
                PlayerPrefs.SetInt(KeyFor(levelIndex), newScore);
                PlayerPrefs.Save();
                return true;
            }
            return false;
        }

        // -------- Arcade global --------

        public static int GetArcadeHighScore()
        {
            return PlayerPrefs.GetInt(ArcadeKey, 0);
        }

        public static bool TrySetArcadeHighScore(int newScore)
        {
            int old = GetArcadeHighScore();
            if (newScore > old)
            {
                PlayerPrefs.SetInt(ArcadeKey, newScore);
                PlayerPrefs.Save();
                return true;
            }
            return false;
        }

        // -------- Boss global --------

        public static int GetBossHighScore()
        {
            return PlayerPrefs.GetInt(BossKey, 0);
        }

        public static bool TrySetBossHighScore(int newScore)
        {
            int old = GetBossHighScore();
            if (newScore > old)
            {
                PlayerPrefs.SetInt(BossKey, newScore);
                PlayerPrefs.Save();
                return true;
            }
            return false;
        }
    }
}
