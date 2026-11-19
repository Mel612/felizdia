using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// =====================================================================
//  EL CUARTO SECRETO: minijuego estilo "cinco noches".
//  Estás encerrada en el viejo cuarto de monitoreo de la U, de 12 AM a 6 AM.
//  Por las cámaras ves a los disfraces acercarse. Cierra la puerta a tiempo,
//  pero cuidado: las puertas y las cámaras gastan energía.
//
//  Controles:  A / ←  puerta izquierda   ·   D / →  puerta derecha
//              W / Espacio  cámaras  ·   1-6  elegir cámara
// =====================================================================
public class NocheMonitoreo : MonoBehaviour
{
    static readonly string[] NombresCam = { "CAM 1  ·  Patio", "CAM 2  ·  Escaleras", "CAM 3  ·  Pasillo Piso 1", "CAM 4  ·  Pasillo Piso 3", "CAM 5  ·  Pasillo oeste", "CAM 6  ·  Pasillo este" };
    const int PuertaIzq = -1, PuertaDer = -2;

    class Bicho
    {
        public string nombre;
        public Sprite sprite;
        public int[] ruta;           // cámaras por las que pasa; el último valor es la puerta
        public int paso;
        public float nivel;          // 0..20: qué tan seguido avanza
        public float tMov, tPuerta;
        public bool corredor;        // el "quinto disfraz": corre si no lo vigilas
        public float sinMirar;
        public int Lugar { get { return ruta[Mathf.Clamp(paso, 0, ruta.Length - 1)]; } }
        public bool EnPuerta { get { return Lugar < 0; } }
    }

    int noche;
    float segPorHora, energia = 100f, t;
    bool puertaIzq, puertaDer, monitor, terminado, gano, sinEnergia;
    int cam;
    float tSinEnergia, ataque;
    readonly List<Bicho> bichos = new List<Bicho>();

    MundoCuarto mundo;          // si la escena 7_CuartoSecreto está dibujada, se usa la oficina de la escena
    RectTransform raiz, panelCam;
    Image imgOficina, imgPersIzq, imgPersDer, imgEnPuertaIzq, imgEnPuertaDer, imgVista, imgEstatica, imgOscuro, imgCara;
    readonly List<Image> enCamara = new List<Image>();
    Text txtHora, txtEnergia, txtControles, txtCam, txtMapa, txtCentro;
    float tEstatica;

