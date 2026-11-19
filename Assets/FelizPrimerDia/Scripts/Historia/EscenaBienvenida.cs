using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// =====================================================================
//  ESCENA 6_Bienvenida: la fiesta en el auditorio (el final del juego).
//  Caminas entre el público, examinas a los 4 Pepinos (E) y desenmascaras
//  al verdadero. Después: persecución de lado entre las butacas y la
//  confrontación con los 4 finales.
// =====================================================================
public class EscenaBienvenida : MonoBehaviour
{
    public const float Ancho = 34f, Alto = 20f;

    Juego juego;
    bool ocupado;
    int pepinoReal;
    readonly List<SpriteRenderer> pepinos = new List<SpriteRenderer>();

    public Vector2 Entrada { get { return new Vector2(0f, -Alto / 2f + 1.6f); } }

    // La morenada se deforma cerca de los Pepinos... y un poquito más cerca del verdadero (pista para el oído)
    void Update()
    {
        if (juego == null || juego.jugador == null) return;
        for (int i = 0; i < pepinos.Count; i++)
        {
            if (pepinos[i] == null) continue;
            float d = Vector2.Distance(juego.jugador.Posicion, pepinos[i].transform.position);
            float cerca = Mathf.Clamp01(1f - (d - 1.2f) / 6f);
            Sonido.Distorsion(cerca * (i == pepinoReal ? 0.9f : 0.55f));
        }
    }

    Dialogo D { get { return juego.dialogo; } }
    UIJuego UI { get { return juego.ui; } }

    public static EscenaBienvenida Crear(Juego j, Transform raiz)
    {
        GameObject go = new GameObject("Bienvenida (auditorio)");
        go.transform.SetParent(raiz, false);
        EscenaBienvenida e = go.AddComponent<EscenaBienvenida>();
        e.juego = j;
        Horno.Empezar("Bienvenida");
        try { e.Dibujar(); }
        finally { Horno.Terminar(); }
        return e;
    }

