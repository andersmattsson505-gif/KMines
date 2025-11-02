using UnityEngine;
using System.Collections;

namespace KMines
{
    public class WinLoseManager : MonoBehaviour
    {
        [Header("Refs")]
        public Board board;
        public GameUI gameUI;
        public LevelLoader levelLoader;
        public GameTimer gameTimer; // <-- ny: används för time bonus

        [Header("UX")]
        public float showEndPanelDelay = 1.0f;

        bool missileGraceActive = false;
        float missileGraceTimer = 0f;

        bool ended;
        bool lost;

        void Update()
        {
            // “grace window” efter att man skjutit missile
            if (missileGraceActive)
            {
                missileGraceTimer -= Time.deltaTime;
                if (missileGraceTimer <= 0f)
                {
                    missileGraceActive = false;
                }
            }

            if (ended) return;
            if (board == null || board.grid == null) return;

            // Lose check
            if (CheckLose())
            {
                ended = true;
                lost = true;
                HandleLose();
                return;
            }

            // Win check
            if (CheckWin())
            {
                ended = true;
                lost = false;
                HandleWin();
                return;
            }
        }

        // Missilen får inte döda dig omedelbart (små explosioner runt dig osv)
        public void BeginMissileGrace() { BeginMissileGrace(0.25f); }
        public void BeginMissileGrace(float seconds)
        {
            missileGraceActive = true;
            missileGraceTimer = Mathf.Max(seconds, 0.01f);
        }

        bool CheckLose()
        {
            if (missileGraceActive)
                return false;

            for (int y = 0; y < board.height; y++)
            {
                for (int x = 0; x < board.width; x++)
                {
                    var c = board.grid[x, y];
                    if (c == null) continue;
                    if (c.opened && c.hasMine)
                        return true;
                }
            }
            return false;
        }

        bool CheckWin()
        {
            int totalSafe = 0;
            int openedSafe = 0;

            for (int y = 0; y < board.height; y++)
            {
                for (int x = 0; x < board.width; x++)
                {
                    var c = board.grid[x, y];
                    if (c == null) continue;

                    bool isMine = c.hasMine;
                    if (!isMine)
                    {
                        totalSafe++;
                        if (c.opened) openedSafe++;
                    }
                }
            }

            return totalSafe > 0 && openedSafe >= totalSafe;
        }

        // -------------------------------------------------
        // Timeout från GameTimer (tiden tog slut)
        // -------------------------------------------------
        public void OnTimeExpired()
        {
            if (ended) return;
            ended = true;
            lost = true;

            SaveHighScoreForThisLevel();

            if (gameUI != null)
                StartCoroutine(ShowTimeUpPanelAfterDelay());
        }

        // -------------------------------------------------
        // Lose (sprängd på mina)
        // -------------------------------------------------
        void HandleLose()
        {
            SaveHighScoreForThisLevel();

            if (gameUI != null)
                StartCoroutine(ShowLosePanelAfterDelay());
        }

        // -------------------------------------------------
        // Win (alla säkra rutor öppnade)
        //
        // Här delar vi ut:
        //  - flagBonus: 2500 per korrekt markerad mina
        //  - timeBonus: kvarvarande tid * 1000 (om timer aktiv)
        //
        // Efter bonus -> spara highscore -> visa Win-panel
        // -------------------------------------------------
        void HandleWin()
        {
            int flagBonus = 0;
            int timeBonus = 0;

            if (board != null)
            {
                // antal korrekt flaggade minor
                int correctFlags = board.CountCorrectFlags();
                flagBonus = correctFlags * 2500;
            }

            if (gameTimer != null && gameTimer.IsActive())
            {
                float tLeft = gameTimer.GetTimeLeft();
                timeBonus = Mathf.RoundToInt(tLeft * 1000f);
            }

            // ge bonuspoängen till total score
            if (board != null)
            {
                if (timeBonus > 0) board.AddScore(timeBonus);
                if (flagBonus > 0) board.AddScore(flagBonus);
            }

            // efter bonus → spara highscore
            SaveHighScoreForThisLevel();

            if (gameUI != null)
                StartCoroutine(ShowWinPanelAfterDelay());
        }

        // -------------------------------------------------
        // Highscore-hantering
        // -------------------------------------------------
        void SaveHighScoreForThisLevel()
        {
            if (board == null) return;

            int levelIndex = 0;
            if (levelLoader != null)
                levelIndex = levelLoader.currentIndex;

            HighScoreStore.TrySetHighScore(levelIndex, board.score);
        }

        // -------------------------------------------------
        // Visa paneler efter liten delay (så man hinner se sista klicket)
        // -------------------------------------------------
        IEnumerator ShowLosePanelAfterDelay()
        {
            yield return new WaitForSeconds(showEndPanelDelay);

            if (gameUI != null)
            {
                gameUI.ShowGameOver(RestartCurrentLevel);
            }
        }

        IEnumerator ShowWinPanelAfterDelay()
        {
            yield return new WaitForSeconds(showEndPanelDelay);

            if (gameUI != null)
            {
                gameUI.ShowWin(LoadNextLevel);
            }
        }

        IEnumerator ShowTimeUpPanelAfterDelay()
        {
            yield return new WaitForSeconds(showEndPanelDelay);

            if (gameUI != null)
            {
                gameUI.ShowEndPanel("TIME UP", RestartCurrentLevel);
            }
        }

        // -------------------------------------------------
        // Restart / Next level
        // -------------------------------------------------
        public void RestartCurrentLevel()
        {
            ended = false;
            lost = false;
            missileGraceActive = false;
            missileGraceTimer = 0f;

            if (levelLoader != null)
            {
                levelLoader.ReloadCurrentLevel();
            }
            else if (board != null)
            {
                board.Build();
            }

            if (gameUI != null)
                gameUI.HideEndPanel();
        }

        public void LoadNextLevel()
        {
            ended = false;
            lost = false;
            missileGraceActive = false;
            missileGraceTimer = 0f;

            if (levelLoader != null)
            {
                levelLoader.LoadNextLevel();
            }
            else if (board != null)
            {
                board.Build();
            }

            if (gameUI != null)
                gameUI.HideEndPanel();
        }
    }
}
