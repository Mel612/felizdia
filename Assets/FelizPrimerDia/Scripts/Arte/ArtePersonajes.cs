using System.Collections.Generic;
using UnityEngine;

public enum Accesorio { Ninguno, Bata, Toga, Bufanda, Gorra, Lentes, Mochila, Pepino, Maniqui }

// Poses especiales (se dibujan de lado).
public enum Pose { Normal, Salto, Agachada, Ataque, Golpe }

// Cómo se ve un personaje.
public class Apariencia
{
    public Color ropa, piel, pelo, pantalon, zapatos, acc;
    public bool peloLargo;
    public Accesorio accesorio;

    public string Clave()
    {
        return "pj2_" + ColorUtility.ToHtmlStringRGB(ropa) + ColorUtility.ToHtmlStringRGB(piel) + ColorUtility.ToHtmlStringRGB(pelo) +
               ColorUtility.ToHtmlStringRGB(pantalon) + ColorUtility.ToHtmlStringRGB(zapatos) + ColorUtility.ToHtmlStringRGB(acc) +
               (peloLargo ? "L" : "C") + (int)accesorio;
    }
}

// Sprites de un personaje: 3 direcciones x (quieto, paso 1, paso 2) + poses de lado.
public class SpritesPersonaje
{
    public Sprite[] abajo = new Sprite[3];
    public Sprite[] arriba = new Sprite[3];
    public Sprite[] lado = new Sprite[3];
    public Sprite salto, agachada, ataque, golpe;
    public Sprite Frente { get { return abajo[0]; } }
}

public static partial class Arte
{
    // Los personajes tienen el doble de detalle que el mundo: 32x48 píxeles = 1 x 1.5 metros.
    public const float PPUPersonaje = 32f;

    static readonly Dictionary<string, SpritesPersonaje> cachePj = new Dictionary<string, SpritesPersonaje>();

    // ---------- Personajes del juego ----------
    public static Apariencia Ap(string ropa, string piel, string pelo, string pantalon, bool peloLargo, Accesorio acc, string accHex)
    {
        Apariencia a = new Apariencia();
        a.ropa = Col.H(ropa);
        a.piel = Col.H(piel);
        a.pelo = Col.H(pelo);
        a.pantalon = Col.H(pantalon);
        a.zapatos = Col.H("#2b2230");
        a.peloLargo = peloLargo;
        a.accesorio = acc;
        a.acc = accHex != null ? Col.H(accHex) : Color.white;
        return a;
    }

    public static Apariencia Ap(string ropa, string piel, string pelo, string pantalon)
    {
        return Ap(ropa, piel, pelo, pantalon, false, Accesorio.Ninguno, null);
    }

    public static SpritesPersonaje Valeria() { return Personaje(Ap("#7b2cbf", "#c68a5a", "#1b1414", "#2b2d42", true, Accesorio.Mochila, "#ffb703")); }
    public static SpritesPersonaje Pepino() { return Personaje(Ap("#d62828", "#c68a5a", "#111111", "#d62828", false, Accesorio.Pepino, null)); }
    public static SpritesPersonaje Maniqui() { return Personaje(Ap("#d9d9d9", "#e6d5c3", "#e6d5c3", "#d9d9d9", false, Accesorio.Maniqui, null)); }

    public static SpritesPersonaje Personaje(Apariencia a)
    {
        string k = a.Clave();
        SpritesPersonaje sp;
        if (cachePj.TryGetValue(k, out sp)) return sp;
        sp = new SpritesPersonaje();
        for (int f = 0; f < 3; f++)
        {
            sp.abajo[f] = CrearPj(k + "_a" + f, DibujarPersonaje(a, 0, f, Pose.Normal));
            sp.arriba[f] = CrearPj(k + "_r" + f, DibujarPersonaje(a, 1, f, Pose.Normal));
            sp.lado[f] = CrearPj(k + "_l" + f, DibujarPersonaje(a, 2, f, Pose.Normal));
        }
        sp.salto = CrearPj(k + "_salto", DibujarPersonaje(a, 2, 1, Pose.Salto));
        sp.agachada = CrearPj(k + "_agachada", DibujarPersonaje(a, 2, 0, Pose.Agachada));
        sp.ataque = CrearPj(k + "_ataque", DibujarPersonaje(a, 2, 0, Pose.Ataque));
        sp.golpe = CrearPj(k + "_golpe", DibujarPersonaje(a, 2, 1, Pose.Golpe));
        cachePj[k] = sp;
        return sp;
    }