    // ---------------------------------------------------------------
    //  EL AUDITORIO (dibujado en la escena)
    // ---------------------------------------------------------------
    void Dibujar()
    {
        Transform t = transform;
        float x0 = -Ancho / 2f, x1 = Ancho / 2f, y0 = -Alto / 2f, y1 = Alto / 2f;
        Color tope = Util.Hex("#2a2733");

        // Piso y paredes
        Util.Mosaico("Alfombra", t, Arte.Suelo(TipoSuelo.Alfombra, Util.Hex("#4a0d1c")), Vector2.zero, new Vector2(Ancho, Alto), -2000);
        Util.Mosaico("Telón (pared del fondo)", t, Arte.Cortina(), new Vector2(0f, y1 - 0.8f), new Vector2(Ancho, 1.6f), -1500);
        SpriteRenderer techo = Util.Rect("Muro del fondo", t, new Vector2(0f, y1 + 0.2f), new Vector2(Ancho + 0.4f, 0.4f), tope, -1400);
        BoxCollider2D bc = Util.Colision(techo.gameObject);
        bc.size = new Vector2(1f, 2.0f / 0.4f);
        bc.offset = new Vector2(0f, -0.8f / 0.4f);
        Util.Muro(t, new Vector2(x0, 0f), new Vector2(0.4f, Alto + 0.4f), tope).GetComponent<SpriteRenderer>().sortingOrder = -1400;
        Util.Muro(t, new Vector2(x1, 0f), new Vector2(0.4f, Alto + 0.4f), tope).GetComponent<SpriteRenderer>().sortingOrder = -1400;
        // Pared de abajo con la entrada al centro
        float hueco = 3f, tramo = (Ancho - hueco) / 2f;
        Util.Muro(t, new Vector2(x0 + tramo / 2f, y0), new Vector2(tramo, 0.4f), tope).GetComponent<SpriteRenderer>().sortingOrder = -1400;
        Util.Muro(t, new Vector2(x1 - tramo / 2f, y0), new Vector2(tramo, 0.4f), tope).GetComponent<SpriteRenderer>().sortingOrder = -1400;
        GameObject tapon = Util.Muro(t, new Vector2(0f, y0), new Vector2(hueco, 0.4f), Util.Hex("#6b2b2b"));
        tapon.name = "Puerta de entrada (cerrada detrás de ti)";
        Util.Plano("Alfombrilla", t, Arte.Alfombrilla("#9d0208"), new Vector2(0f, y0 + 0.5f), -1950);

        // Escenario
        Util.Mosaico("Escenario", t, Arte.Suelo(TipoSuelo.Madera, Util.Hex("#7a4f2e")), new Vector2(0f, y1 - 3.8f), new Vector2(24f, 4.4f), -1990);
        Util.Rect("Borde del escenario", t, new Vector2(0f, y1 - 6f), new Vector2(24f, 0.18f), Util.Hex("#ffd166"), -1980);
        Util.Texto("BIENVENIDA A LOS CACHIMBOS 2026", t, new Vector2(0f, y1 - 0.8f), 1.3f, Util.Hex("#ffd166"), 0);
        Util.Objeto("Mesa del DJ", t, Arte.MesaLarga("#1b1b1f", 64), new Vector2(0f, y1 - 4.3f), new Vector2(3.8f, 0.6f));
        Util.Objeto("Parlante", t, Arte.Rack(), new Vector2(-6.5f, y1 - 4.6f), new Vector2(1.4f, 0.5f));
        Util.Objeto("Parlante", t, Arte.Rack(), new Vector2(6.5f, y1 - 4.6f), new Vector2(1.4f, 0.5f));
        Util.Objeto("Micrófono", t, Arte.Microfono(), new Vector2(-2.5f, y1 - 5.2f));
        for (float x = -10f; x <= 10f; x += 5f)
            Util.Plano("Reflector", t, Arte.Reflector(), new Vector2(x, y1 - 1.9f), 2500);

        // Luces de colores sobre la pista de baile
        string[] colores = { "#ff006e", "#3a86ff", "#ffbe0b", "#8338ec", "#06d6a0" };
        int k = 0;
        for (float x = -9f; x <= 9f; x += 4.5f)
            for (float y = -4f; y <= 2f; y += 3f)
            {
                SpriteRenderer luz = Util.Plano("Luz de fiesta", t, Arte.Luz(), new Vector2(x, y), -1945);
                luz.color = Util.Hex(colores[(k++) % colores.Length]);
                luz.transform.localScale = new Vector3(2.4f, 1.8f, 1f);
            }

        // Butacas a los costados
        for (int f = 0; f < 5; f++)
            for (int c = 0; c < 4; c++)
            {
                Util.Objeto("Butaca", t, Arte.Butaca(), new Vector2(x0 + 1.6f + c * 1.05f, -2f - f * 1.35f), new Vector2(0.8f, 0.3f));
                Util.Objeto("Butaca", t, Arte.Butaca(), new Vector2(x1 - 1.6f - c * 1.05f, -2f - f * 1.35f), new Vector2(0.8f, 0.3f));
            }

        // Mesa de bebidas y decoración
        Util.Objeto("Mesa de bebidas", t, Arte.MesaLarga("#8d6748", 48), new Vector2(12.5f, 2.5f), new Vector2(2.8f, 0.6f));
        Util.Texto("Bebidas", t, new Vector2(12.5f, 4.1f), 0.8f, Util.Hex("#ffe8a3"), 0);
        Util.Objeto("Planta", t, Arte.Planta(), new Vector2(x0 + 1.2f, y1 - 2.6f));
        Util.Objeto("Planta", t, Arte.Planta(), new Vector2(x1 - 1.2f, y1 - 2.6f));
        Util.Objeto("Basurero", t, Arte.Basurero(), new Vector2(-12.5f, 2.2f));

        // Salidas de emergencia
        Util.Plano("Salida izquierda", t, Arte.LetreroSalida(), new Vector2(x0 + 1.2f, 1.5f), -1300);
        Util.Plano("Salida derecha", t, Arte.LetreroSalida(), new Vector2(x1 - 1.2f, -0.5f), -1300);
        Util.Texto("SALIDA DE EMERGENCIA (cerrada)", t, new Vector2(x0 + 3.4f, 0.8f), 0.55f, Util.Hex("#7bd389"), 0);
    }

