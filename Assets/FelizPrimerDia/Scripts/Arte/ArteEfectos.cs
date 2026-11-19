using UnityEngine;

// Armas, efectos y decorado de las escenas de lado (persecuciones).
public static partial class Arte
{
    // =================================================================
    //  ARMAS Y EFECTOS (para las muertes y las persecuciones)
    // =================================================================
    public static Sprite Cuchillo()
    {
        Sprite s;
        if (EnCache("cuchillo", out s)) return s;
        Lienzo L = new Lienzo(22, 8);
        Color hoja = Col.H("#d9dee5"), hojaC = Col.H("#ffffff"), hojaO = Col.H("#8d99ae"), mango = Col.H("#4a2c1a"), mangoC = Col.H("#7a4a2a");
        L.R(0, 2, 7, 5, mango); L.R(1, 4, 6, 4, mangoC); L.P(2, 3, Col.H("#c9a227")); L.P(5, 3, Col.H("#c9a227"));
        L.R(8, 1, 9, 6, Col.H("#6c757d"));
        L.R(10, 2, 19, 5, hoja); L.R(10, 5, 18, 5, hojaC); L.R(10, 2, 19, 2, hojaO);
        L.R(20, 3, 20, 4, hoja); L.P(21, 3, hoja);
        L.Borde(Col.H("#120d16"));
        return Crear("cuchillo", L, new Vector2(0.2f, 0.5f), PPUPersonaje, Vector4.zero);
    }

    public static Sprite Bisturi()
    {
        Sprite s;
        if (EnCache("bisturi", out s)) return s;
        Lienzo L = new Lienzo(18, 6);
        L.R(0, 2, 10, 3, Col.H("#adb5bd")); L.R(0, 3, 10, 3, Col.H("#dee2e6"));
        L.R(11, 1, 15, 4, Col.H("#e9ecef")); L.R(16, 2, 16, 3, Col.H("#e9ecef")); L.P(17, 2, Col.H("#e9ecef"));
        L.R(11, 1, 15, 1, Col.H("#8d99ae"));
        L.Borde(Col.H("#120d16"));
        return Crear("bisturi", L, new Vector2(0.2f, 0.5f), PPUPersonaje, Vector4.zero);
    }

    public static Sprite Mazo()
    {
        Sprite s;
        if (EnCache("mazo", out s)) return s;
        Lienzo L = new Lienzo(18, 22);
        Color m = Col.H("#7a4a2a"), mC = Col.H("#a0673d"), mO = Col.H("#4e2f1b");
        L.R(8, 0, 9, 13, m); L.R(9, 0, 9, 13, mO);
        L.R(1, 14, 16, 21, m); L.R(1, 20, 16, 21, mC); L.R(1, 14, 16, 15, mO);
        L.R(3, 14, 3, 21, Col.H("#c9a227")); L.R(14, 14, 14, 21, Col.H("#c9a227"));
        L.Borde(Col.H("#120d16"));
        return Crear("mazo", L, new Vector2(0.5f, 0.05f), PPUPersonaje, Vector4.zero);
    }

    public static Sprite Rayo(int variante)
    {
        string k = "rayo_" + variante;
        Sprite s;
        if (EnCache(k, out s)) return s;
        Lienzo L = new Lienzo(16, 32);
        System.Random r = new System.Random(variante * 13 + 5);
        int x = 8;
        for (int y = 31; y >= 0; y--)
        {
            if (y % 4 == 0) x = Mathf.Clamp(x + r.Next(-3, 4), 2, 13);
            L.R(x - 1, y, x + 1, y, Col.H("#ffd60a"));
            L.P(x, y, Color.white);
        }
        return Crear(k, L, PivotCentro, PPUPersonaje, Vector4.zero);
    }

    public static Sprite Estela()
    {
        Sprite s;
        if (EnCache("estela", out s)) return s;
        Lienzo L = new Lienzo(48, 16);
        for (int x = 0; x < 48; x++)
        {
            float t = x / 47f;
            float y = 8f + Mathf.Sin(t * 3.1416f) * 6f;
            float ancho = 3f * Mathf.Sin(t * 3.1416f);
            for (int yy = 0; yy < 16; yy++)
                if (Mathf.Abs(yy - y) <= ancho) L.P(x, yy, new Color(1, 1, 1, 0.9f * Mathf.Sin(t * 3.1416f)));
        }
        return Crear("estela", L, PivotCentro, PPUPersonaje, Vector4.zero);
    }

