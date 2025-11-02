using UnityEngine;
using UnityEngine.UI;

namespace KMines
{
    /// <summary>
    /// Minimal HUD som bara exponerar samma fält som tidigare så att Boot.cs m.fl. inte bryts.
    /// Vi ritar INTE längre sidoguttar här – låt Canvas/scene hantera layouten.
    /// </summary>
    [DisallowMultipleComponent]
    public class KaelenHUD : MonoBehaviour
    {
        [Header("Auto-wired if empty")]
        public Board board;
        public LevelLoader loader;

        [Header("Layout")]
        public float topBarHeight = 96f;
        public float sideGutterWidth = 0f; // måste finnas kvar för Boot.cs

        [Header("Colors")]
        public Color topBarColor = new Color(0.06f, 0.07f, 0.1f, 1f);
        public Color gutterColor = Color.black;
        public Color accentColor = new Color(0.35f, 0.8f, 1f, 1f);
        public Color scoreTextColor = Color.white;

        void Awake()
        {
            if (!board) board = FindObjectOfType<Board>();
            if (!loader) loader = FindObjectOfType<LevelLoader>();
            // Vi gör inget mer – din befintliga UI-hierarki får ligga kvar.
        }

        // enkel helper så andra kan uppdatera score i UI om de hittar en Text
        public void SetScore(int value)
        {
            var txt = GetComponentInChildren<Text>();
            if (txt) txt.text = value.ToString();
        }
    }
}
