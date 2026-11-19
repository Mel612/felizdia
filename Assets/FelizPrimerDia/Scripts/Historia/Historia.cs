using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Arma la historia sobre el mapa: personajes de la Planta Baja, escaleras, ascensor,
// casilleros, los Pepinos de los pasillos, las 4 facultades y el final en el auditorio.
public partial class Historia : MonoBehaviour
{
    Juego juego;
    bool ocupado;
    Pepino pepinoPiso;
    Vector2 posNatita;

    Dialogo D { get { return juego.dialogo; } }
    UIJuego UI { get { return juego.ui; } }
    Jugador J { get { return juego.jugador; } }

    public static Historia Construir(Juego j)
    {
        GameObject go = new GameObject("Historia");
        go.transform.SetParent(j.Raiz, false);
        Historia h = go.AddComponent<Historia>();
        h.juego = j;
        Horno.Empezar("Historia");
        try { h.Montar(); }
        finally { Horno.Terminar(); }
        return h;
    }

    static SpritesPersonaje Persona(string ropa, string piel, string pelo, string pantalon, bool largo, Accesorio acc, string accHex)
    {
        return Arte.Personaje(Arte.Ap(ropa, piel, pelo, pantalon, largo, acc, accHex));
    }

    void Lanzar(IEnumerator rutina)
    {
        if (ocupado || juego.Terminado) return;
        StartCoroutine(Envolver(rutina));
    }

    IEnumerator Envolver(IEnumerator r)
    {
        ocupado = true;
        yield return StartCoroutine(r);
        ocupado = false;
    }

    // =================================================================
    void Montar()
    {
        Mapa m = juego.mapa;
        Transform t = transform;
        int piso = juego.piso;

        // Nivel extra: la historia de Mateo (HistoriaMateo.cs)
        if (Protagonista.EsMateo) { MontarMateo(m, t, piso); return; }

        if (piso == 0) MontarPlantaBaja(m, t);

        // ---------- Escaleras y ascensor ----------
        Interactuable.Crear(t, Mapa.Escaleras(piso), "Usar las escaleras", delegate { Lanzar(UsarEscaleras(piso)); }, 1.6f);
        Interactuable.Crear(t, Mapa.Ascensor(piso), "Usar el ascensor", delegate { Lanzar(UsarAscensor(piso)); }, 1.4f);

        // ---------- Casilleros y el Pepino de este piso ----------
        if (piso > 0)
        {
            float[] xs = { -13f, 3f, 15f };
            foreach (float x in xs) CrearCasillero(t, Mapa.Origen(piso) + new Vector2(x, -2.1f));

            List<Vector2> ruta = new List<Vector2>
            {
                Mapa.Origen(piso) + new Vector2(-12f, 0.9f), Mapa.Origen(piso) + new Vector2(17f, 0.9f),
                Mapa.Origen(piso) + new Vector2(17f, -1.6f), Mapa.Origen(piso) + new Vector2(-12f, -1.6f)
            };
            Pepino pep = Pepino.Crear(t, ruta[1 + (piso % 2) * 2], ruta);
            pep.velocidadPatrulla = 1.6f + piso * 0.1f;
            pepinoPiso = pep;
        }

        // ---------- Facultad principal del piso ----------
        if (piso == 1) CrearFacultad<FacDerecho>(m.Salas["derecho"]);
        else if (piso == 2) CrearFacultad<FacPeriodismo>(m.Salas["periodismo"]);
        else if (piso == 3) CrearFacultad<FacMedicina>(m.Salas["medicina"]);
        else if (piso == 4) CrearFacultad<FacSistemas>(m.Salas["sistemas"]);

        // ---------- Salas de otras carreras para explorar ----------
        foreach (Exploracion.Sala ex in Exploracion.Salas)
        {
            Sala s;
            if (m.Salas.TryGetValue(ex.id, out s)) MontarExploracion(t, s, ex);
        }

        // ---------- Grabaciones de Camila, la ñatita y la ofrenda ----------
        // (van al final y en su propio grupo para no mover los objetos ya dibujados)
        Horno.Empezar("Extras");
        try { MontarExtras(m, t, piso); }
        finally { Horno.Terminar(); }
    }

    // =================================================================
    //  GRABACIONES DE CAMILA · LA ÑATITA · LA OFRENDA
    // =================================================================
    static readonly string[] SalaAudio = { "patio", "biblioteca", "psicologia", "administracion", "radio", "bioquimica", "servidores", "comercial" };
    static readonly Vector2[] PosAudio =
    {
        new Vector2(-4.5f, -2.6f), new Vector2(4.5f, 1.5f), new Vector2(-4f, -2.5f), new Vector2(-4.5f, -2.5f),
        new Vector2(4f, -2.5f), new Vector2(4.5f, 1.2f), new Vector2(4.5f, -2.5f), new Vector2(4.5f, -2.5f)
    };

    bool EnEstePiso(Mapa m, string id, out Sala s)
    {
        return m.Salas.TryGetValue(id, out s) && Mapa.PisoDe(s.Centro) == juego.piso;
    }

