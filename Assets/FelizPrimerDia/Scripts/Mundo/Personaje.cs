using System;
using UnityEngine;

// Personajes no jugables (NPC). Tienen sprite, nombre encima y se puede hablar con ellos.
public class Personaje : MonoBehaviour
{
    public string nombre;
    public SpriteRenderer sr;
    public Interactuable interaccion;
    public AnimadorPersonaje animador;
    TextMesh etiqueta;

    public static Personaje Crear(Transform padre, string nombre, Vector2 pos, SpritesPersonaje sprites, Action alHablar)
    {
        Horno.Suspender();
        GameObject go = new GameObject(nombre);
        go.transform.SetParent(padre, false);
        go.transform.position = pos;
        go.layer = Util.CapaSinVision;

        Personaje p = go.AddComponent<Personaje>();
        p.nombre = nombre;
        p.sr = go.AddComponent<SpriteRenderer>();
        Util.AplicarMaterial(p.sr);
        p.animador = Util.Animar(go, p.sr, sprites);

        CircleCollider2D c = go.AddComponent<CircleCollider2D>();
        c.radius = 0.35f;
        c.offset = new Vector2(0f, -0.35f);

        p.etiqueta = Util.Texto(nombre, go.transform, pos + new Vector2(0f, 1.05f), 0.8f, Util.Hex("#ffe8a3"), 9);

        p.interaccion = go.AddComponent<Interactuable>();
        p.interaccion.accion = "Hablar con " + nombre;
        p.interaccion.alInteractuar = alHablar;
        p.interaccion.radio = 1.5f;
        Horno.Reanudar();
        return p;
    }

    public void Mostrar(bool visible)
    {
        gameObject.SetActive(visible);
    }

    public void MoverA(Vector2 pos) { transform.position = pos; }

    public void CambiarNombre(string nuevo)
    {
        nombre = nuevo;
        if (etiqueta != null) etiqueta.text = nuevo;
    }
}
