using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// =====================================================================
//  TUTORIAL JUGANDO (Día 1, Planta Baja, con Valeria)
//  En vez de leer, el jugador aprende haciendo. Un aviso corto arriba
//  dice qué tecla presionar y una flecha amarilla marca adónde ir.
//    1 Moverse   2 Correr   3 Hablar con alguien   4 Cuaderno (Tab)
//    5 Mapa (Q)  6 Esconderse en un casillero
//    → Susto: el Pepino pasa frente al casillero... y se va.
//    → Explicación del bucle en 2 líneas y el objetivo del día.
//  Mientras dura, el reloj no avanza. Esc lo salta.
//  Sale la primera vez que se juega y siempre en dificultad Cachimbo.
// =====================================================================
public class Tutorial : MonoBehaviour
{
    const string Clave = "fpd_tutorial_hecho";

    Juego j;
    Text txtTecla, txtTexto, txtPaso;
    RectTransform caja;
    SpriteRenderer flecha;
    Transform objetivo;
    bool usado, saltar, ocupadoAntes;
    float t;

    public static bool Toca(Juego j)
    {
        if (j == null || j.tipoEscena != TipoEscena.Piso || j.piso != 0 || j.jugador == null) return false;
        if (Protagonista.EsMateo || Memoria.Dia != 1) return false;
        bool hecho = false;
        try { hecho = PlayerPrefs.GetInt(Clave, 0) == 1; } catch { }
        return !hecho || Dificultad.Actual == Nivel.Cachimbo;
    }

    public static IEnumerator Jugar(Juego j)
    {
        GameObject go = new GameObject("Tutorial", typeof(RectTransform));
        go.transform.SetParent(j.ui.transform, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = rt.offsetMax = Vector2.zero;
        Tutorial tu = go.AddComponent<Tutorial>();
        tu.j = j;
        tu.Armar();
        Interactuable.AlUsarCualquiera += tu.AlUsar;
        j.RelojDetenido = true;
        try
        {
            yield return j.StartCoroutine(tu.Pasos());
        }
        finally
        {
            Interactuable.AlUsarCualquiera -= tu.AlUsar;
            j.RelojDetenido = false;
            if (tu.flecha != null) Destroy(tu.flecha.gameObject);
            Destroy(go);
            try { PlayerPrefs.SetInt(Clave, 1); PlayerPrefs.Save(); } catch { }
        }
        j.ui.Toast("OBJETIVO: consigue los 4 sellos y entra al Auditorio a las 18:00.  (Q: mapa con tus misiones)", 7f);
    }

    void Armar()
    {
        caja = Util.PanelArte(transform, "Aviso", new Color(0.05f, 0.04f, 0.08f, 0.92f), Util.Hex("#ffd166"),
                              new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-480, -250), new Vector2(480, -110));
        txtTecla = Util.TextoUI(caja, "", 46, Util.Hex("#ffd166"), TextAnchor.UpperCenter, Vector2.zero, Vector2.one, new Vector2(10, 0), new Vector2(-10, -12));
        txtTecla.fontStyle = FontStyle.Bold;
        txtTexto = Util.TextoUI(caja, "", 28, Color.white, TextAnchor.LowerCenter, Vector2.zero, Vector2.one, new Vector2(16, 16), new Vector2(-16, 0));
        txtPaso = Util.TextoUI(caja, "", 18, new Color(1, 1, 1, 0.5f), TextAnchor.LowerRight, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, -30), new Vector2(-6, -4));

