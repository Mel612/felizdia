using UnityEngine;
using UnityEngine.UI;

// Presiona ESPACIO cuando el marcador pase por la zona verde.
// Medicina: "RCP al ritmo".
public class MJRitmo : Minijuego
{
    public int objetivo = 8;
    public int maxFallos = 3;
    public float velocidad = 0.75f;       // recorridos de barra por segundo
    public float anchoZona = 0.16f;       // 0..1
    public string nombreContador = "Compresiones";
    public string textoExito = "¡PULSO RECUPERADO!";
    public string textoFallo = "LÍNEA PLANA...";

    const float AnchoBarra = 900f;
    RectTransform marcador;
    Text txtMonitor;
    float pos;
    int dir = 1;
    int aciertos, fallos;
    float latido;

    protected override void Construir()
    {
        txtMonitor = TextoEn(area, "♥", 120, Util.Hex("#ff4d6d"), new Vector2(0, 150), new Vector2(600, 160), TextAnchor.MiddleCenter);
        RectTransform barra = Caja(area, new Vector2(0, -40), new Vector2(AnchoBarra, 60), Util.Hex("#2a2a35"));
        Caja(barra, Vector2.zero, new Vector2(AnchoBarra * anchoZona, 60), Util.Hex("#2f9e44"));
        marcador = Caja(barra, Vector2.zero, new Vector2(14, 100), Color.white);
        Actualizar();
    }

    protected override void Tick(float dt)
    {
        pos += dir * velocidad * dt;
        if (pos > 1f) { pos = 1f; dir = -1; }
        if (pos < 0f) { pos = 0f; dir = 1; }
        marcador.anchoredPosition = new Vector2((pos - 0.5f) * AnchoBarra, 0f);

        latido = Mathf.MoveTowards(latido, 0f, dt * 3f);
        txtMonitor.transform.localScale = Vector3.one * (1f + latido * 0.35f);

        if (Entrada.Bajada(Tecla.Accion))
        {
            if (Mathf.Abs(pos - 0.5f) <= anchoZona / 2f)
            {
                aciertos++;
                latido = 1f;
                velocidad *= 1.07f;
            }
            else
            {
                fallos++;
                Juego.I.ui.Golpe(0.2f);
            }
            Actualizar();
            if (aciertos >= objetivo) Terminar(true, textoExito);
            else if (fallos >= maxFallos) Terminar(false, textoFallo);
        }
    }

    void Actualizar()
    {
        txtEstado.text = nombreContador + ": " + aciertos + "/" + objetivo + "     Fallos: " + fallos + "/" + maxFallos;
    }
}
