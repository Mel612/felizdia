using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// =====================================================================
//  FELIZ PRIMER DÍA — script principal.
//  Lo más fácil: menú "Feliz Primer Día ▸ Construir escenas". Crea la escena
//  Campus con todo el edificio dibujado y este componente ya puesto.
//  Al darle Play, el juego usa lo que está dibujado en la escena (si moviste
//  algo en el editor, se respeta) y agrega los personajes, diálogos y el loop.
// =====================================================================
public class Juego : MonoBehaviour
{
    public static Juego I { get; private set; }

    [Header("Reloj del día (horas en formato 24h)")]
    public float horaInicio = 6.75f;          // 6:45
    public float horaFin = 19.5f;             // 19:30
    [Tooltip("Minutos reales que dura un día (el reloj se pausa en diálogos y minijuegos)")]
    public float minutosRealesPorDia = 14f;
    public float horaBienvenida = 18f;        // abre el auditorio
    public float horaPepinoActivo = 9f;       // desde esta hora el Pepino patrulla los pasillos

    [Header("Qué escena es esta (lo configura el menú Construir escenas)")]
    public TipoEscena tipoEscena = TipoEscena.Piso;
    [Range(0, 4)] public int piso = 0;

    [Header("Cámara")]
    [Tooltip("Zoom de la cámara en las escenas que no son pisos. En los pisos se elige en OPCIONES (Cerca / Normal / Lejos).")]
    public float zoomOtras = 6f;
    public float zoomCamara { get { return tipoEscena == TipoEscena.Piso ? Ajustes.ZoomJuego : zoomOtras; } }

    [Header("Cinemáticas y sonido")]
    [Tooltip("Pantalla de inicio animada y cinemática de llegada (si lo desactivas, sale la pantalla de texto)")]
    public bool usarMenuInicio = true;
    [Tooltip("Muestra los controles táctiles en la PC (se usan con el mouse) para probarlos sin celular.")]
    public bool forzarControlesTactiles = false;
    // El volumen y el brillo se cambian en el juego: menú OPCIONES o tecla P (se guardan solos).

    [Header("Pruebas rápidas (solo para desarrollo)")]
    [Tooltip("Empieza el día con todos los recuerdos aprendidos")]
    public bool modoDesarrollo = false;

    [HideInInspector] public Jugador jugador;
    [HideInInspector] public UIJuego ui;
    [HideInInspector] public Dialogo dialogo;
    [HideInInspector] public Mapa mapa;
    [HideInInspector] public Camera camara;
    [HideInInspector] public Transform camaraSeguir;     // si no es null, la cámara sigue esto (persecuciones)
    [HideInInspector] public EscenaBienvenida bienvenida;

    public float Hora { get; private set; }
    public void FijarHora(float h) { Hora = h; }
    public bool RelojDetenido;                        // el tutorial detiene el reloj      // para las vistas previas del editor

    // Hora en que aparece el Pepino: cada día que mueres aparece un poco más temprano.
    public float HoraPepino
    {
        get { return horaPepinoActivo - Mathf.Min(1.5f, 0.25f * (Memoria.Dia - 1) * Dificultad.Escalada); }
    }

    float tRevisarLogros;
    bool fantasmaRevisado;
    public Transform Raiz { get; private set; }
    public bool Terminado { get; private set; }
    public int FrameDesbloqueo { get; private set; }

    readonly HashSet<object> bloqueos = new HashSet<object>();

    public bool Bloqueado { get { return bloqueos.Count > 0 || Terminado; } }
    public bool PuedeInteractuar { get { return !Bloqueado && Time.frameCount - FrameDesbloqueo > 1; } }

    public string HoraTexto
    {
        get
        {
            int h = Mathf.FloorToInt(Hora);
            int m = Mathf.FloorToInt((Hora - h) * 60f);
            return h.ToString("00") + ":" + m.ToString("00");
        }
    }

    void Awake()
    {
        I = this;
        ControlesTactiles.Forzar = forzarControlesTactiles;
        Application.targetFrameRate = 60;
    }

    void Start()
    {
        Ajustes.Cargar();
        if (modoDesarrollo)
            foreach (string id in Pistas.Catalogo.Keys)
                if (id != "final_malo") Memoria.Aprender(id);
        Construir();
    }

