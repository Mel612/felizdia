using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// =====================================================================
//  EL CELULAR DE VALERIA (o de Mateo)
//  Durante el día llegan mensajes del grupo de la promo: chismes, pedidos
//  de los guías (pistas de las misiones) y mensajes de un número
//  desconocido... Cambian según el bucle (Día 2, 3...).
//  Aparece una notificación a la derecha y con C abres el chat completo.
//  Para agregar mensajes: edita la lista "Lista" (hora, quién, texto, condición).
// =====================================================================
public class Celular : MonoBehaviour
{
    class Mensaje
    {
        public string id;
        public float hora;
        public string de;
        public string color;
        public string texto;
        public Func<bool> cuando;
    }

    const string Grupo = "PROMO CACHIMBOS 2026";
    const string GrupoMateo = "LAB DE SISTEMAS";
    const string Desconocido = "#ff4d4d";

    static Mensaje M(string id, float hora, string de, string color, string texto, Func<bool> cuando = null)
    { return new Mensaje { id = id, hora = hora, de = de, color = color, texto = texto, cuando = cuando }; }

    static readonly List<Mensaje> Valeria = new List<Mensaje>
    {
        M("v_hola", 7.0f, "Camila (delegada)", "#ffd166", "¡Bienvenidos, cachimbos! Hoy es la Bienvenida a las 18:00 en el Auditorio. Traigan su Pasaporte con los 4 sellos."),
        M("v_otra", 7.1f, "Número desconocido", Desconocido, "¿Otra vez tú?", () => Memoria.Dia >= 2),
        M("v_derecho", 7.4f, "Diego", "#8ecae6", "alguien sabe dónde queda Derecho?? estoy perdido jaja"),
        M("v_derecho2", 7.6f, "Andrea", "#f4a261", "Piso 1, Diego. Lee el pasaporte."),
        M("v_cafe", 8.5f, "Joaquín (Radio U)", "#7bd389", "Estoy en vivo desde las 6 de la mañana. Le debo una a quien me traiga un café de la Cafetería."),
        M("v_camila3", 9.0f, "Camila (privado)", "#ffd166", "...perdón, ¿nos conocemos? Siento que ya te escribí esto antes.", () => Memoria.Dia >= 3),
        M("v_codigo", 9.5f, "Doña Rosa (Biblioteca)", "#c9ada7", "Quien se llevó el Código Penal a la Cafetería, por favor devuélvalo. Gracias."),
        M("v_natita", 10.0f, "Diego", "#8ecae6", "dicen que en Odontología hay una ñatita que cumple deseos si le llevas flores, coca y una vela. creepy", () => Memoria.Dia >= 2),
        M("v_usb", 10.3f, "Iván (Sistemas)", "#c77dff", "Perdí mi USB en Diseño Gráfico (Piso 2). Tiene el respaldo de los servidores. Recompensa."),
        M("v_pepino", 11.0f, "Diego", "#8ecae6", "vieron al tipo disfrazado de pepino?? qué miedo jaja"),
        M("v_pepino2", 11.2f, "Camila (delegada)", "#ffd166", "Es parte de la Bienvenida. No se asusten."),
        M("v_bata", 12.5f, "Ariel (Medicina)", "#2a9d8f", "Sin bata y barbijo nadie entra a Simulación. Bioseguridad, cachimbos."),
        M("v_feliz", 13.5f, "Número desconocido", Desconocido, "Feliz primer día."),
        M("v_casillero", 14.0f, "Número desconocido", Desconocido, "Ayer te escondiste en un casillero. ¿Hoy también?", () => Memoria.Muertes >= 2),
        M("v_mochila", 15.0f, "Andrea", "#f4a261", "¿alguien vio mi mochila? la dejé en el Patio y ahora no está"),
        M("v_90", 16.5f, "Camila (delegada)", "#ffd166", "Faltan 90 minutos para la Bienvenida. Sin los 4 sellos no entran."),
        M("v_auditorio", 17.6f, "Número desconocido", Desconocido, "Te veo en el auditorio."),
    };

