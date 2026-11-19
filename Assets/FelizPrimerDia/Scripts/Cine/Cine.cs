using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// =====================================================================
//  CINE: motor de cinemáticas (estilo Resident Evil).
//  Barras negras, la cámara se mueve sola, los personajes actúan y
//  aparecen subtítulos. Se puede saltar con Esc.
//
//  Uso (dentro de una corrutina):
//    Cine c = Cine.Empezar();
//    c.Camara(pos, 3f);
//    yield return c.Decir("Valeria", "¿Hola?", 2f);
//    yield return c.Terminar();
// =====================================================================
public class Cine : MonoBehaviour
{
    public static bool Activa { get; private set; }

    public bool Saltada { get; private set; }

    Juego juego;
    Transform ancla;
    float zoomObjetivo, zoomInicial;
    bool ocultarJugador;
    readonly List<GameObject> creados = new List<GameObject>();

    UIJuego UI { get { return juego != null ? juego.ui : null; } }

    public static Cine Empezar() { return Empezar(true); }

    public bool EnGaleria { get; private set; }

    // En la galería (editor) no hay tiempo: las acciones en paralelo se ejecutan de una vez.
    public new Coroutine StartCoroutine(IEnumerator rutina)
    {
        if (EnGaleria) { Drenar(rutina); return null; }
        return base.StartCoroutine(rutina);
    }

    public static void Drenar(IEnumerator e)
    {
        Stack<IEnumerator> pila = new Stack<IEnumerator>();
        pila.Push(e);
        int seguro = 0;
        while (pila.Count > 0 && seguro++ < 200000)
        {
            IEnumerator top = pila.Peek();
            if (top.MoveNext()) { IEnumerator hijo = top.Current as IEnumerator; if (hijo != null) pila.Push(hijo); }
            else pila.Pop();
        }
    }

    // Para dibujar los decorados en el editor (escenas de galería): sin esperas ni interfaz.
    public static Cine ParaGaleria(Transform padre)
    {
        GameObject go = new GameObject("Galería (cine)");
        go.transform.SetParent(padre, false);
        Cine c = go.AddComponent<Cine>();
        c.juego = Juego.I;
        c.Saltada = true;
        c.EnGaleria = true;
        GameObject a = new GameObject("Cámara de cine");
        a.transform.SetParent(go.transform, false);
        c.ancla = a.transform;
        return c;
    }

    public static Cine Empezar(bool ocultarJugador)
    {
        Juego j = Juego.I;
        GameObject go = new GameObject("Cinemática");
        go.transform.SetParent(j.Raiz, false);
        Cine c = go.AddComponent<Cine>();
        c.juego = j;
        j.Bloquear(c);
        Activa = true;
        GameObject a = new GameObject("Cámara de cine");
        a.transform.SetParent(go.transform, false);
        c.ancla = a.transform;
        c.zoomInicial = j.camara != null ? j.camara.orthographicSize : j.zoomCamara;
        c.zoomObjetivo = c.zoomInicial;
        if (j.camara != null) c.ancla.position = new Vector3(j.camara.transform.position.x, j.camara.transform.position.y, 0f);
        j.camaraSeguir = c.ancla;
        c.ocultarJugador = ocultarJugador;
        c.MostrarJugador(!ocultarJugador);
        j.ui.Barras(true);
        return c;
    }

    public void MostrarJugador(bool ver)
    {
        if (juego == null || juego.jugador == null) return;
        foreach (SpriteRenderer sr in juego.jugador.GetComponentsInChildren<SpriteRenderer>()) sr.enabled = ver;
    }

    void Update()
    {
        if (!Saltada && Entrada.Bajada(Tecla.Saltar)) Saltada = true;
        if (juego != null && juego.camara != null)
            juego.camara.orthographicSize = Mathf.Lerp(juego.camara.orthographicSize, zoomObjetivo, 1f - Mathf.Exp(-4f * Time.deltaTime));
    }

    // ---------------------------------------------------------------
    //  CÁMARA
    // ---------------------------------------------------------------
    // Corte directo
    public void Camara(Vector2 pos, float zoom)
    {
        ancla.position = pos;
        zoomObjetivo = zoom;
        if (juego != null && juego.camara != null)
        {
            juego.camara.transform.position = new Vector3(pos.x, pos.y, -10f);
            juego.camara.orthographicSize = zoom;
        }
    }

    // Movimiento suave de la cámara
    public IEnumerator Mover(Vector2 hasta, float zoom, float segundos)
    {
        Vector2 desde = ancla.position;
        float z0 = zoomObjetivo;
        float t = 0f;
        while (t < segundos && !Saltada)
        {
            t += Time.deltaTime;
            float k = Suave(t / segundos);
            ancla.position = Vector2.Lerp(desde, hasta, k);
            zoomObjetivo = Mathf.Lerp(z0, zoom, k);
            yield return null;
        }
        ancla.position = hasta;
        zoomObjetivo = zoom;
    }

    public void Temblor(float fuerza) { if (UI != null) UI.Golpe(fuerza); }

