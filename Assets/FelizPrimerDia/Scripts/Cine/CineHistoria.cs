using System.Collections;
using UnityEngine;

// =====================================================================
//  CINEMÁTICAS DE LA HISTORIA (dentro del mismo campus dibujado)
//    Intro           Valeria llega a la U por la Av. Héroes del Acre
//    PrimerPepino    la primera vez que ves al Pepino en un pasillo
//    Natita          la ofrenda a la ñatita de Odontología
//    Desenmascarar   en la Bienvenida, frente al Pepino verdadero
// =====================================================================
public static class CineHistoria
{
    // ---------------------------------------------------------------
    //  INTRO: llegada al campus (Planta Baja, primer día)
    // ---------------------------------------------------------------
    public static IEnumerator Intro(Juego j)
    {
        Cine c = Cine.Empezar(true);
        // La entrada de la Unifranz de día (se arma lejos del campus)
        Vector2 o = FachadaUnifranz.OrigenDia;
        Vector2 puerta = FachadaUnifranz.Construir(c, o, false);
        float vereda = -0.9f;

        Sonido.Musica("m_campus", 3f);
        c.Camara(o + new Vector2(0f, 9f), 4f);                       // empieza mirando el cielo y el Illimani
        yield return c.Fundido(0f, 1.5f);
        c.Cartel("UNIFRANZ  ·  SEDE LA PAZ", "Av. Héroes del Acre esq. Landaeta  —  Lunes, 6:58 a.m.", 4f);
        yield return c.Mover(o + new Vector2(0f, 4.6f), 6.8f, 5f);   // baja y muestra el edificio completo

        SpriteRenderer val = c.Actor("Valeria", Arte.Valeria(), o + new Vector2(11f, vereda));
        yield return c.Mover(o + new Vector2(4.5f, 1.6f), 3.6f, 1.5f);
        Coroutine caminar = c.StartCoroutine(c.Caminar(val.transform, new Vector2(puerta.x + 1.8f, o.y + vereda), 2.2f));
        yield return c.Decir("Valeria", "Primer día en la U... y todavía no sé qué quiero estudiar.", 3f);
        yield return caminar;

        Sonido.Efecto("blip", 0.6f, 1.5f);
        yield return c.Decir("Mensaje de Seba", "Suerte en tu primer día, hermanita. Y cuidado con la Bienvenida, jaja.", 3.2f);
        yield return c.Decir("Valeria", "Mi hermano y sus bromas...", 2f);

        // Mientras tanto, alguien la mira desde una ventana del tercer piso
        Vector2 ventana = o + new Vector2(-4.59f, 4.98f);
        SpriteRenderer mascara = c.Objeto("Máscara en la ventana", Arte.Mascara(), ventana, -3020);
        mascara.transform.localScale = new Vector3(0.55f, 0.55f, 1f);
        Sonido.Musica(null, 0.4f);
        c.Camara(ventana + new Vector2(0f, 0.3f), 1.6f);
        Sonido.Efecto("susto", 0.5f, 0.7f);
        yield return c.Esperar(1.4f);
        yield return c.Decir(null, "(Alguien con una máscara de Pepino la observa desde una ventana.)", 2.4f);
        mascara.enabled = false;
        yield return c.Esperar(0.6f);
        yield return c.Decir(null, "(Ya no está.)", 1.4f);

        // De vuelta: Valeria entra al edificio
        Sonido.Musica("m_campus", 1.5f);
        c.Camara(val.transform.position + new Vector3(0f, 1f, 0f), 3.4f);
        yield return c.StartCoroutine(c.Caminar(val.transform, new Vector2(puerta.x, o.y + vereda), 2.2f));
        yield return c.StartCoroutine(c.Caminar(val.transform, puerta + new Vector2(0f, 0.25f), 1.6f));
        Sonido.Efecto("puerta", 0.5f, 1.3f);
        yield return c.Fundido(1f, 0.8f);
        c.Cartel("PRIMER DÍA", "Semana de Inducción  —  7:00 a.m.", 2.5f);
        yield return c.Esperar(2.2f);
        yield return c.Terminar();
    }

    // ---------------------------------------------------------------
    //  LA PRIMERA VEZ QUE APARECE EL PEPINO
    // ---------------------------------------------------------------
    public static IEnumerator PrimerPepino(Juego j, Transform pepino)
    {
        Memoria.Aprender("cine_pepino");
        Cine c = Cine.Empezar(false);
        Sonido.Musica(null, 0.3f);
        Sonido.Efecto("apagon", 0.6f, 1f);
        yield return c.Parpadeo(4, 0.2f);
        Vector2 pos = pepino.position;
        yield return c.Mover(pos + new Vector2(0f, 0.5f), 2.4f, 1.6f);
        Sonido.Efecto("susto");
        if (Protagonista.EsMateo) c.Cartel("EL QUINTO DISFRAZ", "Alguien más busca las grabaciones. Y te busca a ti.", 3.5f);
        else c.Cartel("EL PEPINO", "Desde ahora patrulla los pasillos. Si te ve, corre... y escóndete.", 3.5f);
        yield return c.Esperar(1f);
        SpriteRenderer sr = pepino.GetComponent<SpriteRenderer>();
        for (int i = 0; i < 3 && !c.Saltada; i++)
        {
            if (sr != null) sr.flipX = !sr.flipX;
            yield return c.Esperar(0.35f);
        }
        Sonido.Latido(0.6f);
        yield return c.Decir(Protagonista.Nombre, Protagonista.EsMateo ? "...Ese disfraz es dorado. No es el mismo Pepino de las cámaras." : "...¿Hola? ¿Es parte de la Inducción?", 2.4f);
        if (j.jugador != null) yield return c.Mover(j.jugador.Posicion, j.zoomCamara, 1f);
        j.ui.OscuridadInstantanea(0f);
        yield return c.Terminar();
    }

