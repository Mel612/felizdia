using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// =====================================================================
//  SUSTOS AL AZAR, MENSAJES EN LAS PAREDES Y EL ESPEJO DE LOS BAÑOS
//  - Cada cierto tiempo pasa algo: las luces parpadean, pasos detrás de ti,
//    la sombra del Pepino cruza el fondo del pasillo, un casillero se cierra
//    de golpe o llega un mensaje del número desconocido. Más seguido en cada bucle.
//  - Cuando el Pepino ya anda suelto, aparecen mensajes escritos en rojo en
//    las paredes del pasillo. Cambian según lo que pasó en los bucles.
//  - En los baños, a veces, el Pepino aparece en el espejo.
// =====================================================================
public class Sustos : MonoBehaviour
{
    Juego j;
    int piso;
    float proximo;
    bool pintadasHechas, espejoHecho;
    readonly List<TextMesh> pintadas = new List<TextMesh>();
    Sala banos;
    Vector2 posEspejo;
    float enBano;

    public static void Montar(Transform t, Juego juego, int piso)
    {
        GameObject go = new GameObject("Sustos");
        go.transform.SetParent(t, false);
        Sustos s = go.AddComponent<Sustos>();
        s.j = juego;
        s.piso = piso;
        s.proximo = s.Espera() * 0.6f;
        // Espejos en los baños (si este piso tiene)
        Sala b;
        if (juego.mapa != null && juego.mapa.Salas.TryGetValue("banos" + piso, out b))
        {
            s.banos = b;
            s.posEspejo = b.P(0f, 3.6f);
            Horno.Suspender();
            Util.Rect("Espejo", go.transform, s.posEspejo, new Vector2(4.2f, 0.9f), Util.Hex("#a9c6de"), -1470);
            Util.Rect("Marco del espejo", go.transform, s.posEspejo + new Vector2(0f, -0.5f), new Vector2(4.4f, 0.12f), Util.Hex("#4a5560"), -1465);
            Horno.Reanudar();
        }
        s.espejoHecho = Memoria.Tiene("espejo_" + piso);
    }

    // Segundos hasta el próximo susto: menos en cada bucle
    float Espera() { return Random.Range(35f, 70f) * Mathf.Max(0.45f, 1f - 0.1f * (Memoria.Dia - 1)); }

    bool Libre
    {
        get
        {
            return j != null && j.jugador != null && !j.Bloqueado && !Cine.Activa && !j.ui.PantallaVisible &&
                   !j.RelojDetenido && j.Hora >= 7.25f && Pepino.Cazando == 0;
        }
    }

    void Update()
    {
        if (j == null || j.jugador == null) return;

        if (!pintadasHechas && j.Hora >= j.HoraPepino) { pintadasHechas = true; Pintar(); }
        RevisarEspejo();

        if (!Libre) return;
        proximo -= Time.deltaTime;
        if (proximo > 0f) return;
        proximo = Espera();
        switch (Random.Range(0, 5))
        {
            case 0: StartCoroutine(Luces()); break;
            case 1: StartCoroutine(Pasos()); break;
            case 2: StartCoroutine(Sombra()); break;
            case 3: Casillero(); break;
            default: Celular.Forzar("Número desconocido", MensajeDesconocido()); break;
        }
    }

    // ---------- Los sustos ----------
    IEnumerator Luces()
    {
        Sonido.Efecto("electricidad", 0.5f, 1f);
        for (int i = 0; i < 4; i++)
        {
            j.ui.OscuridadInstantanea(0.9f);
            yield return new WaitForSeconds(Random.Range(0.05f, 0.12f));
            j.ui.OscuridadInstantanea(0f);
            yield return new WaitForSeconds(Random.Range(0.08f, 0.25f));
        }
    }

    IEnumerator Pasos()
    {
        for (int i = 0; i < 6; i++)
        {
            Sonido.Efecto("paso_pepino", 0.15f + i * 0.07f, Random.Range(0.85f, 0.92f));
            yield return new WaitForSeconds(0.42f);
        }
        Sonido.Latido(0.5f);
        j.ui.Toast("...¿pasos? Detrás de ti.", 2.5f);
        yield return new WaitForSeconds(1.5f);
        Sonido.Latido(0f);
    }

    IEnumerator Sombra()
    {
        // La silueta del Pepino cruza el fondo del pasillo y desaparece
        Vector2 o = Mapa.Origen(piso);
        float lado = Random.value < 0.5f ? -1f : 1f;
        float x = Mathf.Clamp(j.jugador.Posicion.x - o.x + lado * 9f, -Mapa.LargoPasillo / 2f + 2f, Mapa.LargoPasillo / 2f - 2f);
        Vector2 desde = o + new Vector2(x, 0.4f), hasta = desde + new Vector2(3.2f, 0f);
        Horno.Suspender();
        GameObject go = new GameObject("Sombra del Pepino");
        go.transform.SetParent(transform, false);
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        Util.AplicarMaterial(sr);
        Horno.Reanudar();
        SpritesPersonaje sp = Arte.Pepino();
        sr.sortingOrder = OrdenPorY.Orden(desde.y);
        Sonido.Efecto("respiracion", 0.4f, 0.8f);
        float t = 0f, dur = 1.3f;
        while (t < dur)
        {
            t += Time.deltaTime;
            float k = t / dur;
            go.transform.position = Vector2.Lerp(desde, hasta, k);
            sr.sprite = sp.lado[1 + ((int)(t * 7f)) % 2];
            sr.color = new Color(0f, 0f, 0f, 0.8f * Mathf.Sin(k * Mathf.PI));
            yield return null;
        }
        Destroy(go);
    }