    static Sprite CrearPj(string k, Lienzo L) { return Crear(k, L, PivotCentro, PPUPersonaje, Vector4.zero); }

    public static Sprite Sombra()
    {
        Sprite s;
        if (EnCache("sombra", out s)) return s;
        Lienzo L = new Lienzo(14, 6);
        L.Elipse(7f, 3f, 7f, 3f, new Color(0, 0, 0, 0.22f));
        L.Elipse(7f, 3f, 5f, 2f, new Color(0, 0, 0, 0.18f));
        return Crear("sombra", L, PivotCentro);
    }

    // =================================================================
    //  DIBUJO DEL PERSONAJE (32 x 48)
    //  dir: 0 = de frente, 1 = de espaldas, 2 = de lado (mirando a la derecha)
    //  frame: 0 quieto, 1 y 2 pasos
    // =================================================================
    class Pal
    {
        public Color ropa, ropaO, ropaC, pant, pantO, piel, pielO, pielC, pelo, peloO, peloC, zap, zapC, ojo, iris, rubor, boca;
    }

    public static Lienzo DibujarPersonaje(Apariencia a, int dir, int frame, Pose pose)
    {
        Lienzo L = new Lienzo(32, 48);
        bool maniqui = a.accesorio == Accesorio.Maniqui;
        Pal p = new Pal();
        p.ropa = a.ropa; p.ropaO = Col.Oscuro(a.ropa, 0.7f); p.ropaC = Col.Claro(a.ropa, 0.22f);
        p.pant = a.pantalon; p.pantO = Col.Oscuro(a.pantalon, 0.68f);
        p.piel = a.piel; p.pielO = Col.Oscuro(a.piel, 0.8f); p.pielC = Col.Claro(a.piel, 0.18f);
        p.pelo = a.pelo; p.peloO = Col.Oscuro(a.pelo, 0.65f); p.peloC = Col.Claro(a.pelo, 0.28f);
        p.zap = a.zapatos; p.zapC = Col.Claro(a.zapatos, 0.25f);
        p.ojo = maniqui ? Col.Oscuro(a.piel, 0.75f) : Col.H("#1b1420");
        p.iris = Col.H("#5a3a22");
        p.rubor = Col.Mezcla(a.piel, Col.H("#ff5d73"), 0.35f);
        p.boca = Col.H("#7a2e2e");

        int baja = pose == Pose.Agachada ? 6 : 0;   // el cuerpo baja al agacharse

        if (dir == 2) DibujarLado(L, a, p, frame, pose, baja, maniqui);
        else DibujarFrenteEspalda(L, a, p, dir, frame, maniqui);

        if (a.accesorio == Accesorio.Pepino) PintarPepino(L, a, p, dir, baja);

        L.Borde(Col.H("#120d16"));
        return L;
    }