    // ---------------------------------------------------------------
    //  EL PÚBLICO Y LOS CUATRO PEPINOS (se crean al jugar)
    // ---------------------------------------------------------------
    public void Poblar()
    {
        Horno.Suspender();
        System.Random r = new System.Random(Memoria.Dia * 7 + 1);
        string[] ropas = { "#e63946", "#457b9d", "#2a9d8f", "#f4a261", "#8338ec", "#06d6a0", "#ef476f", "#118ab2", "#ffb703", "#6a4c93" };
        string[] pieles = { "#c68a5a", "#a47148", "#e0ac69", "#8d5524", "#d4a373" };
        string[] pelos = { "#1b1b1b", "#2b1d14", "#5a3825", "#111111", "#6f1d1b" };
        for (int i = 0; i < 24; i++)
        {
            Vector2 pos = new Vector2(-8.5f + (float)r.NextDouble() * 17f, -5.5f + (float)r.NextDouble() * 8f);
            if (Vector2.Distance(pos, Entrada) < 3f) continue;
            Apariencia a = Arte.Ap(ropas[r.Next(ropas.Length)], pieles[r.Next(pieles.Length)], pelos[r.Next(pelos.Length)], "#22223b",
                                   r.Next(2) == 0, r.Next(4) == 0 ? Accesorio.Gorra : Accesorio.Ninguno, ropas[r.Next(ropas.Length)]);
            Figura("Invitado", pos, Arte.Personaje(a).abajo[0], true);
        }

        Vector2[] todas = { new Vector2(-6f, 1.5f), new Vector2(5f, -3.5f), new Vector2(-2.5f, -4.5f), new Vector2(7f, 1.8f),
                            new Vector2(1f, 1.2f), new Vector2(-7.5f, -2.5f) };
        Vector2[] posPepinos = new Vector2[Mathf.Clamp(Dificultad.PepinosFiesta, 2, todas.Length)];
        System.Array.Copy(todas, posPepinos, posPepinos.Length);
        pepinoReal = Random.Range(0, posPepinos.Length);
        for (int i = 0; i < posPepinos.Length; i++)
        {
            int indice = i;
            SpriteRenderer sr = Figura("Pepino " + (i + 1), posPepinos[i], Arte.Pepino().abajo[0], true);
            pepinos.Add(sr);
            Interactuable it = sr.gameObject.AddComponent<Interactuable>();
            it.accion = "Examinar al Pepino";
            it.radio = 1.6f;
            it.alInteractuar = delegate { Lanzar(Examinar(indice)); };
            it.disponible = delegate { return !Protagonista.EsMateo; };     // Mateo no examina Pepinos: va a la cabina
        }
        Horno.Reanudar();
    }

    SpriteRenderer Figura(string nombre, Vector2 pos, Sprite s, bool baila)
    {
        GameObject go = new GameObject(nombre);
        go.transform.SetParent(transform, false);
        go.transform.position = pos;
        go.layer = Util.CapaSinVision;
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = s;
        Util.AplicarMaterial(sr);
        go.AddComponent<OrdenPorY>().dinamico = true;
        CircleCollider2D c = go.AddComponent<CircleCollider2D>();
        c.radius = 0.3f;
        c.offset = new Vector2(0f, -0.4f);
        Util.Plano("Sombra", go.transform, Arte.Sombra(), pos + new Vector2(0f, -0.72f), -900);
        if (baila) go.AddComponent<Bailarin>();
        return sr;
    }

    // ---------------------------------------------------------------
    //  FLUJO DEL FINAL
    // ---------------------------------------------------------------
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

    public IEnumerator Flujo()
    {
        if (Protagonista.EsMateo) { yield return StartCoroutine(FlujoMateo()); yield break; }
        juego.Bloquear(this);
        UI.Oscuridad(0.3f);
        yield return new WaitForSeconds(0.3f);
        yield return D.DecirCo("Valeria",
            "La puerta se cierra detrás de mí.",
            "Música a todo volumen, luces de colores, decenas de estudiantes disfrazados...",
            "...y " + (pepinos.Count == 6 ? "SEIS" : "CUATRO") + " personas con máscara de Pepino bailando entre la gente.",
            "Uno de ellos es el verdadero. Tengo que encontrarlo antes de que él me encuentre a mí.");
        juego.Desbloquear(this);
        UI.Toast("Acércate a cada Pepino y examínalo (E). Cuando estés segura, desenmascáralo.", 6f);
    }

