using UnityEngine;

public enum TipoSuelo { Ceramica, Piedra, Pasto, Terracota, Alfombra, Madera, Clinico, Tecnico, Estudio, Azulejo, Calle }

public static partial class Arte
{
    // =================================================================
    //  SUELOS (texturas que se repiten, 32x32 px = 2x2 m)
    // =================================================================
    public static Sprite Suelo(TipoSuelo tipo, Color c)
    {
        string k = "suelo_" + tipo + ColorUtility.ToHtmlStringRGB(c);
        Sprite s;
        if (EnCache(k, out s)) return s;
        return Crear(k, DibujarSuelo(tipo, c), PivotCentro);
    }

    public static Lienzo DibujarSuelo(TipoSuelo tipo, Color c)
    {
        Lienzo L = new Lienzo(32, 32);
        System.Random r = new System.Random(Semilla(tipo.ToString()));
        Color junta;
        switch (tipo)
        {
            case TipoSuelo.Ceramica:
                junta = Col.Oscuro(c, 0.78f);
                for (int y = 0; y < 32; y++)
                    for (int x = 0; x < 32; x++)
                    {
                        Color p = c;
                        if (x % 16 == 0 || y % 16 == 0) p = junta;
                        else if (x % 16 == 1 || y % 16 == 15) p = Col.Claro(c, 0.08f);
                        L.P(x, y, p);
                    }
                L.Ruido(0.05f, 1);
                break;

            case TipoSuelo.Piedra:
                junta = Col.Oscuro(c, 0.8f);
                for (int y = 0; y < 32; y++)
                    for (int x = 0; x < 32; x++)
                    {
                        int fila = y / 16;
                        int xx = (x + fila * 8) % 32;
                        Color p = (xx % 16 == 0 || y % 16 == 0) ? junta : c;
                        L.P(x, y, p);
                    }
                L.Ruido(0.07f, 2);
                for (int i = 0; i < 18; i++) L.P(r.Next(32), r.Next(32), Col.Oscuro(c, 0.88f));
                break;

            case TipoSuelo.Pasto:
                L.R(0, 0, 31, 31, c);
                L.Ruido(0.16f, 3);
                for (int i = 0; i < 60; i++)
                {
                    int x = r.Next(32), y = r.Next(31);
                    Color b = r.Next(2) == 0 ? Col.Oscuro(c, 0.78f) : Col.Claro(c, 0.15f);
                    L.P(x, y, b); L.P(x, y + 1, b);
                }
                for (int i = 0; i < 4; i++) L.P(r.Next(32), r.Next(32), i % 2 == 0 ? Col.H("#ffe066") : Col.H("#ffffff"));
                break;

            case TipoSuelo.Terracota:
                junta = Col.Claro(c, 0.25f);
                for (int y = 0; y < 32; y++)
                    for (int x = 0; x < 32; x++)
                    {
                        bool alterna = ((x / 16) + (y / 16)) % 2 == 0;
                        Color p = alterna ? c : Col.Oscuro(c, 0.9f);
                        if (x % 16 == 0 || y % 16 == 0) p = junta;
                        L.P(x, y, p);
                    }
                L.Ruido(0.06f, 4);
                break;

            case TipoSuelo.Alfombra:
                L.R(0, 0, 31, 31, c);
                L.Ruido(0.07f, 5);
                for (int y = 4; y < 32; y += 8)
                    for (int x = 4; x < 32; x += 8)
                    {
                        Color d = Col.Claro(c, 0.12f);
                        L.P(x, y, d); L.P(x - 1, y, d); L.P(x + 1, y, d); L.P(x, y - 1, d); L.P(x, y + 1, d);
                    }
                break;

            case TipoSuelo.Madera:
                for (int y = 0; y < 32; y++)
                {
                    int tabla = y / 4;
                    float var = 0.88f + (tabla * 37 % 10) / 70f;
                    for (int x = 0; x < 32; x++)
                    {
                        Color p = Col.Oscuro(c, var);
                        if (y % 4 == 0) p = Col.Oscuro(c, 0.7f);
                        if ((x + tabla * 11) % 32 == 0) p = Col.Oscuro(c, 0.65f);
                        L.P(x, y, p);
                    }
                }
                for (int i = 0; i < 40; i++)
                {
                    int x = r.Next(31), y = r.Next(32);
                    if (y % 4 == 0) continue;
                    L.P(x, y, Col.Oscuro(L.G(x, y), 0.9f)); L.P(x + 1, y, Col.Oscuro(L.G(x + 1, y), 0.9f));
                }
                break;

            case TipoSuelo.Clinico:
                junta = Col.Oscuro(c, 0.86f);
                for (int y = 0; y < 32; y++)
                    for (int x = 0; x < 32; x++)
                        L.P(x, y, (x % 8 == 0 || y % 8 == 0) ? junta : c);
                L.Ruido(0.03f, 6);
                break;

            case TipoSuelo.Tecnico:
                for (int y = 0; y < 32; y++)
                    for (int x = 0; x < 32; x++)
                    {
                        Color p = c;
                        int lx = x % 16, ly = y % 16;
                        if (lx == 0 || ly == 0) p = Col.Oscuro(c, 0.65f);
                        else if (lx == 15 || ly == 15) p = Col.Oscuro(c, 0.85f);
                        else if (lx % 4 == 2 && ly % 4 == 2 && lx > 1 && ly > 1 && lx < 14 && ly < 14) p = Col.Oscuro(c, 0.75f);
                        L.P(x, y, p);
                    }
                break;

            case TipoSuelo.Estudio:
                L.R(0, 0, 31, 31, c);
                L.Ruido(0.1f, 7);
                break;

            case TipoSuelo.Azulejo:
                junta = Col.Claro(c, 0.45f);
                for (int y = 0; y < 32; y++)
                    for (int x = 0; x < 32; x++)
                        L.P(x, y, (x % 8 == 0 || y % 8 == 0) ? junta : c);
                L.Ruido(0.05f, 8);
                break;

            case TipoSuelo.Calle:
                L.R(0, 0, 31, 31, c);
                L.Ruido(0.12f, 9);
                for (int i = 0; i < 30; i++) L.P(r.Next(32), r.Next(32), Col.Oscuro(c, 0.8f));
                break;
        }
        return L;
    }

