using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// El asesino. Patrulla los pasillos; si te ve, te persigue.
// Si te pierde de vista, sigue tu rastro. Escóndete en los casilleros.
public class Pepino : MonoBehaviour
{
    public float velocidadPatrulla = 1.8f;
    public float velocidadCaza = 3.9f;
    public float distanciaVision = 6.5f;
    public float distanciaAtrapar = 0.7f;
    public float segundosParaRendirse = 4f;

    public List<Vector2> ruta = new List<Vector2>();

    enum Estado { Patrulla, Caza }
    Estado estado = Estado.Patrulla;
    int indiceRuta;
    float sinVerte;
    Vector2 ultimaVista;
    float tVisto = -10f;
    bool enPersecucion;
    float aturdidoHasta;
    bool revisando;            // va a revisar un casillero que recuerda
    Vector2 casilleroRevisar;

    public static int Cazando;      // cuántos Pepinos te están persiguiendo (para la música)
    public static readonly List<Pepino> Todos = new List<Pepino>();
    public bool EsQuinto { get; private set; }       // historia de Mateo: Sebastián con el quinto disfraz
    public bool Visible { get { return sr != null && sr.enabled; } }

    // Convierte a este Pepino en "el quinto disfraz" (dorado)
    public void HacerQuinto()
    {
        EsQuinto = true;
        if (sr != null) sr.color = new Color(1f, 0.82f, 0.45f);
        velocidadCaza *= 1.05f;
    }

    void OnEnable() { if (!Todos.Contains(this)) Todos.Add(this); }
    bool cazandoContado;
    float tPaso;

    Rigidbody2D rb;
    SpriteRenderer sr;
    TextMesh alerta;
    Jugador jugador;

    public static Pepino Crear(Transform padre, Vector2 pos, List<Vector2> ruta)
    {
        Horno.Suspender();
        GameObject go = new GameObject("Pepino");
        go.transform.SetParent(padre, false);
        go.transform.position = pos;
        go.layer = Util.CapaSinVision;

        Pepino p = go.AddComponent<Pepino>();
        p.ruta = ruta;
        p.sr = go.AddComponent<SpriteRenderer>();
        Util.AplicarMaterial(p.sr);
        Util.Animar(go, p.sr, Arte.Pepino());

        p.rb = go.AddComponent<Rigidbody2D>();
        p.rb.gravityScale = 0f;
        p.rb.freezeRotation = true;
        p.rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        CircleCollider2D c = go.AddComponent<CircleCollider2D>();
        c.radius = 0.32f;
        c.offset = new Vector2(0f, -0.4f);

        p.alerta = Util.Texto("!", go.transform, pos + new Vector2(0f, 1.2f), 2f, Util.Hex("#ff3b3b"), 9);
        p.alerta.gameObject.SetActive(false);
        Horno.Reanudar();
        return p;
    }

    void Start()
    {
        jugador = Juego.I.jugador;
        // El Pepino no empuja a Valeria: la atrapa.
        Collider2D mio = GetComponent<Collider2D>();
        if (jugador != null && jugador.Colisionador != null && mio != null)
            Physics2D.IgnoreCollision(mio, jugador.Colisionador);

        // Al llegar al piso, el Pepino no te espera en la puerta: empieza lejos.
        if (jugador != null && ruta != null && ruta.Count > 1 && Vector2.Distance(rb.position, jugador.Posicion) < 14f)
        {
            rb.position = ruta[1];
            transform.position = ruta[1];
            indiceRuta = 1;
        }
        // ---------- Dificultad + lo que el Pepino aprendió de tus muertes ----------
        float aprende = Dificultad.Escalada;
        float mult = Dificultad.VelPepino * (1f + Mathf.Min(0.2f, 0.03f * Memoria.Muertes * aprende));
        velocidadPatrulla *= mult;
        velocidadCaza *= mult;
        distanciaVision *= Dificultad.VisionPepino;
        segundosParaRendirse += Mathf.Min(2f, 0.25f * Memoria.Muertes * aprende);
        int piso = Mapa.PisoDe(rb.position);
        if (Partida.Evento == "apagon" && Partida.PisoApagon == piso) distanciaVision *= 0.65f;   // a oscuras ve menos
        bool vigilaEscaleras = aprende > 0f && Memoria.Escapes >= 2;
        if (vigilaEscaleras && ruta != null) ruta.Add(Mapa.LlegadaEscaleras(piso));

        float aturdido = Partida.PepinoAturdido ? (vigilaEscaleras ? 7f : 12f) : 2f;
        aturdidoHasta = Time.time + aturdido;
        Partida.PepinoAturdido = false;
    }

