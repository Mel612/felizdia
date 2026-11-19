using System.Collections;
using UnityEngine;

// =====================================================================
//  CINEMÁTICAS DE LOS FINALES
//  Se arman en un "estudio" lejos del campus (x = 30000) con vista de lado,
//  como las persecuciones: pared al fondo, piso, muebles y personajes.
//  Cada final tiene varios planos (escenas cortas) y termina en negro;
//  después aparece la pantalla del final con su texto.
// =====================================================================
public static class CineFinales
{
    static Vector2 B;              // origen del plano actual
    static Transform set;          // carpeta con los objetos del plano actual
    static int planos;

    public static Vector2 Origen { get { return B; } }

    // ---------- Modo galería: los planos quedan dibujados uno al lado del otro ----------
    public static bool ModoGaleria;
    public static float FilaGaleria;
    public static string EtiquetaGaleria = "";
    static int planosFila;

    public static void NuevaFila(float y) { FilaGaleria = y; planosFila = 0; }
    public static Transform Set { get { return set; } }

    public static IEnumerator Reproducir(string final)
    {
        Juego j = Juego.I;
        if (j == null) yield break;
        Cine c = Cine.Empezar(true);
        planos = 0;
        yield return c.Fundido(1f, 0.6f);
        switch (final)
        {
            case "final_bueno": yield return Bueno(c); break;
            case "final_verdadero": yield return Verdadero(c); break;
            case "final_natita": yield return Natita(c); break;
            case "final_oscuro": yield return Oscuro(c); break;
            case "final_malo": yield return Malo(c); break;
            case "final_pesadilla": yield return Pesadilla(c); break;
        }
        yield return c.Fundido(1f, 1f);
        BorrarSet();
        yield return c.Terminar();
        j.ui.Telon(1f, 0f);          // queda en negro hasta la pantalla del final
    }

    // =================================================================
    //  FINAL BUENO: las pruebas en la pantalla gigante
    // =================================================================
    static IEnumerator Bueno(Cine c)
    {
        Sonido.Musica(null, 0.5f);
        // Plano 1: el escenario, la pantalla con las pruebas
        Auditorio(c, true);
        Pj(Arte.Valeria(), -3.5f, false);
        SpriteRenderer cam = Pj(Arte.Camila(), 3.5f, true);
        Pantalla(new Vector2(0f, 3.4f), "HISTORIA CLÍNICA  ·  EXPEDIENTE\nGRABACIÓN  ·  USUARIO c.rojas");
        c.Camara(B + new Vector2(0f, 2.1f), 3.6f);
        yield return c.Fundido(0f, 1f);
        Sonido.Efecto("click", 0.8f, 0.8f);
        yield return c.Decir("Valeria", "Esto es lo que pasó en la Bienvenida del año pasado.", 3f);
        Sonido.Efecto("estatica", 0.4f, 0.6f);
        yield return c.Decir(null, "(Todo el auditorio mira la pantalla. La música se apaga.)", 2.6f);
        yield return c.Mover(B + new Vector2(2.5f, 1.6f), 2.6f, 1.2f);
        yield return c.Decir("Camila", "...", 1.4f);

        // Plano 2: llega seguridad
        SpriteRenderer teo = Pj(Arte.Teodoro(), 9f, true);
        SpriteRenderer gua = Pj(Arte.Guardia(), 10.2f, true);
        Coroutine a = c.StartCoroutine(c.CaminarLado(teo, Arte.Teodoro(), 4.6f, 2f));
        yield return c.StartCoroutine(c.CaminarLado(gua, Arte.Guardia(), 5.8f, 2f));
        yield return a;
        yield return c.Decir("Don Teodoro", "Señorita Camila... acompáñenos, por favor.", 2.8f);
        cam.sprite = Arte.Camila().agachada;
        yield return c.Esperar(1f);
        yield return c.Fundido(1f, 0.8f);

        // Plano 3: el dormitorio, jueves
        Dormitorio(c, "#c8b6ff", "#7b2cbf", false);
        SpriteRenderer dormida = Acostada(Arte.Valeria());
        TextMesh reloj = Reloj("LUN 6:45");
        c.Camara(B + new Vector2(0f, 1.8f), 3f);
        yield return c.Fundido(0f, 1f);
        Sonido.Efecto("alarma", 0.35f, 1.6f);
        yield return c.Esperar(1.2f);
        reloj.text = "JUE 6:45";
        reloj.color = Util.Hex("#7bd389");
        Sonido.Musica("m_triste", 1.5f);
        Sentarse(dormida, Arte.Valeria());
        yield return c.Decir("Valeria", "...¿Jueves?", 2f);
        yield return c.Decir("Valeria", "Se acabó. Y por fin sé qué quiero estudiar.", 3f);
        yield return c.Fundido(1f, 1f);

        // Plano 4: una semana después
        Calle(c, true);
        Obj(Arte.Puerta("#5c3d2e"), 0f, 0f, -2900);
        Obj(Arte.Mascara(), 0.2f, 0f, 10);
        Texto("CASA DE SEBASTIÁN QUISPE", 0f, 3.6f, 0.9f, Util.Hex("#cfcfcf"));
        c.Camara(B + new Vector2(0f, 1.4f), 2.6f);
        c.Cartel("UNA SEMANA DESPUÉS", "", 2.5f);
        yield return c.Fundido(0f, 1.2f);
        Sonido.Efecto("susto", 0.4f, 0.6f);
        yield return c.Esperar(2.5f);
    }

