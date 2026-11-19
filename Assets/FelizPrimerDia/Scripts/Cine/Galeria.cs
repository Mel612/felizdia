using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

// =====================================================================
//  GALERÍA DE CINEMÁTICAS Y RECUERDOS
//  Escenas 8_Cinematicas y 9_Recuerdos: todos los planos quedan DIBUJADOS
//  uno al lado del otro (con su nombre encima) para verlos en el editor.
//  Al darle Play aparece una lista: eliges una y se reproduce animada.
//  (El juego arma estas cinemáticas solo; cambiar los dibujos de la galería
//   no cambia lo que se ve al jugar la historia.)
// =====================================================================
public class Galeria : MonoBehaviour
{
    public class Item
    {
        public string etiqueta;
        public Func<Cine, IEnumerator> dibujar;      // para dibujar en el editor
        public Func<Juego, IEnumerator> ver;         // para reproducir al jugar
        public float alto = 16f;                     // espacio vertical de la fila
    }

    static IEnumerator Privado(Type t, string metodo, params object[] args)
    {
        MethodInfo m = t.GetMethod(metodo, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
        return m != null ? (IEnumerator)m.Invoke(null, args) : null;
    }

    static IEnumerator Fachada(Cine c, bool noche)
    {
        FachadaUnifranz.Construir(c, new Vector2(0f, CineFinales.FilaGaleria), noche);
        Horno.Suspender();
        Util.Texto(CineFinales.EtiquetaGaleria, c.transform, new Vector2(0f, CineFinales.FilaGaleria + 12.5f), 1.4f, Util.Hex("#ffd166"), 50);
        Horno.Reanudar();
        yield break;
    }

    static IEnumerator CuartoMateo(Cine c)
    {
        yield return Privado(typeof(CineMateo), "CuartoMonitoreo", c);
        CineFinales.Pj(Arte.Mateo(), -1.6f, false);
    }

    public static List<Item> Lista(int cual)
    {
        List<Item> l = new List<Item>();
        if (cual == 0)
        {
            l.Add(new Item { etiqueta = "Entrada de la U (noche, menú)", alto = 22f, dibujar = c => Fachada(c, true), ver = null });
            l.Add(new Item { etiqueta = "Entrada de la U (día, llegada)", alto = 22f, dibujar = c => Fachada(c, false), ver = j => CineHistoria.Intro(j) });
            string[] finales = { "Bueno", "Verdadero", "Natita", "Oscuro", "Malo", "Pesadilla" };
            string[] ids = { "final_bueno", "final_verdadero", "final_natita", "final_oscuro", "final_malo", "final_pesadilla" };
            string[] nombres = { "Final bueno", "Final verdadero (perdón)", "Final de la ñatita", "Final oscuro", "Final malo", "Final pesadilla" };
            for (int i = 0; i < finales.Length; i++)
            {
                string metodo = finales[i], id = ids[i];
                l.Add(new Item { etiqueta = nombres[i], dibujar = c => Privado(typeof(CineFinales), metodo, c), ver = j => CineFinales.Reproducir(id) });
            }
            l.Add(new Item { etiqueta = "Mateo: prólogo (cuarto de monitoreo)", dibujar = CuartoMateo, ver = j => CineMateo.Prologo(j) });
            l.Add(new Item { etiqueta = "Mateo: final En vivo", dibujar = c => Privado(typeof(CineMateo), "EnVivo", c, false), ver = j => CineMateo.Final("mateo_envivo") });
            l.Add(new Item { etiqueta = "Mateo: final Testigo", dibujar = c => Privado(typeof(CineMateo), "EnVivo", c, true), ver = j => CineMateo.Final("mateo_testigo") });
            l.Add(new Item { etiqueta = "Mateo: final Solo un espectador", dibujar = c => Privado(typeof(CineMateo), "Espectador", c), ver = j => CineMateo.Final("mateo_espectador") });
            l.Add(new Item { etiqueta = "Mateo: final Borrado", dibujar = c => Privado(typeof(CineMateo), "Borrado", c), ver = j => CineMateo.Final("mateo_borrado") });
        }
        else
        {
            string[,] rec =
            {
                { "audio_1", "PrimerDia" }, { "audio_2", "Sebastian" }, { "audio_3", "LaNoche" }, { "audio_4", "Retiro" },
                { "audio_5", "Cementerio" }, { "audio_6", "Funciono" }, { "audio_7", "YoRecuerdo" }, { "audio_8", "SeRio" },
                { "anuario", "Anuario" }, { "presupuesto", "Presupuesto" }, { "afiche", "Afiche" }, { "quemaduras", "Quemaduras" },
                { "motivo", "Denuncia" }, { "cicatriz", "Cicatriz" }, { "usuario", "Registros" }, { "camara_vieja", "CintaRoja" },
                { "m_video_1", "VideoFiesta" }, { "m_video_2", "VideoEncendedor" }, { "m_video_3", "VideoClinica" }, { "m_video_4", "VideoCara" },
            };
            for (int i = 0; i < rec.GetLength(0); i++)
            {
                string id = rec[i, 0], metodo = rec[i, 1];
                string pista = id.StartsWith("m_video_") ? "m_archivo_" + id.Substring(8) : id;
                string titulo = Pistas.Existe(pista) ? Pistas.Catalogo[pista].titulo : id;
                if (id == "camara_vieja") titulo = "La cinta roja";
                l.Add(new Item { etiqueta = "Recuerdo: " + titulo, dibujar = c => Privado(typeof(Recuerdos), metodo, c), ver = j => Recuerdos.Reproducir(id) });
            }
        }
        return l;
    }

    // ---------------------------------------------------------------
    //  DIBUJAR (lo usa el editor al construir las escenas)
    // ---------------------------------------------------------------
    public static void Hornear(int cual, Transform raiz)
    {
        CineFinales.ModoGaleria = true;
        try
        {
            Cine c = Cine.ParaGaleria(raiz);
            float y = 0f;
            foreach (Item e in Lista(cual))
            {
                CineFinales.EtiquetaGaleria = e.etiqueta;
                CineFinales.NuevaFila(y);
                try { Cine.Drenar(e.dibujar(c)); }
                catch (Exception ex) { Debug.LogWarning("[Feliz Primer Día] No se pudo dibujar \"" + e.etiqueta + "\": " + ex.Message); }
                y -= e.alto;
            }
        }
        finally { CineFinales.ModoGaleria = false; }

        // Marcarlos como dibujados para que sigan en la escena al darle Play
        if (Horno.Horneando)
        {
            int n = 0;
            foreach (Transform t in raiz.GetComponentsInChildren<Transform>(true))
            {
                if (t == raiz || t.GetComponent<IdHorneado>() != null) continue;
                IdHorneado id = t.gameObject.AddComponent<IdHorneado>();
                id.id = "galeria_" + cual + "/" + (n++);
            }
        }
    }

    public static Vector2 Fila(int cual, int indice)
    {
        float y = 0f;
        List<Item> l = Lista(cual);
        for (int i = 0; i < indice && i < l.Count; i++) y -= l[i].alto;
        return new Vector2(0f, y);
    }

    // ---------------------------------------------------------------
    //  JUGAR: lista para ver cada cinemática
    // ---------------------------------------------------------------
    Juego juego;
    int cual, sel;
    List<Item> lista;
    RectTransform panel;
    Text txt;
    bool viendo;

    public static Galeria Crear(Juego j, int cual)
    {
        GameObject go = new GameObject("Galería", typeof(RectTransform));
        go.transform.SetParent(j.ui.transform, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = rt.offsetMax = Vector2.zero;
        Galeria g = go.AddComponent<Galeria>();
        g.juego = j;
        g.cual = cual;
        g.lista = Lista(cual);
        g.panel = Util.PanelArte(go.transform, "Lista", new Color(0.04f, 0.03f, 0.07f, 0.9f), Util.Hex("#ffd166"),
            new Vector2(0, 0), new Vector2(0, 1), new Vector2(20, 20), new Vector2(620, -20));
        g.txt = Util.TextoRelleno(g.panel, "", 26, Color.white, TextAnchor.UpperLeft, 20);
        j.ui.HudVisible = false;
        g.Mirar();
        g.Dibujar();
        Sonido.Musica("m_menu", 1.5f);
        return g;
    }

    // La cámara muestra el primer plano dibujado de lo que eliges
    void Mirar()
    {
        bool fachada = cual == 0 && sel < 2;
        Vector2 p = Fila(cual, sel) + new Vector2(fachada ? -1f : 1.5f, fachada ? 4.5f : 2.4f);
        if (juego.camara != null)
        {
            juego.camara.transform.position = new Vector3(p.x, p.y, -10f);
            juego.camara.orthographicSize = fachada ? 7.5f : 4.6f;
        }
    }

    void Dibujar()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append("<b>").Append(cual == 0 ? "CINEMÁTICAS" : "RECUERDOS").Append("</b>\n<size=20><color=#aaaaaa>Flechas: elegir · E: ver animada · Esc: saltar</color></size>\n\n");
        int desde = Mathf.Clamp(sel - 7, 0, Mathf.Max(0, lista.Count - 15));
        for (int i = desde; i < Mathf.Min(lista.Count, desde + 15); i++)
            sb.Append(i == sel ? "<color=#ff4d4d>> " : "<color=#cccccc>   ").Append(lista[i].etiqueta)
              .Append(lista[i].ver == null ? " <size=18>(solo dibujo)</size>" : "").Append("</color>\n");
        txt.text = sb.ToString();
    }

    void Update()
    {
        if (viendo) return;
        transform.SetAsLastSibling();
        if (Entrada.Bajada(Tecla.Abajo)) { sel = (sel + 1) % lista.Count; Sonido.Efecto("click", 0.5f, 1f); Mirar(); Dibujar(); }
        if (Entrada.Bajada(Tecla.Arriba)) { sel = (sel + lista.Count - 1) % lista.Count; Sonido.Efecto("click", 0.5f, 1f); Mirar(); Dibujar(); }
        if (Entrada.Avanzar() && lista[sel].ver != null) StartCoroutine(Ver());
    }

    IEnumerator Ver()
    {
        viendo = true;
        panel.gameObject.SetActive(false);
        yield return StartCoroutine(lista[sel].ver(juego));
        juego.ui.Telon(0f, 0.5f);
        juego.ui.Recuerdo(false, null);
        juego.camaraSeguir = null;
        Sonido.Musica("m_menu", 1.5f);
        panel.gameObject.SetActive(true);
        Mirar();
        Dibujar();
        yield return null;
        viendo = false;
    }
}