    // ---------------------------------------------------------------
    void Construir()
    {
        bloqueos.Clear();
        Terminado = false;
        Pepino.Cazando = 0;
        MundoCuarto.Actual = null;
        Sonido.Latido(0f);
        Sonido.BajarMusica(1f);
        Hora = Partida.DiaNuevo ? horaInicio : Partida.Hora;
        Interactuable.Todos.Clear();

        // ¿La escena tiene el campus dibujado? Entonces se usa ese.
        GameObject dibujado = GameObject.Find(Horno.NombreRaiz);
        if (dibujado == null) dibujado = GameObject.Find(Horno.NombreRaizAntiguo);
        if (dibujado != null) Horno.Cargar(dibujado.transform);
        else Horno.Olvidar();

        Raiz = new GameObject("Mundo (día " + Memoria.Dia + ")").transform;
        PrepararCamara();

        ui = UIJuego.Crear(Raiz);
        dialogo = Dialogo.Crear(ui);
        Ajustes.Aplicar();                  // volumen y brillo guardados

        ConstruirContenido(Raiz, false);
        if (tipoEscena == TipoEscena.Piso)
        {
            MiniMapa.Crear(this);     // mapa (Q) con tus misiones
            Celular.Crear(this);      // mensajes del grupo de la promo (C)
            Inventario.Crear(this);   // inventario (I)
        }

        if (Horno.HayEscenaDibujada && Horno.Faltantes > 0)
            Debug.LogWarning("[Feliz Primer Día] " + Horno.Faltantes + " objetos no se encontraron en la escena dibujada (se crearon por código). " +
                             "Si cambiaste el código del mapa, usa el menú Feliz Primer Día ▸ Construir escenas para volver a dibujarla.");

        if (tipoEscena == TipoEscena.Piso) StartCoroutine(Intro());
        else if (tipoEscena == TipoEscena.Persecucion) StartCoroutine(FlujoPersecucion());
        else if (tipoEscena == TipoEscena.Cuarto) StartCoroutine(FlujoCuarto());
        else if (tipoEscena == TipoEscena.Galeria) Galeria.Crear(this, piso);
        else if (tipoEscena == TipoEscena.Menu) StartCoroutine(FlujoMenu());
        else if (tipoEscena == TipoEscena.Pantallas)
        {
            ui.HudVisible = false;
            camara.transform.position = new Vector3(Pantallas.Centro.x, Pantallas.Centro.y, -10f);
            camara.orthographicSize = Pantallas.Zoom;
            Sonido.Musica("m_menu", 1f);
        }
        else if (bienvenida != null) StartCoroutine(bienvenida.Flujo());
        CalcularLimites();
    }

    // Lo que tiene cada escena. Es el MISMO código para dibujar la escena en el editor
    // y para jugar: por eso los objetos dibujados coinciden con los del juego.
    void ConstruirContenido(Transform raiz, bool horneando)
    {
        if (tipoEscena == TipoEscena.Piso)
        {
            GameObject goMapa = new GameObject(horneando ? "Mapa: " + Mapa.NombresPiso[piso] : "Mapa");
            goMapa.transform.SetParent(raiz, false);
            mapa = goMapa.AddComponent<Mapa>();
            mapa.piso = piso;
            mapa.Construir();

            jugador = Jugador.Crear(raiz, PuntoDeLlegada());
            Historia.Construir(this);
            Guardado.Montar(raiz, piso);        // máquina de escribir para guardar la partida
            if (!horneando) Distracciones.Montar(raiz, piso);   // botellas para distraer al Pepino
            if (!horneando) Sustos.Montar(raiz, this, piso);    // sustos al azar, mensajes en las paredes, espejo
            // La entrada de la Unifranz (de noche para el menú y de día para la llegada)
            if (piso == 0 && horneando)
            {
                FachadaUnifranz.Construir(null, FachadaUnifranz.OrigenNoche, true);
                FachadaUnifranz.Construir(null, FachadaUnifranz.OrigenDia, false);
            }
        }
        else if (tipoEscena == TipoEscena.Cuarto)
        {
            MundoCuarto.Construir(raiz);
        }
        else if (tipoEscena == TipoEscena.Pantallas)
        {
            // Las pantallas se dibujan desde el editor (Pantallas.Construir)
        }
        else if (tipoEscena == TipoEscena.Menu)
        {
            // La entrada de la Unifranz de noche (el fondo del menú) queda dibujada en la escena
            if (horneando)
            {
                FachadaUnifranz.Construir(null, FachadaUnifranz.OrigenNoche, true);
                // Figuras de muestra (en el juego las pone la pantalla de inicio)
                Horno.Suspender();
                Vector2 o = FachadaUnifranz.OrigenNoche;
                Util.Objeto("Pepino bajo el farol", raiz, Arte.Pepino().Frente, o + new Vector2(5.6f, -0.9f));
                SpriteRenderer luz = Util.Plano("Luz del farol", raiz, Arte.Luz(), o + new Vector2(5.6f, 0.6f), 30001);
                luz.color = new Color(1f, 0.85f, 0.55f, 0.35f);
                luz.transform.localScale = new Vector3(2.4f, 2.4f, 1f);
                Horno.Reanudar();
            }
        }
        else if (tipoEscena == TipoEscena.Galeria)
        {
            if (horneando) Galeria.Hornear(piso, raiz);
        }
        else if (tipoEscena == TipoEscena.Persecucion)
        {
            // En el juego, el nivel se arma al empezar la persecución (FlujoPersecucion).
            if (horneando)
                for (int p = 1; p < Mapa.Pisos; p++) Persecucion.Hornear(EscenarioPersecucion.Pasillo, p, raiz);
        }
        else
        {
            bienvenida = EscenaBienvenida.Crear(this, raiz);
            jugador = Jugador.Crear(raiz, bienvenida.Entrada);
            bienvenida.Poblar();
            if (horneando) Persecucion.Hornear(EscenarioPersecucion.Auditorio, 0, raiz);
        }
    }

    Vector2 PuntoDeLlegada()
    {
        if (Partida.Llegada == "guardado") return Partida.PosGuardada;
        if (Partida.Llegada == "ascensor") return Mapa.LlegadaAscensor(piso);
        if (Partida.Llegada == "inicio" && piso == 0) return mapa.PuntoInicio;
        if (Partida.Llegada == "cuarto" && piso == 0) return mapa.Salas["seguridad"].P(4.4f, 1.8f);
        return Mapa.LlegadaEscaleras(piso);
    }