    // =================================================================
    //  PAREDES (vista 3/4: se ve el frente de las paredes horizontales)
    // =================================================================
    public static Sprite ParedFrente(Color pintura)
    {
        string k = "pared_" + ColorUtility.ToHtmlStringRGB(pintura);
        Sprite s;
        if (EnCache(k, out s)) return s;
        Lienzo L = new Lienzo(16, 16);
        for (int y = 0; y < 16; y++)
            for (int x = 0; x < 16; x++)
            {
                Color p = pintura;
                if (y <= 2) p = Col.Oscuro(pintura, 0.5f);          // zócalo
                else if (y == 3) p = Col.Oscuro(pintura, 0.68f);
                else if (y == 15) p = Col.Oscuro(pintura, 0.75f);   // sombra bajo el techo
                else if (y == 14) p = Col.Claro(pintura, 0.08f);
                L.P(x, y, p);
            }
        L.Ruido(0.04f, 11);
        return Crear(k, L, PivotCentro);
    }

    // Sombra suave que proyecta una pared sobre el piso (gradiente vertical).
    public static Sprite SombraPared()
    {
        Sprite s;
        if (EnCache("sombra_pared", out s)) return s;
        Lienzo L = new Lienzo(4, 16);
        for (int y = 0; y < 16; y++)
        {
            float t = y / 15f;                              // 1 arriba (pegado a la pared)
            L.R(0, y, 3, y, new Color(0.05f, 0.03f, 0.08f, 0.55f * t * t));
        }
        Sprite sp = Crear("sombra_pared", L, PivotCentro, 16f, Vector4.zero);
        sp.texture.filterMode = FilterMode.Bilinear;
        return sp;
    }

