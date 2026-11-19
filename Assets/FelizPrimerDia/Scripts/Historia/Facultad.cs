using System;
using System.Collections;
using UnityEngine;

// Base de cada facultad: guía que explica la carrera -> minijuego -> sello -> evento de terror
// -> (sobrevivir) -> prueba.
public abstract class Facultad : MonoBehaviour
{
    protected Sala sala;
    protected Juego juego;
    protected bool enEvento;
    // Se guarda en la memoria del día: si cambias de piso y vuelves, se mantiene.
    protected bool sobrevivio
    {
        get { return Memoria.Tiene(SelloId + "_sobrevivio"); }
        set { if (value) Memoria.Obtener(SelloId + "_sobrevivio"); else Memoria.Quitar(SelloId + "_sobrevivio"); }
    }
    bool ocupado;

    protected abstract string SelloId { get; }
    protected abstract string NombreCarrera { get; }

    // Color de la tinta del sello gigante
    Color ColorSello
    {
        get
        {
            if (SelloId == Ids.SelloMedicina) return Util.Hex("#2a9d8f");
            if (SelloId == Ids.SelloDerecho) return Util.Hex("#c1121f");
            if (SelloId == Ids.SelloPeriodismo) return Util.Hex("#1d4ed8");
            return Util.Hex("#7b2cbf");
        }
    }
    protected abstract void Montar();
    protected abstract IEnumerator EventoTerror();

    public void Iniciar(Sala s)
    {
        sala = s;
        juego = Juego.I;
        Horno.Empezar("Facultad_" + GetType().Name);
        try { Montar(); }
        finally { Horno.Terminar(); }
    }

    // ---------- Atajos ----------
    protected Dialogo D { get { return juego.dialogo; } }
    protected UIJuego UI { get { return juego.ui; } }
    protected Jugador J { get { return juego.jugador; } }

    protected IEnumerator Decir(string quien, params string[] lineas) { return D.DecirCo(quien, lineas); }

    protected IEnumerator Elegir(string quien, string pregunta, string[] opciones, Resultado<int> r)
    {
        return D.ElegirCo(quien, pregunta, opciones, r);
    }

    // Ejecuta una corrutina evitando que se dispare dos veces a la vez.
    protected void Lanzar(IEnumerator rutina)
    {
        if (ocupado || juego.Terminado) return;
        StartCoroutine(Envolver(rutina));
    }

    IEnumerator Envolver(IEnumerator rutina)
    {
        ocupado = true;
        yield return StartCoroutine(rutina);
        ocupado = false;
    }

    // ---------- Flujo común del guía ----------
    protected IEnumerator FlujoGuia(string guia, string[] explicacion, Func<Minijuego> crearMinijuego, string idDominio)
    {
        if (enEvento) yield break;
        if (Memoria.Tiene(SelloId))
        {
            yield return Decir(guia, "Ya tienes el sello de " + NombreCarrera + ". ¡Suerte con los demás!");
            yield break;
        }

        // Indumentaria obligatoria para entrar al laboratorio (misiones)
        if (!Misiones.TienePrenda(SelloId))
        {
            yield return Misiones.Rechazo(D, guia, SelloId);
            yield break;
        }

        yield return Decir(guia, explicacion);

        bool saltar = false;
        if (Memoria.Sabe(idDominio))
        {
            Resultado<int> r = new Resultado<int>();
            yield return Elegir("Valeria", "(Siento que ya hice esto antes... muchas veces.)",
                new string[] { "Hacer la prueba otra vez", "\"Ya sé cómo es.\"  (saltar y ahorrar tiempo)" }, r);
            saltar = r.valor == 1;
        }

        if (saltar)
        {
            yield return Decir(guia, "¿Qué? ...Lo hiciste perfecto sin que te explique nada. Qué miedo, jaja. Toma tu sello.");
        }
        else
        {
            Minijuego mj = crearMinijuego();
            Resultado<bool> ok = new Resultado<bool>();
            yield return mj.Jugar(ok);
            if (!ok.valor)
            {
                yield return Decir(guia, "Casi. Respira e inténtalo otra vez cuando quieras.");
                yield break;
            }
            Memoria.Aprender(idDominio);
            yield return Decir(guia, "¡Muy bien! Aquí tienes tu sello de " + NombreCarrera + ".");
        }

        Memoria.Obtener(SelloId);
        int sellosHoy = Memoria.Cuantos(Ids.Sellos);
        UI.SelloGrande(NombreCarrera, sellosHoy, ColorSello);
        UI.Toast(sellosHoy >= 4 ? "¡PASAPORTE COMPLETO! Entra al Auditorio a las 18:00." : "SELLO OBTENIDO: " + NombreCarrera + "  (" + sellosHoy + "/4)", 4f);
        UI.Destello(new Color(1f, 0.82f, 0.4f), 0.3f);

        enEvento = true;
        Sonido.Efecto("apagon", 0.7f, 1f);
        yield return StartCoroutine(EventoTerror());
        enEvento = false;
    }

