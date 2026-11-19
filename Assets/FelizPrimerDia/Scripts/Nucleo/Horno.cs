using System.Collections.Generic;
using UnityEngine;

// =====================================================================
//  HORNO: une el código con la escena dibujada.
//
//  - En el editor (menú "Feliz Primer Día ▸ Construir escenas") el juego se
//    construye una vez y cada pared, mueble, cartel o texto se guarda en la
//    escena con un identificador fijo (componente IdHorneado).
//  - Al darle Play, cuando el código va a crear ese mismo objeto, primero lo
//    busca en la escena por su identificador y, si existe, USA EL DE LA ESCENA.
//    Así, si mueves una mesa o cambias un sprite en el editor, el juego lo respeta.
//
//  Los identificadores son "contexto/número" (ej. "Mapa/153"): el número es el
//  orden en que el código crea los objetos dentro de ese contexto.
// =====================================================================
public static class Horno
{
    public const string NombreRaiz = "Mundo";
    public const string NombreRaizAntiguo = "MUNDO DIBUJADO (escena)";   // escenas construidas antes

    // true mientras el editor está construyendo la escena
    public static bool Horneando;

    static Dictionary<string, GameObject> existentes;
    static readonly Stack<string> contextos = new Stack<string>();
    static readonly Stack<int> contadores = new Stack<int>();
    static int suspendido;

    // Diagnóstico: cuántos objetos se tomaron de la escena y cuántos faltaban.
    public static int Reutilizados, Faltantes;

    public static bool HayEscenaDibujada { get { return existentes != null && existentes.Count > 0; } }

    // Lee los objetos dibujados de la escena (se llama al darle Play).
    public static void Cargar(Transform raiz)
    {
        existentes = new Dictionary<string, GameObject>();
        Reutilizados = Faltantes = 0;
        contextos.Clear();
        contadores.Clear();
        suspendido = 0;
        if (raiz == null) return;
        foreach (IdHorneado id in raiz.GetComponentsInChildren<IdHorneado>(true))
            if (!string.IsNullOrEmpty(id.id) && !existentes.ContainsKey(id.id)) existentes[id.id] = id.gameObject;
    }

    public static void Olvidar()
    {
        existentes = null;
        contextos.Clear();
        contadores.Clear();
        suspendido = 0;
    }

    public static void Empezar(string contexto)
    {
        contextos.Push(contexto);
        contadores.Push(0);
    }

    public static void Terminar()
    {
        if (contextos.Count == 0) return;
        contextos.Pop();
        contadores.Pop();
    }

    // Lo que se crea mientras está suspendido (personajes, efectos) nunca se guarda ni se reutiliza.
    public static void Suspender() { suspendido++; }
    public static void Reanudar() { if (suspendido > 0) suspendido--; }

    static bool Activo { get { return contextos.Count > 0 && suspendido == 0; } }

    // Avanza el contador. Devuelve el objeto de la escena con ese id (o null si hay que crearlo).
    public static GameObject Siguiente(out string id)
    {
        id = null;
        if (!Activo) return null;
        int n = contadores.Pop();
        contadores.Push(n + 1);
        id = contextos.Peek() + "/" + n;
        GameObject g;
        if (existentes != null && existentes.TryGetValue(id, out g) && g != null)
        {
            Reutilizados++;
            return g;
        }
        if (HayEscenaDibujada) Faltantes++;
        return null;
    }

    public static T Reusar<T>(out string id) where T : Component
    {
        GameObject g = Siguiente(out id);
        if (g == null) return null;
        T c = g.GetComponent<T>();
        if (c == null) { Reutilizados--; Faltantes++; }
        return c;
    }

    public static void Marcar(GameObject go, string id)
    {
        if (id == null || !Horneando) return;
        IdHorneado m = go.GetComponent<IdHorneado>();
        if (m == null) m = go.AddComponent<IdHorneado>();
        m.id = id;
    }
}
