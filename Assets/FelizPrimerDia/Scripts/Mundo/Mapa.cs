using System.Collections.Generic;
using UnityEngine;

// =====================================================================
//  MAPA DE LA SEDE LA PAZ (Av. Héroes del Acre esq. Landaeta, San Pedro)
//
//  La sede es un edificio urbano de varios pisos. Cada piso se dibuja como un
//  pasillo central con salas arriba y abajo. La distribución es APROXIMADA:
//  ajusta nombres, anchos y orden en Definir() con fotos o el plano real.
//  Cada fila (arriba / abajo) debe sumar 48 de ancho.
// =====================================================================
public class Mapa : MonoBehaviour
{
    public const int Pisos = 5;
    public const float SeparacionPisos = 200f;
    public const float LargoPasillo = 48f;
    public const float AnchoPasillo = 6f;
    public const float FondoSala = 9f;
    public const float Grosor = 0.4f;

    public static readonly string[] NombresPiso = { "Planta Baja", "Piso 1", "Piso 2", "Piso 3", "Piso 4" };

    public class DefSala
    {
        public string id;
        public string nombre;
        public float ancho;
        public bool abierta;          // tiene puerta al pasillo
        public bool especial;         // puerta controlada por la historia (ej. auditorio)
        public Color color;
        public string info;           // texto del cartel si está cerrada (para conocer la carrera)

        public DefSala(string id, string nombre, float ancho, bool abierta, string colorHex, string info)
        {
            this.id = id; this.nombre = nombre; this.ancho = ancho; this.abierta = abierta;
            color = Util.Hex(colorHex); this.info = info;
        }
    }

    public readonly Dictionary<string, Sala> Salas = new Dictionary<string, Sala>();
    public int piso;                                   // cada escena dibuja un solo piso
    public Vector2 PuntoInicio { get; private set; }

    readonly List<DefSala>[] arriba = new List<DefSala>[Pisos];
    readonly List<DefSala>[] abajo = new List<DefSala>[Pisos];

