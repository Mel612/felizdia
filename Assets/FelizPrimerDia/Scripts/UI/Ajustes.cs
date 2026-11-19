using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// =====================================================================
//  OPCIONES: volumen de la música, volumen de los efectos y brillo.
//  Se guardan en la computadora (PlayerPrefs) y se aplican al instante.
//  Se abren desde el menú de inicio (OPCIONES) o en el juego con P (pausa).
// =====================================================================
public static class Ajustes
{
    public static float Musica = 0.5f;
    public static float Efectos = 0.9f;
    public static float Brillo = 1f;          // 0.5 (oscuro) .. 1.5 (claro)
    public static int Camara = 0;             // 0 cerca · 1 normal · 2 lejos
    public static readonly string[] NombresCamara = { "CERCA", "NORMAL", "LEJOS" };
    public static float ZoomJuego { get { return new[] { 4.3f, 5.2f, 6.2f }[Mathf.Clamp(Camara, 0, 2)]; } }
    static bool cargado;

    public static void Cargar()
    {
        if (cargado) return;
        cargado = true;
        try
        {
            Musica = PlayerPrefs.GetFloat("fpd_vol_musica", Musica);
            Efectos = PlayerPrefs.GetFloat("fpd_vol_efectos", Efectos);
            Brillo = PlayerPrefs.GetFloat("fpd_brillo", Brillo);
            Camara = PlayerPrefs.GetInt("fpd_camara", Camara);
        }
        catch { }
        Aplicar();
    }

    public static void Guardar()
    {
        try
        {
            PlayerPrefs.SetFloat("fpd_vol_musica", Musica);
            PlayerPrefs.SetFloat("fpd_vol_efectos", Efectos);
            PlayerPrefs.SetFloat("fpd_brillo", Brillo);
            PlayerPrefs.SetInt("fpd_camara", Camara);
            PlayerPrefs.Save();
        }
        catch { }
    }

    public static void Aplicar()
    {
        Sonido.VolumenMusica = Musica;
        Sonido.VolumenEfectos = Efectos;
        if (Juego.I != null && Juego.I.ui != null) Juego.I.ui.Brillo(Brillo);
        Juego j = Juego.I;
        if (j != null && j.camara != null && !Cine.Activa && j.camaraSeguir == null &&
            j.tipoEscena == TipoEscena.Piso)
            j.camara.orthographicSize = j.zoomCamara;
    }

    public static string Barra(float v, float min, float max)
    {
        int n = Mathf.RoundToInt(Mathf.InverseLerp(min, max, v) * 10f);
        return "<color=#ffd166>" + new string('|', n * 2) + "</color><color=#555555>" + new string('|', (10 - n) * 2) + "</color>  " + (n * 10) + "%";
    }
}

// Panel para cambiar las opciones (flechas arriba/abajo eligen, izquierda/derecha cambian, E o Esc cierra)
public class PanelOpciones : MonoBehaviour
{
    Text txt;
    int sel;
    bool listo;

    public static IEnumerator Abrir(Transform padre)
    {
        Ajustes.Cargar();
        PanelOpciones p = Crear(padre);
        GameObject go = p.gameObject;
        yield return null;
        while (!p.listo) yield return null;
        Ajustes.Guardar();
        Destroy(go);
    }

