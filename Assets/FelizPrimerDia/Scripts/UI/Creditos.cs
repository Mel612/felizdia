using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// =====================================================================
//  CRÉDITOS que suben por la pantalla, como en los juegos de antes.
//  Salen desde el menú (CRÉDITOS) y al romper el bucle.
//  E / Espacio (mantener): más rápido · Esc: saltar.
//  Para cambiar nombres, edita el texto de Texto() más abajo.
// =====================================================================
public class Creditos : MonoBehaviour
{
    public const string Equipo = "TRES NEURONAS Y UN CROMOSOMA";

    static string T(string titulo) { return "<size=30><color=#e63946>" + titulo + "</color></size>\n"; }
    static string N(string nombre) { return "<size=40>" + nombre + "</size>\n"; }
    static string P(string rol, string nombre) { return "<size=26><color=#999999>" + rol + "</color></size>\n<size=36>" + nombre + "</size>\n\n"; }

    public static string Texto()
    {
        string eq = "<color=#ffd166>" + Equipo + "</color>";
        return
            "<size=96><b>FELIZ PRIMER DÍA</b></size>\n" +
            "<size=30><color=#ffd166>UNIFRANZ  ·  SEDE LA PAZ</color></size>\n\n\n\n" +
            "<size=28><color=#999999>Un juego de</color></size>\n<size=54><b>" + eq + "</b></size>\n\n\n\n" +
            P("Historia y guion", eq) +
            P("Programación", eq) +
            P("Arte en pixel art", eq) +
            P("Música y efectos de sonido", eq + "\n<size=24><color=#999999>(generados por código, nota por nota)</color></size>") +
            P("Cinemáticas", eq) +
            "\n\n" + T("REPARTO") +
            N("Valeria Quispe") + "<size=24><color=#999999>la cachimba que vive su primer día una y otra vez</color></size>\n\n" +
            N("Mateo Choque") + "<size=24><color=#999999>Ing. de Sistemas, el que lo vio todo por las cámaras</color></size>\n\n" +
            N("Camila") + "<size=24><color=#999999>la delegada que recuerda</color></size>\n\n" +
            N("Andrea") + N("Diego") + N("Sebastián") +
            N("Don Teodoro") + "<size=24><color=#999999>guardia de Seguridad</color></size>\n\n" +
            N("Ing. Ramiro Vargas") + N("Doña Rosa") + N("Martín") + N("Lucía") + N("Joaquín") + N("Paola") + N("Ariel") + N("Iván") +
            N("Marcos") + N("Nico") + N("Gabriela") + N("Lucero") + N("Fernanda") + "\n" +
            N("La ñatita") + "\n" +
            "<size=48><color=#7bd389>EL PEPINO</color></size>\n<size=24><color=#999999>como él mismo</color></size>\n\n\n\n" +
            T("CARRERAS QUE CONOCISTE") +
            "<size=32>Medicina  ·  Derecho  ·  Periodismo  ·  Ingeniería de Sistemas\n" +
            "Psicología  ·  Administración de Empresas  ·  Economía  ·  Contaduría Pública\n" +
            "Diseño Gráfico  ·  Publicidad y Marketing  ·  Hotelería y Turismo\n" +
            "Odontología  ·  Bioquímica y Farmacia  ·  Ingeniería Comercial</size>\n\n\n\n" +
            T("INSPIRADO EN") +
            N("Feliz día de tu muerte") + N("Resident Evil") + N("Five Nights at Freddy's") + "\n\n\n" +
            T("GRACIAS A") +
            N("Unifranz, sede La Paz") +
            "<size=28><color=#bbbbbb>y a todos los que probaron el juego y murieron muchas veces</color></size>\n\n\n\n\n" +
            "<size=30><color=#bbbbbb>Dedicado a todos los cachimbos que alguna vez\nse perdieron buscando el aula en su primer día.</color></size>\n\n\n\n\n\n" +
            "<size=60><b>GRACIAS POR JUGAR</b></size>\n\n\n\n\n\n\n\n" +
            "<size=26><color=#7bd389>... nos vemos en la próxima Bienvenida.</color></size>";
    }

    Text txt;
    RectTransform rtTexto;
    float y, alto;
    bool listo;

    public static IEnumerator Rodar(Transform padre)
    {
        Creditos c = Crear(padre);
        GameObject go = c.gameObject;
        Sonido.Musica("m_triste", 1.5f);
        yield return null;
        while (!c.listo) yield return null;
        Destroy(go);
    }

    // Arma la pantalla de créditos (desplazamiento 0 = el texto empieza abajo de la pantalla)
    public static Creditos Crear(Transform padre)
    {
        GameObject go = new GameObject("Créditos", typeof(RectTransform));
        go.transform.SetParent(padre, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = rt.offsetMax = Vector2.zero;
        Util.Panel(go.transform, "Fondo", new Color(0.01f, 0.01f, 0.03f, 1f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        Creditos c = go.AddComponent<Creditos>();

        // Texto anclado abajo y centrado: empieza justo debajo de la pantalla y sube
        c.txt = Util.TextoUI(go.transform, Texto(), 36, Color.white, TextAnchor.UpperCenter,
                             new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), Vector2.zero, Vector2.zero);
        c.txt.lineSpacing = 1.1f;
        c.rtTexto = c.txt.rectTransform;
        c.rtTexto.pivot = new Vector2(0.5f, 1f);
        c.rtTexto.sizeDelta = new Vector2(1500f, 100f);
        c.alto = Mathf.Max(3000f, c.txt.preferredHeight);
        c.rtTexto.sizeDelta = new Vector2(1500f, c.alto);
        c.y = 0f;
        c.rtTexto.anchoredPosition = new Vector2(0f, c.y);

        Util.TextoUI(go.transform, "Mantén E para adelantar  ·  Esc para saltar", 20, new Color(1, 1, 1, 0.35f), TextAnchor.LowerRight,
                     new Vector2(1, 0), new Vector2(1, 0), new Vector2(-700, 14), new Vector2(-20, 50));
        return c;
    }

    public void Desplazar(float y) { this.y = y; rtTexto.anchoredPosition = new Vector2(0f, y); }

    void Update()
    {
        transform.SetAsLastSibling();
        bool rapido = Entrada.Presionada(Tecla.Interactuar) || Entrada.Presionada(Tecla.Accion);
        y += (rapido ? 420f : 75f) * Time.unscaledDeltaTime;
        rtTexto.anchoredPosition = new Vector2(0f, y);
        // Termina cuando todo el texto pasó y salió por arriba (1080 de alto en la pantalla de referencia)
        if (y > alto + 1100f || Entrada.Bajada(Tecla.Saltar)) listo = true;
    }
}
