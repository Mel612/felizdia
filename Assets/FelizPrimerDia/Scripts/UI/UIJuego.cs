using System.Text;
using UnityEngine;
using UnityEngine.UI;

// Toda la interfaz: reloj, pasaporte, pruebas, avisos, oscuridad, sangre, pantallas y cuaderno.
public class UIJuego : MonoBehaviour
{
    public Canvas canvas;
    public RectTransform raizUI;

    Text txtHora, txtPiso, txtPasaporte, txtPrompt, txtToast, txtAyuda, txtLogro;
    RectTransform cajaLogro;
    // Cinemáticas
    RectTransform barraArriba, barraAbajo;
    Text txtSubtitulo, txtSaltar, txtCartel, txtCartelSub;
    Image telon;
    float barrasObjetivo, barrasActual, telonObjetivo, telonActual, velTelon = 1.5f;
    float cartelHasta, cartelAlpha;
    public bool HudVisible = true;
    // Recuerdos (efecto de cinta VHS)
    RectTransform vhs;
    Image vhsGrano;
    Text vhsTexto;
    float vhsT;
    float logroHasta;
    float oscuridadBase;
    Image oscuridad, ambiente, flash, vineta;
    Image portadaIzq, portadaDer, chorreado;
    RectTransform pantalla;
    Text pantTitulo, pantCuerpo, pantPie;
    RectTransform cuaderno;
    Text cuadernoTexto;
    bool cuadernoAbierto;

    float toastHasta;
    float oscuridadObjetivo, oscuridadActual;
    float flashAlpha;
    Color flashColor = new Color(0.6f, 0f, 0f);

    CanvasScaler escalador;

    public bool PantallaVisible { get { return pantalla.gameObject.activeSelf; } }

