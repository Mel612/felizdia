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
    bool investigando;         // escuchó un ruido (una botella, o a Valeria corriendo) y va a ver
    Vector2 puntoRuido;
    float esperaRuido;
    static float tAvisoOido = -100f;

    // Un ruido en el mundo: los Pepinos cercanos del mismo piso van a ver qué fue.
    // fuerte = una botella que se rompe: distrae incluso al que te está persiguiendo si te perdió de vista.
    public static int Ruido(Vector2 pos, float radio, bool fuerte)
    {
        int n = 0;
        foreach (Pepino p in Todos)
        {
            if (p == null || p.enPersecucion || p.rb == null || !p.sr.enabled) continue;
            if (Mapa.PisoDe(p.rb.position) != Mapa.PisoDe(pos)) continue;
            if (Vector2.Distance(p.rb.position, pos) > radio) continue;
            if (p.estado == Estado.Caza)
            {
                if (!fuerte) continue;
                p.estado = Estado.Patrulla;
            }
            p.investigando = true;
            p.puntoRuido = pos;
            p.esperaRuido = 0f;
            p.revisando = false;
            n++;
        }
        return n;
    }

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
        Sonido.Distorsion(cerca * 0.85f);
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

        // Si corres cerca, te escucha (aunque no te vea) y viene a ver
        if (!teVe && estado != Estado.Caza && mismoPiso && jugador.Corriendo &&
            Vector2.Distance(yo, pj) < 4.5f * Dificultad.VisionPepino)
        {
            if (!investigando && Time.time - tAvisoOido > 8f)
            {
                tAvisoOido = Time.time;
                Juego.I.ui.Toast("El Pepino te ESCUCHÓ correr... Camina despacio cerca de él.", 3f);
            }
            investigando = true;
            puntoRuido = pj;
            esperaRuido = 0f;
        }

        Vector2 destino;
        float vel;
        if (estado == Estado.Caza)
        {
            investigando = false;
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
        else if (investigando)
        {
            // Va a ver el ruido, mira un rato y vuelve a su ruta
            vel = velocidadPatrulla * 1.45f;
            destino = puntoRuido;
            if (Vector2.Distance(yo, puntoRuido) < 0.6f)
            {
                vel = 0f;
                esperaRuido += Time.fixedDeltaTime;
                if (sr != null && Mathf.Repeat(esperaRuido, 0.6f) < Time.fixedDeltaTime) sr.flipX = !sr.flipX;
                if (esperaRuido > 1.8f) { investigando = false; indiceRuta = RutaMasCercana(yo); }
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
        alerta.text = estado == Estado.Caza ? "!" : "?";
        alerta.gameObject.SetActive(estado == Estado.Caza || investigando);
    }

    // Te alcanzó: empieza la persecución de lado. Si llegas a las escaleras, te salvas.
    IEnumerator Atrapada()
    {
        enPersecucion = true;
        int piso = Mapa.PisoDe(rb.position);
        Juego.I.ui.Golpe(0.5f);

        // Forcejeo: si presionas E rápido, te sueltas y sigues explorando
        Resultado<bool> libre = new Resultado<bool>();
        yield return Forcejeo.Jugar(Juego.I, transform, libre);
        if (libre.valor)
        {
            enPersecucion = false;
            estado = Estado.Patrulla;
            investigando = false;
            indiceRuta = RutaMasCercana(rb.position);
            aturdidoHasta = Time.time + 3.5f;
            alerta.gameObject.SetActive(false);
            Juego.I.ui.Toast("¡TE SOLTASTE! Lo dejaste aturdido unos segundos. Corre y escóndete.", 3.5f);
            yield break;
        }
        yield return Juego.I.dialogo.DecirCo("Pepino", "(No pudiste soltarte... pero le pisas el pie y te zafas un segundo.)", "¡CORRE!");
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

    // El flash del celular: lo ciega unos segundos y pierde tu rastro
    public bool Cegar(Vector2 desde, Vector2 mirando)
    {
        if (enPersecucion || rb == null || !sr.enabled) return false;
        Vector2 a = rb.position - desde;
        float d = a.magnitude;
        if (d > 6f) return false;
        if (d > 2.2f && Vector2.Dot(a.normalized, mirando) < 0.35f) return false;   // tiene que estar adelante
        if (Physics2D.Linecast(desde, rb.position).collider != null) return false;   // una pared en medio
        aturdidoHasta = Time.time + 3.5f;
        estado = Estado.Patrulla;
        investigando = false;
        revisando = false;
        indiceRuta = RutaMasCercana(rb.position);
        alerta.text = "*";
        alerta.gameObject.SetActive(true);
        StartCoroutine(BlancoUnInstante());
        return true;
    }

    IEnumerator BlancoUnInstante()
    {
        Color antes = sr.color;
        sr.color = new Color(1f, 1f, 1f, 1f);
        yield return new WaitForSeconds(0.15f);
        sr.color = EsQuinto ? new Color(1f, 0.82f, 0.45f) : Color.white;
        yield return new WaitForSeconds(3.3f);
        if (alerta != null && estado != Estado.Caza) alerta.gameObject.SetActive(false);
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