    // ------------------------------------------------------------------
    void Definir()
    {
        for (int i = 0; i < Pisos; i++) { arriba[i] = new List<DefSala>(); abajo[i] = new List<DefSala>(); }

        // ---------- PLANTA BAJA ----------
        arriba[0].Add(new DefSala("patio", "Patio", 12, true, "#3f5f36", null));
        arriba[0].Add(new DefSala("cafeteria", "Cafetería", 12, true, "#6b4f3a", null));
        arriba[0].Add(new DefSala("auditorio", "AUDITORIO", 24, false, "#3d2236", null) { especial = true });

        abajo[0].Add(new DefSala("seguridad", "Seguridad", 12, true, "#34404f", null));
        abajo[0].Add(new DefSala("ingreso", "Ingreso", 12, true, "#4d4d5c", null));
        abajo[0].Add(new DefSala("banos0", "Baños", 12, false, "#2e3a3a",
            "Baños.\nAlguien escribió con labial en el espejo:\n<color=#ff4d4d>FELIZ PRIMER DÍA</color>"));
        abajo[0].Add(new DefSala("biblioteca", "Biblioteca", 12, false, "#3a3428",
            "BIBLIOTECA\nLibros, salas de estudio y computadoras para investigar.\nHoy está cerrada por la Inducción."));

        // ---------- PISO 1 — Derecho y Ciencias Sociales / Empresariales ----------
        arriba[1].Add(new DefSala("derecho", "Sala de Juicios Simulados — DERECHO", 24, true, "#4f3626", null));
        arriba[1].Add(new DefSala("psicologia", "Psicología", 12, false, "#3b3550",
            "PSICOLOGÍA\nEstudia la mente y el comportamiento humano.\nPractican evaluación y entrevistas en la Cámara Gesell:\nuna sala con un espejo que por el otro lado es una ventana."));
        arriba[1].Add(new DefSala("aulas1a", "Aulas", 12, false, "#2f3138",
            "Aulas del Piso 1.\nCerradas por la Semana de Inducción."));

        abajo[1].Add(new DefSala("economia", "Economía", 12, false, "#2f3d33",
            "ECONOMÍA\nAnaliza cómo se producen y reparten los recursos:\nmercados, precios, estadística y políticas económicas."));
        abajo[1].Add(new DefSala("contaduria", "Contaduría Pública", 12, false, "#2f3d33",
            "CONTADURÍA PÚBLICA\nRegistra y analiza las finanzas de empresas e instituciones:\ncontabilidad, auditoría e impuestos."));
        abajo[1].Add(new DefSala("administracion", "Administración de Empresas", 12, false, "#2f3d33",
            "ADMINISTRACIÓN DE EMPRESAS\nAprende a dirigir organizaciones:\nplanificación, talento humano, emprendimiento e innovación."));
        abajo[1].Add(new DefSala("banos1", "Baños", 12, false, "#2e3a3a",
            "Baños.\nUna de las puertas está cerrada por dentro.\nSe escucha a alguien respirando... y después nada."));

        // ---------- PISO 2 — Comunicación y Diseño ----------
        arriba[2].Add(new DefSala("periodismo", "Set de TV — PERIODISMO", 24, true, "#24242f", null));
        arriba[2].Add(new DefSala("diseno", "Diseño Gráfico", 12, false, "#3d2f45",
            "DISEÑO GRÁFICO Y PRODUCCIÓN CROSSMEDIA\nComunicación visual: ilustración, identidad de marca,\nanimación y contenido para muchos medios a la vez."));
        arriba[2].Add(new DefSala("publicidad", "Publicidad y Marketing", 12, false, "#45302f",
            "PUBLICIDAD Y MARKETING\nCrea campañas y construye marcas:\ncreatividad publicitaria y marketing digital."));

        abajo[2].Add(new DefSala("hoteleria", "Hotelería y Turismo", 12, false, "#2f4045",
            "ADMINISTRACIÓN DE HOTELERÍA Y TURISMO\nGestión de hoteles, servicios turísticos y atención al cliente.\nSalen mucho de viaje de práctica."));
        abajo[2].Add(new DefSala("radio", "Cabina de Radio", 12, false, "#24242f",
            "Cabina de Radio (Periodismo).\nEl micrófono está encendido. Por los parlantes solo se oye\nuna respiración... y una risa."));
        abajo[2].Add(new DefSala("aulas2b", "Aulas", 24, false, "#2f3138",
            "Aulas del Piso 2.\nCerradas por la Semana de Inducción."));

        // ---------- PISO 3 — Ciencias de la Salud ----------
        arriba[3].Add(new DefSala("medicina", "Sala de Simulación — MEDICINA", 24, true, "#2f4f4f", null));
        arriba[3].Add(new DefSala("odontologia", "Odontología", 12, false, "#2f4a4f",
            "ODONTOLOGÍA\nCuida la salud de la boca y los dientes.\nPractican con simuladores y sillones dentales antes de atender pacientes."));
        arriba[3].Add(new DefSala("bioquimica", "Bioquímica y Farmacia", 12, false, "#2f4f3c",
            "BIOQUÍMICA Y FARMACIA\nEstudia los medicamentos y los análisis clínicos.\nMucho laboratorio: reactivos, microscopios y batas."));

        abajo[3].Add(new DefSala("archivo", "Archivo de la Clínica", 12, false, "#3a3a3a",
            "Archivo de la Clínica.\nSolo personal autorizado.\nLa cerradura está rayada, como si alguien la hubiera forzado."));
        abajo[3].Add(new DefSala("aulas3b", "Aulas", 36, false, "#2f3138",
            "Aulas del Piso 3.\nCerradas por la Semana de Inducción."));

        // ---------- PISO 4 — Ingenierías ----------
        arriba[4].Add(new DefSala("sistemas", "Laboratorio de Computación — ING. DE SISTEMAS", 24, true, "#1f2a40", null));
        arriba[4].Add(new DefSala("servidores", "Sala de Servidores", 12, false, "#1a2233",
            "Sala de Servidores (Ingeniería de Sistemas).\nSe escucha el zumbido de los equipos... y alguien tecleando."));
        arriba[4].Add(new DefSala("comercial", "Ingeniería Comercial", 12, false, "#2b3a2f",
            "INGENIERÍA COMERCIAL\nNegocios, comercio exterior, finanzas y logística.\nAprenden cómo se mueven los productos y el dinero."));

        abajo[4].Add(new DefSala("aulas4b", "Aulas", 48, false, "#2f3138",
            "Aulas del Piso 4.\nCerradas por la Semana de Inducción."));
    }

    // ------------------------------------------------------------------
    public const float AltoPared = 1.2f;   // frente visible de las paredes (vista 3/4)

    static readonly Color ColorPasillo = new Color(0.79f, 0.77f, 0.73f);
    static readonly Color PinturaPasillo = new Color(0.85f, 0.82f, 0.76f);

    // En qué piso está una sala (aunque ese piso no esté construido en esta escena). -1 si no existe.
    public int PisoDeSala(string id)
    {
        for (int p = 0; p < Pisos; p++)
        {
            if (arriba[p] != null) foreach (DefSala d in arriba[p]) if (d.id == id) return p;
            if (abajo[p] != null) foreach (DefSala d in abajo[p]) if (d.id == id) return p;
        }
        return -1;
    }

    // Solo define las salas (sin dibujar nada): para el mapa de la escena 10_Pantallas
    public void PrepararPlano() { if (arriba[0] == null) Definir(); }