    IEnumerator FlujoPersecucion()
    {
        int p = Mathf.Clamp(Partida.PisoPersecucion, 1, Mapa.Pisos - 1);
        Resultado<bool> r = new Resultado<bool>();
        yield return Persecucion.Jugar(EscenarioPersecucion.Pasillo, p, r);
        if (!r.valor)
        {
            Morir("El Pepino te alcanzó al final del pasillo. Lo último que viste fue su sonrisa pintada.", null, TipoMuerte.Cuchillo);
            yield break;
        }
        Memoria.Escapes++;
        if (Memoria.Escapes >= 3) Logros.Desbloquear("escapista");
        Partida.PepinoAturdido = true;
        Partida.IrAPiso(p, "escape");
    }

    // El cuarto secreto: una noche y de vuelta (a la Planta Baja o al menú)
    IEnumerator FlujoCuarto()
    {
        camara.transform.position = new Vector3(MundoCuarto.Centro.x, MundoCuarto.Centro.y, -10f);
        camara.orthographicSize = MundoCuarto.Zoom;
        yield return null;
        Resultado<bool> r = new Resultado<bool>();
        yield return StartCoroutine(NocheMonitoreo.Jugar(Partida.NocheCuarto, r));
        ui.Telon(1f, 0.5f);
        yield return new WaitForSeconds(0.6f);
        if (Partida.CuartoDesdeMenu)
        {
            Partida.CuartoDesdeMenu = false;
            Partida.IrAlMenu();                     // vuelve a la pantalla de inicio
            yield break;
        }
        Partida.ResultadoCuarto = r.valor ? 1 : 2;
        Hora = Mathf.Min(Hora + 1f, horaFin - 0.05f);  // dentro del cuarto pasó una hora
        Partida.IrAPiso(0, "cuarto");
    }

    void PrepararCamara()
    {
        camara = Camera.main;
        if (camara == null)
        {
            GameObject go = new GameObject("Main Camera");
            go.tag = "MainCamera";
            camara = go.AddComponent<Camera>();
        }
        if (camara.GetComponent<AudioListener>() == null) camara.gameObject.AddComponent<AudioListener>();
        camara.orthographic = true;
        camara.orthographicSize = zoomCamara;
        camara.clearFlags = CameraClearFlags.SolidColor;
        camara.backgroundColor = Util.Hex("#0b0b10");
    }

    // El día vuelve a empezar: se carga la Planta Baja (todo vuelve a su lugar).
    // Lo aprendido sigue en Memoria.
    public void Reiniciar()
    {
        StopAllCoroutines();
        if (Partida.EnMenu && usarMenuInicio && Partida.PuedeCargar(Partida.EscenaMenu))
        {
            Partida.IrAlMenu();                     // partida terminada o abandonada: a la pantalla de inicio
            return;
        }
        if (Partida.PuedeCargar(Partida.EscenasPiso[0]))
        {
            Partida.ReiniciarDia();
            return;
        }
        // Sin las escenas en Build Settings: se reconstruye esta misma escena.
        Partida.NuevoDia();
        if (Raiz != null) Destroy(Raiz.gameObject);
        Construir();
    }

    void OnDestroy()
    {
        if (I == this) I = null;
    }

    // =================================================================
    //  DIBUJAR EL CAMPUS EN LA ESCENA (lo usa el editor)
    //  Construye todo una vez con el Horno encendido: cada objeto queda marcado
    //  con su id para que el juego lo reutilice al darle Play.
    // =================================================================
    public static GameObject Hornear(TipoEscena tipo, int piso)
    {
        Horno.Olvidar();
        Horno.Horneando = true;
        GameObject raizH = new GameObject(Horno.NombreRaiz);
        GameObject goTmp = new GameObject("tmp");
        Juego anterior = I;
        try
        {
            Juego j = goTmp.AddComponent<Juego>();
            I = j;
            j.tipoEscena = tipo;
            j.piso = piso;
            j.Raiz = raizH.transform;
            j.Hora = j.horaInicio;
            j.ConstruirContenido(raizH.transform, true);
        }
        finally
        {
            I = anterior;
            Horno.Horneando = false;
            if (Application.isPlaying) Destroy(goTmp); else DestroyImmediate(goTmp);
        }
        return raizH;
    }

