using System;
using System.Collections.Generic;
using UnityEngine;

// =====================================================================
//  SINTETIZADOR: todos los sonidos y la música del juego se generan por
//  código (no hay archivos de audio). Cada sonido es una "receta" que
//  mezcla ondas, ruido, envolventes, filtros y eco.
//  Para cambiar un sonido, busca su nombre en Generar().
// =====================================================================
public static class Sintetizador
{
    public const int FS = 22050;                 // muestras por segundo
    static readonly Dictionary<string, AudioClip> cache = new Dictionary<string, AudioClip>();

    public static readonly string[] Efectos =
    {
        "paso", "paso_pepino", "latido", "susto", "grito", "cuchillo", "golpe", "caida", "electricidad", "cristal",
        "alarma", "sello", "logro", "click", "blip", "estatica", "apagon", "ascensor", "puerta", "salto",
        "respiracion", "zumbido", "trueno"
    };
    public static readonly string[] Temas = { "m_menu", "m_campus", "m_noche", "m_tension", "m_persecucion", "m_fiesta", "m_triste", "m_oscuro", "m_morenada" };

    public static AudioClip Clip(string nombre)
    {
        AudioClip c;
        if (cache.TryGetValue(nombre, out c) && c != null) return c;
        float[] d = Generar(nombre);
        if (d == null) return null;
        Normalizar(d, nombre.StartsWith("m_") ? 0.8f : 0.9f);
        c = AudioClip.Create(nombre, d.Length, 1, FS, false);
        c.SetData(d, 0);
        cache[nombre] = c;
        return c;
    }