    // Plano de cualquier piso (para el mapa grande): salas con su nombre y su área, sin construir nada.
    public List<Sala> PlanoDePiso(int p)
    {
        List<Sala> l = new List<Sala>();
        if (p < 0 || p >= Pisos || arriba[p] == null) return l;
        Vector2 o = Origen(p);
        for (int fila = 0; fila < 2; fila++)
        {
            bool esArriba = fila == 0;
            float x = -LargoPasillo / 2f;
            float yPasillo = esArriba ? AnchoPasillo / 2f : -AnchoPasillo / 2f;
            float yFondo = esArriba ? yPasillo + FondoSala : yPasillo - FondoSala;
            foreach (DefSala d in esArriba ? arriba[p] : abajo[p])
            {
                Sala s = new Sala();
                s.id = d.id; s.nombre = d.nombre; s.piso = p; s.arriba = esArriba; s.ancho = d.ancho;
                s.area = new Rect(o.x + x, o.y + Mathf.Min(yPasillo, yFondo), d.ancho, FondoSala);
                s.Centro = s.area.center;
                l.Add(s);
                x += d.ancho;
            }
        }
        return l;
    }

    public void Construir()
    {
        Horno.Empezar("Mapa");
        try { ConstruirInterno(); }
        finally { Horno.Terminar(); }
    }

    void ConstruirInterno()
    {
        Definir();
        // Las salas de otras carreras que se pueden explorar tienen la puerta abierta
        for (int p = 0; p < Pisos; p++)
        {
            foreach (DefSala d in arriba[p]) if (Exploracion.EsExplorable(d.id)) d.abierta = true;
            foreach (DefSala d in abajo[p]) if (Exploracion.EsExplorable(d.id)) d.abierta = true;
        }
        piso = Mathf.Clamp(piso, 0, Pisos - 1);
        ConstruirPiso(piso);
        if (piso != 0)
        {
            PuntoInicio = LlegadaEscaleras(piso);
            return;
        }
        PuntoInicio = Salas["ingreso"].Centro + new Vector2(0f, -1.5f);

        // Fachada: la calle y la acera de la Av. Héroes del Acre.
        Vector2 o = Origen(0);
        Util.Mosaico("Acera", transform, Arte.Suelo(TipoSuelo.Piedra, Util.Hex("#a8a196")), o + new Vector2(0, -13.5f), new Vector2(60f, 3f), -2000);
        Util.Mosaico("Calle", transform, Arte.Suelo(TipoSuelo.Calle, Util.Hex("#4a4a50")), o + new Vector2(0, -18f), new Vector2(60f, 6f), -2000);
        for (float x = -28f; x < 30f; x += 4f)
            Util.Rect("Línea", transform, o + new Vector2(x, -18f), new Vector2(1.8f, 0.15f), Util.Hex("#e9c46a"), -1990);
        Util.Texto("Av. Héroes del Acre", transform, o + new Vector2(10f, -18f), 2.2f, new Color(1, 1, 1, 0.55f), 0);
        Util.Rect("Letrero", transform, o + new Vector2(-6f, -13.2f), new Vector2(7.5f, 1.1f), Util.Hex("#0b3d91"), -1980);
        Util.Texto("UNIFRANZ  ·  SEDE LA PAZ", transform, o + new Vector2(-6f, -13.2f), 1.5f, Util.Hex("#ffffff"), 0);
        Util.Objeto("Planta", transform, Arte.Planta(), o + new Vector2(-11f, -13.3f));
        Util.Objeto("Planta", transform, Arte.Planta(), o + new Vector2(-1f, -13.3f));
    }

    // Carpeta en la Hierarchy (en la escena dibujada se reutiliza la misma).
    public static Transform Grupo(string nombre, Transform padre)
    {
        string id;
        GameObject ya = Horno.Siguiente(out id);
        if (ya != null) return ya.transform;
        GameObject go = new GameObject(nombre);
        go.transform.SetParent(padre, false);
        Horno.Marcar(go, id);
        return go.transform;
    }

    public static Vector2 Origen(int piso) { return new Vector2(piso * SeparacionPisos, 0f); }

    public static int PisoDe(Vector2 pos)
    {
        return Mathf.Clamp(Mathf.RoundToInt(pos.x / SeparacionPisos), 0, Pisos - 1);
    }

    // Posiciones fijas en cada piso: escaleras y ascensor al fondo izquierdo del pasillo.
    public static Vector2 Escaleras(int piso) { return Origen(piso) + new Vector2(-22.6f, 0.9f); }
    public static Vector2 Ascensor(int piso) { return Origen(piso) + new Vector2(-22.6f, -1.6f); }
    public static Vector2 LlegadaEscaleras(int piso) { return Origen(piso) + new Vector2(-20.2f, 0.6f); }
    public static Vector2 LlegadaAscensor(int piso) { return Origen(piso) + new Vector2(-20.2f, -1.7f); }

