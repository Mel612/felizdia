using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
#endif

// =====================================================================
//  CONTROLES TÁCTILES (celular / tablet)
//  Aparecen solos en Android/iOS. Para probarlos en la PC: en el objeto
//  "Juego" marca "Forzar Controles Tactiles" (se usan con el mouse).
//
//   Abajo izquierda: JOYSTICK (moverse, y en los menús arriba/abajo)
//   Abajo derecha:   E (usar / aceptar) · CORRER · FLASH  (y SALTAR en las persecuciones)
//   Arriba izquierda: MENÚ → se despliega MAPA · INVENTARIO · CELULAR · BOTELLA · CUADERNO · PAUSA
//                     (en menús, pausa y cinemáticas ese botón es ATRÁS)
//   Centro abajo:    1-6 (solo cámaras del cuarto secreto y videos del cuaderno)
//  Todo se acomoda a la "zona segura" de la pantalla (sin el notch).
//
//  Cada botón "aprieta" una tecla del juego: el resto del código no cambia.
// =====================================================================
public static class Tactil
{
    static readonly HashSet<Tecla> presionadas = new HashSet<Tecla>();
    static readonly HashSet<Tecla> bajadas = new HashSet<Tecla>();
    static readonly HashSet<Tecla> antes = new HashSet<Tecla>();
    public static Vector2 Joystick;
    static int frame = -1;

    public static bool Revisar(Tecla t, bool bajada) { return bajada ? bajadas.Contains(t) : presionadas.Contains(t); }

    // Lo llama ControlesTactiles una vez por cuadro, con las teclas que están apretadas
    public static void Actualizar(HashSet<Tecla> ahora, Vector2 joy)
    {
        if (frame == Time.frameCount) return;
        frame = Time.frameCount;
        Joystick = joy;
        // El joystick también funciona como flechas (para menús, minijuegos y persecuciones)
        if (joy.y > 0.55f) ahora.Add(Tecla.Arriba);
        if (joy.y < -0.55f) ahora.Add(Tecla.Abajo);
        if (joy.x < -0.55f) ahora.Add(Tecla.Izquierda);
        if (joy.x > 0.55f) ahora.Add(Tecla.Derecha);
        bajadas.Clear();
        foreach (Tecla t in ahora) if (!antes.Contains(t)) bajadas.Add(t);
        presionadas.Clear();
        foreach (Tecla t in ahora) presionadas.Add(t);
        antes.Clear();
        foreach (Tecla t in ahora) antes.Add(t);
    }

    public static void Limpiar() { presionadas.Clear(); bajadas.Clear(); antes.Clear(); Joystick = Vector2.zero; }
}

public class ControlesTactiles : MonoBehaviour
{
    public static bool Forzar;      // Juego ▸ "Forzar Controles Tactiles" (para probar en la PC)
    public static bool Activos { get { return Application.isMobilePlatform || Forzar; } }

    enum Grupo { Siempre, Accion, Menu, ItemMenu, Atras, Numero, Saltar }

    class Boton
    {
        public RectTransform rt;
        public Image img;
        public Text txt;
        public Tecla tecla;
        public Grupo grupo;
        public int numero;           // 1..6 para los botones de números
    }

    UIJuego ui;
    RectTransform raiz, zona, baseJoy, palanca, filaNumeros, barraMenu;
    readonly List<Boton> botones = new List<Boton>();
    Boton botonMenu, botonSaltar;
    bool desplegado, cerrarMenu, menuTocadoAntes;
    int dedoJoy = -1;
    Vector2 centroJoy;
    readonly HashSet<Tecla> ahora = new HashSet<Tecla>();
    Rect ultimaZona;

    const float RadioJoy = 150f;
    static readonly Vector2 PosJoy = new Vector2(240, 230);

    public static void Crear(UIJuego ui)
    {
        GameObject go = new GameObject("Controles táctiles", typeof(RectTransform));
        go.transform.SetParent(ui.transform, false);
        Canvas cv = go.AddComponent<Canvas>();
        cv.overrideSorting = true;
        cv.sortingOrder = 450;                        // encima de diálogos y menús (el brillo va en 500)
        ControlesTactiles c = go.AddComponent<ControlesTactiles>();
        c.ui = ui;
        c.raiz = go.GetComponent<RectTransform>();
        c.raiz.anchorMin = Vector2.zero; c.raiz.anchorMax = Vector2.one; c.raiz.offsetMin = c.raiz.offsetMax = Vector2.zero;
        // "Zona segura": sin el notch ni los bordes curvos del celular
        GameObject gz = new GameObject("Zona segura", typeof(RectTransform));
        gz.transform.SetParent(go.transform, false);
        c.zona = gz.GetComponent<RectTransform>();
        c.zona.anchorMin = Vector2.zero; c.zona.anchorMax = Vector2.one; c.zona.offsetMin = c.zona.offsetMax = Vector2.zero;
        c.Armar();
    }

