using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// =====================================================================
//  PANTALLA DE INICIO (estilo Resident Evil)
//  De fondo se ve la entrada de la U de noche, con truenos y el Pepino
//  parado bajo un farol.
//  Menú: Continuar · Cargar partida · Nueva partida · (Cuarto secreto) ·
//        Logros · Opciones · Cómo jugar · Créditos.
//  El CUARTO SECRETO es un nivel escondido: no aparece en el menú hasta que
//  lo encuentras jugando la historia.
// =====================================================================
public class MenuInicio : MonoBehaviour
{
    // Lo que se ve en el menú depende de lo que ya desbloqueaste
    readonly List<string> ids = new List<string>();
    readonly List<string> nombres = new List<string>();

    Juego juego;
    RectTransform raiz, panelInfo;
    Text txtOpciones, txtInfo, txtTitulo, txtMateo;
    int seleccion;
    bool empezar;
    bool infoAbierta, eligiendoNoche, eligiendoEspacio, ocupado;
    float tTrueno = 4f, tPepino = 2f, tTitulo;
    SpriteRenderer pepino, farol;

    public static IEnumerator Mostrar(Juego j)
    {
        Cine c = Cine.Empezar(true);
        j.ui.Barras(false);
        j.ui.HudVisible = false;
        // La entrada de la Unifranz de noche (se arma lejos del campus)
        Vector2 o = FachadaUnifranz.OrigenNoche;
        FachadaUnifranz.Construir(c, o, true);
        c.Camara(o + new Vector2(-1.2f, 4.2f), 6.6f);
        j.ui.Telon(1f, 0f);
        j.ui.Telon(0f, 2.5f);
        Sonido.Musica("m_menu", 2f);
        Sonido.Ambiente(null, 0f);

        // Necesita RectTransform que ocupe toda la pantalla (si no, el texto sale en una columna)
        GameObject go = new GameObject("Menú de inicio", typeof(RectTransform));
        go.transform.SetParent(j.ui.transform, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        MenuInicio m = go.AddComponent<MenuInicio>();
        m.juego = j;
        m.pepino = c.Actor("Pepino", Arte.Pepino(), o + new Vector2(5.6f, -0.9f));      // bajo el farol
        m.farol = c.Objeto("Luz del farol (parpadeo)", Arte.Luz(), o + new Vector2(5.6f, 0.6f), 30001);
        m.farol.color = new Color(1f, 0.85f, 0.55f, 0.2f);
        m.farol.transform.localScale = new Vector3(2.4f, 2.4f, 1f);
        m.Construir();

        while (!m.empezar) yield return null;

        Sonido.Efecto("susto", 0.6f, 0.7f);
        j.ui.Telon(1f, 0.8f);
        yield return new WaitForSeconds(0.9f);
        Destroy(go);
        j.ui.HudVisible = true;
        yield return c.Terminar();
        j.ui.Telon(1f, 0f);
    }

    // Dibuja el menú en la escena 00_Menu para verlo en el editor (en la pestaña Game).
    // Es una muestra: al darle Play se borra y el menú de verdad se arma solo.
    public static GameObject VistaPrevia()
    {
        GameObject go = new GameObject("(muestra) Menú de inicio", typeof(RectTransform));
        Canvas cv = go.AddComponent<Canvas>();
        cv.renderMode = RenderMode.ScreenSpaceOverlay;
        cv.sortingOrder = 100;
        CanvasScaler cs = go.AddComponent<CanvasScaler>();
        cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1920, 1080);
        cs.matchWidthOrHeight = 0.5f;
        go.AddComponent<SoloEnEditor>();

        ArmarEn(go.transform);
        return go;
    }

    // Dibuja el menú (sin lógica) dentro de un padre que ocupa toda la pantalla
    public static void ArmarEn(Transform padre)
    {
        GameObject g = new GameObject("Menú de inicio", typeof(RectTransform));
        g.transform.SetParent(padre, false);
        RectTransform rt = g.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = rt.offsetMax = Vector2.zero;
        MenuInicio m = g.AddComponent<MenuInicio>();
        m.Construir();
        if (Application.isPlaying) Destroy(m); else DestroyImmediate(m);
    }