    void MontarExtras(Mapa m, Transform t, int piso)
    {
        Sala s;
        for (int i = 0; i < SalaAudio.Length; i++)
        {
            if (!EnEstePiso(m, SalaAudio[i], out s)) continue;
            int n = i;
            Vector2 pos = s.P(PosAudio[i].x, PosAudio[i].y);
            Util.Objeto("Grabación de Camila " + (i + 1), t, Arte.Celular(), pos + new Vector2(0f, -0.25f));
            Interactuable it = Interactuable.Crear(t, pos, "Revisar el celular olvidado", delegate { Lanzar(EscucharAudio(n)); }, 1.2f);
            it.textoDinamico = delegate { return Memoria.Sabe(Ids.Audios[n]) ? "Volver a escuchar la grabación" : "Revisar el celular olvidado"; };
        }

        // La ñatita (Odontología, Piso 3)
        if (EnEstePiso(m, "odontologia", out s))
        {
            posNatita = s.P(0f, 0.3f);
            Util.Objeto("Ñatita", t, Arte.Natita(), posNatita, new Vector2(1.2f, 0.5f));
            Interactuable.Crear(t, s.P(0f, -0.5f), "Mirar la ñatita", delegate { Lanzar(HablarNatita()); }, 1.4f);
        }

        // Ofrenda: flores del Patio
        if (EnEstePiso(m, "patio", out s))
        {
            Interactuable fl = Interactuable.Crear(t, s.P(5f, 2.2f), "Recoger flores de la jardinera", delegate
            {
                Memoria.Obtener("ofrenda_flores");
                UI.Toast("Tienes FLORES para la ñatita. (" + Memoria.Cuantos(Ids.Ofrenda) + "/3)", 3.5f);
            }, 1.2f);
            fl.disponible = delegate { return Memoria.Sabe("natita") && !Memoria.Tiene("ofrenda_flores") && !Memoria.Tiene("ofrenda_natita"); };
        }

        // El cuarto secreto (puerta sin letrero en Seguridad)
        if (EnEstePiso(m, "seguridad", out s))
        {
            Horno.Suspender();
            Util.Rect("Puerta sin letrero", t, s.P(4.4f, 3.2f), new Vector2(1.4f, 0.5f), Util.Hex("#3d2b1f"), -1370);
            Util.Rect("Manija", t, s.P(4.9f, 3.15f), new Vector2(0.12f, 0.12f), Util.Hex("#b08968"), -1365);
            Horno.Reanudar();
            Interactuable.Crear(t, s.P(4.4f, 2.5f), "Puerta sin letrero", delegate { Lanzar(CuartoSecreto()); }, 1.3f);
        }

        // Objetos de las misiones de indumentaria
        if (EnEstePiso(m, "cafeteria", out s))
        {
            Misiones.Mision toga = Misiones.DeSello(Ids.SelloDerecho), prensa = Misiones.DeSello(Ids.SelloPeriodismo);
            ObjetoMision(t, s.P(-3.2f, -1.6f), "Recoger el libro olvidado en la mesa", toga, "Es un CÓDIGO PENAL con el sello de la Biblioteca. Se lo llevo a Doña Rosa.");
            ObjetoMision(t, s.P(-4.6f, 2.2f), "Comprar un café", prensa, "Un café cargado para Joaquín. Cuidado, quema.");
        }
        if (EnEstePiso(m, "diseno", out s))
            ObjetoMision(t, s.P(1.5f, -1.2f), "Agarrar la memoria USB del escritorio", Misiones.DeSello(Ids.SelloSistemas),
                "Una memoria USB con una etiqueta: \"RESPALDO SERVIDORES — Iván\".");

        // Ofrenda: vela de Hotelería
        if (EnEstePiso(m, "hoteleria", out s))
        {
            Util.Objeto("Vela", t, Arte.Vela(), s.P(3.5f, -2.2f));
            Interactuable v = Interactuable.Crear(t, s.P(3.5f, -2f), "Tomar la vela del centro de mesa", delegate
            {
                Memoria.Obtener("ofrenda_vela");
                UI.Toast("Tienes una VELA para la ñatita. (" + Memoria.Cuantos(Ids.Ofrenda) + "/3)", 3.5f);
            }, 1.2f);
            v.disponible = delegate { return Memoria.Sabe("natita") && !Memoria.Tiene("ofrenda_vela") && !Memoria.Tiene("ofrenda_natita"); };
        }
    }