    // =================================================================
    //  RECETAS
    // =================================================================
    static float[] Generar(string n)
    {
        switch (n)
        {
            // ---------------- Efectos ----------------
            case "paso": return Paso(0.9f, 1);
            case "paso_pepino": return Paso(0.6f, 2);
            case "latido":
            {
                float[] b = Buf(0.75f);
                Golpe(b, 0f, 62f, 0.16f, 1f);
                Golpe(b, 0.22f, 55f, 0.18f, 0.75f);
                return b;
            }
            case "susto":
            {
                float[] b = Buf(2.2f);
                float[] fr = { 220f, 233.1f, 311.1f, 466.2f, 493.9f };          // racimo disonante
                foreach (float f in fr)
                    for (int i = 0; i < b.Length; i++)
                    {
                        float t = T(i);
                        float env = Mathf.Min(1f, t / 0.02f) * Mathf.Exp(-t * 1.6f);
                        b[i] += Sierra(f * (1f + 0.004f * Mathf.Sin(t * 30f)), t) * env * 0.25f;
                    }
                Ruido(b, 0f, 0.4f, 0.5f, 7, 0.02f, 2.5f);
                PasaBajos(b, 3200f);
                Eco(b, 0.11f, 0.35f);
                return b;
            }
            case "grito":
            {
                float[] b = Buf(1.4f);
                float fase = 0f;
                for (int i = 0; i < b.Length; i++)
                {
                    float t = T(i);
                    float f = 780f + 380f * Mathf.Sin(Mathf.Min(t * 2.2f, 1.6f)) + 25f * Mathf.Sin(t * 44f);
                    fase += f / FS;
                    float env = Mathf.Min(1f, t / 0.05f) * (t > 1f ? Mathf.Max(0f, 1f - (t - 1f) / 0.4f) : 1f);
                    b[i] = (Frac(fase) * 2f - 1f) * 0.6f * env + (Frac(fase * 2.01f) * 2f - 1f) * 0.25f * env;
                }
                Ruido(b, 0f, 1.3f, 0.15f, 11, 0.02f, 0.5f);
                PasaBajos(b, 2600f);
                Eco(b, 0.09f, 0.25f);
                return b;
            }
            case "cuchillo":
            {
                float[] b = Buf(0.9f);
                float[] fr = { 3100f, 4720f, 6180f, 7900f };
                foreach (float f in fr)
                    for (int i = 0; i < b.Length; i++) { float t = T(i); b[i] += Mathf.Sin(2f * Mathf.PI * f * t) * Mathf.Exp(-t * 7f) * 0.15f; }
                Ruido(b, 0f, 0.12f, 0.6f, 3, 0.002f, 25f);                      // "shhk"
                Golpe(b, 0.18f, 90f, 0.12f, 0.9f);                               // impacto
                Ruido(b, 0.18f, 0.25f, 0.5f, 5, 0.001f, 18f);
                return b;
            }
            case "golpe":
            {
                float[] b = Buf(0.5f);
                Golpe(b, 0f, 70f, 0.25f, 1f);
                Ruido(b, 0f, 0.2f, 0.45f, 13, 0.001f, 20f);
                PasaBajos(b, 1800f);
                return b;
            }
            case "caida":
            {
                float[] b = Buf(1.4f);
                for (int i = 0; i < (int)(0.9f * FS); i++)
                {
                    float t = T(i);
                    b[i] = (Azar(i * 7 + 3) * 2f - 1f) * Mathf.Sin(Mathf.PI * t / 0.9f) * 0.5f;
                }
                PasaBajos(b, 900f);
                Golpe(b, 0.9f, 55f, 0.35f, 1.2f);
                Ruido(b, 0.9f, 0.3f, 0.6f, 17, 0.001f, 12f);
                return b;
            }
            case "electricidad":
            {
                float[] b = Buf(1.5f);
                for (int i = 0; i < b.Length; i++)
                {
                    float t = T(i);
                    float env = Mathf.Min(1f, t / 0.03f) * Mathf.Max(0f, 1f - t / 1.5f);
                    b[i] = (Cuadrada(60f, t) * 0.3f + Cuadrada(120f, t) * 0.2f + Sierra(180f, t) * 0.15f) * env;
                    if (Azar(i / 40) > 0.9f) b[i] += (Azar(i) * 2f - 1f) * 0.8f * env;   // chispazos
                }
                return b;
            }
            case "cristal":
            {
                float[] b = Buf(1.2f);
                Ruido(b, 0f, 0.9f, 0.7f, 21, 0.001f, 5f);
                PasaAltos(b, 2500f);
                System.Random r = new System.Random(5);
                for (int k = 0; k < 14; k++)
                {
                    float t0 = (float)r.NextDouble() * 0.6f, f = 2500f + (float)r.NextDouble() * 5000f;
                    for (int i = (int)(t0 * FS); i < b.Length; i++) { float t = T(i) - t0; b[i] += Mathf.Sin(2f * Mathf.PI * f * t) * Mathf.Exp(-t * 12f) * 0.2f; }
                }
                Golpe(b, 0f, 80f, 0.2f, 0.7f);
                return b;
            }
            case "alarma":
            {
                float[] b = Buf(1.4f);
                for (int i = 0; i < b.Length; i++)
                {
                    float t = T(i);
                    float f = ((int)(t / 0.35f)) % 2 == 0 ? 720f : 960f;
                    b[i] = (Cuadrada(f, t) * 0.5f + Mathf.Sin(2f * Mathf.PI * f * 2f * t) * 0.2f) * 0.6f;
                }
                PasaBajos(b, 3000f);
                return b;
            }
            case "sello":
            {
                float[] b = Buf(1.4f);
                Campana(b, 0f, 880f, 1.2f, 0.6f);
                Campana(b, 0.08f, 1318.5f, 1.1f, 0.45f);
                Campana(b, 0.16f, 1760f, 1f, 0.35f);
                Eco(b, 0.13f, 0.3f);
                return b;
            }
            case "logro":
            {
                float[] b = Buf(1.5f);
                float[] fr = { 523.3f, 659.3f, 784f, 1046.5f };
                for (int k = 0; k < fr.Length; k++) Campana(b, k * 0.09f, fr[k], 1f, 0.45f);
                Eco(b, 0.12f, 0.3f);
                return b;
            }
            case "click":
            {
                float[] b = Buf(0.05f);
                for (int i = 0; i < b.Length; i++) { float t = T(i); b[i] = Mathf.Sin(2f * Mathf.PI * 1400f * t) * Mathf.Exp(-t * 120f); }
                return b;
            }
            case "blip":
            {
                float[] b = Buf(0.035f);
                for (int i = 0; i < b.Length; i++) { float t = T(i); b[i] = Cuadrada(520f, t) * Mathf.Exp(-t * 60f) * 0.5f; }
                PasaBajos(b, 2500f);
                return b;
            }
            case "estatica":
            {
                float[] b = Buf(1.2f);
                Ruido(b, 0f, 1.2f, 0.5f, 31, 0.05f, 0.5f);
                PasaAltos(b, 900f);
                PasaBajos(b, 4500f);
                for (int i = 0; i < b.Length; i++) if (Azar(i / 120 + 999) > 0.93f) b[i] *= 2.5f;   // crujidos
                return b;
            }
            case "apagon":
            {
                float[] b = Buf(1.3f);
                float fase = 0f;
                for (int i = 0; i < b.Length; i++)
                {
                    float t = T(i);
                    float f = Mathf.Lerp(240f, 30f, Mathf.Clamp01(t / 1.1f));
                    fase += f / FS;
                    b[i] = (Frac(fase) * 2f - 1f) * Mathf.Max(0f, 1f - t / 1.3f) * 0.5f;
                }
                PasaBajos(b, 1200f);
                Golpe(b, 0f, 100f, 0.1f, 0.6f);
                return b;
            }
            case "ascensor":
            {
                float[] b = Buf(1.6f);
                Campana(b, 0f, 1046.5f, 1.4f, 0.5f);
                Campana(b, 0.35f, 784f, 1.4f, 0.5f);
                return b;
            }
            case "puerta":
            {
                float[] b = Buf(1.1f);
                float fase = 0f;
                for (int i = 0; i < b.Length; i++)
                {
                    float t = T(i);
                    float f = 110f + 60f * Mathf.Sin(t * 5f) + 40f * (Azar(i / 300) - 0.5f);
                    fase += f / FS;
                    float env = Mathf.Sin(Mathf.PI * Mathf.Clamp01(t / 1.1f));
                    b[i] = (Frac(fase) < 0.12f ? 1f : -0.1f) * env * 0.5f;
                }
                PasaBajos(b, 1600f);
                Eco(b, 0.07f, 0.2f);
                return b;
            }
            case "salto":
            {
                float[] b = Buf(0.15f);
                float fase = 0f;
                for (int i = 0; i < b.Length; i++) { float t = T(i); fase += (300f + t * 3000f) / FS; b[i] = Mathf.Sin(2f * Mathf.PI * fase) * Mathf.Exp(-t * 18f) * 0.6f; }
                return b;
            }
            case "respiracion":
            {
                float[] b = Buf(2f);
                for (int i = 0; i < b.Length; i++)
                {
                    float t = T(i);
                    float env = Mathf.Pow(Mathf.Sin(Mathf.PI * (t % 1f)), 2f) * (t < 1f ? 0.8f : 0.5f);
                    b[i] = (Azar(i * 3 + 1) * 2f - 1f) * env * 0.5f;
                }
                PasaBajos(b, 1100f);
                PasaAltos(b, 250f);
                return b;
            }
            case "zumbido":          // tubo fluorescente (ambiente)
            {
                float[] b = Buf(2f);
                for (int i = 0; i < b.Length; i++) { float t = T(i); b[i] = Mathf.Sin(2f * Mathf.PI * 100f * t) * 0.4f + Mathf.Sin(2f * Mathf.PI * 200f * t) * 0.2f + Mathf.Sin(2f * Mathf.PI * 300f * t) * 0.1f; }
                return b;
            }
            case "trueno":
            {
                float[] b = Buf(3f);
                Ruido(b, 0f, 3f, 1f, 41, 0.01f, 1.4f);
                PasaBajos(b, 400f);
                Golpe(b, 0f, 45f, 0.6f, 1f);
                return b;
            }

            // ---------------- Música (bucles) ----------------
            case "m_menu": return MusicaMenu();
            case "m_campus": return MusicaCampus(false);
            case "m_noche": return MusicaCampus(true);
            case "m_tension": return MusicaTension();
            case "m_persecucion": return MusicaPersecucion();
            case "m_fiesta": return MusicaFiesta();
            case "m_triste": return MusicaTriste();
            case "m_oscuro": return MusicaOscuro();
            case "m_morenada": return MusicaMorenada();
        }
        return null;
    }

