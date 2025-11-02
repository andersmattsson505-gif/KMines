using UnityEngine;
using UnityEngine.SceneManagement;

namespace KMines
{
    /// <summary>
    /// Sätt detta på ett GameObject i MainMenu-scenen.
    /// Knapparna i UI ska kopplas till dessa publika metoder i Inspector.
    /// 
    /// Viktigt:
    /// - OnArcadePressed() laddar nu scenen "ArcadeMenu"
    ///   istället för att starta Quick Play direkt.
    ///   Det betyder att spelaren hamnar i Arcade-hubben
    ///   där de kan välja Quick Play eller Custom Run.
    /// 
    /// - Campaign går till WorldMap.
    /// - Boss kör GameModeSettings.LaunchBoss() (orörd).
    /// - Credits laddar "Credits".
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        /// <summary>
        /// Campaign-knappen i huvudmenyn.
        /// Laddar världskartan / campaign select.
        /// </summary>
        public void OnCampaignPressed()
        {
            SceneManager.LoadScene("WorldMap");
        }

        /// <summary>
        /// Arcade-knappen i huvudmenyn.
        /// Istället för att hoppa rakt in i Quick Play
        /// går vi nu till ArcadeMenu-scenen.
        /// Där väljer spelaren Quick Play eller Custom Run.
        /// </summary>
        public void OnArcadePressed()
        {
            SceneManager.LoadScene("ArcadeMenu");
        }

        /// <summary>
        /// Boss Mode / special encounter.
        /// Detta använder fortfarande GameModeSettings
        /// så vi ändrar det inte här.
        /// </summary>
        public void OnBossPressed()
        {
            GameModeSettings.LaunchBoss();
        }

        /// <summary>
        /// Options. (Inte implementerat ännu).
        /// </summary>
        public void OnOptionsPressed()
        {
            Debug.Log("OPTIONS pressed (todo: visa options UI)");
        }

        /// <summary>
        /// Credits / juridik / tack.
        /// Kräver att scenen 'Credits' finns i Build Settings.
        /// </summary>
        public void OnCreditsPressed()
        {
            SceneManager.LoadScene("Credits");
        }
    }
}