    // =================================================================
    //  EL CUARTO SECRETO (minijuego de las cámaras)
    // =================================================================
    IEnumerator CuartoSecreto()
    {
        if (!Memoria.Tiene("llave_cuarto"))
        {
            yield return D.DecirCo("Valeria", "Una puerta vieja, sin letrero, cerrada con llave.", "Por debajo se ve la luz verde de unos monitores... y se escucha un ventilador.");
            yield break;
        }
        int noche = Memoria.Sabe("noche2_ok") ? 3 : Memoria.Sabe("noche1_ok") ? 2 : 1;
        Resultado<int> r = new Resultado<int>();
        yield return D.ElegirCo("Valeria", "El viejo cuarto de monitoreo. Huele a polvo y a café frío. (Entrar te quita 1 hora del día.)",
            new[] { "Entrar y cerrar la puerta (Noche " + noche + ")", "Mejor no" }, r);
        if (r.valor != 0) yield break;

        Sonido.Efecto("puerta", 0.7f, 0.8f);
        // Encontraste el nivel escondido: desde ahora aparece en el menú de inicio
        if (NocheMonitoreo.MarcarEncontrado())
        {
            Sonido.Efecto("logro", 0.7f, 1f);
            UI.Toast("¡NIVEL ESCONDIDO ENCONTRADO!  Ahora el CUARTO SECRETO aparece en el menú de inicio.", 5f);
        }
        yield return D.DecirCo("Valeria", "Me siento frente a los monitores. Todo está en silencio...", "...sin darme cuenta, me quedo dormida.");
        UI.Telon(1f, 0.6f);
        yield return new WaitForSeconds(0.7f);
        // El cuarto es su propia escena (7_CuartoSecreto). Al terminar vuelves aquí.
        if (Partida.PuedeCargar(Partida.EscenaCuarto))
        {
            Partida.IrACuarto(noche, false);
            yield break;
        }
        UI.Telon(0f, 0.6f);
        Resultado<bool> ok = new Resultado<bool>();
        yield return StartCoroutine(NocheMonitoreo.Jugar(noche, ok));
        juego.AvanzarReloj(1f);
        if (ok.valor)
        {
            Memoria.Aprender("noche" + noche + "_ok");
            yield return D.DecirCo("Valeria", "Me despierto de golpe. Era un sueño... pero en el monitor quedó una grabación vieja.");
            yield return StartCoroutine(Recuerdos.Reproducir("camara_vieja"));
            if (Memoria.Aprender("camara_vieja")) UI.Toast("Nueva pista: " + Pistas.Catalogo["camara_vieja"].titulo, 4f);
        }
        else
            yield return D.DecirCo("Valeria", "Me despierto en el piso, sudando. Fue solo una pesadilla...", "...¿verdad?");
    }

    // Al volver del cuarto secreto (escena 7) se cuenta qué pasó
    void Start()
    {
        if (juego != null && juego.piso == 0 && Partida.ResultadoCuarto != 0) Lanzar(VolverDelCuarto());
    }

    IEnumerator VolverDelCuarto()
    {
        int res = Partida.ResultadoCuarto;
        Partida.ResultadoCuarto = 0;
        yield return new WaitForSeconds(0.5f);
        if (res == 1)
        {
            Memoria.Aprender("noche" + Partida.NocheCuarto + "_ok");
            yield return D.DecirCo("Valeria", "Me despierto de golpe. Era un sueño... pero en el monitor quedó una grabación vieja.");
            yield return StartCoroutine(Recuerdos.Reproducir("camara_vieja"));
            if (Memoria.Aprender("camara_vieja")) UI.Toast("Nueva pista: " + Pistas.Catalogo["camara_vieja"].titulo, 4f);
        }
        else
            yield return D.DecirCo("Valeria", "Me despierto en el piso del cuarto, sudando. Fue solo una pesadilla...", "...¿verdad?");
    }

    void ObjetoMision(Transform t, Vector2 pos, string accion, Misiones.Mision mis, string texto)
    {
        Misiones.Mision mm = mis;
        Interactuable it = Interactuable.Crear(t, pos, accion, delegate
        {
            Memoria.Obtener(Misiones.Objeto(mm));
            Sonido.Efecto("click", 0.7f, 1.3f);
            D.Decir("Valeria", new[] { texto });
            UI.Toast("Tienes " + mm.objeto.ToUpper() + ". Llévaselo a " + mm.guiaNombre + " (" + Misiones.NombreSala(mm.guiaSala) + ").", 4.5f);
        }, 1.2f);
        it.disponible = delegate
        {
            return Memoria.Tiene(Misiones.Activa(mm)) && !Memoria.Tiene(Misiones.Objeto(mm)) && !Memoria.Tiene(Misiones.Prenda(mm));
        };
    }

    IEnumerator EscucharAudio(int i)
    {
        string id = Ids.Audios[i];
        bool nuevo = !Memoria.Sabe(id);
        if (nuevo) yield return D.DecirCo("Valeria", "Un celular con funda roja, tirado en el piso. Tiene una grabación. (Le das Play.)");
        // La grabación se ve como un recuerdo (video)
        yield return StartCoroutine(Recuerdos.Reproducir(id));
        if (i == 7)
        {
            yield return D.DecirCo("Valeria", "Yo tenía 17. Pensé que era una broma.", "Nunca pregunté qué le pasó a esa chica.");
            if (Memoria.Aprender("secreto")) UI.Toast("Nuevo en tu cuaderno: " + Pistas.Catalogo["secreto"].titulo, 3.5f);
        }
        if (i == 4 && Memoria.Aprender("natita")) UI.Toast("Nueva pista: " + Pistas.Catalogo["natita"].titulo, 3.5f);
        if (nuevo)
        {
            Memoria.Aprender(id);
            UI.Toast("GRABACIÓN DE CAMILA " + Memoria.CuantosRecuerdos(Ids.Audios) + "/8  ·  la puedes releer en el cuaderno (Tab)", 4f);
        }
    }

