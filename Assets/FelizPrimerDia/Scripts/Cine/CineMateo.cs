using System.Collections;
using UnityEngine;

// =====================================================================
//  CINEMÁTICAS DE MATEO
//    Prologo      la noche que vio morir a Valeria por las cámaras
//    Desbloqueo   el aviso de nuevo personaje (al terminar con Valeria)
//    Finales      En vivo · Testigo · Solo un espectador · Borrado
// =====================================================================
public static class CineMateo
{
    static Vector2 O { get { return CineFinales.Origen; } }

    static SpriteRenderer R(float x, float y, float w, float h, string hex, int orden)
    {
        Horno.Suspender();
        SpriteRenderer r = Util.Rect("Rect", CineFinales.Set, O + new Vector2(x, y), new Vector2(w, h), Util.Hex(hex), orden);
        Horno.Reanudar();
        return r;
    }

    // El cuarto de monitoreo visto de lado: escritorio con 3 monitores verdes
    static void CuartoMonitoreo(Cine c)
    {
        CineFinales.Nuevo();
        CineFinales.Fondo(c, Util.Hex("#26222e"), Arte.Suelo(TipoSuelo.Tecnico, Util.Hex("#2e2823")), true);
        R(0.6f, 0.45f, 3.6f, 0.14f, "#4a3a2c", 300);
        R(-0.9f, 0.2f, 0.12f, 0.5f, "#3a2d22", 299); R(2.1f, 0.2f, 0.12f, 0.5f, "#3a2d22", 299);
        for (int i = 0; i < 3; i++)
        {
            R(-0.4f + i * 1f, 1.05f, 0.85f, 0.7f, "#8d8d86", 301);
            R(-0.4f + i * 1f, 1.05f, 0.7f, 0.55f, i == 1 ? "#2a8a5c" : "#1d4d37", 302);
        }
        CineFinales.Obj(Arte.Afiche(0), -3.5f, 3f, -2965);
    }

    // ---------------------------------------------------------------
    public static IEnumerator Prologo(Juego j)
    {
        Cine c = Cine.Empezar(true);
        Sonido.Musica("m_noche", 2f);
        CuartoMonitoreo(c);
        SpriteRenderer mat = CineFinales.Pj(Arte.Mateo(), -1.6f, false);
        c.Camara(O + new Vector2(0f, 1.6f), 2.8f);
        c.Cartel("MATEO CHOQUE", "Ing. de Sistemas  ·  Lunes, 18:40  ·  Cuarto de monitoreo", 3.5f);
        yield return c.Fundido(0f, 1.2f);
        yield return c.Decir("Mateo", "Respaldos de las cámaras... listo. Ahora a ver si las cámaras viejas todavía prenden.", 3f);
        yield return c.Decir("Mateo", "Prefiero esto a la fiesta. Las fiestas no son lo mío.", 2.6f);
        Sonido.Efecto("estatica", 0.6f, 1f);

        // Lo que ve en el monitor
        yield return c.Fundido(1f, 0.3f);
        j.ui.Recuerdo(true, "CAM 3  ·  PISO 3  ·  18:47");
        CineFinales.Nuevo();
        CineFinales.Fondo(c, Util.Hex("#2b3a3a"), Arte.Suelo(TipoSuelo.Clinico, Util.Hex("#9fb0b0")), true);
        SpriteRenderer val = CineFinales.Pj(Arte.Valeria(), 3f, true);
        SpriteRenderer pep = CineFinales.Pj(Arte.Pepino(), 8f, true);
        c.Camara(O + new Vector2(0f, 1.6f), 3f);
        yield return c.Fundido(0f, 0.3f);
        Sonido.Latido(0.8f);
        Coroutine a = c.StartCoroutine(c.CaminarLado(val, Arte.Valeria(), -3.5f, 3.4f));
        yield return c.StartCoroutine(c.CaminarLado(pep, Arte.Pepino(), -2.5f, 3.9f));
        yield return a;
        Sonido.Efecto("cuchillo", 0.7f, 0.9f);
        j.ui.Destello(new Color(0.7f, 0f, 0f), 0.5f);
        val.sprite = Arte.Valeria().agachada;
        Sonido.Latido(0f);
        yield return c.Esperar(1f);
        j.ui.Recuerdo(false, null);

        // De vuelta al cuarto
        yield return c.Fundido(1f, 0.3f);
        CuartoMonitoreo(c);
        mat = CineFinales.Pj(Arte.Mateo(), -1.6f, false);
        mat.sprite = Arte.Mateo().Frente;
        c.Camara(O + new Vector2(-1.2f, 1.2f), 2f);
        yield return c.Fundido(0f, 0.4f);
        yield return c.Decir("Mateo", "...", 1.4f);
        yield return c.Decir("Mateo", "Podría haber activado la alarma. Podría haber cerrado las puertas.", 3f);
        yield return c.Decir("Mateo", "Y solo me quedé mirando.", 2.4f);
        Sonido.Efecto("trueno", 0.6f, 1f);
        yield return c.Fundido(1f, 0.8f);
        Sonido.Efecto("alarma", 0.35f, 1.6f);
        c.Cartel("6:45", "LUNES.  Otra vez.", 3f);
        yield return c.Esperar(2.5f);
        CineFinales.BorrarSet();
        yield return c.Terminar();
        j.ui.Toast("Estás en el bucle. Recupera los 4 videos en las computadoras y consigue aliados. A las 18:00: la cabina del auditorio.", 7f);
    }

