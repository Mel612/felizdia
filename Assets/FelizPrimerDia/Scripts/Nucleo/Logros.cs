using System.Collections.Generic;
using System.Text;
using UnityEngine;

// =====================================================================
//  LOGROS: se guardan en la computadora (PlayerPrefs), así que se quedan
//  aunque cierres el juego. Se ven en la última página del cuaderno (Tab).
// =====================================================================
public static class Logros
{
    public struct Logro
    {
        public string id, titulo, texto;
        public Logro(string i, string t, string x) { id = i; titulo = t; texto = x; }
    }

    public static readonly List<Logro> Lista = new List<Logro>
    {
        new Logro("primer_bucle", "Bienvenida al bucle", "Morir por primera vez."),
        new Logro("seis_muertes", "Coleccionista de muertes", "Morir de las 6 formas distintas."),
        new Logro("carreras", "Orientación vocacional", "Conocer las 14 carreras."),
        new Logro("sellos", "Cachimbo modelo", "Juntar los 4 sellos en un mismo día."),
        new Logro("pruebas", "Detective", "Juntar las 4 pruebas en un mismo día."),
        new Logro("fantasma", "Fantasma", "Llegar a las 18:00 con los 4 sellos sin que el Pepino te vea."),
        new Logro("escapista", "Escapista", "Escapar de 3 persecuciones."),
        new Logro("audios", "Te escucho, Camila", "Encontrar las 8 grabaciones de Camila."),
        new Logro("natita", "Devota de la ñatita", "Llevarle la ofrenda completa a la ñatita."),
        new Logro("evacuacion", "Simulacro aprobado", "Evacuar a tiempo durante un simulacro."),
        new Logro("indumentaria", "Bien vestida", "Conseguir las 4 indumentarias en un mismo día."),
        new Logro("mateo", "Alguien estaba mirando", "Desbloquear a Mateo."),
        new Logro("aviso", "Siempre fui yo", "Escribir la advertencia en el directorio (Mateo)."),
        new Logro("mateo_envivo", "En vivo", "Final de Mateo: proyectar la verdad."),
        new Logro("mateo_testigo", "Testigo", "El final verdadero de Mateo."),
        new Logro("mateo_espectador", "Solo un espectador", "Final de Mateo: quedarse mirando."),
        new Logro("mateo_borrado", "Borrado", "Final de Mateo: aceptar la plata de Sebastián."),
        new Logro("cuarto", "Turno de noche", "Sobrevivir la Noche 1 en el cuarto secreto."),
        new Logro("cuarto3", "Cinco disfraces", "Sobrevivir la Noche 3 en el cuarto secreto."),
        new Logro("final_bueno", "Final bueno", "Desenmascarar a Camila frente a todos."),
        new Logro("final_verdadero", "Final verdadero", "Perdonar."),
        new Logro("final_natita", "Las dos despiertan", "El final de la ñatita."),
        new Logro("final_oscuro", "La nueva Pepino", "El final oscuro."),
        new Logro("final_malo", "Bucle eterno", "Acusar a la persona equivocada."),
        new Logro("final_pesadilla", "El último bucle", "Quedarte sin bucles en Pesadilla."),
        new Logro("pesadilla", "Sobreviviente", "Romper el bucle en dificultad Pesadilla."),
        new Logro("contrarreloj", "Contra el reloj", "Romper el bucle en modo Contrarreloj."),
    };

    static string Clave(string id) { return "fpd_logro_" + id; }

    public static bool Tiene(string id)
    {
        try { return PlayerPrefs.GetInt(Clave(id), 0) == 1; } catch { return false; }
    }

    public static int Conseguidos
    {
        get { int n = 0; foreach (Logro l in Lista) if (Tiene(l.id)) n++; return n; }
    }

    public static void Desbloquear(string id)
    {
        if (Tiene(id)) return;
        try { PlayerPrefs.SetInt(Clave(id), 1); PlayerPrefs.Save(); } catch { return; }
        foreach (Logro l in Lista)
            if (l.id == id && Juego.I != null && Juego.I.ui != null)
            {
                Juego.I.ui.Logro(l.titulo);
                break;
            }
    }

    // Cada forma de morir se guarda para el logro "Coleccionista de muertes".
    public static void RegistrarMuerte(TipoMuerte tipo)
    {
        Desbloquear("primer_bucle");
        try { PlayerPrefs.SetInt("fpd_muerte_" + (int)tipo, 1); } catch { }
        int n = 0;
        foreach (TipoMuerte t in System.Enum.GetValues(typeof(TipoMuerte)))
        {
            try { if (PlayerPrefs.GetInt("fpd_muerte_" + (int)t, 0) == 1) n++; } catch { }
        }
        if (n >= System.Enum.GetValues(typeof(TipoMuerte)).Length) Desbloquear("seis_muertes");
    }

    // Revisa los logros que dependen del estado del día (se llama cada tanto).
    public static void Revisar()
    {
        if (Memoria.Cuantos(Ids.Sellos) >= 4) Desbloquear("sellos");
        if (Memoria.Cuantos(Ids.Pruebas) >= 4) Desbloquear("pruebas");
        if (Historia.NumCarreras() >= Historia.TotalCarreras) Desbloquear("carreras");
        if (Memoria.CuantosRecuerdos(Ids.Audios) >= Ids.Audios.Length) Desbloquear("audios");
    }

    public static string Texto()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("<b>LOGROS  ").Append(Conseguidos).Append("/").Append(Lista.Count).Append("</b>\n\n");
        foreach (Logro l in Lista)
        {
            bool ok = Tiene(l.id);
            sb.Append(ok ? "<color=#2d6a4f>■ <b>" + l.titulo + "</b>" : "<color=#9a8a75>□ <b>???</b>")
              .Append(" — ").Append(l.texto).Append("</color>\n");
        }
        string rec = "";
        for (int i = 0; i < 4; i++)
        {
            float r = 0f;
            try { r = PlayerPrefs.GetFloat("fpd_record_" + i, 0f); } catch { }
            if (r > 0f) rec += "   " + Dificultad.Nombres[i] + ": " + Dificultad.Formato(r);
        }
        if (rec != "") sb.Append("\n<b>Récords contrarreloj:</b>").Append(rec).Append("\n");
        return sb.ToString();
    }
}
