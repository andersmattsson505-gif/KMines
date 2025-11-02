using UnityEngine;
using UnityEngine.UI;

namespace KMines
{
    [RequireComponent(typeof(Button))]
    public class PauseButtonUI : MonoBehaviour
    {
        // Valfritt: om du vill sätta manuellt. Lämna tomt – vi auto-finder ändå.
        public GameUI gameUI;

        Button btn;

        void Awake()
        {
            btn = GetComponent<Button>();
            btn.onClick.AddListener(OnPausePressed);
        }

        void Start()
        {
            // Boot har DefaultExecutionOrder(-5000) => Boot.Start har redan kört här.
            if (gameUI == null)
            {
                gameUI = FindObjectOfType<GameUI>();
                if (gameUI == null)
                {
                    Debug.LogWarning("[PauseButtonUI] Hittade ingen GameUI i scenen ännu. " +
                                     "Knappen kommer försöka igen vid första klick.");
                }
            }
        }

        void OnPausePressed()
        {
            if (gameUI == null)
                gameUI = FindObjectOfType<GameUI>();

            if (gameUI != null)
                gameUI.ShowPausePanel();
            else
                Debug.LogWarning("[PauseButtonUI] Kunde inte pausa – GameUI saknas.");
        }
    }
}