    public static Sprite Humo()
    {
        Sprite s;
        if (EnCache("humo", out s)) return s;
        Lienzo L = new Lienzo(16, 16);
        L.Circulo(8f, 8f, 7f, new Color(0.6f, 0.6f, 0.65f, 0.55f));
        L.Circulo(6f, 9f, 4f, new Color(0.8f, 0.8f, 0.85f, 0.5f));
        return Crear("humo", L, PivotCentro, PPUPersonaje, Vector4.zero);
    }

    // Fondo de la "franja" de las muertes (rayas diagonales, estilo Among Us).
    public static Sprite FondoMuerte(string colorHex)
    {
        string k = "fondo_muerte_" + colorHex;
        Sprite s;
        if (EnCache(k, out s)) return s;
        Lienzo L = new Lienzo(32, 32);
        Color c = Col.H(colorHex), cO = Col.Oscuro(c, 0.75f);
        for (int y = 0; y < 32; y++)
            for (int x = 0; x < 32; x++)
                L.P(x, y, ((x + y) / 8) % 2 == 0 ? c : cO);
        return Crear(k, L, PivotCentro, 100f, Vector4.zero);
    }

    public static Sprite BordeFranja()
    {
        Sprite s;
        if (EnCache("borde_franja", out s)) return s;
        Lienzo L = new Lienzo(8, 4);
        L.R(0, 0, 7, 3, Col.H("#120d16"));
        L.R(0, 1, 7, 2, Col.H("#3a0008"));
        return Crear("borde_franja", L, PivotCentro, 100f, Vector4.zero);
    }

    // =================================================================
    //  DECORADO DE LADO (persecuciones estilo plataformas)
    // =================================================================

    // Pared del pasillo vista de lado: zócalo, baranda y pintura. 32 x 96 px = 2 x 6 m.
    public static Sprite ParedLateral(Color pintura)
    {
        string k = "pared_lat_" + ColorUtility.ToHtmlStringRGB(pintura);
        Sprite s;
        if (EnCache(k, out s)) return s;
        Lienzo L = new Lienzo(32, 96);
        Color zocalo = Col.Oscuro(pintura, 0.55f), baranda = Col.H("#6b4a2f");
        for (int y = 0; y < 96; y++)
            for (int x = 0; x < 32; x++)
            {
                Color c = pintura;
                if (y < 4) c = Col.Oscuro(pintura, 0.35f);
                else if (y < 22) c = (x % 16 == 0) ? Col.Oscuro(zocalo, 0.85f) : zocalo;
                else if (y < 25) c = baranda;
                else if (y == 25) c = Col.Oscuro(baranda, 0.7f);
                else if (y > 88) c = Col.H("#2a2733");
                else if (y > 85) c = Col.Oscuro(pintura, 0.8f);
                L.P(x, y, c);
            }
        L.Ruido(0.03f, 61);
        return Crear(k, L, new Vector2(0.5f, 0f));
    }

    public static Sprite PisoLateral()
    {
        Sprite s;
        if (EnCache("piso_lat", out s)) return s;
        Lienzo L = new Lienzo(32, 32);
        Color a = Col.H("#b9b4aa"), b = Col.H("#a39e93");
        for (int y = 0; y < 32; y++)
            for (int x = 0; x < 32; x++)
            {
                Color c = ((x / 16) + (y / 16)) % 2 == 0 ? a : b;
                if (y >= 30) c = Col.H("#e0dbd0");
                else if (y >= 28) c = Col.Oscuro(c, 0.85f);
                L.P(x, y, c);
            }
        L.Ruido(0.04f, 62);
        return Crear("piso_lat", L, new Vector2(0.5f, 1f));
    }

