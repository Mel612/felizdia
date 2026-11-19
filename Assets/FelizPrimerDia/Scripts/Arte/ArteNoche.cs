using UnityEngine;

// Arte del cuarto secreto de monitoreo (minijuego estilo "cinco noches").
public static partial class Arte
{
    static float AzarN(int n)
    {
        unchecked { uint x = (uint)n * 2654435761u; x ^= x >> 15; x *= 2246822519u; x ^= x >> 13; return (x & 0xFFFF) / 65535f; }
    }

    // La oficina vista de frente: dos puertas, escritorio con monitores viejos, afiches.
    public static Sprite Oficina()
    {
        Sprite s;
        if (EnCache("oficina_noche", out s)) return s;
        Lienzo L = new Lienzo(240, 135);
        Color pared = Col.H("#26222e"), paredO = Col.H("#1b1822"), piso = Col.H("#2e2823"), pisoO = Col.H("#231e1a");
        L.R(0, 0, 239, 134, pared);
        for (int y = 30; y < 135; y += 8) L.R(0, y, 239, y, paredO);                    // azulejos
        for (int x = 0; x < 240; x += 12) for (int y = 30; y < 135; y += 16) L.R(x + ((y / 8) % 2) * 6, y, x + ((y / 8) % 2) * 6, y + 7, paredO);
        for (int x = 0; x < 240; x += 10) for (int y = 0; y < 30; y += 6) L.R(x, y, x + 4, y + 2, ((x / 10 + y / 6) % 2 == 0) ? piso : pisoO);
        L.R(0, 28, 239, 30, Col.H("#3a3340"));

        // Puertas (huecos oscuros con marco)
        int[][] puertas = { new[] { 10, 50 }, new[] { 190, 230 } };
        foreach (int[] p in puertas)
        {
            L.R(p[0] - 3, 26, p[1] + 3, 113, Col.H("#4a4252"));
            L.R(p[0], 28, p[1], 110, Col.H("#050407"));
        }
        // Botoneras junto a las puertas
        L.R(54, 62, 60, 78, Col.H("#3a3a3a")); L.R(55, 72, 59, 76, Col.H("#d62828")); L.R(55, 64, 59, 68, Col.H("#f1f1e6"));
        L.R(180, 62, 186, 78, Col.H("#3a3a3a")); L.R(181, 72, 185, 76, Col.H("#d62828")); L.R(181, 64, 185, 68, Col.H("#f1f1e6"));

        // Afiches: la Bienvenida y "NO CORRER"
        L.R(68, 80, 92, 112, Col.H("#ffd166")); L.R(70, 82, 90, 110, Col.H("#e63946"));
        L.Circulo(80f, 100f, 6f, Col.H("#7bd389")); L.R(76, 96, 84, 96, Col.H("#14110f")); L.P(77, 102, Col.H("#14110f")); L.P(83, 102, Col.H("#14110f"));
        L.R(150, 86, 172, 108, Col.H("#f1faee")); L.R(152, 100, 170, 104, Col.H("#0b3d91")); L.R(152, 90, 166, 92, Col.H("#adb5bd"));
        // Reloj de pared
        L.Circulo(120f, 120f, 7f, Col.H("#f1f1e6")); L.Circulo(120f, 120f, 5.5f, Col.H("#dcdcd0"));
        L.R(120, 120, 120, 125, Col.H("#14110f")); L.R(120, 120, 123, 120, Col.H("#14110f"));

        // Escritorio con monitores CRT
        L.R(64, 18, 176, 46, Col.H("#4a3a2c")); L.R(64, 44, 176, 46, Col.H("#6b543f"));
        L.R(68, 18, 70, 6, Col.H("#3a2d22")); L.R(170, 18, 172, 6, Col.H("#3a2d22"));
        for (int i = 0; i < 3; i++)
        {
            int x = 76 + i * 30;
            L.R(x, 46, x + 24, 68, Col.H("#8d8d86")); L.R(x + 2, 48, x + 22, 66, Col.H("#3b3b37"));
            L.R(x + 4, 50, x + 20, 64, Col.H(i == 1 ? "#1f6f4a" : "#163b2a"));
            for (int y = 51; y < 64; y += 2) L.R(x + 4, y, x + 20, y, Col.H(i == 1 ? "#2a8a5c" : "#1d4d37"));
        }
        // Ventilador, teléfono y vaso de café
        L.Circulo(166f, 56f, 6f, Col.H("#5c5c5c")); L.Circulo(166f, 56f, 2f, Col.H("#999999")); L.R(165, 46, 167, 50, Col.H("#5c5c5c"));
        L.R(68, 46, 74, 50, Col.H("#d62828")); L.R(176 - 10, 46, 176 - 7, 51, Col.H("#f1faee"));
        // Lámpara del techo
        L.R(110, 130, 130, 134, Col.H("#4a4a4a")); L.R(112, 128, 128, 129, Col.H("#fff1c1"));
        L.Borde(Col.H("#0b0a0e"));
        return Crear("oficina_noche", L, PivotCentro);
    }

