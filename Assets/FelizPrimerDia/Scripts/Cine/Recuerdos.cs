using System.Collections;
using UnityEngine;

// =====================================================================
//  RECUERDOS: cuando Valeria encuentra una pista o una grabación, en vez
//  de leer un texto largo se reproduce un "video" corto de lo que pasó,
//  con efecto de cinta VHS (sepia, líneas, grano y la fecha de la cámara).
//  Esc salta el recuerdo. Lo escrito queda igual en el cuaderno (Tab).
// =====================================================================
public static class Recuerdos
{
    static readonly string[] Ids =
    {
        "audio_1", "audio_2", "audio_3", "audio_4", "audio_5", "audio_6", "audio_7", "audio_8",
        "anuario", "presupuesto", "afiche", "quemaduras", "motivo", "cicatriz", "usuario", "camara_vieja",
        "m_video_1", "m_video_2", "m_video_3", "m_video_4"
    };

    public static bool Existe(string id) { return System.Array.IndexOf(Ids, id) >= 0; }

    // Qué video corresponde a una pista del cuaderno (o null si no tiene)
    public static string VideoDe(string pista)
    {
        if (Existe(pista)) return pista;
        if (pista == "secreto") return "audio_8";
        if (pista.StartsWith("m_archivo_")) return "m_video_" + pista.Substring(10);
        return null;
    }

    // Personajes de los recuerdos
    static SpritesPersonaje Seba { get { return Arte.Personaje(Arte.Ap("#3a0ca3", "#c68a5a", "#1b1b1b", "#14213d", false, Accesorio.Ninguno, null)); } }
    static SpritesPersonaje Abuela { get { return Arte.Personaje(Arte.Ap("#6d597a", "#a47148", "#d9d9d9", "#3d405b", true, Accesorio.Bufanda, "#6a040f")); } }
    static SpritesPersonaje Funcionario { get { return Arte.Personaje(Arte.Ap("#2b2d42", "#c08f63", "#555555", "#2b2d42", false, Accesorio.Lentes, null)); } }
    static SpritesPersonaje Doctor { get { return Arte.Personaje(Arte.Ap("#f1faee", "#a47148", "#2b1d14", "#457b9d", false, Accesorio.Bata, null)); } }
    static SpritesPersonaje Nico { get { return Arte.Personaje(Arte.Ap("#ff70a6", "#c68a5a", "#2b1d14", "#14213d", false, Accesorio.Gorra, "#3a86ff")); } }
    static SpritesPersonaje Herida { get { return Arte.Personaje(Arte.Ap("#f1faee", "#e0ac69", "#6f1d1b", "#f1faee", true, Accesorio.Ninguno, null)); } }
    static SpritesPersonaje Estudiante(int i)
    {
        string[] ropa = { "#e63946", "#2a9d8f", "#f4a261", "#457b9d", "#8338ec" };
        return Arte.Personaje(Arte.Ap(ropa[i % ropa.Length], i % 2 == 0 ? "#c68a5a" : "#8d5524", "#111111", "#22223b", i % 3 == 0, Accesorio.Ninguno, null));
    }

