using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum EscenarioPersecucion { Pasillo, Auditorio }

// =====================================================================
//  PERSECUCIÓN DE LADO (estilo plataformas, como Mario)
//  La cámara cambia a vista lateral: Valeria corre hacia la salida mientras
//  el Pepino la persigue. Hay que saltar obstáculos y agacharse bajo carteles.
//
//  Controles: → / D correr · ← / A frenar · ESPACIO / W / ↑ saltar
//             ↓ / S agacharse · SHIFT esprintar
// =====================================================================
public class Persecucion : MonoBehaviour
{
    // Cada nivel de lado está dibujado lejos del campus (a la izquierda), uno debajo del otro.
    public Vector2 Base;

    public static Vector2 BaseDe(EscenarioPersecucion esc, int piso)
    {
        return esc == EscenarioPersecucion.Auditorio ? new Vector2(-4000f, -200f) : new Vector2(-4000f, -40f * piso);
    }

    public static float LargoDe(EscenarioPersecucion esc) { return esc == EscenarioPersecucion.Auditorio ? 58f : 72f; }

    // Usado por el editor para dibujar los niveles en la escena.
    public static Persecucion Hornear(EscenarioPersecucion esc, int piso, Transform padre)
    {
        GameObject go = new GameObject(esc == EscenarioPersecucion.Auditorio ? "Persecución: Auditorio" : "Persecución: " + Mapa.NombresPiso[piso]);
        go.transform.SetParent(padre, false);
        Persecucion p = go.AddComponent<Persecucion>();
        p.escenario = esc;
        p.piso = piso;
        p.largo = LargoDe(esc);
        p.Construir();
        return p;
    }

    public EscenarioPersecucion escenario;
    public int piso;
    public float largo = 72f;

    // Física de Valeria
    const float Gravedad = 40f, Salto = 14f, Acel = 32f;
    const float VelCorrer = 5.3f, VelSprint = 6.3f, VelAgachada = 2.9f;
    const float Ancho = 0.5f, Alto = 1.35f, AltoAgachada = 0.8f;

    class Solido { public Rect r; public bool techo; }
    readonly List<Solido> solidos = new List<Solido>();

    Transform raiz, objetivoCamara;
    SpriteRenderer srValeria, srPepino, srCuchillo;
    SpritesPersonaje sV, sP;
    Vector2 pos, vel;                 // pos = pies de Valeria (coordenadas locales: el piso es y = 0)
    bool enSuelo, agachada;
    float pepinoX, pepinoVel, tAnim, tPepinoAnim, tiempo;
    bool terminado, exito;
    Text txtHud, txtAyuda;
    RectTransform hud;

    // ---------------------------------------------------------------
    public static IEnumerator Jugar(EscenarioPersecucion esc, int piso, Resultado<bool> resultado)
    {
        Juego j = Juego.I;
        GameObject go = new GameObject("Persecución lateral");
        go.transform.SetParent(j.Raiz, false);
        Persecucion p = go.AddComponent<Persecucion>();
        p.escenario = esc;
        p.piso = piso;
        p.largo = LargoDe(esc);
        p.Construir();

        j.Bloquear(p);
        j.ui.Prompt(null);
        string temaAntes = Sonido.TemaActual;
        Sonido.Musica("m_persecucion", 0.3f);
        Sonido.Latido(0.8f);
        j.ui.OscuridadInstantanea(0f);
        j.camaraSeguir = p.objetivoCamara;
        if (j.camara != null) j.camara.orthographicSize = 4.2f;      // más cerca: personajes grandes
        p.CrearHud();

        while (!p.terminado) yield return null;
        yield return new WaitForSeconds(p.exito ? 0.6f : 0.25f);

        j.camaraSeguir = null;
        Sonido.Latido(0f);
        if (p.exito) { Sonido.Efecto("puerta", 0.7f, 1f); Sonido.Musica(temaAntes, 1.5f); }
        if (j.camara != null) j.camara.orthographicSize = j.zoomCamara;
        if (p.hud != null) Destroy(p.hud.gameObject);
        j.Desbloquear(p);
        resultado.valor = p.exito;
        Destroy(go);
    }

