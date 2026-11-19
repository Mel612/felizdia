using UnityEngine;

// =====================================================================
//  CON QUIÉN SE JUEGA: Valeria (historia principal) o Mateo (nivel extra).
//  Mateo se desbloquea al ver un final de Valeria que rompa el bucle
//  (bueno, verdadero, el de la ñatita u oscuro). Se guarda en la computadora.
// =====================================================================
public enum Quien { Valeria, Mateo }

public static class Protagonista
{
    public static Quien Actual = Quien.Valeria;
    public static bool EsMateo { get { return Actual == Quien.Mateo; } }
    public static string Nombre { get { return EsMateo ? "Mateo" : "Valeria"; } }
    public static SpritesPersonaje Sprites { get { return EsMateo ? Arte.Mateo() : Arte.Valeria(); } }

    public static bool MateoDesbloqueado
    {
        get { try { return PlayerPrefs.GetInt("fpd_mateo", 0) == 1; } catch { return false; } }
    }

    // Devuelve true si se desbloqueó recién
    public static bool DesbloquearMateo()
    {
        if (MateoDesbloqueado) return false;
        try { PlayerPrefs.SetInt("fpd_mateo", 1); PlayerPrefs.Save(); } catch { }
        return true;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void AlIniciar() { Actual = Quien.Valeria; }
}