    public static IEnumerator Reproducir(string id)
    {
        Juego j = Juego.I;
        if (j == null || !Existe(id)) yield break;
        Cine c = Cine.Empezar(true);
        Sonido.Musica(null, 0.4f);
        Sonido.Latido(0f);
        yield return c.Fundido(1f, 0.35f);
        Sonido.Efecto("estatica", 0.5f, 0.9f);
        Sonido.Ambiente("estatica", 0.06f);
        j.ui.Recuerdo(true, Fecha(id));
        switch (id)
        {
            case "audio_1": yield return PrimerDia(c); break;
            case "audio_2": yield return Sebastian(c); break;
            case "audio_3": yield return LaNoche(c); break;
            case "audio_4": yield return Retiro(c); break;
            case "audio_5": yield return Cementerio(c); break;
            case "audio_6": yield return Funciono(c); break;
            case "audio_7": yield return YoRecuerdo(c); break;
            case "audio_8": yield return SeRio(c); break;
            case "anuario": yield return Anuario(c); break;
            case "presupuesto": yield return Presupuesto(c); break;
            case "afiche": yield return Afiche(c); break;
            case "quemaduras": yield return Quemaduras(c); break;
            case "motivo": yield return Denuncia(c); break;
            case "cicatriz": yield return Cicatriz(c); break;
            case "usuario": yield return Registros(c); break;
            case "camara_vieja": yield return CintaRoja(c); break;
            case "m_video_1": yield return VideoFiesta(c); break;
            case "m_video_2": yield return VideoEncendedor(c); break;
            case "m_video_3": yield return VideoClinica(c); break;
            case "m_video_4": yield return VideoCara(c); break;
        }
        Sonido.Efecto("estatica", 0.6f, 1.2f);
        yield return c.Fundido(1f, 0.4f);
        j.ui.Recuerdo(false, null);
        Sonido.Ambiente(null, 0f);
        CineFinales.BorrarSet();
        yield return c.Terminar();
    }

    static string Fecha(string id)
    {
        switch (id)
        {
            case "audio_1": return "03/02/2025  07:02";
            case "audio_2": return "05/02/2025  12:40";
            case "audio_3": return "07/02/2025  23:14";
            case "audio_4": return "21/02/2025  10:05";
            case "audio_5": return "08/11/2025  19:30";
            case "audio_6": return "LUNES  06:45";
            case "audio_7": return "LUNES  18:40";
            case "audio_8": return "08/02/2025  00:03";
            case "quemaduras": return "08/02/2025  00:40";
            case "motivo": return "10/02/2025  09:15";
            case "camara_vieja": return "07/02/2025  22:58   CAM 3";
            case "cicatriz": return "HOY  11:47   CÁMARA DEL SET";
            case "usuario": return "HOY  06:12   SISTEMA U";
            case "m_video_1": return "07/02/2025  22:31   CELULAR 1";
            case "m_video_2": return "07/02/2025  23:13   CELULAR 2";
            case "m_video_3": return "08/02/2025  00:40   BASE DE DATOS CLÍNICA";
            case "m_video_4": return "07/02/2025  23:20   CELULAR 4";
        }
        return "ENERO 2026";
    }

    // ---------------------------------------------------------------
    //  Atajos
    // ---------------------------------------------------------------
    static Vector2 O { get { return CineFinales.Origen; } }
    static void Cam(Cine c, float x, float y, float zoom) { c.Camara(O + new Vector2(x, y), zoom); }
    static IEnumerator Ver(Cine c) { return c.Fundido(0f, 0.6f); }

    static void Sala(Cine c, string pared, Sprite piso, bool techo)
    {
        CineFinales.Nuevo();
        CineFinales.Fondo(c, Util.Hex(pared), piso, techo);
    }

    static SpriteRenderer Rect(float x, float y, float w, float h, string hex, int orden)
    {
        Horno.Suspender();
        SpriteRenderer r = Util.Rect("Rect", CineFinales.Set, O + new Vector2(x, y), new Vector2(w, h), Util.Hex(hex), orden);
        Horno.Reanudar();
        return r;
    }

    static void Mesa(float x, float ancho)
    {
        Rect(x, 0.45f, ancho, 0.12f, "#6b4a2f", 300);
        Rect(x - ancho / 2f + 0.1f, 0.2f, 0.1f, 0.45f, "#4a3220", 299);
        Rect(x + ancho / 2f - 0.1f, 0.2f, 0.1f, 0.45f, "#4a3220", 299);
    }

    // =================================================================
    //  LAS GRABACIONES DE CAMILA
    // =================================================================
    static IEnumerator PrimerDia(Cine c)
    {
        CineFinales.Nuevo();
        Vector2 o = O;
        FachadaUnifranz.Construir(c, o, false);
        SpriteRenderer cam = c.Actor("Camila", Arte.Camila(), o + new Vector2(9f, -0.9f));
        c.Camara(o + new Vector2(5f, 1.4f), 3.2f);
        yield return Ver(c);
        Coroutine a = c.StartCoroutine(c.Caminar(cam.transform, o + new Vector2(3.2f, -0.9f), 1.8f));
        yield return c.Decir("Camila", "Hola, yo del futuro. Hoy es mi primer día en Unifranz.", 3f);
        yield return a;
        yield return c.Decir("Camila", "Quiero estudiar Periodismo. Estoy nerviosa... pero feliz.", 3f);
        yield return c.Decir("Camila", "No te olvides de esta sensación, ¿ya?", 2.4f);
    }