    Vector2 W(float x, float y) { return Base + new Vector2(x, y); }

    // ---------------------------------------------------------------
    //  CONSTRUCCIÓN DEL NIVEL
    // ---------------------------------------------------------------
    public void Construir()
    {
        Base = BaseDe(escenario, piso);
        raiz = transform;
        Horno.Empezar("Persecucion_" + escenario + "_" + piso);
        try { ConstruirNivel(); }
        finally { Horno.Terminar(); }
        CrearPersonajes();
    }

    void ConstruirNivel()
    {
        sV = Protagonista.Sprites;
        sP = Arte.Pepino();
        bool aud = escenario == EscenarioPersecucion.Auditorio;
        float x0 = -14f, x1 = largo + 14f, ancho = x1 - x0, cx = (x0 + x1) / 2f;

        // Fondo: pared y piso
        if (!aud)
        {
            Color pintura = piso % 2 == 0 ? Util.Hex("#d8cfbf") : Util.Hex("#c9d6d8");
            Util.Mosaico("Pared", raiz, Arte.ParedLateral(pintura), W(cx, 0f), new Vector2(ancho, 6f), -3000);
            Util.Mosaico("Piso", raiz, Arte.PisoLateral(), W(cx, 0f), new Vector2(ancho, 3f), -2990);
        }
        else
        {
            Util.Mosaico("Telón", raiz, Arte.Cortina(), W(cx, 3f), new Vector2(ancho, 6f), -3000);
            Util.Mosaico("Piso", raiz, Arte.Suelo(TipoSuelo.Alfombra, Util.Hex("#6e1423")), W(cx, -1.5f), new Vector2(ancho, 3f), -2990);
            Util.Rect("Borde", raiz, W(cx, -0.05f), new Vector2(ancho, 0.12f), Util.Hex("#ffd166"), -2985);
        }
        Util.Rect("Techo", raiz, W(cx, 6.6f), new Vector2(ancho, 1.2f), Util.Hex("#1b1822"), -2980);

        Decorar(aud);
        CrearObstaculos(aud);

        // Meta
        float meta = largo;
        Util.Plano("Salida", raiz, Arte.Puerta(aud ? "#3d2236" : "#2d6a4f"), W(meta + 1.5f, 0f), -2900);
        Util.Plano("Letrero", raiz, Arte.LetreroSalida(), W(meta + 1.5f, 3.3f), -2900);
        Util.Texto(aud ? "DETRÁS DEL TELÓN" : "ESCALERAS", raiz, W(meta + 1.5f, 3.9f), 1f, Util.Hex("#7bd389"), 0);

    }

    void CrearPersonajes()
    {
        Horno.Suspender();
        // Valeria
        pos = new Vector2(1.5f, 0f);
        GameObject gv = new GameObject("Valeria (lateral)");
        gv.transform.SetParent(raiz, false);
        srValeria = gv.AddComponent<SpriteRenderer>();
        srValeria.sprite = sV.lado[0];
        srValeria.sortingOrder = 500;
        Util.AplicarMaterial(srValeria);

        // Pepino
        pepinoX = -5.5f;
        GameObject gp = new GameObject("Pepino (lateral)");
        gp.transform.SetParent(raiz, false);
        srPepino = gp.AddComponent<SpriteRenderer>();
        srPepino.sprite = sP.lado[0];
        if (Protagonista.EsMateo) srPepino.color = new Color(1f, 0.82f, 0.45f);      // el quinto disfraz (Sebastián)
        srPepino.sortingOrder = 490;
        Util.AplicarMaterial(srPepino);
        GameObject gc = new GameObject("Cuchillo");
        gc.transform.SetParent(gp.transform, false);
        srCuchillo = gc.AddComponent<SpriteRenderer>();
        srCuchillo.sprite = Arte.Cuchillo();
        srCuchillo.sortingOrder = 491;
        Util.AplicarMaterial(srCuchillo);

        GameObject gcam = new GameObject("Cámara lateral");
        gcam.transform.SetParent(raiz, false);
        objetivoCamara = gcam.transform;
        Horno.Reanudar();
        Actualizar();
    }

