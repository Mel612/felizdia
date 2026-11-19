using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// =====================================================================
//  HACKEO DE CÁMARAS (habilidad de Mateo)
//  Muestra el plano del piso con los puntos de: tú (celeste) y los
//  disfraces (dorado / rojo). Se cierra solo o con E / Esc.
// =====================================================================
public class MapaHack : MonoBehaviour
{
    RectTransform area;
    Rect limites;
    Juego juego;
    readonly List<Image> puntos = new List<Image>();
    Image yo;
    float t;

    public static IEnumerator Mostrar(Juego j, int piso, float segundos)
    {
        GameObject go = new GameObject("Hackeo de cámaras", typeof(RectTransform));
        go.transform.SetParent(j.ui.transform, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = rt.offsetMax = Vector2.zero;
        MapaHack m = go.AddComponent<MapaHack>();
        m.juego = j;
        j.Bloquear(m);
        m.Construir(piso);
        Sonido.Efecto("estatica", 0.4f, 1.3f);
        float tt = 0f;
        yield return null;
        while (tt < segundos && !(tt > 0.4f && (Entrada.Avanzar() || Entrada.Bajada(Tecla.Saltar))))
        {
            tt += Time.deltaTime;
            yield return null;
        }
        j.Desbloquear(m);
        Destroy(go);
    }

    void Construir(int piso)
    {
        RectTransform panel = Util.PanelArteCentro(transform, "Monitor", new Color(0.02f, 0.07f, 0.04f, 0.96f), Util.Hex("#7bd389"), Vector2.zero, new Vector2(1300, 640));
        Util.TextoUI(panel, "<b>CÁMARAS DE SEGURIDAD  ·  " + Mapa.NombresPiso[piso].ToUpper() + "</b>     <color=#ff4d4d>● EN VIVO</color>", 30, Util.Hex("#b7e4c7"),
            TextAnchor.UpperLeft, Vector2.zero, Vector2.one, new Vector2(30, 0), new Vector2(-30, -18));
        Util.TextoUI(panel, "<color=#4cc9f0>●</color> Tú     <color=#ffd166>●</color> Quinto disfraz     <color=#ff4d4d>●</color> Pepino          [E] cerrar", 24, Util.Hex("#b7e4c7"),
            TextAnchor.LowerLeft, Vector2.zero, Vector2.one, new Vector2(30, 16), new Vector2(-30, 0));

        GameObject ga = new GameObject("Plano", typeof(RectTransform));
        ga.transform.SetParent(panel, false);
        area = ga.GetComponent<RectTransform>();
        area.anchorMin = Vector2.zero; area.anchorMax = Vector2.one;
        area.offsetMin = new Vector2(40, 70); area.offsetMax = new Vector2(-40, -70);

        // Límites del piso
        bool primero = true;
        foreach (Sala s in juego.mapa.Salas.Values)
        {
            if (Mapa.PisoDe(s.Centro) != piso) continue;
            if (primero) { limites = s.area; primero = false; }
            else limites = Rect.MinMaxRect(Mathf.Min(limites.xMin, s.area.xMin), Mathf.Min(limites.yMin, s.area.yMin),
                                           Mathf.Max(limites.xMax, s.area.xMax), Mathf.Max(limites.yMax, s.area.yMax));
        }
        Vector2 esc = Mapa.Escaleras(piso);
        limites = Rect.MinMaxRect(Mathf.Min(limites.xMin, esc.x - 2f), limites.yMin, limites.xMax, limites.yMax);

        // Salas
        foreach (Sala s in juego.mapa.Salas.Values)
        {
            if (Mapa.PisoDe(s.Centro) != piso) continue;
            Vector2 a = Norm(new Vector2(s.area.xMin, s.area.yMin)), b = Norm(new Vector2(s.area.xMax, s.area.yMax));
            RectTransform r = Util.Panel(area, s.nombre, new Color(0.1f, 0.25f, 0.15f, 0.9f), a, b, new Vector2(3, 3), new Vector2(-3, -3));
            Util.TextoUI(r, s.nombre.Length > 22 ? s.nombre.Substring(0, 22) : s.nombre, 18, Util.Hex("#74a57f"), TextAnchor.MiddleCenter,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        }
        Vector2 pe = Norm(esc);
        Util.Panel(area, "Escaleras", Util.Hex("#40916c"), pe - new Vector2(0.015f, 0.04f), pe + new Vector2(0.015f, 0.04f), Vector2.zero, Vector2.zero);

        yo = Punto(Util.Hex("#4cc9f0"));
        foreach (Pepino p in Pepino.Todos)
            if (p != null && Mapa.PisoDe(p.transform.position) == piso) puntos.Add(Punto(p.EsQuinto ? Util.Hex("#ffd166") : Util.Hex("#ff4d4d")));
        Actualizar();
    }

    Image Punto(Color c)
    {
        RectTransform r = Util.PanelCentro(area, "Punto", c, Vector2.zero, new Vector2(22, 22));
        r.anchorMin = r.anchorMax = Vector2.zero;
        return r.GetComponent<Image>();
    }

    Vector2 Norm(Vector2 w)
    {
        return new Vector2(Mathf.InverseLerp(limites.xMin, limites.xMax, w.x), Mathf.InverseLerp(limites.yMin, limites.yMax, w.y));
    }

    void Colocar(Image img, Vector2 mundo)
    {
        Vector2 n = Norm(mundo);
        RectTransform r = img.rectTransform;
        r.anchorMin = r.anchorMax = n;
        r.anchoredPosition = Vector2.zero;
    }

    void Actualizar()
    {
        if (juego.jugador != null) Colocar(yo, juego.jugador.Posicion);
        int k = 0;
        foreach (Pepino p in Pepino.Todos)
        {
            if (p == null || k >= puntos.Count || Mapa.PisoDe(p.transform.position) != juego.piso) continue;
            Colocar(puntos[k], p.transform.position);
            puntos[k].enabled = p.Visible;
            k++;
        }
    }

    void Update()
    {
        transform.SetAsLastSibling();
        t += Time.deltaTime;
        Actualizar();
        Color c = yo.color;
        yo.color = new Color(c.r, c.g, c.b, (Mathf.Sin(t * 8f) + 1f) * 0.3f + 0.4f);
    }
}
