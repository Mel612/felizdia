using System.Collections;
using UnityEngine;

// =====================================================================
//  DISTRACCIONES
//  En los pasillos hay botellas vacías tiradas. Recógelas con E (puedes
//  llevar 2) y lánzalas con F hacia donde miras: al romperse, los Pepinos
//  cercanos van a ver qué fue... y tú pasas por el otro lado.
//  Ojo: si corres cerca del Pepino, también te escucha (ver Pepino.cs).
// =====================================================================
public class Distracciones : MonoBehaviour
{
    public const int Maximo = 2;
    const float Distancia = 5.5f;

    // Dónde hay botellas en cada piso (respecto al centro del pasillo)
    static readonly Vector2[] Lugares = { new Vector2(-4f, -2.2f), new Vector2(7f, 1.1f), new Vector2(17f, -2.2f) };

    Juego juego;
    bool lanzando;

    public static void Montar(Transform t, int piso)
    {
        GameObject go = new GameObject("Distracciones");
        go.transform.SetParent(t, false);
        Distracciones d = go.AddComponent<Distracciones>();
        d.juego = Juego.I;

        Horno.Suspender();
        for (int k = 0; k < Lugares.Length; k++)
        {
            string clave = "botella_" + piso + "_" + k;
            if (Memoria.Tiene(clave)) continue;                 // ya la recogiste hoy
            Vector2 pos = Mapa.Origen(piso) + Lugares[k];
            SpriteRenderer sr = Util.Plano("Botella vacía", go.transform, Arte.Botella(), pos, OrdenPorY.Orden(pos.y));
            Interactuable it = Interactuable.Crear(go.transform, pos, "Recoger botella (para distraer)", null, 1.1f);
            it.disponible = delegate { return Partida.Lanzables < Maximo && !Memoria.Tiene(clave); };
            it.alInteractuar = delegate
            {
                Memoria.Obtener(clave);
                Partida.Lanzables++;
                if (sr != null) sr.gameObject.SetActive(false);
                Sonido.Efecto("cristal", 0.25f, 1.6f);
                Juego.I.ui.Toast("Tienes " + Partida.Lanzables + " botella" + (Partida.Lanzables > 1 ? "s" : "") +
                                 ".  Presiona F para lanzarla: el Pepino irá a ver el ruido.", 4.5f);
            };
        }
        Horno.Reanudar();
    }

    void Update()
    {
        Juego j = juego;
        if (j == null || j.jugador == null) return;
        // La batería del celular se recupera de a poquito
        if (!j.Bloqueado) Partida.Bateria = Mathf.Min(100f, Partida.Bateria + 0.15f * Time.deltaTime);
        if (Entrada.Bajada(Tecla.Flash) && !j.Bloqueado && !j.jugador.Escondido && !Cine.Activa) Flash(j);
        if (lanzando) return;
        if (!Entrada.Bajada(Tecla.Lanzar)) return;
        if (j.Bloqueado || j.jugador.Escondido || Cine.Activa) return;
        if (Partida.Lanzables <= 0)
        {
            j.ui.Toast("No tienes nada para lanzar. Busca botellas vacías en los pasillos.", 2.5f);
            return;
        }
        Partida.Lanzables--;
        StartCoroutine(Lanzar(j.jugador.Posicion, j.jugador.Mirando));
    }

    // ---------- Flash de la cámara: ciega al Pepino 3,5 segundos ----------
    public const float CostoFlash = 25f;

    void Flash(Juego j)
    {
        if (Partida.Bateria < CostoFlash)
        {
            Sonido.Efecto("blip", 0.4f, 0.6f);
            j.ui.Toast("Batería baja (" + Mathf.FloorToInt(Partida.Bateria) + "%). El flash necesita " + (int)CostoFlash + "%.", 2.5f);
            return;
        }
        Partida.Bateria -= CostoFlash;
        Sonido.Efecto("click", 1f, 0.6f);
        Sonido.Efecto("estatica", 0.25f, 1.8f);
        j.ui.Destello(Color.white, 0.85f);
        int n = 0;
        foreach (Pepino p in Pepino.Todos.ToArray())
            if (p != null && p.Cegar(j.jugador.Posicion, j.jugador.Mirando)) n++;
        if (n > 0) j.ui.Toast("¡LO CEGASTE! Tienes unos segundos. Batería: " + Mathf.FloorToInt(Partida.Bateria) + "%", 3f);
    }

    IEnumerator Lanzar(Vector2 desde, Vector2 dir)
    {
        lanzando = true;
        if (dir.sqrMagnitude < 0.01f) dir = Vector2.right;
        dir.Normalize();
        Vector2 hasta = desde + dir * Distancia;
        RaycastHit2D h = Physics2D.Linecast(desde + dir * 0.6f, hasta);
        if (h.collider != null) hasta = h.point - dir * 0.3f;          // choca con la pared

        Sonido.Efecto("salto", 0.3f, 1.6f);
        SpriteRenderer b = Util.Plano("Botella lanzada", transform, Arte.Botella(), desde, 31000);
        float dur = 0.45f, t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            float k = t / dur;
            Vector2 p = Vector2.Lerp(desde, hasta, k) + new Vector2(0f, Mathf.Sin(k * Mathf.PI) * 1.4f);
            b.transform.position = p;
            b.transform.localRotation = Quaternion.Euler(0f, 0f, -720f * k);
            yield return null;
        }
        Destroy(b.gameObject);

        // Se rompe: vidrios y ruido
        Sonido.Efecto("cristal", 0.8f, 1f);
        SpriteRenderer vidrios = Util.Plano("Vidrios", transform, Arte.Vidrios(), hasta, -900);
        int n = Pepino.Ruido(hasta, 11f, true);
        if (n > 0) juego.ui.Toast("¡Funcionó! El Pepino fue a ver el ruido.", 2.5f);
        lanzando = false;

        float f = 0f;
        while (f < 6f) { f += Time.deltaTime; if (f > 4f) vidrios.color = new Color(1, 1, 1, 1f - (f - 4f) / 2f); yield return null; }
        Destroy(vidrios.gameObject);
    }
}
