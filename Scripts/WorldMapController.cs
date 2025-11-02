using UnityEngine;
using UnityEngine.SceneManagement;

namespace KMines
{
    // Sätt detta på ett GameObject i WorldMap-scenen.
    // Varje level-nod i kartan kan ha en knapp som anropar PlayLevel(index).
    public class WorldMapController : MonoBehaviour
    {
        public void PlayLevel(int levelIndex)
        {
            GameModeSettings.LaunchCampaignLevel(levelIndex);
        }

        public void PlayBoss()
        {
            GameModeSettings.LaunchBoss();
        }

        public void BackToMenu()
        {
            SceneManager.LoadScene("MainMenu");
        }
    }
}