    // =================================================================
    //  FINAL VERDADERO: perdón
    // =================================================================
    static IEnumerator Verdadero(Cine c)
    {
        Sonido.Musica(null, 0.5f);
        DetrasDelTelon(c);
        Pj(Arte.Valeria(), -1.4f, false);
        SpriteRenderer pep = Pj(Arte.Pepino(), 1.4f, true);
        SpriteRenderer cuchillo = Obj(Arte.Cuchillo(), 1.0f, 0.75f, 600);
        c.Camara(B + new Vector2(0f, 1.3f), 2.4f);
        yield return c.Fundido(0f, 1f);
        yield return c.Decir("Valeria", "Sé lo que te hizo mi hermano. Voy a denunciarlo contigo.", 3f);
        yield return c.Decir("Valeria", "Esta vez te van a creer. Tengo pruebas.", 2.6f);
        // Se quita la máscara
        pep.sprite = Arte.Camila().lado[0];
        pep.flipX = true;
        Sonido.Efecto("golpe", 0.4f, 1.4f);
        yield return c.Esperar(0.8f);
        // El cuchillo cae
        float t = 0f;
        Vector3 p0 = cuchillo.transform.position;
        while (t < 0.35f && !c.Saltada)
        {
            t += Time.deltaTime;
            cuchillo.transform.position = p0 + new Vector3(0f, -0.75f * (t / 0.35f) * (t / 0.35f), 0f);
            cuchillo.transform.localRotation = Quaternion.Euler(0, 0, -t * 500f);
            yield return null;
        }
        Sonido.Efecto("cuchillo", 0.5f, 0.7f);
        Sonido.Musica("m_triste", 2f);
        yield return c.Decir("Camila", "...Nadie me había creído. Nunca.", 2.8f);
        pep.sprite = Arte.Camila().agachada;
        yield return c.Esperar(1f);
        yield return c.Fundido(1f, 1f);

        // Plano 2: la fiscalía
        Calle(c, false);
        Obj(Arte.Puerta("#3d405b"), 2f, 0f, -2900);
        Util.Rect("Letrero", set, B + new Vector2(2f, 3.7f), new Vector2(6.5f, 0.8f), Util.Hex("#0b3d91"), -2850);
        Texto("FISCALÍA DEPARTAMENTAL  ·  LA PAZ", 2f, 3.7f, 0.7f, Color.white);
        SpriteRenderer v2 = Pj(Arte.Valeria(), -6f, false);
        SpriteRenderer c2 = Pj(Arte.Camila(), -7.2f, false);
        c.Camara(B + new Vector2(-1f, 1.8f), 3.4f);
        c.Cartel("JUEVES", "9:00 a.m.", 2.5f);
        yield return c.Fundido(0f, 1f);
        Coroutine a = c.StartCoroutine(c.CaminarLado(c2, Arte.Camila(), 1.2f, 1.6f));
        yield return c.StartCoroutine(c.CaminarLado(v2, Arte.Valeria(), 2.4f, 1.6f));
        yield return a;
        yield return c.Decir(null, "(Entran juntas.)", 2f);
        Sonido.Efecto("puerta", 0.5f, 1f);
        v2.enabled = c2.enabled = false;
        yield return c.Esperar(1.5f);
    }