    // =================================================================
    //  MÚSICA
    // =================================================================
    static float Nota(int midi) { return 440f * Mathf.Pow(2f, (midi - 69) / 12f); }

    // Caja de música desafinada en La menor + zumbido grave. Lenta e inquietante.
    static float[] MusicaMenu()
    {
        float bpm = 66f, negra = 60f / bpm;
        int[] mel = { 69, 72, 76, 74, 72, 71, 72, 69, 68, 71, 74, 72, 71, 69, 68, 64 };
        float largo = mel.Length * negra;
        float[] b = Buf(largo + 3f);
        for (int k = 0; k < mel.Length; k++)
        {
            float desafino = (k % 3 == 1) ? 1.012f : 1f;          // caja de música vieja
            Campana(b, k * negra, Nota(mel[k] + 12) * desafino, 2.2f, 0.32f);
            if (k % 4 == 0) Campana(b, k * negra, Nota(mel[k] - 12), 3f, 0.2f);
        }
        Pad(b, 0f, largo, new[] { Nota(45), Nota(52) }, 0.16f, 0.3f);
        Eco(b, 0.37f, 0.35f);
        return Plegar(b, largo);
    }

    // Arpegios de piano eléctrico (Am - F - C - G). De noche: más lento, grave y con zumbido.
    static float[] MusicaCampus(bool noche)
    {
        float bpm = noche ? 84f : 100f, corchea = 30f / bpm;
        int[][] acordes = noche
            ? new[] { new[] { 57, 60, 64 }, new[] { 53, 57, 60 }, new[] { 52, 56, 59 }, new[] { 57, 60, 63 } }   // Am F E Am(b5)
            : new[] { new[] { 57, 60, 64 }, new[] { 53, 57, 60 }, new[] { 48, 52, 55 }, new[] { 55, 59, 62 } };  // Am F C G
        int[] patron = { 0, 1, 2, 1, 0, 1, 2, 1 };
        float largo = acordes.Length * 8 * corchea;
        float[] b = Buf(largo + 2f);
        for (int c = 0; c < acordes.Length; c++)
        {
            for (int k = 0; k < 8; k++)
            {
                float t0 = (c * 8 + k) * corchea;
                Piano(b, t0, Nota(acordes[c][patron[k]] + (noche ? -12 : 0) + 12), noche ? 0.9f : 0.6f, 0.22f);
            }
            Piano(b, c * 8 * corchea, Nota(acordes[c][0] - 12), 2f, 0.3f);
            if (!noche) for (int k = 0; k < 8; k++) Ruido(b, (c * 8 + k) * corchea + corchea * 0.5f, 0.05f, 0.05f, 50 + k, 0.002f, 60f);
        }
        if (noche) Pad(b, 0f, largo, new[] { Nota(33), Nota(40) }, 0.18f, 0.5f);
        PasaBajos(b, noche ? 2000f : 3500f);
        Eco(b, corchea * 3f, noche ? 0.4f : 0.25f);
        return Plegar(b, largo);
    }

