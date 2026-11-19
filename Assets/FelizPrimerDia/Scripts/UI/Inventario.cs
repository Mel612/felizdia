using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// =====================================================================
//  INVENTARIO (estilo Resident Evil) — tecla I (en el celular: botón INV)
//  Muestra en casillas todo lo que llevas HOY: sellos, pruebas, ropa de las
//  misiones, objetos, la ofrenda de la ñatita, llaves, botellas y el celular.
//  Con las flechas eliges una casilla y a la derecha ves qué es y para qué sirve.
//  Mientras está abierto, el juego se pausa. Lo del inventario se pierde al morir
//  (lo que aprendiste queda en el cuaderno).
// =====================================================================
public class Inventario : MonoBehaviour
{
    class Item
    {
        public string nombre, descripcion, icono;
        public Color color;
        public int cantidad;
    }

    const int Columnas = 4, Filas = 4;

    Juego juego;
    RectTransform pantalla;
    readonly List<Image> casillas = new List<Image>();
    readonly List<Image> iconos = new List<Image>();
    readonly List<Text> cantidades = new List<Text>();
    Text txtNombre, txtDescripcion, txtTitulo;
    Image iconoGrande;
    List<Item> items = new List<Item>();
    int sel;
    bool abierto;
    float escalaAntes = 1f, t;

    public static bool HayUnoAbierto;

