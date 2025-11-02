using UnityEngine;

namespace KMines
{
    [System.Serializable]
    public struct BoardTheme
    {
        public Color backplateColor;
        public string closedTexPath;
        public string openTexPath;
        public Color numberColor;
    }

    public static class ThemeLibrary
    {
        public static BoardTheme GetTheme(string themeName)
        {
            // fallback / default
            BoardTheme t;
            t.closedTexPath = "Art/tile_closed";
            t.openTexPath   = "Art/tile_open";
            t.numberColor   = Color.white;
            t.backplateColor = new Color(0.13f, 0.15f, 0.18f, 1f);

            if (string.IsNullOrEmpty(themeName))
                return t;

            switch (themeName.ToLower())
            {
                case "desert":
                    // sandgul
                    t.backplateColor = new Color(0.60f, 0.50f, 0.30f, 1f);
                    // (senare kan du peka closedTexPath / openTexPath till desert-texturer)
                    return t;

                case "ice":
                    // kall blå
                    t.backplateColor = new Color(0.20f, 0.30f, 0.45f, 1f);
                    return t;

                case "boss":
                    // mörkröd
                    t.backplateColor = new Color(0.20f, 0.00f, 0.00f, 1f);
                    return t;

                case "grass":
                case "default":
                default:
                    return t;
            }
        }
    }
}