    IEnumerator HablarNatita()
    {
        if (Memoria.Tiene("ofrenda_natita"))
        {
            yield return D.DecirCo("Valeria", "Las velas siguen encendidas. La ñatita parece sonreír.", "\"Las dos\", recuerdo que dijo.");
            yield break;
        }
        if (!Memoria.Sabe("natita"))
        {
            yield return D.DecirCo("Valeria",
                "En una vitrina hay un cráneo con chulo de colores, lentes oscuros y flores secas. Una ñatita.",
                "Tiene un cartelito escrito a mano: \"Se reciben ofrendas: flores, coca y velas. NO pedir favores con rabia.\"",
                "Alguien dejó una vela derretida hace poco... y una tarjeta de delegada.");
            Memoria.Aprender("natita");
            UI.Toast("Nueva pista: " + Pistas.Catalogo["natita"].titulo, 3.5f);
            yield break;
        }
        int tiene = Memoria.Cuantos(Ids.Ofrenda);
        if (tiene < 3)
        {
            List<string> falta = new List<string>();
            if (!Memoria.Tiene("ofrenda_flores")) falta.Add("flores (Patio)");
            if (!Memoria.Tiene("ofrenda_coca")) falta.Add("hojas de coca (Don Teodoro)");
            if (!Memoria.Tiene("ofrenda_vela")) falta.Add("una vela (Hotelería, Piso 2)");
            yield return D.DecirCo("Valeria", "La ñatita me mira desde sus lentes oscuros. Necesito la ofrenda completa.",
                "Me falta: " + string.Join(", ", falta.ToArray()) + ".");
            yield break;
        }

        yield return StartCoroutine(CineHistoria.Natita(juego, posNatita, Memoria.Sabe("secreto")));
        Memoria.Obtener("ofrenda_natita");
        Memoria.Aprender("natita_ofrenda");
        Logros.Desbloquear("natita");
        UI.Toast("La ofrenda está puesta por HOY. Llévale la verdad a Camila en la Bienvenida.", 5f);
    }

    void MontarPlantaBaja(Mapa m, Transform t)
    {
        Sala ingreso = m.Salas["ingreso"];
        Personaje.Crear(t, "Diego", ingreso.Centro + new Vector2(2.6f, 0.8f), Persona("#e76f51", "#c68a5a", "#2b1d14", "#264653", false, Accesorio.Mochila, "#2a9d8f"),
            delegate { Lanzar(HablarDiego()); });
        Interactuable.Crear(t, ingreso.Centro + new Vector2(-3.4f, 2.6f), "Leer el directorio", delegate
        {
            D.Decir("Directorio — Sede La Paz", new string[]
            {
                "PLANTA BAJA: Ingreso · Seguridad · Patio · Cafetería · Auditorio · Biblioteca\n" +
                "PISO 1: DERECHO · Psicología · Economía · Contaduría · Administración\n" +
                "PISO 2: PERIODISMO · Diseño Gráfico · Publicidad y Marketing · Hotelería y Turismo · Radio",
                "PISO 3: MEDICINA · Odontología · Bioquímica y Farmacia\n" +
                "PISO 4: INGENIERÍA DE SISTEMAS · Sala de Servidores · Ingeniería Comercial\n\n" +
                "<color=#ff4d4d>Alguien rayó con marcador: \"NO USES LAS ESCALERAS\"</color>"
            });
        }, 1.4f);

        Sala seg = m.Salas["seguridad"];
        Personaje.Crear(t, "Don Teodoro (Guardia)", seg.Centro + new Vector2(0f, -1.4f), Persona("#264653", "#a47148", "#555555", "#1d3557", false, Accesorio.Gorra, "#1d3557"),
            delegate { Lanzar(HablarTeodoro()); });

        Sala patio = m.Salas["patio"];
        Personaje.Crear(t, "Camila (Delegada)", patio.Centro + new Vector2(0f, 0.2f), Persona("#ffb703", "#e0ac69", "#6f1d1b", "#3d405b", true, Accesorio.Bufanda, "#e63946"),
            delegate { Lanzar(HablarCamila()); });

        Sala cafe = m.Salas["cafeteria"];
        Personaje.Crear(t, "Andrea", cafe.Centro + new Vector2(2.2f, 1.4f), Persona("#6a040f", "#d4a373", "#111111", "#343a40", true, Accesorio.Mochila, "#ff70a6"),
            delegate { Lanzar(HablarAndrea()); });

        // Auditorio: lleva a la escena de la Bienvenida
        Sala aud = m.Salas["auditorio"];
        Interactuable puerta = Interactuable.Crear(t, aud.FrentePuerta, "Entrar al Auditorio", delegate { Lanzar(PuertaAuditorio()); }, 1.6f);
        puerta.textoDinamico = delegate
        {
            return juego.Hora < juego.horaBienvenida ? "Auditorio (abre a las 18:00)" : "Entrar a la Bienvenida";
        };
    }

