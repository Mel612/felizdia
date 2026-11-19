using UnityEngine;

// Hace que una figura "baile": rebota y se da vuelta al ritmo de la música.
public class Bailarin : MonoBehaviour
{
    public float ritmo = 2.2f;      // golpes por segundo
    public float fase;
    Vector3 escala;
    SpriteRenderer sr;

    void Start()
    {
        escala = transform.localScale;
        sr = GetComponent<SpriteRenderer>();
        if (fase == 0f) fase = Random.Range(0f, 10f);
    }

    void Update()
    {
        float t = Time.time * ritmo * Mathf.PI + fase;
        float rebote = Mathf.Abs(Mathf.Sin(t));
        transform.localScale = new Vector3(escala.x, escala.y * (0.94f + rebote * 0.08f), escala.z);
        if (sr != null) sr.flipX = Mathf.Sin(t * 0.25f) > 0f;
    }
}
