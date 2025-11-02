using UnityEngine;

namespace KMines
{
    [CreateAssetMenu(fileName = "KMinesLevel", menuName = "KMines/Level", order = 0)]
    public class KMinesLevel : ScriptableObject
    {
        public int width = 10;
        public int height = 13;
        [Range(0.05f, 0.30f)] public float mineDensity = 0.16f;
        public int missiles = 3;
        public float countdownSeconds = 0f; // 0 = ingen timer
        public string themeName = "";
    }
}