    // Aviso de nuevo personaje (después de un final de Valeria)
    public static IEnumerator Desbloqueo(Juego j)
    {
        Cine c = Cine.Empezar(true);
        Sonido.Musica("m_tension", 1f);
        j.ui.OcultarPantalla();
        CuartoMonitoreo(c);
        SpriteRenderer mat = CineFinales.Pj(Arte.Mateo(), -1.6f, false);
        c.Camara(O + new Vector2(-0.4f, 1.3f), 2.4f);
        yield return c.Fundido(0f, 1f);
        yield return c.Decir(null, "(Mientras tanto, en el viejo cuarto de monitoreo...)", 2.6f);
        mat.sprite = Arte.Mateo().Frente;
        Sonido.Efecto("logro", 0.8f, 0.9f);
        c.Cartel("NUEVO PERSONAJE: MATEO CHOQUE", "Alguien estuvo mirando todo este tiempo...", 4f);
        yield return c.Esperar(3.5f);
        yield return c.Fundido(1f, 0.8f);
        CineFinales.BorrarSet();
        yield return c.Terminar();
        j.ui.Telon(1f, 0f);
    }

    // =================================================================
    //  FINALES DE MATEO
    // =================================================================
    public static IEnumerator Final(string final)
    {
        Juego j = Juego.I;
        Cine c = Cine.Empezar(true);
        yield return c.Fundido(1f, 0.5f);
        switch (final)
        {
            case "mateo_envivo": yield return EnVivo(c, false); break;
            case "mateo_testigo": yield return EnVivo(c, true); break;
            case "mateo_espectador": yield return Espectador(c); break;
            case "mateo_borrado": yield return Borrado(c); break;
        }
        yield return c.Fundido(1f, 1f);
        CineFinales.BorrarSet();
        yield return c.Terminar();
        j.ui.Telon(1f, 0f);
    }

    static IEnumerator EnVivo(Cine c, bool testigo)
    {
        Sonido.Musica(null, 0.5f);
        CineFinales.Auditorio(c, true);
        CineFinales.Pantalla(new Vector2(0f, 3.4f), "EN VIVO  ·  RADIO UNIFRANZ\nVIDEO 4:  SEBASTIÁN QUISPE");
        SpriteRenderer seb = CineFinales.Pj(Arte.Sebastian(), 3.2f, true);
        CineFinales.Obj(Arte.Mascara(), 3.6f, 0f, 501);
        CineFinales.Pj(Arte.Valeria(), -3.2f, false);
        CineFinales.Pj(Arte.Camila(), -2f, false);
        c.Camara(O + new Vector2(0f, 2.1f), 3.6f);
        yield return c.Fundido(0f, 1f);
        Sonido.Efecto("click", 0.8f, 0.8f);
        yield return c.Decir("Voz de Joaquín (radio)", "Estamos en vivo desde la Bienvenida de Unifranz. Lo que van a ver pasó hace un año, aquí mismo.", 3.6f);
        Sonido.Efecto("estatica", 0.4f, 0.7f);
        yield return c.Decir(null, "(En la pantalla gigante, Sebastián se quita la máscara y se ríe. Todo el auditorio lo mira.)", 3.4f);
        SpriteRenderer teo = CineFinales.Pj(Arte.Teodoro(), 9f, true);
        yield return c.StartCoroutine(c.CaminarLado(teo, Arte.Teodoro(), 4.4f, 2.2f));
        yield return c.Decir("Don Teodoro", "Joven Quispe. Esta vez no voy a mirar para otro lado.", 2.8f);
        seb.sprite = Arte.Sebastian().agachada;
        yield return c.Esperar(0.8f);

        if (testigo)
        {
            SpriteRenderer mat = CineFinales.Pj(Arte.Mateo(), -8f, false);
            c.Camara(O + new Vector2(-3f, 1.4f), 2.6f);
            yield return c.StartCoroutine(c.CaminarLado(mat, Arte.Mateo(), -4.2f, 1.8f));
            Sonido.Musica("m_triste", 2f);
            yield return c.Decir("Mateo", "Camila... yo no estaba esa noche. Pero estuve mirando mil noches.", 3.4f);
            yield return c.Decir("Mateo", "Perdón por tardar tanto en hacer algo.", 2.8f);
            yield return c.Decir("Camila", "(Se quita los guantes. Por primera vez, no esconde las manos.)", 3f);
            yield return c.Fundido(1f, 1f);
            CineFinales.Calle(c, false);
            CineFinales.Obj(Arte.Puerta("#3d405b"), 3f, 0f, -2900);
            SpriteRenderer v = CineFinales.Pj(Arte.Valeria(), -7f, false);
            SpriteRenderer ca = CineFinales.Pj(Arte.Camila(), -8.2f, false);
            SpriteRenderer m2 = CineFinales.Pj(Arte.Mateo(), -9.4f, false);
            c.Camara(O + new Vector2(-1f, 1.8f), 3.4f);
            c.Cartel("JUEVES", "Por fin.", 2.5f);
            yield return c.Fundido(0f, 1f);
            Coroutine a = c.StartCoroutine(c.CaminarLado(v, Arte.Valeria(), 1.6f, 1.6f));
            Coroutine b = c.StartCoroutine(c.CaminarLado(ca, Arte.Camila(), 0.4f, 1.6f));
            yield return c.StartCoroutine(c.CaminarLado(m2, Arte.Mateo(), -0.8f, 1.6f));
            yield return a; yield return b;
            yield return c.Decir(null, "(Los tres salen juntos de la U. Nadie se queda mirando.)", 3f);
        }
        else
        {
            Sonido.Musica("m_triste", 2f);
            yield return c.Decir(null, "(Por fin alguien que miraba, actuó.)", 2.8f);
        }
    }

