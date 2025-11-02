using UnityEngine;

namespace KMines
{
    /// <summary>
    /// Enkel "ge mig resurser"-hjälpare för test.
    /// Kör i Start() och ser till att spelaren har minst X Visor charges.
    /// Det här påverkar PlayerPrefs, så värdet hänger kvar mellan rundor.
    /// </summary>
    public class VisorCheatInit : MonoBehaviour
    {
        [Tooltip("Minsta antal Visor-charges spelaren ska ha vid start av scenen.")]
        public int minVisorCharges = 3;

        void Start()
        {
            int have = PlayerInventory.GetPulseVisorOwned();
            if (have < minVisorCharges)
            {
                int diff = minVisorCharges - have;
                PlayerInventory.AddPulseVisor(diff);
                Debug.Log("[VisorCheatInit] Added " + diff + " Visor charges. Total now: " + PlayerInventory.GetPulseVisorOwned());
            }
        }
    }
}