        GameObject gf = new GameObject("Flecha del tutorial");
        flecha = gf.AddComponent<SpriteRenderer>();
        flecha.sprite = SpriteFlecha();
        flecha.sortingOrder = 32000;
        gf.SetActive(false);
    }

    static Sprite SpriteFlecha()
    {
        Lienzo L = new Lienzo(13, 12);
        Color c = Util.Hex("#ffd166"), b = Util.Hex("#3a2a00");
        for (int y = 0; y < 7; y++) L.R(6 - y, y, 6 + y, y, c);
        L.R(4, 7, 8, 11, c);
        L.Borde(b);
        return Arte.Crear("flecha_tutorial", L, Arte.PivotPies);
    }

    void Mostrar(string tecla, string texto, int paso)
    {
        txtTecla.text = tecla;
        txtTexto.text = texto;
        txtPaso.text = paso > 0 ? "Tutorial " + paso + "/6   ·   Esc: saltar" : "Esc: saltar";
        Sonido.Efecto("blip", 0.5f, 1.3f);
        t = 0f;
    }

    void AlUsar(Interactuable i) { usado = i != null && i.Texto.StartsWith("Hablar"); }

    void Update()
    {
        t += Time.unscaledDeltaTime;
        // Esc salta el tutorial (pero no cuando se usa para cerrar el mapa, el cuaderno o un diálogo)
        bool ocupado = Cine.Activa || MiniMapa.HayUnoAbierto || j.ui.CuadernoAbierto || j.Bloqueado;
        if (Entrada.Bajada(Tecla.Saltar) && !ocupado && !ocupadoAntes) saltar = true;
        ocupadoAntes = ocupado;
        // La caja late un poco al aparecer cada paso
        float s = 1f + Mathf.Max(0f, 0.12f - t * 0.4f);
        caja.localScale = new Vector3(s, s, 1f);
        caja.gameObject.SetActive(!Cine.Activa && !j.ui.PantallaVisible);

        if (objetivo != null && objetivo.gameObject.activeInHierarchy)
        {
            flecha.gameObject.SetActive(true);
            flecha.transform.position = objetivo.position + new Vector3(0f, 1.6f + Mathf.Abs(Mathf.Sin(t * 4f)) * 0.35f, 0f);
            flecha.transform.localScale = new Vector3(1f, -1f, 1f);       // apunta hacia abajo
        }
        else flecha.gameObject.SetActive(false);
    }

    // Interactuable disponible más cercano cuyo texto empiece con...
    Transform MasCercano(string empieza)
    {
        Interactuable mejor = null; float d = 1e9f;
        foreach (Interactuable i in Interactuable.Todos)
        {
            if (i == null || !i.Disponible || !i.Texto.StartsWith(empieza)) continue;
            float di = Vector2.Distance(i.transform.position, j.jugador.Posicion);
            if (di < d) { d = di; mejor = i; }
        }
        return mejor != null ? mejor.transform : null;
    }

    IEnumerator Esperar(System.Func<bool> listo)
    {
        while (!listo() && !saltar) yield return null;
        if (!saltar)
        {
            Sonido.Efecto("sello", 0.4f, 1.6f);
            txtTecla.text = "<color=#7bd389>¡BIEN!</color>";
            float h = 0f;
            while (h < 0.7f && !saltar) { h += Time.unscaledDeltaTime; yield return null; }
        }
    }

    IEnumerator Pasos()
    {
        Jugador pj = j.jugador;

        // 1. Moverse
        Mostrar("W A S D  o  FLECHAS", "Muévete por la Planta Baja.", 1);
        Vector2 ini = pj.Posicion;
        yield return Esperar(() => Vector2.Distance(ini, pj.Posicion) > 2.5f);

        // 2. Correr
        Mostrar("mantén  SHIFT", "Corre mientras te mueves. Te va a salvar la vida.", 2);
        float corriendo = 0f;
        yield return Esperar(() =>
        {
            if (Entrada.Presionada(Tecla.Correr) && Entrada.Movimiento().sqrMagnitude > 0.1f) corriendo += Time.deltaTime;
            return corriendo > 0.8f;
        });

        // 3. Hablar
        usado = false;
        objetivo = MasCercano("Hablar");
        Mostrar("acércate y presiona  E", "Habla con alguien. Sigue la flecha amarilla.", 3);
        float refrescar = 0f;
        yield return Esperar(() =>
        {
            refrescar -= Time.deltaTime;
            if (refrescar <= 0f) { refrescar = 0.5f; objetivo = MasCercano("Hablar"); }
            return usado && !j.Bloqueado;
        });
        objetivo = null;

        // 4. Cuaderno
        Mostrar("TAB", "Abre tu cuaderno. Lo que descubres aquí NO se borra nunca.", 4);
        yield return Esperar(() => j.ui.CuadernoAbierto);
        Mostrar("TAB", "Ciérralo otra vez.", 4);
        yield return Esperar(() => !j.ui.CuadernoAbierto);

        // 5. Mapa
        Mostrar("Q", "Abre el mapa: ahí ves tus misiones y dónde estás.", 5);
        yield return Esperar(() => MiniMapa.HayUnoAbierto);
        Mostrar("Q", "Las salas en AMARILLO son tus misiones. Cierra el mapa.", 5);
        yield return Esperar(() => !MiniMapa.HayUnoAbierto);

        // 6. Esconderse
        objetivo = MasCercano("Esconderte");
        Mostrar("acércate al casillero y presiona  E", "Escóndete. ¡Rápido!", 6);
        yield return Esperar(() => { if (objetivo == null) objetivo = MasCercano("Esconderte"); return pj.Escondido; });
        objetivo = null;
        if (saltar) yield break;

        // El susto: el Pepino pasa frente al casillero
        if (!string.IsNullOrEmpty(Historia.UltimoCasillero)) Memoria.Olvidar(Historia.UltimoCasillero);
        txtTecla.text = ""; txtTexto.text = "<color=#ff6b6b>Shh... no salgas.</color>";
        yield return j.StartCoroutine(PasaElPepino(pj.Posicion));

        // El bucle, en dos líneas
        Mostrar("<color=#ff4d4d>EL PEPINO</color>", "Si te atrapa, mueres y el día vuelve a empezar a las 6:45.\nPierdes los sellos, pero no lo que aprendiste.", 0);
        float h2 = 0f;
        while (h2 < 2f && !saltar) { h2 += Time.unscaledDeltaTime; yield return null; }
        txtPaso.text = "E: continuar";
        while (!Entrada.Avanzar() && !saltar) yield return null;
        if (pj.Escondido) Mostrar("E", "Sal del casillero cuando quieras.", 0);
        while (pj.Escondido && !saltar) yield return null;
    }

    IEnumerator PasaElPepino(Vector2 casillero)
    {
        Cine c = Cine.Empezar(false);
        SpritesPersonaje sp = Arte.Pepino();
        float y = casillero.y - 1.3f;
        SpriteRenderer pep = c.Actor("Pepino", sp, new Vector2(casillero.x + 9f, y));
        Sonido.Musica(null, 0.4f);
        Sonido.Efecto("apagon", 0.5f, 1f);
        yield return c.Parpadeo(2, 0.15f);
        Sonido.Latido(0.7f);
        yield return c.CaminarLado(pep, sp, casillero.x + 0.4f, 2.2f);
        // Se detiene frente al casillero...
        pep.sprite = sp.abajo[0];
        Sonido.Efecto("respiracion", 0.6f, 0.8f);
        yield return c.Esperar(1.4f);
        pep.flipX = !pep.flipX;
        yield return c.Esperar(0.6f);
        // ...y sigue de largo
        yield return c.CaminarLado(pep, sp, casillero.x - 10f, 2.6f);
        Sonido.Latido(0f);
        yield return c.Terminar();
    }
}