    // ---------------------------------------------------------------
    public static IEnumerator Jugar(int noche, Resultado<bool> resultado)
    {
        Juego j = Juego.I;
        GameObject go = new GameObject("Cuarto secreto (noche " + noche + ")", typeof(RectTransform));
        go.transform.SetParent(j.ui.transform, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = rt.offsetMax = Vector2.zero;
        NocheMonitoreo n = go.AddComponent<NocheMonitoreo>();
        n.noche = Mathf.Clamp(noche, 1, 3);
        j.Bloquear(n);
        bool hudAntes = j.ui.HudVisible;
        j.ui.HudVisible = false;
        string temaAntes = Sonido.TemaActual;
        Sonido.Musica(null, 0.5f);
        Sonido.Latido(0f);
        n.Preparar();
        n.Construir();
        while (!n.terminado) yield return null;
        yield return n.StartCoroutine(n.Final());
        resultado.valor = n.gano;
        if (n.gano)
        {
            Logros.Desbloquear(n.noche >= 3 ? "cuarto3" : "cuarto");
            if (n.noche == 1) Logros.Desbloquear("cuarto");
            DesbloquearNoche(Mathf.Min(3, n.noche + 1));
        }
        Sonido.Ambiente(null, 0f);
        Sonido.Latido(0f);
        Sonido.Musica(temaAntes, 1f);
        j.ui.HudVisible = hudAntes;
        j.Desbloquear(n);
        Destroy(go);
    }

    // Noches disponibles en el menú (0 = todavía no encontraste el cuarto)
    public static int NochesDesbloqueadas
    {
        get { try { return PlayerPrefs.GetInt("fpd_noches", 0); } catch { return 0; } }
    }

    // NIVEL ESCONDIDO: solo aparece en el menú cuando entraste al cuarto jugando la historia
    public static bool CuartoEncontrado
    {
        get { try { return PlayerPrefs.GetInt("fpd_cuarto_encontrado", 0) == 1; } catch { return false; } }
    }

    // Devuelve true la primera vez
    public static bool MarcarEncontrado()
    {
        if (CuartoEncontrado) return false;
        try { PlayerPrefs.SetInt("fpd_cuarto_encontrado", 1); PlayerPrefs.Save(); } catch { }
        DesbloquearNoche(1);
        return true;
    }

    public static void DesbloquearNoche(int n)
    {
        if (NochesDesbloqueadas >= n) return;
        try { PlayerPrefs.SetInt("fpd_noches", n); PlayerPrefs.Save(); } catch { }
    }

    // ---------------------------------------------------------------
    void Preparar()
    {
        float[] horas = { 22f, 26f, 30f };
        segPorHora = horas[noche - 1];
        ataque = noche == 1 ? 3.6f : noche == 2 ? 3f : 2.4f;
        float[] nivPepino = { 7f, 10f, 14f }, nivManiqui = { 5f, 8f, 12f };

        Bicho pep = new Bicho { nombre = "Pepino", sprite = Arte.Pepino().Frente, ruta = new[] { 0, 1, 2, 5, PuertaDer }, nivel = nivPepino[noche - 1] };
        Bicho man = new Bicho { nombre = "Maniquí", sprite = Arte.Maniqui().Frente, ruta = new[] { 3, 1, 4, PuertaIzq }, nivel = nivManiqui[noche - 1] };
        pep.tMov = 6f; man.tMov = 9f;           // los primeros segundos son tranquilos
        bichos.Add(pep);
        bichos.Add(man);
        if (noche >= 2)
        {
            // El quinto disfraz (el presupuesto decía 5...). Si no lo vigilas en la CAM 1, corre a la puerta izquierda.
            Bicho quinto = new Bicho { nombre = "El quinto disfraz", sprite = Arte.Pepino().Frente, ruta = new[] { 0, PuertaIzq }, corredor = true };
            quinto.nivel = noche == 2 ? 20f : 15f;          // aquí: segundos que aguanta sin que lo mires
            bichos.Add(quinto);
        }
    }

    void Construir()
    {
        Vector2 cero = Vector2.zero, uno = Vector2.one;
        mundo = Juego.I.tipoEscena == TipoEscena.Cuarto ? MundoCuarto.Actual : null;
        raiz = Util.Panel(transform, "Cuarto", mundo != null ? new Color(0, 0, 0, 0) : Color.black, cero, uno, cero, cero);
        if (mundo == null)
        {
            // Sin la escena dibujada: la oficina se arma como interfaz
            imgOficina = Util.Imagen(raiz, "Oficina", Arte.Oficina(), cero, uno, cero, cero);
            imgEnPuertaIzq = Util.Imagen(raiz, "En la puerta izq", Arte.Pepino().Frente, new Vector2(0.05f, 0.21f), new Vector2(0.205f, 0.78f), cero, cero);
            imgEnPuertaDer = Util.Imagen(raiz, "En la puerta der", Arte.Pepino().Frente, new Vector2(0.795f, 0.21f), new Vector2(0.95f, 0.78f), cero, cero);
            imgEnPuertaIzq.preserveAspect = imgEnPuertaDer.preserveAspect = true;
            imgPersIzq = Util.Imagen(raiz, "Persiana izq", Arte.Persiana(), new Vector2(0.042f, 0.207f), new Vector2(0.208f, 0.815f), cero, cero);
            imgPersDer = Util.Imagen(raiz, "Persiana der", Arte.Persiana(), new Vector2(0.792f, 0.207f), new Vector2(0.958f, 0.815f), cero, cero);
        }

        // Monitor (cámaras)
        panelCam = Util.Panel(raiz, "Monitor", new Color(0.02f, 0.05f, 0.03f, 0.97f), new Vector2(0.08f, 0.1f), new Vector2(0.92f, 0.92f), cero, cero);
        imgVista = Util.Imagen(panelCam, "Vista", Arte.VistaCamara(0), new Vector2(0.02f, 0.04f), new Vector2(0.72f, 0.96f), cero, cero);
        for (int i = 0; i < 3; i++)
        {
            Image b = Util.Imagen(imgVista.rectTransform, "Figura en cámara", Arte.Pepino().Frente, new Vector2(0.45f, 0.1f), new Vector2(0.55f, 0.5f), cero, cero);
            b.preserveAspect = true;
            enCamara.Add(b);
        }
        imgEstatica = Util.Imagen(imgVista.rectTransform, "Estática", Arte.Estatica(0), cero, uno, cero, cero);
        txtCam = Util.TextoUI(imgVista.rectTransform, "", 34, Util.Hex("#e9f5db"), TextAnchor.UpperLeft, cero, uno, new Vector2(20, 0), new Vector2(0, -14));
        txtCam.gameObject.AddComponent<Outline>().effectColor = Color.black;
        txtMapa = Util.TextoUI(panelCam, "", 30, Util.Hex("#b7e4c7"), TextAnchor.UpperLeft, new Vector2(0.74f, 0.04f), new Vector2(0.99f, 0.96f), cero, cero);

        imgOscuro = Util.Panel(raiz, "Sin energía", new Color(0, 0, 0, 0), cero, uno, cero, cero).GetComponent<Image>();

        // Textos
        txtHora = Util.TextoUI(raiz, "", 54, Color.white, TextAnchor.UpperRight, new Vector2(0.6f, 0.86f), new Vector2(0.98f, 0.99f), cero, cero);
        txtHora.fontStyle = FontStyle.Bold;
        txtHora.gameObject.AddComponent<Outline>().effectColor = Color.black;
        txtEnergia = Util.TextoUI(raiz, "", 32, Color.white, TextAnchor.LowerLeft, new Vector2(0.02f, 0.01f), new Vector2(0.5f, 0.12f), cero, cero);
        txtEnergia.gameObject.AddComponent<Outline>().effectColor = Color.black;
        txtControles = Util.TextoUI(raiz, "", 24, new Color(1, 1, 1, 0.75f), TextAnchor.LowerRight, new Vector2(0.4f, 0.01f), new Vector2(0.98f, 0.12f), cero, cero);
        txtControles.gameObject.AddComponent<Outline>().effectColor = Color.black;
        txtCentro = Util.TextoUI(raiz, "", 96, Color.white, TextAnchor.MiddleCenter, new Vector2(0, 0.35f), new Vector2(1, 0.65f), cero, cero);
        txtCentro.fontStyle = FontStyle.Bold;
        txtCentro.gameObject.AddComponent<Outline>().effectColor = Color.black;

        imgCara = Util.Imagen(raiz, "Susto", Arte.CaraSusto(), new Vector2(0.15f, 0f), new Vector2(0.85f, 1f), cero, cero);
        imgCara.preserveAspect = true;
        imgCara.gameObject.SetActive(false);

        Sonido.Ambiente("zumbido", 0.05f);
        StartCoroutine(Intro());
    }

    IEnumerator Intro()
    {
        txtCentro.text = "12:00 AM\n<size=48>NOCHE " + noche + "</size>";
        yield return new WaitForSeconds(2.2f);
        txtCentro.text = "";
    }

    // ---------------------------------------------------------------
    void Update()
    {
        transform.SetAsLastSibling();
        if (terminado) return;
        float dt = Time.deltaTime;
        t += dt;

        // Controles
        if (!sinEnergia)
        {
            if (Entrada.Bajada(Tecla.Izquierda) && !monitor) { puertaIzq = !puertaIzq; Sonido.Efecto("puerta", 0.6f, 0.55f); }
            if (Entrada.Bajada(Tecla.Derecha) && !monitor) { puertaDer = !puertaDer; Sonido.Efecto("puerta", 0.6f, 0.55f); }
            if (Entrada.Bajada(Tecla.Arriba) || Entrada.Bajada(Tecla.Accion)) { monitor = !monitor; Sonido.Efecto("click", 0.6f, 0.7f); if (monitor) Sonido.Efecto("estatica", 0.25f, 1.2f); }
            int n = Entrada.NumeroBajado();
            if (monitor && n >= 1 && n <= 6) { cam = n - 1; Sonido.Efecto("estatica", 0.3f, 1.4f); tEstatica = 0.35f; }
            if (monitor && Entrada.Bajada(Tecla.Saltar)) { monitor = false; }
        }

        // Energía
        float gasto = 0.17f + (puertaIzq ? 0.55f : 0f) + (puertaDer ? 0.55f : 0f) + (monitor ? 0.22f : 0f);
        energia -= gasto * dt * (noche == 3 ? 1.12f : 1f);
        if (energia <= 0f && !sinEnergia)
        {
            energia = 0f;
            sinEnergia = true;
            puertaIzq = puertaDer = monitor = false;
            tSinEnergia = Random.Range(4f, 9f);
            Sonido.Efecto("apagon", 0.9f, 0.8f);
            Sonido.Ambiente(null, 0f);
        }

        // Hora (6 horas)
        int hora = Mathf.FloorToInt(t / segPorHora);
        if (hora >= 6) { gano = true; terminado = true; return; }

        // Disfraces
        bool alguienEnPuerta = false;
        foreach (Bicho b in bichos)
        {
            if (b.corredor)
            {
                bool loMiro = monitor && cam == 0 && !b.EnPuerta;
                if (!b.EnPuerta) b.sinMirar = loMiro ? Mathf.Max(0f, b.sinMirar - dt * 3f) : b.sinMirar + dt;
                if (!b.EnPuerta && b.sinMirar > b.nivel)
                {
                    b.paso = b.ruta.Length - 1;
                    b.tPuerta = ataque - 1.3f;       // llega corriendo: tienes muy poco tiempo
                    Sonido.Efecto("paso_pepino", 1f, 1.3f);
                    Sonido.Efecto("paso_pepino", 0.9f, 1.4f);
                }
            }
            else if (!b.EnPuerta)
            {
                b.tMov -= dt;
                if (b.tMov <= 0f)
                {
                    b.tMov = 4.5f;
                    if (Random.value * 20f < b.nivel)
                    {
                        b.paso++;
                        if (monitor && (cam == b.ruta[b.paso - 1] || cam == b.Lugar)) tEstatica = 0.5f;
                        if (b.EnPuerta) Sonido.Efecto("paso_pepino", 0.7f, 0.9f);
                    }
                }
            }

            if (b.EnPuerta)
            {
                alguienEnPuerta = true;
                bool cerrada = b.Lugar == PuertaIzq ? puertaIzq : puertaDer;
                b.tPuerta += dt;
                if (cerrada && b.tPuerta > 2.5f)
                {
                    Sonido.Efecto("golpe", 0.8f, 0.7f);           // golpea la persiana y se va
                    b.paso = 0; b.tPuerta = 0f; b.sinMirar = 0f; b.tMov = 5f;
                }
                else if (!cerrada && b.tPuerta > ataque)
                {
                    StartCoroutine(Susto(b));
                    return;
                }
            }
        }
        Sonido.Latido(alguienEnPuerta ? 0.85f : 0f);

        // Sin energía: la última visita
        if (sinEnergia)
        {
            tSinEnergia -= dt;
            if (tSinEnergia <= 0f) { StartCoroutine(Susto(bichos[0])); return; }
        }

        Dibujar(hora);
    }

    void Dibujar(int hora)
    {
        txtHora.text = (hora == 0 ? 12 : hora) + " AM\n<size=30>Noche " + noche + "</size>";
        int barras = Mathf.CeilToInt(energia / 20f);
        string color = energia > 40f ? "#7bd389" : energia > 15f ? "#ffd166" : "#ff4d4d";
        txtEnergia.text = "Energía: <color=" + color + ">" + Mathf.CeilToInt(energia) + "%  " + new string('|', barras * 3) + "</color>";
        txtControles.text = sinEnergia ? "<color=#ff4d4d>SIN ENERGÍA</color>"
            : "[A] Puerta izq: " + (puertaIzq ? "<color=#ff4d4d>CERRADA</color>" : "abierta") +
              "     [D] Puerta der: " + (puertaDer ? "<color=#ff4d4d>CERRADA</color>" : "abierta") +
              "     [W] Cámaras";

        Bicho izq = null, der = null;
        foreach (Bicho b in bichos) { if (b.Lugar == PuertaIzq) izq = b; if (b.Lugar == PuertaDer) der = b; }
        Color sombra = new Color(0.55f, 0.55f, 0.6f, 1f);
        Color luzOficina = sinEnergia ? new Color(0.25f, 0.25f, 0.35f) : Color.white;
        if (mundo != null)
        {
            mundo.persianaIzq.enabled = puertaIzq;
            mundo.persianaDer.enabled = puertaDer;
            mundo.enPuertaIzq.enabled = izq != null && !puertaIzq;
            mundo.enPuertaDer.enabled = der != null && !puertaDer;
            if (izq != null) { mundo.enPuertaIzq.sprite = izq.sprite; mundo.enPuertaIzq.color = sombra; }
            if (der != null) { mundo.enPuertaDer.sprite = der.sprite; mundo.enPuertaDer.color = sombra; }
            mundo.oficina.color = luzOficina;
            if (mundo.luz != null) mundo.luz.enabled = !sinEnergia;
        }
        else
        {
            imgPersIzq.enabled = puertaIzq;
            imgPersDer.enabled = puertaDer;
            imgEnPuertaIzq.enabled = izq != null && !puertaIzq;
            imgEnPuertaDer.enabled = der != null && !puertaDer;
            if (izq != null) { imgEnPuertaIzq.sprite = izq.sprite; imgEnPuertaIzq.color = sombra; }
            if (der != null) { imgEnPuertaDer.sprite = der.sprite; imgEnPuertaDer.color = sombra; }
            imgOficina.color = luzOficina;
        }
        imgOscuro.color = new Color(0, 0, 0, sinEnergia ? 0.6f : 0f);

        panelCam.gameObject.SetActive(monitor);
        if (!monitor) return;
        imgVista.sprite = Arte.VistaCamara(cam);
        txtCam.text = NombresCam[cam] + "   <color=#ff4d4d>● REC</color>";
        int k = 0;
        foreach (Bicho b in bichos)
        {
            if (b.EnPuerta || b.Lugar != cam || k >= enCamara.Count) continue;
            Image img = enCamara[k];
            img.sprite = b.sprite;
            img.color = b.corredor ? new Color(1f, 0.85f, 0.4f, 1f) : new Color(0.75f, 0.9f, 0.75f, 1f);
            float cerca = (b.paso + 1f) / b.ruta.Length;                   // más grande mientras más cerca está
            float x = 0.3f + k * 0.22f, alto = 0.25f + cerca * 0.35f;
            img.rectTransform.anchorMin = new Vector2(x, 0.08f);
            img.rectTransform.anchorMax = new Vector2(x + 0.18f, 0.08f + alto);
            img.enabled = true;
            k++;
        }
        for (; k < enCamara.Count; k++) enCamara[k].enabled = false;

        tEstatica -= Time.deltaTime;
        imgEstatica.sprite = Arte.Estatica(Random.Range(0, 4));
        imgEstatica.color = new Color(1, 1, 1, tEstatica > 0f ? 0.85f : 0.12f + Random.value * 0.08f);
        string mapa = "<b>CÁMARAS</b>\n\n";
        for (int i = 0; i < NombresCam.Length; i++)
            mapa += (i == cam ? "<color=#ffffff>> " : "<color=#74a57f>   ") + NombresCam[i].Replace("  ·  ", " ") + "</color>\n";
        mapa += "\n<size=22>Teclas 1-6: cámara\nW: bajar el monitor</size>";
        if (noche >= 2) mapa += "\n\n<size=22><color=#ffd166>Vigila la CAM 1:\nel quinto disfraz corre\nsi no lo miras.</color></size>";
        txtMapa.text = mapa;
    }

    IEnumerator Susto(Bicho b)
    {
        terminado = true;
        gano = false;
        panelCam.gameObject.SetActive(false);
        imgCara.gameObject.SetActive(true);
        imgCara.color = b.nombre == "Maniquí" ? new Color(0.9f, 0.9f, 0.95f) : Color.white;
        Sonido.Efecto("grito", 1f, 0.8f);
        Sonido.Efecto("susto", 1f, 1f);
        Juego.I.ui.Golpe(0.8f);
        float tt = 0f;
        while (tt < 0.9f)
        {
            tt += Time.deltaTime;
            float s = Mathf.Lerp(0.4f, 1.35f, Mathf.Clamp01(tt / 0.25f));
            imgCara.rectTransform.localScale = new Vector3(s, s, 1f);
            imgCara.rectTransform.anchoredPosition = new Vector2(Random.Range(-25f, 25f), Random.Range(-25f, 25f));
            yield return null;
        }
        imgCara.gameObject.SetActive(false);
        imgOscuro.color = new Color(0.25f, 0f, 0f, 0.9f);
    }

    IEnumerator Final()
    {
        Sonido.Latido(0f);
        Sonido.Ambiente(null, 0f);
        if (gano)
        {
            panelCam.gameObject.SetActive(false);
            Sonido.Efecto("ascensor", 1f, 1f);
            Sonido.Efecto("logro", 0.8f, 1f);
            txtCentro.text = "6:00 AM";
            yield return new WaitForSeconds(1.2f);
            txtCentro.text = "6:00 AM\n<size=40><color=#7bd389>¡Sobreviviste la noche " + noche + "!</color></size>";
        }
        else
        {
            yield return new WaitForSeconds(0.9f);
            txtCentro.text = "<color=#ff4d4d>TE ATRAPARON</color>\n<size=40>Noche " + noche + "</size>";
        }
        yield return new WaitForSeconds(2.5f);
    }
}
