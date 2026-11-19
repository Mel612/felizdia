using UnityEngine;
using UnityEngine.SceneManagement;

public enum TipoEscena { Piso, Persecucion, Bienvenida, Cuarto, Galeria, Menu, Pantallas }

// =====================================================================
//  PARTIDA: lo que se conserva al cambiar de escena durante el mismo día
//  (la hora, por dónde llegas al piso, la persecución pendiente...).
//  Lo aprendido entre días está en Memoria.
//
//  Escenas del juego (se crean con el menú Feliz Primer Día ▸ Construir escenas):
//    0_PlantaBaja · 1_Piso1_Derecho · 2_Piso2_Periodismo · 3_Piso3_Medicina
//    4_Piso4_Sistemas · 5_Persecuciones · 6_Bienvenida
// =====================================================================
public static class Partida
{
    public static readonly string[] EscenasPiso =
    {
        "0_PlantaBaja", "1_Piso1_Derecho", "2_Piso2_Periodismo", "3_Piso3_Medicina", "4_Piso4_Sistemas"
    };
    public const string EscenaPersecuciones = "5_Persecuciones";
    public const string EscenaBienvenida = "6_Bienvenida";
    public const string EscenaCuarto = "7_CuartoSecreto";
    public const string EscenaCinematicas = "8_Cinematicas";      // galería (para ver y probar)
    public const string EscenaRecuerdos = "9_Recuerdos";          // galería (para ver y probar)
    public const string EscenaMenu = "00_Menu";                   // pantalla de inicio (la primera del juego)
    public const string EscenaPantallas = "10_Pantallas";         // galería: todas las pantallas (menú, pausa, mapa, opciones...)

    public static string[] TodasLasEscenas
    {
        get
        {
            string[] t = new string[EscenasPiso.Length + 7];
            EscenasPiso.CopyTo(t, 0);
            t[EscenasPiso.Length] = EscenaPersecuciones;
            t[EscenasPiso.Length + 1] = EscenaBienvenida;
            t[EscenasPiso.Length + 2] = EscenaCuarto;
            t[EscenasPiso.Length + 3] = EscenaCinematicas;
            t[EscenasPiso.Length + 4] = EscenaRecuerdos;
            t[EscenasPiso.Length + 5] = EscenaMenu;
            t[EscenasPiso.Length + 6] = EscenaPantallas;
            return t;
        }
    }

    public static float Hora = -1f;              // -1 = el día recién empieza
    public static string Llegada = "inicio";     // inicio | escaleras | ascensor | escape | cuarto | guardado
    public static Vector2 PosGuardada;           // dónde estabas al guardar (Llegada = "guardado")
    public static int PisoPersecucion = 1;       // piso de la persecución pendiente
    public static bool PepinoAturdido;           // acabas de escapar: el Pepino tarda en volver

    // ---------- Eventos al azar (uno por día, desde el día 2) ----------
    //  "apagon": un piso sin luz · "simulacro": alarma, hay que bajar a la Planta Baja
    //  "ascensor": el ascensor no funciona · "" = día normal
    public static string Evento = "";
    public static int PisoApagon = 1;
    public static float HoraSimulacro = 11f;
    public static int SimulacroEstado;           // 0 esperando · 1 sonando · 2 evacuaste
    public static float SimulacroLimite;         // hora del juego en que se acaba el tiempo
    public static bool VistaHoy;                 // el Pepino te vio hoy (logro "Fantasma")

    // ---------- El cuarto secreto ----------
    public static bool IntroPendiente;           // al recargar la Planta Baja, mostrar la cinemática de inicio
    public static int NocheCuarto = 1;
    public static bool CuartoDesdeMenu;          // se entró desde el menú de inicio (al salir vuelve al menú)
    public static int ResultadoCuarto;           // 0 nada · 1 ganaste · 2 perdiste (se muestra al volver a la Planta Baja)