    // ---------------------------------------------------------------
    void Update()
    {
        if (Raiz == null) return;

        if (Dificultad.Contrarreloj && Dificultad.Elegida && !Terminado && ui != null && !ui.PantallaVisible)
            Dificultad.TiempoTotal += Time.unscaledDeltaTime;

        // Galería de pantallas: flechas para mover la cámara, Z / X para acercar o alejar
        if (tipoEscena == TipoEscena.Pantallas && camara != null)
        {
            Vector3 d = Vector3.zero;
            if (Entrada.Presionada(Tecla.Izquierda)) d.x -= 1f;
            if (Entrada.Presionada(Tecla.Derecha)) d.x += 1f;
            if (Entrada.Presionada(Tecla.Arriba)) d.y += 1f;
            if (Entrada.Presionada(Tecla.Abajo)) d.y -= 1f;
            camara.transform.position += d * camara.orthographicSize * 1.2f * Time.deltaTime;
            if (Entrada.Presionada(Tecla.Interactuar)) camara.orthographicSize = Mathf.Max(3f, camara.orthographicSize - 12f * Time.deltaTime);
            if (Entrada.Presionada(Tecla.Accion)) camara.orthographicSize = Mathf.Min(30f, camara.orthographicSize + 12f * Time.deltaTime);
        }

        // P: pausa (opciones, volver al menú)
        if (!pausado && !Terminado && ui != null && !ui.PantallaVisible && !Cine.Activa && tipoEscena != TipoEscena.Galeria && tipoEscena != TipoEscena.Menu && tipoEscena != TipoEscena.Pantallas &&
            (!Bloqueado || tipoEscena == TipoEscena.Cuarto || tipoEscena == TipoEscena.Persecucion) && Entrada.Bajada(Tecla.Pausa))
            StartCoroutine(Pausa());

        if (tipoEscena == TipoEscena.Piso && !Terminado) RevisarSimulacro();
        ActualizarSonido();

        tRevisarLogros -= Time.deltaTime;
        if (tRevisarLogros <= 0f)
        {
            tRevisarLogros = 1f;
            Logros.Revisar();
            if (!fantasmaRevisado && Hora >= horaBienvenida)
            {
                fantasmaRevisado = true;
                if (!Partida.VistaHoy && Memoria.Cuantos(Ids.Sellos) >= 4) Logros.Desbloquear("fantasma");
            }
        }

        if (!Bloqueado && !RelojDetenido && tipoEscena != TipoEscena.Cuarto && tipoEscena != TipoEscena.Galeria && tipoEscena != TipoEscena.Menu && tipoEscena != TipoEscena.Pantallas)
        {
            float horasPorSegundo = (horaFin - horaInicio) / (minutosRealesPorDia * 60f);
            Hora += horasPorSegundo * Time.deltaTime;
            if (Hora >= horaFin)
            {
                Hora = horaFin;
                Morir("La Bienvenida terminó... y tú no estabas. El Pepino te encontró sola en la salida.", null);
            }
        }
    }

    // Límites del mapa: la cámara no muestra lo que hay fuera del edificio
    Rect limitesCamara;
    bool hayLimites;

    void CalcularLimites()
    {
        hayLimites = false;
        if (tipoEscena == TipoEscena.Piso && mapa != null)
        {
            bool primero = true;
            Rect r = new Rect();
            foreach (Sala s in mapa.Salas.Values)
            {
                if (Mapa.PisoDe(s.Centro) != piso) continue;
                r = primero ? s.area : Rect.MinMaxRect(Mathf.Min(r.xMin, s.area.xMin), Mathf.Min(r.yMin, s.area.yMin),
                                                       Mathf.Max(r.xMax, s.area.xMax), Mathf.Max(r.yMax, s.area.yMax));
                primero = false;
            }
            if (primero) return;
            Vector2 e = Mapa.Escaleras(piso);
            r = Rect.MinMaxRect(Mathf.Min(r.xMin, e.x - 2f), r.yMin, r.xMax, r.yMax);
            float abajo = piso == 0 ? Mathf.Min(r.yMin, Mapa.Origen(0).y - 21f) : r.yMin;   // la PB incluye la vereda y la calle
            limitesCamara = Rect.MinMaxRect(r.xMin - 0.8f, abajo - 0.6f, r.xMax + 0.8f, r.yMax + 0.8f);
            hayLimites = true;
        }
        else if (tipoEscena == TipoEscena.Bienvenida)
        {
            limitesCamara = new Rect(-EscenaBienvenida.Ancho / 2f - 1f, -EscenaBienvenida.Alto / 2f - 1f, EscenaBienvenida.Ancho + 2f, EscenaBienvenida.Alto + 2f);
            hayLimites = true;
        }
    }

    Vector3 Limitar(Vector3 p)
    {
        if (!hayLimites || camara == null) return p;
        float alto = camara.orthographicSize, ancho = alto * camara.aspect;
        p.x = limitesCamara.width <= ancho * 2f ? limitesCamara.center.x : Mathf.Clamp(p.x, limitesCamara.xMin + ancho, limitesCamara.xMax - ancho);
        p.y = limitesCamara.height <= alto * 2f ? limitesCamara.center.y : Mathf.Clamp(p.y, limitesCamara.yMin + alto, limitesCamara.yMax - alto);
        return p;
    }

    void LateUpdate()
    {
        if (camara != null && (jugador != null || camaraSeguir != null))
        {
            Vector3 objetivo = camaraSeguir != null ? camaraSeguir.position : jugador.transform.position;
            if (camaraSeguir == null) objetivo = Limitar(objetivo);    // en cinemáticas la cámara va libre
            objetivo.z = -10f;
            Vector3 actual = camara.transform.position;
            // Si cambia de piso (salto grande), mover la cámara de inmediato.
            if ((actual - objetivo).sqrMagnitude > 400f) camara.transform.position = objetivo;
            else camara.transform.position = Vector3.Lerp(actual, objetivo, 1f - Mathf.Exp(-8f * Time.deltaTime));
        }
    }

    // ---------- Música y ambiente según lo que pasa ----------
    void ActualizarSonido()
    {
        if (Terminado || Cine.Activa || ui == null || ui.PantallaVisible || tipoEscena == TipoEscena.Cuarto || tipoEscena == TipoEscena.Galeria || tipoEscena == TipoEscena.Menu || tipoEscena == TipoEscena.Pantallas) return;
        bool alarma = Partida.Evento == "simulacro" && Partida.SimulacroEstado == 1 && tipoEscena == TipoEscena.Piso;
        bool sinLuz = Partida.Evento == "apagon" && Partida.PisoApagon == piso && tipoEscena == TipoEscena.Piso;
        if (alarma) Sonido.Ambiente("alarma", 0.3f);
        else if (tipoEscena == TipoEscena.Piso && !sinLuz) Sonido.Ambiente("zumbido", 0.035f);
        else Sonido.Ambiente(null, 0f);

        if (tipoEscena == TipoEscena.Persecucion) return;           // la persecución pone su música
        if (Bloqueado && Sonido.TemaActual == "m_persecucion") return;
        string tema;
        if (tipoEscena == TipoEscena.Bienvenida) tema = "m_morenada";
        else if (Pepino.Cazando > 0) tema = "m_tension";
        else if (Hora >= 17f || sinLuz) tema = "m_noche";
        else tema = "m_campus";
        Sonido.Musica(tema, tema == "m_tension" ? 0.6f : 2f);
    }