    void Armar()
    {
        // Joystick (abajo a la izquierda)
        baseJoy = Circulo("Joystick", PosJoy, new Vector2(260, 260), new Color(1, 1, 1, 0.1f));
        palanca = Circulo("Palanca", Vector2.zero, new Vector2(115, 115), new Color(1, 1, 1, 0.3f));
        palanca.SetParent(baseJoy, false);
        palanca.anchorMin = palanca.anchorMax = new Vector2(0.5f, 0.5f);
        palanca.anchoredPosition = Vector2.zero;

        // Botones de acción (abajo a la derecha, donde llega el pulgar)
        Vector2 abajoDer = new Vector2(1, 0);
        Agregar("E", Tecla.Interactuar, abajoDer, new Vector2(-150, 170), 190, Util.Hex("#ffd166"), Grupo.Siempre, 64);
        Agregar("CORRER", Tecla.Correr, abajoDer, new Vector2(-350, 105), 135, Util.Hex("#4cc9f0"), Grupo.Accion, 24);
        botonSaltar = Agregar("SALTAR", Tecla.Accion, abajoDer, new Vector2(-340, 285), 125, Util.Hex("#7bd389"), Grupo.Saltar, 24);
        Agregar("FLASH", Tecla.Flash, abajoDer, new Vector2(-150, 380), 115, Util.Hex("#ffffff"), Grupo.Accion, 24);

        // Arriba a la izquierda, debajo del reloj: MENÚ (en el juego) o ATRÁS (en menús y cinemáticas)
        Vector2 arribaIzq = new Vector2(0, 1);
        botonMenu = Agregar("MENÚ", Tecla.Saltar, arribaIzq, new Vector2(85, -235), 115, Util.Hex("#ffd166"), Grupo.Menu, 24);
        Agregar("ATRÁS", Tecla.Saltar, arribaIzq, new Vector2(85, -235), 115, Util.Hex("#ff8080"), Grupo.Atras, 24);

        // El menú desplegable: una barra con los demás botones
        GameObject gb = new GameObject("Menú desplegado", typeof(RectTransform));
        gb.transform.SetParent(zona, false);
        barraMenu = gb.GetComponent<RectTransform>();
        barraMenu.anchorMin = barraMenu.anchorMax = arribaIzq;
        barraMenu.pivot = new Vector2(0f, 0.5f);
        barraMenu.sizeDelta = new Vector2(7 * 128 + 24, 140);
        barraMenu.anchoredPosition = new Vector2(155, -235);
        Image fondo = gb.AddComponent<Image>();
        fondo.color = new Color(0.04f, 0.03f, 0.07f, 0.85f);
        fondo.raycastTarget = false;
        string[] nombres = { "MAPA", "INVENTARIO", "CELULAR", "BOTELLA", "CUADERNO", "PAUSA", "CERRAR" };
        Tecla[] teclas = { Tecla.Mapa, Tecla.Inventario, Tecla.Celular, Tecla.Lanzar, Tecla.Cuaderno, Tecla.Pausa, Tecla.Saltar };
        for (int i = 0; i < nombres.Length; i++)
        {
            Boton b = Agregar(nombres[i], teclas[i], arribaIzq, new Vector2(85 + 128 + 12 + i * 128, -235), 112, new Color(1, 1, 1, 0.95f), Grupo.ItemMenu, 18);
            if (i == nombres.Length - 1) b.tecla = Tecla.N6 + 100;      // CERRAR: no aprieta ninguna tecla
        }

        // Números (solo para las cámaras del cuarto secreto y los videos del cuaderno)
        GameObject gf = new GameObject("Números 1-6", typeof(RectTransform));
        gf.transform.SetParent(zona, false);
        filaNumeros = gf.GetComponent<RectTransform>();
        filaNumeros.anchorMin = filaNumeros.anchorMax = new Vector2(0.5f, 0f);
        filaNumeros.anchoredPosition = Vector2.zero;
        Tecla[] nums = { Tecla.N1, Tecla.N2, Tecla.N3, Tecla.N4, Tecla.N5, Tecla.N6 };
        for (int i = 0; i < 6; i++)
        {
            Boton b = Agregar((i + 1).ToString(), nums[i], new Vector2(0.5f, 0f), new Vector2(-375 + i * 150, 80), 110, Util.Hex("#ffd166"), Grupo.Numero, 44);
            b.rt.SetParent(filaNumeros, true);
            b.numero = i + 1;
        }
    }