    // =================================================================
    //  FINAL DE LA ÑATITA: las dos despiertan
    // =================================================================
    static IEnumerator Natita(Cine c)
    {
        Sonido.Musica("m_oscuro", 1f);
        // Plano 1: Odontología de noche, la ñatita y las velas
        Nuevo();
        Fondo(c, Util.Hex("#9fb8c4"), Arte.Suelo(TipoSuelo.Clinico, Util.Hex("#cfdcdc")), true);
        Obj(Arte.VentanaIllimani(true), -4f, 3.4f, -2970);
        Obj(Arte.Natita(), 0f, 0f, 5);
        Obj(Arte.Vela(), -1f, 0f, 6); Obj(Arte.Vela(), 1f, 0f, 6);
        SpriteRenderer luz = Obj(Arte.Luz(), 0f, 1.2f, 7);
        luz.color = new Color(1f, 0.65f, 0.25f, 0.55f);
        luz.transform.localScale = new Vector3(4f, 4f, 1f);
        SpriteRenderer val = Pj(Arte.Valeria(), -6f, false);
        SpriteRenderer cam = Pj(Arte.Camila(), 6f, true);
        c.Camara(B + new Vector2(0f, 1.6f), 3.2f);
        c.Cartel("MEDIANOCHE", "Odontología  ·  Piso 3", 2.5f);
        yield return c.Fundido(0f, 1.2f);
        Coroutine a = c.StartCoroutine(c.CaminarLado(val, Arte.Valeria(), -1.9f, 1.4f));
        yield return c.StartCoroutine(c.CaminarLado(cam, Arte.Camila(), 1.9f, 1.4f));
        yield return a;
        yield return c.Decir("Valeria", "Yo vi el video. Hace un año. Y me reí. Perdón.", 3.2f);
        yield return c.Decir("Camila", "Yo pedí que tuvieras miedo. Ya no quiero que nadie tenga miedo.", 3.2f);
        yield return c.Mover(B + new Vector2(0f, 1.2f), 2.2f, 1.5f);
        yield return c.Decir("Las dos", "Que termine el día.", 2.6f);
        for (int i = 0; i < 5 && !c.Saltada; i++)
        {
            luz.transform.localScale = new Vector3(Random.Range(3f, 4.5f), Random.Range(3f, 4.5f), 1f);
            yield return c.Esperar(0.15f);
        }
        luz.enabled = false;
        Sonido.Efecto("trueno", 0.7f, 1f);
        Juego.I.ui?.Telon(1f, 0f);
        yield return c.Esperar(1.5f);

        // Plano 2: el dormitorio de Valeria, jueves
        Sonido.Musica("m_triste", 1f);
        Dormitorio(c, "#c8b6ff", "#7b2cbf", false);
        SpriteRenderer v = Acostada(Arte.Valeria());
        TextMesh reloj = Reloj("JUE 6:45");
        reloj.color = Util.Hex("#7bd389");
        c.Camara(B + new Vector2(0f, 1.8f), 3f);
        yield return c.Fundido(0f, 1f);
        Sonido.Efecto("alarma", 0.3f, 1.6f);
        Sentarse(v, Arte.Valeria());
        yield return c.Decir("Valeria (mensaje)", "¿Fiscalía a las 9?", 2.6f);
        yield return c.Fundido(1f, 0.6f);

        // Plano 3: el dormitorio de Camila
        Dormitorio(c, "#ffd6a5", "#e63946", true);
        SpriteRenderer cm = Acostada(Arte.Camila());
        TextMesh r2 = Reloj("JUE 6:45");
        r2.color = Util.Hex("#7bd389");
        c.Camara(B + new Vector2(0f, 1.8f), 3f);
        yield return c.Fundido(0f, 0.8f);
        Sonido.Efecto("blip", 0.6f, 1.5f);
        Sentarse(cm, Arte.Camila());
        yield return c.Decir(null, "(Camila responde con un solo emoji. Esta vez no es de risa.)", 3.2f);
        yield return c.Esperar(1f);
        yield return c.Fundido(1f, 0.8f);

        // Plano 4: un año después. Valeria (Medicina) y Mateo (Sistemas) cuidan a la ñatita.
        Nuevo();
        Fondo(c, Util.Hex("#cfe3ea"), Arte.Suelo(TipoSuelo.Clinico, Util.Hex("#e3eeec")), true);
        Obj(Arte.VentanaIllimani(false), -4f, 3.4f, -2970);
        Obj(Arte.Natita(), 0f, 0f, 5);
        Obj(Arte.Vela(), -0.9f, 0f, 6); Obj(Arte.Ramo(), 0.9f, 0f, 6);
        Util.Rect("Cámara", set, B + new Vector2(1.6f, 4.6f), new Vector2(0.5f, 0.3f), Util.Hex("#343a40"), -2900);
        Util.Rect("Luz de la cámara", set, B + new Vector2(1.75f, 4.55f), new Vector2(0.08f, 0.08f), Util.Hex("#ff4d4d"), -2899);
        Pj(Arte.ValeriaMedicina(), -2.2f, false);
        Pj(Arte.Mateo(), 2.3f, true);
        Obj(Arte.Laptop(), 2.9f, 0.85f, 600);
        c.Camara(B + new Vector2(0f, 1.8f), 3.2f);
        c.Cartel("UN AÑO DESPUÉS", "8 de noviembre  ·  Odontología, Piso 3", 3f);
        yield return c.Fundido(0f, 1.2f);
        yield return c.Decir("Mateo", "Listo. La cámara la vigila día y noche. Y conectada al sistema de la clínica.", 3.2f);
        yield return c.Decir("Valeria", "Flores, coca y una vela. Cada año. Pero nunca más un favor con rabia.", 3.2f);
        yield return c.Decir("Mateo", "Medicina y Sistemas cuidando a una ñatita... nadie nos va a creer.", 3f);
        yield return c.Fundido(1f, 0.6f);

        // Plano 5: esa noche, en el monitor de Mateo...
        Juego.I.ui?.Recuerdo(true, "CAM ÑATITA  ·  08/11  ·  23:58");
        Nuevo();
        Fondo(c, Util.Hex("#2b3a3a"), Arte.Suelo(TipoSuelo.Clinico, Util.Hex("#7f9090")), true);
        Obj(Arte.Natita(), 0f, 0f, 5);
        SpriteRenderer figura = Pj(Arte.Personaje(Arte.Ap("#1b1b1f", "#c68a5a", "#111111", "#1b1b1f", false, Accesorio.Ninguno, null)), 6f, true);
        figura.color = new Color(0.3f, 0.3f, 0.35f);
        Obj(Arte.Vela(), 6f, 0.9f, 501);
        c.Camara(B + new Vector2(1.5f, 1.4f), 2.6f);
        yield return c.Fundido(0f, 0.6f);
        yield return c.StartCoroutine(c.CaminarLado(figura, Arte.Personaje(Arte.Ap("#1b1b1f", "#c68a5a", "#111111", "#1b1b1f", false, Accesorio.Ninguno, null)), 1.3f, 1f));
        Sonido.Efecto("susto", 0.5f, 0.6f);
        yield return c.Decir("???", "(Susurra.) Ñatita... quiero pedirte un favor.", 3f);
        Juego.I.ui?.Recuerdo(false, null);
        c.Cartel("CONTINUARÁ...", "", 3f);
        yield return c.Esperar(2.6f);
    }