    // ---------- Evento: simulacro de evacuación ----------
    void RevisarSimulacro()
    {
        if (Partida.Evento != "simulacro" || Bloqueado) return;
        if (Partida.SimulacroEstado == 0 && Hora >= Partida.HoraSimulacro)
        {
            Partida.SimulacroEstado = 1;
            Partida.SimulacroLimite = Hora + 0.5f * Dificultad.FactorTiempo;
            if (piso == 0) { Evacuado(); return; }
            ui.Golpe(0.4f);
            ui.Destello(new Color(1f, 0.1f, 0.1f), 0.35f);
            ui.Toast("¡ALARMA! SIMULACRO DE EVACUACIÓN. Todos bajan a la Planta Baja... ¡ve tú también!", 6f);
        }
        else if (Partida.SimulacroEstado == 1)
        {
            if (piso == 0) Evacuado();
            else if (Hora >= Partida.SimulacroLimite)
                Morir("El simulacro terminó. Todo el edificio salió a la calle... menos tú.\nEn el piso vacío, solo se escuchaban unos pasos con zapatillas de colores.",
                      "simulacro", TipoMuerte.Cuchillo);
        }
    }

    void Evacuado()
    {
        Partida.SimulacroEstado = 2;
        ui.Toast("Evacuaste a tiempo. El simulacro terminó y todos vuelven a entrar.", 4f);
        Logros.Desbloquear("evacuacion");
    }

    public void AvanzarReloj(float horas)
    {
        Hora = Mathf.Min(Hora + horas, horaFin - 0.01f);
    }

    // ---------------------------------------------------------------
    public void Bloquear(object quien) { bloqueos.Add(quien); }

    public void Desbloquear(object quien)
    {
        if (bloqueos.Remove(quien) && bloqueos.Count == 0) FrameDesbloqueo = Time.frameCount;
    }

    // ---------------------------------------------------------------
    IEnumerator Intro()
    {
        if (!(Partida.DiaNuevo && piso == 0))
        {
            // Llegas a otro piso en el mismo día
            yield return null;
            AplicarApagon();
            if (Partida.Llegada == "guardado")
            {
                Sonido.Efecto("sello", 0.5f, 1.4f);
                ui.Toast("PARTIDA CARGADA  ·  " + Protagonista.Nombre + "  ·  Día " + Memoria.Dia + "  ·  " + HoraTexto + "\n" + Partida.TextoEvento(), 5f);
            }
            else if (Partida.Llegada == "escape")
                ui.Toast("¡Escapaste! Llegaste a las escaleras del " + Mapa.NombresPiso[piso] + ". El Pepino perdió tu rastro... por ahora.", 4f);
            else
                ui.Toast(Mapa.NombresPiso[piso].ToUpper() + "  ·  " + NombreFacultadPiso(piso), 3f);
            yield break;
        }
        Bloquear("intro");
        if (Partida.IntroPendiente)
        {
            // Se recargó la escena con el personaje elegido (Mateo): su prólogo
            Partida.IntroPendiente = false;
            ui.Telon(1f, 0f);
            if (Protagonista.EsMateo) yield return StartCoroutine(CineMateo.Prologo(this));
            else yield return StartCoroutine(CineHistoria.Intro(this));
        }
        else if (Memoria.Dia == 1 && !Dificultad.Elegida && usarMenuInicio && Partida.PuedeCargar(Partida.EscenaMenu))
        {
            // Se dio Play desde la Planta Baja: el menú tiene su propia escena (00_Menu)
            ui.Telon(1f, 0f);
            Partida.IrAlMenu();
            yield break;
        }
        else if (Memoria.Dia == 1 && !Dificultad.Elegida && usarMenuInicio)
        {
            yield return StartCoroutine(MenuInicio.Mostrar(this));
            yield return StartCoroutine(ElegirDificultad());
            ui.OcultarPantalla();
            if (Protagonista.EsMateo)
            {
                // El campus se armó para Valeria: se vuelve a cargar con la historia de Mateo
                Partida.IntroPendiente = true;
                Partida.ReiniciarDia();
                yield break;
            }
            yield return StartCoroutine(CineHistoria.Intro(this));
        }
        else if (Memoria.Dia == 1 && !Dificultad.Elegida)
        {
            ui.MostrarPantalla("FELIZ PRIMER DÍA",
                "Valeria Quispe tiene 18 años y todavía no sabe qué estudiar.\n" +
                "Hoy empieza la Semana de Inducción en Unifranz La Paz.\n\n" +
                "Consigue los 4 sellos del Pasaporte de Carreras\n" +
                "(Medicina, Derecho, Periodismo e Ingeniería de Sistemas)\n" +
                "y entra a la Bienvenida a los Cachimbos a las 18:00.\n\n" +
                "<size=26><color=#aaaaaa>WASD / Flechas: moverse   ·   Shift: correr   ·   E: interactuar\n" +
                "Tab: cuaderno de pistas   ·   1-4: elegir opciones</color></size>",
                "Presiona E para empezar", Util.Hex("#07060b"), 1);
            yield return new WaitForSeconds(0.4f);
            while (!Entrada.Avanzar()) yield return null;
            yield return StartCoroutine(ElegirDificultad());
        }
        else
        {
            string bucles = Dificultad.BuclesMax > 0
                ? "\n<color=#ff4d4d>BUCLE " + Memoria.Dia + " DE " + Dificultad.BuclesMax + "</color>" : "";
            ui.MostrarPantalla("6:45",
                "La alarma suena otra vez.\nEl mismo día. La misma mañana.\n\n" +
                "<color=#ff6b6b>DÍA " + Memoria.Dia + "</color>" + bucles + "\n\n" +
                TextoDelDia() +
                "<size=26><color=#aaaaaa>Lo que aprendiste sigue en tu cuaderno (Tab).\nLos sellos y pruebas, no.</color></size>",
                "Presiona E", Util.Hex("#000000"));
            yield return new WaitForSeconds(0.4f);
            while (!Entrada.Avanzar()) yield return null;
        }
        ui.OcultarPantalla();
        Desbloquear("intro");
        AplicarApagon();
        string evento = Partida.TextoEvento();
        string inicio = Protagonista.EsMateo
            ? "Recupera los 4 videos en las computadoras y consigue aliados. A las 18:00: la cabina del auditorio."
            : "Estás en el Ingreso. Habla con la gente (E). El auditorio abre a las 18:00.";
        if (Tutorial.Toca(this)) { StartCoroutine(Tutorial.Jugar(this)); yield break; }
        ui.Toast(evento != "" ? evento : inicio, 6f);
    }

