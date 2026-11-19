using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Sigilo: mantén ESPACIO para contener la respiración cuando el Pepino esté cerca.
// Si respiras cuando está al lado, o te quedas sin aire, te descubre.
public class MJRespiracion : Minijuego
{
    public float aireMaximo = 4f;
    public float recuperacion = 1.5f;     // aire por segundo al respirar
    public string textoExito = "LOS PASOS SE ALEJAN...";

    struct Fase
    {
        public int tipo;      // 0 lejos, 1 se acerca, 2 al lado
        public float dur;
        public Fase(int t, float d) { tipo = t; dur = d; }
    }

    readonly List<Fase> fases = new List<Fase>();
    int indice;
    float tFase, duracionTotal, transcurrido;
    float aire;
    Barra barraAire, barraTiempo;
    Text txtPeligro, txtValeria;

    protected override void Construir()
    {
        fases.Add(new Fase(0, 2.2f)); fases.Add(new Fase(1, 0.8f)); fases.Add(new Fase(2, 2.6f));
        fases.Add(new Fase(0, 1.8f)); fases.Add(new Fase(1, 0.7f)); fases.Add(new Fase(2, 3.0f));
        fases.Add(new Fase(0, 1.5f)); fases.Add(new Fase(1, 0.6f)); fases.Add(new Fase(2, 3.3f));
        fases.Add(new Fase(0, 1.0f));
        foreach (Fase f in fases) duracionTotal += f.dur;

        aire = aireMaximo;
        txtPeligro = TextoEn(area, "", 54, Color.white, new Vector2(0, 160), new Vector2(1200, 120), TextAnchor.MiddleCenter);
        txtValeria = TextoEn(area, "", 30, Util.Hex("#cfcfcf"), new Vector2(0, 60), new Vector2(1100, 60), TextAnchor.MiddleCenter);
        TextoEn(area, "AIRE", 26, Color.white, new Vector2(-520, -40), new Vector2(120, 40), TextAnchor.MiddleLeft);
        barraAire = new Barra(area, new Vector2(60, -40), new Vector2(900, 40), Util.Hex("#222230"), Util.Hex("#4cc9f0"));
        TextoEn(area, "AGUANTA", 26, Color.white, new Vector2(-520, -110), new Vector2(160, 40), TextAnchor.MiddleLeft);
        barraTiempo = new Barra(area, new Vector2(60, -110), new Vector2(900, 24), Util.Hex("#222230"), Util.Hex("#ffd166"));
    }

    protected override void Tick(float dt)
    {
        bool conteniendo = Entrada.Presionada(Tecla.Accion);
        aire += conteniendo ? -dt : dt * recuperacion;
        aire = Mathf.Clamp(aire, 0f, aireMaximo);
        transcurrido += dt;

        tFase += dt;
        if (tFase >= fases[indice].dur)
        {
            tFase = 0f;
            indice++;
            if (indice >= fases.Count) { Terminar(true, textoExito); return; }
        }

        int tipo = fases[indice].tipo;
        if (tipo == 0) { txtPeligro.text = "Pasos lejanos..."; txtPeligro.color = Util.Hex("#9a9a9a"); }
        else if (tipo == 1) { txtPeligro.text = "Se acerca..."; txtPeligro.color = Util.Hex("#ffd166"); }
        else { txtPeligro.text = "¡ESTÁ AL LADO!  CONTÉN (ESPACIO)"; txtPeligro.color = Util.Hex("#ff4d4d"); }

        txtValeria.text = conteniendo ? "(conteniendo la respiración...)" : "(respirando bajito)";
        barraAire.Valor(aire / aireMaximo);
        barraAire.Pintar(aire < aireMaximo * 0.25f ? Util.Hex("#ff4d4d") : Util.Hex("#4cc9f0"));
        barraTiempo.Valor(transcurrido / duracionTotal);
        txtEstado.text = "Mantén ESPACIO para no respirar. Suéltalo para recuperar aire.";

        if (tipo == 2 && !conteniendo) { Terminar(false, "TE ESCUCHÓ RESPIRAR"); return; }
        if (aire <= 0f) Terminar(false, "TE QUEDASTE SIN AIRE Y JADEASTE");
    }
}