    static IEnumerator Sebastian(Cine c)
    {
        Sala(c, "#7d8f69", Arte.Suelo(TipoSuelo.Pasto, Util.Hex("#40916c")), false);
        CineFinales.Obj(Arte.VentanaIllimani(false), -3f, 3.4f, -2970);
        CineFinales.Obj(Arte.Arbol(), 4f, 0f, -2890);
        SpriteRenderer cam = CineFinales.Pj(Arte.Camila(), -1.2f, false);
        SpriteRenderer seb = CineFinales.Pj(Seba, 6f, true);
        Cam(c, 0.5f, 1.5f, 2.8f);
        yield return Ver(c);
        yield return c.StartCoroutine(c.CaminarLado(seb, Seba, 0.4f, 1.8f));
        yield return c.Decir("Sebastián", "¿Cachimba? Soy Sebastián, de último año.", 2.6f);
        yield return c.Decir("Sebastián", "Ven a la Bienvenida. A los nuevos les tenemos una... sorpresa.", 3f);
        cam.sprite = Arte.Camila().Frente;
        yield return c.Decir("Camila", "(Sonríe.) ¡Ya! Ahí estaré.", 2.4f);
    }

    static IEnumerator LaNoche(Cine c)
    {
        CineFinales.Auditorio(c, false);
        SpriteRenderer cam = CineFinales.Pj(Arte.Camila(), 0f, false);
        for (int i = 0; i < 4; i++)
        {
            float x = i < 2 ? -3.2f + i * 1.1f : 2.1f + (i - 2) * 1.1f;
            CineFinales.Pj(Estudiante(i), x, i >= 2);
            CineFinales.Obj(Arte.Celular(), x + (i >= 2 ? -0.35f : 0.35f), 0.9f, 600);
        }
        SpriteRenderer seb = CineFinales.Pj(Seba, 1.1f, true);
        Cam(c, 0f, 1.4f, 2.8f);
        yield return Ver(c);
        yield return c.Decir("Voz de Camila", "Me echaron alcohol en el pelo. \"Es el bautizo\", decían.", 3f);
        seb.sprite = Seba.ataque;
        yield return c.Decir("Voz de Camila", "Alguien sacó un encendedor.", 2f);
        Sonido.Efecto("grito", 0.9f, 1.15f);
        Juego.I.ui?.Destello(new Color(1f, 0.45f, 0.1f), 0.6f);
        c.Temblor(0.5f);
        cam.sprite = Arte.Camila().agachada;
        cam.color = Util.Hex("#ffb38a");
        yield return c.Esperar(1f);
        yield return c.Decir("Voz de Camila", "Todos tenían el celular en la mano. Nadie me ayudó.", 3.2f);
    }

    static IEnumerator Retiro(Cine c)
    {
        Sala(c, "#d8cfbf", Arte.Suelo(TipoSuelo.Ceramica, Util.Hex("#cfc4ae")), true);
        CineFinales.Obj(Arte.Afiche(2), 2.5f, 3f, -2965);
        Mesa(0.3f, 2.2f);
        CineFinales.Pj(Funcionario, 1f, true);
        SpriteRenderer cam = CineFinales.Pj(Arte.Camila(), -1.3f, false);
        Rect(0.3f, 0.55f, 0.5f, 0.08f, "#f1faee", 301);
        Cam(c, 0f, 1.3f, 2.4f);
        yield return Ver(c);
        yield return c.Decir("Funcionario", "No hay pruebas, señorita. Las cámaras de esa noche no grababan.", 3.2f);
        yield return c.Decir("Funcionario", "Es su palabra contra la de él. Y él es... muy querido aquí.", 3f);
        cam.sprite = Arte.Camila().agachada;
        yield return c.Decir("Voz de Camila", "Hoy retiré la denuncia.", 2.4f);
    }

