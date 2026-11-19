using System.Collections;
using UnityEngine;

// 🎥 PERIODISMO — Set de TV (Piso 2)
// Minijuego: Al aire (teleprompter) · Terror: "MIRA ARRIBA" y cae el reflector
// Salvarse: QTE, moverse al instante · Prueba 3: la grabación (editar los clips).
public class FacPeriodismo : Facultad
{
    protected override string SelloId { get { return Ids.SelloPeriodismo; } }
    protected override string NombreCarrera { get { return "Periodismo"; } }

    Personaje guia;
    SpriteRenderer reflector;
    Vector2 marca;

    protected override void Montar()
    {
        marca = sala.P(0f, 0.6f);
        Util.Rect("Pantalla", transform, sala.P(0f, 3.9f), new Vector2(9f, 0.95f), Util.Hex("#1a1a2e"), -1475);
        Util.Rect("Pantalla luz", transform, sala.P(0f, 3.9f), new Vector2(8.6f, 0.7f), Util.Hex("#3a0ca3"), -1470);
        Util.Texto("NOTICIAS UNIFRANZ", transform, sala.P(0f, 3.9f), 1f, Util.Hex("#ffd166"), 0);
        Obj(Arte.MesaLarga("#3a0ca3", 72), sala.P(0f, 1.9f), new Vector2(4.4f, 0.6f));
        Util.Texto("X", transform, marca + new Vector2(0f, -0.6f), 1.4f, Util.Hex("#ffd166"), -2990);
        Obj(Arte.CamaraTV(), sala.P(-1.2f, -3.2f), new Vector2(0.8f, 0.4f));
        Obj(Arte.CamaraTV(), sala.P(1.8f, -3.2f), new Vector2(0.8f, 0.4f));
        Util.Rect("Teleprompter", transform, sala.P(0f, -1.6f), new Vector2(1.6f, 0.2f), Util.Hex("#4cc9f0"), -1300);
        reflector = Util.Plano("Reflector", transform, Arte.Reflector(), marca + new Vector2(0, 3.3f), 2500);
        reflector.transform.position = marca + new Vector2(0, 3.3f);       // por si quedó caído de otro día
        Util.Plano("Reflector", transform, Arte.Reflector(), sala.P(-4f, 3.3f), 2500);
        Util.Plano("Reflector", transform, Arte.Reflector(), sala.P(4f, 3.3f), 2500);
        Obj(Arte.Escritorio(true, "#f72585"), sala.P(9.5f, 2.6f), new Vector2(1.8f, 0.6f));
        Obj(Arte.Silla("#f72585"), sala.P(9.5f, 1.85f));
        Util.Texto("Edición", transform, sala.P(9.5f, 4.0f), 0.7f, Util.Hex("#2b2433"), 0);
        Obj(Arte.Microfono(), sala.P(-9.8f, 2.9f));
        Obj(Arte.Planta(), sala.P(-11f, -3.8f));

        guia = Personaje.Crear(transform, "Sofía (Periodismo)", sala.P(-8f, -1f),
            Arte.Personaje(Arte.Ap("#f72585", "#8d5524", "#2b2b2b", "#14213d", true, Accesorio.Ninguno, null)),
            delegate { Lanzar(HablarGuia()); });

        Interactuable cabina = Interactuable.Crear(transform, sala.P(9.5f, 1.6f), "Usar la cabina de edición",
            delegate { Lanzar(Cabina()); }, 1.6f);
        cabina.disponible = delegate { return !Memoria.Tiene(Ids.PruebaGrabacion) && !enEvento; };
    }

    IEnumerator HablarGuia()
    {
        string[] explicacion =
        {
            "¡Hola! Este es el set de TV de PERIODISMO. También tenemos cabina de radio y sala de edición.",
            "En Periodismo aprendes a investigar, entrevistar, redactar noticias, presentar en cámara y producir contenido digital.",
            Dificultad.SegunReto(
                "Una noticia sigue un orden lógico, como un diagrama de flujo. Tu prueba: responde cómo se arma una noticia (teclas 1-4).",
                "Tu prueba: presentar una noticia EN VIVO. Párate en la X y sigue el teleprompter: presiona cada flecha a tiempo.",
                "Tu prueba: una noticia larga EN VIVO y sin pausas. Párate en la X y sigue el teleprompter: presiona cada flecha a tiempo.")
        };
        yield return FlujoGuia("Sofía", explicacion, delegate
        {
            if (Dificultad.Reto == NivelReto.Basico) return RetoBasico();
            J.Teletransportar(marca);
            MJSecuencia mj = Minijuego.Crear<MJSecuencia>(TituloReto("¡AL AIRE!"), "Lee el teleprompter sin trabarte: presiona la flecha de cada palabra.");
            if (Dificultad.Reto == NivelReto.Avanzado)
            {
                mj.palabras = new string[] { "Buenos", "días,", "La", "Paz.", "Hoy", "comienza", "la", "Semana", "de", "Inducción", "en", "Unifranz,", "con", "más", "de", "mil", "estudiantes", "nuevos." };
                mj.ventana = 1.25f;
            }
            else
            {
                mj.palabras = new string[] { "Buenos", "días,", "La", "Paz.", "Hoy", "comienza", "la", "Semana", "de", "Inducción", "en", "Unifranz." };
                mj.ventana = 1.5f;
            }
            mj.maxErrores = 2;
            mj.textoExito = "¡QUÉ BUENA TOMA!";
            mj.textoFallo = "TE TRABASTE...";
            return mj;
        }, "dom_periodismo");
    }