    bool Activo
    {
        get
        {
            Juego j = Juego.I;
            return j != null && !j.Bloqueado && j.Hora >= j.HoraPepino;
        }
    }

    void OnDisable() { MarcarCaza(false); Todos.Remove(this); }

    void MarcarCaza(bool caza)
    {
        if (caza == cazandoContado) return;
        cazandoContado = caza;
        Cazando = Mathf.Max(0, Cazando + (caza ? 1 : -1));
    }

    // Latido y pasos según qué tan cerca está
    void SonidoCercania()
    {
        if (jugador == null || !sr.enabled || enPersecucion) { Sonido.Latido(0f); return; }
        float d = Vector2.Distance(rb.position, jugador.Posicion);
        bool mismoPiso = Mapa.PisoDe(rb.position) == jugador.Piso;
        float cerca = mismoPiso ? Mathf.Clamp01(1f - (d - 1.5f) / 9f) : 0f;
        if (estado == Estado.Caza) cerca = Mathf.Max(cerca, 0.75f);
        Sonido.Latido(cerca);
        if (cerca > 0.05f && rb.linearVelocity.sqrMagnitude > 0.1f)
        {
            tPaso -= Time.deltaTime;
            if (tPaso <= 0f)
            {
                tPaso = estado == Estado.Caza ? 0.3f : 0.55f;
                Sonido.Efecto("paso_pepino", 0.15f + cerca * 0.5f, Random.Range(0.85f, 0.95f));
            }
        }
    }

    void Update()
    {
        MarcarCaza(estado == Estado.Caza && sr.enabled);
        SonidoCercania();
        // Solo se muestra cuando ya es hora de que aparezca.
        Juego j = Juego.I;
        bool visible = j != null && j.Hora >= j.HoraPepino;
        if (visible && jugador != null && !Memoria.Sabe("cine_pepino") && !Cine.Activa && !j.Bloqueado &&
            Mapa.PisoDe(rb.position) == jugador.Piso && Vector2.Distance(rb.position, jugador.Posicion) < 11f)
        {
            aturdidoHasta = Time.time + 3f;
            StartCoroutine(CineHistoria.PrimerPepino(j, transform));
        }
        if (sr.enabled != visible)
        {
            sr.enabled = visible;
            Transform sombra = transform.Find("Sombra");
            if (sombra != null) sombra.gameObject.SetActive(visible);
            Collider2D c = GetComponent<Collider2D>();
            if (c != null) c.enabled = visible;
            if (!visible) alerta.gameObject.SetActive(false);
        }
    }

    bool PuedeVerA(Vector2 objetivo)
    {
        Vector2 yo = rb.position;
        if (Vector2.Distance(yo, objetivo) > distanciaVision) return false;
        // Solo las paredes (capa Default) bloquean la vista.
        return Physics2D.Linecast(yo, objetivo).collider == null;
    }