    static readonly List<Mensaje> Mateo = new List<Mensaje>
    {
        M("m_vargas", 7.0f, "Ing. Vargas", "#c77dff", "Mateo, pasa por mi oficina en el Piso 4. Es sobre las cámaras viejas."),
        M("m_radio", 9.0f, "Joaquín (Radio U)", "#7bd389", "Si tienes algo para la radio, pásalo. Transmitimos todo en vivo."),
        M("m_deja", 11.0f, "Número desconocido", "#ffd166", "Deja de buscar esos videos, cachimbo."),
        M("m_directorio", 14.0f, "Ing. Vargas", "#c77dff", "El directorio del Ingreso todavía acepta avisos. Por si alguien quiere avisar algo."),
        M("m_cabina", 17.0f, "Sebastián", "#ffd166", "Nos vemos en la cabina."),
    };

    // Lo que ya llegó hoy (se borra al empezar un día nuevo)
    static readonly HashSet<string> entregados = new HashSet<string>();
    static readonly List<string> historial = new List<string>();

    public static void NuevoDia() { entregados.Clear(); historial.Clear(); }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void AlIniciar() { NuevoDia(); }

    static Celular actual;

    // Un mensaje que llega ahora mismo (lo usan los sustos al azar)
    public static void Forzar(string de, string texto)
    {
        if (actual == null) return;
        Mensaje m = M("f_" + historial.Count + "_" + Time.frameCount, actual.juego.Hora, de, Desconocido, texto);
        actual.Mostrar(m);
        actual.tEspera = 6f;
        if (actual.abierto) actual.Dibujar();
    }

    void OnDestroy() { if (actual == this) actual = null; if (abierto) HayUnoAbierto = false; }

    Juego juego;
    RectTransform noti, pantalla;
    Text txtNoti, txtChat;
    float tNoti = -1f, tEspera = 2f, tRevisar;
    bool abierto;

    static List<Mensaje> Lista { get { return Protagonista.EsMateo ? Mateo : Valeria; } }
    static string NombreGrupo { get { return Protagonista.EsMateo ? GrupoMateo : Grupo; } }

    public static Celular Crear(Juego j)
    {
        if (j == null || j.ui == null) return null;
        GameObject go = new GameObject("Celular", typeof(RectTransform));
        go.transform.SetParent(j.ui.transform, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = rt.offsetMax = Vector2.zero;
        Celular c = go.AddComponent<Celular>();
        c.juego = j;
        actual = c;
        c.Armar();
        // Lo que "ya llegó" antes de entrar a este piso va directo al historial, sin notificación
        foreach (Mensaje m in Lista)
            if (!entregados.Contains(m.id) && m.hora < j.Hora - 0.4f && (m.cuando == null || m.cuando()))
                Entregar(m);
        return c;
    }

    static void Entregar(Mensaje m)
    {
        entregados.Add(m.id);
        historial.Add(Linea(m));
    }

    static string Linea(Mensaje m)
    {
        int h = Mathf.FloorToInt(m.hora), mi = Mathf.FloorToInt((m.hora - h) * 60f);
        return "<color=" + m.color + "><b>" + m.de + "</b></color>  <size=18><color=#777777>" + h.ToString("00") + ":" + mi.ToString("00") + "</color></size>\n" + m.texto;
    }

    void Armar()
    {
        Vector2 cero = Vector2.zero;
        // Notificación (a la derecha, entra deslizándose)
        noti = Util.PanelArte(transform, "Notificación", new Color(0.07f, 0.09f, 0.08f, 0.95f), Util.Hex("#25d366"),
                              new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-560, -20), new Vector2(-20, 170));
        txtNoti = Util.TextoUI(noti, "", 24, Color.white, TextAnchor.UpperLeft, cero, Vector2.one, new Vector2(18, 14), new Vector2(-14, -12));
        noti.gameObject.SetActive(false);

        // Chat completo (C)
        pantalla = Util.PanelArteCentro(transform, "Chat", new Color(0.05f, 0.07f, 0.06f, 0.97f), Util.Hex("#25d366"), new Vector2(0, 0), new Vector2(760, 900));
        RectTransform barra = Util.Panel(pantalla, "Barra", Util.Hex("#075e54"), new Vector2(0, 1), new Vector2(1, 1), new Vector2(6, -96), new Vector2(-6, -6));
        Util.TextoUI(barra, "<b>" + NombreGrupo + "</b>\n<size=20><color=#cfe9e1>" + (Protagonista.EsMateo ? "Mateo" : "Valeria") + " y otros</color></size>",
                     30, Color.white, TextAnchor.MiddleLeft, cero, Vector2.one, new Vector2(24, 0), new Vector2(-10, 0));
        txtChat = Util.TextoUI(pantalla, "", 24, Color.white, TextAnchor.LowerLeft, cero, Vector2.one, new Vector2(26, 64), new Vector2(-20, -110));
        txtChat.verticalOverflow = VerticalWrapMode.Truncate;
        Util.TextoUI(pantalla, "C o Esc: cerrar   ·   el tiempo sigue corriendo...", 20, new Color(1, 1, 1, 0.5f), TextAnchor.LowerCenter, cero, new Vector2(1, 0), new Vector2(0, 16), new Vector2(0, 54));
        pantalla.gameObject.SetActive(false);
    }

