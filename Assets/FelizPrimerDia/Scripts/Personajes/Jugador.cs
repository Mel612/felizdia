using System.Collections.Generic;
using UnityEngine;

// Valeria. Movimiento 2D visto desde arriba (top-down).
public class Jugador : MonoBehaviour
{
    public float velocidad = 3.6f;
    public float velocidadCorrer = 5.4f;

    public bool Escondido { get; private set; }
    public Vector2 Posicion { get { return rb != null ? rb.position : (Vector2)transform.position; } }
    public int Piso { get { return Mapa.PisoDe(Posicion); } }

    // Rastro de posiciones recientes: el Pepino lo sigue cuando te pierde de vista.
    public readonly List<Vector2> Rastro = new List<Vector2>();

    Rigidbody2D rb;
    SpriteRenderer sr;
    Collider2D col;
    Vector2 entrada;
    public Vector2 Mirando { get; private set; } = Vector2.right;     // última dirección en que se movió
    public bool Corriendo { get; private set; }
    float tRastro;
    Interactuable cercano;

    public static Jugador Crear(Transform padre, Vector2 pos)
    {
        Horno.Suspender();
        GameObject go = new GameObject(Protagonista.Nombre + " (Jugador)");
        go.transform.SetParent(padre, false);
        go.transform.position = pos;
        go.layer = Util.CapaSinVision;
        go.tag = "Player";

        Jugador j = go.AddComponent<Jugador>();
        j.sr = go.AddComponent<SpriteRenderer>();
        Util.AplicarMaterial(j.sr);
        Util.Animar(go, j.sr, Protagonista.Sprites);

        j.rb = go.AddComponent<Rigidbody2D>();
        j.rb.bodyType = RigidbodyType2D.Dynamic;
        j.rb.gravityScale = 0f;
        j.rb.freezeRotation = true;
        j.rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        j.rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        CircleCollider2D c = go.AddComponent<CircleCollider2D>();
        c.radius = 0.32f;
        c.offset = new Vector2(0f, -0.4f);
        j.col = c;
        Horno.Reanudar();
        return j;
    }

    public Collider2D Colisionador { get { return col; } }

    void Update()
    {
        Juego juego = Juego.I;
        if (juego == null) return;

        bool puedeMoverse = !juego.Bloqueado && !Escondido;
        entrada = puedeMoverse ? Entrada.Movimiento() : Vector2.zero;

        // Rastro
        tRastro += Time.deltaTime;
        if (tRastro > 0.25f && !Escondido)
        {
            tRastro = 0f;
            Rastro.Add(Posicion);
            if (Rastro.Count > 40) Rastro.RemoveAt(0);
        }

        // Interacción
        cercano = juego.Bloqueado ? null : Interactuable.MasCercano(Posicion);
        juego.ui.Prompt(cercano != null ? "[E] " + cercano.Texto : null);
        if (cercano != null && juego.PuedeInteractuar && Entrada.Bajada(Tecla.Interactuar))
            cercano.Usar();
    }

    float tPaso;

    void FixedUpdate()
    {
        bool corre = Entrada.Presionada(Tecla.Correr);
        float v = corre ? velocidadCorrer : velocidad;
        if (entrada.sqrMagnitude > 0.01f) Mirando = entrada.normalized;
        Corriendo = corre && entrada.sqrMagnitude > 0.01f && !Escondido;
        rb.linearVelocity = entrada * v;

        // Pasos
        if (entrada.sqrMagnitude > 0.01f)
        {
            tPaso -= Time.fixedDeltaTime;
            if (tPaso <= 0f)
            {
                tPaso = corre ? 0.26f : 0.38f;
                Sonido.Efecto("paso", corre ? 0.35f : 0.22f, Random.Range(0.9f, 1.1f));
            }
        }
        else tPaso = 0f;
    }

    public void Esconder(bool esconder)
    {
        Escondido = esconder;
        sr.color = esconder ? new Color(1f, 1f, 1f, 0.25f) : Color.white;
        if (esconder) rb.linearVelocity = Vector2.zero;
    }

    public void Teletransportar(Vector2 pos)
    {
        rb.linearVelocity = Vector2.zero;
        rb.position = pos;
        transform.position = pos;
        Rastro.Clear();
    }
}