    RectTransform Circulo(string nombre, Vector2 pos, Vector2 tam, Color c)
    {
        GameObject go = new GameObject(nombre, typeof(RectTransform));
        go.transform.SetParent(zona, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = Vector2.zero;
        rt.sizeDelta = tam;
        rt.anchoredPosition = pos;
        Image im = go.AddComponent<Image>();
        im.sprite = Arte.CirculoUI();
        im.color = c;
        im.raycastTarget = false;
        return rt;
    }

    Boton Agregar(string texto, Tecla tecla, Vector2 ancla, Vector2 pos, float tam, Color c, Grupo grupo, int letra)
    {
        GameObject go = new GameObject("Botón " + texto, typeof(RectTransform));
        go.transform.SetParent(zona, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = ancla;
        rt.sizeDelta = new Vector2(tam, tam);
        rt.anchoredPosition = pos;
        Image im = go.AddComponent<Image>();
        im.sprite = Arte.CirculoUI();
        im.color = new Color(0.05f, 0.04f, 0.08f, 0.5f);
        im.raycastTarget = false;
        Text t = Util.TextoUI(rt, texto, letra, c, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, new Vector2(4, 0), new Vector2(-4, 0));
        t.fontStyle = FontStyle.Bold;
        t.raycastTarget = false;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        Boton b = new Boton { rt = rt, img = im, txt = t, tecla = tecla, grupo = grupo };
        botones.Add(b);
        return b;
    }

    // ---------- Dedos en la pantalla ----------
    struct Dedo { public int id; public Vector2 pos; public bool empezo; }
    readonly List<Dedo> dedos = new List<Dedo>();

    void LeerDedos()
    {
        dedos.Clear();
#if ENABLE_INPUT_SYSTEM
        Touchscreen ts = Touchscreen.current;
        if (ts != null)
            foreach (TouchControl t in ts.touches)
                if (t.press.isPressed)
                    dedos.Add(new Dedo { id = t.touchId.ReadValue(), pos = t.position.ReadValue(), empezo = t.press.wasPressedThisFrame });
        if (dedos.Count == 0 && Forzar && Mouse.current != null && Mouse.current.leftButton.isPressed)
            dedos.Add(new Dedo { id = 999, pos = Mouse.current.position.ReadValue(), empezo = Mouse.current.leftButton.wasPressedThisFrame });
#else
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch t = Input.GetTouch(i);
            if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled) continue;
            dedos.Add(new Dedo { id = t.fingerId, pos = t.position, empezo = t.phase == TouchPhase.Began });
        }
        if (dedos.Count == 0 && Forzar && Input.GetMouseButton(0))
            dedos.Add(new Dedo { id = 999, pos = Input.mousePosition, empezo = Input.GetMouseButtonDown(0) });
#endif
    }

    float Escala { get { return raiz.rect.width > 0f ? Screen.width / raiz.rect.width : 1f; } }

    void AjustarZonaSegura()
    {
        Rect s = Screen.safeArea;
        if (s == ultimaZona || Screen.width <= 0) return;
        ultimaZona = s;
        float e = Escala;
        zona.offsetMin = new Vector2(s.xMin / e, s.yMin / e);
        zona.offsetMax = new Vector2(-(Screen.width - s.xMax) / e, -(Screen.height - s.yMax) / e);
    }

