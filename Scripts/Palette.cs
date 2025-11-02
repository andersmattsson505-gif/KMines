using UnityEngine;

namespace KMines
{
    public static class Palette
    {
        public static Color ForCount(int n)
        {
            switch (Mathf.Clamp(n,1,8))
            {
                case 1: return new Color(0.20f, 0.95f, 0.45f);
                case 2: return new Color(0.20f, 0.90f, 1.00f);
                case 3: return new Color(1.00f, 0.70f, 0.20f);
                case 4: return new Color(1.00f, 0.40f, 0.20f);
                case 5: return new Color(1.00f, 0.25f, 0.20f);
                case 6: return new Color(0.90f, 0.30f, 0.80f);
                case 7: return new Color(0.70f, 0.60f, 1.00f);
                default: return new Color(0.90f, 0.95f, 1.00f);
            }
        }
    }
}
