using System;
using System.Collections.Generic;
using UnityEngine;

// Cualquier cosa con la que el jugador puede interactuar con E (personas, carteles, escondites...).
public class Interactuable : MonoBehaviour
{
    public static readonly List<Interactuable> Todos = new List<Interactuable>();

    public string accion = "Interactuar";
    public float radio = 1.3f;
    public Func<bool> disponible;       // si devuelve false, no se puede usar (ni se muestra)
    public Func<string> textoDinamico;  // texto que cambia (ej. "Esconderte" / "Salir")
    public Action alInteractuar;

    public bool Disponible { get { return isActiveAndEnabled && (disponible == null || disponible()); } }
    public string Texto { get { return textoDinamico != null ? textoDinamico() : accion; } }

    void OnEnable() { if (!Todos.Contains(this)) Todos.Add(this); }
    void OnDisable() { Todos.Remove(this); }

    public void Usar() { if (alInteractuar != null) alInteractuar(); }

    public static Interactuable Crear(Transform padre, Vector2 pos, string accion, Action alInteractuar, float radio)
    {
        GameObject go = new GameObject("Interactuable: " + accion);
        go.transform.SetParent(padre, false);
        go.transform.position = pos;
        Interactuable i = go.AddComponent<Interactuable>();
        i.accion = accion;
        i.alInteractuar = alInteractuar;
        i.radio = radio;
        return i;
    }

    public static Interactuable Crear(Transform padre, Vector2 pos, string accion, Action alInteractuar)
    {
        return Crear(padre, pos, accion, alInteractuar, 1.3f);
    }

    public static Interactuable MasCercano(Vector2 pos)
    {
        Interactuable mejor = null;
        float mejorD = float.MaxValue;
        for (int i = 0; i < Todos.Count; i++)
        {
            Interactuable it = Todos[i];
            if (it == null || !it.Disponible) continue;
            float d = Vector2.Distance(pos, it.transform.position);
            if (d <= it.radio && d < mejorD) { mejor = it; mejorD = d; }
        }
        return mejor;
    }
}