    // Charco de luz de una lámpara del techo (radial, suave).
    public static Sprite Luz()
    {
        Sprite s;
        if (EnCache("luz", out s)) return s;
        int n = 64;
        Lienzo L = new Lienzo(n, n);
        for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float dx = (x + 0.5f) / n - 0.5f, dy = (y + 0.5f) / n - 0.5f;
                float d = Mathf.Sqrt(dx * dx + dy * dy) * 2f;
                float a = Mathf.Clamp01(1f - d);
                L.P(x, y, new Color(1f, 0.95f, 0.78f, a * a * 0.34f));
            }
        Sprite sp = Crear("luz", L, PivotCentro, 16f, Vector4.zero);
        sp.texture.filterMode = FilterMode.Bilinear;
        return sp;
    }

    public static Sprite ParedTope()
    {
        Sprite s;
        if (EnCache("pared_tope", out s)) return s;
        Lienzo L = new Lienzo(8, 8);
        L.R(0, 0, 7, 7, Col.H("#2a2733"));
        L.R(0, 7, 7, 7, Col.H("#3b3746"));
        L.Ruido(0.06f, 12);
        return Crear("pared_tope", L, PivotCentro);
    }

    // =================================================================
    //  MUEBLES (pivote en la base, para ordenar por profundidad)
    // =================================================================
    static Sprite Mueble(string k, Lienzo L, bool contorno)
    {
        if (contorno) L.Borde(Col.H("#120d16"));
        return Crear(k, L, PivotPies);
    }

    public static Sprite Escritorio(bool conPC, string pantallaHex)
    {
        string k = "escritorio_" + conPC + pantallaHex;
        Sprite s;
        if (EnCache(k, out s)) return s;
        Lienzo L = new Lienzo(32, 24);
        Color mad = Col.H("#9c6b45"), madO = Col.H("#6e4a30"), madC = Col.H("#b98a5e");
        L.R(1, 1, 2, 9, madO); L.R(29, 1, 30, 9, madO);
        L.R(1, 6, 30, 12, madO);
        L.R(1, 12, 30, 21, mad); L.R(1, 21, 30, 21, madC); L.R(1, 12, 30, 12, Col.Oscuro(mad, 0.85f));
        if (conPC)
        {
            Color bisel = Col.H("#1e1e24"), pant = Col.H(pantallaHex);
            L.R(15, 14, 16, 15, bisel);
            L.R(9, 15, 22, 23, bisel);
            L.R(10, 16, 21, 22, pant);
            L.R(11, 21, 15, 21, Col.Claro(pant, 0.4f));
            L.R(11, 18, 18, 18, Col.Claro(pant, 0.25f));
            L.R(8, 13, 23, 13, Col.H("#4a4a55"));
        }
        else
        {
            L.R(5, 15, 11, 19, Col.H("#f1f1f1")); L.R(6, 18, 10, 18, Col.H("#9aa5b1"));
            L.R(20, 15, 21, 18, Col.H("#3a86ff"));
        }
        return Mueble(k, L, true);
    }

    public static Sprite Silla(string colorHex)
    {
        string k = "silla_" + colorHex;
        Sprite s;
        if (EnCache(k, out s)) return s;
        Lienzo L = new Lienzo(12, 14);
        Color c = Col.H(colorHex), cO = Col.Oscuro(c, 0.7f);
        L.R(2, 0, 2, 4, Col.H("#333")); L.R(9, 0, 9, 4, Col.H("#333"));
        L.R(1, 4, 10, 7, c); L.R(1, 4, 10, 4, cO);
        L.R(2, 8, 9, 13, cO); L.R(3, 9, 8, 12, c);
        return Mueble(k, L, true);
    }

    public static Sprite Planta()
    {
        Sprite s;
        if (EnCache("planta", out s)) return s;
        Lienzo L = new Lienzo(14, 22);
        Color hoja = Col.H("#2d8a4e"), hojaO = Col.H("#1f5f36"), hojaC = Col.H("#52b788");
        L.Circulo(7f, 13f, 5f, hojaO);
        L.Circulo(5f, 15f, 4f, hoja);
        L.Circulo(9f, 16f, 4f, hoja);
        L.Circulo(7f, 18.5f, 3.5f, hoja);
        L.P(5, 17, hojaC); L.P(9, 18, hojaC); L.P(7, 20, hojaC); L.P(4, 14, hojaC);
        Color maceta = Col.H("#c8553d"), macetaO = Col.H("#8f3b2a");
        L.R(3, 0, 10, 7, maceta); L.R(2, 6, 11, 8, macetaO); L.R(3, 0, 3, 5, macetaO);
        return Mueble("planta", L, true);
    }

    public static Sprite Arbol()
    {
        Sprite s;
        if (EnCache("arbol", out s)) return s;
        Lienzo L = new Lienzo(36, 46);
        L.R(15, 0, 20, 14, Col.H("#6b4226")); L.R(15, 0, 16, 14, Col.H("#4e2f1b"));
        Color h = Col.H("#2f7d4a"), hO = Col.H("#1d5432"), hC = Col.H("#5cb85c");
        L.Circulo(18f, 26f, 13f, hO);
        L.Circulo(12f, 28f, 9f, h);
        L.Circulo(24f, 29f, 9f, h);
        L.Circulo(18f, 35f, 9f, h);
        L.Circulo(14f, 33f, 4f, hC); L.Circulo(23f, 36f, 3f, hC); L.Circulo(20f, 27f, 3f, hC);
        L.Ruido(0.08f, 21);
        return Mueble("arbol", L, true);
    }

    public static Sprite MesaRedonda()
    {
        Sprite s;
        if (EnCache("mesa_redonda", out s)) return s;
        Lienzo L = new Lienzo(24, 20);
        L.R(10, 0, 13, 8, Col.H("#4a4a4a"));
        L.Elipse(12f, 12f, 11f, 7f, Col.H("#6e4a30"));
        L.Elipse(12f, 13f, 11f, 6f, Col.H("#a47148"));
        L.Elipse(10f, 15f, 4f, 2f, Col.H("#c08a5e"));
        L.Circulo(15f, 13f, 1.6f, Col.H("#ffffff"));      // vaso
        return Mueble("mesa_redonda", L, true);
    }

    public static Sprite Camilla()
    {
        Sprite s;
        if (EnCache("camilla", out s)) return s;
        Lienzo L = new Lienzo(24, 40);
        Color metal = Col.H("#9aa5b1"), metalO = Col.H("#5c6773");
        L.R(2, 0, 3, 6, metalO); L.R(20, 0, 21, 6, metalO);
        L.R(1, 4, 22, 38, metal);
        L.R(3, 6, 20, 36, Col.H("#e9f5f9"));
        L.R(3, 6, 20, 24, Col.H("#a8dadc"));            // sábana
        L.R(3, 24, 20, 24, Col.H("#7fb8bb"));
        L.R(6, 30, 17, 35, Col.H("#ffffff"));           // almohada
        L.R(1, 4, 22, 5, metalO);
        return Mueble("camilla", L, true);
    }

    public static Sprite Estante()
    {
        Sprite s;
        if (EnCache("estante", out s)) return s;
        Lienzo L = new Lienzo(32, 30);
        Color mad = Col.H("#7a4f2e"), madO = Col.H("#52331d");
        L.R(0, 0, 31, 29, madO);
        L.R(1, 1, 30, 28, mad);
        string[] libros = { "#e63946", "#457b9d", "#f4a261", "#2a9d8f", "#8338ec", "#ffb703", "#06d6a0", "#ef476f" };
        System.Random r = new System.Random(5);
        for (int fila = 0; fila < 3; fila++)
        {
            int y0 = 2 + fila * 9;
            L.R(1, y0 - 1, 30, y0 - 1, madO);
            int x = 2;
            while (x < 29)
            {
                int ancho = 1 + r.Next(2), alto = 5 + r.Next(3);
                Color c = Col.H(libros[r.Next(libros.Length)]);
                L.R(x, y0, x + ancho, y0 + alto, c);
                L.R(x, y0 + alto - 1, x + ancho, y0 + alto - 1, Col.Claro(c, 0.3f));
                x += ancho + 1;
            }
        }
        return Mueble("estante", L, true);
    }

    public static Sprite Casillero()
    {
        Sprite s;
        if (EnCache("casillero", out s)) return s;
        Lienzo L = new Lienzo(16, 28);
        Color m = Col.H("#5c7a99"), mO = Col.H("#3d5570"), mC = Col.H("#88a3bf");
        L.R(0, 0, 15, 27, mO);
        L.R(1, 1, 14, 26, m);
        L.R(1, 26, 14, 26, mC);
        for (int y = 20; y <= 24; y += 2) L.R(4, y, 11, y, mO);
        L.R(11, 10, 12, 14, Col.H("#d9d9d9"));
        L.R(7, 1, 8, 26, mO);
        return Mueble("casillero", L, true);
    }

    public static Sprite Rack()
    {
        Sprite s;
        if (EnCache("rack", out s)) return s;
        Lienzo L = new Lienzo(24, 38);
        L.R(0, 0, 23, 37, Col.H("#0d1117"));
        L.R(2, 2, 21, 35, Col.H("#1f2630"));
        for (int y = 4; y < 34; y += 5)
        {
            L.R(3, y, 20, y + 3, Col.H("#2d3743"));
            L.P(5, y + 1, Col.H("#39ff14")); L.P(7, y + 1, y % 2 == 0 ? Col.H("#39ff14") : Col.H("#ff3b3b"));
            L.R(12, y + 1, 18, y + 1, Col.H("#11161d"));
        }
        return Mueble("rack", L, true);
    }

    public static Sprite UPS()
    {
        Sprite s;
        if (EnCache("ups", out s)) return s;
        Lienzo L = new Lienzo(16, 14);
        L.R(0, 0, 15, 13, Col.H("#3c4148"));
        L.R(1, 1, 14, 12, Col.H("#5a6069"));
        L.R(3, 8, 7, 10, Col.H("#9be15d"));
        L.R(10, 3, 10, 6, Col.H("#e63946")); L.R(11, 3, 11, 6, Col.H("#3a86ff")); L.R(12, 3, 12, 6, Col.H("#ffd60a")); L.R(13, 3, 13, 6, Col.H("#2dc653"));
        return Mueble("ups", L, true);
    }

    public static Sprite CamaraTV()
    {
        Sprite s;
        if (EnCache("camaratv", out s)) return s;
        Lienzo L = new Lienzo(18, 30);
        Color n = Col.H("#1b1b1f");
        L.R(8, 0, 9, 14, n); L.R(2, 0, 3, 3, n); L.R(14, 0, 15, 3, n);
        L.R(3, 3, 8, 4, n); L.R(9, 3, 14, 4, n);
        L.R(2, 15, 15, 25, Col.H("#2d2d35"));
        L.R(12, 18, 17, 23, Col.H("#111"));
        L.Circulo(15f, 20.5f, 2f, Col.H("#4cc9f0"));
        L.P(4, 23, Col.H("#ff3b3b"));
        return Mueble("camaratv", L, true);
    }

    public static Sprite Reflector()
    {
        Sprite s;
        if (EnCache("reflector", out s)) return s;
        Lienzo L = new Lienzo(20, 16);
        L.R(1, 2, 18, 13, Col.H("#3a3a42"));
        L.Circulo(10f, 8f, 5f, Col.H("#fff3b0"));
        L.Circulo(10f, 8f, 3f, Col.H("#ffffff"));
        L.R(8, 13, 11, 15, Col.H("#222"));
        L.Borde(Col.H("#120d16"));
        return Crear("reflector", L, PivotCentro);
    }

    public static Sprite MesaLarga(string colorHex, int ancho)
    {
        string k = "mesalarga_" + colorHex + ancho;
        Sprite s;
        if (EnCache(k, out s)) return s;
        Lienzo L = new Lienzo(ancho, 22);
        Color c = Col.H(colorHex), cO = Col.Oscuro(c, 0.68f), cC = Col.Claro(c, 0.2f);
        L.R(0, 0, ancho - 1, 10, cO);
        L.R(0, 10, ancho - 1, 21, c);
        L.R(0, 21, ancho - 1, 21, cC);
        L.R(ancho / 2 - 6, 3, ancho / 2 + 5, 7, cC);
        return Mueble(k, L, true);
    }

    public static Sprite Banca()
    {
        Sprite s;
        if (EnCache("banca", out s)) return s;
        Lienzo L = new Lienzo(64, 12);
        Color m = Col.H("#8b5e3c"), mO = Col.H("#5e3d24");
        L.R(2, 0, 3, 4, mO); L.R(60, 0, 61, 4, mO);
        L.R(0, 4, 63, 7, m); L.R(0, 4, 63, 4, mO);
        L.R(0, 8, 63, 11, mO); L.R(0, 11, 63, 11, Col.Claro(m, 0.2f));
        return Mueble("banca", L, true);
    }

    public static Sprite Butaca()
    {
        Sprite s;
        if (EnCache("butaca", out s)) return s;
        Lienzo L = new Lienzo(14, 14);
        Color r = Col.H("#9d0208"), rO = Col.H("#6a040f");
        L.R(1, 0, 12, 6, rO); L.R(2, 2, 11, 6, r);
        L.R(1, 7, 12, 13, rO); L.R(2, 8, 11, 12, r);
        return Mueble("butaca", L, true);
    }

    public static Sprite Sofa(string colorHex)
    {
        string k = "sofa_" + colorHex;
        Sprite s;
        if (EnCache(k, out s)) return s;
        Lienzo L = new Lienzo(40, 20);
        Color c = Col.H(colorHex), cO = Col.Oscuro(c, 0.7f), cC = Col.Claro(c, 0.15f);
        L.R(0, 2, 39, 19, cO);
        L.R(4, 3, 35, 9, c); L.R(4, 9, 35, 9, cC);
        L.R(4, 10, 35, 17, c); L.R(19, 10, 20, 17, cO);
        L.R(0, 2, 4, 14, c); L.R(35, 2, 39, 14, c);
        return Mueble(k, L, true);
    }

    public static Sprite Mostrador()
    {
        Sprite s;
        if (EnCache("mostrador", out s)) return s;
        Lienzo L = new Lienzo(48, 22);
        L.R(0, 0, 47, 14, Col.H("#22313f"));
        L.R(0, 14, 47, 21, Col.H("#d9d9d9"));
        L.R(0, 21, 47, 21, Col.H("#ffffff"));
        L.R(4, 4, 43, 6, Col.H("#2f6fb0"));
        L.R(30, 15, 40, 20, Col.H("#1e1e24")); L.R(31, 16, 39, 19, Col.H("#4cc9f0"));
        return Mueble("mostrador", L, true);
    }

    public static Sprite Pizarra()
    {
        Sprite s;
        if (EnCache("pizarra", out s)) return s;
        Lienzo L = new Lienzo(48, 14);
        L.R(0, 0, 47, 13, Col.H("#8d99ae"));
        L.R(1, 1, 46, 12, Col.H("#f8f9fa"));
        L.R(4, 9, 20, 9, Col.H("#3a86ff")); L.R(4, 6, 28, 6, Col.H("#3a86ff")); L.R(4, 3, 14, 3, Col.H("#e63946"));
        L.R(34, 3, 42, 10, Col.H("#adb5bd"));
        return Crear("pizarra", L, PivotCentro);
    }

    public static Sprite Afiche(int variante)
    {
        string k = "afiche_" + variante;
        Sprite s;
        if (EnCache(k, out s)) return s;
        string[] fondos = { "#ffb703", "#3a86ff", "#e63946", "#2a9d8f", "#8338ec" };
        Lienzo L = new Lienzo(12, 14);
        Color f = Col.H(fondos[variante % fondos.Length]);
        L.R(0, 0, 11, 13, Col.H("#f1f1f1"));
        L.R(1, 1, 10, 12, f);
        L.R(2, 9, 9, 10, Col.H("#ffffff"));
        L.Circulo(6f, 5.5f, 2.6f, Col.Claro(f, 0.5f));
        return Crear(k, L, PivotCentro);
    }

    public static Sprite Basurero()
    {
        Sprite s;
        if (EnCache("basurero", out s)) return s;
        Lienzo L = new Lienzo(8, 10);
        L.R(1, 0, 6, 8, Col.H("#2a9d8f"));
        L.R(0, 8, 7, 9, Col.H("#1f7067"));
        L.R(2, 2, 2, 6, Col.H("#36b5a5"));
        return Mueble("basurero", L, true);
    }

    public static Sprite SillonDental()
    {
        Sprite s;
        if (EnCache("sillon_dental", out s)) return s;
        Lienzo L = new Lienzo(20, 34);
        L.R(8, 0, 11, 8, Col.H("#adb5bd"));
        L.R(3, 6, 16, 18, Col.H("#4cc9f0")); L.R(3, 6, 16, 7, Col.H("#3aa0c0"));
        L.R(4, 18, 15, 30, Col.H("#4cc9f0")); L.R(6, 28, 13, 32, Col.H("#3aa0c0"));
        L.Circulo(17f, 30f, 2.5f, Col.H("#fff3b0"));
        return Mueble("sillon_dental", L, true);
    }

    public static Sprite MesaLab()
    {
        Sprite s;
        if (EnCache("mesa_lab", out s)) return s;
        Lienzo L = new Lienzo(40, 24);
        L.R(0, 0, 39, 10, Col.H("#d9d9d9"));
        L.R(0, 10, 39, 16, Col.H("#212529"));
        L.R(5, 16, 8, 22, Col.Alfa(Col.H("#90e0ef"), 0.9f)); L.R(6, 17, 7, 19, Col.H("#06d6a0"));
        L.R(14, 16, 15, 21, Col.Alfa(Col.H("#90e0ef"), 0.9f)); L.P(14, 17, Col.H("#ef476f")); L.P(15, 17, Col.H("#ef476f"));
        L.R(24, 16, 29, 18, Col.H("#adb5bd")); L.R(26, 18, 27, 23, Col.H("#495057"));   // microscopio
        L.R(33, 16, 36, 20, Col.Alfa(Col.H("#90e0ef"), 0.9f)); L.R(34, 17, 35, 18, Col.H("#ffd166"));
        return Mueble("mesa_lab", L, true);
    }

    public static Sprite Cama()
    {
        Sprite s;
        if (EnCache("cama", out s)) return s;
        Lienzo L = new Lienzo(26, 36);
        L.R(0, 0, 25, 35, Col.H("#6d4c35"));
        L.R(2, 2, 23, 33, Col.H("#f8f9fa"));
        L.R(2, 2, 23, 22, Col.H("#e9c46a"));
        L.R(5, 26, 20, 31, Col.H("#ffffff"));
        return Mueble("cama", L, true);
    }

    public static Sprite Microfono()
    {
        Sprite s;
        if (EnCache("microfono", out s)) return s;
        Lienzo L = new Lienzo(10, 18);
        L.R(4, 0, 5, 12, Col.H("#333"));
        L.R(1, 0, 8, 1, Col.H("#333"));
        L.R(2, 12, 7, 17, Col.H("#6c757d")); L.R(3, 13, 6, 16, Col.H("#adb5bd"));
        return Mueble("microfono", L, true);
    }

    public static Sprite Escaleras()
    {
        Sprite s;
        if (EnCache("escaleras", out s)) return s;
        Lienzo L = new Lienzo(32, 34);
        for (int i = 0; i < 8; i++)
        {
            Color c = Col.Mezcla(Col.H("#8d8d99"), Col.H("#3b3b46"), i / 8f);
            L.R(2, i * 4, 29, i * 4 + 3, c);
            L.R(2, i * 4 + 3, 29, i * 4 + 3, Col.Claro(c, 0.2f));
        }
        L.R(0, 0, 1, 33, Col.H("#5c4033")); L.R(30, 0, 31, 33, Col.H("#5c4033"));
        return Mueble("escaleras", L, true);
    }

    public static Sprite Ascensor()
    {
        Sprite s;
        if (EnCache("ascensor", out s)) return s;
        Lienzo L = new Lienzo(32, 34);
        L.R(0, 0, 31, 33, Col.H("#5c636e"));
        L.R(3, 0, 28, 30, Col.H("#b8c0cc"));
        L.R(15, 0, 16, 30, Col.H("#6c7480"));
        L.R(5, 2, 13, 28, Col.H("#c9d0da")); L.R(18, 2, 26, 28, Col.H("#c9d0da"));
        L.R(12, 31, 19, 32, Col.H("#1b1b1b")); L.P(14, 31, Col.H("#ff6b35")); L.P(17, 31, Col.H("#ff6b35"));
        return Mueble("ascensor", L, true);
    }

    public static Sprite Cortina()
    {
        Sprite s;
        if (EnCache("cortina", out s)) return s;
        Lienzo L = new Lienzo(16, 16);
        for (int y = 0; y < 16; y++)
            for (int x = 0; x < 16; x++)
            {
                float f = 0.7f + 0.3f * Mathf.Abs(Mathf.Sin(x * 0.8f));
                L.P(x, y, Col.Oscuro(Col.H("#9d0208"), f));
            }
        return Crear("cortina", L, PivotCentro);
    }

    public static Sprite Alfombrilla(string colorHex)
    {
        string k = "alfombrilla_" + colorHex;
        Sprite s;
        if (EnCache(k, out s)) return s;
        Lienzo L = new Lienzo(32, 12);
        Color c = Col.H(colorHex);
        L.R(0, 0, 31, 11, Col.Oscuro(c, 0.7f));
        L.R(2, 2, 29, 9, c);
        L.Ruido(0.1f, 31);
        return Crear(k, L, PivotCentro);
    }

    public static Sprite Sangre(int variante)
    {
        string k = "sangre_" + variante;
        Sprite s;
        if (EnCache(k, out s)) return s;
        Lienzo L = new Lienzo(32, 24);
        System.Random r = new System.Random(variante * 97 + 3);
        Color c = Col.H("#7a0010"), cC = Col.H("#a4161a");
        L.Elipse(16f, 12f, 9f, 6f, c);
        for (int i = 0; i < 9; i++) L.Circulo(4 + r.Next(24), 3 + r.Next(18), 1 + r.Next(3), c);
        L.Elipse(14f, 13f, 4f, 2f, cC);
        return Crear(k, L, PivotCentro);
    }

    public static Sprite Pixel()
    {
        Sprite s;
        if (EnCache("pixel", out s)) return s;
        Lienzo L = new Lienzo(4, 4);
        L.R(0, 0, 3, 3, Color.white);
        return Crear("pixel", L, PivotCentro, 4f, Vector4.zero);
    }
}
