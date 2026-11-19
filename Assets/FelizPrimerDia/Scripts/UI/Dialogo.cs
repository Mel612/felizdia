using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Caja de diálogo con opciones. Mientras está abierta, el juego (y el reloj) se pausan.
public class Dialogo : MonoBehaviour
{
    class Paso
    {
        public string quien;
        public string texto;
        public string[] opciones;
        public Action<int> alElegir;
        public Action alTerminar;
    }

    RectTransform panel, pestana;
    Image retrato;
    Text txtNombre, txtCuerpo, txtPie;
    readonly Queue<Paso> cola = new Queue<Paso>();
    Paso actual;
    int frameMostrado;

    public bool Abierto { get { return actual != null || cola.Count > 0; } }

    public static Dialogo Crear(UIJuego ui)
    {
        Dialogo d = ui.gameObject.AddComponent<Dialogo>();
        // Hoja de papel clara y un poco transparente (deja ver el juego detrás)
        d.panel = Util.PanelArte(ui.transform, "Dialogo", new Color(0.97f, 0.94f, 0.87f, 0.9f), Util.Hex("#6b4a2f"),
                             new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(-640, 24), new Vector2(640, 254));
        // Retrato de quien habla (a la izquierda)
        RectTransform marco = Util.PanelArte(d.panel, "Retrato", new Color(0.86f, 0.8f, 0.7f, 0.9f), Util.Hex("#6b4a2f"),
                             new Vector2(0, 1), new Vector2(0, 1), new Vector2(22, -202), new Vector2(186, -30));
        d.retrato = Util.Imagen(marco, "Cara", null, Vector2.zero, Vector2.one, new Vector2(10, 10), new Vector2(-10, -10));
        d.retrato.preserveAspect = true;
        // Pestaña roja con el nombre (sobresale arriba)
        d.pestana = Util.PanelArte(d.panel, "Nombre", Util.Hex("#a4161a"), Util.Hex("#5c0b0e"),
                             new Vector2(0, 1), new Vector2(0, 1), new Vector2(24, -14), new Vector2(384, 42));
        d.txtNombre = Util.TextoUI(d.pestana, "", 28, Color.white, TextAnchor.MiddleLeft, Vector2.zero, Vector2.one, new Vector2(20, 0), new Vector2(-12, 0));
        d.txtNombre.fontStyle = FontStyle.Bold;
        d.txtCuerpo = Util.TextoUI(d.panel, "", 29, Util.Hex("#2b2118"), TextAnchor.UpperLeft, Vector2.zero, Vector2.one, new Vector2(206, 40), new Vector2(-34, -40));
        d.txtPie = Util.TextoUI(d.panel, "", 22, new Color(0.42f, 0.29f, 0.18f, 0.8f), TextAnchor.LowerRight, Vector2.zero, Vector2.one, new Vector2(30, 12), new Vector2(-24, -12));
        d.panel.gameObject.SetActive(false);
        return d;
    }

    // ---------- API ----------
    public void Decir(string quien, string[] lineas, Action alTerminar)
    {
        for (int i = 0; i < lineas.Length; i++)
        {
            Paso p = new Paso();
            p.quien = quien;
            p.texto = lineas[i];
            if (i == lineas.Length - 1) p.alTerminar = alTerminar;
            cola.Enqueue(p);
        }
        if (lineas.Length == 0 && alTerminar != null) alTerminar();
    }

    public void Decir(string quien, string[] lineas) { Decir(quien, lineas, null); }

    public void Elegir(string quien, string pregunta, string[] opciones, Action<int> alElegir)
    {
        Paso p = new Paso();
        p.quien = quien;
        p.texto = pregunta;
        p.opciones = opciones;
        p.alElegir = alElegir;
        cola.Enqueue(p);
    }

    // Versiones para usar dentro de corrutinas: yield return dialogo.DecirCo(...)
    public IEnumerator DecirCo(string quien, params string[] lineas)
    {
        bool listo = false;
        Decir(quien, lineas, delegate { listo = true; });
        while (!listo) yield return null;
    }

    public IEnumerator ElegirCo(string quien, string pregunta, string[] opciones, Resultado<int> resultado)
    {
        bool listo = false;
        Elegir(quien, pregunta, opciones, delegate (int i) { resultado.valor = i; listo = true; });
        while (!listo) yield return null;
    }