    // Arma el panel (también lo usa la escena 10_Pantallas para dibujarlo)
    public static PanelOpciones Crear(Transform padre)
    {
        GameObject go = new GameObject("Opciones", typeof(RectTransform));
        go.transform.SetParent(padre, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = rt.offsetMax = Vector2.zero;
        PanelOpciones p = go.AddComponent<PanelOpciones>();
        RectTransform panel = Util.PanelArteCentro(go.transform, "Panel", new Color(0.04f, 0.03f, 0.07f, 0.95f), Util.Hex("#ffd166"), new Vector2(0, 0), new Vector2(900, 680));
        p.txt = Util.TextoRelleno(panel, "", 32, Color.white, TextAnchor.UpperLeft, 40);
        p.Dibujar();
        return p;
    }

    void Dibujar()
    {
        string[] nombres = { "Música", "Efectos", "Brillo", "Cámara" };
        string cam = "";
        for (int i = 0; i < 3; i++) cam += (i == Ajustes.Camara ? "<color=#ffd166>[" + Ajustes.NombresCamara[i] + "]</color>" : "<color=#777777> " + Ajustes.NombresCamara[i] + " </color>") + "  ";
        string[] barras = { Ajustes.Barra(Ajustes.Musica, 0f, 1f), Ajustes.Barra(Ajustes.Efectos, 0f, 1f), Ajustes.Barra(Ajustes.Brillo, 0.5f, 1.5f), cam };
        System.Text.StringBuilder sb = new System.Text.StringBuilder("<b>OPCIONES</b>\n\n");
        for (int i = 0; i < 4; i++)
            sb.Append(i == sel ? "<color=#ff4d4d>> " : "   ").Append(nombres[i]).Append(i == sel ? "</color>" : "")
              .Append("\n      ").Append(barras[i]).Append("\n\n");
        sb.Append(sel == 4 ? "<color=#ff4d4d>> Volver</color>" : "   Volver");
        sb.Append("\n\n<size=22><color=#888888>Arriba/abajo: elegir · Izquierda/derecha: cambiar · E: aceptar</color></size>");
        txt.text = sb.ToString();
    }

    void Update()
    {
        transform.SetAsLastSibling();
        if (Entrada.Bajada(Tecla.Arriba)) { sel = (sel + 4) % 5; Sonido.Efecto("click", 0.5f, 1f); Dibujar(); }
        if (Entrada.Bajada(Tecla.Abajo)) { sel = (sel + 1) % 5; Sonido.Efecto("click", 0.5f, 1f); Dibujar(); }
        int dir = Entrada.Bajada(Tecla.Derecha) ? 1 : Entrada.Bajada(Tecla.Izquierda) ? -1 : 0;
        if (dir != 0 && sel < 4)
        {
            if (sel == 0) Ajustes.Musica = Mathf.Clamp01(Mathf.Round((Ajustes.Musica + dir * 0.1f) * 10f) / 10f);
            if (sel == 1) Ajustes.Efectos = Mathf.Clamp01(Mathf.Round((Ajustes.Efectos + dir * 0.1f) * 10f) / 10f);
            if (sel == 2) Ajustes.Brillo = Mathf.Clamp(Mathf.Round((Ajustes.Brillo + dir * 0.1f) * 10f) / 10f, 0.5f, 1.5f);
            if (sel == 3) Ajustes.Camara = Mathf.Clamp(Ajustes.Camara + dir, 0, 2);
            Ajustes.Aplicar();
            Sonido.Efecto("blip", 0.6f, 1f);
            Dibujar();
        }
        if (Entrada.Bajada(Tecla.Saltar) || (sel == 4 && Entrada.Avanzar())) listo = true;
    }
}

// Menú de pausa (tecla P):
//   0 Continuar · 1 Guardar partida · 2 Opciones · 3 Guardar y volver al menú · 4 Volver al menú sin guardar
public class PanelPausa : MonoBehaviour
{
    static readonly string[] Ops = { "CONTINUAR", "GUARDAR PARTIDA", "OPCIONES", "GUARDAR Y VOLVER AL MENÚ", "VOLVER AL MENÚ SIN GUARDAR", "SALIR DEL JUEGO" };
    Text txt;
    int sel;
    bool listo, puedeGuardar;
    string aviso = "";

    public static IEnumerator Abrir(Transform padre, Resultado<int> r, bool puedeGuardar, string aviso)
    {
        PanelPausa p = Crear(padre, puedeGuardar, aviso);
        GameObject go = p.gameObject;
        Sonido.Efecto("click", 0.6f, 0.8f);
        yield return null;
        while (!p.listo) yield return null;
        r.valor = p.sel;
        Destroy(go);
    }