    // =================================================================
    //  FINAL OSCURO: la nueva Pepino
    // =================================================================
    static IEnumerator Oscuro(Cine c)
    {
        Sonido.Musica(null, 0.3f);
        DetrasDelTelon(c);
        SpriteRenderer val = Pj(Arte.Valeria(), -1.4f, false);
        Pj(Arte.Camila(), 1.4f, true);
        c.Camara(B + new Vector2(0f, 1.3f), 2.4f);
        yield return c.Fundido(0f, 0.8f);
        yield return c.Esperar(0.6f);
        val.sprite = Arte.Valeria().ataque;
        Obj(Arte.Cuchillo(), -0.6f, 0.8f, 600);
        Sonido.Efecto("cuchillo");
        Sonido.Efecto("grito", 0.8f, 1.2f);
        Juego.I.ui?.TelonColor(new Color(0.6f, 0f, 0f));
        Juego.I.ui?.Telon(1f, 0.05f);
        yield return c.Esperar(1.2f);
        Juego.I.ui?.TelonColor(Color.black);
        yield return c.Esperar(0.8f);

        Sonido.Musica("m_oscuro", 1f);
        Dormitorio(c, "#9d8189", "#5a189a", false);
        Obj(Arte.Mochila(), 1.4f, 0.9f, 20);
        SpriteRenderer v = Pj(Arte.Valeria(), -2.6f, false);
        Reloj("JUE 7:10");
        c.Camara(B + new Vector2(0f, 1.6f), 2.8f);
        c.Cartel("JUEVES", "", 2f);
        yield return c.Fundido(0f, 1f);
        yield return c.StartCoroutine(c.CaminarLado(v, Arte.Valeria(), 0.4f, 1.2f));
        Sonido.Efecto("puerta", 0.4f, 1.8f);
        yield return c.Esperar(0.6f);
        SpriteRenderer m = Obj(Arte.Mascara(), 1.4f, 2f, 21);
        Sonido.Efecto("susto", 0.6f, 0.6f);
        yield return c.Mover(B + new Vector2(1f, 1.9f), 1.6f, 1f);
        yield return c.Esperar(0.6f);
        v.sprite = Arte.Valeria().Frente;
        v.color = Util.Hex("#ffb3b3");
        c.Cartel("LA NUEVA PEPINO", "", 2.5f);
        yield return c.Decir(null, "(Valeria sonríe.)", 2.5f);
        m.enabled = true;
    }