    public static bool DiaNuevo { get { return Hora < 0f; } }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void AlIniciar() { NuevoDia(); }

    public static int Lanzables;
    public static float Bateria = 100f;          // batería del celular (el flash gasta)                 // botellas/latas que lleva Valeria (para distraer al Pepino)

    public static void NuevoDia()
    {
        Hora = -1f;
        Lanzables = 0;
        Bateria = 100f;
        Celular.NuevoDia();
        Llegada = "inicio";
        PisoPersecucion = 1;
        PepinoAturdido = false;
        SimulacroEstado = 0;
        VistaHoy = false;
        Evento = "";
        if (Memoria.Dia >= 2)
        {
            int e = Random.Range(0, 5);              // 2 de cada 5 días son normales
            if (e == 1) { Evento = "apagon"; PisoApagon = Random.Range(1, EscenasPiso.Length); }
            else if (e == 2) { Evento = "simulacro"; HoraSimulacro = Random.Range(10f, 15.5f); }
            else if (e == 3) Evento = "ascensor";
        }
    }

    public static string TextoEvento()
    {
        if (Evento == "apagon") return "Hoy hay un CORTE DE LUZ en el " + Mapa.NombresPiso[PisoApagon] + ".";
        if (Evento == "simulacro") return "Hoy hay SIMULACRO DE EVACUACIÓN. Cuando suene la alarma, baja a la Planta Baja.";
        if (Evento == "ascensor") return "Hoy el ASCENSOR está en mantenimiento.";
        return "";
    }

    public static bool PuedeCargar(string escena) { return Application.CanStreamedLevelBeLoaded(escena); }

    public static void Cargar(string escena)
    {
        if (!PuedeCargar(escena))
        {
            Debug.LogError("[Feliz Primer Día] No encuentro la escena \"" + escena + "\". " +
                           "Usa el menú Feliz Primer Día ▸ Construir escenas (las agrega a Build Settings).");
            if (Juego.I != null && Juego.I.ui != null)
                Juego.I.ui.Toast("Falta la escena " + escena + ". Usa el menú Feliz Primer Día ▸ Construir escenas.", 6f);
            return;
        }
        Horno.Olvidar();
        SceneManager.LoadScene(escena);
    }

    // Guarda la hora actual y va a otro piso.
    public static void IrAPiso(int piso, string llegada)
    {
        if (Juego.I != null) Hora = Juego.I.Hora;
        Llegada = llegada;
        Cargar(EscenasPiso[Mathf.Clamp(piso, 0, EscenasPiso.Length - 1)]);
    }

    public static void IrAPersecucion(int piso)
    {
        if (Juego.I != null) Hora = Juego.I.Hora;
        PisoPersecucion = piso;
        Cargar(EscenaPersecuciones);
    }

    public static void IrACuarto(int noche, bool desdeMenu)
    {
        if (Juego.I != null && !desdeMenu) Hora = Juego.I.Hora;
        NocheCuarto = noche;
        CuartoDesdeMenu = desdeMenu;
        ResultadoCuarto = 0;
        Cargar(EscenaCuarto);
    }

    public static void IrABienvenida()
    {
        if (Juego.I != null) Hora = Juego.I.Hora;
        Cargar(EscenaBienvenida);
    }

    // Vuelve a la pantalla de inicio (si la escena del menú existe; si no, a la Planta Baja)
    public static void IrAlMenu()
    {
        NuevoDia();
        Cargar(PuedeCargar(EscenaMenu) ? EscenaMenu : EscenasPiso[0]);
    }

    // ¿Toca mostrar el menú? (partida nueva sin empezar)
    public static bool EnMenu { get { return Memoria.Dia == 1 && !Dificultad.Elegida; } }

    // Empieza el día otra vez desde la Planta Baja.
    public static void ReiniciarDia()
    {
        NuevoDia();
        Cargar(EscenasPiso[0]);
    }
}