    public static UIJuego Crear(Transform raiz)
    {
        GameObject go = new GameObject("UI");
        go.transform.SetParent(raiz, false);
        UIJuego ui = go.AddComponent<UIJuego>();

        ui.canvas = go.AddComponent<Canvas>();
        ui.canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        ui.canvas.sortingOrder = 100;
        CanvasScaler cs = go.AddComponent<CanvasScaler>();
        ui.escalador = cs;
        cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1920, 1080);
        cs.matchWidthOrHeight = 0.5f;
        ui.raizUI = go.GetComponent<RectTransform>();
        ui.Construir();
        return ui;
    }

    void Construir()
    {
        Transform t = transform;
        Vector2 cero = Vector2.zero, uno = Vector2.one;

        // Capas de ambiente (atardecer) y oscuridad (apagones)
        ambiente = Util.Panel(t, "Ambiente", new Color(0.05f, 0.02f, 0.12f, 0f), cero, uno, cero, cero).GetComponent<Image>();
        vineta = Util.Imagen(t, "Viñeta", Arte.Vineta("vineta", 0.6f, 1.45f), cero, uno, cero, cero);
        vineta.color = new Color(0, 0, 0, 0.55f);
        oscuridad = Util.Imagen(t, "Oscuridad", Arte.Vineta("linterna", 0.1f, 0.6f), new Vector2(-0.35f, -0.35f), new Vector2(1.35f, 1.35f), cero, cero);
        oscuridad.color = new Color(0f, 0f, 0f, 0f);
        Color hudFondo = new Color(0.05f, 0.04f, 0.08f, 0.85f), hudBorde = Util.Hex("#ffd166");

        // Reloj (arriba izquierda)
        RectTransform cajaHora = Util.PanelArte(t, "Reloj", hudFondo, hudBorde, new Vector2(0, 1), new Vector2(0, 1), new Vector2(20, -150), new Vector2(380, -20));
        txtHora = Util.TextoRelleno(cajaHora, "", 44, Color.white, TextAnchor.UpperLeft, 14);
        txtPiso = Util.TextoUI(cajaHora, "", 24, Util.Hex("#cfcfcf"), TextAnchor.LowerLeft, cero, uno, new Vector2(16, 12), new Vector2(-12, -12));

        // Pasaporte (arriba derecha)
        RectTransform cajaPas = Util.PanelArte(t, "Pasaporte", hudFondo, hudBorde, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-470, -345), new Vector2(-20, -20));
        txtPasaporte = Util.TextoRelleno(cajaPas, "", 20, Color.white, TextAnchor.UpperLeft, 12);

        // Prompt de interacción (abajo centro)
        txtPrompt = Util.TextoUI(t, "", 32, Util.Hex("#ffe8a3"), TextAnchor.MiddleCenter, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(-600, 250), new Vector2(600, 310));
        Outline o = txtPrompt.gameObject.AddComponent<Outline>();
        o.effectColor = Color.black;

        // Avisos (arriba centro)
        txtToast = Util.TextoUI(t, "", 32, Color.white, TextAnchor.MiddleCenter, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(-560, -220), new Vector2(560, -100));
        Outline o2 = txtToast.gameObject.AddComponent<Outline>();
        o2.effectColor = Color.black;

        txtAyuda = Util.TextoUI(t, "[Q] Mapa     [Tab] Cuaderno     [P] Menú / guardar", 22, new Color(1, 1, 1, 0.6f), TextAnchor.LowerLeft, cero, cero, new Vector2(20, 16), new Vector2(800, 50));

        // Aviso de logro (abajo derecha)
        cajaLogro = Util.PanelArte(t, "Logro", new Color(0.08f, 0.06f, 0.02f, 0.92f), Util.Hex("#ffd166"), new Vector2(1, 0), new Vector2(1, 0), new Vector2(-560, 20), new Vector2(-20, 130));
        txtLogro = Util.TextoRelleno(cajaLogro, "", 28, Color.white, TextAnchor.MiddleLeft, 16);
        cajaLogro.gameObject.SetActive(false);

        // Destello rojo (sangre / golpe)
        flash = Util.Panel(t, "Flash", new Color(0.6f, 0, 0, 0), cero, uno, cero, cero).GetComponent<Image>();

        // Cuaderno
        cuaderno = Util.PanelArteCentro(t, "Cuaderno", Util.Hex("#f3ead2"), Util.Hex("#6b4a2f"), Vector2.zero, new Vector2(1200, 860));
        Image hojas = Util.Imagen(cuaderno, "Líneas", Arte.PapelCuaderno(), Vector2.zero, Vector2.one, new Vector2(20, 20), new Vector2(-20, -90));
        hojas.type = Image.Type.Tiled;
        Util.Panel(cuaderno, "Margen", Util.Hex("#e5989b"), new Vector2(0, 0), new Vector2(0, 1), new Vector2(34, 20), new Vector2(37, -20));
        Text tituloCuaderno = Util.TextoUI(cuaderno, "CUADERNO DE VALERIA", 40, Util.Hex("#3b2a1a"), TextAnchor.UpperCenter, cero, uno, new Vector2(30, 30), new Vector2(-30, -24));
        tituloCuaderno.fontStyle = FontStyle.Bold;
        cuadernoTexto = Util.TextoUI(cuaderno, "", 25, Util.Hex("#2b2015"), TextAnchor.UpperLeft, cero, uno, new Vector2(46, 40), new Vector2(-46, -90));
        cuaderno.gameObject.SetActive(false);

        // Cinemáticas: barras negras, subtítulos, cartel de lugar y telón para fundidos
        // Barras de cine transparentes: un degradado oscuro suave arriba y abajo (se ve el juego detrás)
        barraArriba = Util.Panel(t, "Barra cine arriba", new Color(0, 0, 0, 0.6f), new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 0), new Vector2(0, 0));
        barraAbajo = Util.Panel(t, "Barra cine abajo", new Color(0, 0, 0, 0.6f), new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 0), new Vector2(0, 0));
        Image ia = barraArriba.GetComponent<Image>(), ib = barraAbajo.GetComponent<Image>();
        ia.sprite = Arte.DegradadoCine(true); ib.sprite = Arte.DegradadoCine(false);
        ia.raycastTarget = ib.raycastTarget = false;
        txtSubtitulo = Util.TextoUI(barraAbajo, "", 34, Color.white, TextAnchor.MiddleCenter, cero, uno, new Vector2(140, 10), new Vector2(-140, -10));
        Outline os = txtSubtitulo.gameObject.AddComponent<Outline>();
        os.effectColor = new Color(0, 0, 0, 0.9f);
        os.effectDistance = new Vector2(2, -2);
        Shadow ss = txtSubtitulo.gameObject.AddComponent<Shadow>();
        ss.effectColor = new Color(0, 0, 0, 0.8f);
        ss.effectDistance = new Vector2(3, -3);
        txtSaltar = Util.TextoUI(barraArriba, "[Esc] saltar", 22, new Color(1, 1, 1, 0.6f), TextAnchor.MiddleRight, cero, uno, new Vector2(20, 0), new Vector2(-30, 0));
        txtSaltar.gameObject.AddComponent<Outline>().effectColor = new Color(0, 0, 0, 0.8f);
        txtCartel = Util.TextoUI(t, "", 64, Color.white, TextAnchor.MiddleCenter, new Vector2(0, 0.5f), new Vector2(1, 0.62f), cero, cero);
        txtCartel.fontStyle = FontStyle.Bold;
        Shadow sc = txtCartel.gameObject.AddComponent<Shadow>();
        sc.effectColor = new Color(0, 0, 0, 0.9f);
        sc.effectDistance = new Vector2(4, -4);
        txtCartelSub = Util.TextoUI(t, "", 30, Util.Hex("#ffd166"), TextAnchor.UpperCenter, new Vector2(0, 0.42f), new Vector2(1, 0.5f), cero, cero);
        txtCartelSub.gameObject.AddComponent<Outline>().effectColor = Color.black;
        // Efecto de recuerdo: sepia, líneas de TV, grano y "REC"
        vhs = Util.Panel(t, "Recuerdo (VHS)", new Color(0.55f, 0.33f, 0.12f, 0.22f), cero, uno, cero, cero);
        vhs.GetComponent<Image>().raycastTarget = false;
        Image lineas = Util.Imagen(vhs, "Líneas", Arte.LineasTV(), cero, uno, cero, cero);
        lineas.color = new Color(1, 1, 1, 0.35f);
        vhsGrano = Util.Imagen(vhs, "Grano", Arte.Estatica(0), cero, uno, cero, cero);
        vhsGrano.color = new Color(1, 1, 1, 0.07f);
        Util.Imagen(vhs, "Viñeta", Arte.Vineta("vineta_vhs", 0.45f, 1.2f), cero, uno, cero, cero).color = new Color(0.1f, 0.05f, 0f, 0.6f);
        vhsTexto = Util.TextoUI(vhs, "", 34, Util.Hex("#f1faee"), TextAnchor.UpperLeft, new Vector2(0, 0.8f), new Vector2(1, 1), new Vector2(40, 0), new Vector2(-40, -168));
        vhsTexto.gameObject.AddComponent<Outline>().effectColor = Color.black;
        vhs.gameObject.SetActive(false);

        telon = Util.Panel(t, "Telón (fundidos)", Color.black, cero, uno, cero, cero).GetComponent<Image>();
        telon.color = new Color(0, 0, 0, 0);
        telon.raycastTarget = false;

        // Pantalla completa (intro, muerte, finales)
        pantalla = Util.Panel(t, "Pantalla", Color.black, cero, uno, cero, cero);
        portadaIzq = Util.Imagen(pantalla, "Pepino", Arte.Pepino().Frente, new Vector2(0.03f, 0.18f), new Vector2(0.2f, 0.78f), cero, cero);
        portadaIzq.preserveAspect = true;
        portadaDer = Util.Imagen(pantalla, "Valeria", Arte.Valeria().Frente, new Vector2(0.8f, 0.18f), new Vector2(0.97f, 0.78f), cero, cero);
        portadaDer.preserveAspect = true;
        chorreado = Util.Imagen(pantalla, "Sangre", Arte.Chorreado(), new Vector2(0, 0.78f), new Vector2(1, 1), cero, cero);
        pantTitulo = Util.TextoUI(pantalla, "", 96, Color.white, TextAnchor.MiddleCenter, new Vector2(0, 0.7f), new Vector2(1, 0.92f), new Vector2(60, 0), new Vector2(-60, 0));
        pantTitulo.fontStyle = FontStyle.Bold;
        Shadow sombraTitulo = pantTitulo.gameObject.AddComponent<Shadow>();
        sombraTitulo.effectColor = new Color(0, 0, 0, 0.9f);
        sombraTitulo.effectDistance = new Vector2(6, -6);
        pantCuerpo = Util.TextoUI(pantalla, "", 34, Util.Hex("#eeeeee"), TextAnchor.UpperCenter, new Vector2(0, 0.14f), new Vector2(1, 0.7f), new Vector2(160, 0), new Vector2(-160, 0));
        pantPie = Util.TextoUI(pantalla, "", 28, Util.Hex("#ffd166"), TextAnchor.MiddleCenter, new Vector2(0, 0.02f), new Vector2(1, 0.14f), new Vector2(60, 0), new Vector2(-60, 0));
        pantalla.gameObject.SetActive(false);
    }

    void Update()
    {
        AnimarCine();
        Juego j = Juego.I;
        if (j == null) return;

        // Reloj y piso
        int piso = j.jugador != null ? j.jugador.Piso : 0;
        string dia = Dificultad.BuclesMax > 0 ? "Bucle " + Memoria.Dia + "/" + Dificultad.BuclesMax : "Día " + Memoria.Dia;
        txtHora.text = j.HoraTexto + "   <size=26><color=#ff8080>" + dia + "</color>" +
                       (Dificultad.Contrarreloj ? "  <color=#8ecae6>" + Dificultad.Formato(Dificultad.TiempoTotal) + "</color>" : "") + "</size>";
        string extra = j.jugador != null && j.jugador.Escondido ? "   <color=#8ecae6>(escondida)</color>" : "";
        if (Partida.SimulacroEstado == 1 && Partida.Evento == "simulacro")
        {
            int min = Mathf.Max(0, Mathf.CeilToInt((Partida.SimulacroLimite - j.Hora) * 60f));
            extra = "   <color=#ff4d4d>¡SIMULACRO! Baja a la PB (" + min + " min)</color>";
        }
        else if (oscuridadBase > 0f) extra += "   <color=#aaaaaa>(sin luz)</color>";
        txtPiso.text = Mapa.NombresPiso[piso] + extra;

        // Pasaporte y pruebas (Mateo tiene el suyo)
        StringBuilder sb = new StringBuilder();
        if (Protagonista.EsMateo) { PasaporteMateo(sb, j); txtPasaporte.text = sb.ToString(); }
        else {
        sb.Append("<b>PASAPORTE DE CARRERAS</b>\n");
        sb.Append(Linea(Ids.SelloMedicina, "Medicina (Piso 3)"));
        sb.Append(Linea(Ids.SelloDerecho, "Derecho (Piso 1)"));
        sb.Append(Linea(Ids.SelloPeriodismo, "Periodismo (Piso 2)"));
        sb.Append(Linea(Ids.SelloSistemas, "Ing. de Sistemas (Piso 4)"));
        sb.Append("\n<b>PRUEBAS</b>  ").Append(Memoria.Cuantos(Ids.Pruebas)).Append("/4");
        sb.Append("     <b>CARRERAS</b>  ").Append(Historia.CarrerasConocidas());
        sb.Append("\n").Append(Misiones.Resumen());
        sb.Append("\n<b>GRABACIONES</b>  ").Append(Memoria.CuantosRecuerdos(Ids.Audios)).Append("/8");
        if (Memoria.Sabe("natita")) sb.Append("     <b>OFRENDA</b>  ").Append(Memoria.Tiene("ofrenda_natita") ? "<color=#7bd389>puesta</color>" : Memoria.Cuantos(Ids.Ofrenda) + "/3");
        if (Memoria.Tiene("tarjeta")) sb.Append("\n<color=#8ecae6>Tarjeta del ascensor</color>");
        if (Dificultad.MarcarObjetivo) sb.Append("\n<color=#ffd166>→ ").Append(Objetivo(j)).Append("</color>");
        txtPasaporte.text = sb.ToString();
        }

        // Aviso temporal
        if (Time.time > toastHasta) txtToast.text = "";

        // Ventana angosta: la interfaz se achica con el ancho para no tapar el juego
        if (escalador != null)
            escalador.matchWidthOrHeight = Screen.width < Screen.height * 1.6f ? 0f : 0.5f;

        // Ambiente: oscurece hacia la noche
        float tarde = Mathf.InverseLerp(15f, 19.5f, j.Hora);
        ambiente.color = new Color(0.05f, 0.02f, 0.12f, tarde * 0.4f);
        float parpadeo = (tarde > 0.3f && Random.value < 0.004f) ? 0.25f : 0f;
        vineta.color = new Color(0, 0, 0, Mathf.Clamp01(0.45f + tarde * 0.4f + parpadeo - Mathf.Max(0f, Ajustes.Brillo - 1f) * 0.5f));

        // Apagones
        oscuridadActual = Mathf.MoveTowards(oscuridadActual, Mathf.Max(oscuridadObjetivo, oscuridadBase), Time.deltaTime * 2.5f);

        // Aviso de logro
        if (cajaLogro.gameObject.activeSelf && Time.time > logroHasta) cajaLogro.gameObject.SetActive(false);
        oscuridad.color = new Color(0, 0, 0, oscuridadActual);

        // Destello
        flashAlpha = Mathf.MoveTowards(flashAlpha, 0f, Time.deltaTime * 0.8f);
        flash.color = new Color(flashColor.r, flashColor.g, flashColor.b, flashAlpha);

        // Cuaderno
        if (Entrada.Bajada(Tecla.Cuaderno) && !PantallaVisible)
        {
            if (cuadernoAbierto) CerrarCuaderno();
            else if (!j.Bloqueado) AbrirCuaderno();
        }
    }

    // ---------- Pasaporte de Mateo ----------
    static void PasaporteMateo(StringBuilder sb, Juego j)
    {
        sb.Append("<b>MATEO  ·  NIVEL EXTRA</b>\n");
        sb.Append("<b>VIDEOS</b>  ").Append(Memoria.Cuantos(Historia.ArchivosMateo)).Append("/4\n");
        for (int i = 0; i < 4; i++)
        {
            bool ok = Memoria.Tiene(Historia.ArchivosMateo[i]);
            sb.Append(ok ? "<color=#7bd389>■ " : "<color=#888888>□ ").Append("Video ").Append(i + 1).Append("  ·  ").Append(Historia.LugarArchivo[i]).Append("</color>\n");
        }
        string[] nombres = { "Joaquín", "Paola", "Teodoro" };
        sb.Append("<b>ALIADOS</b>  ");
        for (int i = 0; i < 3; i++) sb.Append(Memoria.Tiene(Historia.AliadosMateo[i]) ? "<color=#7bd389>" : "<color=#777777>").Append(nombres[i]).Append("</color> ");
        sb.Append("\n<b>CARRERAS</b>  ").Append(Historia.CarrerasConocidas());
        if (Memoria.Tiene("llave_cabina")) sb.Append("    <color=#8ecae6>Llave de la cabina</color>");
        if (Dificultad.MarcarObjetivo) sb.Append("\n<color=#ffd166>→ ").Append(ObjetivoMateo(j)).Append("</color>");
    }

    static string ObjetivoMateo(Juego j)
    {
        if (!Memoria.Tiene("clave_vargas")) return "Habla con el Ing. Vargas (Laboratorio, Piso 4)";
        for (int i = 0; i < 4; i++)
            if (!Memoria.Tiene(Historia.ArchivosMateo[i])) return "Recupera el video " + (i + 1) + " en " + Historia.LugarArchivo[i];
        if (!Memoria.Tiene(Historia.AliadosMateo[0])) return "Pide a Joaquín (Radio, Piso 2) que transmita";
        if (!Memoria.Tiene(Historia.AliadosMateo[1])) return "Pide ayuda a Paola (Psicología, Piso 1)";
        if (!Memoria.Tiene(Historia.AliadosMateo[2])) return "Pide la llave de la cabina a Don Teodoro (PB)";
        if (!Memoria.Sabe("m_aviso")) return "Escribe la advertencia en el directorio (PB)";
        return j.Hora < j.horaBienvenida ? "Espera las 18:00 (Auditorio, PB)" : "¡Entra a la Bienvenida y sube a la cabina!";
    }

    // Cachimbo: qué hacer ahora
    static string Objetivo(Juego j)
    {
        string[] sellos = { Ids.SelloDerecho, Ids.SelloPeriodismo, Ids.SelloMedicina, Ids.SelloSistemas };
        string[] donde = { "Piso 1: sello de Derecho", "Piso 2: sello de Periodismo", "Piso 3: sello de Medicina", "Piso 4: sello de Sistemas" };
        bool puedeSubir = Memoria.Tiene("escaleras_ok") || Memoria.Tiene("con_diego") || Memoria.Tiene("tarjeta") || j.Hora >= 9f;
        if (Partida.SimulacroEstado == 1 && Partida.Evento == "simulacro") return "¡Baja a la Planta Baja!";
        if (!puedeSubir && Memoria.Sabe("escaleras")) return "Pide a Diego que suba contigo o la tarjeta a Don Teodoro";
        for (int i = 0; i < 4; i++)
        {
            if (Memoria.Tiene(sellos[i])) continue;
            Misiones.Mision m = Misiones.DeSello(sellos[i]);
            if (m != null && !Memoria.Tiene(Misiones.Prenda(m)))
            {
                if (Memoria.Tiene(Misiones.Objeto(m))) return "Lleva " + m.objeto + " a " + m.guiaNombre + " (" + Misiones.NombreSala(m.guiaSala) + ")";
                if (Memoria.Tiene(Misiones.Activa(m))) return "Busca " + m.objeto + " en " + m.dondeObjeto;
                return "Pide " + m.prenda.ToLower() + " a " + m.guiaNombre + " (" + Misiones.NombreSala(m.guiaSala) + ")";
            }
            return "Ve al " + donde[i];
        }
        if (j.Hora < j.horaBienvenida) return "Busca pruebas o espera las 18:00 (Auditorio, PB)";
        return "¡Entra al Auditorio (Planta Baja)!";
    }

    public void Logro(string titulo)
    {
        txtLogro.text = "<color=#ffd166><b>LOGRO DESBLOQUEADO</b></color>\n" + titulo;
        cajaLogro.gameObject.SetActive(true);
        Sonido.Efecto("logro");
        cajaLogro.SetAsLastSibling();
        logroHasta = Time.time + 4.5f;
    }

    public void OscuridadBase(float alpha) { oscuridadBase = Mathf.Clamp01(alpha); }

    // ---------------------------------------------------------------
    //  CINEMÁTICAS
    // ---------------------------------------------------------------
    public void Barras(bool mostrar)
    {
        barrasObjetivo = mostrar ? 1f : 0f;
        if (mostrar) { Prompt(null); txtToast.text = ""; }
    }

    public void Subtitulo(string quien, string texto)
    {
        if (string.IsNullOrEmpty(texto)) { txtSubtitulo.text = ""; return; }
        txtSubtitulo.text = string.IsNullOrEmpty(quien) ? "<i>" + texto + "</i>" : "<color=#ffd166><b>" + quien + ":</b></color>  " + texto;
    }

    // Cartel grande tipo "lugar y hora" (como en las películas)
    public void Cartel(string titulo, string sub, float duracion)
    {
        txtCartel.text = titulo ?? "";
        txtCartelSub.text = sub ?? "";
        cartelHasta = Time.time + duracion;
        cartelAlpha = 0f;
    }

    // Fundido a negro (alpha 1) o desde negro (alpha 0)
    public void Telon(float alpha, float segundos)
    {
        telonObjetivo = Mathf.Clamp01(alpha);
        velTelon = 1f / Mathf.Max(0.01f, segundos);
        if (segundos <= 0f) telonActual = telonObjetivo;
    }

    public void TelonColor(Color c) { telon.color = new Color(c.r, c.g, c.b, telonActual); }

    // Activa el efecto de cinta vieja (recuerdos). fecha = texto del contador de la cámara.
    public void Recuerdo(bool activo, string fecha)
    {
        vhs.gameObject.SetActive(activo);
        vhsT = 0f;
        if (activo) vhsTexto.text = "<color=#ff4d4d>●</color> REC     " + fecha + "\n<size=24><color=#ffd166>RECUERDO</color></size>";
    }

    void AnimarCine()
    {
        if (vhs.gameObject.activeSelf)
        {
            vhsT += Time.deltaTime;
            vhsGrano.sprite = Arte.Estatica(((int)(vhsT * 20f)) % 4);
            vhsGrano.color = new Color(1, 1, 1, 0.05f + Random.value * 0.05f);
            vhs.SetAsLastSibling();
            vhsTexto.enabled = ((int)(vhsT * 1.5f)) % 2 == 0 || vhsT > 2f;
        }
        barrasActual = Mathf.MoveTowards(barrasActual, barrasObjetivo, Time.deltaTime * 2.5f);
        float k = barrasActual * barrasActual * (3f - 2f * barrasActual);
        float alto = 150f * k;
        barraArriba.offsetMin = new Vector2(0, -alto);
        barraAbajo.offsetMax = new Vector2(0, alto);
        txtSaltar.enabled = barrasActual > 0.9f;
        if (barrasActual <= 0f) txtSubtitulo.text = "";
        bool hudVisible = barrasActual < 0.5f && HudVisible;
        if (txtHora != null)
        {
            txtHora.transform.parent.gameObject.SetActive(hudVisible);
            txtPasaporte.transform.parent.gameObject.SetActive(hudVisible);
            txtAyuda.enabled = hudVisible;
        }

        telonActual = Mathf.MoveTowards(telonActual, telonObjetivo, Time.deltaTime * velTelon);
        Color c = telon.color;
        telon.color = new Color(c.r, c.g, c.b, telonActual);
        telon.transform.SetAsLastSibling();
        if (PantallaVisible) pantalla.SetAsLastSibling();

        float objetivoCartel = Time.time < cartelHasta ? 1f : 0f;
        cartelAlpha = Mathf.MoveTowards(cartelAlpha, objetivoCartel, Time.deltaTime * 1.5f);
        txtCartel.color = new Color(1, 1, 1, cartelAlpha);
        Color cs = txtCartelSub.color;
        txtCartelSub.color = new Color(cs.r, cs.g, cs.b, cartelAlpha);
    }

    string Linea(string sello, string nombre)
    {
        bool ok = Memoria.Tiene(sello);
        return (ok ? "<color=#7bd389>■ " : "<color=#888888>□ ") + nombre + "</color>\n";
    }

    // ---------------------------------------------------------------
    public void Prompt(string texto) { txtPrompt.text = texto ?? ""; }

    public void Toast(string texto, float duracion)
    {
        txtToast.text = texto;
        toastHasta = Time.time + duracion;
    }

    public void Toast(string texto) { Toast(texto, 3.5f); }

    // Brillo (opciones): 1 normal · menos de 1 más oscuro · más de 1 más claro. Va encima de todo.
    Image capaBrillo;
    public void Brillo(float b)
    {
        if (capaBrillo == null)
        {
            GameObject go = new GameObject("Brillo", typeof(RectTransform));
            go.transform.SetParent(transform, false);
            Canvas c = go.AddComponent<Canvas>();
            c.overrideSorting = true;
            c.sortingOrder = 500;
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = rt.offsetMax = Vector2.zero;
            capaBrillo = go.AddComponent<Image>();
            capaBrillo.raycastTarget = false;
        }
        if (b < 1f) capaBrillo.color = new Color(0f, 0f, 0f, Mathf.Clamp01((1f - b) * 1.3f));
        else capaBrillo.color = new Color(1f, 0.97f, 0.9f, Mathf.Clamp01((b - 1f) * 0.45f));
        capaBrillo.enabled = Mathf.Abs(b - 1f) > 0.01f;
    }

    public void Oscuridad(float alpha) { oscuridadObjetivo = Mathf.Clamp01(alpha); }
    public void OscuridadInstantanea(float alpha) { oscuridadObjetivo = oscuridadActual = Mathf.Clamp01(alpha); }

    public void Sangre()
    {
        flashColor = new Color(0.55f, 0f, 0f);
        flashAlpha = 1f;
    }

    public void Golpe(float intensidad)
    {
        flashColor = new Color(0.8f, 0.05f, 0.05f);
        flashAlpha = Mathf.Max(flashAlpha, intensidad);
    }

    public void Destello(Color c, float intensidad)
    {
        flashColor = c;
        flashAlpha = Mathf.Max(flashAlpha, intensidad);
    }

    public void MostrarPantalla(string titulo, string cuerpo, string pie, Color fondo)
    {
        MostrarPantalla(titulo, cuerpo, pie, fondo, 0);
    }

    // estilo: 0 normal, 1 portada (personajes), 2 muerte (sangre)
    public void MostrarPantalla(string titulo, string cuerpo, string pie, Color fondo, int estilo)
    {
        portadaIzq.gameObject.SetActive(estilo == 1);
        portadaDer.gameObject.SetActive(estilo == 1);
        chorreado.gameObject.SetActive(estilo == 2);
        pantTitulo.color = estilo == 1 ? Util.Hex("#e63946") : Color.white;
        pantalla.GetComponent<Image>().color = fondo;
        pantTitulo.text = titulo;
        pantCuerpo.text = cuerpo;
        pantPie.text = pie;
        pantalla.gameObject.SetActive(true);
        pantalla.SetAsLastSibling();
    }

    public void OcultarPantalla() { pantalla.gameObject.SetActive(false); }

    // ---------------------------------------------------------------
    const int PistasPorPagina = 5;
    int paginaCuaderno;
    readonly System.Collections.Generic.List<string> bloquesCuaderno = new System.Collections.Generic.List<string>();
    readonly System.Collections.Generic.List<string> videosCuaderno = new System.Collections.Generic.List<string>();

    void AbrirCuaderno()
    {
        cuadernoAbierto = true;
        Juego.I.Bloquear(this);
        bloquesCuaderno.Clear();
        videosCuaderno.Clear();
        foreach (string id in Memoria.Recuerdos)
        {
            Pistas.Pista p;
            if (!Pistas.Catalogo.TryGetValue(id, out p)) continue;
            bloquesCuaderno.Add("<b>• " + p.titulo + "</b>{VIDEO}\n" + p.texto + "\n\n");
            videosCuaderno.Add(Recuerdos.VideoDe(id));
        }
        // Lo más nuevo primero
        bloquesCuaderno.Reverse();
        videosCuaderno.Reverse();
        paginaCuaderno = 0;
        DibujarCuaderno();
        cuaderno.gameObject.SetActive(true);
        cuaderno.SetAsLastSibling();
    }

    void DibujarCuaderno()
    {
        StringBuilder sb = new StringBuilder();
        int paginasPistas = Mathf.Max(1, Mathf.CeilToInt(bloquesCuaderno.Count / (float)PistasPorPagina));
        int paginas = paginasPistas + 1;                 // la última página: logros
        paginaCuaderno = Mathf.Clamp(paginaCuaderno, 0, paginas - 1);
        if (paginaCuaderno == paginas - 1)
        {
            sb.Append("<size=22>").Append(Logros.Texto()).Append("</size>\n");
            sb.Append("<color=#7a6a55>Página ").Append(paginas).Append("/").Append(paginas).Append("  (←/→)      [Tab] cerrar</color>");
            cuadernoTexto.text = sb.ToString();
            return;
        }
        if (bloquesCuaderno.Count == 0)
            sb.Append("Todavía no anotaste nada.\n\nCada vez que algo salga mal, lo que aprendas quedará aquí...\naunque el día vuelva a empezar.\n\n");
        for (int i = paginaCuaderno * PistasPorPagina; i < Mathf.Min(bloquesCuaderno.Count, (paginaCuaderno + 1) * PistasPorPagina); i++)
        {
            int tecla = i - paginaCuaderno * PistasPorPagina + 1;
            sb.Append(bloquesCuaderno[i].Replace("{VIDEO}", videosCuaderno[i] != null ? "   <color=#a4161a>[" + tecla + "] ver recuerdo</color>" : ""));
        }
        sb.Append("<color=#7a6a55>Página ").Append(paginaCuaderno + 1).Append("/").Append(paginas)
          .Append("  (←/→)      Charlas con Camila: ").Append(Memoria.CuantosRecuerdos(Ids.CharlasCamila))
          .Append("/4      [Tab] cerrar</color>");
        cuadernoTexto.text = sb.ToString();
    }

    void LateUpdate()
    {
        if (!cuadernoAbierto) return;
        if (Entrada.Bajada(Tecla.Derecha)) { paginaCuaderno++; DibujarCuaderno(); }
        if (Entrada.Bajada(Tecla.Izquierda)) { paginaCuaderno--; DibujarCuaderno(); }
        // Volver a ver un recuerdo (video) con su número
        int n = Entrada.NumeroBajado();
        int i = paginaCuaderno * PistasPorPagina + n - 1;
        if (n >= 1 && n <= PistasPorPagina && i < videosCuaderno.Count && videosCuaderno[i] != null)
        {
            CerrarCuaderno();
            StartCoroutine(Recuerdos.Reproducir(videosCuaderno[i]));
        }
    }

    void CerrarCuaderno()
    {
        cuadernoAbierto = false;
        cuaderno.gameObject.SetActive(false);
        Juego.I.Desbloquear(this);
    }
}
