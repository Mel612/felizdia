using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Rondas de opción múltiple (teclas 1-4). Sirve para:
//  - Derecho: "¡Objeción!"   - Sistemas: "Hackeo"   - Final: "¿Quién es?"
public class MJPreguntas : Minijuego
{
    public int maxErrores = 1;            // errores permitidos (al superar este número, pierdes)
    public float tiempoPorRonda = 0f;     // 0 = sin límite
    public string textoExito = "¡CORRECTO!";
    public string textoFallo = "FALLASTE";
    public string etiquetaRonda = "Ronda";

    class Ronda
    {
        public string enunciado;
        public string[] opciones;
        public int correcta;
        public string siAcierta;
    }

    readonly List<Ronda> rondas = new List<Ronda>();
    int ronda, errores;
    float tRonda;
    Text txtEnunciado, txtOpciones, txtFeedback;
    Barra barraTiempo;

    // correcta: índice desde 0
    public void AgregarRonda(string enunciado, int correcta, params string[] opciones)
    {
        Ronda r = new Ronda();
        r.enunciado = enunciado;
        r.correcta = correcta;
        r.opciones = opciones;
        rondas.Add(r);
    }

    public void AgregarRondaConRespuesta(string enunciado, string siAcierta, int correcta, params string[] opciones)
    {
        AgregarRonda(enunciado, correcta, opciones);
        rondas[rondas.Count - 1].siAcierta = siAcierta;
    }

    protected override void Construir()
    {
        txtEnunciado = TextoEn(area, "", 32, Color.white, new Vector2(0, 150), new Vector2(1180, 200), TextAnchor.MiddleCenter);
        txtOpciones = TextoEn(area, "", 30, Util.Hex("#e8e8e8"), new Vector2(0, -60), new Vector2(1100, 240), TextAnchor.UpperLeft);
        txtFeedback = TextoEn(area, "", 28, Util.Hex("#ffd166"), new Vector2(0, -210), new Vector2(1180, 50), TextAnchor.MiddleCenter);
        if (tiempoPorRonda > 0f)
            barraTiempo = new Barra(area, new Vector2(0, 260), new Vector2(900, 14), Util.Hex("#222230"), Util.Hex("#ffd166"));
        Mostrar();
    }

    void Mostrar()
    {
        Ronda r = rondas[ronda];
        txtEnunciado.text = r.enunciado;
        string ops = "";
        for (int i = 0; i < r.opciones.Length; i++)
            ops += "<color=#ffd166>" + (i + 1) + ")</color>  " + r.opciones[i] + "\n";
        txtOpciones.text = ops;
        txtEstado.text = (rondas.Count > 1 ? etiquetaRonda + " " + (ronda + 1) + "/" + rondas.Count + "     " : "") +
                         "Errores: " + errores + "/" + maxErrores;
        tRonda = 0f;
    }

    protected override void Tick(float dt)
    {
        Ronda r = rondas[ronda];
        if (tiempoPorRonda > 0f)
        {
            tRonda += dt;
            barraTiempo.Valor(1f - tRonda / tiempoPorRonda);
            if (tRonda >= tiempoPorRonda) { Error("¡Se acabó el tiempo!"); return; }
        }

        int n = Entrada.NumeroBajado();
        if (n < 1 || n > r.opciones.Length) return;

        if (n - 1 == r.correcta)
        {
            txtFeedback.text = string.IsNullOrEmpty(r.siAcierta) ? "¡Bien!" : r.siAcierta;
            ronda++;
            if (ronda >= rondas.Count) Terminar(true, textoExito);
            else Mostrar();
        }
        else Error("No era eso...");
    }

    void Error(string msg)
    {
        errores++;
        Juego.I.ui.Golpe(0.25f);
        txtFeedback.text = msg;
        tRonda = 0f;
        if (errores > maxErrores) Terminar(false, textoFallo);
        else Mostrar();
    }
}