    // Pulso grave + cuerdas disonantes + golpes metálicos (el Pepino te está cazando).
    static float[] MusicaTension()
    {
        float bpm = 96f, corchea = 30f / bpm;
        int pulsos = 32;
        float largo = pulsos * corchea;
        float[] b = Buf(largo + 2f);
        for (int k = 0; k < pulsos; k++)
        {
            float t0 = k * corchea;
            Golpe(b, t0, Nota(33), 0.18f, k % 4 == 0 ? 0.9f : 0.55f);
            if (k % 8 == 6) Campana(b, t0, Nota(82 + (k % 16 == 6 ? 0 : 1)), 0.8f, 0.12f);
        }
        Pad(b, 0f, largo, new[] { Nota(69), Nota(70), Nota(75) }, 0.12f, 1.2f);
        Eco(b, corchea * 2f, 0.3f);
        return Plegar(b, largo);
    }

    // Persecución: batería rápida, bajo ostinato y golpes de orquesta.
    static float[] MusicaPersecucion()
    {
        float bpm = 152f, sem = 15f / bpm;           // semicorchea
        int pasos = 64;
        float largo = pasos * sem;
        float[] b = Buf(largo + 1.5f);
        int[] bajo = { 33, 33, 36, 33, 34, 33, 31, 33 };
        for (int k = 0; k < pasos; k++)
        {
            float t0 = k * sem;
            if (k % 4 == 0) Golpe(b, t0, 60f, 0.15f, 1f);                                 // bombo
            if (k % 8 == 4) Ruido(b, t0, 0.12f, 0.6f, 60 + k, 0.001f, 25f);               // caja
            if (k % 2 == 1) Ruido(b, t0, 0.03f, 0.12f, 90 + k, 0.0005f, 90f);             // hi-hat
            if (k % 2 == 0) Bajo(b, t0, Nota(bajo[(k / 2) % bajo.Length]), sem * 1.8f, 0.4f);
            if (k == 0 || k == 24 || k == 32 || k == 56) Pad(b, t0, sem * 3f, new[] { Nota(57), Nota(60), Nota(63) }, 0.35f, 0.01f);
        }
        PasaBajos(b, 5000f);
        return Plegar(b, largo);
    }

