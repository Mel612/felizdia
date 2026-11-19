using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// =====================================================================
//  MAPA (estilo Resident Evil) — se abre con Q
//  Pantalla completa con el plano del piso: tu posición (punto celeste),
//  las salas de tus MISIONES en amarillo con su número y, a la derecha,
//  la lista de todo lo que te falta y en qué piso está.
//  Flechas arriba/abajo: ver otros pisos. Q, Esc o E: cerrar.
//  Mientras está abierto, el juego se pausa.
// =====================================================================
public class MiniMapa : MonoBehaviour
{
    public class Destino
    {
        public string texto;
        public string sala;     // id de la sala
        public int piso;
    }

    Juego juego;
    RectTransform pantalla, plano;
    Text txtTitulo, txtPisos, txtLista;
    Image yo, escaleras;
    readonly Dictionary<string, Image> salas = new Dictionary<string, Image>();
    readonly Dictionary<string, Text> numeros = new Dictionary<string, Text>();
    List<Destino> destinos = new List<Destino>();
    Rect limites;
    int pisoVisto;
    bool abierto, otroPiso;
    float t, escalaAntes = 1f;

    static readonly Color ColSala = new Color(0.16f, 0.27f, 0.42f, 1f);
    static readonly Color ColPasillo = new Color(0.32f, 0.45f, 0.6f, 1f);

    public bool Abierto { get { return abierto; } }

