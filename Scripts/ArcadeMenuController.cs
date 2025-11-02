using UnityEngine;
using UnityEngine.UI;

namespace KMines
{
    /// <summary>
    /// Controller för ArcadeMenu-scenen.
    ///
    /// Koppla i Inspector:
    ///  - widthField, heightField: InputField (Legacy)
    ///  - mineDensityField: InputField (Legacy)
    ///  - missilesToggle: Toggle (Legacy)
    ///  - timedToggle: Toggle (Legacy)
    ///  - startTimeField: InputField (Legacy, sekunder)
    ///  - timeBonusToggle: Toggle (Legacy)
    ///  - themeDropdown: Dropdown (Legacy)
    ///
    ///  - quickPlayButton: Button "QUICK PLAY"
    ///  - customStartButton: Button "START CUSTOM RUN"
    ///  - backButton: Button "BACK"
    ///
    /// Viktigt:
    ///  - Vi clAmpar width/height till max 12x16 innan vi skickar in det
    ///    till GameModeSettings. Det betyder att vi alltid håller oss
    ///    inom en visuell layout som funkar för mobilen.
    /// </summary>
    public class ArcadeMenuController : MonoBehaviour
    {
        [Header("Custom Run Settings (UI)")]
        public InputField widthField;          // tex "12"
        public InputField heightField;         // tex "16"
        public InputField mineDensityField;    // tex "0.20"
        public Toggle missilesToggle;          // tillåt missiler?
        public Toggle timedToggle;             // timed-läge på/av?
        public InputField startTimeField;      // tex "30" sek
        public Toggle timeBonusToggle;         // +5 sek per säker ruta?

        [Header("Theme / Look")]
        public Dropdown themeDropdown;
        // 0 = Random
        // 1 = Grass
        // 2 = Desert
        // 3 = Ice
        // 4 = Toxic
        // 5 = Boss Scorpion

        [Header("Navigation")]
        public Button quickPlayButton;
        public Button customStartButton;
        public Button backButton;

        void Start()
        {
            if (quickPlayButton != null)
                quickPlayButton.onClick.AddListener(OnQuickPlayPressed);

            if (customStartButton != null)
                customStartButton.onClick.AddListener(OnCustomRunPressed);

            if (backButton != null)
                backButton.onClick.AddListener(OnBackPressed);
        }

        // QUICK PLAY – standard arcade-run
        public void OnQuickPlayPressed()
        {
            GameModeSettings.LaunchArcadeQuick();
        }

        // CUSTOM RUN – bygg en runda med spelarnas val
        public void OnCustomRunPressed()
        {
            int w = ParseIntField(widthField, 12);
            int h = ParseIntField(heightField, 16);

            // MAXTAK enligt vår design (12x16 är "riktig" Kaelen Mines layout)
            w = Mathf.Clamp(w, 1, 12);
            h = Mathf.Clamp(h, 1, 16);

            float dens = ParseFloatField(mineDensityField, 0.20f);

            bool allowMissiles = missilesToggle != null && missilesToggle.isOn;
            bool timed = timedToggle != null && timedToggle.isOn;
            float startTimeSeconds = ParseFloatField(startTimeField, 30f);

            // Panic/timebonus-läge: +5 sek per säker ruta
            float bonusPerReveal = (timeBonusToggle != null && timeBonusToggle.isOn) ? 5f : 0f;

            string chosenTheme = GetThemeNameFromDropdown();

            GameModeSettings.LaunchArcadeCustom(
                w,
                h,
                dens,
                allowMissiles,
                timed,
                startTimeSeconds,
                bonusPerReveal,
                chosenTheme
            );
        }

        // BACK – tillbaka till Main Menu
        public void OnBackPressed()
        {
            GameModeSettings.BackToMenu();
        }

        // -------------------------
        // Helpers
        // -------------------------
        int ParseIntField(InputField field, int fallback)
        {
            if (field == null) return fallback;
            if (string.IsNullOrWhiteSpace(field.text)) return fallback;
            int val;
            if (!int.TryParse(field.text, out val)) return fallback;
            if (val < 1) val = 1;
            return val;
        }

        float ParseFloatField(InputField field, float fallback)
        {
            if (field == null) return fallback;
            if (string.IsNullOrWhiteSpace(field.text)) return fallback;

            float val;
            // Först försök invariant kultur (punkt som decimal)
            if (!float.TryParse(
                    field.text,
                    System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out val))
            {
                // Sen försök systemlokal (svenska decimaler med komma etc)
                if (!float.TryParse(field.text, out val))
                    return fallback;
            }

            if (val < 0f) val = 0f;
            return val;
        }

        string GetThemeNameFromDropdown()
        {
            if (themeDropdown == null) return "random";
            int idx = themeDropdown.value;
            switch (idx)
            {
                case 1: return "grass";
                case 2: return "desert";
                case 3: return "ice";
                case 4: return "toxic";
                case 5: return "boss_scorpion";
                default: return "random"; // 0
            }
        }
    }
}