    static IEnumerator Cementerio(Cine c)
    {
        Sala(c, "#4a4a52", Arte.Suelo(TipoSuelo.Piedra, Util.Hex("#6c6a64")), false);
        // Nichos con ñatitas
        for (int fila = 0; fila < 2; fila++)
            for (int k = 0; k < 6; k++)
            {
                float x = -5f + k * 2f, y = 1.4f + fila * 1.9f;
                Rect(x, y + 0.6f, 1.5f, 1.5f, "#1b1822", -2950);
                if ((k + fila) % 2 == 0) CineFinales.Obj(Arte.Natita(), x, y, -2940).transform.localScale = new Vector3(0.6f, 0.6f, 1f);
            }
        for (int k = 0; k < 5; k++) CineFinales.Obj(Arte.Vela(), -4f + k * 2f, 0f, 400);
        CineFinales.Obj(Arte.Ramo(), 0.6f, 0f, 401);
        CineFinales.Pj(Abuela, -1.4f, false);
        SpriteRenderer cam = CineFinales.Pj(Arte.Camila(), -0.3f, false);
        Cam(c, 0f, 1.8f, 3f);
        c.Cartel("8 DE NOVIEMBRE", "Fiesta de las Ñatitas  ·  Cementerio General", 3f);
        yield return Ver(c);
        yield return c.Decir("Abuela", "Las ñatitas cumplen favores, hijita. Flores, coca y velas...", 3.2f);
        yield return c.Decir("Abuela", "Pero nunca les pidas nada con rabia.", 2.6f);
        yield return c.Mover(O + new Vector2(-0.2f, 1.6f), 2f, 1.2f);
        Sonido.Efecto("trueno", 0.5f, 1.1f);
        yield return c.Decir("Camila", "(Susurra.) Quiero que alguien de esa familia sienta mi miedo. Un día. Solo uno.", 3.6f);
        cam.color = Util.Hex("#ffb3b3");
    }

    static IEnumerator Funciono(Cine c)
    {
        CineFinales.Dormitorio(c, "#ffd6a5", "#e63946", true);
        SpriteRenderer cam = CineFinales.Acostada(Arte.Camila());
        TextMesh reloj = CineFinales.Reloj("LUN 6:45");
        Cam(c, 0f, 1.8f, 3f);
        yield return Ver(c);
        Sonido.Efecto("alarma", 0.35f, 1.6f);
        CineFinales.Sentarse(cam, Arte.Camila());
        yield return c.Decir("Camila", "¿Lunes otra vez? Ayer era lunes...", 2.6f);
        for (int i = 0; i < 2; i++)
        {
            Juego.I.ui?.Telon(1f, 0f);
            yield return c.Esperar(0.25f);
            Juego.I.ui?.Telon(0f, 0f);
            Sonido.Efecto("alarma", 0.35f, 1.6f);
            reloj.text = "LUN 6:45";
            yield return c.Esperar(0.6f);
        }
        yield return c.Decir("Voz de Camila", "Funcionó. El día se repite. Y su hermanita... vino a la U.", 3.4f);
    }

    static IEnumerator YoRecuerdo(Cine c)
    {
        CineFinales.DetrasDelTelon(c);
        SpriteRenderer cam = CineFinales.Pj(Arte.Camila(), 0.6f, true);
        cam.sprite = Arte.Camila().agachada;
        CineFinales.Obj(Arte.Mascara(), -0.4f, 0f, 501);
        CineFinales.Obj(Arte.Cuchillo(), -1f, 0.05f, 501);
        Cam(c, 0f, 1.2f, 2.2f);
        yield return Ver(c);
        Juego.I.ui?.Destello(new Color(0.7f, 0f, 0f), 0.4f);
        yield return c.Decir("Voz de Camila", "Recuerdo cada vez que la mato. CADA VEZ.", 3f);
        yield return c.Decir("Voz de Camila", "Ya no quiero. Pero si me detengo, el día vuelve a empezar igual.", 3.4f);
        yield return c.Decir("Voz de Camila", "La ñatita no me deja ir.", 2.4f);
    }