    // Lo que cambió hoy: evento al azar y lo que el Pepino aprendió.
    string TextoDelDia()
    {
        string t = "";
        string ev = Partida.TextoEvento();
        if (ev != "") t += "<size=28><color=#ffd166>" + ev + "</color></size>\n";
        if (Dificultad.Escalada > 0f)
        {
            if (Memoria.Dia == 3) t += "<size=26><color=#ff8080>El Pepino aparece más temprano cada día.</color></size>\n";
            if (Memoria.Escapes >= 2 && Memoria.Dia >= 2) t += "<size=26><color=#ff8080>El Pepino ahora vigila las escaleras.</color></size>\n";
            if (Memoria.Muertes >= 3) t += "<size=26><color=#ff8080>El Pepino es más rápido y te busca por más tiempo.</color></size>\n";
        }
        return t == "" ? "" : t + "\n";
    }

    void AplicarApagon()
    {
        bool apagon = Partida.Evento == "apagon" && Partida.PisoApagon == piso && tipoEscena == TipoEscena.Piso;
        ui.OscuridadBase(apagon ? 0.55f : 0f);
        if (apagon) Sonido.Efecto("apagon", 0.7f, 1f);
    }

    // Menú de dificultad y modo (solo al empezar una partida nueva).
    IEnumerator ElegirDificultad()
    {
        // Personaje (Mateo aparece cuando ya lo desbloqueaste)
        Protagonista.Actual = Quien.Valeria;
        if (Protagonista.MateoDesbloqueado)
        {
            ui.MostrarPantalla("PERSONAJE",
                "<color=#c77dff><b>1.  VALERIA QUISPE</b></color>\n<size=26><color=#bbbbbb>La historia principal. Su primer día... una y otra vez.</color></size>\n\n" +
                "<color=#4cc9f0><b>2.  MATEO CHOQUE</b></color>  <size=26><color=#ffd166>NIVEL EXTRA</color></size>\n<size=26><color=#bbbbbb>Ing. de Sistemas. Vio todo por las cámaras... y no hizo nada.\nHackea computadoras, recupera los videos y enfrenta al quinto disfraz.</color></size>",
                "Presiona 1 o 2", Util.Hex("#07060b"));
            yield return new WaitForSeconds(0.3f);
            int p = 0;
            while (p < 1 || p > 2) { p = Entrada.NumeroBajado(); yield return null; }
            Protagonista.Actual = p == 2 ? Quien.Mateo : Quien.Valeria;
        }

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        for (int i = 0; i < 4; i++)
        {
            int reto = (int)Dificultad.RetoDe((Nivel)i);
            sb.Append("<color=").Append(Dificultad.Colores[i]).Append("><b>").Append(i + 1).Append(".  ").Append(Dificultad.Nombres[i])
              .Append("</b></color>   <size=22><color=").Append(Dificultad.ColoresReto[reto]).Append(">· Retos ").Append(Dificultad.NombresReto[reto])
              .Append("</color></size>\n<size=24><color=#bbbbbb>").Append(Dificultad.Descripciones[i]).Append("</color></size>\n\n");
        }
        ui.MostrarPantalla("DIFICULTAD", sb.ToString(), "Presiona 1, 2, 3 o 4", Util.Hex("#07060b"));
        yield return new WaitForSeconds(0.3f);
        int n = 0;
        while (n < 1 || n > 4) { n = Entrada.NumeroBajado(); yield return null; }
        Dificultad.Actual = (Nivel)(n - 1);

        float rec = Dificultad.Record;
        ui.MostrarPantalla("MODO DE JUEGO",
            "<b>1.  HISTORIA</b>\n<size=26><color=#bbbbbb>Juega a tu ritmo.</color></size>\n\n" +
            "<b>2.  CONTRARRELOJ</b>\n<size=26><color=#bbbbbb>Rompe el bucle lo más rápido posible. El tiempo corre de verdad\n(también mientras hablas y en los minijuegos) y se guarda tu récord.</color></size>\n\n" +
            (rec > 0f ? "<size=26><color=#8ecae6>Récord en " + Dificultad.Nombre + ": " + Dificultad.Formato(rec) + "</color></size>\n\n" : "") +
            "<size=26>Dificultad: <color=" + Dificultad.ColorHex + ">" + Dificultad.Nombre + "</color>" +
            "   ·   Retos de las carreras: <color=" + Dificultad.ColorReto + ">" + Dificultad.NombreReto + "</color></size>",
            "Presiona 1 o 2", Util.Hex("#07060b"));
        yield return new WaitForSeconds(0.3f);
        n = 0;
        while (n < 1 || n > 2) { n = Entrada.NumeroBajado(); yield return null; }
        Dificultad.Contrarreloj = n == 2;
        Dificultad.Elegida = true;
        Dificultad.TiempoTotal = 0f;
    }