    void Casillero()
    {
        Interactuable mejor = null; float d = 9f;
        foreach (Interactuable i in Interactuable.Todos)
        {
            if (i == null || !i.Texto.StartsWith("Esconderte")) continue;
            float di = Vector2.Distance(i.transform.position, j.jugador.Posicion);
            if (di < d) { d = di; mejor = i; }
        }
        Sonido.Efecto("puerta", 0.8f, 0.7f);
        j.ui.Golpe(0.15f);
        j.ui.Toast(mejor != null ? "¡CLAC! Un casillero se cerró de golpe... y no hay nadie." : "¡CLAC! Una puerta se cerró de golpe.", 2.5f);
    }

    string MensajeDesconocido()
    {
        string[] m = Protagonista.EsMateo
            ? new[] { "Sé dónde estás, Mateo.", "Las cámaras también te graban a ti.", "Borra todo y nadie sale herido." }
            : new[] { "Te estoy mirando.", "Bonita mochila.", "¿Ya elegiste carrera?", "No corras. Es peor.", "Estoy más cerca de lo que crees." };
        return m[Random.Range(0, m.Length)];
    }

    // ---------- Mensajes en las paredes ----------
    void Pintar()
    {
        List<string> mensajes = new List<string>();
        float xCasillero = float.NaN;
        string prefijo = "casillero_" + piso + "_";
        foreach (string id in Memoria.Recuerdos)
            if (id.StartsWith(prefijo)) { float x; if (float.TryParse(id.Substring(prefijo.Length), out x)) xCasillero = x; }

        if (Protagonista.EsMateo) { mensajes.Add("DEJA LOS VIDEOS"); mensajes.Add("TE ESTOY VIENDO, MATEO"); }
        else
        {
            if (Memoria.Muertes >= 1) mensajes.Add("TE VI MORIR " + Memoria.Muertes + (Memoria.Muertes == 1 ? " VEZ" : " VECES"));
            if (Memoria.Dia >= 2) mensajes.Add("¿OTRA VEZ?");
            if (Memoria.Escapes >= 1) mensajes.Add("NO VAS A ESCAPAR");
            if (Memoria.Dia >= 4) mensajes.Add("¿YA SABES QUIÉN SOY?");
            mensajes.Add("FELIZ PRIMER DÍA");
        }
        Vector2 o = Mapa.Origen(piso);
        float yPared = Mapa.AnchoPasillo / 2f - 0.7f;
        float[] lugares = { -6f, 8f, 18f };
        int cuantos = Mathf.Min(2, mensajes.Count);
        Horno.Suspender();
        for (int i = 0; i < cuantos; i++)
        {
            string txt = mensajes[(i + piso + Memoria.Dia) % mensajes.Count];
            PintarUno(txt, o + new Vector2(lugares[(i + piso) % lugares.Length], yPared));
        }
        if (!float.IsNaN(xCasillero) && !Protagonista.EsMateo)
            PintarUno("TE VI EN ESTE CASILLERO", new Vector2(xCasillero, o.y + yPared));
        Horno.Reanudar();

        // Si aparecen mientras estás en el piso, se nota
        if (Libre && !j.RelojDetenido)
        {
            Sonido.Efecto("susto", 0.35f, 0.7f);
            j.ui.Toast("Alguien escribió algo en las paredes...", 3f);
        }
    }

    void PintarUno(string txt, Vector2 pos)
    {
        TextMesh tm = Util.Texto(txt, transform, pos, 0.55f, new Color(0.62f, 0.04f, 0.06f, 0.92f), -1440);
        pintadas.Add(tm);
        // Chorreados debajo de las letras
        float ancho = txt.Length * 0.17f;
        for (int k = 0; k < 6; k++)
        {
            float x = pos.x - ancho / 2f + (k + 0.5f) * ancho / 6f + Random.Range(-0.1f, 0.1f);
            float largo = Random.Range(0.15f, 0.55f);
            Util.Rect("Chorreado", transform, new Vector2(x, pos.y - 0.25f - largo / 2f), new Vector2(0.06f, largo), new Color(0.55f, 0.03f, 0.05f, 0.9f), -1441);
        }
    }

    // ---------- El espejo del baño ----------
    void RevisarEspejo()
    {
        if (banos == null || espejoHecho || !Libre) { enBano = 0f; return; }
        if (!banos.area.Contains(j.jugador.Posicion)) { enBano = 0f; return; }
        enBano += Time.deltaTime;
        if (enBano < 1.5f) return;
        espejoHecho = true;
        Memoria.Obtener("espejo_" + piso);
        StartCoroutine(Espejo());
    }

    IEnumerator Espejo()
    {
        Horno.Suspender();
        GameObject go = new GameObject("Reflejo del Pepino");
        go.transform.SetParent(transform, false);
        go.transform.position = posEspejo + new Vector2(0f, -0.25f);
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        Util.AplicarMaterial(sr);
        Horno.Reanudar();
        sr.sprite = Arte.Pepino().abajo[0];
        sr.sortingOrder = -1460;
        sr.color = new Color(1f, 1f, 1f, 0.85f);
        Sonido.Efecto("susto", 0.9f, 1f);
        j.ui.Golpe(0.35f);
        j.ui.Destello(new Color(0.8f, 0f, 0f), 0.25f);
        yield return new WaitForSeconds(0.45f);
        Destroy(go);
        j.ui.Toast("...en el espejo. Detrás de ti. Pero cuando te das vuelta, no hay nadie.", 3.5f);
    }
}