    // ---------- Frente y espalda ----------
    static void DibujarFrenteEspalda(Lienzo L, Apariencia a, Pal p, int dir, int frame, bool maniqui)
    {
        bool espalda = dir == 1;

        // Piernas y zapatos
        int lI = frame == 1 ? 2 : 0, lD = frame == 2 ? 2 : 0;
        PiernaFrente(L, p, 11, lI);
        PiernaFrente(L, p, 17, lD);

        // Torso
        int y0 = 10, y1 = 22;
        L.R(9, y0, 22, y1, p.ropa);
        L.R(9, y0, 9, y1, p.ropaO); L.R(22, y0, 22, y1, p.ropaO);
        L.R(10, y1, 21, y1, p.ropaC);
        L.R(9, y0, 22, y0 + 1, p.ropaO);
        L.R(10, y0 + 2, 10, y1 - 1, Col.Mezcla(p.ropa, p.ropaC, 0.5f));
        L.R(9, y0, 22, y0, p.pantO);                       // cinturón
        if (!espalda)
        {
            L.R(14, 20, 17, 22, p.ropaO);                  // cuello en V
            L.R(15, 20, 16, 22, p.piel);
            L.R(15, 13, 16, 17, p.ropaO);                  // pliegue
        }
        else L.R(15, 12, 16, 21, p.ropaO);

        // Brazos (se balancean al caminar)
        int bI = frame == 1 ? 2 : frame == 2 ? -1 : 0;
        int bD = frame == 2 ? 2 : frame == 1 ? -1 : 0;
        Brazo(L, p, 7, 12 + bI, 21);
        Brazo(L, p, 23, 12 + bD, 21);

        // Cuello y cabeza
        L.R(14, 22, 17, 24, p.pielO);
        Cabeza(L, p, 16f, 34.5f);

        if (!espalda)
        {
            if (!maniqui) Cara(L, p, true);
            else { L.R(11, 32, 12, 33, p.ojo); L.R(20, 32, 21, 33, p.ojo); }
            if (!maniqui) PeloFrente(L, a, p);
        }
        else
        {
            if (!maniqui) PeloEspalda(L, a, p);
        }

        // Accesorios
        Color acc = a.acc, accO = Col.Oscuro(acc, 0.7f), accC = Col.Claro(acc, 0.3f);
        switch (a.accesorio)
        {
            case Accesorio.Bata:
                {
                    Color b = Col.H("#f4f6f8"), bO = Col.H("#c3ccd6");
                    if (!espalda)
                    {
                        L.R(9, 3, 13, 22, b); L.R(18, 3, 22, 22, b);
                        L.R(9, 3, 9, 22, bO); L.R(22, 3, 22, 22, bO); L.R(13, 3, 13, 20, bO); L.R(18, 3, 18, 20, bO);
                        L.R(11, 12, 12, 13, Col.H("#4cc9f0"));    // bolígrafo en el bolsillo
                    }
                    else { L.R(9, 3, 22, 22, b); L.R(9, 3, 9, 22, bO); L.R(22, 3, 22, 22, bO); L.R(15, 3, 16, 14, bO); }
                    Brazo(L, p, 7, 12 + bI, 21, b, bO);
                    Brazo(L, p, 23, 12 + bD, 21, b, bO);
                    break;
                }
            case Accesorio.Toga:
                {
                    Color t = Col.H("#16131c"), tC = Col.H("#2e2738");
                    L.R(8, 1, 23, 22, t); L.R(7, 1, 24, 4, t);
                    L.R(10, 2, 10, 21, tC); L.R(21, 2, 21, 21, tC);
                    Brazo(L, p, 6, 11 + bI, 22, t, tC); Brazo(L, p, 24, 11 + bD, 22, t, tC);
                    if (!espalda) { L.R(13, 19, 18, 22, Color.white); L.R(15, 17, 16, 19, Color.white); }
                    break;
                }
            case Accesorio.Bufanda:
                L.R(9, 21, 22, 24, acc); L.R(9, 21, 22, 21, accO);
                for (int x = 9; x <= 22; x += 3) L.R(x, 22, x, 24, accC);
                if (!espalda) { L.R(18, 13, 21, 21, acc); L.R(18, 13, 21, 13, accO); L.R(19, 12, 19, 12, accO); L.R(21, 12, 21, 12, accO); }
                else L.R(11, 14, 14, 21, acc);
                break;
            case Accesorio.Gorra:
                for (int y = 39; y < 48; y++)
                    for (int x = 0; x < 32; x++)
                    {
                        float dx = (x + 0.5f - 16f) / 11.5f, dy = (y + 0.5f - 38f) / 9.5f;
                        if (dx * dx + dy * dy <= 1f) L.P(x, y, acc);
                    }
                L.R(10, 44, 14, 45, accC);
                if (!espalda) { L.R(4, 39, 27, 40, accO); L.R(14, 42, 17, 44, Col.H("#ffd166")); }
                break;
            case Accesorio.Lentes:
                if (!espalda)
                {
                    Color m = Col.H("#1b1b1b"), vidrio = Col.Alfa(Col.H("#bde0fe"), 0.35f);
                    Marco(L, 9, 29, 13, 35, m, vidrio); Marco(L, 18, 29, 22, 35, m, vidrio);
                    L.R(14, 33, 17, 33, m);
                }
                break;
            case Accesorio.Mochila:
                if (espalda) { L.R(10, 9, 21, 21, acc); L.R(10, 9, 21, 10, accO); L.R(10, 9, 10, 21, accO); L.R(12, 12, 19, 15, accO); L.R(13, 13, 18, 14, accC); L.R(11, 20, 20, 21, accC); }
                else { L.R(10, 12, 11, 22, accO); L.R(20, 12, 21, 22, accO); }
                break;
        }
    }

