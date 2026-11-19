using System.Collections;
using UnityEngine;

// ⚖️ DERECHO — Sala de Juicios Simulados (Piso 1)
// Minijuego: ¡Objeción! · Terror: el juez es el Pepino y cierra las puertas
// Salvarse: esconderse en el estrado de testigos y moverse cuando no mira
// Prueba 2: el expediente de la novatada (+ post-it con la contraseña del sistema).
public class FacDerecho : Facultad
{
    protected override string SelloId { get { return Ids.SelloDerecho; } }
    protected override string NombreCarrera { get { return "Derecho"; } }

    Personaje guia;
    SpriteRenderer juez;
    bool ventanaAbierta, seEscondio;
    Vector2 posEstrado;

    protected override void Montar()
    {
        juez = Figura("Juez", sala.P(0f, 3.55f), Arte.Personaje(Arte.Ap("#111111", "#c8a27a", "#9e9e9e", "#111111", false, Accesorio.Toga, null)).Frente);
        Obj(Arte.MesaLarga("#5a3a22", 80), sala.P(0f, 2.2f), new Vector2(5f, 0.6f));
        Util.Texto("JUEZ", transform, sala.P(0f, 2.55f), 0.7f, Util.Hex("#ffd166"), 0);
        // Escudo en la pared
        Util.Rect("Escudo", transform, sala.P(0f, 4.0f), new Vector2(1.2f, 0.7f), Util.Hex("#d4a017"), -1470);
        Util.Rect("Escudo", transform, sala.P(0f, 4.0f), new Vector2(0.9f, 0.45f), Util.Hex("#0b3d91"), -1465);
        for (int i = 0; i < 3; i++)
        {
            Obj(Arte.Banca(), sala.P(-6f, -0.4f - i * 1.4f), new Vector2(3.8f, 0.4f));
            Obj(Arte.Banca(), sala.P(6f, -0.4f - i * 1.4f), new Vector2(3.8f, 0.4f));
        }
        posEstrado = sala.P(9f, 2.2f);
        Obj(Arte.MesaLarga("#7a5536", 28), posEstrado + new Vector2(0f, -0.7f));
        Util.Texto("Testigos", transform, posEstrado + new Vector2(0, 1.1f), 0.6f, Util.Hex("#ffd166"), 0);
        Obj(Arte.Estante(), sala.P(-10.5f, 2.9f), new Vector2(1.9f, 0.5f));
        Obj(Arte.Planta(), sala.P(11f, -3.8f));

        guia = Personaje.Crear(transform, "Martín (Derecho)", sala.P(-9f, 0.6f),
            Arte.Personaje(Arte.Ap("#1d3557", "#b07d56", "#1b1b1b", "#2b2d42", false, Accesorio.Ninguno, null)),
            delegate { Lanzar(HablarGuia()); });

        Escondite(posEstrado, "Esconderte en el estrado de testigos", delegate { return ventanaAbierta; },
            delegate { seEscondio = true; });

        Interactuable archivero = Interactuable.Crear(transform, sala.P(-10.5f, 2.1f), "Revisar el archivero",
            delegate { Lanzar(Archivero()); }, 1.5f);
        archivero.disponible = delegate { return !Memoria.Tiene(Ids.PruebaExpediente) && !enEvento; };
    }

    IEnumerator HablarGuia()
    {
        string[] explicacion =
        {
            "Bienvenida a la Sala de Juicios Simulados de DERECHO.",
            "Aquí practicamos juicios como si fueran reales: hay juez, fiscal, defensa y testigos.",
            "En Derecho estudias leyes, la Constitución, derecho penal, civil, laboral... y aprendes a argumentar y defender casos.",
            "Tu prueba: un juicio simulado. Cuando un testigo mienta, elige la EVIDENCIA que lo contradice (teclas 1-4)."
        };
        yield return FlujoGuia("Martín", explicacion, delegate
        {
            MJPreguntas mj = Minijuego.Crear<MJPreguntas>("¡OBJECIÓN!", "Escucha al testigo y presenta la evidencia que lo contradice.");
            mj.maxErrores = 1;
            mj.textoExito = "¡OBJECIÓN ACEPTADA!";
            mj.textoFallo = "OBJECIÓN DENEGADA";
            mj.AgregarRondaConRespuesta("TESTIGO: \"A las 10:00 vi al acusado en la cafetería.\"", "La foto lo pone en otro lugar. ¡Bien!", 1,
                "El carnet universitario del acusado",
                "Foto con hora: el acusado en la BIBLIOTECA a las 10:00",
                "Un recibo de fotocopias de las 15:00",
                "El horario de clases del testigo");
            mj.AgregarRondaConRespuesta("TESTIGO: \"El acusado nunca tuvo la llave del laboratorio.\"", "El registro tiene su firma. ¡Exacto!", 2,
                "Una lista de compras",
                "Una mochila vacía",
                "Registro de préstamo de llaves FIRMADO por el acusado",
                "Una foto del laboratorio cerrado");
            mj.AgregarRondaConRespuesta("TESTIGO: \"Jamás hablé con la víctima.\"", "Los mensajes lo desmienten. ¡Caso cerrado!", 0,
                "Capturas de MENSAJES entre el testigo y la víctima",
                "El certificado de nacimiento del testigo",
                "Un mapa de La Paz",
                "Una nota de la materia de Historia");
            return mj;
        }, "dom_derecho");
    }

