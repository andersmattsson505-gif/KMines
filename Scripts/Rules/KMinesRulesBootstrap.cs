using UnityEngine;

namespace KMines
{
    [DefaultExecutionOrder(150)]
    public class KMinesRulesBootstrap : MonoBehaviour
    {
        public WinLoseManager rules;

        void Start()
        {
            if (!rules) rules = FindObjectOfType<WinLoseManager>();
            if (!rules) rules = gameObject.AddComponent<WinLoseManager>();
        }
    }
}