    // ---------------------------------------------------------------
    //  TIEMPO Y TEXTO
    // ---------------------------------------------------------------
    public IEnumerator Esperar(float segundos)
    {
        float t = 0f;
        while (t < segundos && !Saltada) { t += Time.deltaTime; yield return null; }
    }

    // Subtítulo. Se puede adelantar con E / Espacio.
    public IEnumerator Decir(string quien, string texto, float segundos)
    {
        if (Saltada) yield break;
        if (UI != null) UI.Subtitulo(quien, texto);
        Sonido.Efecto("blip", 0.2f, 1f);
        float t = 0f;
        yield return null;
        while (t < segundos && !Saltada)
        {
            t += Time.deltaTime;
            if (t > 0.3f && Entrada.Avanzar()) break;
            yield return null;
        }
        if (UI != null) UI.Subtitulo(null, null);
    }

    public void Cartel(string titulo, string sub, float segundos) { if (!Saltada && UI != null) UI.Cartel(titulo, sub, segundos); }

    public IEnumerator Fundido(float alpha, float segundos)
    {
        if (UI != null) UI.Telon(alpha, Saltada ? 0f : segundos);
        yield return Esperar(segundos);
    }

    // ---------------------------------------------------------------
    //  ACTORES
    // ---------------------------------------------------------------
    // Personaje vista desde arriba (camina con animación en 4 direcciones)
    public SpriteRenderer Actor(string nombre, SpritesPersonaje sprites, Vector2 pos)
    {
        Horno.Suspender();
        GameObject go = new GameObject("Actor: " + nombre);
        go.transform.SetParent(transform, false);
        go.transform.position = pos;
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        Util.AplicarMaterial(sr);
        Util.Animar(go, sr, sprites);
        Util.Plano("Sombra", go.transform, Arte.Sombra(), pos + new Vector2(0f, -0.72f), -900);
        Horno.Reanudar();
        creados.Add(go);
        return sr;
    }

    // Objeto o decorado suelto (sprite fijo)
    public SpriteRenderer Objeto(string nombre, Sprite sprite, Vector2 pos, int orden)
    {
        Horno.Suspender();
        GameObject go = new GameObject(nombre);
        go.transform.SetParent(transform, false);
        go.transform.position = pos;
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = orden;
        Util.AplicarMaterial(sr);
        Horno.Reanudar();
        creados.Add(go);
        return sr;
    }

    public IEnumerator Caminar(Transform t, Vector2 hasta, float velocidad)
    {
        Vector2 desde = t.position;
        float dist = Vector2.Distance(desde, hasta);
        float dur = dist / Mathf.Max(0.1f, velocidad), tt = 0f;
        while (tt < dur && !Saltada)
        {
            tt += Time.deltaTime;
            t.position = Vector2.Lerp(desde, hasta, tt / dur);
            yield return null;
        }
        t.position = hasta;
    }

    // Caminata de lado (para escenarios laterales): alterna los cuadros del sprite "lado".
    public IEnumerator CaminarLado(SpriteRenderer sr, SpritesPersonaje s, float hastaX, float velocidad)
    {
        Transform t = sr.transform;
        float x0 = t.position.x;
        sr.flipX = hastaX < x0;
        float dur = Mathf.Abs(hastaX - x0) / Mathf.Max(0.1f, velocidad), tt = 0f;
        while (tt < dur && !Saltada)
        {
            tt += Time.deltaTime;
            t.position = new Vector3(Mathf.Lerp(x0, hastaX, tt / dur), t.position.y, 0f);
            sr.sprite = s.lado[1 + ((int)(tt * 7f)) % 2];
            yield return null;
        }
        t.position = new Vector3(hastaX, t.position.y, 0f);
        sr.sprite = s.lado[0];
    }

    // Parpadeo de luces (apagones, tensión)
    public IEnumerator Parpadeo(int veces, float base_)
    {
        for (int i = 0; i < veces && !Saltada; i++)
        {
            if (UI != null) UI.OscuridadInstantanea(0.85f);
            yield return Esperar(Random.Range(0.05f, 0.15f));
            if (UI != null) UI.OscuridadInstantanea(base_);
            yield return Esperar(Random.Range(0.08f, 0.3f));
        }
    }

    public void Registrar(GameObject go) { creados.Add(go); }

    // ---------------------------------------------------------------
    public IEnumerator Terminar() { return Terminar(true); }

    public IEnumerator Terminar(bool devolverCamara)
    {
        UI.Subtitulo(null, null);
        UI.Barras(false);
        UI.Telon(0f, 0.4f);
        foreach (GameObject go in creados) if (go != null) Destroy(go);
        creados.Clear();
        if (devolverCamara)
        {
            juego.camaraSeguir = null;
            zoomObjetivo = juego.zoomCamara;
            if (juego.camara != null) juego.camara.orthographicSize = juego.zoomCamara;
        }
        MostrarJugador(true);
        Activa = false;
        juego.Desbloquear(this);
        Destroy(gameObject);
        yield break;
    }

    void OnDestroy()
    {
        if (Activa && juego != null && juego.camaraSeguir == ancla) juego.camaraSeguir = null;
        Activa = false;
    }

    static float Suave(float t) { t = Mathf.Clamp01(t); return t * t * (3f - 2f * t); }
}