    // Guía que explica la carrera + objeto para revisar.
    void MontarExploracion(Transform t, Sala s, Exploracion.Sala ex)
    {
        Exploracion.Sala dato = ex;
        Personaje.Crear(t, dato.guia, s.P(dato.posGuia.x, dato.posGuia.y), Arte.Personaje(dato.apariencia),
            delegate { Lanzar(HablarGuiaCarrera(dato)); });
        Interactuable.Crear(t, s.P(dato.posObjeto.x, dato.posObjeto.y), dato.objeto, delegate { Lanzar(RevisarObjeto(dato)); }, 1.5f);
    }

    IEnumerator HablarGuiaCarrera(Exploracion.Sala ex)
    {
        Resultado<bool> resuelto = new Resultado<bool>();
        if (Protagonista.EsMateo) yield return StartCoroutine(AliadoMateo(ex, resuelto));
        else yield return StartCoroutine(Misiones.HablarGuia(D, ex, resuelto));
        if (resuelto.valor) yield break;
        string nombre = ex.guia.Contains("(") ? ex.guia.Substring(0, ex.guia.IndexOf('(')).Trim() : ex.guia;
        yield return D.DecirCo(nombre, ex.explicacion);
        if (Memoria.Aprender("carrera_" + ex.id))
            UI.Toast("CARRERA CONOCIDA: " + ex.carrera + "  (" + CarrerasConocidas() + ")", 3.5f);
    }

    IEnumerator RevisarObjeto(Exploracion.Sala ex)
    {
        if (ex.pista != null && Recuerdos.Existe(ex.pista))
        {
            // Solo la primera línea y luego el recuerdo (video)
            yield return D.DecirCo("Valeria", ex.textoObjeto[0]);
            yield return StartCoroutine(Recuerdos.Reproducir(ex.pista));
        }
        else yield return D.DecirCo("Valeria", ex.textoObjeto);
        if (ex.pista != null && Memoria.Aprender(ex.pista))
            UI.Toast("Nueva pista en tu cuaderno: " + Pistas.Catalogo[ex.pista].titulo, 3.5f);
    }

    public static int TotalCarreras { get { return Exploracion.Salas.Count + 4; } }

    public static int NumCarreras()
    {
        int n = 0;
        foreach (Exploracion.Sala ex in Exploracion.Salas) if (Memoria.Sabe("carrera_" + ex.id)) n++;
        foreach (string f in new[] { "dom_medicina", "dom_derecho", "dom_periodismo", "dom_sistemas" }) if (Memoria.Sabe(f)) n++;
        return n;
    }

    public static string CarrerasConocidas()
    {
        int n = 0;
        foreach (Exploracion.Sala ex in Exploracion.Salas) if (Memoria.Sabe("carrera_" + ex.id)) n++;
        foreach (string f in new[] { "dom_medicina", "dom_derecho", "dom_periodismo", "dom_sistemas" }) if (Memoria.Sabe(f)) n++;
        return n + "/" + (Exploracion.Salas.Count + 4);
    }

    void CrearFacultad<T>(Sala s) where T : Facultad
    {
        GameObject go = new GameObject("Facultad: " + typeof(T).Name.Replace("Fac", ""));
        go.transform.SetParent(transform, false);
        go.AddComponent<T>().Iniciar(s);
    }

    public static string UltimoCasillero;     // para que el tutorial no le enseñe un casillero al Pepino

    void CrearCasillero(Transform t, Vector2 pos)
    {
        Util.Objeto("Casillero", t, Arte.Casillero(), pos + new Vector2(0f, -0.85f));
        Interactuable i = Interactuable.Crear(t, pos, "Esconderte en el casillero", null, 1.2f);
        i.textoDinamico = delegate { return J.Escondido ? "Salir del casillero" : "Esconderte en el casillero"; };
        string clave = "casillero_" + juego.piso + "_" + Mathf.RoundToInt(pos.x);
        i.alInteractuar = delegate
        {
            Sonido.Efecto("puerta", 0.4f, 1.7f);
            if (J.Escondido) { J.Esconder(false); return; }
            J.Teletransportar(pos);
            J.Esconder(true);
            UltimoCasillero = clave;
            // El Pepino aprende: recuerda los casilleros que usaste en OTROS días
            bool conocido = Dificultad.Escalada > 0f && Memoria.Sabe(clave) && !Memoria.Tiene(clave);
            Memoria.Aprender(clave);
            Memoria.Obtener(clave);
            if (conocido && pepinoPiso != null && pepinoPiso.RevisarCasillero(pos))
                UI.Toast("Pasos que se acercan directo hacia ti... El Pepino RECUERDA este casillero. ¡Sal de ahí!", 4f);
        };
    }

