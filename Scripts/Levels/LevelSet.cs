using UnityEngine;

namespace KMines
{
    [CreateAssetMenu(fileName = "KMinesLevelSet", menuName = "KMines/Level Set", order = 1)]
    public class LevelSet : ScriptableObject
    {
        public KMinesLevel[] levels;
    }
}