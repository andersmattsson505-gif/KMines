#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace KMines
{
    /// <summary>
    /// Enkla menykommandon för att skapa/hantera Board i scenen från Editorn.
    /// Låter dig jobba i Inspector utan att ändra fungerande runtime-logik.
    /// </summary>
    public static class KMinesBoardMenu
    {
        [MenuItem("KMines/Create Board in Scene")]
        static void CreateBoard()
        {
            var existing = Object.FindObjectOfType<Board>();
            if (existing != null)
            {
                Selection.activeObject = existing.gameObject;
                EditorUtility.DisplayDialog("KMines", "Det finns redan ett Board i scenen. Markerar det istället.", "OK");
                return;
            }

            var go = new GameObject("Board");
            Undo.RegisterCreatedObjectUndo(go, "Create KMines Board");
            var b = go.AddComponent<Board>();
            b.autoBuildInEditor = true;
            b.width = 10;
            b.height = 16;
            b.tileSize = 1f;
            b.mineDensity = 0.16f;
            b.Build();
            Selection.activeObject = go;
        }

        [MenuItem("KMines/Rebuild Board (Editor)")]
        static void RebuildBoard()
        {
            var b = Object.FindObjectOfType<Board>();
            if (b != null)
            {
                b.Build();
                Selection.activeObject = b.gameObject;
            }
            else
            {
                EditorUtility.DisplayDialog("KMines", "Hittade inget Board i scenen.", "OK");
            }
        }

        [MenuItem("KMines/Select Board")]
        static void SelectBoard()
        {
            var b = Object.FindObjectOfType<Board>();
            if (b != null) Selection.activeObject = b.gameObject;
        }
    }
}
#endif
