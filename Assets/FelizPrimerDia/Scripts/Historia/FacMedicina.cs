using System.Collections;
using UnityEngine;

// 🏥 MEDICINA — Sala de Simulación (Piso 3)
// Minijuego: RCP al ritmo · Terror: el maniquí de más · Salvarse: esconderse bajo la sábana
// Prueba 1: la historia clínica.
public class FacMedicina : Facultad
{
    protected override string SelloId { get { return Ids.SelloMedicina; } }
    protected override string NombreCarrera { get { return "Medicina"; } }

    Personaje guia;
    bool ventanaAbierta, seEscondio;
    Vector2 posCamilla;

    protected override void Montar()
    {
        // Camillas con maniquíes acostados
        Sprite maniqui = Arte.Maniqui().Frente;
        for (int i = 0; i < 3; i++)
        {
            Vector2 b = sala.P(-4f + i * 3.5f, 0.6f);
            Obj(Arte.Camilla(), b, new Vector2(1.4f, 1f));
            SpriteRenderer m = Figura("Maniquí", b + new Vector2(0f, 1.25f), maniqui);
            m.GetComponent<OrdenPorY>().extra = 120;
            Obj(Arte.UPS(), b + new Vector2(1.1f, 2.3f));
        }
        posCamilla = sala.P(8.5f, 0.5f);
        Obj(Arte.Camilla(), posCamilla + new Vector2(0f, -1.1f));
        Obj(Arte.Estante(), sala.P(10.5f, 2.9f), new Vector2(1.9f, 0.5f));
        Obj(Arte.MesaLab(), sala.P(-9.5f, 2.9f), new Vector2(2.4f, 0.5f));
        Obj(Arte.Planta(), sala.P(-11f, -3.8f));
        Util.Rect("Cruz", transform, sala.P(-5.5f, 3.9f), new Vector2(0.7f, 0.22f), Util.Hex("#e63946"), -1470);
        Util.Rect("Cruz", transform, sala.P(-5.5f, 3.9f), new Vector2(0.22f, 0.7f), Util.Hex("#e63946"), -1470);

        guia = Personaje.Crear(transform, "Lucía (5to año)", sala.P(-8f, -1f),
            Arte.Personaje(Arte.Ap("#4a6fa5", "#a47148", "#3d2b1f", "#4a6fa5", true, Accesorio.Bata, null)),
            delegate { Lanzar(HablarGuia()); });

        Escondite(posCamilla, "Esconderte bajo la sábana", delegate { return ventanaAbierta; },
            delegate { seEscondio = true; });

        Interactuable archivo = Interactuable.Crear(transform, sala.P(10.5f, 2.1f), "Revisar el archivo de la clínica",
            delegate { Lanzar(Archivo()); }, 1.5f);
        archivo.disponible = delegate { return !Memoria.Tiene(Ids.PruebaHistoria) && !enEvento; };
    }

    IEnumerator HablarGuia()
    {
        string[] explicacion =
        {
            "¡Hola! Bienvenida a la Sala de Simulación de MEDICINA.",
            "Estos maniquíes respiran, tienen pulso y hasta 'sangran'. Así practicamos antes de atender pacientes reales.",
            "En Medicina estudias anatomía, fisiología, farmacología... y haces prácticas en la clínica y en hospitales.",
            Dificultad.SegunReto(
                "En Medicina todo sigue un orden, como un diagrama de flujo. Tu prueba: responde lo básico de la bioseguridad y la atención (teclas 1-4).",
                "Tu prueba: hacer RCP (reanimación). Presiona ESPACIO justo cuando la línea pase por la zona verde. ¡Al ritmo!",
                "Tu prueba: RCP de verdad. Más compresiones, menos margen y casi sin errores. Presiona ESPACIO en la zona verde.")
        };
        yield return FlujoGuia("Lucía", explicacion, delegate
        {
            if (Dificultad.Reto == NivelReto.Basico) return RetoBasico();
            MJRitmo mj = Minijuego.Crear<MJRitmo>(TituloReto("RCP AL RITMO"), "Presiona ESPACIO cuando la línea blanca pase por la zona verde.");
            if (Dificultad.Reto == NivelReto.Avanzado)
            {
                mj.objetivo = 12;
                mj.maxFallos = 2;
                mj.anchoZona = 0.13f;
            }
            else
            {
                mj.objetivo = 8;
                mj.maxFallos = 3;
            }
            return mj;
        }, "dom_medicina");
    }