    public static string NombreFacultadPiso(int p)
    {
        switch (p)
        {
            case 1: return "Derecho · Psicología · Ciencias Empresariales";
            case 2: return "Periodismo · Diseño Gráfico · Hotelería y Turismo";
            case 3: return "Medicina · Odontología · Bioquímica y Farmacia";
            case 4: return "Ingeniería de Sistemas · Ingeniería Comercial";
        }
        return "Ingreso · Patio · Cafetería · Biblioteca · Auditorio";
    }

    // ---------- Pantalla de inicio (escena 00_Menu) ----------
    IEnumerator FlujoMenu()
    {
        Bloquear("menu");
        yield return null;
        yield return StartCoroutine(MenuInicio.Mostrar(this));
        yield return StartCoroutine(ElegirDificultad());
        ui.Telon(1f, 0f);
        ui.OcultarPantalla();
        // Al cargar la Planta Baja se muestra la llegada de Valeria (o el prólogo de Mateo)
        Partida.IntroPendiente = true;
        Partida.ReiniciarDia();
    }

    // ---------- Pausa ----------
    bool pausado;
    public bool Pausado { get { return pausado; } }

    IEnumerator Pausa()
    {
        pausado = true;
        float escalaAntes = Time.timeScale;
        Time.timeScale = 0f;
        Bloquear("pausa");
        Resultado<int> r = new Resultado<int>();
        Resultado<int> esp = new Resultado<int>();
        bool puedeGuardar = tipoEscena == TipoEscena.Piso && jugador != null;
        string aviso = "";
        bool salir = false;
        while (true)
        {
            yield return StartCoroutine(PanelPausa.Abrir(ui.transform, r, puedeGuardar, aviso));
            aviso = "";
            if (r.valor == 2) { yield return StartCoroutine(PanelOpciones.Abrir(ui.transform)); continue; }
            if (r.valor == 1 || r.valor == 3)
            {
                yield return StartCoroutine(PanelEspacios.Abrir(ui.transform, r.valor == 1 ? "GUARDAR PARTIDA" : "GUARDAR Y VOLVER AL MENÚ", esp));
                if (esp.valor == 0) continue;
                if (!Guardado.Guardar(esp.valor)) { aviso = "No se pudo guardar."; continue; }
                try { PlayerPrefs.SetInt("fpd_ultimo_espacio", esp.valor); } catch { }
                Sonido.Efecto("sello", 0.5f, 1.4f);
                aviso = "Partida guardada en el espacio " + esp.valor + ".";
                if (r.valor == 3) { salir = true; break; }
                continue;
            }
            if (r.valor == 4) salir = true;
            break;
        }
        Time.timeScale = escalaAntes <= 0f ? 1f : escalaAntes;
        Desbloquear("pausa");
        pausado = false;
        if (salir) VolverAlMenu();
    }

    // Deja la partida y vuelve a la pantalla de inicio (lo que no guardaste se pierde)
    public void VolverAlMenu()
    {
        StopAllCoroutines();
        Time.timeScale = 1f;
        Memoria.BorrarTodo();
        Dificultad.Reiniciar();
        Protagonista.Actual = Quien.Valeria;
        Partida.IntroPendiente = false;
        Partida.CuartoDesdeMenu = false;
        Partida.ResultadoCuarto = 0;
        Reiniciar();
    }

    // ---------------------------------------------------------------
    public void Morir(string motivo, string recuerdo) { Morir(motivo, recuerdo, TipoMuerte.Cuchillo); }

    public void Morir(string motivo, string recuerdo, TipoMuerte tipo)
    {
        if (Terminado) return;
        Terminado = true;
        Memoria.Muertes++;
        Logros.RegistrarMuerte(tipo);
        bool nuevo = Memoria.Aprender(recuerdo);
        if (!nuevo && Dificultad.PistaExtra)
        {
            // Cachimbo: cada muerte te regala una pista que todavía no sabías
            foreach (string id in PistasDeAyuda)
                if (Memoria.Aprender(id)) { recuerdo = id; nuevo = true; break; }
        }
        StartCoroutine(SecuenciaMuerte(motivo, nuevo ? recuerdo : null, tipo));
    }

    static readonly string[] PistasDeAyuda = { "escaleras", "sis_ups", "med_maniqui", "der_estrado", "per_reflector", "quien_es", "natita", "pepino_aprende" };

    // Pesadilla: ¿ya se usaron todos los bucles?
    bool SinBucles { get { return Dificultad.BuclesMax > 0 && Memoria.Dia >= Dificultad.BuclesMax; } }