    // Cumbia de fiesta (la Bienvenida): güiro, bajo 1-5 y teclado a contratiempo.
    static float[] MusicaFiesta()
    {
        float bpm = 98f, corchea = 30f / bpm;
        int[][] acordes = { new[] { 57, 60, 64 }, new[] { 55, 59, 62 }, new[] { 53, 57, 60 }, new[] { 52, 56, 59 } };   // Am G F E
        float largo = acordes.Length * 8 * corchea;
        float[] b = Buf(largo + 1.5f);
        for (int c = 0; c < acordes.Length; c++)
            for (int k = 0; k < 8; k++)
            {
                float t0 = (c * 8 + k) * corchea;
                // Güiro: largo-corto-corto
                Ruido(b, t0, k % 2 == 0 ? corchea * 0.8f : corchea * 0.35f, 0.18f, 200 + k, 0.01f, 8f);
                if (k == 0 || k == 4) Golpe(b, t0, 62f, 0.12f, 0.7f);
                if (k == 0) Bajo(b, t0, Nota(acordes[c][0] - 24), corchea * 1.6f, 0.5f);
                if (k == 3) Bajo(b, t0, Nota(acordes[c][2] - 24), corchea * 0.9f, 0.4f);
                if (k == 4) Bajo(b, t0, Nota(acordes[c][0] - 24), corchea * 1.6f, 0.5f);
                if (k % 2 == 1) foreach (int nn in acordes[c]) Piano(b, t0, Nota(nn + 12), 0.18f, 0.1f);
            }
        PasaAltos(b, 60f);
        return Plegar(b, largo);
    }