    // BÁSICO: el orden lógico de una noticia y las decisiones antes de publicar.
    Minijuego RetoBasico()
    {
        MJPreguntas mj = Minijuego.Crear<MJPreguntas>(TituloReto("ARMA LA NOTICIA"), "Elige la respuesta correcta (teclas 1-4).");
        mj.maxErrores = 2;
        mj.textoExito = "¡NOTICIA LISTA PARA SALIR!";
        mj.textoFallo = "ESA NOTICIA NO SALE AL AIRE";
        mj.etiquetaRonda = "Pregunta";
        mj.AgregarRondaConRespuesta("Orden de una noticia: ¿qué va PRIMERO?", "Correcto: titular, entrada, cuerpo y cierre.", 1,
            "El cierre",
            "El titular",
            "El cuerpo",
            "La firma del periodista");
        mj.AgregarRondaConRespuesta("¿Para qué sirve la ENTRADA de una noticia?", "Así es: resume lo más importante en pocas líneas.", 0,
            "Para resumir lo más importante al inicio",
            "Para poner la publicidad",
            "Para despedirse del público",
            "Para escribir la fecha");
        mj.AgregarRondaConRespuesta("DECISIÓN: ¿La fuente está verificada?  → NO.  ¿Qué haces?", "Bien: primero se verifica, después se publica.", 2,
            "Publicarla igual, es urgente",
            "Inventar los datos que faltan",
            "No publicar todavía y verificar la información",
            "Compartirla solo en redes sociales");
        mj.AgregarRondaConRespuesta("Las 5 W son: qué, quién, cuándo, dónde y...", "¡Exacto! ...y por qué.", 3,
            "cuánto cuesta",
            "quién la escribió",
            "qué opino yo",
            "por qué");
        return mj;
    }

    protected override IEnumerator EventoTerror()
    {
        juego.Bloquear(this);
        J.Teletransportar(marca);
        yield return Decir("Sofía", "¡Perfecto! Hagamos una toma más, en vivo de verdad.", "¡Al aire en 3... 2... 1...!");
        guia.Mostrar(false);
        UI.Oscuridad(0.35f);
        yield return Decir("Teleprompter", "<color=#ff4d4d>V  A  L  E  R  I  A</color>", "<color=#ff4d4d>M  I  R  A . . .</color>");

        MJSecuencia mj = Minijuego.Crear<MJSecuencia>("¡MUÉVETE!", "");
        mj.textoArriba = "M  I  R  A     A  R  R  I  B  A";
        mj.largo = 1;
        mj.ventana = Memoria.Sabe("per_reflector") ? 1.6f : 0.75f;
        mj.maxErrores = 0;
        mj.textoExito = "¡TE MOVISTE!";
        mj.textoFallo = "";
        Resultado<bool> ok = new Resultado<bool>();
        yield return mj.Jugar(ok);

        if (!ok.valor)
        {
            yield return CaeReflector(marca);
            Morir("Un reflector de 40 kilos. En vivo.\nLa transmisión se llenó de estática... y de rojo.", "per_reflector", TipoMuerte.Reflector);
            yield break;
        }

        J.Teletransportar(marca + new Vector2(1.6f, -0.4f));
        yield return CaeReflector(marca);
        UI.Oscuridad(0f);
        juego.Desbloquear(this);
        sobrevivio = true;
        Memoria.Aprender("per_reflector");
        yield return Decir("Valeria", "Cayó justo donde yo estaba parada.", "Sofía desapareció... pero la cámara sigue grabando.", "¡La cabina de edición! Ahí debe estar el video.");
    }

    IEnumerator CaeReflector(Vector2 destino)
    {
        Vector3 a = reflector.transform.position;
        Vector3 b = destino;
        float t = 0f;
        while (t < 0.25f)
        {
            t += Time.deltaTime;
            reflector.transform.position = Vector3.Lerp(a, b, t / 0.25f);
            yield return null;
        }
        UI.Golpe(0.7f);
        yield return new WaitForSeconds(0.4f);
    }

    IEnumerator Cabina()
    {
        if (!sobrevivio)
        {
            yield return Decir("Cabina de edición", "Computadoras de edición. Están bloqueadas mientras graban en el set.");
            yield break;
        }
        MJOrdenar mj = Minijuego.Crear<MJOrdenar>("RESCATAR LA GRABACIÓN", "Ordena los clips de la cámara por tiempo antes de que el archivo se corrompa.");
        mj.ordenCorrecto.Add("00:00 — Set vacío, luces encendidas");
        mj.ordenCorrecto.Add("00:12 — Valeria se para en la X");
        mj.ordenCorrecto.Add("00:19 — Algo se mueve en las vigas");
        mj.ordenCorrecto.Add("00:23 — El reflector cae");
        mj.ordenCorrecto.Add("00:27 — El Pepino se quita un guante");
        mj.tiempoLimite = 30f;
        mj.maxErrores = 2;
        mj.textoExito = "VIDEO RECUPERADO";
        mj.textoFallo = "ARCHIVO CORRUPTO";
        mj.tituloIzquierda = "CLIPS";
        mj.tituloDerecha = "LÍNEA DE TIEMPO";
        Resultado<bool> ok = new Resultado<bool>();
        yield return mj.Jugar(ok);
        if (!ok.valor)
        {
            yield return Decir("Valeria", "Se trabó... Todavía queda una copia. Lo intento otra vez.");
            yield break;
        }
        yield return Decir("Valeria", "Ahí está. La grabación del set...");
        yield return Recuerdos.Reproducir("cicatriz");
        Memoria.Obtener(Ids.PruebaGrabacion);
        Memoria.Aprender("cicatriz");
        UI.Toast("PRUEBA OBTENIDA: La grabación  (" + Memoria.Cuantos(Ids.Pruebas) + "/4)", 4f);
    }
}