    static IEnumerator Espectador(Cine c)
    {
        Sonido.Musica("m_oscuro", 1f);
        CuartoMonitoreo(c);
        SpriteRenderer mat = CineFinales.Pj(Arte.Mateo(), -1.6f, false);
        c.Camara(O + new Vector2(0f, 1.3f), 2.4f);
        yield return c.Fundido(0f, 1f);
        yield return c.Decir(null, "(Mateo mira la Bienvenida por los monitores. No baja.)", 2.8f);
        Juego.I.ui?.Destello(new Color(0.6f, 0f, 0f), 0.4f);
        yield return c.Decir(null, "(En el monitor del medio, el Pepino levanta el cuchillo.)", 2.8f);
        for (int i = 0; i < 3; i++)
        {
            Juego.I.ui?.Telon(1f, 0f);
            Sonido.Efecto("alarma", 0.3f, 1.6f);
            c.Cartel(i == 0 ? "NOCHE 2" : i == 1 ? "NOCHE 47" : "NOCHE 1.000", "", 0.8f);
            yield return c.Esperar(0.9f);
            Juego.I.ui?.Telon(0f, 0f);
            yield return c.Esperar(0.2f);
        }
        mat.sprite = Arte.Personaje(Arte.Ap("#264653", "#a47148", "#111111", "#1d3557", false, Accesorio.Gorra, "#1d3557")).lado[0];
        c.Cartel("EL NUEVO GUARDIA NOCTURNO", "", 3f);
        yield return c.Esperar(2.6f);
    }

    static IEnumerator Borrado(Cine c)
    {
        Sonido.Musica(null, 0.5f);
        CuartoMonitoreo(c);
        CineFinales.Pantalla(new Vector2(0.6f, 3.2f), "> borrar video_1.mp4 ... video_4.mp4\nARCHIVOS ELIMINADOS: 4/4");
        CineFinales.Pj(Arte.Mateo(), -1.6f, false);
        SpriteRenderer seb = CineFinales.Pj(Arte.Sebastian(), 3.4f, true);
        c.Camara(O + new Vector2(0.6f, 1.8f), 2.8f);
        yield return c.Fundido(0f, 1f);
        R(2.6f, 0.95f, 0.4f, 0.2f, "#2d6a4f", 600);                       // la plata
        yield return c.Decir("Sebastián", "Bórralos y esto es tuyo. Nadie tiene por qué saber.", 3f);
        for (int i = 0; i < 4 && !c.Saltada; i++) { Sonido.Efecto("click", 0.6f, 0.9f); yield return c.Esperar(0.3f); }
        Sonido.Musica("m_oscuro", 1f);
        yield return c.StartCoroutine(c.CaminarLado(seb, Arte.Sebastian(), 9f, 1.6f));
        yield return c.Fundido(1f, 1f);
        CineFinales.Dormitorio(c, "#adb5bd", "#1b1b1f", false);
        SpriteRenderer m2 = CineFinales.Acostada(Arte.Mateo());
        CineFinales.Reloj("LUN 6:45");
        c.Camara(O + new Vector2(0f, 1.8f), 3f);
        yield return c.Fundido(0f, 1f);
        Sonido.Efecto("alarma", 0.35f, 1.6f);
        CineFinales.Sentarse(m2, Arte.Mateo());
        yield return c.Decir("Mateo", "...Lunes. Otra vez. Y ahora solo yo me acuerdo.", 3f);
    }
}