    public static PanelPausa Crear(Transform padre, bool puedeGuardar, string aviso)
    {
        GameObject go = new GameObject("Pausa", typeof(RectTransform));
        go.transform.SetParent(padre, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = rt.offsetMax = Vector2.zero;
        Util.Panel(go.transform, "Oscuro", new Color(0f, 0f, 0f, 0.6f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        PanelPausa p = go.AddComponent<PanelPausa>();
        p.puedeGuardar = puedeGuardar;
        p.aviso = aviso ?? "";
        RectTransform panel = Util.PanelArteCentro(go.transform, "Panel", new Color(0.04f, 0.03f, 0.07f, 0.95f), Util.Hex("#e63946"), Vector2.zero, new Vector2(900, 740));
        p.txt = Util.TextoRelleno(panel, "", 34, Color.white, TextAnchor.UpperCenter, 36);
        p.Dibujar();
        return p;
    }

    bool Habilitada(int i) { return puedeGuardar || (i != 1 && i != 3); }

    void Dibujar()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder("<b><color=#e63946>PAUSA</color></b>\n");
        sb.Append("<size=22><color=#999999>").Append(Protagonista.Nombre).Append("  ·  Día ").Append(Memoria.Dia);
        if (Juego.I != null) sb.Append("  ·  ").Append(Juego.I.HoraTexto);
        sb.Append("</color></size>\n\n");
        for (int i = 0; i < Ops.Length; i++)
        {
            string o = Ops[i];
            if (!Habilitada(i)) sb.Append("<color=#555555>").Append(o).Append("</color>");
            else sb.Append(i == sel ? "<color=#ff4d4d>>  " + o + "  <</color>" : "<color=#bbbbbb>" + o + "</color>");
            sb.Append("\n\n");
        }
        if (!puedeGuardar) sb.Append("<size=20><color=#888888>Aquí no se puede guardar (solo en los pisos del edificio).</color></size>\n");
        if (aviso != "") sb.Append("<size=22><color=#7bd389>").Append(aviso).Append("</color></size>");
        txt.text = sb.ToString();
    }

    void Mover(int d)
    {
        do sel = (sel + Ops.Length + d) % Ops.Length; while (!Habilitada(sel));
        Sonido.Efecto("click", 0.5f, 1f);
        Dibujar();
    }

    void Update()
    {
        transform.SetAsLastSibling();
        if (Entrada.Bajada(Tecla.Arriba)) Mover(-1);
        if (Entrada.Bajada(Tecla.Abajo)) Mover(1);
        if (Entrada.Bajada(Tecla.Pausa) || Entrada.Bajada(Tecla.Saltar)) { sel = 0; listo = true; }
        else if (Entrada.Avanzar()) { Sonido.Efecto("blip", 0.6f, 0.8f); listo = true; }
    }
}

// Elegir uno de los 3 espacios para guardar (devuelve 0 si se cancela)
public class PanelEspacios : MonoBehaviour
{
    Text txt;
    int sel;
    bool listo;
    string titulo;

    public static IEnumerator Abrir(Transform padre, string titulo, Resultado<int> r)
    {
        PanelEspacios p = Crear(padre, titulo);
        GameObject go = p.gameObject;
        yield return null;
        while (!p.listo) yield return null;
        r.valor = p.sel < Guardado.Espacios ? p.sel + 1 : 0;
        Destroy(go);
    }

    public static PanelEspacios Crear(Transform padre, string titulo)
    {
        GameObject go = new GameObject("Espacios", typeof(RectTransform));
        go.transform.SetParent(padre, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = rt.offsetMax = Vector2.zero;
        PanelEspacios p = go.AddComponent<PanelEspacios>();
        p.titulo = titulo;
        RectTransform panel = Util.PanelArteCentro(go.transform, "Panel", new Color(0.04f, 0.03f, 0.07f, 0.97f), Util.Hex("#ffd166"), Vector2.zero, new Vector2(1100, 600));
        p.txt = Util.TextoRelleno(panel, "", 28, Color.white, TextAnchor.UpperLeft, 36);
        p.Dibujar();
        return p;
    }

    void Dibujar()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder("<b>").Append(titulo).Append("</b>\n\n");
        for (int i = 0; i < Guardado.Espacios; i++)
            sb.Append(i == sel ? "<color=#ff4d4d>> </color>" : "   ").Append("<color=#ffd166>ESPACIO ").Append(i + 1).Append("</color>   ")
              .Append(Guardado.Resumen(Guardado.Leer(i + 1))).Append("\n\n");
        sb.Append(sel == Guardado.Espacios ? "<color=#ff4d4d>> Cancelar</color>" : "   Cancelar");
        sb.Append("\n\n<size=20><color=#888888>Arriba/abajo o 1-3: elegir · E: aceptar · Esc: cancelar</color></size>");
        txt.text = sb.ToString();
    }

    void Update()
    {
        transform.SetAsLastSibling();
        int n = Entrada.NumeroBajado();
        if (n >= 1 && n <= Guardado.Espacios) { sel = n - 1; listo = true; return; }
        if (Entrada.Bajada(Tecla.Arriba)) { sel = (sel + Guardado.Espacios) % (Guardado.Espacios + 1); Sonido.Efecto("click", 0.5f, 1f); Dibujar(); }
        if (Entrada.Bajada(Tecla.Abajo)) { sel = (sel + 1) % (Guardado.Espacios + 1); Sonido.Efecto("click", 0.5f, 1f); Dibujar(); }
        if (Entrada.Bajada(Tecla.Saltar)) { sel = Guardado.Espacios; listo = true; }
        else if (Entrada.Avanzar()) listo = true;
    }
}

// ¿Seguro? Sí / No  (flechas para elegir, E para aceptar, Esc = No)
public class PanelConfirmar : MonoBehaviour
{
    Text txt;
    bool si, listo;
    string pregunta, detalle;

    public static IEnumerator Preguntar(Transform padre, string pregunta, string detalle, Resultado<bool> r)
    {
        GameObject go = new GameObject("Confirmar", typeof(RectTransform));
        go.transform.SetParent(padre, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = rt.offsetMax = Vector2.zero;
        Util.Panel(go.transform, "Oscuro", new Color(0f, 0f, 0f, 0.65f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        PanelConfirmar p = go.AddComponent<PanelConfirmar>();
        p.pregunta = pregunta;
        p.detalle = detalle ?? "";
        RectTransform panel = Util.PanelArteCentro(go.transform, "Panel", new Color(0.05f, 0.02f, 0.04f, 0.97f), Util.Hex("#e63946"), Vector2.zero, new Vector2(860, 400));
        p.txt = Util.TextoRelleno(panel, "", 40, Color.white, TextAnchor.MiddleCenter, 36);
        p.Dibujar();
        Sonido.Efecto("blip", 0.6f, 0.7f);
        yield return null;
        while (!p.listo) yield return null;
        r.valor = p.si;
        Sonido.Efecto("click", 0.6f, p.si ? 0.8f : 1.2f);
        Destroy(go);
    }

    void Dibujar()
    {
        string bSi = si ? "<color=#ff4d4d>>  SÍ  <</color>" : "<color=#888888>SÍ</color>";
        string bNo = !si ? "<color=#7bd389>>  NO  <</color>" : "<color=#888888>NO</color>";
        txt.text = "<b>" + pregunta + "</b>\n" +
                   (detalle != "" ? "<size=24><color=#bbbbbb>" + detalle + "</color></size>\n" : "") +
                   "\n" + bSi + "          " + bNo;
    }

    void Update()
    {
        transform.SetAsLastSibling();
        if (Entrada.Bajada(Tecla.Izquierda) || Entrada.Bajada(Tecla.Derecha) || Entrada.Bajada(Tecla.Arriba) || Entrada.Bajada(Tecla.Abajo))
        { si = !si; Sonido.Efecto("click", 0.5f, 1f); Dibujar(); }
        if (Entrada.Bajada(Tecla.N1)) { si = true; listo = true; }
        else if (Entrada.Bajada(Tecla.N2) || Entrada.Bajada(Tecla.Saltar)) { si = false; listo = true; }
        else if (Entrada.Avanzar()) listo = true;
    }

    // Cierra el juego (en el editor de Unity, detiene el Play)
    public static void SalirDelJuego()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