    static IEnumerator SeRio(Cine c)
    {
        CineFinales.Dormitorio(c, "#c8b6ff", "#7b2cbf", false);
        SpriteRenderer val = CineFinales.Pj(Arte.Valeria(), -0.2f, false);
        CineFinales.Reloj("SÁB 0:03");
        CineFinales.Pantalla(new Vector2(2.6f, 3.2f),
            "GRUPO: FAMILIA QUISPE\n\nSeba: mira lo que le hicimos\na una cachimba jaja  [video]\n\nValeria:  jajajaja");
        Cam(c, 1f, 2f, 3f);
        yield return Ver(c);
        Sonido.Efecto("blip", 0.6f, 1.5f);
        yield return c.Decir("Voz de Camila", "Sebastián mandó el video al grupo de su familia.", 3f);
        yield return c.Decir("Voz de Camila", "Su hermanita respondió. Valeria. Ella también se rió.", 3.2f);
        val.sprite = Arte.Valeria().Frente;
        yield return c.Decir("Valeria", "...Ni siquiera vi el video completo. Solo me reí.", 3f);
        val.color = Util.Hex("#ffb3b3");
    }

    // =================================================================
    //  PISTAS DE LAS SALAS PARA EXPLORAR
    // =================================================================
    static IEnumerator Anuario(Cine c)
    {
        Sala(c, "#3a3428", Arte.Suelo(TipoSuelo.Alfombra, Util.Hex("#2f5d50")), true);
        for (int k = 0; k < 4; k++) CineFinales.Obj(Arte.Estante(), -4.5f + k * 3f, 1.2f, -2960);
        Mesa(0.4f, 2.4f);
        Rect(0.5f, 0.56f, 0.8f, 0.1f, "#f1faee", 301);
        SpriteRenderer fig = CineFinales.Pj(Arte.Pepino(), -0.8f, false);
        Cam(c, 0f, 1.2f, 2.3f);
        yield return Ver(c);
        yield return c.Decir(null, "(Biblioteca, de noche. Alguien con máscara de Pepino abre el anuario de egresados.)", 3.2f);
        for (int i = 0; i < 6 && !c.Saltada; i++)
        {
            fig.sprite = i % 2 == 0 ? Arte.Pepino().ataque : Arte.Pepino().lado[0];
            Sonido.Efecto("estatica", 0.25f, 2.2f);
            yield return c.Esperar(0.22f);
        }
        Rect(0.5f, 0.6f, 0.5f, 0.06f, "#d62828", 302);
        yield return c.Decir(null, "(Raya la foto de SEBASTIÁN QUISPE con bolígrafo rojo. Hasta romper el papel.)", 3.2f);
    }

    static IEnumerator Presupuesto(Cine c)
    {
        Sala(c, "#c9d6d8", Arte.Suelo(TipoSuelo.Ceramica, Util.Hex("#b8c4c6")), true);
        Mesa(0.4f, 3f);
        for (int k = 0; k < 5; k++) CineFinales.Obj(Arte.Mascara(), -0.6f + k * 0.5f, 0.52f, 302).transform.localScale = new Vector3(0.7f, 0.7f, 1f);
        CineFinales.Pj(Funcionario, 2.3f, true);
        CineFinales.Pj(Arte.Pepino(), -1.7f, false);
        Cam(c, 0.3f, 1.3f, 2.5f);
        yield return Ver(c);
        yield return c.Decir("Encargado", "¿Cinco disfraces de Pepino? Pero en la Bienvenida solo se disfrazan cuatro.", 3.4f);
        yield return c.Decir("???", "...El quinto es para mí.", 2.6f);
    }