    // Persiana metálica (puerta cerrada)
    public static Sprite Persiana()
    {
        Sprite s;
        if (EnCache("persiana", out s)) return s;
        Lienzo L = new Lienzo(41, 83);
        L.R(0, 0, 40, 82, Col.H("#5f6670"));
        for (int y = 2; y < 83; y += 5) { L.R(0, y, 40, y, Col.H("#3f454d")); L.R(0, y + 1, 40, y + 1, Col.H("#7d858f")); }
        L.R(0, 0, 40, 3, Col.H("#d62828"));
        for (int x = 2; x < 40; x += 6) L.R(x, 0, x + 2, 3, Col.H("#ffd166"));
        return Crear("persiana", L, PivotCentro);
    }

    // Vista de una cámara (pasillo en perspectiva u otro lugar)
    public static Sprite VistaCamara(int cam)
    {
        Sprite s;
        string k = "camara_" + cam;
        if (EnCache(k, out s)) return s;
        Lienzo L = new Lienzo(160, 90);
        string[] paredes = { "#1d2a1f", "#2a2630", "#2c3038", "#2b3a3a", "#2e2a2a", "#2a2a33" };
        Color pared = Col.H(paredes[cam % paredes.Length]), oscuro = Col.Oscuro(pared, 0.55f), claro = Col.Claro(pared, 0.15f);
        L.R(0, 0, 159, 89, oscuro);
        if (cam == 0)
        {
            // Patio de noche: cielo, árboles, banca
            for (int y = 40; y < 90; y++) L.R(0, y, 159, y, Col.Mezcla(Col.H("#141a33"), Col.H("#070914"), (y - 40) / 50f));
            L.R(0, 0, 159, 40, Col.H("#1f3a24"));
            for (int i = 0; i < 160; i += 3) L.P(i, (int)(AzarN(i) * 38), Col.H("#2d5a35"));
            L.Circulo(30f, 52f, 14f, Col.H("#10241a")); L.R(28, 30, 32, 42, Col.H("#2b1d14"));
            L.Circulo(128f, 55f, 16f, Col.H("#10241a")); L.R(126, 30, 130, 42, Col.H("#2b1d14"));
            L.R(62, 26, 98, 30, Col.H("#5c4033")); L.R(64, 20, 66, 26, Col.H("#3d2b1f")); L.R(94, 20, 96, 26, Col.H("#3d2b1f"));
        }
        else
        {
            // Pasillo en perspectiva
            // Piso que se angosta hacia el fondo
            for (int y = 0; y < 30; y++)
                L.R(Mathf.Max(0, y * 2), y, Mathf.Min(159, 159 - y * 2), y, Col.Mezcla(Col.H("#3a352f"), Col.H("#1a1714"), y / 30f));
            L.R(0, 30, 159, 89, oscuro);
            L.R(20, 30, 140, 75, pared);
            L.R(60, 30, 100, 62, Col.H("#0b0a0e"));                         // fondo del pasillo
            L.R(66, 30, 94, 56, Col.H("#15131a"));
            for (int x = 22; x < 58; x += 12) L.R(x, 34, x + 7, 58, claro);  // puertas laterales
            for (int x = 102; x < 140; x += 12) L.R(x, 34, x + 7, 58, claro);
            L.R(70, 76, 90, 78, Col.H("#fff1c1"));                            // luz del techo
            if (cam == 1)
                for (int k2 = 0; k2 < 8; k2++) L.R(20 + k2 * 6, 30 + k2 * 5, 60, 34 + k2 * 5, Col.H("#4a4252"));   // escaleras
            if (cam == 4 || cam == 5)
            {
                int x = cam == 4 ? 12 : 128;
                L.R(x, 30, x + 20, 70, Col.H("#3f454d"));                     // puerta de Seguridad
                L.R(x + 3, 60, x + 17, 64, Col.H("#d62828"));
            }
        }
        L.Ruido(0.08f, cam * 13 + 5);
        return Crear(k, L, PivotCentro);
    }

    // Estática de las cámaras
    public static Sprite Estatica(int variante)
    {
        Sprite s;
        string k = "estatica_" + variante;
        if (EnCache(k, out s)) return s;
        Lienzo L = new Lienzo(80, 45);
        for (int y = 0; y < 45; y++)
            for (int x = 0; x < 80; x++)
            {
                float v = AzarN(x * 131 + y * 977 + variante * 7919);
                L.P(x, y, new Color(v, v, v, v * 0.9f));
            }
        return Crear(k, L, PivotCentro);
    }