    // =================================================================
    //  ESCALERAS Y ASCENSOR
    // =================================================================
    string[] OpcionesPisos(int actual, List<int> destinos)
    {
        List<string> ops = new List<string>();
        for (int p = 0; p < Mapa.Pisos; p++)
        {
            if (p == actual) continue;
            destinos.Add(p);
            string extra = p == 1 ? " (Derecho)" : p == 2 ? " (Periodismo)" : p == 3 ? " (Medicina)" : p == 4 ? " (Sistemas)" : "";
            ops.Add(Mapa.NombresPiso[p] + extra);
        }
        return ops.ToArray();
    }

    IEnumerator UsarEscaleras(int piso)
    {
        if (J.Escondido) yield break;
        List<int> destinos = new List<int>();
        Resultado<int> r = new Resultado<int>();
        yield return D.ElegirCo("Escaleras", "¿A qué piso vas?", OpcionesPisos(piso, destinos), r);
        int destino = destinos[r.valor];

        // La primera muerte: subir sola antes de las 9:00
        if (destino > piso && juego.Hora < 9f && !Memoria.Tiene("escaleras_ok") && !Protagonista.EsMateo)
        {
            if (Memoria.Tiene("con_diego"))
            {
                Memoria.Obtener("escaleras_ok");
                UI.Toast("Subes con Diego. Arriba algo se mueve en la sombra... y se aleja.", 4f);
            }
            else
            {
                juego.Bloquear(this);
                UI.Oscuridad(0.5f);
                yield return D.DecirCo("Valeria", "Las escaleras están vacías. Demasiado vacías.", "Algo cruje arriba...");
                UI.Sangre();
                Sonido.Efecto("paso_pepino", 0.9f, 0.8f);
                yield return new WaitForSeconds(0.4f);
                juego.Desbloquear(this);
                juego.Morir("En las escaleras vacías, alguien te empujó por el hueco.\nLo último que viste fue una máscara sonriente asomándose desde arriba.", "escaleras", TipoMuerte.Empujon);
                yield break;
            }
        }
        else if (destino > piso) Memoria.Obtener("escaleras_ok");

        juego.AvanzarReloj(0.05f * Mathf.Abs(destino - piso));
        Partida.IrAPiso(destino, "escaleras");
    }

    IEnumerator UsarAscensor(int piso)
    {
        if (J.Escondido) yield break;
        if (Partida.Evento == "ascensor" && !Protagonista.EsMateo)
        {
            yield return D.DecirCo("Ascensor", "Un cartel pegado con cinta: \"FUERA DE SERVICIO — Mantenimiento. Disculpe las molestias.\"");
            yield break;
        }
        if (!Memoria.Tiene("tarjeta"))
        {
            yield return D.DecirCo("Ascensor", "Pide tarjeta: \"Solo personal y estudiantes autorizados\".");
            yield break;
        }
        List<int> destinos = new List<int>();
        Resultado<int> r = new Resultado<int>();
        yield return D.ElegirCo("Ascensor", "¿A qué piso? (Es lento: gastas 15 minutos)", OpcionesPisos(piso, destinos), r);
        int destino = destinos[r.valor];
        juego.AvanzarReloj(0.25f);
        Sonido.Efecto("ascensor");
        Partida.IrAPiso(destino, "ascensor");
    }

    // =================================================================
    //  PERSONAJES DE LA PLANTA BAJA
    // =================================================================
    IEnumerator HablarDiego()
    {
        List<string> ops = new List<string>();
        List<int> ids = new List<int>();
        ops.Add("Charlar"); ids.Add(0);
        if (Memoria.Sabe("escaleras") && !Memoria.Tiene("con_diego") && !Memoria.Tiene("escaleras_ok")) { ops.Add("\"¿Subimos juntos por las escaleras?\""); ids.Add(1); }
        if ((Memoria.Sabe("motivo") || Memoria.Sabe("usuario")) && !Memoria.Sabe("diego_primo")) { ops.Add("\"¿Conoces a Camila?\""); ids.Add(2); }
        ops.Add("Nada, chau"); ids.Add(3);

        Resultado<int> r = new Resultado<int>();
        yield return D.ElegirCo("Diego", "¡Hola! ¿También eres cachimba? Yo soy Diego. Estoy igual de perdido que tú, jaja.", ops.ToArray(), r);
        switch (ids[r.valor])
        {
            case 0:
                yield return D.DecirCo("Diego",
                    "Dicen que la Bienvenida de esta noche va a estar loca. Mi prima la organiza.",
                    "Hay que juntar los 4 sellos del pasaporte: Medicina, Derecho, Periodismo y Sistemas.",
                    "Yo voy a empezar por la cafetería, obvio.");
                break;
            case 1:
                Memoria.Obtener("con_diego");
                yield return D.DecirCo("Diego", "¡Dale! Vamos juntos. Así no me pierdo yo tampoco.", "(Diego te acompañará la próxima vez que subas por las escaleras.)");
                break;
            case 2:
                yield return D.DecirCo("Diego",
                    "¿Camila? Es mi prima.",
                    "Esta mañana me pidió que te acompañara a algunos lados. Que eras 'especial'.",
                    "...¿Por qué pones esa cara?");
                Memoria.Aprender("diego_primo");
                break;
        }
    }