    // =================================================================
    //  FINAL MALO: bucle eterno
    // =================================================================
    static IEnumerator Malo(Cine c)
    {
        Sonido.Musica("m_fiesta", 0.5f);
        Sonido.BajarMusica(0.35f);
        Auditorio(c, false);
        SpriteRenderer val = Pj(Arte.Valeria(), -2f, false);
        SpriteRenderer an = Pj(Arte.Andrea(), 2f, true);
        c.Camara(B + new Vector2(0f, 1.6f), 3f);
        yield return c.Fundido(0f, 0.8f);
        val.sprite = Arte.Valeria().ataque;
        yield return c.Decir("Valeria", "¡Es Andrea! ¡Tiene el disfraz en la mochila!", 2.8f);
        Sonido.Musica(null, 0.3f);
        Sonido.BajarMusica(1f);
        an.sprite = Arte.Andrea().agachada;
        yield return c.Decir(null, "(Nadie te cree. Andrea llora. Alguien se ríe.)", 2.8f);
        val.sprite = Arte.Valeria().lado[0];
        SpriteRenderer pep = Pj(Arte.Pepino(), -7f, false);
        yield return c.StartCoroutine(c.CaminarLado(pep, Arte.Pepino(), -2.9f, 1.4f));
        Sonido.Latido(0.9f);
        yield return c.Mover(B + new Vector2(-2.4f, 1.6f), 2f, 0.8f);
        yield return c.Decir("Pepino", "Feliz primer día.", 2.4f);
        Sonido.Latido(0f);
        for (int i = 0; i < 3; i++)
        {
            Juego.I.ui?.Telon(1f, 0f);
            Sonido.Efecto("alarma", 0.4f, 1.6f - i * 0.15f);
            c.Cartel("6:45", "", 0.6f);
            yield return c.Esperar(0.7f);
            Juego.I.ui?.Telon(0f, 0f);
            yield return c.Esperar(0.12f);
        }
    }

    // =================================================================
    //  FINAL PESADILLA: el último bucle
    // =================================================================
    static IEnumerator Pesadilla(Cine c)
    {
        Sonido.Musica("m_oscuro", 1f);
        Nuevo();
        Fondo(c, Util.Hex("#7d8f69"), Arte.Suelo(TipoSuelo.Piedra, Util.Hex("#9a9a8f")), false);
        Obj(Arte.VentanaIllimani(true), -3f, 3.4f, -2970);
        Obj(Arte.VentanaIllimani(true), 3f, 3.4f, -2970);
        Obj(Arte.Ramo(), 0f, 0f, 5);
        Util.Rect("Foto", set, B + new Vector2(0.9f, 0.45f), new Vector2(0.6f, 0.8f), Util.Hex("#f1faee"), -5);
        Texto("Valeria", 0.9f, 1f, 0.35f, Util.Hex("#333333"));
        SpriteRenderer cam = Pj(Arte.Camila(), 7f, true);
        c.Camara(B + new Vector2(0f, 1.6f), 3.2f);
        c.Cartel("EL PATIO", "Jueves... para todos menos para ti.", 3f);
        yield return c.Fundido(0f, 1.5f);
        yield return c.StartCoroutine(c.CaminarLado(cam, Arte.Camila(), 2.2f, 1.1f));
        Obj(Arte.Mascara(), 1.7f, 0f, 9);
        Sonido.Efecto("golpe", 0.3f, 1.6f);
        yield return c.Mover(B + new Vector2(1.2f, 1.2f), 2.2f, 1.5f);
        yield return c.Decir("Camila", "Ya no recuerdo cuántas veces fueron.", 3f);
        yield return c.Decir("Camila", "Solo que la última... no volvió.", 3f);
        cam.sprite = Arte.Camila().agachada;
        yield return c.Esperar(1.5f);
    }