    static void PiernaFrente(Lienzo L, Pal p, int x0, int alza)
    {
        L.R(x0, 3 + alza, x0 + 3, 9, p.pant);
        L.R(x0 + 3, 3 + alza, x0 + 3, 9, p.pantO);
        L.R(x0 - 1, alza, x0 + 4, 2 + alza, p.zap);
        L.R(x0, 2 + alza, x0 + 2, 2 + alza, p.zapC);
    }

    static void Brazo(Lienzo L, Pal p, int x0, int yMano, int yTop) { Brazo(L, p, x0, yMano, yTop, p.ropa, p.ropaO); }

    static void Brazo(Lienzo L, Pal p, int x0, int yMano, int yTop, Color manga, Color mangaO)
    {
        L.R(x0, yMano + 2, x0 + 1, yTop, manga);
        L.R(x0 + 1, yMano + 2, x0 + 1, yTop, mangaO);
        L.R(x0, yMano, x0 + 1, yMano + 1, p.piel);
    }

    static void Cabeza(Lienzo L, Pal p, float cx, float cy)
    {
        L.Elipse(cx, cy, 10.5f, 10.5f, p.piel);
        for (int y = 0; y < 48; y++)
            for (int x = 0; x < 32; x++)
            {
                if (!Lienzo.Igual(L.G(x, y), p.piel)) continue;
                float dx = x + 0.5f - cx, dy = y + 0.5f - cy;
                if (dx > 6.5f || dy < -8f) L.P(x, y, p.pielO);
                else if (dx < -6f && dy > 2f) L.P(x, y, p.pielC);
            }
    }

    static void Ojo(Lienzo L, Pal p, int x, int y)
    {
        L.R(x, y, x + 2, y + 4, p.ojo);
        L.R(x + 1, y + 1, x + 2, y + 2, p.iris);
        L.P(x, y + 4, Color.white);
        L.P(x, y + 3, Color.white);
    }

    static void Cara(Lienzo L, Pal p, bool frente)
    {
        Ojo(L, p, 10, 30); Ojo(L, p, 19, 30);
        L.R(10, 36, 12, 36, p.peloO); L.R(19, 36, 21, 36, p.peloO);     // cejas
        L.P(16, 30, p.pielO);                                             // nariz
        L.R(15, 27, 16, 27, p.boca); L.P(14, 28, p.boca); L.P(17, 28, p.boca);
        L.R(8, 29, 9, 29, p.rubor); L.R(22, 29, 23, 29, p.rubor);
    }