    IEnumerator HablarTeodoro()
    {
        List<string> ops = new List<string>();
        List<int> ids = new List<int>();
        ops.Add("Saludar"); ids.Add(0);
        if (Memoria.Sabe("escaleras") && !Memoria.Tiene("tarjeta")) { ops.Add("Pedir la tarjeta del ascensor"); ids.Add(1); }
        ops.Add("Preguntar por las cámaras"); ids.Add(2);
        if (Memoria.Sabe("natita") && !Memoria.Tiene("ofrenda_coca") && !Memoria.Tiene("ofrenda_natita")) { ops.Add("Pedirle hojas de coca"); ids.Add(4); }
        if (!Memoria.Tiene("llave_cuarto")) { ops.Add("\"¿Qué hay detrás de esa puerta sin letrero?\""); ids.Add(5); }
        ops.Add("Nada, gracias"); ids.Add(3);

        Resultado<int> r = new Resultado<int>();
        yield return D.ElegirCo("Don Teodoro", "Buenos días, señorita. Carnet en mano siempre, ¿ya?", ops.ToArray(), r);
        switch (ids[r.valor])
        {
            case 0:
                yield return D.DecirCo("Don Teodoro", "Bienvenida a Unifranz. Cualquier cosa, aquí estoy en Seguridad.", "Y no corra por los pasillos, que hoy están raros.");
                break;
            case 1:
                Memoria.Obtener("tarjeta");
                yield return D.DecirCo("Don Teodoro", "¿Le dan miedo las escaleras? Tome, pero me la devuelve, ¿ya?", "El ascensor es lento, pero seguro.");
                break;
            case 2:
                if (Memoria.Tiene(Ids.PruebaRegistros))
                {
                    yield return D.DecirCo("Don Teodoro",
                        "...¿Recuperó los respaldos? Ay, señorita.",
                        "Yo las borré. La delegada, la señorita Camila, me dijo que eran de una broma para la Bienvenida.",
                        "Que no quería que se arruinara la sorpresa. Yo no sabía... se lo juro.");
                    Memoria.Aprender("teodoro_camaras");
                }
                else yield return D.DecirCo("Don Teodoro", "¿Las cámaras? Justo hoy no graban, señorita. Mantenimiento.", "(Evita mirarte a los ojos.)");
                break;
            case 5:
                yield return D.DecirCo("Don Teodoro",
                    "¿Esa? Es el cuarto de monitoreo viejo. Ahí cuidaba yo de noche, antes de las cámaras nuevas.",
                    "Una noche de la Bienvenida pasada me quedé dormido ahí... y vi cosas en los monitores. Disfraces que caminaban solos.",
                    "(Te da una llave oxidada.) Si entra, cierre bien las puertas. Y vigile la energía, señorita. Siempre se acaba.");
                Memoria.Obtener("llave_cuarto");
                if (Memoria.Aprender("cuarto_secreto")) UI.Toast("Nueva pista: " + Pistas.Catalogo["cuarto_secreto"].titulo, 3.5f);
                break;
            case 4:
                yield return D.DecirCo("Don Teodoro", "¿Coca? (Saca una bolsita de su chamarra.) ...¿Es para la ñatita de Odontología?",
                    "Ah, usted también sabe. Tome, señorita. Dígale que es de parte de Teodoro.",
                    "Y no le pida nada con rabia, ¿ya? Esas cosas se cobran.");
                Memoria.Obtener("ofrenda_coca");
                UI.Toast("Tienes HOJAS DE COCA para la ñatita. (" + Memoria.Cuantos(Ids.Ofrenda) + "/3)", 3.5f);
                break;
        }
    }

    IEnumerator HablarAndrea()
    {
        bool puedePreguntar = Memoria.Sabe("andrea_disfraz") && !Memoria.Sabe("andrea_inocente") &&
                              (Memoria.Sabe("diego_primo") || Memoria.Sabe("usuario") || Memoria.Sabe("motivo"));
        if (puedePreguntar)
        {
            Resultado<int> r = new Resultado<int>();
            yield return D.ElegirCo("Andrea", "¿Otra vez tú, Quispe?", new string[] { "Preguntar por el disfraz de su mochila", "Irte" }, r);
            if (r.valor == 0)
            {
                yield return D.DecirCo("Andrea",
                    "¿Esto? Es para la fiesta. Me lo dio Camila, la delegada.",
                    "Me dijo que me lo pusiera a las siete en punto y que me parara cerca del escenario.",
                    "...¿Por qué me miras así? Me estás asustando.");
                Memoria.Aprender("andrea_inocente");
            }
            yield break;
        }
        yield return D.DecirCo("Andrea",
            "¿Qué miras, Quispe? ¿Vienes a arruinarme la U como arruinaste la promo?",
            "(Algo de rombos de colores asoma de su mochila. Lo esconde rápido.)");
        Memoria.Aprender("andrea_disfraz");
    }