    // =================================================================
    //  DECORADOS
    // =================================================================
    public static void Nuevo()
    {
        BorrarSet();
        planos++;
        if (ModoGaleria)
        {
            B = new Vector2(planosFila * 34f, FilaGaleria);
            planosFila++;
            GameObject g = new GameObject("Plano | " + EtiquetaGaleria + " | " + planosFila);
            g.transform.SetParent(Juego.I.Raiz, false);
            set = g.transform;
            Horno.Suspender();
            Util.Texto(EtiquetaGaleria + "  ·  plano " + planosFila, set, B + new Vector2(0f, 8f), 1.4f, Util.Hex("#ffd166"), 50);
            Horno.Reanudar();
            return;
        }
        B = new Vector2(30000f + planos * 200f, 0f);
        GameObject go = new GameObject("Plano de cine " + planos);
        go.transform.SetParent(Juego.I.Raiz, false);
        set = go.transform;
    }

    public static void BorrarSet()
    {
        if (ModoGaleria) { set = null; return; }          // en la galería se quedan todos
        if (set != null) Object.Destroy(set.gameObject);
        set = null;
    }

    public static void Fondo(Cine c, Color pared, Sprite piso, bool techo)
    {
        Horno.Suspender();
        Util.Mosaico("Pared", set, Arte.ParedLateral(pared), B, new Vector2(30f, 6f), -3000);
        Util.Mosaico("Piso", set, piso, B + new Vector2(0f, -1.5f), new Vector2(30f, 3f), -2990);
        if (techo) Util.Rect("Techo", set, B + new Vector2(0f, 6.6f), new Vector2(30f, 1.2f), Util.Hex("#1b1822"), -2980);
        Horno.Reanudar();
    }

    public static void Auditorio(Cine c, bool conPublico)
    {
        Nuevo();
        Horno.Suspender();
        Util.Mosaico("Telón", set, Arte.Cortina(), B + new Vector2(0f, 3f), new Vector2(30f, 6f), -3000);
        Util.Mosaico("Escenario", set, Arte.Suelo(TipoSuelo.Madera, Util.Hex("#6b4a2f")), B + new Vector2(0f, -1.5f), new Vector2(30f, 3f), -2990);
        Util.Rect("Borde", set, B + new Vector2(0f, -0.05f), new Vector2(30f, 0.12f), Util.Hex("#ffd166"), -2985);
        string[] focos = { "#ff006e", "#3a86ff", "#ffbe0b", "#8338ec" };
        for (int i = 0; i < 4; i++)
            Util.Plano("Foco", set, Arte.FocoEscenario(focos[i]), B + new Vector2(-7.5f + i * 5f, 6.1f), -2940).transform.localScale = new Vector3(1.4f, 1.2f, 1f);
        if (conPublico)
            for (float x = -10f; x < 10f; x += 0.8f)
                Util.Plano("Público", set, Arte.Silueta(((int)((x + 10f) * 3f)) % 8), B + new Vector2(x, -2.3f), 800).color = new Color(0.05f, 0.05f, 0.08f, 1f);
        Horno.Reanudar();
    }