    IEnumerator FinalPesadilla()
    {
        ui.OcultarPantalla();
        yield return StartCoroutine(CineFinales.Reproducir("final_pesadilla"));
        yield return StartCoroutine(SecuenciaFinal("FINAL PESADILLA\nEL ÚLTIMO BUCLE",
            "La alarma no suena.\nNo hay 6:45. No hay jueves. No hay nada.\n\n" +
            "En el Patio, alguien deja un ramo de flores junto a una foto tuya.\n" +
            "Camila lo mira desde lejos, con la máscara en la mano.\n\n" +
            "\"Ya no recuerdo cuántas veces fueron\", dice. \"Solo que la última no volvió.\"",
            true, "final_pesadilla"));
    }

    IEnumerator SecuenciaMuerte(string motivo, string recuerdoNuevo, TipoMuerte tipo)
    {
        ui.Prompt(null);
        if (jugador != null)
            Util.Plano("Sangre", Raiz, Arte.Sangre(Memoria.Muertes % 4), jugador.Posicion + new Vector2(0f, -0.6f), -950);
        yield return StartCoroutine(CinematicaMuerte.Reproducir(ui, tipo));
        string cuerpo = motivo;
        if (recuerdoNuevo != null && Pistas.Existe(recuerdoNuevo))
            cuerpo += "\n\n<size=28><color=#ffd166>Nuevo recuerdo: " + Pistas.Catalogo[recuerdoNuevo].titulo + "</color></size>";
        ui.MostrarPantalla("MORISTE", cuerpo, "Presiona E", Util.Hex("#2a0005"), 2);
        yield return new WaitForSeconds(1.2f);
        while (!Entrada.Avanzar()) yield return null;
        if (SinBucles) { yield return StartCoroutine(FinalPesadilla()); yield break; }
        ui.MostrarPantalla("6:45", "", "", Util.Hex("#000000"));
        yield return new WaitForSeconds(1.2f);
        Memoria.NuevoDia();
        Reiniciar();
    }

    // ---------------------------------------------------------------
    // rompeElLoop = true: final definitivo (R para jugar de nuevo).
    // rompeElLoop = false: final malo, el día vuelve a empezar.
    public void Final(string titulo, string texto, bool rompeElLoop) { Final(titulo, texto, rompeElLoop, null); }

    public void Final(string titulo, string texto, bool rompeElLoop, string logro)
    {
        if (Terminado) return;
        Terminado = true;
        StartCoroutine(SecuenciaFinal(titulo, texto, rompeElLoop, logro));
    }

    IEnumerator SecuenciaFinal(string titulo, string texto, bool rompeElLoop, string logro)
    {
        if (logro != null) Logros.Desbloquear(logro);
        bool mateoNuevo = false;
        Sonido.Latido(0f);
        Sonido.Ambiente(null, 0f);
        Sonido.Musica(logro == "final_oscuro" || logro == "final_malo" || logro == "final_pesadilla" ? "m_oscuro" : "m_triste", 1.5f);
        bool gano = rompeElLoop && logro != "final_pesadilla";
        string pie = "Presiona E";
        if (rompeElLoop)
        {
            pie = "FIN  ·  " + Dificultad.Nombre + "  ·  Días vividos: " + Memoria.Dia + "  ·  Muertes: " + Memoria.Muertes;
            if (gano && Dificultad.Actual == Nivel.Pesadilla) Logros.Desbloquear("pesadilla");
            if (Dificultad.Contrarreloj && gano)
            {
                Logros.Desbloquear("contrarreloj");
                bool record = Dificultad.GuardarRecord();
                pie += "\nTIEMPO: " + Dificultad.Formato(Dificultad.TiempoTotal) + (record ? "   <color=#7bd389>¡NUEVO RÉCORD!</color>"
                       : "   (récord: " + Dificultad.Formato(Dificultad.Record) + ")");
            }
            if (gano && !Protagonista.EsMateo && (logro == "final_bueno" || logro == "final_verdadero" || logro == "final_natita" || logro == "final_oscuro"))
            {
                mateoNuevo = Protagonista.DesbloquearMateo();
                if (mateoNuevo) { Logros.Desbloquear("mateo"); pie += "\n<color=#4cc9f0>¡NUEVO PERSONAJE DESBLOQUEADO: MATEO!</color>"; }
            }
            pie += "\nLogros: " + Logros.Conseguidos + "/" + Logros.Lista.Count + "   ·   Presiona R para jugar de nuevo";
        }
        ui.MostrarPantalla(titulo, texto, pie, rompeElLoop ? Util.Hex("#0d1b2a") : Util.Hex("#2b0000"));
        yield return new WaitForSeconds(1.5f);
        if (rompeElLoop)
        {
            while (!Entrada.Bajada(Tecla.Reiniciar)) yield return null;
            if (gano)
            {
                ui.OcultarPantalla();
                yield return StartCoroutine(Creditos.Rodar(ui.transform));
            }
            if (mateoNuevo) yield return StartCoroutine(CineMateo.Desbloqueo(this));
            Memoria.BorrarTodo();
            Dificultad.Reiniciar();
            Protagonista.Actual = Quien.Valeria;
        }
        else
        {
            while (!Entrada.Avanzar()) yield return null;
            if (SinBucles) { yield return StartCoroutine(FinalPesadilla()); yield break; }
            Memoria.NuevoDia();
        }
        Reiniciar();
    }
}