    public static Inventario Crear(Juego j)
    {
        if (j == null || j.ui == null) return null;
        GameObject go = new GameObject("Inventario (I)", typeof(RectTransform));
        go.transform.SetParent(j.ui.transform, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = rt.offsetMax = Vector2.zero;
        Inventario inv = go.AddComponent<Inventario>();
        inv.juego = j;
        inv.Armar();
        return inv;
    }

    void Armar()
    {
        Vector2 cero = Vector2.zero, uno = Vector2.one;
        pantalla = Util.Panel(transform, "Pantalla", new Color(0.02f, 0.03f, 0.05f, 0.96f), cero, uno, cero, cero);
        txtTitulo = Util.TextoUI(pantalla, "INVENTARIO", 60, Util.Hex("#e9f1ff"), TextAnchor.UpperLeft, new Vector2(0.05f, 0.86f), new Vector2(0.6f, 0.97f), cero, cero);
        txtTitulo.fontStyle = FontStyle.Bold;

        // Casillas (izquierda)
        RectTransform rejilla = Util.PanelArte(pantalla, "Casillas", new Color(0.05f, 0.07f, 0.1f, 0.95f), Util.Hex("#8ecae6"),
                                               new Vector2(0.05f, 0.12f), new Vector2(0.5f, 0.84f), cero, cero);
        for (int f = 0; f < Filas; f++)
            for (int c = 0; c < Columnas; c++)
            {
                Vector2 a = new Vector2(c / (float)Columnas, 1f - (f + 1) / (float)Filas), b = new Vector2((c + 1) / (float)Columnas, 1f - f / (float)Filas);
                RectTransform cas = Util.Panel(rejilla, "Casilla", new Color(0.12f, 0.16f, 0.22f, 1f), a, b, new Vector2(12, 12), new Vector2(-12, -12));
                casillas.Add(cas.GetComponent<Image>());
                RectTransform ico = Util.Panel(cas, "Ícono", Color.white, new Vector2(0.15f, 0.15f), new Vector2(0.85f, 0.85f), cero, cero);
                Image im = ico.GetComponent<Image>();
                im.preserveAspect = true;
                iconos.Add(im);
                Text n = Util.TextoUI(cas, "", 26, Color.white, TextAnchor.LowerRight, cero, uno, new Vector2(4, 4), new Vector2(-8, -4));
                n.fontStyle = FontStyle.Bold;
                n.gameObject.AddComponent<Outline>().effectColor = Color.black;
                cantidades.Add(n);
            }

        // Detalle (derecha)
        RectTransform lado = Util.PanelArte(pantalla, "Detalle", new Color(0.03f, 0.06f, 0.13f, 0.95f), Util.Hex("#ffd166"),
                                            new Vector2(0.53f, 0.12f), new Vector2(0.95f, 0.84f), cero, cero);
        RectTransform ig = Util.Panel(lado, "Ícono grande", Color.white, new Vector2(0.3f, 0.55f), new Vector2(0.7f, 0.95f), cero, cero);
        iconoGrande = ig.GetComponent<Image>();
        iconoGrande.preserveAspect = true;
        txtNombre = Util.TextoUI(lado, "", 40, Util.Hex("#ffd166"), TextAnchor.UpperCenter, new Vector2(0, 0.38f), new Vector2(1, 0.53f), new Vector2(20, 0), new Vector2(-20, 0));
        txtNombre.fontStyle = FontStyle.Bold;
        txtDescripcion = Util.TextoUI(lado, "", 28, Color.white, TextAnchor.UpperCenter, new Vector2(0, 0.02f), new Vector2(1, 0.38f), new Vector2(30, 0), new Vector2(-30, 0));

        Util.TextoUI(pantalla, "<color=#8ecae6>Flechas</color>  elegir        <color=#8ecae6>I / Esc</color>  cerrar        " +
                               "<color=#ff8080>Lo que llevas se pierde si mueres.</color>",
                     26, new Color(1, 1, 1, 0.8f), TextAnchor.MiddleLeft, new Vector2(0.05f, 0.02f), new Vector2(0.97f, 0.1f), cero, cero);
        pantalla.gameObject.SetActive(false);
    }

    // ---------- Qué llevas hoy ----------
    static void Agregar(List<Item> l, string nombre, string desc, string icono, string color, int cantidad = 1)
    {
        l.Add(new Item { nombre = nombre, descripcion = desc, icono = icono, color = Util.Hex(color), cantidad = cantidad });
    }

    static List<Item> Contenido()
    {
        List<Item> l = new List<Item>();
        Agregar(l, "Celular", "Batería: " + Mathf.FloorToInt(Partida.Bateria) + "%.\nC: ver mensajes del grupo.  X: flash para cegar al Pepino (gasta 25%).", "celular", "#4cc9f0");
        if (Partida.Lanzables > 0)
            Agregar(l, "Botella vacía", "Lánzala con F hacia donde miras. Al romperse, el Pepino va a ver el ruido.", "botella", "#3f8f5a", Partida.Lanzables);

        // Sellos
        string[] sellos = { Ids.SelloMedicina, Ids.SelloDerecho, Ids.SelloPeriodismo, Ids.SelloSistemas };
        string[] carreras = { "Medicina", "Derecho", "Periodismo", "Ing. de Sistemas" };
        string[] colores = { "#2a9d8f", "#c1121f", "#1d4ed8", "#7b2cbf" };
        for (int i = 0; i < 4; i++)
            if (Memoria.Tiene(sellos[i]))
                Agregar(l, "Sello de " + carreras[i], "Un sello en tu Pasaporte de Carreras. Con los 4 entras a la Bienvenida (18:00).", "sello", colores[i]);

        // Pruebas
        if (Memoria.Tiene(Ids.PruebaHistoria)) Agregar(l, "Historia clínica", "De la Bienvenida del año pasado. El nombre de la paciente está tachado con marcador.", "papel", "#f1faee");
        if (Memoria.Tiene(Ids.PruebaExpediente)) Agregar(l, "Expediente", "Entre los juicios simulados, uno no es simulado: es una denuncia REAL.", "carpeta", "#e9c46a");
        if (Memoria.Tiene(Ids.PruebaGrabacion)) Agregar(l, "Grabación", "La grabación del set de Periodismo. Lo que pasó esa noche.", "cinta", "#adb5bd");
        if (Memoria.Tiene(Ids.PruebaRegistros)) Agregar(l, "Registros de \"c.rojas\"", "El registro de cambios de la lista de la Bienvenida, con el usuario que los hizo.", "usb", "#c77dff");

        // Ropa y objetos de las misiones
        foreach (Misiones.Mision m in Misiones.Lista)
        {
            if (Memoria.Tiene(Misiones.Prenda(m)))
                Agregar(l, m.prenda, "Te deja entrar al laboratorio de " + m.facultad + ".", "ropa", "#ffd166");
            else if (Memoria.Tiene(Misiones.Objeto(m)))
                Agregar(l, Mayuscula(m.objeto), "Llévaselo a " + m.guiaNombre + " (" + Misiones.NombreSala(m.guiaSala) + ").", IconoObjeto(m.id), "#f4a261");
        }

        // La ofrenda de la ñatita
        if (Memoria.Tiene("ofrenda_flores")) Agregar(l, "Flores", "Para la ñatita de Odontología.", "flor", "#ff70a6");
        if (Memoria.Tiene("ofrenda_coca")) Agregar(l, "Hojas de coca", "Para la ñatita. Don Teodoro te las dio.", "hoja", "#52b788");
        if (Memoria.Tiene("ofrenda_vela")) Agregar(l, "Vela", "Para la ñatita. Hay que encenderla frente a ella.", "vela", "#ffe8a3");

        // Llaves y accesos
        if (Memoria.Tiene("llave_cuarto")) Agregar(l, "Llave vieja", "Don Teodoro dice que abre una puerta en Seguridad que nadie usa.", "llave", "#c9a227");
        if (Memoria.Tiene("llave_cabina")) Agregar(l, "Llave de la cabina", "Abre la cabina de luces del auditorio.", "llave", "#c9a227");
        if (Memoria.Tiene("tarjeta")) Agregar(l, "Tarjeta del ascensor", "Te deja usar el ascensor (cuando no está en mantenimiento).", "tarjeta", "#8ecae6");
        if (Memoria.Tiene("clave_vargas")) Agregar(l, "Clave del Ing. Vargas", "Abre las computadoras con los videos de las cámaras.", "papel", "#c77dff");
        for (int i = 0; i < Historia.ArchivosMateo.Length; i++)
            if (Memoria.Tiene(Historia.ArchivosMateo[i])) Agregar(l, "Video " + (i + 1), "Recuperado de " + Historia.LugarArchivo[i] + ".", "usb", "#4cc9f0");
        return l;
    }

    static string Mayuscula(string s) { return string.IsNullOrEmpty(s) ? s : char.ToUpper(s[0]) + s.Substring(1); }

    static string IconoObjeto(string mision)
    {
        switch (mision) { case "toga": return "libro"; case "prensa": return "taza"; case "bata": return "libro"; default: return "usb"; }
    }

    // ---------- Abrir / cerrar ----------
    void Abrir()
    {
        abierto = true;
        HayUnoAbierto = true;
        escalaAntes = Time.timeScale > 0f ? Time.timeScale : 1f;
        Time.timeScale = 0f;
        juego.Bloquear(this);
        items = Contenido();
        sel = 0;
        pantalla.gameObject.SetActive(true);
        Sonido.Efecto("puerta", 0.4f, 1.6f);
        txtTitulo.text = "INVENTARIO  <size=28><color=#8ecae6>" + Protagonista.Nombre + "  ·  Día " + Memoria.Dia + "  ·  " + juego.HoraTexto + "</color></size>";
        Dibujar();
    }

    void Cerrar()
    {
        abierto = false;
        HayUnoAbierto = false;
        Time.timeScale = escalaAntes;
        juego.Desbloquear(this);
        pantalla.gameObject.SetActive(false);
        Sonido.Efecto("click", 0.5f, 0.9f);
    }

    void OnDestroy() { if (abierto) { Time.timeScale = escalaAntes; HayUnoAbierto = false; } }

    void Dibujar()
    {
        for (int i = 0; i < casillas.Count; i++)
        {
            bool hay = i < items.Count;
            iconos[i].enabled = hay;
            cantidades[i].text = hay && items[i].cantidad > 1 ? "x" + items[i].cantidad : "";
            if (hay) { iconos[i].sprite = Icono(items[i].icono); iconos[i].color = items[i].color; }
        }
        if (sel < items.Count)
        {
            Item it = items[sel];
            iconoGrande.enabled = true;
            iconoGrande.sprite = Icono(it.icono);
            iconoGrande.color = it.color;
            txtNombre.text = it.nombre.ToUpper() + (it.cantidad > 1 ? "  x" + it.cantidad : "");
            txtDescripcion.text = it.descripcion;
        }
        else
        {
            iconoGrande.enabled = false;
            txtNombre.text = "";
            txtDescripcion.text = "<color=#777777>Casilla vacía.</color>";
        }
    }

    void Update()
    {
        if (juego == null || juego.ui == null) return;
        if (!abierto)
        {
            if (Entrada.Bajada(Tecla.Inventario) && !juego.Bloqueado && !juego.Pausado && !juego.ui.PantallaVisible && !Cine.Activa && !MiniMapa.HayUnoAbierto)
                Abrir();
            return;
        }
        transform.SetAsLastSibling();
        if (Entrada.Bajada(Tecla.Inventario) || Entrada.Bajada(Tecla.Saltar)) { Cerrar(); return; }
        int antes = sel;
        if (Entrada.Bajada(Tecla.Derecha)) sel = (sel + 1) % casillas.Count;
        if (Entrada.Bajada(Tecla.Izquierda)) sel = (sel + casillas.Count - 1) % casillas.Count;
        if (Entrada.Bajada(Tecla.Abajo)) sel = (sel + Columnas) % casillas.Count;
        if (Entrada.Bajada(Tecla.Arriba)) sel = (sel + casillas.Count - Columnas) % casillas.Count;
        if (sel != antes) { Sonido.Efecto("click", 0.4f, 1.2f); Dibujar(); }

        // La casilla elegida late
        t += Time.unscaledDeltaTime;
        float p = (Mathf.Sin(t * 6f) + 1f) * 0.5f;
        for (int i = 0; i < casillas.Count; i++)
            casillas[i].color = i == sel ? Color.Lerp(Util.Hex("#3a5a80"), Util.Hex("#ffd166"), p * 0.6f) : new Color(0.12f, 0.16f, 0.22f, 1f);
    }

    // ---------- Íconos dibujados por código (blancos: se tiñen con el color del objeto) ----------
    static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();

    static Sprite Icono(string tipo)
    {
        Sprite s;
        if (cache.TryGetValue(tipo, out s) && s != null) return s;
        Lienzo L = new Lienzo(24, 24);
        Color w = Color.white, g = new Color(0.65f, 0.65f, 0.65f, 1f), o = new Color(0.25f, 0.25f, 0.25f, 1f);
        switch (tipo)
        {
            case "celular": L.R(7, 2, 16, 21, w); L.R(8, 5, 15, 18, o); L.R(11, 3, 12, 3, g); L.R(9, 15, 14, 16, g); break;
            case "botella": L.R(9, 2, 14, 14, w); L.R(10, 15, 13, 18, w); L.R(10, 19, 13, 21, g); L.R(9, 7, 14, 10, g); break;
            case "sello": L.Circulo(11.5f, 11.5f, 10f, w); L.Circulo(11.5f, 11.5f, 7.5f, o); L.Circulo(11.5f, 11.5f, 5.5f, w); L.R(10, 10, 13, 13, o); break;
            case "papel": L.R(5, 2, 18, 21, w); for (int y = 5; y < 19; y += 3) L.R(7, y, 16, y, g); L.R(14, 18, 18, 21, g); break;
            case "carpeta": L.R(3, 4, 20, 18, w); L.R(3, 18, 10, 20, w); L.R(4, 7, 19, 8, g); break;
            case "cinta": L.R(2, 6, 21, 17, w); L.Circulo(7.5f, 11.5f, 3f, o); L.Circulo(15.5f, 11.5f, 3f, o); L.R(7, 7, 16, 8, g); break;
            case "usb": L.R(8, 2, 15, 15, w); L.R(9, 16, 14, 21, g); L.R(10, 18, 10, 19, o); L.R(13, 18, 13, 19, o); break;
            case "ropa": L.R(6, 2, 17, 15, w); L.R(2, 11, 6, 17, w); L.R(17, 11, 21, 17, w); L.R(9, 15, 14, 18, w); L.R(10, 16, 13, 17, o); break;
            case "libro": L.R(4, 3, 19, 20, w); L.R(4, 3, 6, 20, g); L.R(9, 13, 16, 15, o); break;
            case "taza": L.R(5, 3, 15, 15, w); L.R(16, 7, 19, 12, w); L.R(17, 8, 18, 11, o); L.R(7, 17, 9, 20, g); L.R(11, 18, 13, 21, g); break;
            case "flor": L.R(11, 2, 12, 12, g); L.Circulo(11.5f, 16f, 3f, w); L.Circulo(7.5f, 14f, 3f, w); L.Circulo(15.5f, 14f, 3f, w); L.Circulo(9f, 19f, 2.6f, w); L.Circulo(14f, 19f, 2.6f, w); L.Circulo(11.5f, 16.5f, 1.8f, o); break;
            case "hoja": L.Elipse(11.5f, 11.5f, 6f, 10f, w); L.R(11, 3, 12, 20, g); break;
            case "vela": L.R(8, 2, 15, 15, w); L.R(11, 16, 12, 17, o); L.Elipse(11.5f, 19.5f, 2f, 3f, g); break;
            case "llave": L.Circulo(7f, 15f, 5f, w); L.Circulo(7f, 15f, 2f, o); L.R(11, 14, 21, 16, w); L.R(17, 10, 18, 14, w); L.R(20, 11, 21, 14, w); break;
            case "tarjeta": L.R(2, 6, 21, 17, w); L.R(2, 13, 21, 14, o); L.R(4, 8, 9, 10, g); break;
            default: L.R(4, 4, 19, 19, w); break;
        }
        s = Arte.Crear("icono_inv_" + tipo, L, Arte.PivotCentro);
        cache[tipo] = s;
        return s;
    }
}