    void Decorar(bool aud)
    {
        System.Random r = new System.Random(piso * 31 + (aud ? 7 : 3));
        float x0 = -12f, x1 = largo + 12f;
        if (!aud)
        {
            bool noche = Juego.I != null && Juego.I.Hora >= 17.5f;
            int i = 0;
            for (float x = x0; x < x1; x += 4.5f, i++)
            {
                int tipo = i % 4;
                if (tipo == 0)
                {
                    SpriteRenderer v = Util.Plano("Ventana", raiz, Arte.VentanaIllimani(false), W(x, 3.4f), -2970);
                    if (noche) v.sprite = Arte.VentanaIllimani(true);
                }
                else if (tipo == 1) { Util.Plano("Casillero", raiz, Arte.Casillero(), W(x - 0.5f, 0f), -2960); Util.Plano("Casillero", raiz, Arte.Casillero(), W(x + 0.5f, 0f), -2960); }
                else if (tipo == 2)
                {
                    Util.Plano("Puerta", raiz, Arte.Puerta("#8d6748"), W(x, 0f), -2960);
                    Util.Rect("Placa", raiz, W(x, 2.85f), new Vector2(1.2f, 0.35f), Util.Hex("#0b3d91"), -2955);
                    Util.Texto(piso + "0" + (i % 9 + 1), raiz, W(x, 2.85f), 0.6f, Color.white, 0);
                }
                else
                {
                    Util.Plano("Afiche", raiz, Arte.Afiche(r.Next(5)), W(x - 0.6f, 2.6f), -2965);
                    Util.Plano("Afiche", raiz, Arte.Afiche(r.Next(5)), W(x + 0.6f, 2.9f), -2965);
                    Util.Plano("Planta", raiz, Arte.Planta(), W(x, 0f), -2958);
                }
            }
            for (float x = x0 + 2f; x < x1; x += 7f)
            {
                Util.Plano("Lámpara", raiz, Arte.LamparaTecho(), W(x, 6f), -2900);
                Util.Plano("Luz", raiz, Arte.ConoLuz(), W(x, 5.7f), -2950).transform.localScale = new Vector3(1.2f, 1.4f, 1f);
            }
            Util.Texto(Mapa.NombresPiso[Mathf.Clamp(piso, 0, Mapa.Pisos - 1)].ToUpper(), raiz, W(-6f, 4.7f), 2f, new Color(0, 0, 0, 0.25f), -100);
        }
        else
        {
            // Público bailando y focos de colores
            for (float x = x0; x < x1; x += 0.9f)
            {
                SpriteRenderer sil = Util.Plano("Público", raiz, Arte.Silueta(r.Next(8)), W(x + (float)r.NextDouble() * 0.4f, 0.3f + (float)r.NextDouble() * 0.6f), -2950);
                sil.color = new Color(1, 1, 1, 0.9f);
            }
            string[] focos = { "#ff006e", "#3a86ff", "#ffbe0b", "#8338ec", "#06d6a0" };
            for (float x = x0; x < x1; x += 5f)
                Util.Plano("Foco", raiz, Arte.FocoEscenario(focos[r.Next(focos.Length)]), W(x, 6.1f), -2940).transform.localScale = new Vector3(1.4f, 1.2f, 1f);
            for (float x = x0 + 3f; x < x1; x += 14f)
            {
                Util.Rect("Banderín", raiz, W(x, 5.5f), new Vector2(6f, 0.08f), Util.Hex("#ffd166"), -2930);
                for (int k = 0; k < 6; k++)
                    Util.Rect("Banderín", raiz, W(x - 2.5f + k, 5.25f), new Vector2(0.4f, 0.4f), Util.Hex(focos[k % focos.Length]), -2929);
            }
        }
    }

    void Solid(float x, float y, float w, float h, bool techo)
    {
        Solido s = new Solido();
        s.r = new Rect(x - w / 2f, y, w, h);
        s.techo = techo;
        solidos.Add(s);
    }