    IEnumerator HablarCamila()
    {
        string[] requisito = { "quemaduras", "motivo", "cicatriz", "usuario" };
        int charla = -1;
        for (int i = 0; i < 4; i++)
            if (Memoria.Sabe(requisito[i]) && !Memoria.Sabe(Ids.CharlasCamila[i])) { charla = i; break; }

        if (charla < 0 && Memoria.Sabe("audio_7") && !Memoria.Sabe("camila_recuerda"))
        {
            Resultado<int> rr = new Resultado<int>();
            yield return D.ElegirCo("Camila", "(Camila te sonríe, pero tiene ojeras de mil días.)",
                new string[] { "\"Tú también recuerdas, ¿verdad? Cada bucle.\"", "Mejor no decir nada" }, rr);
            if (rr.valor == 0)
            {
                yield return D.DecirCo("Camila", "(La sonrisa se le cae.)",
                    "Día " + Memoria.Dia + ". Sí. Llevo la cuenta.",
                    "Cada vez que te... cada vez, lo recuerdo. Y a las 6:45 me despierto otra vez con el disfraz en la cama.",
                    "Yo solo pedí UN día. No esto.");
                Memoria.Aprender("camila_recuerda");
                UI.Toast("Nueva pista: " + Pistas.Catalogo["camila_recuerda"].titulo, 3.5f);
            }
            yield break;
        }

        if (charla < 0)
        {
            string extra = Memoria.Dia >= 6 ? "(Sin mirarte.) ¿Cuántas veces van ya, cachimba? ...Olvídalo. Cosas mías."
                         : Memoria.Dia >= 3 ? "(Se queda helada al verte.) ...¿No nos vimos ayer? No. Claro que no. Perdón."
                         : "(Te mira un segundo de más.) ...Te pareces mucho a alguien que conocí.";
            yield return D.DecirCo("Camila",
                "¡Hola, cachimba! Soy Camila, la delegada. ¿Ya tienes tu pasaporte?",
                "Necesitas los 4 sellos para entrar a la Bienvenida. Empieza a las 18:00 en el auditorio.",
                extra);
            yield break;
        }

        Resultado<int> r = new Resultado<int>();
        yield return D.ElegirCo("Camila", "(Camila te sonríe. Lleva guantes y bufanda aunque hace calor.)",
            new string[] { "Hablarle de lo que descubriste", "Mejor no decir nada" }, r);
        if (r.valor == 1) yield break;

        switch (charla)
        {
            case 0:
                yield return D.DecirCo("Valeria", "Camila... en el archivo de la clínica vi algo. Una chica quemada, la noche de la Bienvenida del año pasado.");
                yield return D.DecirCo("Camila", "(Se ajusta la bufanda.) Hay muchas historias en esta U, cachimba.", "(Le tiemblan las manos.) ...¿Por qué me lo preguntas a mí?");
                break;
            case 1:
                yield return D.DecirCo("Valeria", "Vi el expediente. Tu nombre... y el de mi hermano.");
                yield return D.DecirCo("Camila", "...Tienes sus mismos ojos, ¿sabías?",
                    "Retiré la denuncia porque nadie me creyó. Él era el chico estrella. Yo, la cachimba que 'exageraba'.");
                break;
            case 2:
                yield return D.DecirCo("Valeria", "Vi la grabación del set. La mano con cicatrices.");
                yield return D.DecirCo("Camila", "(Se quita un guante. Las cicatrices son reales.)",
                    "Fue una 'broma'. Alcohol, un encendedor... y todos grabando, riéndose.",
                    "Lo grabaron riéndose mientras me quemaba.");
                break;
            case 3:
                yield return D.DecirCo("Valeria", "Los registros dicen 'c.rojas'. Tú me pusiste en esa ruta.");
                yield return D.DecirCo("Camila", "(No lo niega.)",
                    "Solo quería que alguien de su familia sintiera miedo un día. Uno solo.",
                    "...Pero el día no se termina nunca, ¿verdad?");
                break;
        }
        Memoria.Aprender(Ids.CharlasCamila[charla]);
        UI.Toast("Charlas con Camila: " + Memoria.CuantosRecuerdos(Ids.CharlasCamila) + "/4", 3f);
    }

    // =================================================================
    //  LA BIENVENIDA (FINAL)
    // =================================================================
    IEnumerator PuertaAuditorio()
    {
        if (juego.Hora < juego.horaBienvenida)
        {
            yield return D.DecirCo("Auditorio", "Las puertas están cerradas. Un cartel dice:\n\"BIENVENIDA A LOS CACHIMBOS — 18:00. Solo con Pasaporte de Carreras completo.\"");
            yield break;
        }
        int sellos = Memoria.Cuantos(Ids.Sellos);
        if (sellos < 4)
        {
            yield return D.DecirCo("Estudiante en la puerta",
                "Sin los 4 sellos no entras, cachimba. Tienes " + sellos + "/4.",
                "(Detrás de él, alguien con máscara de Pepino te saluda con la mano.)");
            yield break;
        }
        yield return D.DecirCo("Valeria", "Se escucha la música desde adentro. Las luces parpadean por debajo de la puerta.");
        Partida.IrABienvenida();
    }
}