    // Morenada paceña (la Bienvenida): paso pesado de bombo y platillo, la matraca
    // girando, la banda de bronces con la melodía en menor y una tarka respondiendo.
    static float[] MusicaMorenada()
    {
        float bpm = 84f, negra = 60f / bpm, sem = negra / 4f;
        int compases = 8;
        float largo = compases * 4 * negra;
        float[] b = Buf(largo + 2f);
        // Re menor: Dm  Gm  A7  Dm | Dm  C  Bb  A7
        int[][] acordes = { new[] { 50, 53, 57 }, new[] { 55, 58, 62 }, new[] { 57, 61, 64 }, new[] { 50, 53, 57 },
                            new[] { 50, 53, 57 }, new[] { 48, 52, 55 }, new[] { 46, 50, 53 }, new[] { 57, 61, 64 } };
        // Melodía de bronces (negra = 4 semicorcheas). 0 = silencio
        int[] mel = { 69, 0, 69, 70,  69, 67, 65, 64,   67, 0, 67, 69,  70, 69, 67, 65,
                      64, 0, 64, 65,  67, 65, 64, 62,   65, 64, 62, 61,  62, 0, 0, 0,
                      69, 0, 69, 70,  72, 70, 69, 67,   67, 0, 64, 65,  67, 65, 64, 62,
                      65, 0, 65, 64,  62, 61, 62, 64,   61, 0, 57, 61,  62, 0, 0, 0 };
        for (int c = 0; c < compases; c++)
            for (int k = 0; k < 16; k++)
            {
                float t0 = (c * 16 + k) * sem;
                // Bombo en 1 y 3 (paso de morenada), platillo en 2 y 4
                if (k == 0 || k == 8) Golpe(b, t0, 55f, 0.22f, 1f);
                if (k == 6) Golpe(b, t0, 60f, 0.12f, 0.5f);
                if (k == 4 || k == 12) Ruido(b, t0, 0.35f, 0.32f, 400 + c * 16 + k, 0.002f, 6f);
                // Matraca: ráfaga de golpecitos secos
                if (k % 2 == 1) Ruido(b, t0, 0.05f, 0.14f, 500 + c * 16 + k, 0.0005f, 70f);
                // Bajo (tuba)
                if (k == 0 || k == 8) Bajo(b, t0, Nota(acordes[c][0] - 12), negra * 1.6f, 0.55f);
                if (k == 4 || k == 12) Bajo(b, t0, Nota(acordes[c][2] - 24), negra * 0.9f, 0.4f);
                // Acordes de la banda
                if (k == 2 || k == 10) foreach (int nn in acordes[c]) Bronce(b, t0, Nota(nn), sem * 1.6f, 0.07f);
                // Melodía
                int nota = mel[(c * 16 + k) % mel.Length];
                if (nota > 0) Bronce(b, t0, Nota(nota), sem * 1.8f, 0.2f);
                // Tarka respondiendo en los compases pares
                if (c % 2 == 1 && (k == 2 || k == 6 || k == 10)) Tarka(b, t0, Nota(acordes[c][1] + 24), sem * 1.5f, 0.06f);
            }
        PasaBajos(b, 6000f);
        Eco(b, 0.22f, 0.18f);
        return Plegar(b, largo);
    }

    // Bronce: sierra con ataque suave y vibrato (trompeta / bombardino)
    static void Bronce(float[] b, float t0, float f, float dur, float vol)
    {
        int i0 = (int)(t0 * FS), n = (int)((dur + 0.08f) * FS);
        for (int k = 0; k < n && i0 + k < b.Length; k++)
        {
            float t = k / (float)FS;
            float env = Mathf.Min(1f, t / 0.03f) * (t > dur ? Mathf.Max(0f, 1f - (t - dur) / 0.08f) : 1f);
            float fv = f * (1f + 0.006f * Mathf.Sin(2f * Mathf.PI * 5.5f * t));
            b[i0 + k] += (Sierra(fv, t) * 0.6f + Mathf.Sin(2f * Mathf.PI * fv * t) * 0.5f) * env * vol;
        }
    }

    // Tarka: flauta cuadrada, áspera y con aire
    static void Tarka(float[] b, float t0, float f, float dur, float vol)
    {
        int i0 = (int)(t0 * FS), n = (int)(dur * FS);
        for (int k = 0; k < n && i0 + k < b.Length; k++)
        {
            float t = k / (float)FS;
            float env = Mathf.Min(1f, t / 0.02f) * Mathf.Max(0f, 1f - t / dur);
            float cuad = Mathf.Sin(2f * Mathf.PI * f * t) > 0f ? 1f : -1f;
            b[i0 + k] += (cuad * 0.5f + (Azar(i0 + k) * 2f - 1f) * 0.25f) * env * vol;
        }
    }

    // Piano lento y triste (finales).
    static float[] MusicaTriste()
    {
        float bpm = 60f, negra = 60f / bpm;
        int[][] acordes = { new[] { 57, 64, 72 }, new[] { 53, 60, 69 }, new[] { 48, 55, 64 }, new[] { 52, 59, 68 } };
        int[] mel = { 76, 74, 72, 71, 72, 69, 71, 68 };
        float largo = acordes.Length * 2 * negra;
        float[] b = Buf(largo + 3f);
        for (int c = 0; c < acordes.Length; c++)
        {
            float t0 = c * 2 * negra;
            for (int k = 0; k < 3; k++) Piano(b, t0 + k * 0.12f, Nota(acordes[c][k]), 2.4f, 0.22f);
            Piano(b, t0, Nota(mel[c * 2]), 1.2f, 0.3f);
            Piano(b, t0 + negra, Nota(mel[c * 2 + 1]), 1.2f, 0.26f);
        }
        Eco(b, 0.45f, 0.35f);
        return Plegar(b, largo);
    }