    void ArmarOpciones()
    {
        ids.Clear(); nombres.Clear();
        if (Guardado.HayAlguno)
        {
            ids.Add("continuar"); nombres.Add("CONTINUAR");
            ids.Add("cargar"); nombres.Add("CARGAR PARTIDA");
        }
        ids.Add("nueva"); nombres.Add("NUEVA PARTIDA");
        if (NocheMonitoreo.CuartoEncontrado) { ids.Add("cuarto"); nombres.Add("CUARTO SECRETO"); }
        ids.Add("logros"); nombres.Add("LOGROS");
        ids.Add("opciones"); nombres.Add("OPCIONES");
        ids.Add("como"); nombres.Add("CÓMO JUGAR");
        ids.Add("creditos"); nombres.Add("CRÉDITOS");
        ids.Add("salir"); nombres.Add("SALIR");
        seleccion = Mathf.Clamp(seleccion, 0, ids.Count - 1);
    }

    void Construir()
    {
        Vector2 cero = Vector2.zero, uno = Vector2.one;
        raiz = Util.Panel(transform, "Menú", new Color(0.01f, 0.01f, 0.05f, 0.45f), cero, uno, cero, cero);
        Util.Imagen(raiz, "Sangre", Arte.Chorreado(), new Vector2(0, 0.8f), new Vector2(1, 1), cero, cero);

        // El título se achica solo si la ventana es angosta (antes se cortaba a la derecha)
        txtTitulo = Util.TextoUI(raiz, "FELIZ PRIMER DÍA", 110, Util.Hex("#f1faee"), TextAnchor.MiddleLeft, new Vector2(0.05f, 0.64f), new Vector2(0.97f, 0.86f), cero, cero);
        txtTitulo.fontStyle = FontStyle.Bold;
        txtTitulo.horizontalOverflow = HorizontalWrapMode.Wrap;
        txtTitulo.verticalOverflow = VerticalWrapMode.Truncate;
        txtTitulo.resizeTextForBestFit = true;
        txtTitulo.resizeTextMinSize = 40;
        txtTitulo.resizeTextMaxSize = 110;
        Shadow sh = txtTitulo.gameObject.AddComponent<Shadow>();
        sh.effectColor = new Color(0.5f, 0f, 0f, 0.95f);
        sh.effectDistance = new Vector2(7, -7);
        Util.TextoUI(raiz, "UNIFRANZ  ·  SEDE LA PAZ", 34, Util.Hex("#ffd166"), TextAnchor.UpperLeft, new Vector2(0.055f, 0.57f), new Vector2(0.9f, 0.64f), cero, cero);

        // Opciones: caja con tamaño automático para que nunca se encimen con el pie
        txtOpciones = Util.TextoUI(raiz, "", 40, Color.white, TextAnchor.UpperLeft, new Vector2(0.06f, 0.1f), new Vector2(0.5f, 0.55f), cero, cero);
        txtOpciones.verticalOverflow = VerticalWrapMode.Truncate;
        txtOpciones.resizeTextForBestFit = true;
        txtOpciones.resizeTextMinSize = 20;
        txtOpciones.resizeTextMaxSize = 40;
        txtOpciones.lineSpacing = 1.25f;
        txtOpciones.gameObject.AddComponent<Outline>().effectColor = Color.black;

        Util.TextoUI(raiz, "Flechas: elegir     E: aceptar     Esc: volver     M: sonido", 22, new Color(1, 1, 1, 0.5f), TextAnchor.LowerLeft, new Vector2(0.06f, 0.015f), new Vector2(0.5f, 0.07f), cero, cero);
        txtMateo = Util.TextoUI(raiz, "", 22, Color.white, TextAnchor.LowerRight, new Vector2(0.5f, 0.015f), new Vector2(0.97f, 0.07f), cero, cero);
        txtMateo.gameObject.AddComponent<Outline>().effectColor = Color.black;

        panelInfo = Util.PanelArteCentro(raiz, "Info", new Color(0.04f, 0.03f, 0.07f, 0.95f), Util.Hex("#ffd166"), new Vector2(380, -40), new Vector2(1000, 820));
        txtInfo = Util.TextoRelleno(panelInfo, "", 26, Color.white, TextAnchor.UpperLeft, 30);
        panelInfo.gameObject.SetActive(false);
        ArmarOpciones();
        Dibujar();
    }

