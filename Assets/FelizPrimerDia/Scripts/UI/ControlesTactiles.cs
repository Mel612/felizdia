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
//   Izquierda: JOYSTICK (moverse, y en los menús arriba/abajo/izquierda/derecha)
//   Derecha:   E (usar / aceptar) · CORRER · SALTAR
//   Arriba:    MAPA · CEL · FLASH · BOTELLA · CUADERNO · PAUSA · ATRÁS
//   Centro:    1-6 (solo para las cámaras del cuarto secreto y los videos del cuaderno)
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

    class Boton
    {
        public RectTransform rt;
        public Image img;
        public Tecla tecla;
        public bool soloJugando;     // solo en los pisos (no en menús)
        public int numero;           // 1..6 para los botones de opciones
    }

    UIJuego ui;
    RectTransform raiz, baseJoy, palanca, filaNumeros;
    readonly List<Boton> botones = new List<Boton>();
    int dedoJoy = -1;
    Vector2 centroJoy;
    readonly HashSet<Tecla> ahora = new HashSet<Tecla>();

    const float RadioJoy = 150f;

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
        c.Armar();
    }

    void Armar()
    {
        // Joystick (abajo a la izquierda)
        baseJoy = Circulo("Joystick", new Vector2(0, 0), new Vector2(270, 270), new Color(1, 1, 1, 0.13f));
        baseJoy.anchoredPosition = new Vector2(230, 230);
        palanca = Circulo("Palanca", Vector2.zero, new Vector2(120, 120), new Color(1, 1, 1, 0.35f));
        palanca.SetParent(baseJoy, false);
        palanca.anchorMin = palanca.anchorMax = new Vector2(0.5f, 0.5f);
        palanca.anchoredPosition = Vector2.zero;

        // Botones grandes (abajo a la derecha)
        Agregar("E", Tecla.Interactuar, new Vector2(1, 0), new Vector2(-190, 210), 190, Util.Hex("#ffd166"), false);
        Agregar("CORRER", Tecla.Correr, new Vector2(1, 0), new Vector2(-400, 120), 150, Util.Hex("#4cc9f0"), true);
        Agregar("SALTAR", Tecla.Accion, new Vector2(1, 0), new Vector2(-400, 310), 130, Util.Hex("#7bd389"), false);

        // Fila de arriba a la derecha
        string[] nombres = { "ATRÁS", "PAUSA", "CUADERNO", "INV", "BOTELLA", "FLASH", "CEL", "MAPA" };
        Tecla[] teclas = { Tecla.Saltar, Tecla.Pausa, Tecla.Cuaderno, Tecla.Inventario, Tecla.Lanzar, Tecla.Flash, Tecla.Celular, Tecla.Mapa };
        for (int i = 0; i < nombres.Length; i++)
            Agregar(nombres[i], teclas[i], new Vector2(1, 1), new Vector2(-80 - i * 140, -415), 115, new Color(1, 1, 1, 0.9f), i >= 1);

        // Números para elegir opciones (centro abajo)
        GameObject gf = new GameObject("Opciones 1-6", typeof(RectTransform));
        gf.transform.SetParent(raiz, false);
        filaNumeros = gf.GetComponent<RectTransform>();
        filaNumeros.anchorMin = filaNumeros.anchorMax = new Vector2(0.5f, 0f);
        filaNumeros.anchoredPosition = new Vector2(0, 0);
        Tecla[] nums = { Tecla.N1, Tecla.N2, Tecla.N3, Tecla.N4, Tecla.N5, Tecla.N6 };
        for (int i = 0; i < 6; i++)
        {
            Boton b = Agregar((i + 1).ToString(), nums[i], new Vector2(0.5f, 0f), new Vector2(-375 + i * 150, 80), 120, Util.Hex("#ffd166"), false);
            b.rt.SetParent(filaNumeros, true);
            b.numero = i + 1;
        }
    }

    RectTransform Circulo(string nombre, Vector2 pos, Vector2 tam, Color c)
    {
        GameObject go = new GameObject(nombre, typeof(RectTransform));
        go.transform.SetParent(raiz, false);
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

    Boton Agregar(string texto, Tecla tecla, Vector2 ancla, Vector2 pos, float tam, Color c, bool soloJugando)
    {
        GameObject go = new GameObject("Botón " + texto, typeof(RectTransform));
        go.transform.SetParent(raiz, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = ancla;
        rt.sizeDelta = new Vector2(tam, tam);
        rt.anchoredPosition = pos;
        Image im = go.AddComponent<Image>();
        im.sprite = Arte.CirculoUI();
        im.color = new Color(0.05f, 0.04f, 0.08f, 0.55f);
        im.raycastTarget = false;
        Text t = Util.TextoUI(rt, texto, texto.Length > 3 ? 22 : 48, c, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        t.fontStyle = FontStyle.Bold;
        t.raycastTarget = false;
        Boton b = new Boton { rt = rt, img = im, tecla = tecla, soloJugando = soloJugando };
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

    void Update()
    {
        transform.SetAsLastSibling();
        Juego j = Juego.I;
        bool jugando = j != null && (j.tipoEscena == TipoEscena.Piso) && !Cine.Activa && !ui.PantallaVisible;
        bool persecucion = j != null && j.tipoEscena == TipoEscena.Persecucion;

        // ¿Hay que elegir una opción? Muestra los números que sirven
        // (las opciones de diálogos, menús y minijuegos se eligen con el joystick + E;
        //  los números solo hacen falta para las cámaras del cuarto secreto y los videos del cuaderno)
        int n = 0;
        if (j != null && j.tipoEscena == TipoEscena.Cuarto) n = 6;
        else if (ui.CuadernoAbierto) n = 5;
        foreach (Boton b in botones)
        {
            bool ver = b.numero > 0 ? b.numero <= n : (!b.soloJugando || jugando || (persecucion && b.tecla == Tecla.Correr));
            if (b.rt.gameObject.activeSelf != ver) b.rt.gameObject.SetActive(ver);
        }
        // Los números quedan centrados según cuántos hay
        filaNumeros.anchoredPosition = new Vector2((6 - n) * 75f, 0f);

        LeerDedos();
        ahora.Clear();
        Vector2 joy = Vector2.zero;
        bool joyVivo = false;
        foreach (Dedo d in dedos)
        {
            // ¿Es el dedo del joystick?
            if (d.id == dedoJoy) { joyVivo = true; joy = MoverJoy(d.pos); continue; }
            Boton tocado = null;
            foreach (Boton b in botones)
                if (b.rt.gameObject.activeInHierarchy && RectTransformUtility.RectangleContainsScreenPoint(b.rt, d.pos, null)) { tocado = b; break; }
            if (tocado != null) { ahora.Add(tocado.tecla); continue; }
            // Un dedo nuevo en la mitad izquierda de la pantalla agarra el joystick
            if (d.empezo && dedoJoy < 0 && d.pos.x < Screen.width * 0.45f)
            {
                dedoJoy = d.id;
                joyVivo = true;
                centroJoy = d.pos;
                joy = Vector2.zero;
            }
        }
        if (!joyVivo) { dedoJoy = -1; palanca.anchoredPosition = Vector2.zero; baseJoy.anchoredPosition = new Vector2(230, 230); }

        // Los botones apretados se iluminan
        foreach (Boton b in botones)
            b.img.color = ahora.Contains(b.tecla) ? new Color(1f, 1f, 1f, 0.35f) : new Color(0.05f, 0.04f, 0.08f, 0.55f);

        Tactil.Actualizar(ahora, joy);
    }

    Vector2 MoverJoy(Vector2 dedo)
    {
        // La base del joystick sigue al dedo donde lo apoyaste
        float escala = raiz.rect.width > 0f ? Screen.width / raiz.rect.width : 1f;
        baseJoy.anchoredPosition = centroJoy / escala;
        Vector2 delta = (dedo - centroJoy) / escala;
        if (delta.magnitude > RadioJoy) delta = delta.normalized * RadioJoy;
        palanca.anchoredPosition = delta;
        Vector2 v = delta / RadioJoy;
        return v.magnitude < 0.15f ? Vector2.zero : v;
    }

    void OnDisable() { Tactil.Limpiar(); }
}