    // Zumbido oscuro con oleadas (final oscuro / pesadilla).
    static float[] MusicaOscuro()
    {
        float largo = 12f;
        float[] b = Buf(largo + 2f);
        Pad(b, 0f, largo, new[] { Nota(28), Nota(35), Nota(41) }, 0.35f, 2f);
        for (int i = 0; i < b.Length; i++)
        {
            float t = T(i);
            b[i] *= 0.6f + 0.4f * Mathf.Sin(2f * Mathf.PI * t / largo * 2f);
        }
        Ruido(b, 3f, 3f, 0.15f, 300, 2.5f, 0.3f);
        Ruido(b, 9f, 2.5f, 0.12f, 301, 2f, 0.3f);
        PasaBajos(b, 700f);
        Eco(b, 0.6f, 0.4f);
        return Plegar(b, largo);
    }

    // =================================================================
    //  INSTRUMENTOS
    // =================================================================
    static float[] Paso(float tono, int semilla)
    {
        float[] b = Buf(0.12f);
        for (int i = 0; i < b.Length; i++)
        {
            float t = T(i);
            b[i] = (Azar(i * 13 + semilla) * 2f - 1f) * Mathf.Exp(-t * 55f) * 0.7f + Mathf.Sin(2f * Mathf.PI * 90f * tono * t) * Mathf.Exp(-t * 40f) * 0.6f;
        }
        PasaBajos(b, 1400f * tono);
        return b;
    }

    // Golpe grave con caída de tono (bombo, latido, impactos).
    static void Golpe(float[] b, float t0, float f, float dur, float vol)
    {
        int i0 = (int)(t0 * FS), n = (int)(dur * FS * 2f);
        float fase = 0f;
        for (int k = 0; k < n && i0 + k < b.Length; k++)
        {
            float t = k / (float)FS;
            fase += f * (1f + 2.5f * Mathf.Exp(-t * 40f)) / FS;
            b[i0 + k] += Mathf.Sin(2f * Mathf.PI * fase) * Mathf.Exp(-t / dur * 3f) * vol;
        }
    }

    static void Campana(float[] b, float t0, float f, float dur, float vol)
    {
        int i0 = (int)(t0 * FS), n = (int)(dur * FS);
        for (int k = 0; k < n && i0 + k < b.Length; k++)
        {
            float t = k / (float)FS;
            float env = Mathf.Min(1f, t / 0.003f) * Mathf.Exp(-t / dur * 4f);
            b[i0 + k] += (Mathf.Sin(2f * Mathf.PI * f * t) + 0.4f * Mathf.Sin(2f * Mathf.PI * f * 2.76f * t) * Mathf.Exp(-t * 6f)
                          + 0.2f * Mathf.Sin(2f * Mathf.PI * f * 5.4f * t) * Mathf.Exp(-t * 12f)) * env * vol;
        }
    }

    static void Piano(float[] b, float t0, float f, float dur, float vol)
    {
        int i0 = (int)(t0 * FS), n = (int)((dur + 0.3f) * FS);
        for (int k = 0; k < n && i0 + k < b.Length; k++)
        {
            float t = k / (float)FS;
            float env = Mathf.Min(1f, t / 0.005f) * Mathf.Exp(-t * 2.2f) * (t > dur ? Mathf.Max(0f, 1f - (t - dur) / 0.3f) : 1f);
            b[i0 + k] += (Mathf.Sin(2f * Mathf.PI * f * t) + 0.35f * Mathf.Sin(4f * Mathf.PI * f * t) + 0.12f * Mathf.Sin(6f * Mathf.PI * f * t)) * env * vol;
        }
    }