    void Dibujar()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        for (int i = 0; i < nombres.Count; i++)
        {
            string color = ids[i] == "cuarto" ? "#c77dff" : "#bbbbbb";
            sb.Append(i == seleccion ? "<color=#ff4d4d>>  " + nombres[i] + "</color>" : "<color=" + color + ">     " + nombres[i] + "</color>");
            if (i < nombres.Count - 1) sb.Append("\n");
        }
        txtOpciones.text = sb.ToString();
        txtMateo.text = Protagonista.MateoDesbloqueado
            ? "<color=#4cc9f0>Nuevo personaje: MATEO  (elígelo en Nueva partida)</color>"
            : "<color=#777777>MATEO  ·  bloqueado  (termina la historia de Valeria)</color>";
    }

    void CerrarInfo()
    {
        infoAbierta = eligiendoNoche = eligiendoEspacio = false;
        panelInfo.gameObject.SetActive(false);
    }

    void Update()
    {
        if (ocupado) return;                   // cuarto secreto, opciones o créditos en pantalla
        raiz.SetAsLastSibling();
        transform.SetAsLastSibling();

        // Título que late, truenos y el Pepino que se mueve
        tTitulo += Time.deltaTime;
        txtTitulo.color = Color.Lerp(Util.Hex("#f1faee"), Util.Hex("#ffb3b3"), (Mathf.Sin(tTitulo * 2f) + 1f) * 0.25f);
        tTrueno -= Time.deltaTime;
        if (tTrueno <= 0f)
        {
            tTrueno = Random.Range(7f, 13f);
            Sonido.Efecto("trueno", 0.6f, Random.Range(0.85f, 1.1f));
            juego.ui.Destello(Color.white, 0.35f);
        }
        tPepino -= Time.deltaTime;
        if (tPepino <= 0f && pepino != null)
        {
            tPepino = Random.Range(1.5f, 3.5f);
            pepino.flipX = !pepino.flipX;
        }
        if (farol != null) farol.color = new Color(1f, 0.85f, 0.55f, Random.value < 0.03f ? 0.05f : 0.35f);

        int n = Entrada.NumeroBajado();
        if (eligiendoNoche)
        {
            int max = NocheMonitoreo.NochesDesbloqueadas;
            if (n >= 1 && n <= max) { CerrarInfo(); StartCoroutine(JugarNoche(n)); return; }
            if (Entrada.Avanzar() || Entrada.Bajada(Tecla.Saltar)) { CerrarInfo(); Sonido.Efecto("click"); }
            return;
        }
        if (eligiendoEspacio)
        {
            if (n >= 1 && n <= Guardado.Espacios && Guardado.Existe(n)) { CerrarInfo(); StartCoroutine(CargarEspacio(n)); return; }
            if (Entrada.Avanzar() || Entrada.Bajada(Tecla.Saltar)) { CerrarInfo(); Sonido.Efecto("click"); }
            return;
        }
        if (infoAbierta)
        {
            if (Entrada.Avanzar() || Entrada.Bajada(Tecla.Saltar) || n > 0) { CerrarInfo(); Sonido.Efecto("click"); }
            return;
        }
        if (Entrada.Bajada(Tecla.Arriba)) { seleccion = (seleccion + ids.Count - 1) % ids.Count; Sonido.Efecto("click", 0.6f, 1f); Dibujar(); }
        if (Entrada.Bajada(Tecla.Abajo)) { seleccion = (seleccion + 1) % ids.Count; Sonido.Efecto("click", 0.6f, 1f); Dibujar(); }
        if (n >= 1 && n <= ids.Count) { seleccion = n - 1; Dibujar(); Elegir(); }
        else if (Entrada.Avanzar()) Elegir();
    }

    IEnumerator JugarNoche(int n)
    {
        ocupado = true;
        if (Partida.PuedeCargar(Partida.EscenaCuarto))
        {
            juego.ui.Telon(1f, 0.5f);
            yield return new WaitForSeconds(0.6f);
            Partida.IrACuarto(n, true);           // la escena dibujada del cuarto secreto
            yield break;
        }
        raiz.gameObject.SetActive(false);
        Resultado<bool> r = new Resultado<bool>();
        yield return StartCoroutine(NocheMonitoreo.Jugar(n, r));
        raiz.gameObject.SetActive(true);
        Sonido.Musica("m_menu", 1.5f);
        Dibujar();
        ocupado = false;
    }

    IEnumerator CargarEspacio(int espacio)
    {
        ocupado = true;
        Sonido.Efecto("susto", 0.6f, 0.7f);
        juego.ui.Telon(1f, 0.8f);
        yield return new WaitForSeconds(0.9f);
        if (!Guardado.Cargar(espacio))
        {
            juego.ui.Telon(0f, 0.5f);
            juego.ui.Toast("No se pudo cargar esa partida.", 4f);
            ocupado = false;
        }
    }

    IEnumerator AbrirOpciones()
    {
        ocupado = true;
        raiz.gameObject.SetActive(false);
        yield return StartCoroutine(PanelOpciones.Abrir(transform));
        raiz.gameObject.SetActive(true);
        Dibujar();
        ocupado = false;
    }

    IEnumerator VerCreditos()
    {
        ocupado = true;
        raiz.gameObject.SetActive(false);
        yield return StartCoroutine(Creditos.Rodar(transform));
        raiz.gameObject.SetActive(true);
        Sonido.Musica("m_menu", 1.5f);
        Dibujar();
        ocupado = false;
    }

    void Elegir()
    {
        Sonido.Efecto("blip", 0.6f, 0.8f);
        string id = ids[seleccion];
        switch (id)
        {
            case "nueva": empezar = true; return;
            case "continuar":
            {
                int ultimo = Guardado.Ultimo();
                if (ultimo > 0) StartCoroutine(CargarEspacio(ultimo));
                return;
            }
            case "opciones": StartCoroutine(AbrirOpciones()); return;
            case "creditos": StartCoroutine(VerCreditos()); return;
            case "salir": Partida.SalirDelJuego(); return;
        }

        infoAbierta = true;
        panelInfo.gameObject.SetActive(true);
        if (id == "cargar")
        {
            eligiendoEspacio = true;
            string t = "<b>CARGAR PARTIDA</b>\n\nLas partidas se guardan en las MÁQUINAS DE ESCRIBIR\nque hay junto a las escaleras de cada piso.\n\n";
            for (int i = 1; i <= Guardado.Espacios; i++)
                t += "<color=#ffd166>" + i + "</color>   " + Guardado.Resumen(Guardado.Leer(i)) + "\n\n";
            txtInfo.text = t + "<color=#888888>Elige el espacio con su número  ·  E o Esc para volver</color>";
        }
        else if (id == "cuarto")
        {
            eligiendoNoche = true;
            int max = NocheMonitoreo.NochesDesbloqueadas;
            string t = "<b><color=#c77dff>CUARTO SECRETO</color></b>\n<size=22><color=#888888>nivel escondido · lo encontraste</color></size>\n\n" +
                       "Sobrevive de 12 AM a 6 AM en el viejo cuarto de monitoreo.\n\n" +
                       "<color=#ffd166>A / D</color>  cerrar o abrir las puertas\n<color=#ffd166>W</color>  subir o bajar el monitor\n<color=#ffd166>1-6</color>  elegir cámara\n" +
                       "Las puertas y las cámaras gastan energía.\n\n";
            for (int i = 1; i <= 3; i++)
                t += i <= max ? "<color=#7bd389>" + i + "  ·  NOCHE " + i + "</color>\n" : "<color=#555555>" + i + "  ·  NOCHE " + i + " (bloqueada)</color>\n";
            txtInfo.text = t + "\n<color=#888888>Elige la noche con su número  ·  E o Esc para volver</color>";
        }
        else if (id == "logros") txtInfo.text = "<size=22>" + Logros.Texto() + "</size>\n<color=#888888>E o Esc para volver</color>";
        else if (id == "como")
            txtInfo.text =
                "<b>CÓMO JUGAR</b>\n\n" +
                "Valeria Quispe vive su primer día en la U... una y otra vez.\n" +
                "Consigue los 4 sellos del Pasaporte de Carreras (Medicina, Derecho,\nPeriodismo e Ing. de Sistemas) y entra a la Bienvenida a las 18:00.\n" +
                "Cada vez que mueres, el día vuelve a empezar, pero lo que aprendes queda en tu cuaderno.\n" +
                "Para guardar, usa las <color=#ffd166>máquinas de escribir</color> junto a las escaleras.\n\n" +
                "<color=#ffd166>WASD / Flechas</color>  moverse     <color=#ffd166>Shift</color>  correr\n" +
                "<color=#ffd166>E</color>  hablar / usar     <color=#ffd166>1-4</color>  elegir opciones\n" +
                "<color=#ffd166>Tab</color>  cuaderno de pistas y logros     <color=#ffd166>M</color>  sonido\n" +
                "<color=#ffd166>P</color>  pausa (opciones, volver al menú)\n" +
                "<color=#ffd166>Espacio</color>  saltar (persecuciones)     <color=#ffd166>Esc</color>  saltar cinemáticas\n\n" +
                "<color=#888888>E o Esc para volver</color>";
    }
}
