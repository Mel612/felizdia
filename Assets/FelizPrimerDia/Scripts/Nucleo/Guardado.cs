using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// =====================================================================
//  GUARDAR PARTIDA (estilo Resident Evil)
//  En cada piso hay una MÁQUINA DE ESCRIBIR junto a las escaleras.
//  Ahí eliges uno de los 3 espacios y se guarda todo: el día, la hora,
//  dónde estás parada, lo que aprendiste, los sellos y objetos del día,
//  la dificultad y el personaje (Valeria o Mateo).
//  Desde el menú de inicio: CONTINUAR (lo último que guardaste) o CARGAR PARTIDA.
//  Los archivos quedan en la carpeta de datos del juego (Application.persistentDataPath).
// =====================================================================
public static class Guardado
{
    public const int Espacios = 3;

    [Serializable]
    public class Datos
    {
        public int version = 1;
        public long ticks;               // cuándo se guardó (para saber cuál es el último)
        public string fecha;
        public int prota;
        public int nivel;
        public bool contrarreloj;
        public float tiempo;
        public int dia, muertes, escapes;
        public List<string> recuerdos = new List<string>();
        public List<string> delDia = new List<string>();
        public float hora;
        public int piso;
        public float x, y;
        public string evento = "";
        public int pisoApagon;
        public float horaSimulacro;
        public int simulacroEstado;
        public float simulacroLimite;
        public bool vistaHoy;
    }

    static string Ruta(int espacio)
    {
        return Path.Combine(Application.persistentDataPath, "feliz_primer_dia_" + espacio + ".json");
    }

    public static bool Existe(int espacio)
    {
        try { return File.Exists(Ruta(espacio)); } catch { return false; }
    }

    public static bool HayAlguno
    {
        get { for (int i = 1; i <= Espacios; i++) if (Existe(i)) return true; return false; }
    }

    public static Datos Leer(int espacio)
    {
        try
        {
            if (!Existe(espacio)) return null;
            return JsonUtility.FromJson<Datos>(File.ReadAllText(Ruta(espacio)));
        }
        catch (Exception e)
        {
            Debug.LogWarning("[Feliz Primer Día] No se pudo leer la partida " + espacio + ": " + e.Message);
            return null;
        }
    }

    // El espacio guardado más reciente (0 si no hay ninguno)
    public static int Ultimo()
    {
        int mejor = 0; long t = -1;
        for (int i = 1; i <= Espacios; i++)
        {
            Datos d = Leer(i);
            if (d != null && d.ticks > t) { t = d.ticks; mejor = i; }
        }
        return mejor;
    }

    public static string Resumen(Datos d)
    {
        if (d == null) return "<color=#777777>— vacío —</color>";
        string quien = d.prota == (int)Quien.Mateo ? "<color=#4cc9f0>Mateo</color>" : "<color=#c77dff>Valeria</color>";
        int h = Mathf.FloorToInt(d.hora), m = Mathf.FloorToInt((d.hora - h) * 60f);
        int nivel = Mathf.Clamp(d.nivel, 0, Dificultad.Nombres.Length - 1);
        return quien + "  ·  Día " + d.dia + "  ·  " + h.ToString("00") + ":" + m.ToString("00") + "  ·  " +
               Mapa.NombresPiso[Mathf.Clamp(d.piso, 0, Mapa.NombresPiso.Length - 1)] + "  ·  <color=" + Dificultad.Colores[nivel] + ">" +
               Dificultad.Nombres[nivel] + "</color>\n<size=20><color=#888888>      guardado el " + d.fecha + "</color></size>";
    }

    public static bool Guardar(int espacio)
    {
        Juego j = Juego.I;
        if (j == null) return false;
        Datos d = new Datos();
        d.ticks = DateTime.Now.Ticks;
        d.fecha = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
        d.prota = (int)Protagonista.Actual;
        d.nivel = (int)Dificultad.Actual;
        d.contrarreloj = Dificultad.Contrarreloj;
        d.tiempo = Dificultad.TiempoTotal;
        d.dia = Memoria.Dia; d.muertes = Memoria.Muertes; d.escapes = Memoria.Escapes;
        d.recuerdos.AddRange(Memoria.Recuerdos);
        d.delDia.AddRange(Memoria.DelDia);
        d.hora = j.Hora;
        d.piso = j.piso;
        Vector2 p = j.jugador != null ? (Vector2)j.jugador.transform.position : Mapa.LlegadaEscaleras(j.piso);
        d.x = p.x; d.y = p.y;
        d.evento = Partida.Evento;
        d.pisoApagon = Partida.PisoApagon;
        d.horaSimulacro = Partida.HoraSimulacro;
        d.simulacroEstado = Partida.SimulacroEstado;
        d.simulacroLimite = Partida.SimulacroLimite;
        d.vistaHoy = Partida.VistaHoy;
        try
        {
            File.WriteAllText(Ruta(espacio), JsonUtility.ToJson(d, true));
            return true;
        }
        catch (Exception e)
        {
            Debug.LogWarning("[Feliz Primer Día] No se pudo guardar: " + e.Message);
            return false;
        }
    }