    static void PeloFrente(Lienzo L, Apariencia a, Pal p)
    {
        if (a.peloLargo)
        {
            L.R(4, 13, 8, 38, p.pelo); L.R(23, 13, 27, 38, p.pelo);
            L.R(4, 13, 4, 36, p.peloO); L.R(27, 13, 27, 36, p.peloO);
            L.R(7, 13, 8, 30, p.peloO); L.R(23, 13, 24, 30, p.peloO);
            for (int x = 4; x <= 8; x += 2) L.P(x, 12, p.pelo);
            for (int x = 23; x <= 27; x += 2) L.P(x, 12, p.pelo);
        }
        for (int y = 36; y < 48; y++)
            for (int x = 0; x < 32; x++)
            {
                float dx = (x + 0.5f - 16f) / 11.5f, dy = (y + 0.5f - 35.5f) / 11.5f;
                if (dx * dx + dy * dy <= 1f) L.P(x, y, p.pelo);
            }
        // Flequillo con puntas
        for (int x = 6; x <= 25; x++)
        {
            int fondo = 37 - ((x % 4 == 1) ? 2 : (x % 4 == 2 ? 1 : 0));
            L.R(x, fondo, x, 40, p.pelo);
        }
        L.R(6, 26, 7, 39, p.pelo); L.R(24, 26, 25, 39, p.pelo);
        L.R(6, 26, 6, 39, p.peloO); L.R(25, 26, 25, 39, p.peloO);
        L.R(10, 42, 15, 43, p.peloC); L.R(9, 41, 9, 41, p.peloC); L.R(16, 43, 18, 44, p.peloC);
        L.R(8, 38, 23, 38, p.peloO);
    }

    static void PeloEspalda(Lienzo L, Apariencia a, Pal p)
    {
        for (int y = 24; y < 48; y++)
            for (int x = 0; x < 32; x++)
            {
                float dx = (x + 0.5f - 16f) / 11.5f, dy = (y + 0.5f - 35f) / 11.5f;
                if (dx * dx + dy * dy <= 1f && y >= 27) L.P(x, y, p.pelo);
            }
        if (a.peloLargo) { L.R(6, 12, 25, 30, p.pelo); L.R(6, 12, 7, 30, p.peloO); L.R(24, 12, 25, 30, p.peloO); for (int x = 7; x <= 24; x += 3) L.P(x, 11, p.pelo); }
        L.R(11, 40, 15, 42, p.peloC); L.R(17, 43, 20, 44, p.peloC);
        for (int x = 9; x <= 22; x += 4) L.R(x, 28, x, 34, p.peloO);
    }

    static void Marco(Lienzo L, int x0, int y0, int x1, int y1, Color m, Color vidrio)
    {
        L.R(x0, y0, x1, y1, vidrio);
        L.R(x0, y0, x1, y0, m); L.R(x0, y1, x1, y1, m); L.R(x0, y0, x0, y1, m); L.R(x1, y0, x1, y1, m);
    }