    // =================================================================
    //  LA BIENVENIDA DE MATEO: la cabina de luces
    // =================================================================
    IEnumerator FlujoMateo()
    {
        Vector2 cabina = new Vector2(-Ancho / 2f + 2.5f, -Alto / 2f + 2.6f);
        Horno.Suspender();
        Util.Rect("Cabina de luces", transform, cabina + new Vector2(0f, 0.9f), new Vector2(2.6f, 1.6f), Util.Hex("#1b1822"), OrdenPorY.Orden(cabina.y));
        Util.Rect("Ventana de la cabina", transform, cabina + new Vector2(0f, 1.2f), new Vector2(2f, 0.6f), Util.Hex("#4cc9f0"), OrdenPorY.Orden(cabina.y) + 1);
        Util.Texto("CABINA", transform, cabina + new Vector2(0f, 1.9f), 0.6f, Util.Hex("#ffd166"), 9);
        Horno.Reanudar();
        Interactuable.Crear(transform, cabina, "Subir a la cabina de luces", delegate { Lanzar(CabinaMateo()); }, 1.6f);

        juego.Bloquear(this);
        UI.Oscuridad(0.3f);
        yield return new WaitForSeconds(0.3f);
        yield return D.DecirCo("Mateo",
            "Música a todo volumen. Cuatro Pepinos bailando. Valeria buscando al verdadero.",
            "Sebastián está aquí, en algún lado, con el quinto disfraz.",
            "Tengo que llegar a la cabina de luces, al fondo a la izquierda.");
        juego.Desbloquear(this);
        UI.Toast("Ve a la CABINA DE LUCES (fondo izquierdo). Necesitas la llave de Don Teodoro.", 6f);
    }