    // Carga la partida: deja todo como estaba y abre el piso donde guardaste.
    public static bool Cargar(int espacio)
    {
        Datos d = Leer(espacio);
        if (d == null) return false;
        Memoria.Restaurar(d.recuerdos, d.delDia, d.dia, d.muertes, d.escapes);
        Protagonista.Actual = d.prota == (int)Quien.Mateo ? Quien.Mateo : Quien.Valeria;
        Dificultad.Actual = (Nivel)Mathf.Clamp(d.nivel, 0, 3);
        Dificultad.Contrarreloj = d.contrarreloj;
        Dificultad.TiempoTotal = d.tiempo;
        Dificultad.Elegida = true;

        Partida.Hora = Mathf.Max(d.hora, 0f);
        Partida.Evento = d.evento ?? "";
        Partida.PisoApagon = d.pisoApagon;
        Partida.HoraSimulacro = d.horaSimulacro;
        Partida.SimulacroEstado = d.simulacroEstado;
        Partida.SimulacroLimite = d.simulacroLimite;
        Partida.VistaHoy = d.vistaHoy;
        Partida.PepinoAturdido = false;
        Partida.IntroPendiente = false;
        Partida.CuartoDesdeMenu = false;
        Partida.ResultadoCuarto = 0;
        Partida.PosGuardada = new Vector2(d.x, d.y);
        Partida.Llegada = "guardado";
        Partida.Cargar(Partida.EscenasPiso[Mathf.Clamp(d.piso, 0, Partida.EscenasPiso.Length - 1)]);
        return true;
    }

    // ---------- La máquina de escribir (se dibuja en cada piso) ----------
    public static Vector2 PosMaquina(int piso) { return Mapa.Origen(piso) + new Vector2(-10.7f, -2.75f); }

    public static void Montar(Transform t, int piso)
    {
        Horno.Empezar("Guardado");
        try
        {
            Vector2 pos = PosMaquina(piso);
            Util.Objeto("Máquina de escribir (guardar partida)", t, Arte.MaquinaEscribir(), pos, new Vector2(1.4f, 0.4f));
            if (Horno.Horneando) return;
            Interactuable it = Interactuable.Crear(t, pos + new Vector2(0f, 0.9f), "Usar la máquina de escribir (guardar)", delegate
            {
                Juego j = Juego.I;
                if (j != null && !escribiendo) j.StartCoroutine(Escribir(j));
            }, 1.3f);
            it.disponible = delegate { return !escribiendo; };
        }
        finally { Horno.Terminar(); }
    }

    static bool escribiendo;

    static IEnumerator Escribir(Juego j)
    {
        escribiendo = true;
        j.Bloquear("guardar");
        try
        {
            string[] ops = new string[Espacios + 1];
            for (int i = 1; i <= Espacios; i++)
            {
                Datos d = Leer(i);
                ops[i - 1] = "Espacio " + i + ":  " + (d == null ? "vacío" : (d.prota == (int)Quien.Mateo ? "Mateo" : "Valeria") + " · Día " + d.dia + " · " + d.fecha);
            }
            ops[Espacios] = "No guardar";
            Resultado<int> r = new Resultado<int>();
            yield return j.dialogo.ElegirCo("Máquina de escribir",
                "Una máquina de escribir vieja con una hoja en blanco. ¿Guardar lo que has vivido hasta ahora?", ops, r);
            if (r.valor >= Espacios) yield break;
            int espacio = r.valor + 1;
            for (int k = 0; k < 6; k++)
            {
                Sonido.Efecto("click", 0.7f, UnityEngine.Random.Range(0.8f, 1.3f));
                yield return new WaitForSeconds(0.09f);
            }
            Sonido.Efecto("sello", 0.5f, 1.4f);
            if (Guardar(espacio))
            {
                try { PlayerPrefs.SetInt("fpd_ultimo_espacio", espacio); } catch { }
                j.ui.Toast("PARTIDA GUARDADA en el espacio " + espacio + "  ·  Día " + Memoria.Dia + "  ·  " + j.HoraTexto, 4f);
            }
            else j.ui.Toast("No se pudo guardar la partida.", 4f);
        }
        finally
        {
            j.Desbloquear("guardar");
            escribiendo = false;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void AlIniciar() { escribiendo = false; }
}
