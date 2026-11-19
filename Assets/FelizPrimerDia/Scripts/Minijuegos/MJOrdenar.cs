using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Elige los elementos en el orden correcto (teclas 1-5, máximo 5 elementos).
// Sirve para: "Compila o muere" (Sistemas), editar la grabación (Periodismo) y los cables del UPS.
public class MJOrdenar : Minijuego
{
    public List<string> ordenCorrecto = new List<string>();
    public List<string> coloresHex;       // opcional: color de cada elemento (mismo orden que ordenCorrecto)
    public string pista = "";             // texto de ayuda arriba
    public float tiempoLimite = 0f;       // 0 = sin límite
    public int maxErrores = 2;
    public string textoExito = "¡LISTO!";
    public string textoFallo = "TE EQUIVOCASTE";
    public string tituloIzquierda = "DESORDENADO";
    public string tituloDerecha = "TU ORDEN";

    readonly List<int> restantes = new List<int>();
    readonly List<int> colocados = new List<int>();
    int errores;
    int sel;    // paso marcado con la flecha
    Text txtIzq, txtDer, txtPista, txtFeedback;
    Barra barraTiempo;

    protected override void Construir()
    {
        for (int i = 0; i < ordenCorrecto.Count; i++) restantes.Add(i);
        Mezclar(restantes);
        // Evitar que salga ya ordenado
        bool ordenado = true;
        for (int i = 0; i < restantes.Count; i++) if (restantes[i] != i) ordenado = false;
        if (ordenado && restantes.Count > 1) { int tmp = restantes[0]; restantes[0] = restantes[1]; restantes[1] = tmp; }

        txtPista = TextoEn(area, pista, 26, Util.Hex("#8ecae6"), new Vector2(0, 245), new Vector2(1200, 60), TextAnchor.MiddleCenter);
        TextoEn(area, tituloIzquierda, 26, Util.Hex("#9a9a9a"), new Vector2(-300, 190), new Vector2(560, 40), TextAnchor.MiddleLeft);
        TextoEn(area, tituloDerecha, 26, Util.Hex("#9a9a9a"), new Vector2(320, 190), new Vector2(560, 40), TextAnchor.MiddleLeft);
        txtIzq = TextoEn(area, "", 28, Color.white, new Vector2(-300, 20), new Vector2(560, 300), TextAnchor.UpperLeft);
        txtDer = TextoEn(area, "", 28, Util.Hex("#7bd389"), new Vector2(320, 20), new Vector2(560, 300), TextAnchor.UpperLeft);
        txtFeedback = TextoEn(area, "", 28, Util.Hex("#ffd166"), new Vector2(0, -190), new Vector2(1200, 50), TextAnchor.MiddleCenter);
        if (tiempoLimite > 0f)
            barraTiempo = new Barra(area, new Vector2(0, -235), new Vector2(900, 18), Util.Hex("#222230"), Util.Hex("#ffd166"));
        Dibujar();
    }

    string Elemento(int i)
    {
        string t = ordenCorrecto[i];
        if (coloresHex != null && i < coloresHex.Count) t = "<color=" + coloresHex[i] + ">■</color> " + t;
        return t;
    }

    void Dibujar()
    {
        string izq = "";
        if (sel >= restantes.Count) sel = Mathf.Max(0, restantes.Count - 1);
        for (int i = 0; i < restantes.Count; i++)
            izq += i == sel
                ? "<color=#ff4d4d><b>></b></color> <b>" + Elemento(restantes[i]) + "</b>\n\n"
                : "<color=#888888>" + (i + 1) + ")</color> " + Elemento(restantes[i]) + "\n\n";
        if (restantes.Count > 0) izq += "<size=22><color=#888888>↑ ↓ elegir  ·  E colocar</color></size>";
        txtIzq.text = izq;
        string der = "";
        for (int i = 0; i < colocados.Count; i++)
            der += (i + 1) + ". " + Elemento(colocados[i]) + "\n\n";
        txtDer.text = der;
        txtEstado.text = "Errores: " + errores + "/" + maxErrores;
    }

    protected override void Tick(float dt)
    {
        if (tiempoLimite > 0f)
        {
            barraTiempo.Valor(1f - tiempo / tiempoLimite);
            if (tiempo >= tiempoLimite) { Terminar(false, "¡SE ACABÓ EL TIEMPO!"); return; }
        }

        if (restantes.Count > 0)
        {
            if (Entrada.Bajada(Tecla.Arriba)) { sel = (sel + restantes.Count - 1) % restantes.Count; Sonido.Efecto("click", 0.4f, 1.1f); Dibujar(); }
            if (Entrada.Bajada(Tecla.Abajo)) { sel = (sel + 1) % restantes.Count; Sonido.Efecto("click", 0.4f, 1.1f); Dibujar(); }
        }
        int n = Entrada.NumeroBajado();
        if (n < 1 && Entrada.Avanzar()) n = sel + 1;
        if (n < 1 || n > restantes.Count) return;

        int elegido = restantes[n - 1];
        if (elegido == colocados.Count)
        {
            colocados.Add(elegido);
            restantes.RemoveAt(n - 1);
            txtFeedback.text = "";
            Dibujar();
            if (restantes.Count == 0) Terminar(true, textoExito);
        }
        else
        {
            errores++;
            Juego.I.ui.Golpe(0.25f);
            txtFeedback.text = "Ese no va ahí...";
            Dibujar();
            if (errores > maxErrores) Terminar(false, textoFallo);
        }
    }
}