    IEnumerator CabinaMateo()
    {
        if (!Memoria.Tiene("llave_cabina"))
        {
            yield return D.DecirCo("Mateo", "Cerrada con llave. Don Teodoro tiene la llave de la cabina... y no me la dio.");
            yield break;
        }
        juego.Bloquear(this);
        yield return D.DecirCo("Mateo", "Desde la cabina se ve todo el auditorio. Conecto la laptop al proyector...",
            "(Detrás de ti se abre la puerta. Un Pepino DORADO.)");
        yield return D.DecirCo("Sebastián", "(Se levanta la máscara.) Así que tú eres el que anda buscando mis videos.", "Dámelos, cachimbo.");
        juego.Desbloquear(this);

        Resultado<bool> ok = new Resultado<bool>();
        yield return Persecucion.Jugar(EscenarioPersecucion.Auditorio, 0, ok);
        if (!ok.valor)
        {
            juego.Morir("Sebastián te alcanzó en la escalera de la cabina.\nLa laptop cayó al piso. Los videos, con ella.", null, TipoMuerte.Empujon);
            yield break;
        }

        juego.Bloquear(this);
        int archivos = Memoria.Cuantos(Historia.ArchivosMateo);
        int aliados = Memoria.Cuantos(Historia.AliadosMateo);
        yield return D.DecirCo("Mateo", "Volví a la cabina y cerré la puerta. Tengo " + archivos + "/4 videos y " + aliados + "/3 aliados.");
        List<string> ops = new List<string>();
        List<string> fin = new List<string>();
        if (archivos >= 4 && Memoria.Tiene(Historia.AliadosMateo[0])) { ops.Add("Proyectar los videos y que Joaquín lo transmita en vivo"); fin.Add("mateo_envivo"); }
        if (archivos >= 4 && aliados >= 3 && Memoria.Sabe("m_aviso")) { ops.Add("Proyectar los videos... y bajar a hablar con Camila"); fin.Add("mateo_testigo"); }
        ops.Add("Quedarme en la cabina, solo mirando"); fin.Add("mateo_espectador");
        ops.Add("Aceptar la plata de Sebastián y borrar los videos"); fin.Add("mateo_borrado");
        Resultado<int> r = new Resultado<int>();
        yield return D.ElegirCo("Mateo", "¿Qué hago?", ops.ToArray(), r);
        juego.Desbloquear(this);
        string f = fin[r.valor];
        yield return StartCoroutine(CineMateo.Final(f));
        switch (f)
        {
            case "mateo_envivo":
                juego.Final("FINAL DE MATEO\nEN VIVO",
                    "Los videos se proyectan en la pantalla gigante y Joaquín los transmite por la radio.\n" +
                    "Todo La Paz ve la cara de Sebastián Quispe. Don Teodoro y seguridad lo detienen.\n\n" +
                    "Por fin alguien que miraba, actuó. Y el día termina.\n\n" +
                    "<size=26><color=#aaaaaa>Jueves. Mateo vuelve a su laboratorio. Esta vez deja la puerta abierta.</color></size>", true, f);
                break;
            case "mateo_testigo":
                juego.Final("FINAL VERDADERO DE MATEO\nTESTIGO",
                    "Mateo proyecta la verdad y baja del escenario.\n" +
                    "\"Yo no estaba esa noche... pero estuve mirando mil noches. Perdón por tardar.\"\n\n" +
                    "Camila se quita los guantes. Valeria le toma la mano.\n" +
                    "El jueves, los tres salen juntos de la U. Nadie se queda mirando.\n\n" +
                    "<size=26><color=#aaaaaa>Un año después, Mateo programa el sistema de historias clínicas de la clínica de la U.\n" +
                    "Su primera usuaria: una estudiante de Medicina llamada Valeria Quispe.</color></size>", true, f);
                break;
            case "mateo_espectador":
                juego.Final("FINAL DE MATEO\nSOLO UN ESPECTADOR",
                    "Mateo se queda en la cabina. Mira todo por los monitores.\nNo baja. No hace nada.\n\n" +
                    "El día vuelve a empezar. Y otra vez. Y otra vez.\nCon el tiempo, ya nadie lo llama Mateo: es el guardia nocturno del cuarto de abajo.\n\n" +
                    "<color=#ff4d4d>6:45.  6:45.  6:45.</color>", false, f);
                break;
            default:
                juego.Final("FINAL DE MATEO\nBORRADO",
                    "Mateo borra los cuatro videos. Sebastián le deja la plata sobre el escritorio y se va sonriendo.\n\n" +
                    "Lunes. 6:45. La alarma.\nAhora solo Mateo recuerda lo que pasó esa noche.\nY ya no hay ninguna prueba.", true, f);
                break;
        }
    }

    string Descripcion(int i)
    {
        bool sabeCicatriz = Memoria.Sabe("cicatriz");
        if (i == pepinoReal)
            return sabeCicatriz
                ? "Lleva guantes blancos aunque hace calor. Por el borde del guante se ve piel arrugada, brillante...\nCICATRICES DE QUEMADURAS. Como en la grabación. Deja de bailar y te mira."
                : Memoria.Sabe("camara_vieja")
                    ? "Baila con la música. Entre el guante y la manga asoma una CINTA ROJA en la muñeca. Como en el monitor del cuarto secreto."
                    : "Baila con la música. Tiene guantes blancos y la máscara sonriente. No se ve nada raro... ¿o sí?";
        string[] falsos = sabeCicatriz
            ? new[] {
                "Baila sin parar y se ríe a carcajadas. Se saca un guante para tomar un vaso: su mano no tiene ninguna marca.",
                "Es bajito y usa zapatillas de colores. Te ofrece refresco. Sus muñecas están limpias, sin cicatrices.",
                "Se levanta la máscara un segundo para respirar: es un chico de Contaduría, sudando. Te guiña el ojo.",
                "Se acomoda el guante y le ves la mano un segundo: uñas pintadas de verde, piel sin marcas.",
                "Tiene un tatuaje de un cóndor en la muñeca. Ninguna cicatriz. Te pide una selfie." }
            : new[] {
                "Baila con la música. Tiene guantes blancos y la máscara sonriente.",
                "Baila con la música. Es un poco más bajito que los demás. Tiene guantes.",
                "Baila con la música. Tiene guantes y la máscara sonriente. Te saluda.",
                "Baila torpemente. Tiene guantes. Se le cae la máscara y la recoge rápido.",
                "No baila: está quieto junto a la pared, con guantes. Te mira... o eso parece." };
        int k = 0;
        for (int j = 0; j < pepinos.Count; j++)
        {
            if (j == pepinoReal) continue;
            if (j == i) return falsos[k];
            k++;
        }
        return falsos[0];
    }