    static IEnumerator Afiche(Cine c)
    {
        Sala(c, "#f7d6e0", Arte.Suelo(TipoSuelo.Estudio, Util.Hex("#d8d8d8")), true);
        CineFinales.Pantalla(new Vector2(0.3f, 3.3f), "BIENVENIDA A LOS CACHIMBOS\n18:00  ·  AUDITORIO\n¡Ven disfrazado!");
        CineFinales.Obj(Arte.Mascara(), 2.9f, 2.6f, -2900);
        CineFinales.Pj(Nico, 1.4f, true);
        CineFinales.Pj(Arte.Camila(), -1.2f, false);
        Cam(c, 0.3f, 2f, 2.9f);
        yield return Ver(c);
        yield return c.Decir("Camila", "Que el Pepino se vea bien grande. Que todos lo vean.", 3f);
        yield return c.Decir("Nico", "Listo. ¿A nombre de quién facturo el afiche?", 2.6f);
        yield return c.Decir("Camila", "C. Rojas.", 2f);
    }

    // =================================================================
    //  LAS 4 PRUEBAS Y LA CINTA ROJA
    // =================================================================
    static IEnumerator Quemaduras(Cine c)
    {
        Sala(c, "#9fb8c4", Arte.Suelo(TipoSuelo.Clinico, Util.Hex("#cfdcdc")), true);
        CineFinales.Obj(Arte.CamaLado("#dfe7fd"), 0f, 0f, 2);
        CineFinales.Acostada(Herida);
        SpriteRenderer doc = CineFinales.Pj(Doctor, 2.4f, true);
        SpriteRenderer luz = CineFinales.Obj(Arte.Luz(), -0.6f, 1f, 600);
        luz.color = new Color(1f, 0.5f, 0.2f, 0.35f);
        luz.transform.localScale = new Vector3(2.2f, 2.2f, 1f);
        Cam(c, 0.4f, 1.5f, 2.6f);
        c.Cartel("CLÍNICA", "La noche de la Bienvenida pasada", 2.5f);
        yield return Ver(c);
        Sonido.Efecto("respiracion", 0.6f, 1f);
        yield return c.Decir("Doctor", "Quemaduras de segundo grado en brazos y cuello.", 3f);
        yield return c.Decir("Doctor", "¿Cómo te llamas? ...¿Quién te hizo esto?", 2.8f);
        doc.sprite = Doctor.lado[0];
        yield return c.Decir(null, "(Ella no responde. Solo mira la puerta. Tiene el pelo rojo.)", 3f);
    }

    static IEnumerator Denuncia(Cine c)
    {
        Sala(c, "#e9e4d4", Arte.Suelo(TipoSuelo.Madera, Util.Hex("#8d6748")), true);
        CineFinales.Pantalla(new Vector2(0.2f, 3.3f), "DENUNCIA N° 0241  ·  NOVATADA\nVíctima: CAMILA ROJAS\nAcusado: SEBASTIÁN QUISPE");
        Mesa(0.2f, 2.4f);
        CineFinales.Pj(Funcionario, 1.5f, true);
        SpriteRenderer cam = CineFinales.Pj(Arte.Camila(), -1.3f, false);
        Cam(c, 0f, 1.9f, 2.9f);
        yield return Ver(c);
        yield return c.Decir("Camila", "Quiero denunciar lo que me hicieron en la Bienvenida.", 3f);
        Juego.I.ui?.Telon(1f, 0.3f);
        yield return c.Esperar(0.5f);
        CineFinales.Texto("DOS SEMANAS DESPUÉS:  DENUNCIA RETIRADA", 0.2f, 2.2f, 0.45f, Util.Hex("#d62828"));
        cam.sprite = Arte.Camila().agachada;
        Juego.I.ui?.Telon(0f, 0.3f);
        yield return c.Decir(null, "(Dos semanas después, la denuncia fue retirada.)", 3f);
    }