    void Mostrar(Mensaje m)
    {
        Entregar(m);
        txtNoti.text = "<size=18><color=#25d366>" + NombreGrupo + "  ·  ahora</color></size>\n" + Linea(m);
        noti.gameObject.SetActive(true);
        tNoti = 0f;
        Sonido.Efecto("blip", 0.5f, 1.5f);
        Sonido.Efecto("zumbido", 0.25f, 1.6f);     // vibra
    }

    void Update()
    {
        if (juego == null || juego.ui == null) return;
        bool puede = !Cine.Activa && !juego.ui.PantallaVisible && juego.ui.HudVisible;

        // Abrir / cerrar el chat
        if (abierto && (Entrada.Bajada(Tecla.Celular) || Entrada.Bajada(Tecla.Saltar) || !puede)) Cerrar();
        else if (!abierto && puede && !juego.Bloqueado && !MiniMapa.HayUnoAbierto && Entrada.Bajada(Tecla.Celular)) Abrir();

        // Notificación entrando y saliendo
        if (tNoti >= 0f)
        {
            tNoti += Time.deltaTime;
            float x = tNoti < 0.25f ? Mathf.Lerp(600f, 0f, tNoti / 0.25f) : tNoti > 5f ? Mathf.Lerp(0f, 600f, (tNoti - 5f) / 0.3f) : 0f;
            noti.anchoredPosition = new Vector2(x, 0f);
            noti.gameObject.SetActive(puede && !abierto);
            if (tNoti > 5.3f) { tNoti = -1f; noti.gameObject.SetActive(false); }
        }

        // ¿Llegó un mensaje nuevo?
        tEspera -= Time.deltaTime;
        tRevisar -= Time.deltaTime;
        if (tRevisar > 0f) return;
        tRevisar = 0.5f;
        if (!puede || tNoti >= 0f || tEspera > 0f) return;
        foreach (Mensaje m in Lista)
        {
            if (entregados.Contains(m.id) || m.hora > juego.Hora) continue;
            if (m.cuando != null && !m.cuando()) continue;
            Mostrar(m);
            tEspera = 6f;
            if (abierto) Dibujar();
            break;
        }
    }

    public static bool HayUnoAbierto;

    void Abrir()
    {
        abierto = true;
        HayUnoAbierto = true;
        Dibujar();
        pantalla.gameObject.SetActive(true);
        noti.gameObject.SetActive(false);
        Sonido.Efecto("click", 0.5f, 1.4f);
    }

    void Cerrar()
    {
        abierto = false;
        HayUnoAbierto = false;
        pantalla.gameObject.SetActive(false);
    }

    void Dibujar()
    {
        if (historial.Count == 0) { txtChat.text = "<color=#777777>Todavía no hay mensajes.</color>"; return; }
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        int desde = Mathf.Max(0, historial.Count - 8);
        for (int i = desde; i < historial.Count; i++) sb.Append(historial[i]).Append("\n\n");
        txtChat.text = sb.ToString().TrimEnd();
    }
}
