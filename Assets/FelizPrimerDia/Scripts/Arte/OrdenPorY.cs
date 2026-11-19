using UnityEngine;

// Ordena el dibujo según la altura de los pies: lo que está más abajo en pantalla se dibuja encima.
public class OrdenPorY : MonoBehaviour
{
    public bool dinamico;          // true para personajes que se mueven
    public int extra;
    SpriteRenderer sr;

    public static int Orden(float yPies) { return Mathf.RoundToInt(-yPies * 20f); }

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        Aplicar();
    }

    void LateUpdate() { if (dinamico) Aplicar(); }

    void Aplicar()
    {
        if (sr == null) return;
        sr.sortingOrder = Orden(sr.bounds.min.y) + extra;
    }
}