    void ConstruirPiso(int p)
    {
        Vector2 o = Origen(p);
        Transform t = Grupo(NombresPiso[p], transform);

        // Pasillo
        Util.Mosaico("Pasillo", t, Arte.Suelo(TipoSuelo.Ceramica, ColorPasillo), o, new Vector2(LargoPasillo, AnchoPasillo), -2000);
        MuroV(t, o + new Vector2(-LargoPasillo / 2f, 0), AnchoPasillo + Grosor);
        MuroV(t, o + new Vector2(LargoPasillo / 2f, 0), AnchoPasillo + Grosor);
        Util.Texto(NombresPiso[p].ToUpper(), t, o + new Vector2(-13f, 0.2f), 1.6f, new Color(0f, 0f, 0f, 0.18f), -2990);

        // Escaleras y ascensor
        Util.Objeto("Escaleras", t, Arte.Escaleras(), Escaleras(p) + new Vector2(0f, -1f));
        Util.Texto("Escaleras", t, Escaleras(p) + new Vector2(0f, 1.6f), 0.8f, Color.white, 0);
        Util.Objeto("Ascensor", t, Arte.Ascensor(), Ascensor(p) + new Vector2(0f, -1.1f));
        Util.Texto("Ascensor", t, Ascensor(p) + new Vector2(0f, -1.55f), 0.8f, Color.white, 0);

        FilaSalas(p, t, arriba[p], true);
        FilaSalas(p, t, abajo[p], false);
        DecorarPasillo(p, t);
    }

    // ---------- Muros ----------
    // Muro horizontal con su frente visible (si se ve) que también bloquea el paso.
    void MuroH(Transform t, float x0, float x1, float y, bool conFrente, Color pintura)
    {
        float ancho = x1 - x0;
        if (ancho <= 0.01f) return;
        float cx = (x0 + x1) / 2f;
        GameObject tope = Util.Rect("Muro", t, new Vector2(cx, y), new Vector2(ancho + Grosor, Grosor), Util.Hex("#2a2733"), -1400).gameObject;
        if (conFrente)
        {
            float yFrente = y - Grosor / 2f - AltoPared / 2f;
            Util.Mosaico("Pared", t, Arte.ParedFrente(pintura), new Vector2(cx, yFrente), new Vector2(ancho + Grosor, AltoPared), -1500);
            // Sombra de la pared sobre el piso
            Util.Mosaico("Sombra pared", t, Arte.SombraPared(), new Vector2(cx, yFrente - AltoPared / 2f - 0.45f), new Vector2(ancho + Grosor, 0.9f), -1940);
            BoxCollider2D bc = Util.Colision(tope);
            // Collider que cubre el muro y su frente (en unidades locales del Rect escalado)
            float altoTotal = Grosor + AltoPared;
            bc.size = new Vector2(1f, altoTotal / Grosor);
            bc.offset = new Vector2(0f, -(AltoPared / 2f) / Grosor);
        }
        else Util.Colision(tope);
    }

    void MuroV(Transform t, Vector2 centro, float alto)
    {
        Util.Muro(t, centro, new Vector2(Grosor, alto), Util.Hex("#2a2733")).GetComponent<SpriteRenderer>().sortingOrder = -1400;
    }

