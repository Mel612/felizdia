using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Cómo mata el Pepino en cada caso (cada uno tiene su propia animación).
public enum TipoMuerte { Cuchillo, Empujon, Bisturi, Mazo, Reflector, Electrocucion }

// =====================================================================
//  ANIMACIÓN DE MUERTE (estilo Among Us)
//  Se abre una franja en el centro de la pantalla, el Pepino y Valeria
//  aparecen en grande y se ve el ataque. Dura unos 3 segundos.
// =====================================================================
public class CinematicaMuerte : MonoBehaviour
{
    const float Escala = 9f;            // 1 píxel del sprite = 9 píxeles de pantalla (1080p)
    const float AltoFranja = 600f;

    UIJuego ui;
    TipoMuerte tipo;

    RectTransform raiz, franja, escena, bordeArriba, bordeAbajo;
    Image fondo, flash, imgPepino, imgValeria, imgArma, imgCharco;
    RectTransform rtPepino, rtValeria, rtArma, rtCharco;
    Text txtFrase;
    SpritesPersonaje sPepino, sValeria;
    Vector2 temblor;
    float tTemblor;

    class Particula { public RectTransform rt; public Vector2 v; public float vida; public float giro; }
    readonly List<Particula> particulas = new List<Particula>();

    public static IEnumerator Reproducir(UIJuego ui, TipoMuerte tipo)
    {
        Sonido.Musica(null, 0.2f);
        Sonido.Latido(0f);
        Sonido.Efecto("susto", 0.8f, 1f);
        GameObject go = new GameObject("Cinemática de muerte", typeof(RectTransform));
        go.transform.SetParent(ui.transform, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        CinematicaMuerte c = go.AddComponent<CinematicaMuerte>();
        c.ui = ui;
        c.tipo = tipo;
        c.Construir();
        bool listo = false;
        c.StartCoroutine(c.Envolver(delegate { listo = true; }));
        while (!listo) yield return null;
        Destroy(go);
    }

    IEnumerator Envolver(System.Action fin)
    {
        yield return StartCoroutine(Animar());
        fin();
    }

    // ---------------------------------------------------------------
    static Image Img(Transform padre, string nombre, Sprite sprite, Vector2 tam, Vector2 pos)
    {
        GameObject go = new GameObject(nombre);
        go.transform.SetParent(padre, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = tam;
        rt.anchoredPosition = pos;
        Image img = go.AddComponent<Image>();
        img.sprite = sprite;
        img.raycastTarget = false;
        return img;
    }

    static Vector2 TamSprite(Sprite s) { return new Vector2(s.texture.width * Escala, s.texture.height * Escala); }

    void Construir()
    {
        sPepino = Arte.Pepino();
        sValeria = Protagonista.Sprites;
        Vector2 cero = Vector2.zero, uno = Vector2.one;

        raiz = Util.Panel(transform, "Raíz", new Color(0, 0, 0, 0), cero, uno, cero, cero);
        fondo = raiz.GetComponent<Image>();

        string colorFranja = "#3d0a12";
        if (tipo == TipoMuerte.Electrocucion) colorFranja = "#0d1b3d";
        else if (tipo == TipoMuerte.Bisturi) colorFranja = "#0f3a35";
        else if (tipo == TipoMuerte.Mazo) colorFranja = "#3a2410";
        else if (tipo == TipoMuerte.Reflector) colorFranja = "#24123a";
        else if (tipo == TipoMuerte.Empujon) colorFranja = "#2a2a35";

        franja = Util.Panel(raiz, "Franja", Color.white, new Vector2(0, 0.5f), new Vector2(1, 0.5f), cero, cero);
        franja.sizeDelta = new Vector2(0, 0);
        Image imgFranja = franja.GetComponent<Image>();
        imgFranja.sprite = Arte.FondoMuerte(colorFranja);
        imgFranja.type = Image.Type.Tiled;
        imgFranja.pixelsPerUnitMultiplier = 0.25f;

        bordeArriba = Util.Imagen(franja, "Borde", Arte.BordeFranja(), new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -14), new Vector2(0, 14)).rectTransform;
        bordeArriba.GetComponent<Image>().type = Image.Type.Tiled;
        bordeAbajo = Util.Imagen(franja, "Borde", Arte.BordeFranja(), new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, -14), new Vector2(0, 14)).rectTransform;
        bordeAbajo.GetComponent<Image>().type = Image.Type.Tiled;

        GameObject goEsc = new GameObject("Escena");
        goEsc.transform.SetParent(franja, false);
        escena = goEsc.AddComponent<RectTransform>();
        escena.anchorMin = escena.anchorMax = new Vector2(0.5f, 0.5f);
        escena.sizeDelta = new Vector2(1920, AltoFranja);

        // Charco de sangre (crece al final)
        imgCharco = Img(escena, "Charco", Arte.Sangre(2), new Vector2(0, 0), new Vector2(330, -240));
        rtCharco = imgCharco.rectTransform;

        imgPepino = Img(escena, "Pepino", sPepino.lado[0], TamSprite(sPepino.lado[0]), new Vector2(-1300, -40));
        rtPepino = imgPepino.rectTransform;
        imgValeria = Img(escena, "Valeria", sValeria.lado[0], TamSprite(sValeria.lado[0]), new Vector2(1300, -40));
        rtValeria = imgValeria.rectTransform;
        rtValeria.localScale = new Vector3(-1f, 1f, 1f);          // mira hacia el Pepino

        Sprite arma = null;
        if (tipo == TipoMuerte.Cuchillo) arma = Arte.Cuchillo();
        else if (tipo == TipoMuerte.Bisturi) arma = Arte.Bisturi();
        else if (tipo == TipoMuerte.Mazo) arma = Arte.Mazo();
        else if (tipo == TipoMuerte.Reflector) arma = Arte.Reflector();
        if (arma != null)
        {
            float esc = tipo == TipoMuerte.Reflector ? Escala * 2f : Escala;
            imgArma = Img(escena, "Arma", arma, new Vector2(arma.texture.width * esc, arma.texture.height * esc), new Vector2(-2000, 0));
            rtArma = imgArma.rectTransform;
            rtArma.pivot = new Vector2(arma.pivot.x / arma.texture.width, arma.pivot.y / arma.texture.height);
        }

        txtFrase = Util.TextoUI(franja, "", 64, Util.Hex("#ff4d4d"), TextAnchor.MiddleCenter, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -120), new Vector2(0, -20));
        txtFrase.fontStyle = FontStyle.Bold;
        txtFrase.gameObject.AddComponent<Outline>().effectColor = Color.black;

