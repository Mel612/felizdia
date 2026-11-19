using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Base de todos los minijuegos. Abren un panel, pausan el juego y devuelven éxito o fallo.
//
// Uso típico dentro de una corrutina:
//   MJRitmo mj = Minijuego.Crear<MJRitmo>("RCP al ritmo", "Presiona ESPACIO...");
//   mj.objetivo = 8;
//   Resultado<bool> r = new Resultado<bool>();
//   yield return mj.Jugar(r);
//   if (r.valor) { ... }
public abstract class Minijuego : MonoBehaviour
{
    public string titulo = "";
    public string instrucciones = "";
    public Color colorFondo = new Color(0.05f, 0.05f, 0.08f, 0.97f);
    public Color colorBorde = new Color(1f, 0.82f, 0.4f);

    protected RectTransform panel;
    protected RectTransform area;      // zona central para dibujar el minijuego
    protected Text txtTitulo, txtInstrucciones, txtEstado, txtResultado;
    protected bool activo;
    public static Minijuego Actual;          // el que está en pantalla (los controles táctiles muestran 1-4)
    public bool UsaNumeros { get { return this is MJPreguntas || this is MJOrdenar; } }
    protected float tiempo;

    Action<bool> alTerminar;
    int frameInicio;

    public static T Crear<T>(string titulo, string instrucciones) where T : Minijuego
    {
        GameObject go = new GameObject("Minijuego: " + titulo);
        go.transform.SetParent(Juego.I.ui.transform, false);
        T mj = go.AddComponent<T>();
        mj.titulo = titulo;
        mj.instrucciones = instrucciones;
        return mj;
    }

    public void Comenzar(Action<bool> fin)
    {
        alTerminar = fin;
        Dificultad.Ajustar(this);
        Juego.I.Bloquear(this);
        ConstruirPanel();
        Construir();
        activo = true;
        Actual = this;
        frameInicio = Time.frameCount;
    }

    public IEnumerator Jugar(Resultado<bool> resultado)
    {
        bool listo = false;
        Comenzar(delegate (bool ok) { resultado.valor = ok; listo = true; });
        while (!listo) yield return null;
    }

    void ConstruirPanel()
    {
        Transform ui = Juego.I.ui.transform;
        panel = Util.PanelArteCentro(ui, "Panel " + titulo, colorFondo, colorBorde, Vector2.zero, new Vector2(1320, 780));
        txtTitulo = Util.TextoUI(panel, titulo, 48, colorBorde, TextAnchor.UpperCenter, Vector2.zero, Vector2.one, new Vector2(30, 0), new Vector2(-30, -24));
        txtTitulo.fontStyle = FontStyle.Bold;
        txtInstrucciones = Util.TextoUI(panel, instrucciones, 26, Util.Hex("#cfcfcf"), TextAnchor.UpperCenter, Vector2.zero, Vector2.one, new Vector2(60, 0), new Vector2(-60, -90));
        txtEstado = Util.TextoUI(panel, "", 30, Color.white, TextAnchor.LowerCenter, Vector2.zero, Vector2.one, new Vector2(30, 28), new Vector2(-30, 0));

        GameObject goArea = new GameObject("Area");
        goArea.transform.SetParent(panel, false);
        area = goArea.AddComponent<RectTransform>();
        area.anchorMin = Vector2.zero;
        area.anchorMax = Vector2.one;
        area.offsetMin = new Vector2(40, 90);
        area.offsetMax = new Vector2(-40, -180);

        txtResultado = Util.TextoUI(panel, "", 64, Color.white, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        txtResultado.fontStyle = FontStyle.Bold;
        txtResultado.gameObject.AddComponent<Outline>().effectColor = Color.black;
        panel.SetAsLastSibling();
    }

    protected abstract void Construir();
    protected abstract void Tick(float dt);

    void Update()
    {
        if (!activo) return;
        if (Time.frameCount <= frameInicio + 1) return;
        tiempo += Time.deltaTime;
        Tick(Time.deltaTime);
    }

    protected void Terminar(bool exito, string mensaje)
    {
        if (!activo) return;
        activo = false;
        if (Actual == this) Actual = null;
        StartCoroutine(Fin(exito, mensaje));
    }

    IEnumerator Fin(bool exito, string mensaje)
    {
        txtResultado.text = mensaje;
        txtResultado.color = exito ? Util.Hex("#7bd389") : Util.Hex("#ff4d4d");
        if (exito) { Juego.I.ui.Destello(new Color(0.2f, 0.8f, 0.3f), 0.25f); Sonido.Efecto("logro", 0.5f, 1.2f); }
        else { Juego.I.ui.Golpe(0.5f); Sonido.Efecto("golpe", 0.7f, 1f); }
        yield return new WaitForSeconds(1.4f);
        if (panel != null) Destroy(panel.gameObject);
        Juego.I.Desbloquear(this);
        Action<bool> f = alTerminar;
        Destroy(gameObject);
        if (f != null) f(exito);
    }

    // ---------- Ayudas para dibujar ----------
    protected RectTransform Caja(Transform padre, Vector2 pos, Vector2 tam, Color c)
    {
        return Util.PanelCentro(padre, "Caja", c, pos, tam);
    }

    protected Text TextoEn(Transform padre, string texto, int tam, Color c, Vector2 pos, Vector2 tamCaja, TextAnchor alineacion)
    {
        GameObject go = new GameObject("Texto");
        go.transform.SetParent(padre, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = tamCaja;
        rt.anchoredPosition = pos;
        Text t = go.AddComponent<Text>();
        t.font = Util.Fuente;
        t.text = texto;
        t.fontSize = tam;
        t.color = c;
        t.alignment = alineacion;
        t.supportRichText = true;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;
        return t;
    }

    // Barra que se llena de izquierda a derecha (valor 0..1).
    protected class Barra
    {
        public RectTransform fondo, relleno;
        float ancho;

        public Barra(Transform padre, Vector2 pos, Vector2 tam, Color cFondo, Color cRelleno)
        {
            ancho = tam.x;
            fondo = Util.PanelCentro(padre, "Barra", cFondo, pos, tam);
            relleno = Util.Panel(fondo, "Relleno", cRelleno, new Vector2(0, 0), new Vector2(0, 1), Vector2.zero, new Vector2(tam.x, 0));
        }

        public void Valor(float v)
        {
            relleno.offsetMax = new Vector2(ancho * Mathf.Clamp01(v), 0);
        }

        public void Pintar(Color c) { relleno.GetComponent<Image>().color = c; }
    }

    protected static void Mezclar<T>(System.Collections.Generic.IList<T> lista)
    {
        for (int i = lista.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            T tmp = lista[i]; lista[i] = lista[j]; lista[j] = tmp;
        }
    }
}