    // ---------- Salas ----------
    void FilaSalas(int p, Transform t, List<DefSala> fila, bool esArriba)
    {
        Vector2 o = Origen(p);
        float x = -LargoPasillo / 2f;
        float yPasillo = esArriba ? AnchoPasillo / 2f : -AnchoPasillo / 2f;
        float yFondo = esArriba ? yPasillo + FondoSala : yPasillo - FondoSala;

        foreach (DefSala d in fila)
        {
            float x0 = x, x1 = x + d.ancho;
            x = x1;
            float cx = (x0 + x1) / 2f;
            float cy = (yPasillo + yFondo) / 2f;

            Sala s = new Sala();
            s.id = d.id;
            s.nombre = d.nombre;
            s.piso = p;
            s.ancho = d.ancho;
            s.arriba = esArriba;
            s.area = new Rect(o.x + x0, o.y + Mathf.Min(yPasillo, yFondo), d.ancho, FondoSala);
            s.Centro = o + new Vector2(cx, cy);
            float dentro = esArriba ? 1.2f : -1.6f;
            s.Puerta = o + new Vector2(cx, yPasillo + dentro);
            s.FrentePuerta = o + new Vector2(cx, yPasillo - (esArriba ? 1.9f : -1.0f));
            Salas[d.id] = s;

            TipoSuelo ts; Color cs;
            SueloDe(d, out ts, out cs);
            Util.Mosaico("Suelo " + d.nombre, t, Arte.Suelo(ts, cs), s.Centro, new Vector2(d.ancho, FondoSala), -2000);

            Color pintura = Col.Claro(d.color, 0.45f);
            float wx0 = o.x + x0, wx1 = o.x + x1;
            // Muros laterales
            MuroV(t, o + new Vector2(x0, cy), FondoSala + Grosor);
            MuroV(t, o + new Vector2(x1, cy), FondoSala + Grosor);
            // Muro del fondo (se ve su frente solo en las salas de arriba)
            MuroH(t, wx0, wx1, o.y + yFondo, esArriba, pintura);

            // Muro hacia el pasillo
            bool conHueco = d.abierta || d.especial;
            Color pinturaPasillo = esArriba ? PinturaPasillo : pintura;
            float anchoPuerta = 2.4f;
            if (conHueco)
            {
                float tramo = (d.ancho - anchoPuerta) / 2f;
                MuroH(t, wx0, wx0 + tramo, o.y + yPasillo, true, pinturaPasillo);
                MuroH(t, wx1 - tramo, wx1, o.y + yPasillo, true, pinturaPasillo);
                // Marco de la puerta
                float yMarco = o.y + yPasillo - Grosor / 2f - AltoPared / 2f;
                Util.Rect("Marco", t, new Vector2(o.x + cx - anchoPuerta / 2f, yMarco), new Vector2(0.18f, AltoPared + Grosor), Util.Hex("#6b4a2f"), -1390);
                Util.Rect("Marco", t, new Vector2(o.x + cx + anchoPuerta / 2f, yMarco), new Vector2(0.18f, AltoPared + Grosor), Util.Hex("#6b4a2f"), -1390);
                Util.Plano("Alfombrilla", t, Arte.Alfombrilla(d.especial ? "#9d0208" : "#3d405b"),
                    o + new Vector2(cx, yPasillo - (esArriba ? 0.6f : -0.6f) - (esArriba ? AltoPared : 0f)), -1950);

                // Tapón para cerrar la puerta desde la historia
                GameObject tapon = Util.Muro(t, o + new Vector2(cx, yPasillo), new Vector2(anchoPuerta, Grosor), Util.Hex("#6b2b2b"));
                tapon.name = "Puerta " + d.nombre;
                tapon.GetComponent<SpriteRenderer>().sortingOrder = -1390;
                BoxCollider2D bt = tapon.GetComponent<BoxCollider2D>();
                bt.size = new Vector2(1f, (Grosor + AltoPared) / Grosor);
                bt.offset = new Vector2(0f, -(AltoPared / 2f) / Grosor);
                tapon.SetActive(d.especial);
                s.tapon = tapon;

                // Cartel con el nombre junto a la puerta
                string corto = NombreCorto(d);
                Vector2 posCartel = esArriba ? o + new Vector2(cx + 2.4f, yPasillo - 0.75f) : o + new Vector2(cx + 2.4f, yPasillo + 0.75f);
                Util.Rect("Placa", t, posCartel, new Vector2(Mathf.Max(1.6f, corto.Length * 0.17f), 0.42f), Util.Hex("#0b3d91"), -1380);
                Util.Texto(corto, t, posCartel, 0.62f, Color.white, 0);
            }
            else
            {
                MuroH(t, wx0, wx1, o.y + yPasillo, true, pinturaPasillo);
            }

            // Nombre de la sala (sobre la pared del fondo / cerca del fondo)
            float yNombre = esArriba ? yFondo - 0.8f : yFondo + 0.8f;
            Util.Texto(d.nombre, t, o + new Vector2(cx, yNombre), d.ancho >= 24 ? 1.2f : 0.9f, esArriba ? Util.Hex("#2b2433") : new Color(1, 1, 1, 0.75f), 0);

            // Cartel informativo en la puerta de las salas cerradas
            if (!conHueco && !string.IsNullOrEmpty(d.info))
            {
                Vector2 posCartel = esArriba ? o + new Vector2(cx, yPasillo - 0.9f) : o + new Vector2(cx, yPasillo + 0.5f);
                Util.Rect("Cartel", t, posCartel, new Vector2(1.3f, 0.6f), Util.Hex("#f1e3b5"), -1380);
                Util.Rect("Cartel borde", t, posCartel, new Vector2(1.45f, 0.75f), Util.Hex("#6b4a2f"), -1385);
                string info = d.info, titulo = d.nombre;
                Interactuable.Crear(t, posCartel + new Vector2(0f, esArriba ? -0.6f : 0f), "Leer cartel: " + d.nombre, delegate
                {
                    Juego.I.dialogo.Decir(titulo, new string[] { info });
                }, 1.5f);
            }

            // Luz cálida en las salas
            for (float lx = -s.ancho / 2f + 6f; lx < s.ancho / 2f; lx += 12f)
            {
                SpriteRenderer luz = Util.Plano("Luz", t, Arte.Luz(), s.P(lx, esArriba ? -0.5f : 0.5f), -1945);
                luz.transform.localScale = new Vector3(2.6f, 2.2f, 1f);
            }
            Decorar(s, t);
        }
    }

