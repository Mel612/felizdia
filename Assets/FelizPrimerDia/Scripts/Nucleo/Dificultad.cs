using UnityEngine;

public enum Nivel { Cachimbo, Universitario, Titulacion, Pesadilla }

// Nivel de los retos de cada carrera (lo que te piden los guías en cada facultad).
public enum NivelReto { Basico, Intermedio, Avanzado }

// =====================================================================
//  DIFICULTAD Y MODO DE JUEGO (se eligen al empezar la partida)
//    Cachimbo      fácil: Pepino lento, más tiempo, pistas extra y objetivo marcado
//    Universitario normal
//    Titulación    difícil: Pepino rápido y atento, menos tiempo
//    Pesadilla     solo 5 bucles y 6 Pepinos en la fiesta
//  Contrarreloj: cuenta el tiempo real de toda la partida y guarda tu récord.
// =====================================================================
public static class Dificultad
{
    public static Nivel Actual = Nivel.Universitario;
    public static bool Contrarreloj;
    public static bool Elegida;          // ya se eligió en esta partida
    public static float TiempoTotal;     // segundos reales jugados (contrarreloj)

    public static readonly string[] Nombres = { "CACHIMBO", "UNIVERSITARIO", "TITULACIÓN", "PESADILLA" };
    public static readonly string[] Colores = { "#7bd389", "#ffd166", "#f4a261", "#ff4d4d" };
    public static readonly string[] Descripciones =
    {
        "Fácil. El Pepino es lento y no aprende. Más tiempo en los minijuegos.\nUna pista extra cada vez que mueres y el pasaporte te dice a dónde ir.",
        "Normal. Como fue pensado el juego. El Pepino aprende de tus muertes.",
        "Difícil. El Pepino es más rápido, ve más lejos y aprende más rápido.\nMenos tiempo en los minijuegos.",
        "Solo tienes 5 BUCLES para romper el día. En la fiesta hay 6 Pepinos.\nEl Pepino es rapidísimo. Si se acaban los bucles... final especial."
    };

    public static string Nombre { get { return Nombres[(int)Actual]; } }
    public static string ColorHex { get { return Colores[(int)Actual]; } }

    // ---------- Retos de las carreras ----------
    // Cada carrera tiene 3 niveles de reto según la dificultad elegida:
    //   Cachimbo = BÁSICO (lógica, diagramas de flujo y para qué sirve cada cosa)
    //   Universitario = INTERMEDIO
    //   Titulación y Pesadilla = AVANZADO
    public static NivelReto Reto
    {
        get
        {
            if (Actual == Nivel.Cachimbo) return NivelReto.Basico;
            if (Actual == Nivel.Universitario) return NivelReto.Intermedio;
            return NivelReto.Avanzado;
        }
    }

    public static readonly string[] NombresReto = { "BÁSICO", "INTERMEDIO", "AVANZADO" };
    public static readonly string[] ColoresReto = { "#7bd389", "#ffd166", "#ff4d4d" };
    public static string NombreReto { get { return NombresReto[(int)Reto]; } }
    public static string ColorReto { get { return ColoresReto[(int)Reto]; } }

    // Devuelve el valor que corresponde al nivel de reto actual.
    public static T SegunReto<T>(T basico, T intermedio, T avanzado)
    {
        switch (Reto)
        {
            case NivelReto.Basico: return basico;
            case NivelReto.Intermedio: return intermedio;
            default: return avanzado;
        }
    }

    // ---------- Ajustes ----------
    public static float VelPepino { get { return new[] { 0.8f, 1f, 1.15f, 1.25f }[(int)Actual]; } }
    public static float VisionPepino { get { return new[] { 0.8f, 1f, 1.2f, 1.25f }[(int)Actual]; } }
    public static float VelPersecucion { get { return new[] { 0.88f, 1f, 1.07f, 1.1f }[(int)Actual]; } }
    // > 1 = más tiempo para reaccionar en minijuegos y eventos
    public static float FactorTiempo { get { return new[] { 1.4f, 1f, 0.8f, 0.75f }[(int)Actual]; } }
    public static int PepinosFiesta { get { return Actual == Nivel.Pesadilla ? 6 : 4; } }
    public static int BuclesMax { get { return Actual == Nivel.Pesadilla ? 5 : 0; } }    // 0 = sin límite
    public static bool PistaExtra { get { return Actual == Nivel.Cachimbo; } }
    public static bool MarcarObjetivo { get { return Actual == Nivel.Cachimbo; } }
    // Cuánto aprende el Pepino de cada muerte (0 = nada)
    public static float Escalada { get { return new[] { 0f, 1f, 1.5f, 2f }[(int)Actual]; } }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void AlIniciar() { Reiniciar(); }

    public static void Reiniciar()
    {
        Elegida = false;
        Contrarreloj = false;
        TiempoTotal = 0f;
    }

    // Ajusta un minijuego según la dificultad (se llama al empezar cada uno).
    public static void Ajustar(Minijuego mj)
    {
        float f = FactorTiempo;
        if (Mathf.Approximately(f, 1f)) return;
        MJOrdenar o = mj as MJOrdenar;
        if (o != null && o.tiempoLimite > 0f) o.tiempoLimite *= f;
        MJPreguntas p = mj as MJPreguntas;
        if (p != null && p.tiempoPorRonda > 0f) p.tiempoPorRonda *= f;
        MJSecuencia s = mj as MJSecuencia;
        if (s != null) s.ventana *= f;
        MJRitmo r = mj as MJRitmo;
        if (r != null) { r.velocidad /= Mathf.Sqrt(f); r.anchoZona = Mathf.Clamp(r.anchoZona * f, 0.08f, 0.3f); }
        MJRespiracion b = mj as MJRespiracion;
        if (b != null) b.aireMaximo *= f;
    }

    // ---------- Contrarreloj ----------
    public static string Formato(float segundos)
    {
        int s = Mathf.FloorToInt(segundos);
        return (s / 60).ToString("00") + ":" + (s % 60).ToString("00");
    }

    static string ClaveRecord { get { return "fpd_record_" + (int)Actual; } }

    public static float Record
    {
        get { try { return PlayerPrefs.GetFloat(ClaveRecord, 0f); } catch { return 0f; } }
    }

    // Guarda el tiempo si es récord. Devuelve true si lo es.
    public static bool GuardarRecord()
    {
        float r = Record;
        if (r > 0f && r <= TiempoTotal) return false;
        try { PlayerPrefs.SetFloat(ClaveRecord, TiempoTotal); PlayerPrefs.Save(); } catch { }
        return true;
    }
}
