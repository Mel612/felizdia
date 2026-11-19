using UnityEngine;

// =====================================================================
//  SONIDO: música, efectos y ambiente. Vive entre escenas (la música
//  no se corta al subir de piso).
//    Sonido.Efecto("susto");          un efecto
//    Sonido.Musica("m_tension");      cambia la música (con fundido)
//    Sonido.Ambiente("alarma", 0.5f); un sonido en bucle (null para quitarlo)
//    Sonido.Latido(0..1);             latido del corazón según el peligro
//  Tecla M: silenciar / activar el sonido.
// =====================================================================
public class Sonido : MonoBehaviour
{
    public static float VolumenMusica = 0.5f;
    public static float VolumenEfectos = 0.9f;
    public static bool Silencio;

    static Sonido inst;
    AudioSource musA, musB, ambiente, latido;
    AudioSource[] efectos;
    int siguienteEfecto;
    string tema;
    bool usandoA = true;
    float fundido = 1f, velFundido = 1f;
    float latidoObjetivo;
    float volAmbiente;
    float volMusicaExtra = 1f, volMusicaExtraObjetivo = 1f;

    static Sonido I
    {
        get
        {
            if (inst == null)
            {
                GameObject go = new GameObject("Sonido (música y efectos)");
                DontDestroyOnLoad(go);
                inst = go.AddComponent<Sonido>();
                inst.Preparar();
            }
            return inst;
        }
    }

    void Preparar()
    {
        musA = Fuente(true); musB = Fuente(true);
        ambiente = Fuente(true);
        latido = Fuente(true);
        efectos = new AudioSource[10];
        for (int i = 0; i < efectos.Length; i++) efectos[i] = Fuente(false);
        StartCoroutine(Precargar());
    }

    // Genera los sonidos de a uno por cuadro para que el juego no se trabe la primera vez.
    System.Collections.IEnumerator Precargar()
    {
        foreach (string n in Sintetizador.Temas) { Sintetizador.Clip(n); yield return null; }
        foreach (string n in Sintetizador.Efectos) { Sintetizador.Clip(n); yield return null; }
    }

    AudioSource Fuente(bool bucle)
    {
        AudioSource a = gameObject.AddComponent<AudioSource>();
        a.playOnAwake = false;
        a.loop = bucle;
        a.spatialBlend = 0f;
        return a;
    }

    // ---------------------------------------------------------------
    public static void Efecto(string nombre) { Efecto(nombre, 1f, 1f); }

    public static void Efecto(string nombre, float vol, float tono)
    {
        if (!Application.isPlaying) return;
        if (string.IsNullOrEmpty(nombre)) return;
        AudioClip c = Sintetizador.Clip(nombre);
        if (c == null) return;
        Sonido s = I;
        AudioSource a = s.efectos[s.siguienteEfecto];
        s.siguienteEfecto = (s.siguienteEfecto + 1) % s.efectos.Length;
        a.pitch = tono;
        a.PlayOneShot(c, Silencio ? 0f : vol * VolumenEfectos);
    }

    public static void Musica(string nombre) { Musica(nombre, 1.5f); }

    public static void Musica(string nombre, float segundosFundido)
    {
        if (!Application.isPlaying) return;
        Sonido s = I;
        if (s.tema == nombre) return;
        s.tema = nombre;
        AudioSource nueva = s.usandoA ? s.musB : s.musA;
        s.usandoA = !s.usandoA;
        nueva.clip = string.IsNullOrEmpty(nombre) ? null : Sintetizador.Clip(nombre);
        nueva.volume = 0f;
        if (nueva.clip != null) nueva.Play(); else nueva.Stop();
        s.fundido = 0f;
        s.velFundido = 1f / Mathf.Max(0.05f, segundosFundido);
    }

    public static string TemaActual { get { return inst != null ? inst.tema : null; } }

    // Baja la música un rato (por ejemplo durante un susto o un diálogo importante).
    public static void BajarMusica(float factor) { if (!Application.isPlaying) return; I.volMusicaExtraObjetivo = Mathf.Clamp01(factor); }

    public static void Ambiente(string nombre, float vol)
    {
        if (!Application.isPlaying) return;
        Sonido s = I;
        AudioClip c = string.IsNullOrEmpty(nombre) ? null : Sintetizador.Clip(nombre);
        if (c == null) { s.ambiente.Stop(); s.ambiente.clip = null; return; }
        if (s.ambiente.clip != c) { s.ambiente.clip = c; s.ambiente.Play(); }
        s.volAmbiente = vol;
    }

    public static void Latido(float intensidad)
    {
        if (!Application.isPlaying) return;
        Sonido s = I;
        s.latidoObjetivo = Mathf.Clamp01(intensidad);
        if (s.latidoObjetivo > 0f && !s.latido.isPlaying)
        {
            s.latido.clip = Sintetizador.Clip("latido");
            s.latido.Play();
        }
    }

    // Detiene todo (por ejemplo al empezar una cinemática en silencio).
    public static void Callar()
    {
        Musica(null, 0.6f);
        Ambiente(null, 0f);
        Latido(0f);
    }

    // ---------------------------------------------------------------
    void Update()
    {
        if (Entrada.Bajada(Tecla.Silencio))
        {
            Silencio = !Silencio;
            if (Juego.I != null && Juego.I.ui != null) Juego.I.ui.Toast(Silencio ? "Sonido desactivado (M)" : "Sonido activado (M)", 2f);
        }

        fundido = Mathf.MoveTowards(fundido, 1f, velFundido * Time.unscaledDeltaTime);
        volMusicaExtra = Mathf.MoveTowards(volMusicaExtra, volMusicaExtraObjetivo, Time.unscaledDeltaTime * 1.5f);
        float vm = Silencio ? 0f : VolumenMusica * volMusicaExtra;
        AudioSource actual = usandoA ? musA : musB, vieja = usandoA ? musB : musA;
        actual.volume = fundido * vm;
        vieja.volume = (1f - fundido) * vm;
        if (fundido >= 1f && vieja.isPlaying) vieja.Stop();

        // Latido: más fuerte y rápido mientras más cerca está el peligro
        float lv = Silencio ? 0f : latidoObjetivo;
        latido.volume = Mathf.MoveTowards(latido.volume, lv * VolumenEfectos, Time.unscaledDeltaTime * 2f);
        latido.pitch = 0.85f + latidoObjetivo * 0.6f;
        if (latido.volume <= 0.001f && latidoObjetivo <= 0f && latido.isPlaying) latido.Stop();
        ambiente.volume = Silencio ? 0f : volAmbiente * VolumenEfectos;
    }
}
