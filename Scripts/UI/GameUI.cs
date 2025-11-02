using UnityEngine;
using UnityEngine.UI;
using System;

namespace KMines
{
    // GameUI:
    //  - Visar overlay-panel för:
    //      * GAME OVER / YOU WIN / TIME UP
    //      * PAUSED (via hamburgaren)
    //
    //  - Knappar i overlay visas VISUELLT i den här ordningen uppifrån och ner:
    //      RESUME / RESTART
    //      ARCADE MENU
    //      MAIN MENU
    //
    //  - När vi är i paus:
    //      * Timer stoppas
    //      * Input (SmartClickInput) stängs av
    //      * Översta knappen heter "RESUME" och återupptar spelet
    //
    //  - När rundan är slut:
    //      * Timer stoppas
    //      * Input stängs av
    //      * Översta knappen heter "RESTART" och startar om rundan
    //
    //  Boot kopplar in:
    //      - board
    //      - inputRef
    //      - gameTimer
    public class GameUI : MonoBehaviour
    {
        // Referenser in i spelet (kopplas i Boot)
        Board board;
        public SmartClickInput inputRef;   // sätts från Boot
        public GameTimer gameTimer;        // sätts från Boot

        // Canvas + overlay-panel
        Canvas overlayCanvas;
        GameObject overlayRoot;

        // UI-element i overlayn
        Text titleLabel;

        Button restartButton;      // översta knappen visuellt
        Text restartLabel;

        Button arcadeMenuButton;   // mitten
        Text arcadeMenuLabel;

        Button mainMenuButton;     // nederst
        Text mainMenuLabel;

        // fallback font
        Font fallbackFont;

        // callback som WinLoseManager kan vilja köra när vi trycker "Restart"/"Continue"
        Action externalRestartCallback;

        // ----------------------------------------------------
        // Boot kallar denna direkt efter att Board skapats
        // ----------------------------------------------------
        public void SetBoard(Board b)
        {
            board = b;
        }

        // ----------------------------------------------------
        // Unity lifecycle
        // ----------------------------------------------------
        void Awake()
        {
            // Försök få fram ett vettigt runtime-typsnitt
            try { fallbackFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); } catch { }
            if (fallbackFont == null)
            {
                try { fallbackFont = Resources.GetBuiltinResource<Font>("Arial.ttf"); } catch { }
            }

            // Förbered canvas i förväg så vi slipper poppa in i samma frame
            EnsureCanvas();
            // overlayRoot+panel byggs lazily i EnsureOverlayPanel()
        }

        void Update()
        {
            // lämnas tom pga bakåtkompabilitet om andra script antog GameUI.Update finns
        }

        // ----------------------------------------------------
        // Publika metoder WinLoseManager / GameTimer / etc kan anropa
        // ----------------------------------------------------

        // GAME OVER / YOU WIN / TIME UP-variant med callback
        public void ShowGameOver(Action cb)
        {
            ShowEndPanel("GAME OVER", "RESTART", cb);
        }
        public void ShowGameOver()
        {
            ShowEndPanel("GAME OVER", "RESTART", null);
        }

        public void ShowWin(Action cb)
        {
            ShowEndPanel("YOU WIN", "RESTART", cb);
        }
        public void ShowWin()
        {
            ShowEndPanel("YOU WIN", "RESTART", null);
        }

        public void ShowTimeUp(Action cb)
        {
            ShowEndPanel("TIME UP", "RESTART", cb);
        }
        public void ShowTimeUp()
        {
            ShowEndPanel("TIME UP", "RESTART", null);
        }

        // Äldre signatur (existerar i WinLoseManager):
        public void ShowEndPanel(string title, Action cb)
        {
            ShowEndPanel(title, "RESTART", cb);
        }

        // Slutskärm (när rundan är färdig / förlorad / time up)
        // title: "GAME OVER", "YOU WIN", ...
        // buttonText: oftast "RESTART" eller "CONTINUE"
        // cbOptional: om du vill göra nåt special innan restart (t ex load next level)
        public void ShowEndPanel(string title, string buttonText, Action cbOptional = null)
        {
            EnsureOverlayPanel();

            // stoppa input och timer när rundan är slut
            if (inputRef != null) inputRef.enabled = false;
            if (gameTimer != null) gameTimer.PauseTimer();

            externalRestartCallback = cbOptional;

            if (titleLabel != null)
                titleLabel.text = title;

            if (restartLabel != null)
                restartLabel.text = buttonText; // "RESTART" / "CONTINUE"

            if (arcadeMenuLabel != null)
                arcadeMenuLabel.text = "ARCADE MENU";

            if (mainMenuLabel != null)
                mainMenuLabel.text = "MAIN MENU";

            // Översta knappen = Restart
            if (restartButton != null)
            {
                restartButton.onClick.RemoveAllListeners();
                restartButton.onClick.AddListener(OnRestartClicked);
            }

            // Mittenknappen = Arcade Menu
            if (arcadeMenuButton != null)
            {
                arcadeMenuButton.onClick.RemoveAllListeners();
                arcadeMenuButton.onClick.AddListener(OnArcadeMenuClicked);
            }

            // Nedersta knappen = Main Menu
            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.RemoveAllListeners();
                mainMenuButton.onClick.AddListener(OnMainMenuClicked);
            }