    static IEnumerator Cicatriz(Cine c)
    {
        Sala(c, "#1b1822", Arte.Suelo(TipoSuelo.Estudio, Util.Hex("#2b2b33")), true);
        Rect(0f, 4.3f, 14f, 0.25f, "#3d3d45", -2900);                   // vigas
        CineFinales.Obj(Arte.CamaraTV(), -3f, 0f, 200);
        SpriteRenderer refl = CineFinales.Obj(Arte.Reflector(), 0.9f, 4.4f, 210);
        SpriteRenderer pep = CineFinales.Pj(Arte.Pepino(), -0.4f, false);
        pep.transform.position = O + new Vector2(-0.4f, 4.43f);
        Cam(c, 0f, 3.3f, 2.4f);
        yield return Ver(c);
        yield return c.Decir(null, "(La cámara del set. En las vigas del techo hay alguien.)", 2.6f);
        pep.sprite = Arte.Pepino().ataque;
        Sonido.Efecto("golpe", 0.6f, 1.3f);
        float t = 0f;
        Vector3 p0 = refl.transform.position;
        while (t < 0.5f && !c.Saltada) { t += Time.deltaTime; refl.transform.position = p0 + new Vector3(0f, -4.4f * (t / 0.5f) * (t / 0.5f), 0f); yield return null; }
        Sonido.Efecto("cristal", 0.8f, 1f);
        pep.sprite = Arte.Pepino().lado[0];
        yield return c.Mover(O + new Vector2(-0.3f, 5f), 1.2f, 1f);
        Rect(-0.15f, 5.0f, 0.22f, 0.22f, "#e0a98a", 600);              // la mano sin guante
        Rect(-0.15f, 5.0f, 0.22f, 0.05f, "#9c4a3a", 601);
        c.Cartel("CICATRICES", "", 2.4f);
        yield return c.Decir(null, "(Se quita un guante. La mano está llena de cicatrices de quemaduras.)", 3.2f);
    }

    static IEnumerator Registros(Cine c)
    {
        Sala(c, "#0d1117", Arte.Suelo(TipoSuelo.Tecnico, Util.Hex("#2b2f36")), true);
        for (int k = 0; k < 4; k++) CineFinales.Obj(Arte.Rack(), -5f + k * 1.6f, 0f, -2900);
        CineFinales.Pantalla(new Vector2(2.2f, 3f), "> usuario: c.rojas\n> abrir lista_bienvenida\n> mover 'Valeria Quispe'\n  a: RUTA ESPECIAL");
        Mesa(1.8f, 2f);
        SpriteRenderer pep = CineFinales.Pj(Arte.Pepino(), 0.8f, false);
        Cam(c, 1.4f, 2f, 2.7f);
        yield return Ver(c);
        for (int i = 0; i < 8 && !c.Saltada; i++)
        {
            pep.sprite = i % 2 == 0 ? Arte.Pepino().ataque : Arte.Pepino().lado[0];
            Sonido.Efecto("click", 0.5f, 1f + (i % 3) * 0.2f);
            yield return c.Esperar(0.18f);
        }
        yield return c.Decir(null, "(6:12 de la mañana. Alguien entra al sistema con el usuario c.rojas.)", 3.2f);
        yield return c.Decir(null, "(Pone a Valeria Quispe en la ruta del Pepino.)", 2.6f);
    }

    // =================================================================
    //  LOS 4 VIDEOS QUE RECUPERA MATEO
    // =================================================================
    static IEnumerator VideoFiesta(Cine c)
    {
        CineFinales.Auditorio(c, true);
        for (int i = 0; i < 3; i++) CineFinales.Pj(Estudiante(i + 1), -3f + i * 1.4f, false);
        SpriteRenderer cam = CineFinales.Pj(Arte.Camila(), 5f, true);
        Cam(c, 0.5f, 1.4f, 3f);
        yield return Ver(c);
        yield return c.StartCoroutine(c.CaminarLado(cam, Arte.Camila(), 1.6f, 1.6f));
        cam.sprite = Arte.Camila().Frente;
        yield return c.Decir("Voz de cachimbo", "¡Llegó la de Periodismo! ¡Saluda a la cámara!", 2.6f);
        yield return c.Decir("Camila", "(Ríe y saluda.) ¡Primera Bienvenida! ¡Graben bien, ah!", 2.8f);
    }