    public static void DetrasDelTelon(Cine c)
    {
        Nuevo();
        Fondo(c, Util.Hex("#2b2233"), Arte.Suelo(TipoSuelo.Madera, Util.Hex("#4a3220")), true);
        Horno.Suspender();
        Util.Mosaico("Telón", set, Arte.Cortina(), B + new Vector2(-7f, 3f), new Vector2(4f, 6f), -2970);
        Util.Mosaico("Telón", set, Arte.Cortina(), B + new Vector2(7f, 3f), new Vector2(4f, 6f), -2970);
        Util.Plano("Luz", set, Arte.ConoLuz(), B + new Vector2(0f, 5.7f), -2950).transform.localScale = new Vector3(1.3f, 1.4f, 1f);
        Util.Plano("Lámpara", set, Arte.LamparaTecho(), B + new Vector2(0f, 6f), -2900);
        Horno.Reanudar();
    }

    public static void Dormitorio(Cine c, string pared, string colcha, bool espejo)
    {
        Nuevo();
        Fondo(c, Util.Hex(pared), Arte.Suelo(TipoSuelo.Madera, Util.Hex("#8d6748")), true);
        Obj(Arte.VentanaIllimani(false), espejo ? 3f : -3f, 3.4f, -2970);
        SpriteRenderer cama = Obj(Arte.CamaLado(colcha), 0f, 0f, 2);
        cama.flipX = espejo;
        Obj(Arte.Afiche(espejo ? 3 : 1), espejo ? -3.2f : 3.2f, 3f, -2965);
    }

    public static void Calle(Cine c, bool noche)
    {
        Nuevo();
        Fondo(c, Util.Hex(noche ? "#3d405b" : "#d8cfbf"), Arte.Suelo(TipoSuelo.Piedra, Util.Hex(noche ? "#6c6a64" : "#a8a196")), false);
        Obj(Arte.VentanaIllimani(noche), -4f, 3.4f, -2970);
        if (!noche) Obj(Arte.Planta(), -1.5f, 0f, -2890);
    }

    public static SpriteRenderer Obj(Sprite s, float x, float y, int orden)
    {
        Horno.Suspender();
        SpriteRenderer sr = Util.Plano(s != null ? s.name : "Objeto", set, s, B + new Vector2(x, y), orden);
        Horno.Reanudar();
        return sr;
    }

    // Personaje de lado, parado en el piso
    public static SpriteRenderer Pj(SpritesPersonaje s, float x, bool mirarIzquierda)
    {
        SpriteRenderer sr = Obj(s.lado[0], x, 0f, 500);
        sr.flipX = mirarIzquierda;
        return sr;
    }

    public static TextMesh Texto(string t, float x, float y, float tam, Color col)
    {
        Horno.Suspender();
        TextMesh tm = Util.Texto(t, set, B + new Vector2(x, y), tam, col, 0);
        Horno.Reanudar();
        return tm;
    }

    public static void Pantalla(Vector2 pos, string texto)
    {
        Horno.Suspender();
        Util.Rect("Pantalla marco", set, B + pos, new Vector2(7.4f, 3.4f), Util.Hex("#111111"), -2930);
        Util.Rect("Pantalla", set, B + pos, new Vector2(7f, 3f), Util.Hex("#e9f5ff"), -2925);
        Util.Texto(texto, set, B + pos, 0.55f, Util.Hex("#a4161a"), 0);
        Horno.Reanudar();
    }

    // Personaje acostado en la cama (rotado)
    public static SpriteRenderer Acostada(SpritesPersonaje s)
    {
        SpriteRenderer sr = Obj(s.Frente, -0.9f, 0.85f, 3);
        sr.transform.localRotation = Quaternion.Euler(0, 0, 90f);
        return sr;
    }

    public static void Sentarse(SpriteRenderer sr, SpritesPersonaje s)
    {
        sr.transform.localRotation = Quaternion.identity;
        sr.transform.position = B + new Vector2(-0.8f, 0.55f);
        sr.sprite = s.Frente;
    }

    public static TextMesh Reloj(string texto)
    {
        Horno.Suspender();
        Util.Rect("Mesita", set, B + new Vector2(-2.3f, 0.45f), new Vector2(1f, 0.9f), Util.Hex("#6d4c35"), 1);
        Util.Rect("Reloj", set, B + new Vector2(-2.3f, 1.15f), new Vector2(0.9f, 0.45f), Util.Hex("#111111"), 2);
        TextMesh tm = Util.Texto(texto, set, B + new Vector2(-2.3f, 1.15f), 0.3f, Util.Hex("#ff4d4d"), 3);
        Horno.Reanudar();
        return tm;
    }
}