    static string NombreCorto(DefSala d)
    {
        switch (d.id)
        {
            case "derecho": return "DERECHO";
            case "periodismo": return "PERIODISMO";
            case "medicina": return "MEDICINA";
            case "sistemas": return "ING. SISTEMAS";
            case "auditorio": return "AUDITORIO";
        }
        return d.nombre.ToUpper();
    }

    static void SueloDe(DefSala d, out TipoSuelo t, out Color c)
    {
        string id = d.id;
        t = TipoSuelo.Ceramica; c = Col.Claro(d.color, 0.55f);
        if (id == "patio") { t = TipoSuelo.Pasto; c = Util.Hex("#4f8f3a"); }
        else if (id == "ingreso" || id == "seguridad") { t = TipoSuelo.Piedra; c = Util.Hex("#cfc4ae"); }
        else if (id == "cafeteria" || id == "hoteleria") { t = TipoSuelo.Terracota; c = Util.Hex("#c46a44"); }
        else if (id == "auditorio") { t = TipoSuelo.Alfombra; c = Util.Hex("#6e1423"); }
        else if (id == "derecho") { t = TipoSuelo.Madera; c = Util.Hex("#9c6a3e"); }
        else if (id.StartsWith("aulas") || id == "diseno" || id == "publicidad") { t = TipoSuelo.Madera; c = Util.Hex("#c2935f"); }
        else if (id.StartsWith("banos")) { t = TipoSuelo.Azulejo; c = Util.Hex("#6fa8dc"); }
        else if (id == "biblioteca") { t = TipoSuelo.Alfombra; c = Util.Hex("#2f5d50"); }
        else if (id == "psicologia" || id == "comercial") { t = TipoSuelo.Alfombra; c = Util.Hex("#4a4e69"); }
        else if (id == "economia" || id == "contaduria" || id == "administracion") { t = TipoSuelo.Alfombra; c = Util.Hex("#3d5a80"); }
        else if (id == "periodismo" || id == "radio") { t = TipoSuelo.Estudio; c = Util.Hex("#2b2b33"); }
        else if (id == "medicina" || id == "odontologia" || id == "bioquimica" || id == "archivo") { t = TipoSuelo.Clinico; c = Util.Hex("#e3eeec"); }
        else if (id == "sistemas" || id == "servidores") { t = TipoSuelo.Tecnico; c = Util.Hex("#8f97a3"); }
    }

    // ---------- Decoración ----------
    void O(Transform t, Sprite s, Vector2 pos) { Util.Objeto(s.name, t, s, pos); }
    void OS(Transform t, Sprite s, Vector2 pos, Vector2 col) { Util.Objeto(s.name, t, s, pos, col); }

    void FilaEscritorios(Transform t, Sala s, int columnas, int filas, float x0, float y0, float dx, float dy, bool pc, string pantalla, string silla)
    {
        for (int f = 0; f < filas; f++)
            for (int c = 0; c < columnas; c++)
            {
                Vector2 pos = s.P(x0 + c * dx, y0 - f * dy);
                OS(t, Arte.Escritorio(pc, pantalla), pos, new Vector2(1.8f, 0.6f));
                O(t, Arte.Silla(silla), pos + new Vector2(0f, -0.75f));
            }
    }