            if (overlayRoot != null)
                overlayRoot.SetActive(true);
        }

        // PAUS (hamburgaren kallar detta via PauseButtonUI)
        //
        // Vi vill:
        //   - Frysa timer
        //   - Stänga av input
        //   - Visa overlay med:
        //        Title = "PAUSED"
        //        [RESUME]  (överst, dvs restartButton visuellt)
        //        [ARCADE MENU]
        //        [MAIN MENU]
        //
        public void ShowPausePanel()
        {
            EnsureOverlayPanel();

            // pausa spelet
            if (inputRef != null) inputRef.enabled = false;
            if (gameTimer != null) gameTimer.PauseTimer();

            externalRestartCallback = null; // paus har ingen special next-level

            if (titleLabel != null)
                titleLabel.text = "PAUSED";

            if (restartLabel != null)
                restartLabel.text = "RESUME";

            if (arcadeMenuLabel != null)
                arcadeMenuLabel.text = "ARCADE MENU";

            if (mainMenuLabel != null)
                mainMenuLabel.text = "MAIN MENU";

            // Översta knappen = RESUME
            if (restartButton != null)
            {
                restartButton.onClick.RemoveAllListeners();
                restartButton.onClick.AddListener(OnResumeClicked);
            }

            // Mittenknappen = Arcade Menu
            if (arcadeMenuButton != null)
            {
                arcadeMenuButton.onClick.RemoveAllListeners();
                arcadeMenuButton.onClick.AddListener(OnArcadeMenuClicked);
            }

            // Nedersta knappen = Main Menu
            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.RemoveAllListeners();
                mainMenuButton.onClick.AddListener(OnMainMenuClicked);
            }

            if (overlayRoot != null)
                overlayRoot.SetActive(true);
        }

        // Dölj overlay (används när vi RESUME eller efter restart)
        public void HideEndPanel()
        {
            if (overlayRoot != null)
                overlayRoot.SetActive(false);

            externalRestartCallback = null;
        }

        // ----------------------------------------------------
        // Knapplogik
        // ----------------------------------------------------

        // Översta knappen när rundan är över ("RESTART")
        void OnRestartClicked()
        {
            // 1) kör extern callback (t.ex. nästa level)
            externalRestartCallback?.Invoke();

            // 2) bygg om brädet => ny runda, nya minor osv
            if (board != null)
            {
                board.Build();
            }

            // 3) återaktivera input och timer
            if (inputRef != null) inputRef.enabled = true;
            if (gameTimer != null) gameTimer.ResumeTimer();

            // 4) stäng overlay
            HideEndPanel();
        }

        // Översta knappen i paus ("RESUME")
        void OnResumeClicked()
        {
            if (inputRef != null) inputRef.enabled = true;
            if (gameTimer != null) gameTimer.ResumeTimer();

            HideEndPanel();
        }

        // Mittenknapp
        void OnArcadeMenuClicked()
        {
            GameModeSettings.BackToArcadeMenu();
        }

        // Nedersta knappen
        void OnMainMenuClicked()
        {
            GameModeSettings.BackToMenu();
        }

        // ----------------------------------------------------
        // Canvas/Overlay-bygge
        // ----------------------------------------------------

        void EnsureCanvas()
        {
            if (overlayCanvas != null) return;

            var cgo = new GameObject("GameUI_Canvas");
            cgo.transform.SetParent(transform, false);

            overlayCanvas = cgo.AddComponent<Canvas>();
            overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            overlayCanvas.sortingOrder = 50000; // ovanför HUD/visor/missil

            var scaler = cgo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            cgo.AddComponent<GraphicRaycaster>();
        }

        void EnsureOverlayPanel()
        {
            if (overlayRoot != null) return;
            EnsureCanvas();

            // 1) fullskärms dim
            overlayRoot = new GameObject("EndPanelOverlay");
            overlayRoot.transform.SetParent(overlayCanvas.transform, false);

            var rootRT = overlayRoot.AddComponent<RectTransform>();
            rootRT.anchorMin = Vector2.zero;
            rootRT.anchorMax = Vector2.one;
            rootRT.offsetMin = Vector2.zero;
            rootRT.offsetMax = Vector2.zero;
            rootRT.pivot = new Vector2(0.5f, 0.5f);

            var dimImg = overlayRoot.AddComponent<Image>();
            dimImg.color = new Color(0f, 0f, 0f, 0.6f);

            // 2) kort/panel i mitten
            var cardGO = new GameObject("Card");
            cardGO.transform.SetParent(overlayRoot.transform, false);

            var cardRT = cardGO.AddComponent<RectTransform>();
            cardRT.anchorMin = new Vector2(0.5f, 0.5f);
            cardRT.anchorMax = new Vector2(0.5f, 0.5f);
            cardRT.pivot = new Vector2(0.5f, 0.5f);
            cardRT.sizeDelta = new Vector2(480f, 400f);
            cardRT.anchoredPosition = Vector2.zero;

            var cardImg = cardGO.AddComponent<Image>();
            cardImg.color = new Color(0.08f, 0.08f, 0.10f, 0.95f);

            // 3) Titel ("PAUSED", "GAME OVER", etc.)
            var titleGO = new GameObject("Title");
            titleGO.transform.SetParent(cardGO.transform, false);

            var titleRT = titleGO.AddComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0.5f, 1f);
            titleRT.anchorMax = new Vector2(0.5f, 1f);
            titleRT.pivot = new Vector2(0.5f, 1f);
            titleRT.sizeDelta = new Vector2(420f, 80f);
            titleRT.anchoredPosition = new Vector2(0f, -32f);

            titleLabel = titleGO.AddComponent<Text>();
            titleLabel.alignment = TextAnchor.MiddleCenter;
            titleLabel.fontSize = 40;
            titleLabel.color = Color.white;
            titleLabel.text = "GAME OVER";
            titleLabel.font = fallbackFont;

            // Hjälpfunktion för att skapa en knapp + label
            Button MakeButton(out Text labelOut, string goName, float anchoredY, string defaultText)
            {
                var btnGO = new GameObject(goName);
                btnGO.transform.SetParent(cardGO.transform, false);

                var btnRT = btnGO.AddComponent<RectTransform>();
                btnRT.anchorMin = new Vector2(0.5f, 0f);
                btnRT.anchorMax = new Vector2(0.5f, 0f);
                btnRT.pivot = new Vector2(0.5f, 0f);
                btnRT.sizeDelta = new Vector2(300f, 64f);

                // OBS: anchoredY = avstånd från kortets NEDERKANT,
                // och ett STÖRRE värde betyder HÖGRE upp på kortet
                btnRT.anchoredPosition = new Vector2(0f, anchoredY);

                var btnImg = btnGO.AddComponent<Image>();
                btnImg.color = new Color(0.18f, 0.32f, 0.9f, 1f);

                var buttonComp = btnGO.AddComponent<Button>();
                buttonComp.targetGraphic = btnImg;

                var labelGO = new GameObject("Label");
                labelGO.transform.SetParent(btnGO.transform, false);

                var labelRT = labelGO.AddComponent<RectTransform>();
                labelRT.anchorMin = Vector2.zero;
                labelRT.anchorMax = Vector2.one;
                labelRT.offsetMin = Vector2.zero;
                labelRT.offsetMax = Vector2.zero;
                labelRT.pivot = new Vector2(0.5f, 0.5f);

                var textComp = labelGO.AddComponent<Text>();
                textComp.alignment = TextAnchor.MiddleCenter;
                textComp.fontSize = 32;
                textComp.color = Color.white;
                textComp.text = defaultText;
                textComp.font = fallbackFont;

                labelOut = textComp;
                return buttonComp;
            }

            // Nu lägger vi knapparna utifrån hur de SKA synas:
            // Högst upp visuellt = störst anchoredY
            // ------------------------------------------------
            // ÖVERST: restartButton (RESUME / RESTART)
            restartButton = MakeButton(out restartLabel,
                                       "RestartOrResumeButton",
                                       208f,         // högst upp
                                       "RESTART");

            // MITTEN: arcadeMenuButton
            arcadeMenuButton = MakeButton(out arcadeMenuLabel,
                                          "ArcadeMenuButton",
                                          120f,         // mitten
                                          "ARCADE MENU");

            // NEDERST: mainMenuButton
            mainMenuButton = MakeButton(out mainMenuLabel,
                                        "MainMenuButton",
                                        32f,           // längst ner
                                        "MAIN MENU");

            // starta gömt
            overlayRoot.SetActive(false);
        }
    }

    // Små förlängningar så GameTimer kan pausas/resumas av GameUI
    public static class GameTimerExtensions
    {
        public static void PauseTimer(this GameTimer t)
        {
            // Lite försiktig: om t är null gör inget
            if (t == null) return;

            // Vi "pausar" bara genom att säga att den inte är aktiv.
            // En riktig paus/resume kräver att GameTimer kan lagra aktiv/timeLeft-state.
            // Vi utökar GameTimer i framtiden om du vill ha exakt freeze/resume.
            // För nu: vi stannar nedräkningen.
            var f = t.GetType().GetField("active", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (f != null) f.SetValue(t, false);
        }

        public static void ResumeTimer(this GameTimer t)
        {
            if (t == null) return;

            // Aktivera igen om det fanns tid kvar
            var activeF = t.GetType().GetField("active", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var timeLeftF = t.GetType().GetField("timeLeft", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (activeF != null && timeLeftF != null)
            {
                float tl = (float)timeLeftF.GetValue(t);
                if (tl > 0f)
                {
                    activeF.SetValue(t, true);
                }
            }
        }
    }
}
