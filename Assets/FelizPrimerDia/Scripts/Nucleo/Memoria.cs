using System.Collections.Generic;
using UnityEngine;

// La memoria de Valeria.
//  - "Recuerdos": lo que aprende. SOBREVIVE al loop (como en la película).
//  - "Del día": sellos, pruebas, objetos. Se BORRA cada vez que el día vuelve a empezar.
public static class Memoria
{
    static readonly HashSet<string> recuerdos = new HashSet<string>();
    static readonly List<string> ordenRecuerdos = new List<string>();
    static readonly HashSet<string> delDia = new HashSet<string>();

    public static int Dia = 1;
    public static int Muertes = 0;
    public static int Escapes = 0;      // persecuciones superadas (el Pepino aprende)

    // ---------- Recuerdos (persisten entre loops) ----------
    public static bool Sabe(string id) { return recuerdos.Contains(id); }

    public static bool Aprender(string id)
    {
        if (string.IsNullOrEmpty(id) || !recuerdos.Add(id)) return false;
        ordenRecuerdos.Add(id);
        return true;
    }

    public static IList<string> Recuerdos { get { return ordenRecuerdos; } }

    // ---------- Objetos del día (se pierden al reiniciar) ----------
    public static bool Tiene(string id) { return delDia.Contains(id); }
    public static bool Obtener(string id) { return !string.IsNullOrEmpty(id) && delDia.Add(id); }
    public static void Quitar(string id) { delDia.Remove(id); }

    public static int Cuantos(params string[] ids)
    {
        int n = 0;
        foreach (string id in ids) if (delDia.Contains(id)) n++;
        return n;
    }

    public static int CuantosRecuerdos(params string[] ids)
    {
        int n = 0;
        foreach (string id in ids) if (recuerdos.Contains(id)) n++;
        return n;
    }

    public static IEnumerable<string> DelDia { get { return delDia; } }

    // Para cargar una partida guardada
    public static void Restaurar(IList<string> listaRecuerdos, IList<string> listaDelDia, int dia, int muertes, int escapes)
    {
        BorrarTodo();
        if (listaRecuerdos != null) foreach (string id in listaRecuerdos) Aprender(id);
        if (listaDelDia != null) foreach (string id in listaDelDia) Obtener(id);
        Dia = Mathf.Max(1, dia);
        Muertes = muertes;
        Escapes = escapes;
    }

    public static void NuevoDia()
    {
        delDia.Clear();
        Dia++;
    }

    public static void BorrarTodo()
    {
        recuerdos.Clear();
        ordenRecuerdos.Clear();
        delDia.Clear();
        Dia = 1;
        Muertes = 0;
        Escapes = 0;
    }

    // Limpia todo al darle Play (aunque esté desactivado el "Domain Reload").
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void AlIniciar() { BorrarTodo(); }
}

// Identificadores usados en todo el juego (para no escribir mal los textos).
public static class Ids
{
    public const string SelloMedicina = "sello_medicina";
    public const string SelloDerecho = "sello_derecho";
    public const string SelloPeriodismo = "sello_periodismo";
    public const string SelloSistemas = "sello_sistemas";
    public static readonly string[] Sellos = { SelloMedicina, SelloDerecho, SelloPeriodismo, SelloSistemas };

    public const string PruebaHistoria = "prueba_historia";
    public const string PruebaExpediente = "prueba_expediente";
    public const string PruebaGrabacion = "prueba_grabacion";
    public const string PruebaRegistros = "prueba_registros";
    public static readonly string[] Pruebas = { PruebaHistoria, PruebaExpediente, PruebaGrabacion, PruebaRegistros };

    public static readonly string[] CharlasCamila = { "camila_1", "camila_2", "camila_3", "camila_4" };

    // Grabaciones de Camila (coleccionables, se recuerdan entre bucles)
    public static readonly string[] Audios = { "audio_1", "audio_2", "audio_3", "audio_4", "audio_5", "audio_6", "audio_7", "audio_8" };

    // Ofrenda para la ñatita (objetos del día)
    public static readonly string[] Ofrenda = { "ofrenda_flores", "ofrenda_coca", "ofrenda_vela" };
}