    void Decorar(Sala s, Transform t)
    {
        // Para las salas de arriba la pared del fondo está en y=+4.5 (su frente baja hasta ~3.3).
        // Para las de abajo, la pared con frente es la del pasillo (y=+4.5) y el fondo está en y=-4.5.
        switch (s.id)
        {
            case "patio":
                OS(t, Arte.Arbol(), s.P(-3.6f, 0.6f), new Vector2(0.6f, 0.4f));
                OS(t, Arte.Arbol(), s.P(3.6f, 0.9f), new Vector2(0.6f, 0.4f));
                OS(t, Arte.Banca(), s.P(0f, -2.6f), new Vector2(3.8f, 0.4f));
                O(t, Arte.Planta(), s.P(-5f, 2.8f)); O(t, Arte.Planta(), s.P(5f, 2.8f));
                break;
            case "cafeteria":
                OS(t, Arte.Mostrador(), s.P(-2.4f, 2.2f), new Vector2(3f, 0.6f));
                for (int i = 0; i < 3; i++)
                {
                    Vector2 m = s.P(-3.5f + i * 3.5f, -1.4f);
                    OS(t, Arte.MesaRedonda(), m, new Vector2(1.2f, 0.5f));
                    O(t, Arte.Silla("#e76f51"), m + new Vector2(-1f, -0.3f));
                    O(t, Arte.Silla("#e76f51"), m + new Vector2(1f, -0.3f));
                }
                O(t, Arte.Planta(), s.P(5f, 2.6f));
                O(t, Arte.Basurero(), s.P(-5.2f, -3.8f));
                break;
            case "auditorio":
                Util.Rect("Escenario", t, s.P(0f, 2.1f), new Vector2(15f, 2.6f), Util.Hex("#4a2c2a"), -1950);
                Util.Rect("Borde escenario", t, s.P(0f, 0.75f), new Vector2(15f, 0.15f), Util.Hex("#ffd166"), -1940);
                Util.Mosaico("Telón", t, Arte.Cortina(), s.P(0f, 3.9f), new Vector2(s.ancho - 0.4f, AltoPared), -1490);
                Util.Texto("BIENVENIDA A LOS CACHIMBOS", t, s.P(0f, 3.9f), 1.1f, Util.Hex("#ffd166"), 0);
                for (int f = 0; f < 3; f++)
                    for (int x = -9; x <= 9; x++)
                    {
                        if (x == 0 || x == -1 || x == 1) continue;
                        O(t, Arte.Butaca(), s.P(x * 1f, -0.6f - f * 1.25f));
                    }
                break;
            case "seguridad":
                OS(t, Arte.Mostrador(), s.P(0f, -3f), new Vector2(3f, 0.6f));
                OS(t, Arte.Escritorio(true, "#9be15d"), s.P(-3.8f, 2.3f), new Vector2(1.8f, 0.6f));
                OS(t, Arte.Escritorio(true, "#9be15d"), s.P(-1.7f, 2.3f), new Vector2(1.8f, 0.6f));
                O(t, Arte.Planta(), s.P(4.8f, 2.4f));
                break;
            case "ingreso":
                Util.Plano("Alfombra", t, Arte.Alfombrilla("#0b3d91"), s.P(0f, -1f), -1950).transform.localScale = new Vector3(3f, 3f, 1f);
                OS(t, Arte.Sofa("#0b3d91"), s.P(3.2f, -4.2f), new Vector2(2.4f, 0.6f));
                O(t, Arte.Planta(), s.P(-5f, -4f)); O(t, Arte.Planta(), s.P(5.2f, 2.3f));
                Util.Plano("Directorio", t, Arte.Pizarra(), s.P(-3.4f, 3.9f), -1480);
                // Puerta de vidrio hacia la Av. Héroes del Acre
                Util.Rect("Puerta de vidrio", t, s.P(0f, -4.5f), new Vector2(3f, 0.42f), Util.Hex("#9ad1e6"), -1380);
                Util.Rect("Marco vidrio", t, s.P(0f, -4.5f), new Vector2(0.12f, 0.42f), Util.Hex("#4a6572"), -1375);
                break;
            case "banos0": case "banos1":
                for (int i = 0; i < 4; i++) Util.Rect("Cubículo", t, s.P(-4.5f + i * 3f, -2f), new Vector2(0.15f, 4f), Util.Hex("#9aa5b1"), -1300);
                break;
            case "biblioteca": case "archivo":
                for (int i = 0; i < (s.ancho > 12 ? 5 : 3); i++) OS(t, Arte.Estante(), s.P(-3.8f + i * 3.8f, -4.3f), new Vector2(1.9f, 0.5f));
                if (s.id == "biblioteca") { OS(t, Arte.MesaLarga("#8d6748", 48), s.P(0f, 0.5f), new Vector2(2.8f, 0.6f)); O(t, Arte.Silla("#2a9d8f"), s.P(-1f, -0.3f)); O(t, Arte.Silla("#2a9d8f"), s.P(1f, -0.3f)); }
                break;
            case "psicologia":
                OS(t, Arte.Sofa("#6a4c93"), s.P(-2f, 1.2f), new Vector2(2.4f, 0.6f));
                O(t, Arte.Silla("#6a4c93"), s.P(2.4f, 0.5f));
                O(t, Arte.Planta(), s.P(4.8f, 2.6f));
                Util.Rect("Espejo Gesell", t, s.P(0f, 3.9f), new Vector2(5f, 0.8f), Util.Hex("#a9c6de"), -1480);
                break;
            case "economia": case "contaduria": case "administracion": case "comercial":
                FilaEscritorios(t, s, 2, 2, -2f, s.arriba ? 1.6f : 1.6f, 4f, 2.8f, true, "#80ed99", "#3d5a80");
                O(t, Arte.Planta(), s.P(5f, s.arriba ? 2.6f : -4f));
                break;
            case "diseno": case "publicidad":
                FilaEscritorios(t, s, 2, 2, -2f, 1.6f, 4f, 2.8f, true, "#ff70a6", "#ff70a6");
                Util.Plano("Afiche", t, Arte.Afiche(2), s.P(-4.5f, 3.9f), -1480);
                Util.Plano("Afiche", t, Arte.Afiche(4), s.P(4.5f, 3.9f), -1480);
                break;
            case "hoteleria":
                OS(t, Arte.Cama(), s.P(3f, -2.5f), new Vector2(1.5f, 2f));
                OS(t, Arte.Mostrador(), s.P(-2.5f, 0.5f), new Vector2(3f, 0.6f));
                O(t, Arte.Planta(), s.P(-5f, -4f));
                break;
            case "radio":
                OS(t, Arte.Escritorio(true, "#ff3b3b"), s.P(0f, 0f), new Vector2(1.8f, 0.6f));
                O(t, Arte.Microfono(), s.P(-1.5f, -1f)); O(t, Arte.Microfono(), s.P(1.5f, -1f));
                break;
            case "odontologia":
                OS(t, Arte.SillonDental(), s.P(-2.5f, 0.3f), new Vector2(1f, 0.6f));
                OS(t, Arte.SillonDental(), s.P(2.5f, 0.3f), new Vector2(1f, 0.6f));
                break;
            case "bioquimica":
                OS(t, Arte.MesaLab(), s.P(-2.5f, 0.8f), new Vector2(2.4f, 0.6f));
                OS(t, Arte.MesaLab(), s.P(2.5f, -1.8f), new Vector2(2.4f, 0.6f));
                break;
            case "servidores":
                for (int i = 0; i < 4; i++) OS(t, Arte.Rack(), s.P(-4f + i * 2.6f, 1.2f), new Vector2(1.4f, 0.6f));
                break;
            default:
                if (s.id.StartsWith("aulas"))
                {
                    int cols = Mathf.Max(2, Mathf.FloorToInt(s.ancho / 3.2f) - 1);
                    float x0 = -(cols - 1) * 1.6f;
                    float yTop = s.arriba ? 1.4f : 2.0f;
                    FilaEscritorios(t, s, cols, 3, x0, yTop, 3.2f, 2.3f, false, "#000000", "#457b9d");
                    if (!s.arriba) Util.Plano("Pizarra", t, Arte.Pizarra(), s.P(0f, 3.9f), -1480);
                }
                break;
        }
    }

