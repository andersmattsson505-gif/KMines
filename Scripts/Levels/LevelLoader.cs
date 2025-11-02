using UnityEngine;

namespace KMines
{
    [System.Serializable]
    public struct LevelDef
    {
        public int width;
        public int height;
        public float tileSize;
        public float mineDensity;

        public bool timed;
        public float timeLimitSeconds;
    }

    // Laddar levels och talar om för Board + GameTimer hur varje level ska se ut
    public class LevelLoader : MonoBehaviour
    {
        public Board board;
        public GameTimer gameTimer;

        public LevelDef[] levels;
        public int currentIndex = 0;

        public void LoadLevel(int index)
        {
            if (board == null) return;
            if (levels == null || levels.Length == 0) return;

            if (index < 0) index = 0;
            if (index >= levels.Length) index = levels.Length - 1;

            currentIndex = index;
            var lvl = levels[currentIndex];

            int safeW     = (lvl.width      <= 0) ? 10 :  lvl.width;
            int safeH     = (lvl.height     <= 0) ? 16 :  lvl.height;
            float safeT   = (lvl.tileSize   <= 0f) ? 1.0f : lvl.tileSize;
            float safeDen = (lvl.mineDensity<= 0f) ? 0.16f : lvl.mineDensity;

            board.width = safeW;
            board.height = safeH;
            board.tileSize = safeT;
            board.mineDensity = safeDen;

            // bygg själva brädet (Board tar hand om theme/missiles som redan är satta av Boot)
            board.Build();

            // Timer
            if (gameTimer == null)
                gameTimer = FindObjectOfType<GameTimer>();

            if (gameTimer != null)
            {
                if (lvl.timed && lvl.timeLimitSeconds > 0f)
                    gameTimer.StartLevelTimer(true, lvl.timeLimitSeconds);
                else
                    gameTimer.StartLevelTimer(false, 0f);
            }
        }

        public void ReloadCurrentLevel()
        {
            LoadLevel(currentIndex);
        }

        public void LoadNextLevel()
        {
            if (levels == null || levels.Length == 0) return;

            int next = currentIndex + 1;
            if (next >= levels.Length)
                next = 0;

            LoadLevel(next);
        }
    }
}