    // ---------------------------------------------------------------
    //  LA ÑATITA (ofrenda completa)
    // ---------------------------------------------------------------
    public static IEnumerator Natita(Juego j, Vector2 posNatita, bool sabeSecreto)
    {
        Cine c = Cine.Empezar(false);
        Sonido.Musica(null, 1f);
        yield return c.Mover(posNatita + new Vector2(0f, 0.6f), 2.2f, 1.5f);
        yield return c.Decir("Valeria", "Flores. Hojas de coca. Y la vela.", 2.2f);

        // Velas encendidas a los costados
        SpriteRenderer v1 = c.Objeto("Vela", Arte.Vela(), posNatita + new Vector2(-0.9f, -0.2f), OrdenPorY.Orden(posNatita.y - 0.2f));
        SpriteRenderer v2 = c.Objeto("Vela", Arte.Vela(), posNatita + new Vector2(0.9f, -0.2f), OrdenPorY.Orden(posNatita.y - 0.2f));
        SpriteRenderer luz = c.Objeto("Brillo de velas", Arte.Luz(), posNatita + new Vector2(0f, 0.6f), 30000);
        luz.color = new Color(1f, 0.65f, 0.25f, 0.5f);
        luz.transform.localScale = new Vector3(3f, 3f, 1f);
        Sonido.Efecto("apagon", 0.6f, 0.8f);
        j.ui.Oscuridad(0.8f);
        yield return c.Esperar(1.2f);

        Sonido.Musica("m_oscuro", 2f);
        Sonido.Efecto("trueno", 0.6f, 1f);
        string[] lineas =
        {
            "Hija... el favor se pidió con rabia. Y la rabia no se cansa: repite y repite.",
            "Ella pidió que sintieras su miedo. Ya lo sentiste. Pero ella también está presa: recuerda cada vez.",
            "Para cerrar el día tienen que pedirlo LAS DOS. La que se quemó... y la que se rió."
        };
        foreach (string l in lineas)
        {
            if (c.Saltada) break;
            luz.transform.localScale = new Vector3(Random.Range(2.6f, 3.4f), Random.Range(2.6f, 3.4f), 1f);
            j.ui.Destello(new Color(1f, 0.6f, 0.2f), 0.15f);
            yield return c.Decir("La ñatita", l, 4f);
        }
        if (!sabeSecreto)
            yield return c.Decir("Valeria", "¿\"La que se rió\"? ...¿Yo?", 2.5f);
        v1.color = v2.color = new Color(1, 1, 1, 0.7f);
        j.ui.Oscuridad(0f);
        Sonido.Musica(null, 2f);
        yield return c.Terminar();
    }

    // ---------------------------------------------------------------
    //  LA BIENVENIDA: frente al Pepino verdadero
    // ---------------------------------------------------------------
    public static IEnumerator Desenmascarar(Juego j, SpriteRenderer pepino, bool conPruebas)
    {
        Cine c = Cine.Empezar(true);
        Sonido.BajarMusica(0.15f);
        Bailarin b = pepino.GetComponent<Bailarin>();
        if (b != null) { b.enabled = false; pepino.transform.localScale = Vector3.one; }
        pepino.flipX = true;
        Vector2 pp = pepino.transform.position;

        SpriteRenderer val = c.Actor("Valeria", Arte.Valeria(), pp + new Vector2(-3.2f, -0.2f));
        c.Camara(pp + new Vector2(-1.6f, 0.4f), 3.4f);
        yield return c.Fundido(0f, 0.6f);
        yield return c.StartCoroutine(c.Caminar(val.transform, pp + new Vector2(-1.2f, 0f), 2f));
        yield return c.Mover(pp + new Vector2(-0.6f, 0.5f), 2.3f, 0.8f);

        if (conPruebas)
        {
            yield return c.Decir("Valeria", "Se acabó. Tengo las cuatro pruebas. Quítate la máscara.", 2.8f);
            Sonido.Efecto("golpe", 0.7f, 1.2f);
            c.Temblor(0.3f);
            yield return c.Esperar(0.6f);
            pepino.sprite = Arte.Camila().lado[0];
            pepino.flipX = true;
            Sonido.Efecto("susto", 0.7f, 0.75f);
            j.ui.Destello(Color.white, 0.3f);
            c.Cartel("CAMILA ROJAS", "Delegada de la Bienvenida  ·  2do año de Periodismo", 3.5f);
            yield return c.Esperar(1.2f);
            yield return c.Decir("Camila", "...Feliz primer día, Valeria.", 2.6f);
        }
        else
        {
            yield return c.Decir("Valeria", "Te encontré...", 1.8f);
            Sonido.Efecto("cuchillo", 0.8f, 0.9f);
            j.ui.Destello(new Color(0.8f, 0f, 0f), 0.35f);
            yield return c.StartCoroutine(c.Caminar(pepino.transform, pp + new Vector2(-0.5f, 0f), 1.5f));
            yield return c.Decir(null, "(Saca un cuchillo y te acorrala contra el telón.)", 2.4f);
        }
        Object.Destroy(val.gameObject);
        yield return c.Terminar();
    }
}