    void CrearObstaculos(bool aud)
    {
        System.Random r = new System.Random(piso * 17 + (aud ? 99 : 0));
        float x = 9f;
        int agachadasSeguidas = 0;
        while (x < largo - 5f)
        {
            int t = r.Next(aud ? 5 : 6);
            bool agachar = aud ? t == 4 : t >= 4;
            if (agachar && agachadasSeguidas >= 1) { t = 0; agachar = false; }
            agachadasSeguidas = agachar ? agachadasSeguidas + 1 : 0;

            if (!aud)
            {
                switch (t)
                {
                    case 0: Util.Plano("Caja", raiz, Arte.Caja(), W(x, 0f), 300); Solid(x, 0f, 0.95f, 1f, false); break;
                    case 1: Util.Plano("Silla", raiz, Arte.SillaLado(), W(x, 0f), 300); Solid(x, 0f, 0.8f, 1.05f, false); break;
                    case 2: Util.Plano("Carrito", raiz, Arte.Carrito(), W(x, 0f), 300); Solid(x, 0f, 1.6f, 0.95f, false); break;
                    case 3:
                        Util.Plano("Mesa", raiz, Arte.MesaLado(), W(x, 0f), 300); Solid(x, 0f, 2f, 1.1f, false);
                        Util.Plano("Caja", raiz, Arte.Caja(), W(x + 0.4f, 1.1f), 301); Solid(x + 0.4f, 1.1f, 0.95f, 1f, false);
                        break;
                    case 4: Colgante(x, Arte.Banner(piso % 2 == 0 ? "#0b3d91" : "#d62828"), 2.5f); break;
                    default: Colgante(x, Arte.DuctoTecho(), 2f); break;
                }
            }
            else
            {
                switch (t)
                {
                    case 0: case 1:
                        Util.Plano("Butaca", raiz, Arte.ButacaLado(), W(x, 0f), 300); Solid(x, 0f, 1.1f, 1.2f, false);
                        if (t == 1) { Util.Plano("Butaca", raiz, Arte.ButacaLado(), W(x + 1.15f, 0f), 300); Solid(x + 1.15f, 0f, 1.1f, 1.2f, false); }
                        break;
                    case 2:
                        SpriteRenderer parlante = Util.Plano("Parlante", raiz, Arte.Caja(), W(x, 0f), 300);
                        parlante.color = new Color(0.25f, 0.25f, 0.3f, 1f);
                        parlante.transform.localScale = new Vector3(1f, 1.3f, 1f);
                        Solid(x, 0f, 0.95f, 1.3f, false);
                        break;
                    case 3: Util.Plano("Mesa", raiz, Arte.MesaLado(), W(x, 0f), 300); Solid(x, 0f, 2f, 1.1f, false); break;
                    default: Colgante(x, Arte.Banner("#ffb703"), 2.5f); break;
                }
            }
            x += 5.5f + (float)r.NextDouble() * 2.5f;
        }
    }

    // Algo que cuelga del techo: hay que agacharse para pasar por debajo.
    void Colgante(float x, Sprite s, float ancho)
    {
        float yBase = 1.0f;
        Util.Plano("Colgante", raiz, s, W(x, yBase), 300);
        float tope = yBase + s.texture.height / Arte.PPU;
        if (tope < 6f) Util.Rect("Cuerda", raiz, W(x, (tope + 6f) / 2f), new Vector2(0.06f, 6f - tope), Util.Hex("#444"), 299);
        Solid(x, yBase, ancho * 0.9f, 6f - yBase, true);
    }