    // BÁSICO: lógica de la atención y bioseguridad, con pasos y decisiones como en un diagrama de flujo.
    Minijuego RetoBasico()
    {
        MJPreguntas mj = Minijuego.Crear<MJPreguntas>(TituloReto("PRIMEROS PASOS"), "Elige la respuesta correcta (teclas 1-4).");
        mj.maxErrores = 2;
        mj.textoExito = "¡LISTA PARA LA SIMULACIÓN!";
        mj.textoFallo = "REPASA LO BÁSICO";
        mj.etiquetaRonda = "Pregunta";
        mj.AgregarRondaConRespuesta("Flujo del lavado de manos: ¿cuál es el PRIMER paso?", "Primero se mojan las manos; luego jabón, frotar, enjuagar y secar.", 2,
            "Secarse con la toalla",
            "Frotar entre los dedos",
            "Mojarse las manos con agua",
            "Ponerse los guantes");
        mj.AgregarRondaConRespuesta("DECISIÓN: ¿El paciente respira?  → NO.  ¿Qué sigue en el diagrama?", "Correcto: pedir ayuda y empezar la RCP.", 0,
            "Pedir ayuda e iniciar la RCP",
            "Esperar a que despierte solo",
            "Darle agua",
            "Ir a buscar su historia clínica");
        mj.AgregarRondaConRespuesta("¿Para qué sirven la BATA y el BARBIJO?", "Así es: protegen al paciente y a ti de contagios.", 1,
            "Para verse elegante en la clínica",
            "Para protegerse a uno mismo y al paciente de contagios",
            "Para que no haga frío",
            "Para saber quién es el docente");
        mj.AgregarRondaConRespuesta("SIGNOS VITALES: ¿cuál es una temperatura corporal normal?", "Bien: alrededor de 36,5 °C es lo normal.", 3,
            "30 °C",
            "40 °C",
            "42 °C",
            "Alrededor de 36,5 °C");
        return mj;
    }

    protected override IEnumerator EventoTerror()
    {
        juego.Bloquear(this);
        yield return Decir("Lucía", "Voy a traer el desfibrilador de práctica. No toques nada, ¿ya?");
        guia.Mostrar(false);
        yield return Esperar(0.8f);

        UI.Oscuridad(0.92f);
        UI.Toast("Se cortó la luz...", 2f);
        yield return Esperar(1.6f);
        // Aparece un "maniquí" de más
        SpriteRenderer pepino = Figura("¿Maniquí?", sala.P(-6.8f, 1.6f), Util.SpritePepino());
        UI.Oscuridad(0.7f);
        yield return Esperar(0.8f);
        yield return Decir("Valeria", "La luz vuelve a medias... y hay UN MANIQUÍ MÁS.", "Ese no tiene cables.", "...Y está respirando.");
        juego.Desbloquear(this);

        UI.Toast(Memoria.Sabe("med_maniqui")
            ? "¡La camilla con sábana de la derecha! Escóndete (E) y contén la respiración."
            : "Algo no está bien... ¿dónde podrías esconderte?", 5f);

        seEscondio = false;
        ventanaAbierta = true;
        yield return Ventana(5f, delegate { return seEscondio; });
        ventanaAbierta = false;
        if (juego.Terminado) yield break;

        if (!seEscondio)
        {
            yield return Abalanzarse(pepino);
            Morir("El maniquí se levantó. Bata quirúrgica, máscara sonriente... y un bisturí.\nEl monitor marcó línea plana. La tuya.", "med_maniqui", TipoMuerte.Bisturi);
            yield break;
        }

        J.Teletransportar(posCamilla);
        J.Esconder(true);
        pepino.transform.position = sala.P(0f, 0f);

        MJRespiracion mj = Minijuego.Crear<MJRespiracion>("BAJO LA SÁBANA",
            "El Pepino camina entre las camillas buscándote. Mantén ESPACIO cuando esté al lado.");
        Resultado<bool> ok = new Resultado<bool>();
        yield return mj.Jugar(ok);

        if (!ok.valor)
        {
            pepino.transform.position = posCamilla + new Vector2(-1f, 0f);
            yield return Abalanzarse(pepino);
            Morir("La sábana se levantó despacio. Una máscara sonriente te miraba desde arriba.\nBisturí.", "med_maniqui", TipoMuerte.Bisturi);
            yield break;
        }

        Destroy(pepino.gameObject);
        J.Esconder(false);
        UI.Oscuridad(0f);
        sobrevivio = true;
        yield return Decir("Valeria", "Se fue... Lucía no volvió.", "Dejó sus llaves tiradas junto al ARCHIVO de la clínica.");
        Memoria.Aprender("med_maniqui");
    }

    IEnumerator Archivo()
    {
        if (!sobrevivio)
        {
            yield return Decir("Archivo", "Está cerrado con llave. \"Solo personal autorizado\".");
            yield break;
        }
        yield return Decir("Valeria", "Abro el archivo con las llaves de Lucía. Una historia clínica de la Bienvenida del año pasado...");
        yield return Recuerdos.Reproducir("quemaduras");
        yield return Decir("Valeria", "El nombre está tachado con marcador.");
        Memoria.Obtener(Ids.PruebaHistoria);
        Memoria.Aprender("quemaduras");
        UI.Toast("PRUEBA OBTENIDA: Historia clínica  (" + Memoria.Cuantos(Ids.Pruebas) + "/4)", 4f);
    }
}