    static IEnumerator VideoEncendedor(Cine c)
    {
        CineFinales.Auditorio(c, false);
        SpriteRenderer cam = CineFinales.Pj(Arte.Camila(), 0.6f, true);
        SpriteRenderer seb = CineFinales.Pj(Seba, -0.6f, false);
        seb.sprite = Seba.lado[0];
        for (int i = 0; i < 3; i++) { CineFinales.Pj(Estudiante(i), 2.4f + i, true); CineFinales.Obj(Arte.Celular(), 2.05f + i, 0.9f, 600); }
        Cam(c, 0.6f, 1.3f, 2.5f);
        yield return Ver(c);
        yield return c.Decir("Voz de cachimbo", "¡Bautizo, bautizo, bautizo!", 2.2f);
        seb.sprite = Seba.ataque;
        yield return c.Decir(null, "(Un chico de último año, de espaldas a la cámara, saca un encendedor.)", 3f);
        Sonido.Efecto("grito", 0.9f, 1.15f);
        Juego.I.ui?.Destello(new Color(1f, 0.45f, 0.1f), 0.6f);
        cam.sprite = Arte.Camila().agachada;
        yield return c.Decir(null, "(El video se corta. Lo último que se escucha son risas.)", 2.8f);
    }

    static IEnumerator VideoClinica(Cine c)
    {
        Sala(c, "#1b2a3a", Arte.Suelo(TipoSuelo.Tecnico, Util.Hex("#2b2f36")), true);
        CineFinales.Pantalla(new Vector2(0f, 3f),
            "HISTORIA CLÍNICA DIGITAL  N° 7714\nPaciente: [tachado]  ->  recuperado: CAMILA ROJAS\nDiagnóstico: quemaduras de 2° grado\nAcompañante: (nadie)");
        Mesa(0f, 2.4f);
        CineFinales.Obj(Arte.Laptop(), 0.2f, 0.52f, 302);
        Cam(c, 0f, 2.4f, 2.8f);
        yield return Ver(c);
        for (int i = 0; i < 6 && !c.Saltada; i++) { Sonido.Efecto("click", 0.5f, 1f + (i % 3) * 0.2f); yield return c.Esperar(0.15f); }
        yield return c.Decir(null, "(En papel, el nombre estaba tachado. En la base de datos, no.)", 3f);
        yield return c.Decir(null, "(Llegó sola. Nadie la acompañó al hospital.)", 2.8f);
    }

    static IEnumerator VideoCara(Cine c)
    {
        CineFinales.Auditorio(c, false);
        SpriteRenderer seb = CineFinales.Pj(Seba, 0f, false);
        seb.sprite = Seba.Frente;
        CineFinales.Obj(Arte.Mascara(), 0.55f, 0.6f, 600).transform.localScale = new Vector3(0.7f, 0.7f, 1f);
        Cam(c, 0f, 1.1f, 1.8f);
        yield return Ver(c);
        yield return c.Decir("Voz de cachimbo", "¡Seba, Seba! ¡Mira a la cámara!", 2.2f);
        Sonido.Efecto("grito", 0.4f, 0.6f);
        yield return c.Decir("Sebastián", "(Se quita la máscara de Pepino y se ríe.) ¡Feliz primer día, cachimba!", 3f);
        c.Cartel("SEBASTIÁN QUISPE", "Egresado 2025", 2.6f);
        yield return c.Esperar(1.8f);
    }

    static IEnumerator CintaRoja(Cine c)
    {
        CineFinales.Auditorio(c, false);
        SpriteRenderer pep = CineFinales.Pj(Arte.Pepino(), -4f, false);
        Cam(c, -1.5f, 1.4f, 3f);
        yield return Ver(c);
        yield return c.StartCoroutine(c.CaminarLado(pep, Arte.Pepino(), -0.5f, 1.4f));
        yield return c.Esperar(0.4f);
        yield return c.Mover(O + new Vector2(-0.2f, 0.9f), 1.2f, 1f);
        Rect(-0.15f, 0.72f, 0.2f, 0.08f, "#d62828", 600);
        c.Cartel("LA CINTA ROJA", "", 2.4f);
        yield return c.Decir(null, "(Se acomoda el guante. En la muñeca lleva una CINTA ROJA.)", 3f);
    }
}