    void Update()
    {
        transform.SetAsLastSibling();
        AjustarZonaSegura();
        Juego j = Juego.I;
        TipoEscena tipo = j != null ? j.tipoEscena : TipoEscena.Menu;

        // ¿Hay algo en pantalla completa? (pausa, mapa, inventario, cuaderno, celular, pantallas)
        bool pantallaCompleta = j == null || j.Pausado || MiniMapa.HayUnoAbierto || Inventario.HayUnoAbierto ||
                                ui.CuadernoAbierto || Celular.HayUnoAbierto || ui.PantallaVisible;
        bool jugandoLibre = tipo == TipoEscena.Piso && !pantallaCompleta && !Cine.Activa && j != null && !j.Bloqueado;
        bool persecucion = tipo == TipoEscena.Persecucion && !pantallaCompleta;
        bool minijuego = Minijuego.Actual != null;
        bool verAtras = pantallaCompleta || Cine.Activa || tipo == TipoEscena.Menu || tipo == TipoEscena.Cuarto || tipo == TipoEscena.Galeria || tipo == TipoEscena.Pantallas;
        if (!jugandoLibre) desplegado = false;

        int n = 0;
        if (tipo == TipoEscena.Cuarto && !pantallaCompleta) n = 6;
        else if (ui.CuadernoAbierto) n = 5;

        botonSaltar.txt.text = persecucion ? "SALTAR" : "ACCIÓN";
        foreach (Boton b in botones)
        {
            bool ver;
            switch (b.grupo)
            {
                case Grupo.Siempre: ver = true; break;
                case Grupo.Accion: ver = jugandoLibre || (persecucion && b.tecla == Tecla.Correr); break;
                case Grupo.Saltar: ver = persecucion || minijuego; break;
                case Grupo.Menu: ver = jugandoLibre; break;
                case Grupo.ItemMenu: ver = jugandoLibre && desplegado; break;
                case Grupo.Atras: ver = verAtras && !jugandoLibre; break;
                default: ver = b.numero <= n; break;
            }
            if (b.rt.gameObject.activeSelf != ver) b.rt.gameObject.SetActive(ver);
        }
        if (barraMenu.gameObject.activeSelf != (jugandoLibre && desplegado)) barraMenu.gameObject.SetActive(jugandoLibre && desplegado);
        barraMenu.SetAsFirstSibling();
        filaNumeros.anchoredPosition = new Vector2((6 - n) * 75f, 0f);

        LeerDedos();
        ahora.Clear();
        Vector2 joy = Vector2.zero;
        bool joyVivo = false, menuTocado = false, itemTocado = false;
        foreach (Dedo d in dedos)
        {
            if (d.id == dedoJoy) { joyVivo = true; joy = MoverJoy(d.pos); continue; }
            Boton tocado = null;
            foreach (Boton b in botones)
                if (b.rt.gameObject.activeInHierarchy && RectTransformUtility.RectangleContainsScreenPoint(b.rt, d.pos, null)) { tocado = b; break; }
            if (tocado != null)
            {
                if (tocado.grupo == Grupo.Menu) { menuTocado = true; continue; }       // MENÚ abre/cierra la barra
                if (tocado.grupo == Grupo.ItemMenu) { itemTocado = true; cerrarMenu = true; }
                if ((int)tocado.tecla < 100) ahora.Add(tocado.tecla);
                continue;
            }
            // Un dedo nuevo en la mitad izquierda de la pantalla agarra el joystick
            if (d.empezo && dedoJoy < 0 && d.pos.x < Screen.width * 0.45f && !(desplegado && d.pos.y > Screen.height * 0.6f))
            {
                dedoJoy = d.id;
                joyVivo = true;
                centroJoy = d.pos;
            }
        }
        if (menuTocado && !menuTocadoAntes) { desplegado = !desplegado; Sonido.Efecto("click", 0.5f, 1.2f); }
        menuTocadoAntes = menuTocado;
        if (cerrarMenu && !itemTocado) { desplegado = false; cerrarMenu = false; }   // se cierra al soltar el botón elegido
        if (!joyVivo) { dedoJoy = -1; palanca.anchoredPosition = Vector2.zero; baseJoy.anchoredPosition = PosJoy; }

        // Los botones apretados se iluminan
        foreach (Boton b in botones)
        {
            bool on = (b.grupo == Grupo.Menu && desplegado) || ((int)b.tecla < 100 && ahora.Contains(b.tecla));
            b.img.color = on ? new Color(1f, 1f, 1f, 0.35f) : new Color(0.05f, 0.04f, 0.08f, 0.5f);
        }
        Tactil.Actualizar(ahora, joy);
    }

    Vector2 MoverJoy(Vector2 dedo)
    {
        // La base del joystick sigue al dedo donde lo apoyaste
        float escala = Escala;
        baseJoy.anchoredPosition = (centroJoy - new Vector2(Screen.safeArea.xMin, Screen.safeArea.yMin)) / escala;
        Vector2 delta = (dedo - centroJoy) / escala;
        if (delta.magnitude > RadioJoy) delta = delta.normalized * RadioJoy;
        palanca.anchoredPosition = delta;
        Vector2 v = delta / RadioJoy;
        return v.magnitude < 0.15f ? Vector2.zero : v;
    }

    void OnDisable() { Tactil.Limpiar(); }
}
