// --- HUVUD-CANVAS ---
Canvas targetCanvas = null;
var all = FindObjectsOfType<Canvas>();
if (all.Length > 0)
{
    // ta den som heter Canvas först
    for (int i = 0; i < all.Length; i++)
        if (all[i].name == "Canvas") { targetCanvas = all[i]; break; }
    if (targetCanvas == null) targetCanvas = all[0];
}
if (targetCanvas == null)
{
    var cGO = new GameObject("Canvas");
    targetCanvas = cGO.AddComponent<Canvas>();
    targetCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
    cGO.AddComponent<CanvasScaler>();
    cGO.AddComponent<GraphicRaycaster>();
}

// --- NY TOP-HUD ---
var hudTopGO = new GameObject("HUDTop", typeof(RectTransform));
hudTopGO.transform.SetParent(targetCanvas.transform, false);
var hudTopRT = hudTopGO.GetComponent<RectTransform>();
hudTopRT.anchorMin = new Vector2(0f, 1f);
hudTopRT.anchorMax = new Vector2(1f, 1f);
hudTopRT.pivot = new Vector2(0.5f, 1f);
hudTopRT.offsetMin = new Vector2(0f, -96f);
hudTopRT.offsetMax = new Vector2(0f, 0f);
var hudTop = hudTopGO.AddComponent<HUDTop>();

// --- VISOR SCAN EFFECT ---
var visorScanGO = new GameObject("VisorScanEffect");
var visorScan = visorScanGO.AddComponent<VisorScanEffect>();
visorScan.board = board;

// koppla in i HUD
hudTop.scanEffect = visorScan;
hudTop.board = board;
hudTop.gameUI = gameUI;
