using UnityEngine;

/// <summary>
/// Global meta-inventory som lever utanför en enskild runda.
/// Håller antal Missiles och Visor charges som spelaren ÄGER just nu (persistent).
/// - Dessa kan fyllas på gratis (belöningar) eller via köp.
/// - När en runda startar tar vi ut en "loadout" ur lagret.
/// - Under rundan minskar charges, men lagret har redan dragits av då.
/// 
/// Viktigt:
/// 1. Den här klassen gör INGET i spelet ännu. Den bara finns.
/// 2. När vi senare kopplar ihop den med GameModeSettings och HUD
///    kan vi läsa härifrån och minska charges när spelaren faktiskt använder Missile/Visor.
/// </summary>
public static class PlayerInventory
{
    // PlayerPrefs keys
    private const string MissilesKey = "KMines_PlayerInv_Missiles";
    private const string PulseVisorKey = "KMines_PlayerInv_PulseVisor";

    // Laddad state i minnet (spelets session)
    private static bool _loaded = false;
    private static int _missilesOwned = 0;
    private static int _pulseVisorOwned = 0;

    /// <summary>
    /// Säkerställ att vi har läst in värdena från PlayerPrefs.
    /// </summary>
    private static void EnsureLoaded()
    {
        if (_loaded) return;
        _missilesOwned = PlayerPrefs.GetInt(MissilesKey, 0);
        _pulseVisorOwned = PlayerPrefs.GetInt(PulseVisorKey, 0);
        _loaded = true;
    }

    /// <summary>
    /// Spara tillbaka nuvarande state till PlayerPrefs.
    /// </summary>
    private static void SaveNow()
    {
        PlayerPrefs.SetInt(MissilesKey, _missilesOwned);
        PlayerPrefs.SetInt(PulseVisorKey, _pulseVisorOwned);
        PlayerPrefs.Save();
    }

    // ---------------------------------------------------------
    // Publika getters / ändringar
    // ---------------------------------------------------------

    /// <summary>
    /// Hur många missiles äger spelaren globalt just nu (utanför runda)?
    /// </summary>
    public static int GetMissilesOwned()
    {
        EnsureLoaded();
        return _missilesOwned;
    }

    /// <summary>
    /// Hur många Pulse Visor charges äger spelaren globalt just nu?
    /// </summary>
    public static int GetPulseVisorOwned()
    {
        EnsureLoaded();
        return _pulseVisorOwned;
    }

    /// <summary>
    /// Lägg till X missiles i lagret (t.ex. reward eller köp).
    /// Kan använda negativa värden för att ta bort, men normalt inte.
    /// </summary>
    public static void AddMissiles(int amount)
    {
        EnsureLoaded();
        _missilesOwned = Mathf.Max(0, _missilesOwned + amount);
        SaveNow();
    }

    /// <summary>
    /// Lägg till X Pulse Visor charges i lagret.
    /// </summary>
    public static void AddPulseVisor(int amount)
    {
        EnsureLoaded();
        _pulseVisorOwned = Mathf.Max(0, _pulseVisorOwned + amount);
        SaveNow();
    }

    /// <summary>
    /// Försök förbruka 1 Missile från lagret.
    /// Returnerar true om vi hade minst 1 och lyckades dra av den.
    /// </summary>
    public static bool TryConsumeMissile()
    {
        EnsureLoaded();
        if (_missilesOwned <= 0) return false;
        _missilesOwned -= 1;
        SaveNow();
        return true;
    }

    /// <summary>
    /// Försök förbruka 1 Pulse Visor charge från lagret.
    /// Returnerar true om vi hade minst 1 och lyckades dra av den.
    /// </summary>
    public static bool TryConsumePulseVisor()
    {
        EnsureLoaded();
        if (_pulseVisorOwned <= 0) return false;
        _pulseVisorOwned -= 1;
        SaveNow();
        return true;
    }
}
