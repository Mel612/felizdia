using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Presiona la flecha que aparece antes de que se acabe el tiempo.
// Sirve para: teleprompter (Periodismo), esconderse bajo el estrado (Derecho),
// el reflector "¡MIRA ARRIBA!" (1 sola flecha) y la persecución final.
public class MJSecuencia : Minijuego
{
    public int largo = 6;
    public float ventana = 1.2f;          // segundos por flecha
    public int maxErrores = 1;
    public string[] palabras;             // opcional: una palabra por flecha (teleprompter)
    public string textoExito = "¡LO LOGRASTE!";
    public string textoFallo = "MUY LENTA...";
    public string textoArriba = "";       // texto grande arriba (ej. "MIRA ARRIBA")

    readonly List<int> secuencia = new List<int>();
    int indice, errores;
    float t;
    Text txtFlecha, txtSiguientes, txtPalabras, txtArriba;
    Barra barraTiempo;

    protected override void Construir()
    {
        if (palabras != null && palabras.Length > 0) largo = palabras.Length;
        for (int i = 0; i < largo; i++) secuencia.Add(Random.Range(0, 4)); // Arriba, Abajo, Izquierda, Derecha

        txtArriba = TextoEn(area, textoArriba, 46, Util.Hex("#ff4d4d"), new Vector2(0, 230), new Vector2(1200, 80), TextAnchor.MiddleCenter);
        txtPalabras = TextoEn(area, "", 34, Util.Hex("#9a9a9a"), new Vector2(0, 150), new Vector2(1200, 100), TextAnchor.MiddleCenter);
        txtFlecha = TextoEn(area, "", 170, Color.white, new Vector2(0, -10), new Vector2(300, 220), TextAnchor.MiddleCenter);
        txtSiguientes = TextoEn(area, "", 50, new Color(1, 1, 1, 0.3f), new Vector2(330, -10), new Vector2(300, 100), TextAnchor.MiddleLeft);
        barraTiempo = new Barra(area, new Vector2(0, -150), new Vector2(700, 22), Util.Hex("#222230"), Util.Hex("#ffd166"));
        Mostrar();
    }

    void Mostrar()
    {
        txtFlecha.text = Entrada.Simbolo(secuencia[indice]);
        string sig = "";
        for (int i = indice + 1; i < Mathf.Min(indice + 4, secuencia.Count); i++) sig += Entrada.Simbolo(secuencia[i]) + " ";
        txtSiguientes.text = sig;

        if (palabras != null && palabras.Length > 0)
        {
            string s = "";
            for (int i = 0; i < palabras.Length; i++)
            {
                if (i < indice) s += "<color=#ffffff>" + palabras[i] + "</color> ";
                else if (i == indice) s += "<color=#ffd166><b>" + palabras[i] + "</b></color> ";
                else s += palabras[i] + " ";
            }
            txtPalabras.text = s;
        }
        txtEstado.text = (indice + 1) + "/" + largo + "     Errores: " + errores + "/" + maxErrores;
        t = 0f;
    }

    protected override void Tick(float dt)
    {
        t += dt;
        barraTiempo.Valor(1f - t / ventana);
        if (t >= ventana) { Error(); return; }

        int d = Entrada.DireccionBajada();
        if (d < 0) return;
        if (d == secuencia[indice])
        {
            indice++;
            if (indice >= secuencia.Count) { Terminar(true, textoExito); return; }
            Mostrar();
        }
        else Error();
    }

    void Error()
    {
        errores++;
        Juego.I.ui.Golpe(0.25f);
        if (errores > maxErrores) { Terminar(false, textoFallo); return; }
        secuencia[indice] = Random.Range(0, 4);
        Mostrar();
    }
}