    void CrearHud()
    {
        Transform ui = Juego.I.ui.transform;
        hud = Util.PanelArte(ui, "HUD persecución", new Color(0.05f, 0.04f, 0.08f, 0.85f), Util.Hex("#ff4d4d"),
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-420, -150), new Vector2(420, -40));
        txtHud = Util.TextoRelleno(hud, "", 34, Color.white, TextAnchor.MiddleCenter, 12);
        txtAyuda = Util.TextoUI(ui, "→ correr   ·   ESPACIO saltar   ·   ↓ agacharse   ·   SHIFT esprintar", 28, Util.Hex("#ffe8a3"),
            TextAnchor.MiddleCenter, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-700, 40), new Vector2(700, 90));
        txtAyuda.gameObject.AddComponent<Outline>().effectColor = Color.black;
        txtAyuda.transform.SetParent(hud, true);
    }

    // ---------------------------------------------------------------
    //  JUEGO
    // ---------------------------------------------------------------
    bool Choca(float x, float y, float alto, out Solido con)
    {
        Rect yo = new Rect(x - Ancho / 2f, y, Ancho, alto);
        foreach (Solido s in solidos)
            if (yo.Overlaps(s.r)) { con = s; return true; }
        con = null;
        return false;
    }

    void Update()
    {
        if (terminado) return;
        float dt = Mathf.Min(Time.deltaTime, 0.05f);
        tiempo += dt;

        // Pequeña pausa al inicio: "¡CORRE!"
        bool arranque = tiempo < 1.0f;

        // ---------- Valeria ----------
        float entradaX = 0f;
        if (Entrada.Presionada(Tecla.Derecha)) entradaX += 1f;
        if (Entrada.Presionada(Tecla.Izquierda)) entradaX -= 1f;
        bool quiereAgachar = Entrada.Presionada(Tecla.Abajo) && enSuelo;
        Solido tmp;
        // Si hay algo encima, no puede pararse
        if (agachada && !quiereAgachar && Choca(pos.x, pos.y, Alto, out tmp)) quiereAgachar = true;
        agachada = quiereAgachar;

        float velMax = agachada ? VelAgachada : (Entrada.Presionada(Tecla.Correr) ? VelSprint : VelCorrer);
        vel.x = Mathf.MoveTowards(vel.x, entradaX * velMax, Acel * dt);

        bool saltar = Entrada.Bajada(Tecla.Accion) || Entrada.Bajada(Tecla.Arriba);
        if (enSuelo && saltar && !agachada) { vel.y = Salto; enSuelo = false; Sonido.Efecto("salto", 0.4f, 1f); }
        bool sosteniendo = Entrada.Presionada(Tecla.Accion) || Entrada.Presionada(Tecla.Arriba);
        float g = Gravedad * (vel.y > 0f && !sosteniendo ? 2.2f : 1f);       // salto más corto si sueltas
        vel.y -= g * dt;

        float alto = agachada ? AltoAgachada : Alto;

        // Movimiento horizontal
        pos.x += vel.x * dt;
        Solido s;
        if (Choca(pos.x, pos.y, alto, out s))
        {
            if (vel.x > 0f) pos.x = s.r.xMin - Ancho / 2f - 0.001f;
            else if (vel.x < 0f) pos.x = s.r.xMax + Ancho / 2f + 0.001f;
            vel.x = 0f;
        }
        pos.x = Mathf.Max(pos.x, -10f);

        // Movimiento vertical
        pos.y += vel.y * dt;
        enSuelo = false;
        if (pos.y <= 0f) { pos.y = 0f; vel.y = 0f; enSuelo = true; }
        else if (Choca(pos.x, pos.y, alto, out s))
        {
            if (vel.y < 0f) { pos.y = s.r.yMax; vel.y = 0f; enSuelo = true; }
            else { pos.y = s.r.yMin - alto - 0.001f; vel.y = 0f; }
        }
        else if (vel.y <= 0f && Choca(pos.x, pos.y - 0.02f, alto, out s) && pos.y >= s.r.yMax - 0.03f) enSuelo = true;

        // ---------- Pepino ----------
        if (!arranque)
        {
            float objetivo = ((escenario == EscenarioPersecucion.Auditorio ? 4.9f : 4.5f) + tiempo * 0.012f) * Dificultad.VelPersecucion;
            // Si se queda muy atrás, acelera (para que siempre haya tensión)
            if (pos.x - pepinoX > 9f) objetivo += 1.6f;
            pepinoVel = Mathf.MoveTowards(pepinoVel, objetivo, 6f * dt);
            pepinoX += pepinoVel * dt;
        }

        // ---------- Fin ----------
        if (pos.x >= largo)
        {
            terminado = true;
            exito = true;
            txtHud.text = "<color=#7bd389>¡ESCAPASTE!</color>";
            Juego.I.ui.Destello(new Color(0.2f, 0.8f, 0.3f), 0.3f);
        }
        else if (pepinoX + 0.55f >= pos.x - Ancho / 2f)
        {
            terminado = true;
            exito = false;
            srPepino.sprite = sP.golpe;
            Juego.I.ui.Golpe(0.6f);
        }

        Actualizar();
        ActualizarHud(arranque);
    }

    void Actualizar()
    {
        // Valeria
        float dt = Time.deltaTime;
        bool corre = Mathf.Abs(vel.x) > 0.3f;
        if (corre && enSuelo) tAnim += dt * Mathf.Abs(vel.x) * 2.2f;
        Sprite sv;
        if (!enSuelo) sv = sV.salto;
        else if (agachada) sv = sV.agachada;
        else if (corre) sv = sV.lado[1 + ((int)tAnim) % 2];
        else sv = sV.lado[0];
        srValeria.sprite = sv;
        srValeria.flipX = vel.x < -0.3f;
        srValeria.transform.position = W(pos.x, pos.y + 0.75f);

        // Pepino: salta los obstáculos que tiene delante (es más ágil que tú)
        tPepinoAnim += dt * Mathf.Max(pepinoVel, 0.5f) * 2.2f;
        float yPep = 0f;
        foreach (Solido so in solidos)
        {
            if (so.techo) continue;
            float d = Mathf.Abs(pepinoX - (so.r.x + so.r.width / 2f));
            float alcance = so.r.width / 2f + 0.9f;
            if (d < alcance) yPep = Mathf.Max(yPep, so.r.yMax * Mathf.Sin((1f - d / alcance) * 1.5708f) + 0.2f);
        }
        bool pepAgacha = false;
        foreach (Solido so in solidos)
            if (so.techo && Mathf.Abs(pepinoX - (so.r.x + so.r.width / 2f)) < so.r.width / 2f + 0.3f) pepAgacha = true;
        if (!terminado || exito)
        {
            if (yPep > 0.05f) srPepino.sprite = sP.salto;
            else if (pepAgacha) srPepino.sprite = sP.agachada;
            else srPepino.sprite = pepinoVel > 0.3f ? sP.lado[1 + ((int)tPepinoAnim) % 2] : sP.ataque;
        }
        srPepino.transform.position = W(pepinoX, yPep + 0.75f);
        srCuchillo.transform.localPosition = pepAgacha ? new Vector3(0.45f, -0.35f, 0f) : new Vector3(0.1f, 0.18f, 0f);
        srCuchillo.transform.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(tiempo * 14f) * 25f + 60f);

        // Cámara
        // La cámara intenta mostrar a los dos: a Valeria y al Pepino que la persigue.
        float camX = Mathf.Max(pos.x + 1.5f, (pos.x + pepinoX) / 2f + 2.5f);
        camX = Mathf.Clamp(camX, -3f, largo - 2f);
        objetivoCamara.position = new Vector3(W(camX, 0f).x, Base.y + 2.5f, 0f);
    }

    void ActualizarHud(bool arranque)
    {
        if (txtHud == null || terminado) return;
        float falta = Mathf.Max(0f, largo - pos.x);
        float distPepino = Mathf.Max(0f, pos.x - pepinoX);
        string color = distPepino < 2.5f ? "#ff4d4d" : distPepino < 5f ? "#ffd166" : "#7bd389";
        if (arranque) txtHud.text = "<color=#ff4d4d><b>¡CORRE!</b></color>";
        else txtHud.text = "SALIDA: " + Mathf.CeilToInt(falta) + " m      <color=" + color + ">PEPINO: " + distPepino.ToString("0.0") + " m</color>";
        if (distPepino < 2.5f && !arranque && Random.value < 0.05f) Juego.I.ui.Golpe(0.12f);
    }
}