    // Ventana con vista a La Paz: cielo, el Illimani nevado y casas en las laderas.
    public static Sprite VentanaIllimani(bool noche)
    {
        string k = "ventana_" + noche;
        Sprite s;
        if (EnCache(k, out s)) return s;
        Lienzo L = new Lienzo(48, 40);
        Color cieloA = noche ? Col.H("#0b1026") : Col.H("#5aa9e6"), cieloB = noche ? Col.H("#3a1c4a") : Col.H("#bde0fe");
        for (int y = 0; y < 40; y++)
            for (int x = 0; x < 48; x++)
                L.P(x, y, Col.Mezcla(cieloB, cieloA, y / 39f));
        if (noche) { L.P(8, 33, Color.white); L.P(30, 36, Color.white); L.P(40, 30, Color.white); L.Circulo(38f, 33f, 2.5f, Col.H("#f1f1d0")); }
        // Illimani: tres picos nevados
        Color roca = noche ? Col.H("#4a4e69") : Col.H("#8d99ae"), nieve = noche ? Col.H("#c9cfe0") : Col.H("#ffffff");
        int[] picos = { 14, 24, 33 };
        int[] alturas = { 27, 31, 26 };
        for (int x = 0; x < 48; x++)
        {
            float h = 12f;
            for (int i = 0; i < 3; i++) h = Mathf.Max(h, alturas[i] - Mathf.Abs(x - picos[i]) * 1.05f);
            for (int y = 10; y <= (int)h; y++) L.P(x, y, y > h - 5 ? nieve : roca);
        }
        // Laderas con casitas de ladrillo
        Color ladera = noche ? Col.H("#3d2b1f") : Col.H("#a0673d");
        for (int x = 0; x < 48; x++)
        {
            int h = 11 + (int)(3 * Mathf.Sin(x * 0.25f));
            L.R(x, 0, x, h, ladera);
        }
        System.Random r = new System.Random(noche ? 9 : 7);
        string[] casas = { "#c8553d", "#e9c46a", "#f4a261", "#d9d9d9", "#8f3b2a" };
        for (int i = 0; i < 26; i++)
        {
            int x = r.Next(46), y = r.Next(12);
            L.R(x, y, x + 1, y + 1, noche ? (r.Next(3) == 0 ? Col.H("#ffd166") : Col.H("#2b1d14")) : Col.H(casas[r.Next(casas.Length)]));
        }
        // Marco de aluminio
        Color marco = Col.H("#9aa5b1"), marcoO = Col.H("#5c6773");
        L.R(0, 0, 47, 1, marcoO); L.R(0, 38, 47, 39, marco); L.R(0, 0, 1, 39, marco); L.R(46, 0, 47, 39, marcoO);
        L.R(23, 0, 24, 39, marco);
        L.R(3, 30, 8, 35, new Color(1, 1, 1, 0.25f)); L.R(27, 30, 30, 33, new Color(1, 1, 1, 0.2f));
        return Crear(k, L, PivotCentro);
    }

    public static Sprite LamparaTecho()
    {
        Sprite s;
        if (EnCache("lampara", out s)) return s;
        Lienzo L = new Lienzo(28, 6);
        L.R(0, 2, 27, 5, Col.H("#6c757d"));
        L.R(2, 0, 25, 2, Col.H("#fffbe6"));
        L.Borde(Col.H("#120d16"));
        return Crear("lampara", L, new Vector2(0.5f, 1f));
    }

    public static Sprite ConoLuz()
    {
        Sprite s;
        if (EnCache("cono_luz", out s)) return s;
        Lienzo L = new Lienzo(48, 64);
        for (int y = 0; y < 64; y++)
        {
            float t = y / 63f;                       // 1 arriba (en la lámpara)
            float medio = 12f + (1f - t) * 12f;
            for (int x = 0; x < 48; x++)
            {
                float d = Mathf.Abs(x + 0.5f - 24f);
                if (d > medio) continue;
                float a = 0.16f * (0.35f + 0.65f * t) * (1f - d / medio * 0.6f);
                L.P(x, y, new Color(1f, 0.97f, 0.85f, a));
            }
        }
        Sprite sp = Crear("cono_luz", L, new Vector2(0.5f, 1f));
        sp.texture.filterMode = FilterMode.Bilinear;
        return sp;
    }