    public static MiniMapa Crear(Juego j)
    {
        if (j == null || j.mapa == null || j.ui == null) return null;
        GameObject go = new GameObject("Mapa (Q)", typeof(RectTransform));
        go.transform.SetParent(j.ui.transform, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = rt.offsetMax = Vector2.zero;
        MiniMapa m = go.AddComponent<MiniMapa>();
        m.juego = j;
        m.Construir();
        return m;
    }

    void Construir()
    {
        Vector2 cero = Vector2.zero, uno = Vector2.one;
        pantalla = Util.Panel(transform, "Pantalla del mapa", new Color(0.01f, 0.03f, 0.08f, 0.97f), cero, uno, cero, cero);
        // Cuadrícula de fondo (como el plano de Resident Evil)
        for (int i = 1; i < 24; i++)
            Util.Panel(pantalla, "línea", new Color(0.2f, 0.4f, 0.7f, 0.08f), new Vector2(i / 24f, 0f), new Vector2(i / 24f, 1f), new Vector2(-1, 0), new Vector2(1, 0));
        for (int i = 1; i < 14; i++)
            Util.Panel(pantalla, "línea", new Color(0.2f, 0.4f, 0.7f, 0.08f), new Vector2(0f, i / 14f), new Vector2(1f, i / 14f), new Vector2(0, -1), new Vector2(0, 1));

        txtTitulo = Util.TextoUI(pantalla, "", 56, Util.Hex("#e9f1ff"), TextAnchor.UpperLeft, new Vector2(0.04f, 0.86f), new Vector2(0.7f, 0.97f), cero, cero);
        txtTitulo.fontStyle = FontStyle.Bold;
        txtPisos = Util.TextoUI(pantalla, "", 30, Color.white, TextAnchor.UpperRight, new Vector2(0.5f, 0.87f), new Vector2(0.96f, 0.96f), cero, cero);

        GameObject gp = new GameObject("Plano", typeof(RectTransform));
        gp.transform.SetParent(pantalla, false);
        plano = gp.GetComponent<RectTransform>();
        plano.anchorMin = new Vector2(0.03f, 0.12f); plano.anchorMax = new Vector2(0.68f, 0.84f);
        plano.offsetMin = plano.offsetMax = Vector2.zero;

        RectTransform lado = Util.PanelArte(pantalla, "Misiones", new Color(0.03f, 0.06f, 0.13f, 0.95f), Util.Hex("#8ecae6"),
            new Vector2(0.7f, 0.12f), new Vector2(0.97f, 0.84f), cero, cero);
        txtLista = Util.TextoRelleno(lado, "", 24, Color.white, TextAnchor.UpperLeft, 22);

        Util.TextoUI(pantalla, "<color=#8ecae6>↑ ↓</color>  ver otros pisos        <color=#8ecae6>Q / Esc</color>  cerrar        " +
                               "<color=#4cc9f0>●</color> tú     <color=#ffd166>■</color> misión     <color=#8d99ae>■</color> escaleras y ascensor",
            24, new Color(1, 1, 1, 0.75f), TextAnchor.MiddleLeft, new Vector2(0.04f, 0.02f), new Vector2(0.97f, 0.1f), cero, cero);

        pantalla.gameObject.SetActive(false);
    }

    // Dibuja el mapa abierto (sin lógica) para la escena 10_Pantallas
    public static void ArmarEn(Transform padre, Juego j, Vector2 posicionTu)
    {
        GameObject go = new GameObject("Mapa (Q)", typeof(RectTransform));
        go.transform.SetParent(padre, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = rt.offsetMax = Vector2.zero;
        MiniMapa m = go.AddComponent<MiniMapa>();
        m.juego = j;
        m.Construir();
        m.pisoVisto = j.piso;
        m.pantalla.gameObject.SetActive(true);
        m.Dibujar();
        RectTransform ry = m.yo.rectTransform;
        ry.anchorMin = ry.anchorMax = m.Norm(posicionTu);
        ry.anchoredPosition = Vector2.zero;
        foreach (KeyValuePair<string, Image> kv in m.salas)
            if (m.numeros[kv.Key].text != "") kv.Value.color = Util.Hex("#ffd166");
        if (m.otroPiso) m.escaleras.color = Util.Hex("#ffd166");
        if (Application.isPlaying) Destroy(m); else DestroyImmediate(m);
    }

    void Abrir()
    {
        abierto = true;
        pisoVisto = juego.piso;
        escalaAntes = Time.timeScale > 0f ? Time.timeScale : 1f;
        Time.timeScale = 0f;
        juego.Bloquear(this);
        pantalla.gameObject.SetActive(true);
        Sonido.Efecto("blip", 0.6f, 1.2f);
        Dibujar();
    }

    void Cerrar()
    {
        abierto = false;
        Time.timeScale = escalaAntes;
        juego.Desbloquear(this);
        pantalla.gameObject.SetActive(false);
        Sonido.Efecto("click", 0.6f, 0.9f);
    }

    void OnDestroy()
    {
        if (abierto) Time.timeScale = escalaAntes;
    }

    Vector2 Norm(Vector2 w)
    {
        return new Vector2(Mathf.InverseLerp(limites.xMin, limites.xMax, w.x), Mathf.InverseLerp(limites.yMin, limites.yMax, w.y));
    }

    static string NombreCorto(string n)
    {
        int i = n.IndexOf('—');
        return i >= 0 ? n.Substring(i + 1).Trim() : n;
    }

    // Dibuja el piso que estás viendo
    void Dibujar()
    {
        for (int i = plano.childCount - 1; i >= 0; i--) Destroy(plano.GetChild(i).gameObject);
        salas.Clear(); numeros.Clear();
        destinos = Pendientes(juego);

        List<Sala> lista = juego.mapa.PlanoDePiso(pisoVisto);
        Vector2 o = Mapa.Origen(pisoVisto), esc = Mapa.Escaleras(pisoVisto), asc = Mapa.Ascensor(pisoVisto);
        limites = new Rect(o.x - Mapa.LargoPasillo / 2f, o.y - 1f, Mapa.LargoPasillo, 2f);
        foreach (Sala s in lista)
            limites = Rect.MinMaxRect(Mathf.Min(limites.xMin, s.area.xMin), Mathf.Min(limites.yMin, s.area.yMin),
                                      Mathf.Max(limites.xMax, s.area.xMax), Mathf.Max(limites.yMax, s.area.yMax));
        limites = Rect.MinMaxRect(Mathf.Min(limites.xMin, esc.x - 1.5f), limites.yMin, limites.xMax, limites.yMax);

        // Pasillo
        Util.Panel(plano, "Pasillo", ColPasillo, Norm(o + new Vector2(-Mapa.LargoPasillo / 2f, -Mapa.AnchoPasillo / 2f)),
                   Norm(o + new Vector2(Mapa.LargoPasillo / 2f, Mapa.AnchoPasillo / 2f)), Vector2.zero, Vector2.zero);
        // Salas con su nombre
        foreach (Sala s in lista)
        {
            RectTransform r = Util.Panel(plano, s.nombre, ColSala, Norm(new Vector2(s.area.xMin, s.area.yMin)), Norm(new Vector2(s.area.xMax, s.area.yMax)),
                                         new Vector2(3, 3), new Vector2(-3, -3));
            salas[s.id] = r.GetComponent<Image>();
            Text nom = Util.TextoUI(r, NombreCorto(s.nombre), 20, Util.Hex("#cfe3ff"), TextAnchor.UpperCenter, Vector2.zero, Vector2.one, new Vector2(6, 6), new Vector2(-6, -8));
            nom.verticalOverflow = VerticalWrapMode.Truncate;
            Text n = Util.TextoUI(r, "", 44, Util.Hex("#1a1206"), TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, new Vector2(0, -10), new Vector2(0, -10));
            n.fontStyle = FontStyle.Bold;
            numeros[s.id] = n;
        }
        // Escaleras y ascensor
        RectTransform re = Util.Panel(plano, "Escaleras", Util.Hex("#8d99ae"), Norm(esc + new Vector2(-1f, -1.1f)), Norm(esc + new Vector2(1f, 1.1f)), Vector2.zero, Vector2.zero);
        escaleras = re.GetComponent<Image>();
        Util.TextoUI(re, "ESC.", 16, Util.Hex("#14213d"), TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        RectTransform ra = Util.Panel(plano, "Ascensor", Util.Hex("#8d99ae"), Norm(asc + new Vector2(-1f, -1f)), Norm(asc + new Vector2(1f, 0.9f)), Vector2.zero, Vector2.zero);
        Util.TextoUI(ra, "ASC.", 16, Util.Hex("#14213d"), TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        // Tú (solo en el piso donde estás)
        RectTransform ry = Util.PanelCentro(plano, "Tú", Util.Hex("#4cc9f0"), Vector2.zero, new Vector2(26, 26));
        ry.anchorMin = ry.anchorMax = Vector2.zero;
        yo = ry.GetComponent<Image>();
        yo.gameObject.SetActive(pisoVisto == juego.piso);

        // Misiones: número en su sala y lista a la derecha
        otroPiso = false;
        System.Text.StringBuilder sb = new System.Text.StringBuilder("<b><color=#8ecae6>MISIONES</color></b>\n");
        sb.Append("<size=20><color=#999999>").Append(Protagonista.Nombre).Append("  ·  Día ").Append(Memoria.Dia).Append("  ·  ").Append(juego.HoraTexto).Append("</color></size>\n\n");
        int k = 0;
        foreach (Destino d in destinos)
        {
            k++;
            bool enVista = d.piso == pisoVisto;
            if (enVista && numeros.ContainsKey(d.sala))
                numeros[d.sala].text = numeros[d.sala].text == "" ? k.ToString() : numeros[d.sala].text + "," + k;
            if (pisoVisto == juego.piso && d.piso != juego.piso) otroPiso = true;
            string piso = Mapa.NombresPiso[Mathf.Clamp(d.piso, 0, Mapa.NombresPiso.Length - 1)];
            sb.Append(enVista ? "<color=#ffd166>" : "<color=#bbbbbb>").Append(k).Append(".  ").Append(d.texto).Append("</color>\n")
              .Append("<size=19>      ").Append(d.piso == juego.piso ? "<color=#7bd389>en este piso</color>" : "<color=#8ecae6>" + piso + "</color>").Append("</size>\n\n");
        }
        if (destinos.Count == 0) sb.Append("<color=#7bd389>¡No te falta nada!</color>");
        txtLista.text = sb.ToString();

        txtTitulo.text = "MAPA  <color=#8ecae6>·  " + Mapa.NombresPiso[pisoVisto].ToUpper() + "</color>" +
                         (pisoVisto == juego.piso ? "  <size=26><color=#4cc9f0>(estás aquí)</color></size>" : "");
        string pisos = "";
        string[] cortos = { "PB", "P1", "P2", "P3", "P4" };
        for (int i = Mapa.Pisos - 1; i >= 0; i--)
        {
            bool mision = false;
            foreach (Destino d in destinos) if (d.piso == i) mision = true;
            string c = i == pisoVisto ? "#ffffff" : mision ? "#ffd166" : "#556677";
            pisos += (i == pisoVisto ? "<b>[" : " ") + "<color=" + c + ">" + cortos[i] + "</color>" + (i == pisoVisto ? "]</b>" : " ") + "  ";
        }
        txtPisos.text = pisos;
    }

    void Update()
    {
        if (!abierto)
        {
            if (Entrada.Bajada(Tecla.Mapa) && !juego.Bloqueado && !juego.Pausado && !juego.ui.PantallaVisible && !Cine.Activa)
                Abrir();
            return;
        }
        transform.SetAsLastSibling();
        if (Entrada.Bajada(Tecla.Mapa) || Entrada.Bajada(Tecla.Saltar) || Entrada.Bajada(Tecla.Interactuar)) { Cerrar(); return; }
        if (Entrada.Bajada(Tecla.Arriba) && pisoVisto < Mapa.Pisos - 1) { pisoVisto++; Sonido.Efecto("click", 0.5f, 1.1f); Dibujar(); }
        if (Entrada.Bajada(Tecla.Abajo) && pisoVisto > 0) { pisoVisto--; Sonido.Efecto("click", 0.5f, 0.9f); Dibujar(); }

        t += Time.unscaledDeltaTime;
        float p = (Mathf.Sin(t * 5f) + 1f) * 0.5f;
        if (yo != null && yo.gameObject.activeSelf && juego.jugador != null)
        {
            RectTransform r = yo.rectTransform;
            r.anchorMin = r.anchorMax = Norm(juego.jugador.Posicion);
            r.anchoredPosition = Vector2.zero;
            yo.color = new Color(0.3f, 0.79f, 0.94f, 0.45f + p * 0.55f);
        }
        foreach (KeyValuePair<string, Image> kv in salas)
        {
            if (kv.Value == null) continue;
            bool mision = numeros[kv.Key].text != "";
            kv.Value.color = mision ? Color.Lerp(Util.Hex("#b8860b"), Util.Hex("#ffd166"), p) : ColSala;
        }
        if (escaleras != null) escaleras.color = otroPiso ? Color.Lerp(Util.Hex("#8d99ae"), Util.Hex("#ffd166"), p) : Util.Hex("#8d99ae");
    }

    // ---------- Qué te falta (y dónde) ----------
    static void Agregar(List<Destino> l, Juego j, string texto, string sala)
    {
        if (j.mapa == null) return;
        Sala s;
        int piso = j.mapa.Salas.TryGetValue(sala, out s) ? Mapa.PisoDe(s.Centro) : j.mapa.PisoDeSala(sala);
        if (piso < 0) return;
        l.Add(new Destino { texto = texto, sala = sala, piso = piso });
    }

    public static List<Destino> Pendientes(Juego j)
    {
        List<Destino> l = new List<Destino>();
        if (Partida.SimulacroEstado == 1 && Partida.Evento == "simulacro")
        {
            Agregar(l, j, "¡Simulacro! Baja al Ingreso", "ingreso");
            return l;
        }
        if (Protagonista.EsMateo) { PendientesMateo(l, j); return l; }

        string[] sellos = { Ids.SelloDerecho, Ids.SelloPeriodismo, Ids.SelloMedicina, Ids.SelloSistemas };
        string[] facultad = { "derecho", "periodismo", "medicina", "sistemas" };
        string[] nombre = { "Sello de Derecho", "Sello de Periodismo", "Sello de Medicina", "Sello de Sistemas" };
        for (int i = 0; i < 4; i++)
        {
            if (Memoria.Tiene(sellos[i])) continue;
            Misiones.Mision m = Misiones.DeSello(sellos[i]);
            if (m != null && !Memoria.Tiene(Misiones.Prenda(m)))
            {
                if (Memoria.Tiene(Misiones.Objeto(m))) Agregar(l, j, "Lleva " + m.objeto + " a " + m.guiaNombre, m.guiaSala);
                else if (Memoria.Tiene(Misiones.Activa(m))) Agregar(l, j, "Busca " + m.objeto, SalaObjeto(m));
                else Agregar(l, j, m.prenda + ": habla con " + m.guiaNombre, m.guiaSala);
            }
            else Agregar(l, j, nombre[i], facultad[i]);
        }
        if (l.Count == 0)
            Agregar(l, j, j.Hora < j.horaBienvenida ? "Bienvenida a las 18:00" : "¡Entra al Auditorio!", "auditorio");
        return l;
    }

    static string SalaObjeto(Misiones.Mision m)
    {
        switch (m.id)
        {
            case "toga":
            case "prensa": return "cafeteria";
            case "bata": return "biblioteca";
            case "tarjeta": return "diseno";
        }
        return m.guiaSala;
    }

    static void PendientesMateo(List<Destino> l, Juego j)
    {
        if (!Memoria.Tiene("clave_vargas")) { Agregar(l, j, "Habla con el Ing. Vargas", "sistemas"); return; }
        string[] salasVideo = { "biblioteca", "radio", "bioquimica", "servidores" };
        for (int i = 0; i < 4; i++)
            if (!Memoria.Tiene(Historia.ArchivosMateo[i])) Agregar(l, j, "Recupera el video " + (i + 1), salasVideo[i]);
        string[] nombres = { "Aliado: Joaquín", "Aliada: Paola", "Llave: Don Teodoro" };
        string[] salasAliado = { "radio", "psicologia", "seguridad" };
        for (int i = 0; i < 3; i++)
            if (!Memoria.Tiene(Historia.AliadosMateo[i])) Agregar(l, j, nombres[i], salasAliado[i]);
        if (!Memoria.Sabe("m_aviso")) Agregar(l, j, "Escribe en el directorio", "ingreso");
        if (l.Count == 0) Agregar(l, j, j.Hora < j.horaBienvenida ? "Cabina del auditorio a las 18:00" : "¡Sube a la cabina!", "auditorio");
    }
}
