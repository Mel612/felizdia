using UnityEngine;

// =====================================================================
//  EL CUARTO SECRETO DIBUJADO EN LA ESCENA (7_CuartoSecreto)
//  La oficina, las persianas y los disfraces en las puertas son objetos
//  de la escena: los puedes ver y mover en el editor. El minijuego
//  (NocheMonitoreo) los usa al jugar. El monitor de cámaras es interfaz.
// =====================================================================
public class MundoCuarto
{
    public const float Zoom = 4.22f;                          // la oficina (15 x 8.4) llena la pantalla 16:9
    public static readonly Vector2 Centro = Vector2.zero;
    public static MundoCuarto Actual;

    public SpriteRenderer oficina, persianaIzq, persianaDer, enPuertaIzq, enPuertaDer, luz;

    // Medidas del dibujo de la oficina (240 x 135 px a 16 px por unidad)
    static Vector2 Px(float x, float y) { return Centro + new Vector2((x - 120f) / 16f, (y - 67.5f) / 16f); }

    public static MundoCuarto Construir(Transform raiz)
    {
        MundoCuarto m = new MundoCuarto();
        Horno.Empezar("Cuarto");
        try
        {
            GameObject g = new GameObject("Cuarto secreto");
            g.transform.SetParent(raiz, false);
            Transform t = g.transform;
            m.oficina = Util.Plano("Oficina (cuarto de monitoreo)", t, Arte.Oficina(), Px(120f, 67.5f), -100);

            // Disfraces parados en las puertas (detrás de las persianas)
            m.enPuertaIzq = Util.Plano("Maniquí en la puerta izquierda", t, Arte.Maniqui().Frente, Px(30f, 69f), -90);
            m.enPuertaDer = Util.Plano("Pepino en la puerta derecha", t, Arte.Pepino().Frente, Px(210f, 69f), -90);
            m.enPuertaIzq.transform.localScale = m.enPuertaDer.transform.localScale = new Vector3(3.3f, 3.3f, 1f);
            m.enPuertaIzq.color = m.enPuertaDer.color = new Color(0.55f, 0.55f, 0.6f, 1f);

            // Persianas metálicas
            m.persianaIzq = Util.Plano("Persiana izquierda", t, Arte.Persiana(), Px(30f, 69f), -80);
            m.persianaDer = Util.Plano("Persiana derecha", t, Arte.Persiana(), Px(210f, 69f), -80);
            // En el editor se ve la puerta izquierda abierta (con el Maniquí) y la derecha cerrada
            if (Horno.Horneando) m.persianaIzq.enabled = false;

            // Luz de la lámpara
            m.luz = Util.Plano("Luz de la lámpara", t, Arte.Luz(), Px(120f, 100f), -70);
            m.luz.color = new Color(1f, 0.95f, 0.75f, 0.18f);
            m.luz.transform.localScale = new Vector3(6f, 5f, 1f);
        }
        finally { Horno.Terminar(); }
        Actual = m;
        return m;
    }
}