    // ---------- De lado (mirando a la derecha) ----------
    static void DibujarLado(Lienzo L, Apariencia a, Pal p, int frame, Pose pose, int baja, bool maniqui)
    {
        Color pantAtras = p.pantO, zapAtras = Col.Oscuro(p.zap, 0.7f);
        int yCuerpo = 10 - baja;

        // Piernas
        if (pose == Pose.Agachada)
        {
            L.R(9, 0, 14, 2, zapAtras); L.R(10, 3, 13, 6, pantAtras);              // rodilla atrás
            L.R(15, 0, 21, 2, p.zap); L.R(15, 3, 19, 6, p.pant); L.R(16, 7, 20, 9, p.pant);
        }
        else if (pose == Pose.Salto)
        {
            L.R(8, 6, 12, 8, zapAtras); L.R(10, 8, 13, 10, pantAtras);
            L.R(17, 4, 23, 6, p.zap); L.R(17, 7, 20, 9, p.pant); L.R(14, 8, 18, 10, p.pant);
        }
        else if (frame == 0)
        {
            L.R(13, 3, 17, 9, p.pant); L.R(13, 3, 13, 9, p.pantO);
            L.R(13, 0, 20, 2, p.zap); L.R(14, 2, 18, 2, p.zapC);
        }
        else
        {
            bool adelante = frame == 1;
            // pierna de atrás
            int xa = adelante ? 9 : 17;
            L.R(xa + 1, 3, xa + 4, 9, pantAtras); L.R(xa, 0, xa + 5, 2, zapAtras);
            // pierna de adelante
            int xf = adelante ? 17 : 10;
            L.R(xf, 3, xf + 3, 9, p.pant); L.R(xf, 3, xf, 9, p.pantO);
            L.R(xf, 1, xf + 6, 3, p.zap); L.R(xf + 1, 3, xf + 4, 3, p.zapC);
        }

        // Torso
        L.R(11, yCuerpo, 20, yCuerpo + 12, p.ropa);
        L.R(11, yCuerpo, 11, yCuerpo + 12, p.ropaO);
        L.R(12, yCuerpo + 12, 20, yCuerpo + 12, p.ropaC);
        L.R(20, yCuerpo + 2, 20, yCuerpo + 11, p.ropaC);
        L.R(11, yCuerpo, 20, yCuerpo, p.pantO);

        // Mochila (atrás)
        if (a.accesorio == Accesorio.Mochila)
        {
            Color acc = a.acc, accO = Col.Oscuro(acc, 0.7f);
            L.R(6, yCuerpo, 10, yCuerpo + 11, acc); L.R(6, yCuerpo, 6, yCuerpo + 11, accO); L.R(7, yCuerpo + 4, 9, yCuerpo + 6, accO);
            L.R(11, yCuerpo + 2, 12, yCuerpo + 11, accO);
        }

        // Cuello y cabeza
        int cy = 34 - baja;
        L.R(14, yCuerpo + 12, 17, yCuerpo + 14, p.pielO);
        Cabeza(L, p, 16f, cy + 0.5f);
        L.R(26, cy - 4, 27, cy - 3, p.piel);                 // nariz

        if (!maniqui)
        {
            Ojo(L, p, 21, cy - 4);
            L.R(21, cy + 2, 23, cy + 2, p.peloO);
            L.R(24, cy - 7, 25, cy - 7, p.boca);
            L.R(22, cy - 5, 24, cy - 5, p.rubor);
            // Pelo
            for (int y = cy - 8; y < 48; y++)
                for (int x = 0; x < 32; x++)
                {
                    float dx = (x + 0.5f - 16f) / 11.5f, dy = (y + 0.5f - (cy + 1.5f)) / 11.5f;
                    bool dentro = dx * dx + dy * dy <= 1f;
                    if (!dentro) continue;
                    if (y >= cy + 3 || x <= 15) L.P(x, y, p.pelo);
                }
            for (int x = 17; x <= 26; x++) { int fondo = cy + 2 - ((x % 3 == 0) ? 2 : 0); L.R(x, fondo, x, cy + 5, p.pelo); }
            L.R(15, cy - 4, 16, cy - 1, p.pielO);             // oreja
            L.P(16, cy - 2, p.piel);
            L.R(10, cy + 7, 15, cy + 8, p.peloC); L.R(17, cy + 9, 20, cy + 9, p.peloC);
            if (a.peloLargo) { L.R(5, 13 - baja, 11, cy + 2, p.pelo); L.R(5, 13 - baja, 5, cy, p.peloO); for (int y = 13 - baja; y < cy; y += 4) L.P(8, y, p.peloO); }
            else L.R(6, cy - 3, 9, cy + 3, p.pelo);
        }
        else { L.R(21, cy - 2, 22, cy - 1, p.ojo); }

        // Accesorios de lado
        Color ac = a.acc, acO = Col.Oscuro(ac, 0.7f);
        switch (a.accesorio)
        {
            case Accesorio.Bata:
                { Color b = Col.H("#f4f6f8"), bO = Col.H("#c3ccd6"); L.R(10, 3 + baja / 2, 21, yCuerpo + 12, b); L.R(10, 3, 10, yCuerpo + 12, bO); L.R(21, 3, 21, yCuerpo + 12, bO); break; }
            case Accesorio.Toga:
                { Color t = Col.H("#16131c"); L.R(9, 1, 22, yCuerpo + 12, t); L.R(20, yCuerpo + 9, 22, yCuerpo + 12, Color.white); break; }
            case Accesorio.Bufanda:
                L.R(11, yCuerpo + 11, 21, yCuerpo + 14, ac); L.R(7, yCuerpo + 4, 10, yCuerpo + 13, acO); break;
            case Accesorio.Gorra:
                L.R(7, cy + 6, 25, cy + 13, ac); L.R(18, cy + 5, 30, cy + 6, acO); break;
            case Accesorio.Lentes:
                { Color m = Col.H("#1b1b1b"); Marco(L, 20, cy - 5, 24, cy + 1, m, Col.Alfa(Col.H("#bde0fe"), 0.35f)); L.R(14, cy - 1, 19, cy - 1, m); break; }
        }

        // Brazo (delante del cuerpo)
        Color manga = p.ropa, mangaO = p.ropaO;
        if (a.accesorio == Accesorio.Bata) { manga = Col.H("#f4f6f8"); mangaO = Col.H("#c3ccd6"); }
        if (a.accesorio == Accesorio.Toga) { manga = Col.H("#16131c"); mangaO = Col.H("#2e2738"); }
        int hombro = yCuerpo + 11;
        if (pose == Pose.Ataque)
        {
            // Brazo hacia adelante, a la altura del hombro, con el cuchillo listo
            L.R(18, hombro - 2, 24, hombro, manga); L.R(18, hombro - 2, 24, hombro - 2, mangaO);
            L.R(25, hombro - 2, 26, hombro + 1, p.piel);
        }
        else if (pose == Pose.Golpe)
        {
            // Brazo estirado hacia adelante
            L.R(17, hombro - 3, 26, hombro - 1, manga); L.R(17, hombro - 3, 26, hombro - 3, mangaO);
            L.R(27, hombro - 3, 28, hombro - 1, p.piel);
        }
        else if (pose == Pose.Salto)
        {
            L.R(17, hombro - 1, 19, hombro + 6, manga); L.R(17, hombro + 7, 19, hombro + 8, p.piel);
        }
        else
        {
            int dx = frame == 1 ? 3 : frame == 2 ? -3 : 0;
            int mx = 15 + dx;
            L.R(mx, hombro - 9, mx + 2, hombro, manga);
            L.R(mx + 2, hombro - 9, mx + 2, hombro, mangaO);
            L.R(mx, hombro - 11, mx + 2, hombro - 10, p.piel);
        }
    }

