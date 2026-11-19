using UnityEngine;

// Animación de caminar en 4 direcciones (abajo, arriba, lado + espejo).
public class AnimadorPersonaje : MonoBehaviour
{
    public SpritesPersonaje sprites;
    public float velocidadPasos = 8f;

    SpriteRenderer sr;
    Vector2 ultima;
    float t;
    int dir;          // 0 abajo, 1 arriba, 2 lado
    bool primero = true;

    public void Iniciar(SpritesPersonaje s, SpriteRenderer r)
    {
        sprites = s;
        sr = r;
        sr.sprite = s.Frente;
    }

    public void CambiarSprites(SpritesPersonaje s)
    {
        sprites = s;
        if (sr != null) sr.sprite = s.Frente;
    }

    public void Mirar(int direccion, bool izquierda)
    {
        dir = direccion;
        if (sr != null) sr.flipX = direccion == 2 && izquierda;
    }

    void LateUpdate()
    {
        if (sr == null || sprites == null) return;
        Vector2 pos = transform.position;
        if (primero) { ultima = pos; primero = false; }
        Vector2 d = pos - ultima;
        ultima = pos;
        float dt = Mathf.Max(Time.deltaTime, 0.0001f);
        Vector2 v = d / dt;
        bool camina = v.sqrMagnitude > 0.2f && v.sqrMagnitude < 400f;   // ignora teletransportes

        if (camina)
        {
            if (Mathf.Abs(v.x) > Mathf.Abs(v.y) * 0.9f) { dir = 2; sr.flipX = v.x < 0f; }
            else { dir = v.y > 0f ? 1 : 0; sr.flipX = false; }
            t += dt * velocidadPasos * Mathf.Clamp(v.magnitude / 3.6f, 0.6f, 1.6f);
        }
        else t = 0f;

        int frame = camina ? 1 + ((int)t) % 2 : 0;
        Sprite[] set = dir == 0 ? sprites.abajo : dir == 1 ? sprites.arriba : sprites.lado;
        sr.sprite = set[frame];
    }
}