    IEnumerator Examinar(int i)
    {
        yield return D.DecirCo("Valeria", Descripcion(i));
        Resultado<int> r = new Resultado<int>();
        yield return D.ElegirCo("Valeria", "¿Es él?", new[] { "¡Eres tú! (quitarle la máscara)", "Seguir buscando" }, r);
        if (r.valor != 0) yield break;

        if (i != pepinoReal)
        {
            yield return D.DecirCo("Valeria", "Le arranco la máscara... y es un chico cualquiera, asustado.", "Todos nos miran. Alguien apaga la música.");
            juego.Morir("Elegiste al Pepino equivocado.\nEl verdadero estaba detrás de ti. \"Feliz primer día\", te susurró.", "quien_es", TipoMuerte.Cuchillo);
            yield break;
        }
        yield return StartCoroutine(Confrontar());
    }

    IEnumerator Confrontar()
    {
        // Persecución final (de lado, entre las butacas y el público)
        yield return D.DecirCo("Pepino", "(Te aparta la mano antes de que toques la máscara. Saca un cuchillo.)", "¡Corre detrás del telón!");
        Resultado<bool> ok = new Resultado<bool>();
        yield return Persecucion.Jugar(EscenarioPersecucion.Auditorio, 0, ok);
        if (!ok.valor)
        {
            juego.Morir("Te alcanzó entre las butacas. La música siguió sonando.\nNadie escuchó nada.", null, TipoMuerte.Cuchillo);
            yield break;
        }
        int pruebas = Memoria.Cuantos(Ids.Pruebas);
        yield return StartCoroutine(CineHistoria.Desenmascarar(juego, pepinos[pepinoReal], pruebas >= 4));
        juego.Bloquear(this);

        int charlas = Memoria.CuantosRecuerdos(Ids.CharlasCamila);
        List<string> opciones = new List<string>();
        List<int> finales = new List<int>();   // 0 bueno, 1 verdadero, 2 oscuro, 3 malo, 4 huir, 5 ñatita

        if (pruebas >= 4)
        {
            yield return D.DecirCo("Valeria", "Camila me mira, con la máscara de Pepino en la mano.", "Todo el auditorio nos está mirando.");
            opciones.Add("Proyectar las 4 pruebas en la pantalla gigante"); finales.Add(0);
            if (charlas >= 4) { opciones.Add("\"Sé lo que te hizo mi hermano.\""); finales.Add(1); }
            if (charlas >= 4 && Memoria.Sabe("secreto") && Memoria.Tiene("ofrenda_natita"))
            { opciones.Add("\"Yo vi el video. Y me reí.\" (pedirle a la ñatita juntas)"); finales.Add(5); }
            opciones.Add("Quitarle el cuchillo... y usarlo"); finales.Add(2);
        }
        else
        {
            yield return D.DecirCo("Valeria", "Detrás del telón, el Pepino me tiene acorralada.",
                "No tengo pruebas suficientes (" + pruebas + "/4). Nadie me va a creer...");
            opciones.Add("Gritar a todos que Andrea es la asesina"); finales.Add(3);
            opciones.Add("Quitarle el cuchillo... y usarlo"); finales.Add(2);
            opciones.Add("Huir por la salida de emergencia"); finales.Add(4);
        }

        Resultado<int> r = new Resultado<int>();
        yield return D.ElegirCo("Valeria", "¿Qué hago?", opciones.ToArray(), r);
        juego.Desbloquear(this);

        // Cinemática del final elegido
        string[] cines = { "final_bueno", "final_verdadero", "final_oscuro", "final_malo", null, "final_natita" };
        string cine = cines[finales[r.valor]];
        if (cine != null) yield return StartCoroutine(CineFinales.Reproducir(cine));

        switch (finales[r.valor])
        {
            case 0:
                juego.Final("FINAL BUENO\nJUEVES",
                    "Subes al escenario y conectas tu celular al proyector.\n" +
                    "La historia clínica. El expediente. La grabación. Los registros de 'c.rojas'.\n" +
                    "Todo el auditorio ve a Camila con la máscara en la mano. Don Teodoro y seguridad la rodean.\n\n" +
                    "Valeria despierta. 6:45. La alarma.\nMira el celular: JUEVES.\n" +
                    "Se acabó. Y por fin sabe qué carrera quiere estudiar.\n\n" +
                    "<size=26><color=#aaaaaa>Una semana después, Sebastián recibe una foto anónima:\nuna máscara de Pepino en la puerta de su casa.</color></size>", true, "final_bueno");
                break;
            case 1:
                juego.Final("FINAL VERDADERO\nPERDÓN",
                    "No proyectas nada. Te acercas despacio.\n" +
                    "\"Sé lo que te hizo mi hermano. Voy a denunciarlo contigo. Esta vez te van a creer: tengo pruebas.\"\n\n" +
                    "Camila se quita la máscara. Debajo, las cicatrices. Llora.\nEl cuchillo cae al piso.\n\n" +
                    "Nadie más muere.\nAl día siguiente —un jueves, por fin— Valeria y Camila entran juntas a la fiscalía.\nEse semestre, Valeria se inscribe en Derecho.\n\n" +
                    "<size=26><color=#aaaaaa>...pero cada 8 de noviembre, Camila sigue soñando con el mismo día.\n(Hay un final más. Pregúntale a la ñatita.)</color></size>", true, "final_verdadero");
                break;
            case 2:
                juego.Final("FINAL OSCURO\nLA NUEVA PEPINO",
                    "Le arrancas el cuchillo. Y lo usas.\nEl loop se rompe.\n\n" +
                    "Jueves por la mañana. Valeria abre su mochila para ir a clases.\n" +
                    "Adentro está la máscara de Pepino.\n\nSonríe.", true, "final_oscuro");
                break;
            case 3:
                Memoria.Aprender("final_malo");
                juego.Final("FINAL MALO\nBUCLE ETERNO",
                    "\"¡Es Andrea! ¡Tiene el disfraz en la mochila!\"\n" +
                    "Nadie te cree. Andrea llora. Alguien se ríe.\n\n" +
                    "Una mano con guante te toca el hombro.\n\"Feliz primer día\", te susurran al oído.\n\n" +
                    "<color=#ff4d4d>6:45.  6:45.  6:45.</color>", false, "final_malo");
                break;
            case 5:
                juego.Final("FINAL VERDADERO\nLAS DOS DESPIERTAN",
                    "\"Yo vi el video. Hace un año. Sebastián lo mandó al grupo de la familia... y yo me reí.\"\n" +
                    "Camila baja el cuchillo. Nadie le había dicho eso nunca: que lo vieron. Que fue real.\n\n" +
                    "Le tomas la mano, la de las cicatrices, y le hablas de la ñatita. De las velas que siguen encendidas en el Piso 3.\n" +
                    "A medianoche suben juntas a Odontología. Las dos piden lo mismo: que el día termine.\n" +
                    "Las velas se apagan solas.\n\n" +
                    "Jueves. 6:45. Dos alarmas suenan en dos casas distintas.\n" +
                    "Valeria escribe primero: \"¿Fiscalía a las 9?\"\nCamila responde con un solo emoji. Esta vez no es de risa.\n\n" +
                    "<size=26><color=#aaaaaa>Ese semestre Valeria se inscribió en MEDICINA. Quiere salvar vidas, no mirarlas desde un celular.\n" +
                    "Cada 8 de noviembre baja a Odontología con flores, coca y una vela para la ñatita.\n" +
                    "Camila volvió a Periodismo. Su primer reportaje: \"Novatadas: lo que la U prefiere olvidar\".</color></size>", true, "final_natita");
                break;
            default:
                juego.Morir("La salida de emergencia estaba cerrada con cadena.\nDesde adentro, alguien había puesto un candado nuevo.", null);
                break;
        }
    }
}