    // ---------- Funcionamiento ----------
    void Update()
    {
        if (actual == null)
        {
            if (cola.Count > 0) Mostrar(cola.Dequeue());
            return;
        }
        if (Time.frameCount <= frameMostrado + 1) return;

        if (actual.opciones != null)
        {
            int n = Entrada.NumeroBajado();
            if (n >= 1 && n <= actual.opciones.Length)
            {
                Paso p = actual;
                Sonido.Efecto("click", 0.5f, 1f);
                Cerrar();
                if (p.alElegir != null) p.alElegir(n - 1);
            }
        }
        else if (Entrada.Avanzar())
        {
            Paso p = actual;
            Cerrar();
            if (p.alTerminar != null) p.alTerminar();
        }
    }

    void Mostrar(Paso p)
    {
        actual = p;
        Sonido.Efecto("blip", 0.25f, UnityEngine.Random.Range(0.95f, 1.1f));
        frameMostrado = Time.frameCount;
        Juego.I.Bloquear(this);
        txtNombre.text = p.quien ?? "";
        pestana.gameObject.SetActive(!string.IsNullOrEmpty(p.quien));
        Sprite cara = Retrato(p.quien);
        retrato.transform.parent.gameObject.SetActive(cara != null);
        retrato.sprite = cara;
        txtCuerpo.rectTransform.offsetMin = new Vector2(cara != null ? 206 : 34, 40);
        string cuerpo = p.texto;
        if (p.opciones != null)
        {
            cuerpo += "\n";
            for (int i = 0; i < p.opciones.Length; i++)
                cuerpo += "\n<color=#a4161a><b>" + (i + 1) + ")</b></color> " + p.opciones[i];
            txtPie.text = "Elige con 1-" + p.opciones.Length;
        }
        else txtPie.text = "E / Espacio ►";
        txtCuerpo.text = cuerpo;
        panel.gameObject.SetActive(true);
        AjustarAlto();
        panel.SetAsLastSibling();
    }

    // La caja crece con el texto (por ejemplo cuando hay muchas opciones)
    void AjustarAlto()
    {
        const float minimo = 230f, maximo = 640f;
        float ancho = 1280f - txtCuerpo.rectTransform.offsetMin.x - 34f;
        TextGenerationSettings g = txtCuerpo.GetGenerationSettings(new Vector2(ancho, 0f));
        float alto = txtCuerpo.cachedTextGeneratorForLayout.GetPreferredHeight(txtCuerpo.text, g) / txtCuerpo.pixelsPerUnit;
        float total = Mathf.Clamp(alto + 40f + 48f, minimo, maximo);   // margen arriba + el pie "Elige con..."
        panel.offsetMax = new Vector2(panel.offsetMax.x, 24f + total);
    }

    // Cara de quien habla (o null si no tiene retrato)
    static Sprite Retrato(string quien)
    {
        if (string.IsNullOrEmpty(quien)) return null;
        string q = quien.ToLower();
        if (q.StartsWith("valeria")) return Arte.Valeria().Frente;
        if (q.StartsWith("mateo")) return Arte.Mateo().Frente;
        if (q.StartsWith("camila") || q.Contains("voz de camila")) return Arte.Camila().Frente;
        if (q.StartsWith("don teodoro")) return Arte.Teodoro().Frente;
        if (q.StartsWith("andrea")) return Arte.Andrea().Frente;
        if (q.StartsWith("diego")) return Arte.Diego().Frente;
        if (q.StartsWith("sebasti")) return Arte.Sebastian().Frente;
        if (q.StartsWith("ing. vargas")) return Arte.IngVargas().Frente;
        if (q.StartsWith("pepino")) return Arte.Pepino().Frente;
        if (q.Contains("ñatita")) return Arte.Natita();
        foreach (Exploracion.Sala ex in Exploracion.Salas)
        {
            string g = ex.guia.ToLower();
            int par = g.IndexOf('(');
            if (par > 0) g = g.Substring(0, par).Trim();
            if (g.Length > 0 && q.StartsWith(g)) return Arte.Personaje(ex.apariencia).Frente;
        }
        return null;
    }

    void Cerrar()
    {
        actual = null;
        if (cola.Count > 0) { Mostrar(cola.Dequeue()); return; }
        panel.gameObject.SetActive(false);
        Juego.I.Desbloquear(this);
    }
}

// Contenedor simple para devolver valores desde corrutinas.
public class Resultado<T>
{
    public T valor;
}