    // ---------- Ayudas para los eventos ----------

    // Espera hasta que pase el tiempo (solo corre cuando el juego no está pausado) o hasta que listo() sea true.
    protected IEnumerator Ventana(float segundos, Func<bool> listo)
    {
        float t = 0f;
        segundos *= Dificultad.FactorTiempo;
        while (t < segundos && !listo())
        {
            if (juego.Terminado) yield break;
            if (!juego.Bloqueado) t += Time.deltaTime;
            yield return null;
        }
    }

    protected SpriteRenderer Figura(string nombre, Vector2 pos, Sprite sprite)
    {
        Horno.Suspender();
        GameObject go = new GameObject(nombre);
        go.transform.SetParent(transform, false);
        go.transform.position = pos;
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        Util.AplicarMaterial(sr);
        go.AddComponent<OrdenPorY>().dinamico = true;
        Util.Plano("Sombra", go.transform, Arte.Sombra(), pos + new Vector2(0f, -0.72f), -900);
        Horno.Reanudar();
        return sr;
    }

    protected SpriteRenderer Obj(Sprite sprite, Vector2 base_, Vector2 colision)
    {
        return Util.Objeto(sprite.name, transform, sprite, base_, colision);
    }

    protected SpriteRenderer Obj(Sprite sprite, Vector2 base_) { return Obj(sprite, base_, Vector2.zero); }

    protected IEnumerator Abalanzarse(SpriteRenderer figura)
    {
        juego.Bloquear(this);
        Vector3 a = figura.transform.position;
        Vector3 b = J.transform.position;
        float t = 0f;
        while (t < 0.35f)
        {
            t += Time.deltaTime;
            figura.transform.position = Vector3.Lerp(a, b, t / 0.35f);
            yield return null;
        }
        UI.Sangre();
        yield return new WaitForSeconds(0.3f);
    }

    protected IEnumerator Esperar(float s) { yield return new WaitForSeconds(s); }

    protected void Morir(string motivo, string recuerdo, TipoMuerte tipo)
    {
        juego.Desbloquear(this);
        juego.Morir(motivo, recuerdo, tipo);
    }

    // Lugar donde esconderse durante un evento (solo disponible mientras "abierto()" sea true).
    protected Interactuable Escondite(Vector2 pos, string accion, Func<bool> abierto, Action alUsar)
    {
        Interactuable i = Interactuable.Crear(transform, pos, accion, alUsar, 1.6f);
        i.disponible = abierto;
        return i;
    }

    protected void Mueble(string nombre, Vector2 centro, Vector2 tam, string colorHex)
    {
        Util.Rect(nombre, transform, centro, tam, Util.Hex(colorHex), -6);
    }

    protected void Solido(string nombre, Vector2 centro, Vector2 tam, string colorHex)
    {
        GameObject go = Util.Rect(nombre, transform, centro, tam, Util.Hex(colorHex), -6).gameObject;
        Util.Colision(go);
    }
}