        flash = Util.Panel(raiz, "Destello", new Color(1, 1, 1, 0), cero, uno, cero, cero).GetComponent<Image>();
        raiz.SetAsLastSibling();
    }

    // ---------------------------------------------------------------
    void Update()
    {
        // Temblor de pantalla
        if (tTemblor > 0f)
        {
            tTemblor -= Time.deltaTime;
            float f = Mathf.Clamp01(tTemblor * 3f) * 28f;
            temblor = new Vector2(Random.Range(-f, f), Random.Range(-f, f));
        }
        else temblor = Vector2.zero;
        if (raiz != null) raiz.anchoredPosition = temblor;

        // Partículas de sangre
        for (int i = particulas.Count - 1; i >= 0; i--)
        {
            Particula p = particulas[i];
            p.vida -= Time.deltaTime;
            p.v.y -= 2600f * Time.deltaTime;
            p.rt.anchoredPosition += p.v * Time.deltaTime;
            p.rt.localRotation = Quaternion.Euler(0, 0, p.rt.localRotation.eulerAngles.z + p.giro * Time.deltaTime);
            if (p.rt.anchoredPosition.y < -AltoFranja / 2f + 30f) { p.v = Vector2.zero; p.giro = 0f; }
            if (p.vida <= 0f) { Destroy(p.rt.gameObject); particulas.RemoveAt(i); }
        }

        if (flash != null && flash.color.a > 0f)
            flash.color = new Color(flash.color.r, flash.color.g, flash.color.b, Mathf.MoveTowards(flash.color.a, 0f, Time.deltaTime * 3f));
    }

    void Temblar(float s) { tTemblor = Mathf.Max(tTemblor, s); }

    void Destello(Color c) { flash.color = new Color(c.r, c.g, c.b, 0.85f); }

    void Salpicar(Vector2 origen, int cantidad, float fuerza)
    {
        for (int i = 0; i < cantidad; i++)
        {
            float tam = Random.Range(10f, 26f);
            Image img = Img(escena, "Gota", Arte.Pixel(), new Vector2(tam, tam), origen + new Vector2(Random.Range(-20f, 20f), Random.Range(-20f, 20f)));
            img.color = Random.value < 0.7f ? Util.Hex("#a4161a") : Util.Hex("#6a040f");
            Particula p = new Particula();
            p.rt = img.rectTransform;
            p.v = new Vector2(Random.Range(-0.3f, 1f) * fuerza, Random.Range(0.3f, 1.2f) * fuerza);
            p.vida = Random.Range(1.2f, 2.4f);
            p.giro = Random.Range(-500f, 500f);
            particulas.Add(p);
        }
    }

    static float Suave(float t) { t = Mathf.Clamp01(t); return t * t * (3f - 2f * t); }

    IEnumerator Mover(RectTransform rt, Vector2 desde, Vector2 hasta, float dur)
    {
        float t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            rt.anchoredPosition = Vector2.Lerp(desde, hasta, Suave(t / dur));
            yield return null;
        }
        rt.anchoredPosition = hasta;
    }

    IEnumerator Abrir()
    {
        float t = 0f;
        Vector2 pIni = new Vector2(-1300, -40), pFin = new Vector2(-330, -40);
        Vector2 vIni = new Vector2(1300, -40), vFin = new Vector2(330, -40);
        while (t < 0.35f)
        {
            t += Time.deltaTime;
            float k = Suave(t / 0.35f);
            franja.sizeDelta = new Vector2(0, AltoFranja * k);
            fondo.color = new Color(0, 0, 0, 0.75f * k);
            rtPepino.anchoredPosition = Vector2.Lerp(pIni, pFin, k);
            rtValeria.anchoredPosition = Vector2.Lerp(vIni, vFin, k);
            yield return null;
        }
        Temblar(0.15f);
    }

    IEnumerator Cerrar()
    {
        yield return new WaitForSeconds(1.1f);
        float t = 0f;
        while (t < 0.3f)
        {
            t += Time.deltaTime;
            float k = 1f - Suave(t / 0.3f);
            franja.sizeDelta = new Vector2(0, AltoFranja * k);
            yield return null;
        }
        franja.gameObject.SetActive(false);
        fondo.color = new Color(0.16f, 0f, 0.02f, 1f);
    }

    // Valeria cae hacia atrás y aparece el charco.
    IEnumerator Caer(float dur)
    {
        float t = 0f;
        Vector2 p0 = rtValeria.anchoredPosition, p1 = new Vector2(p0.x + 90f, -AltoFranja / 2f + 90f);
        imgValeria.sprite = sValeria.golpe;
        while (t < dur)
        {
            t += Time.deltaTime;
            float k = Suave(t / dur);
            rtValeria.localRotation = Quaternion.Euler(0, 0, -90f * k);
            rtValeria.anchoredPosition = Vector2.Lerp(p0, p1, k);
            yield return null;
        }
        Temblar(0.2f);
        yield return StartCoroutine(Charco(new Vector2(p1.x + 40f, -AltoFranja / 2f + 50f)));
    }

    IEnumerator Charco(Vector2 pos)
    {
        rtCharco.anchoredPosition = pos;
        float t = 0f;
        while (t < 0.6f)
        {
            t += Time.deltaTime;
            float k = Suave(t / 0.6f);
            rtCharco.sizeDelta = new Vector2(420f * k, 150f * k);
            yield return null;
        }
    }

    IEnumerator Mirar()
    {
        // El Pepino gira lentamente hacia la cámara.
        yield return new WaitForSeconds(0.35f);
        imgPepino.sprite = sPepino.abajo[0];
        if (imgArma != null && tipo != TipoMuerte.Reflector) imgArma.gameObject.SetActive(false);
        txtFrase.text = "FELIZ PRIMER DÍA";
        float t = 0f;
        Vector3 e0 = rtPepino.localScale;
        while (t < 0.5f)
        {
            t += Time.deltaTime;
            rtPepino.localScale = Vector3.Lerp(e0, new Vector3(1.08f, 1.08f, 1f), t / 0.5f);
            yield return null;
        }
    }

    // Coloca el arma en la mano del Pepino (en píxeles del sprite: mano en (hx, hy)).
    void ArmaEnMano(float hx, float hy, float angulo)
    {
        if (rtArma == null) return;
        rtArma.anchoredPosition = rtPepino.anchoredPosition + new Vector2((hx - 16f) * Escala, (hy - 24f) * Escala);
        rtArma.localRotation = Quaternion.Euler(0, 0, angulo);
    }

    // ---------------------------------------------------------------
    IEnumerator Animar()
    {
        yield return StartCoroutine(Abrir());
        switch (tipo)
        {
            case TipoMuerte.Empujon: yield return StartCoroutine(AnimEmpujon()); break;
            case TipoMuerte.Mazo: yield return StartCoroutine(AnimMazo()); break;
            case TipoMuerte.Reflector: yield return StartCoroutine(AnimReflector()); break;
            case TipoMuerte.Electrocucion: yield return StartCoroutine(AnimElectro()); break;
            case TipoMuerte.Bisturi: yield return StartCoroutine(AnimCorte(true)); break;
            default: yield return StartCoroutine(AnimCorte(false)); break;
        }
        yield return StartCoroutine(Cerrar());
    }

    // Cuchillo y bisturí: amenaza, embestida, corte, caída.
    IEnumerator AnimCorte(bool bisturi)
    {
        imgPepino.sprite = sPepino.ataque;
        float t = 0f;
        while (t < 0.55f)
        {
            t += Time.deltaTime;
            rtPepino.anchoredPosition = new Vector2(-330f + Mathf.Sin(t * 60f) * 3f, -40f);
            ArmaEnMano(25.5f, 20.5f, 80f);
            yield return null;
        }
        imgPepino.sprite = sPepino.golpe;
        Vector2 a = rtPepino.anchoredPosition, b = new Vector2(70f, -40f);
        t = 0f;
        while (t < 0.12f)
        {
            t += Time.deltaTime;
            rtPepino.anchoredPosition = Vector2.Lerp(a, b, t / 0.12f);
            ArmaEnMano(28f, 19f, 0f);
            yield return null;
        }
        Destello(Color.white);
        Temblar(0.45f);
        Sonido.Efecto("cuchillo");
        Sonido.Efecto("grito", 0.7f, 1.05f);
        Vector2 pecho = rtValeria.anchoredPosition + new Vector2(-20f, 30f);
        Salpicar(pecho, 40, 900f);
        if (bisturi)
        {
            Image estela = Img(escena, "Corte", Arte.Estela(), new Vector2(48 * Escala, 16 * Escala), pecho);
            estela.rectTransform.localRotation = Quaternion.Euler(0, 0, -25f);
            yield return new WaitForSeconds(0.18f);
            Image estela2 = Img(escena, "Corte", Arte.Estela(), new Vector2(48 * Escala, 16 * Escala), pecho + new Vector2(0, -50f));
            estela2.rectTransform.localRotation = Quaternion.Euler(0, 0, 25f);
            Salpicar(pecho + new Vector2(0, -40f), 30, 800f);
            Destello(Util.Hex("#ff4d4d"));
            Temblar(0.3f);
            Sonido.Efecto("cuchillo", 0.8f, 1.2f);
            yield return new WaitForSeconds(0.2f);
            Destroy(estela.gameObject);
            Destroy(estela2.gameObject);
        }
        imgValeria.color = Util.Hex("#ffb3b3");
        yield return StartCoroutine(Caer(0.45f));
        yield return StartCoroutine(Mirar());
    }

    // Escaleras: el Pepino la empuja y ella cae fuera de la franja.
    IEnumerator AnimEmpujon()
    {
        yield return new WaitForSeconds(0.35f);
        imgPepino.sprite = sPepino.golpe;
        yield return StartCoroutine(Mover(rtPepino, rtPepino.anchoredPosition, new Vector2(80f, -40f), 0.14f));
        Temblar(0.3f);
        Destello(Color.white);
        Sonido.Efecto("grito", 0.8f, 1f);
        Sonido.Efecto("caida", 0.9f, 1f);
        imgValeria.sprite = sValeria.salto;
        float t = 0f;
        Vector2 p0 = rtValeria.anchoredPosition;
        while (t < 0.75f)
        {
            t += Time.deltaTime;
            rtValeria.anchoredPosition = p0 + new Vector2(t * 900f, 300f * t - 2200f * t * t);
            rtValeria.localRotation = Quaternion.Euler(0, 0, -t * 600f);
            yield return null;
        }
        imgValeria.gameObject.SetActive(false);
        yield return new WaitForSeconds(0.25f);
        Temblar(0.6f);
        Destello(Util.Hex("#a4161a"));
        Salpicar(new Vector2(780f, -AltoFranja / 2f + 40f), 45, 1000f);
        yield return StartCoroutine(Charco(new Vector2(760f, -AltoFranja / 2f + 50f)));
        yield return StartCoroutine(Mirar());
    }

    // Derecho: el mazo cae tres veces.
    IEnumerator AnimMazo()
    {
        imgPepino.sprite = sPepino.ataque;
        yield return StartCoroutine(Mover(rtPepino, rtPepino.anchoredPosition, new Vector2(60f, -40f), 0.3f));
        for (int golpe = 0; golpe < 3; golpe++)
        {
            float t = 0f;
            while (t < 0.18f)
            {
                t += Time.deltaTime;
                ArmaEnMano(25.5f, 20.5f, Mathf.Lerp(30f, 0f, t / 0.18f));
                yield return null;
            }
            t = 0f;
            while (t < 0.08f)
            {
                t += Time.deltaTime;
                ArmaEnMano(25.5f, 20.5f, Mathf.Lerp(0f, -100f, t / 0.08f));
                yield return null;
            }
            Temblar(0.35f);
            Destello(golpe == 2 ? Util.Hex("#a4161a") : Color.white);
            Sonido.Efecto("golpe", 1f, 1f - golpe * 0.1f);
            if (golpe == 0) Sonido.Efecto("grito", 0.7f, 0.95f);
            Salpicar(rtValeria.anchoredPosition + new Vector2(0, 120f), 18 + golpe * 10, 700f);
            rtValeria.localScale = new Vector3(-1f, 1f - 0.12f * (golpe + 1), 1f);
            rtValeria.anchoredPosition += new Vector2(0, -24f);
            yield return new WaitForSeconds(0.12f);
        }
        imgValeria.color = Util.Hex("#ffb3b3");
        yield return StartCoroutine(Caer(0.35f));
        yield return StartCoroutine(Mirar());
    }

    // Periodismo: el reflector cae desde arriba. El Pepino mira desde las vigas.
    IEnumerator AnimReflector()
    {
        imgPepino.sprite = sPepino.abajo[0];
        rtPepino.anchoredPosition = new Vector2(-560f, 150f);
        rtPepino.localScale = new Vector3(0.6f, 0.6f, 1f);
        imgPepino.color = new Color(0.7f, 0.7f, 0.7f, 1f);
        ArmaEnMano(16f, 60f, 0f);
        rtArma.anchoredPosition = new Vector2(330f, 520f);
        yield return new WaitForSeconds(0.25f);
        imgValeria.sprite = sValeria.abajo[0];
        rtValeria.localScale = Vector3.one;
        txtFrase.text = "M I R A    A R R I B A";
        yield return new WaitForSeconds(0.35f);
        yield return StartCoroutine(Mover(rtArma, rtArma.anchoredPosition, rtValeria.anchoredPosition + new Vector2(0, 60f), 0.16f));
        Temblar(0.7f);
        Destello(Color.white);
        Sonido.Efecto("cristal");
        Sonido.Efecto("golpe", 0.8f, 0.8f);
        Salpicar(rtValeria.anchoredPosition + new Vector2(0, 40f), 55, 1100f);
        float t = 0f;
        while (t < 0.15f)
        {
            t += Time.deltaTime;
            rtValeria.localScale = new Vector3(1.2f, Mathf.Lerp(1f, 0.3f, t / 0.15f), 1f);
            yield return null;
        }
        txtFrase.text = "";
        yield return StartCoroutine(Charco(new Vector2(330f, -AltoFranja / 2f + 50f)));
        yield return StartCoroutine(Mirar());
    }

    // Sistemas: empujón contra el rack, rayos y humo.
    IEnumerator AnimElectro()
    {
        yield return new WaitForSeconds(0.3f);
        imgPepino.sprite = sPepino.golpe;
        yield return StartCoroutine(Mover(rtPepino, rtPepino.anchoredPosition, new Vector2(60f, -40f), 0.14f));
        Sonido.Efecto("electricidad");
        Sonido.Efecto("grito", 0.6f, 1.15f);
        List<Image> rayos = new List<Image>();
        for (int i = 0; i < 5; i++)
        {
            Image r = Img(escena, "Rayo", Arte.Rayo(i), new Vector2(16 * Escala, 32 * Escala), rtValeria.anchoredPosition + new Vector2(Random.Range(-120f, 120f), Random.Range(-80f, 120f)));
            rayos.Add(r);
        }
        float t = 0f;
        int n = 0;
        while (t < 1.1f)
        {
            t += Time.deltaTime;
            n++;
            bool par = (n / 3) % 2 == 0;
            imgValeria.color = par ? Color.white : new Color(0.15f, 0.15f, 0.2f, 1f);
            rtValeria.anchoredPosition = new Vector2(330f + Random.Range(-10f, 10f), -40f + Random.Range(-6f, 6f));
            foreach (Image r in rayos)
            {
                r.gameObject.SetActive(Random.value < 0.6f);
                r.rectTransform.anchoredPosition = rtValeria.anchoredPosition + new Vector2(Random.Range(-140f, 140f), Random.Range(-100f, 140f));
                r.rectTransform.localRotation = Quaternion.Euler(0, 0, Random.Range(-40f, 40f));
            }
            if (n % 8 == 0) { Destello(par ? Util.Hex("#bde0fe") : Color.white); Temblar(0.15f); }
            yield return null;
        }
        foreach (Image r in rayos) Destroy(r.gameObject);
        imgValeria.color = Util.Hex("#3a3a42");
        for (int i = 0; i < 6; i++)
        {
            Image h = Img(escena, "Humo", Arte.Humo(), new Vector2(160, 160), rtValeria.anchoredPosition + new Vector2(Random.Range(-90f, 90f), Random.Range(60f, 200f)));
            h.color = new Color(1, 1, 1, 0.8f);
        }
        yield return StartCoroutine(Caer(0.5f));
        yield return StartCoroutine(Mirar());
    }
}