    protected override IEnumerator EventoTerror()
    {
        juego.Bloquear(this);
        yield return Decir("Martín", "Ahora el juez da la sentencia. Es parte del ejercicio... voy por los expedientes, ya vuelvo.");
        guia.Mostrar(false);
        yield return Esperar(0.6f);
        yield return Decir("Valeria", "El \"juez\" no se quita la toga. Ni se mueve.", "Despacio... gira la cabeza hacia mí.");
        juez.sprite = Util.SpritePepino();
        UI.Golpe(0.4f);
        UI.Oscuridad(0.55f);
        yield return Decir("Juez", "La acusada... VALERIA QUISPE...", "...es CULPABLE.");
        sala.CerrarPuerta();
        UI.Toast("¡Las puertas se cerraron!", 2f);
        yield return Esperar(0.5f);
        juego.Desbloquear(this);

        UI.Toast(Memoria.Sabe("der_estrado")
            ? "¡El estrado de testigos (derecha)! Escóndete (E) y muévete cuando no mire."
            : "El juez baja del estrado con el mazo en la mano...", 5f);

        seEscondio = false;
        ventanaAbierta = true;
        yield return Ventana(4.5f, delegate { return seEscondio; });
        ventanaAbierta = false;
        if (juego.Terminado) yield break;

        if (!seEscondio)
        {
            yield return Abalanzarse(juez);
            Morir("El mazo cayó. Una y otra vez.\nEl eco en la sala de juicios duró más que tú.", "der_estrado", TipoMuerte.Mazo);
            yield break;
        }

        J.Teletransportar(posEstrado);
        J.Esconder(true);
        juez.transform.position = sala.P(3f, 1f);

        MJSecuencia mj = Minijuego.Crear<MJSecuencia>("BAJO EL ESTRADO",
            "Ves sus pies caminando. Muévete de escondite en escondite justo cuando no mira: ¡presiona la flecha a tiempo!");
        mj.largo = 7;
        mj.ventana = Memoria.Sabe("der_estrado") ? 1.4f : 1.0f;
        mj.maxErrores = 1;
        mj.textoExito = "SALIÓ DE LA SALA...";
        mj.textoFallo = "TE VIO";
        Resultado<bool> ok = new Resultado<bool>();
        yield return mj.Jugar(ok);

        if (!ok.valor)
        {
            juez.transform.position = posEstrado + new Vector2(-1.2f, 0f);
            yield return Abalanzarse(juez);
            Morir("Te encontró debajo del estrado.\n\"Sentencia ejecutada\", dijo, antes de que el mazo cayera.", "der_estrado", TipoMuerte.Mazo);
            yield break;
        }

        juez.gameObject.SetActive(false);
        J.Esconder(false);
        sala.AbrirPuerta();
        UI.Oscuridad(0f);
        sobrevivio = true;
        Memoria.Aprender("der_estrado");
        yield return Decir("Valeria", "Se fue. Las puertas se abrieron solas.", "El archivero del fondo quedó abierto...");
    }

    IEnumerator Archivero()
    {
        if (!sobrevivio)
        {
            yield return Decir("Archivero", "Expedientes de juicios simulados. Está cerrado con llave.");
            yield break;
        }
        yield return Decir("Valeria",
            "Uno de los expedientes no es simulado. Es una denuncia REAL.");
        yield return Recuerdos.Reproducir("motivo");
        yield return Decir("Valeria", "...Sebastián. Mi hermano.",
            "Pegado en la tapa hay un post-it: \"Sistema U — clave: cachimbo2025\".");
        Memoria.Obtener(Ids.PruebaExpediente);
        Memoria.Aprender("motivo");
        Memoria.Aprender("password");
        UI.Toast("PRUEBA OBTENIDA: El expediente  (" + Memoria.Cuantos(Ids.Pruebas) + "/4)", 4f);
    }
}