    // La cara del susto final
    public static Sprite CaraSusto()
    {
        Sprite s;
        if (EnCache("cara_susto", out s)) return s;
        Lienzo L = new Lienzo(64, 64);
        Color verde = Col.H("#7bd389"), verdeO = Col.H("#2d6a4f"), negro = Col.H("#050407");
        L.Elipse(32f, 32f, 29f, 31f, verdeO);
        L.Elipse(32f, 33f, 26f, 28f, verde);
        L.Elipse(20f, 40f, 7f, 8f, negro); L.Elipse(44f, 40f, 7f, 8f, negro);           // ojos huecos
        L.Circulo(21f, 40f, 1.6f, Col.H("#ff3b3b")); L.Circulo(45f, 40f, 1.6f, Col.H("#ff3b3b"));
        L.Elipse(32f, 17f, 19f, 8f, Col.H("#a4161a"));                                   // sonrisa enorme
        L.Elipse(32f, 19f, 17f, 5f, negro);
        for (int x = 16; x <= 48; x += 4) L.R(x, 18, x + 1, 22, Col.H("#f1f1e6"));     // dientes
        for (int x = 18; x <= 46; x += 4) L.R(x, 13, x + 1, 16, Col.H("#f1f1e6"));
        L.R(8, 50, 12, 56, Col.H("#ffd166")); L.R(52, 50, 56, 56, Col.H("#ffd166"));     // rombos
        L.R(30, 55, 31, 62, Col.H("#1b4332")); L.R(31, 46, 34, 52, Col.H("#1b4332"));   // grietas
        L.Borde(negro);
        return Crear("cara_susto", L, PivotCentro);
    }

    // Líneas horizontales de televisor viejo (para los recuerdos)
    public static Sprite LineasTV()
    {
        Sprite s;
        if (EnCache("lineas_tv", out s)) return s;
        Lienzo L = new Lienzo(2, 270);
        for (int y = 0; y < 270; y += 2) L.R(0, y, 1, y, new Color(0f, 0f, 0f, 0.6f));
        return Crear("lineas_tv", L, PivotCentro);
    }

    // Degradado para las barras de cine: oscuro en el borde de la pantalla, transparente hacia el centro
    public static Sprite DegradadoCine(bool arriba)
    {
        Sprite s;
        string k = "degradado_cine_" + arriba;
        if (EnCache(k, out s)) return s;
        Lienzo L = new Lienzo(2, 64);
        for (int y = 0; y < 64; y++)
        {
            float t = arriba ? y / 63f : 1f - y / 63f;       // 1 = borde de la pantalla
            L.R(0, y, 1, y, new Color(1f, 1f, 1f, Mathf.Pow(t, 1.6f)));
        }
        return Crear(k, L, PivotCentro);
    }

    // Máquina de escribir sobre una mesita (punto de guardado, como en Resident Evil)
    public static Sprite MaquinaEscribir()
    {
        Sprite s;
        if (EnCache("maquina_escribir", out s)) return s;
        Lienzo L = new Lienzo(24, 26);
        Color mad = Col.H("#6b4a2f"), madO = Col.H("#4a3220"), negro = Col.H("#1a1a1f"), metal = Col.H("#3c3c44"), papel = Col.H("#f1ead2");
        // Mesita
        L.R(2, 0, 3, 11, madO); L.R(20, 0, 21, 11, madO);
        L.R(0, 10, 23, 13, mad); L.R(0, 13, 23, 13, Col.Claro(mad, 0.2f));
        // Cuerpo de la máquina
        L.R(3, 14, 20, 19, negro);
        L.R(4, 15, 19, 18, metal);
        for (int x = 5; x <= 18; x += 2) { L.P(x, 16, Col.H("#d8d8d0")); L.P(x, 17, Col.H("#9a9a92")); }   // teclas
        L.R(2, 19, 21, 21, negro);                                        // rodillo
        L.R(1, 20, 1, 20, metal); L.R(22, 20, 22, 20, metal);
        // Hoja de papel con texto
        L.R(7, 21, 16, 25, papel);
        L.R(8, 23, 14, 23, Col.H("#8a8270")); L.R(8, 22, 12, 22, Col.H("#8a8270"));
        // Cinta de tinta roja
        L.R(18, 18, 19, 19, Col.H("#a4161a"));
        L.Borde(Col.H("#0b0a0e"));
        return Crear("maquina_escribir", L, PivotPies);
    }
}