    public static Sprite Puerta(string colorHex)
    {
        string k = "puerta_" + colorHex;
        Sprite s;
        if (EnCache(k, out s)) return s;
        Lienzo L = new Lienzo(24, 40);
        Color c = Col.H(colorHex), cO = Col.Oscuro(c, 0.7f), cC = Col.Claro(c, 0.2f);
        L.R(0, 0, 23, 39, Col.H("#3b2a1f"));
        L.R(2, 0, 21, 37, c);
        L.R(2, 0, 3, 37, cC); L.R(20, 0, 21, 37, cO);
        L.R(6, 22, 17, 33, Col.H("#9ad1e6")); L.R(6, 22, 17, 23, Col.H("#6aa5bf"));
        L.R(5, 5, 18, 18, cO); L.R(6, 6, 17, 17, c);
        L.R(17, 18, 19, 19, Col.H("#d4a017"));
        return Crear(k, L, new Vector2(0.5f, 0f));
    }

    public static Sprite LetreroSalida()
    {
        Sprite s;
        if (EnCache("salida", out s)) return s;
        Lienzo L = new Lienzo(24, 10);
        L.R(0, 0, 23, 9, Col.H("#1b4332"));
        L.R(1, 1, 22, 8, Col.H("#2dc653"));
        L.R(3, 4, 6, 5, Color.white); L.R(5, 3, 5, 6, Color.white); L.P(6, 4, Color.white);   // flecha
        L.R(9, 3, 20, 6, Col.H("#ffffff"));
        L.R(10, 4, 19, 5, Col.H("#2dc653"));
        return Crear("salida", L, PivotCentro);
    }

    // Obstáculos (pivote en la base)
    public static Sprite Caja()
    {
        Sprite s;
        if (EnCache("caja", out s)) return s;
        Lienzo L = new Lienzo(16, 16);
        Color c = Col.H("#c8956a"), cO = Col.H("#9c6b45"), cC = Col.H("#e0b48a");
        L.R(0, 0, 15, 15, c); L.R(0, 14, 15, 15, cC); L.R(0, 0, 15, 1, cO);
        L.R(7, 0, 8, 15, Col.H("#d9c38c"));
        L.R(2, 5, 5, 7, cO);
        return Mueble("caja", L, true);
    }

    public static Sprite Carrito()
    {
        Sprite s;
        if (EnCache("carrito", out s)) return s;
        Lienzo L = new Lienzo(28, 26);
        Color am = Col.H("#ffd60a"), amO = Col.H("#c9a227");
        L.Circulo(5f, 2f, 2f, Col.H("#222")); L.Circulo(22f, 2f, 2f, Col.H("#222"));
        L.R(1, 3, 26, 14, am); L.R(1, 3, 26, 4, amO); L.R(1, 14, 26, 14, Col.Claro(am, 0.3f));
        L.R(4, 6, 12, 12, Col.H("#3a86ff"));                       // balde
        L.R(18, 14, 19, 25, Col.H("#8d6748"));                     // trapeador
        L.R(15, 22, 22, 25, Col.H("#e9ecef"));
        L.R(8, 15, 13, 17, Col.H("#d62828"));                      // letrero "piso mojado"
        return Mueble("carrito", L, true);
    }

    public static Sprite SillaLado()
    {
        Sprite s;
        if (EnCache("silla_lado", out s)) return s;
        Lienzo L = new Lienzo(14, 18);
        Color c = Col.H("#3a86ff"), cO = Col.H("#1d4ed8"), m = Col.H("#333");
        L.R(1, 0, 1, 7, m); L.R(11, 0, 11, 7, m);
        L.R(0, 7, 12, 9, c); L.R(0, 7, 12, 7, cO);
        L.R(10, 9, 12, 17, c); L.R(10, 9, 10, 17, cO);
        return Mueble("silla_lado", L, true);
    }

    public static Sprite MesaLado()
    {
        Sprite s;
        if (EnCache("mesa_lado", out s)) return s;
        Lienzo L = new Lienzo(32, 18);
        Color m = Col.H("#9c6b45"), mO = Col.H("#6e4a30");
        L.R(2, 0, 3, 13, mO); L.R(28, 0, 29, 13, mO);
        L.R(0, 13, 31, 17, m); L.R(0, 17, 31, 17, Col.Claro(m, 0.25f)); L.R(0, 13, 31, 13, mO);
        L.R(8, 18 - 1, 14, 17, Col.H("#f1f1f1"));
        return Mueble("mesa_lado", L, true);
    }