    void DecorarPasillo(int p, Transform t)
    {
        Vector2 o = Origen(p);
        // Afiches en la pared de arriba del pasillo (evitando puertas)
        float yAfiche = AnchoPasillo / 2f - Grosor / 2f - AltoPared / 2f;
        int v = p;
        for (float x = -14f; x < 24f; x += 7f)
        {
            if (HayPuertaArriba(p, x)) continue;
            Util.Plano("Afiche", t, Arte.Afiche(v++), o + new Vector2(x, yAfiche), -1480);
        }
        // Luces del techo sobre el pasillo
        for (float x = -18f; x < 24f; x += 8f)
        {
            SpriteRenderer luz = Util.Plano("Luz", t, Arte.Luz(), o + new Vector2(x, -0.3f), -1945);
            luz.transform.localScale = new Vector3(2.2f, 1.4f, 1f);
        }
        // Plantas y basureros en el borde de abajo
        Util.Objeto("Planta", t, Arte.Planta(), o + new Vector2(-9f, -2.6f));
        Util.Objeto("Basurero", t, Arte.Basurero(), o + new Vector2(9f, -2.7f));
        Util.Objeto("Planta", t, Arte.Planta(), o + new Vector2(22.5f, -2.6f));
        if (p == 0)
        {
            Util.Rect("Banner", t, o + new Vector2(6f, yAfiche), new Vector2(7f, 0.7f), Util.Hex("#ffb703"), -1470);
            Util.Texto("SEMANA DE INDUCCIÓN", t, o + new Vector2(6f, yAfiche), 0.9f, Util.Hex("#3d0e0e"), 0);
        }
    }

    bool HayPuertaArriba(int p, float xLocal)
    {
        float x = -LargoPasillo / 2f;
        foreach (DefSala d in arriba[p])
        {
            float cx = x + d.ancho / 2f;
            if ((d.abierta || d.especial) && Mathf.Abs(xLocal - cx) < 3.6f) return true;
            if (!d.abierta && !d.especial && !string.IsNullOrEmpty(d.info) && Mathf.Abs(xLocal - cx) < 1.6f) return true;
            x += d.ancho;
        }
        return false;
    }
}

// Una sala ya construida en el mundo.
public class Sala
{
    public string id;
    public string nombre;
    public int piso;
    public float ancho;
    public bool arriba;
    public Rect area;
    public Vector2 Centro;
    public Vector2 Puerta;
    public Vector2 FrentePuerta;
    public GameObject tapon;

    public bool Contiene(Vector2 p) { return area.Contains(p); }

    public void CerrarPuerta() { if (tapon != null) tapon.SetActive(true); }
    public void AbrirPuerta() { if (tapon != null) tapon.SetActive(false); }

    // Punto relativo al centro de la sala (x: -12..12 en salas grandes, y: -4.5..4.5).
    public Vector2 P(float x, float y) { return Centro + new Vector2(x, y); }
}