    void FixedUpdate()
    {
        if (!Activo || jugador == null || enPersecucion || Time.time < aturdidoHasta)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 yo = rb.position;
        Vector2 pj = jugador.Posicion;
        bool mismoPiso = Mapa.PisoDe(yo) == Mapa.PisoDe(pj);
        bool teVe = mismoPiso && !jugador.Escondido && PuedeVerA(pj);

        if (teVe)
        {
            if (estado != Estado.Caza)
            {
                Juego.I.ui.Toast((EsQuinto ? "¡El quinto disfraz te vio!" : "¡El Pepino te vio!") + " Corre (Shift) y escóndete en un casillero.", 3f);
                Juego.I.ui.Golpe(0.25f);
                Sonido.Efecto("susto", 0.7f, 1.1f);
            }
            estado = Estado.Caza;
            Partida.VistaHoy = true;
            revisando = false;
            sinVerte = 0f;
            ultimaVista = pj;
            tVisto = Time.time;
        }

        // Si te escondes en sus narices, te encuentra.
        if (jugador.Escondido && estado == Estado.Caza && Time.time - tVisto < 0.6f && Vector2.Distance(yo, pj) < 2.2f)
        {
            Juego.I.Morir("Te vio meterte al casillero. Abrió la puerta despacio... y sonrió.", null, TipoMuerte.Cuchillo);
            return;
        }

        Vector2 destino;
        float vel;
        if (estado == Estado.Caza)
        {
            vel = velocidadCaza;
            if (teVe) destino = pj;
            else
            {
                sinVerte += Time.fixedDeltaTime;
                destino = PuntoDeRastro(yo);
                if (sinVerte > segundosParaRendirse || jugador.Escondido && sinVerte > 1.5f)
                {
                    estado = Estado.Patrulla;
                    indiceRuta = RutaMasCercana(yo);
                }
            }

            if (!jugador.Escondido && mismoPiso && Vector2.Distance(yo, pj) < distanciaAtrapar)
            {
                rb.linearVelocity = Vector2.zero;
                StartCoroutine(Atrapada());
                return;
            }
        }
        else if (revisando)
        {
            // Va directo al casillero que recuerda
            vel = velocidadPatrulla * 1.35f;
            destino = casilleroRevisar;
            if (Vector2.Distance(yo, casilleroRevisar) < 0.9f)
            {
                revisando = false;
                if (jugador.Escondido && Vector2.Distance(pj, casilleroRevisar) < 1f)
                {
                    rb.linearVelocity = Vector2.zero;
                    Juego.I.Morir("Abrió tu casillero sin dudar.\n\"Ayer también te escondiste aquí\", susurró.", "pepino_aprende", TipoMuerte.Cuchillo);
                    return;
                }
                indiceRuta = RutaMasCercana(yo);
            }
        }
        else
        {
            vel = velocidadPatrulla;
            if (ruta == null || ruta.Count == 0) { rb.linearVelocity = Vector2.zero; return; }
            destino = ruta[indiceRuta];
            if (Vector2.Distance(yo, destino) < 0.3f) indiceRuta = (indiceRuta + 1) % ruta.Count;
        }

        Vector2 dir = destino - yo;
        rb.linearVelocity = dir.sqrMagnitude > 0.01f ? dir.normalized * vel : Vector2.zero;
        alerta.gameObject.SetActive(estado == Estado.Caza);
    }

    // Te alcanzó: empieza la persecución de lado. Si llegas a las escaleras, te salvas.
    IEnumerator Atrapada()
    {
        enPersecucion = true;
        int piso = Mapa.PisoDe(rb.position);
        Juego.I.ui.Golpe(0.5f);
        yield return Juego.I.dialogo.DecirCo("Pepino", "(Te agarra de la mochila. Te sueltas de un tirón...)", "¡CORRE!");
        // La persecución de lado es otra escena (5_Persecuciones)
        if (Partida.PuedeCargar(Partida.EscenaPersecuciones))
        {
            Partida.IrAPersecucion(piso);
            yield break;
        }
        // Sin esa escena: se juega aquí mismo
        Resultado<bool> r = new Resultado<bool>();
        yield return Persecucion.Jugar(EscenarioPersecucion.Pasillo, piso, r);
        enPersecucion = false;
        if (!r.valor)
        {
            Juego.I.Morir("El Pepino te alcanzó al final del pasillo. Lo último que viste fue su sonrisa pintada.", null, TipoMuerte.Cuchillo);
            yield break;
        }
        jugador.Teletransportar(Mapa.LlegadaEscaleras(piso));
        rb.position = ruta != null && ruta.Count > 1 ? ruta[1] : rb.position + new Vector2(30f, 0f);
        transform.position = rb.position;
        estado = Estado.Patrulla;
        indiceRuta = 1;
        aturdidoHasta = Time.time + 12f;
        alerta.gameObject.SetActive(false);
        Juego.I.ui.Toast("¡Escapaste! Llegaste a las escaleras. El Pepino perdió tu rastro... por ahora.", 4f);
    }

    // El jugador se metió en un casillero que el Pepino recuerda de otro día.
    public bool RevisarCasillero(Vector2 pos)
    {
        Juego j = Juego.I;
        if (j == null || j.Hora < j.HoraPepino || enPersecucion) return false;
        if (Mapa.PisoDe(rb.position) != Mapa.PisoDe(pos)) return false;
        revisando = true;
        casilleroRevisar = pos + new Vector2(0f, 0.6f);
        Sonido.Efecto("susto", 0.5f, 0.8f);
        return true;
    }

    Vector2 PuntoDeRastro(Vector2 yo)
    {
        List<Vector2> r = jugador.Rastro;
        for (int i = r.Count - 1; i >= 0; i--)
            if (Physics2D.Linecast(yo, r[i]).collider == null) return r[i];
        return ultimaVista;
    }

    int RutaMasCercana(Vector2 yo)
    {
        int mejor = 0;
        float d = float.MaxValue;
        for (int i = 0; i < ruta.Count; i++)
        {
            float di = Vector2.Distance(yo, ruta[i]);
            if (di < d) { d = di; mejor = i; }
        }
        return mejor;
    }
}