    // Cartel colgante del techo (hay que agacharse para pasar por debajo).
    public static Sprite Banner(string colorHex)
    {
        string k = "banner_" + colorHex;
        Sprite s;
        if (EnCache(k, out s)) return s;
        Lienzo L = new Lienzo(40, 40);
        Color c = Col.H(colorHex), cO = Col.Oscuro(c, 0.7f);
        L.R(4, 14, 4, 39, Col.H("#555")); L.R(35, 14, 35, 39, Col.H("#555"));     // cuerdas
        L.R(0, 0, 39, 15, c);
        L.R(0, 0, 39, 1, cO);
        for (int x = 0; x < 40; x += 4) { L.Borrar(x, 0); L.Borrar(x + 1, 0); L.Borrar(x, 1); }
        for (int x = 4; x < 36; x += 3) L.R(x, 6, x + 1, 9, Col.H("#ffffff"));
        return Mueble(k, L, true);
    }

    public static Sprite DuctoTecho()
    {
        Sprite s;
        if (EnCache("ducto", out s)) return s;
        Lienzo L = new Lienzo(32, 40);
        Color m = Col.H("#8d99ae"), mO = Col.H("#5c6773"), mC = Col.H("#c9d0da");
        L.R(12, 12, 19, 39, mO);
        L.R(0, 0, 31, 13, m); L.R(0, 12, 31, 13, mC); L.R(0, 0, 31, 1, mO);
        for (int x = 4; x < 32; x += 8) L.R(x, 2, x, 11, mO);
        L.R(6, 4, 9, 7, Col.H("#d62828"));
        return Mueble("ducto", L, true);
    }

    public static Sprite ButacaLado()
    {
        Sprite s;
        if (EnCache("butaca_lado", out s)) return s;
        Lienzo L = new Lienzo(18, 20);
        Color r = Col.H("#9d0208"), rO = Col.H("#6a040f"), rC = Col.H("#c1121f");
        L.R(2, 0, 3, 6, Col.H("#333")); L.R(13, 0, 14, 6, Col.H("#333"));
        L.R(0, 6, 15, 10, r); L.R(0, 10, 15, 10, rC); L.R(0, 6, 15, 6, rO);
        L.R(13, 6, 17, 19, r); L.R(13, 6, 13, 19, rO); L.R(17, 6, 17, 19, rC);
        return Mueble("butaca_lado", L, true);
    }

    // Silueta del público en la fiesta.
    public static Sprite Silueta(int variante)
    {
        string k = "silueta_" + variante;
        Sprite s;
        if (EnCache(k, out s)) return s;
        Lienzo L = new Lienzo(16, 26);
        Color c = new Color(0.06f, 0.03f, 0.08f, 1f);
        int brazo = variante % 3;
        L.R(4, 0, 11, 15, c);
        L.Circulo(7.5f, 19f, 4.5f, c);
        if (brazo == 0) { L.R(1, 14, 3, 25, c); L.R(12, 14, 14, 25, c); }
        else if (brazo == 1) L.R(12, 14, 14, 24, c);
        else { L.R(1, 8, 3, 14, c); L.R(12, 8, 14, 14, c); }
        if (variante % 4 == 3) L.R(4, 23, 11, 25, c);                 // gorro
        return Mueble(k, L, false);
    }

    public static Sprite FocoEscenario(string colorHex)
    {
        string k = "foco_" + colorHex;
        Sprite s;
        if (EnCache(k, out s)) return s;
        Lienzo L = new Lienzo(40, 90);
        Color c = Col.H(colorHex);
        for (int y = 0; y < 90; y++)
        {
            float t = y / 89f;
            float medio = 3f + (1f - t) * 17f;
            for (int x = 0; x < 40; x++)
            {
                float d = Mathf.Abs(x + 0.5f - 20f);
                if (d <= medio) L.P(x, y, new Color(c.r, c.g, c.b, 0.13f * (0.4f + 0.6f * t)));
            }
        }
        Sprite sp = Crear(k, L, new Vector2(0.5f, 1f));
        sp.texture.filterMode = FilterMode.Bilinear;
        return sp;
    }
}