    // =================================================================
    //  EL PEPINO: traje de rombos del carnaval paceño + máscara sonriente
    // =================================================================
    static void PintarPepino(Lienzo L, Apariencia a, Pal p, int dir, int baja)
    {
        Color[] rombos = { Col.H("#d62828"), Col.H("#f7b801"), Col.H("#2a9d8f"), Col.H("#3a86ff") };
        int yLimite = 25 - baja;
        for (int y = 0; y < yLimite; y++)
            for (int x = 0; x < L.w; x++)
            {
                Color c = L.G(x, y);
                if (c.a < 0.5f) continue;
                bool oscuro = Lienzo.Igual(c, p.ropaO) || Lienzo.Igual(c, p.pantO);
                bool tela = Lienzo.Igual(c, p.ropa) || Lienzo.Igual(c, p.ropaC) || Lienzo.Igual(c, p.pant) || oscuro ||
                            Lienzo.Igual(c, Col.Mezcla(p.ropa, p.ropaC, 0.5f));
                if (tela)
                {
                    int i = ((x + y) / 4 + (x - y + 64) / 4) % 4;
                    Color r = rombos[i];
                    if ((x + y) % 4 == 0 || (x - y + 64) % 4 == 0) r = Col.Oscuro(r, 0.8f);
                    L.P(x, y, oscuro ? Col.Oscuro(r, 0.72f) : r);
                }
                else if (Lienzo.Igual(c, p.piel) || Lienzo.Igual(c, p.pielO)) L.P(x, y, Col.H("#eeeeee"));   // guantes
            }
        // Cuello de volantes
        Color volante = Col.H("#f7f7f7");
        for (int x = 9; x <= 22; x += 2) { L.R(x, 21 - baja, x + 1, 23 - baja, volante); L.P(x, 20 - baja, volante); }

        Color mask = Col.H("#f4f1e8"), maskO = Col.H("#d6cfbd"), capucha = Col.H("#141018"), capuchaC = Col.H("#2b2333");
        Color rojo = Col.H("#c1121f"), ojo = Col.H("#050505"), rosa = Col.H("#ff8fa3");
        int cy = 34 - baja;

        if (dir == 1)
        {
            for (int y = 23 - baja; y < 48; y++) for (int x = 0; x < 32; x++)
                { float dx = (x + 0.5f - 16f) / 12f, dy = (y + 0.5f - (cy + 1f)) / 12f; if (dx * dx + dy * dy <= 1f) L.P(x, y, capucha); }
            L.R(11, cy + 6, 16, cy + 8, capuchaC);
            L.Circulo(16f, 46f, 2.2f, Col.H("#f7b801"));
            return;
        }

        // Capucha
        for (int y = 22 - baja; y < 48; y++) for (int x = 0; x < 32; x++)
            { float dx = (x + 0.5f - 16f) / 12.5f, dy = (y + 0.5f - (cy + 1f)) / 12.5f; if (dx * dx + dy * dy <= 1f) L.P(x, y, capucha); }
        L.Circulo(dir == 2 ? 11f : 16f, 46f, 2.3f, Col.H("#f7b801"));    // pompón

        if (dir == 0)
        {
            L.Elipse(16f, cy + 0.5f, 9.5f, 9.5f, mask);
            for (int y = 0; y < 48; y++) for (int x = 22; x < 27; x++) if (Lienzo.Igual(L.G(x, y), mask)) L.P(x, y, maskO);
            // Ojos huecos
            L.Elipse(11.5f, cy - 0.5f, 1.8f, 2.6f, ojo); L.Elipse(20.5f, cy - 0.5f, 1.8f, 2.6f, ojo);
            L.R(9, cy + 3, 13, cy + 3, ojo); L.R(18, cy + 3, 22, cy + 3, ojo);       // cejas pintadas
            // Sonrisa enorme
            L.R(11, cy - 7, 20, cy - 6, rojo);
            L.R(9, cy - 5, 10, cy - 4, rojo); L.R(21, cy - 5, 22, cy - 4, rojo);
            L.P(8, cy - 3, rojo); L.P(23, cy - 3, rojo);
            for (int x = 12; x <= 19; x += 2) L.P(x, cy - 6, Col.H("#fff5f5"));      // dientes
            L.R(8, cy - 2, 9, cy - 1, rosa); L.R(22, cy - 2, 23, cy - 1, rosa);
            L.P(16, cy - 3, maskO);
        }
        else
        {
            L.Elipse(20f, cy + 0.5f, 7.5f, 9.5f, mask);
            for (int y = 0; y < 48; y++) for (int x = 12; x < 16; x++) if (Lienzo.Igual(L.G(x, y), mask)) L.P(x, y, maskO);
            L.Elipse(22.5f, cy - 0.5f, 1.4f, 2.6f, ojo);
            L.R(21, cy + 3, 25, cy + 3, ojo);
            L.R(20, cy - 7, 26, cy - 6, rojo); L.R(27, cy - 5, 27, cy - 4, rojo);
            L.R(24, cy - 2, 25, cy - 1, rosa);
            L.R(27, cy - 1, 28, cy, mask);
            for (int x = 21; x <= 25; x += 2) L.P(x, cy - 6, Col.H("#fff5f5"));
        }
    }
}
