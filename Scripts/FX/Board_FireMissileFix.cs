// KMines – Board missile FX fix
// Replace the missile-FX part in Board.FireMissileAt(...) with this:
//
//    // FX (static call)
//    MissileHitFX.Spawn(new Vector3(cx, 0f, cy));
//
// so it matches your static MissileHitFX.Spawn(...) signature.