    static void Bajo(float[] b, float t0, float f, float dur, float vol)
    {
        int i0 = (int)(t0 * FS), n = (int)(dur * FS);
        for (int k = 0; k < n && i0 + k < b.Length; k++)
        {
            float t = k / (float)FS;
            float env = Mathf.Min(1f, t / 0.004f) * Mathf.Max(0f, 1f - t / dur);
            b[i0 + k] += (Mathf.Sin(2f * Mathf.PI * f * t) * 0.8f + Sierra(f, t) * 0.25f) * env * vol;
        }
    }

    // Colchón sostenido (cuerdas / sintetizador), con ataque lento.
    static void Pad(float[] b, float t0, float dur, float[] fr, float vol, float ataque)
    {
        int i0 = (int)(t0 * FS), n = (int)(dur * FS);
        for (int k = 0; k < n && i0 + k < b.Length; k++)
        {
            float t = k / (float)FS;
            float env = Mathf.Min(1f, t / Mathf.Max(ataque, 0.001f)) * Mathf.Min(1f, (dur - t) / Mathf.Max(ataque, 0.05f));
            float s = 0f;
            foreach (float f in fr)
                s += Sierra(f * 1.003f, t) * 0.5f + Sierra(f * 0.997f, t + 0.13f) * 0.5f;
            b[i0 + k] += s / fr.Length * env * vol;
        }
    }

    static void Ruido(float[] b, float t0, float dur, float vol, int semilla, float ataque, float caida)
    {
        int i0 = (int)(t0 * FS), n = (int)(dur * FS);
        for (int k = 0; k < n && i0 + k < b.Length; k++)
        {
            float t = k / (float)FS;
            float env = Mathf.Min(1f, t / Mathf.Max(ataque, 0.0001f)) * Mathf.Exp(-t * caida);
            b[i0 + k] += (Azar(k * 31 + semilla * 7919) * 2f - 1f) * env * vol;
        }
    }

    // =================================================================
    //  PROCESOS
    // =================================================================
    static float[] Buf(float seg) { return new float[Mathf.Max(1, (int)(seg * FS))]; }
    static float T(int i) { return i / (float)FS; }
    static float Frac(float x) { return x - Mathf.Floor(x); }
    static float Sierra(float f, float t) { return Frac(f * t) * 2f - 1f; }
    static float Cuadrada(float f, float t) { return Frac(f * t) < 0.5f ? 1f : -1f; }

    // Ruido blanco repetible (sin estado).
    static float Azar(int n)
    {
        unchecked
        {
            uint x = (uint)n * 747796405u + 2891336453u;
            x = ((x >> (int)((x >> 28) + 4u)) ^ x) * 277803737u;
            x = (x >> 22) ^ x;
            return (x & 0xFFFFFF) / 16777215f;
        }
    }

    static void PasaBajos(float[] b, float corte)
    {
        float a = 1f - Mathf.Exp(-2f * Mathf.PI * corte / FS), y = 0f;
        for (int i = 0; i < b.Length; i++) { y += a * (b[i] - y); b[i] = y; }
    }

    static void PasaAltos(float[] b, float corte)
    {
        float a = 1f - Mathf.Exp(-2f * Mathf.PI * corte / FS), y = 0f;
        for (int i = 0; i < b.Length; i++) { y += a * (b[i] - y); b[i] -= y; }
    }

    // Eco con varias repeticiones (da sensación de pasillo / sala grande).
    static void Eco(float[] b, float retardo, float realim)
    {
        int d = Mathf.Max(1, (int)(retardo * FS));
        for (int i = d; i < b.Length; i++) b[i] += b[i - d] * realim;
    }

    // Convierte un buffer con "cola" en un bucle perfecto: la cola se suma al principio.
    static float[] Plegar(float[] b, float largo)
    {
        int n = Mathf.Min(b.Length, (int)(largo * FS));
        float[] r = new float[n];
        Array.Copy(b, r, n);
        for (int i = n; i < b.Length; i++) r[(i - n) % n] += b[i];
        return r;
    }

    static void Normalizar(float[] b, float pico)
    {
        float m = 0.0001f;
        for (int i = 0; i < b.Length; i++) m = Mathf.Max(m, Mathf.Abs(b[i]));
        float g = pico / m;
        for (int i = 0; i < b.Length; i++) b[i] *= g;
    }
}
