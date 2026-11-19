using UnityEngine;

// =====================================================================
//  LA ENTRADA DE LA UNIFRANZ (vista de frente), para la pantalla de inicio
//  (de noche) y la cinemática de llegada (de día).
//  Edificio blanco de 7 pisos con la torre gris, la marquesina oscura con el
//  letrero, banderas, la entrada de vidrio y "UNIFRANZ" abajo a la izquierda.
//  Detrás: el cielo de La Paz con el Illimani.
// =====================================================================
public static class FachadaUnifranz
{
    public const float Ancho = 16f;        // unidades (256 px / 16)

    // Dónde está dibujada en la escena de la Planta Baja (lejos del campus)
    public static readonly Vector2 OrigenNoche = new Vector2(-6000f, 0f);
    public static readonly Vector2 OrigenDia = new Vector2(-6000f, 100f);

    static Transform padre;
    static Cine cine;

    // Arma todo en "o" (centro de la base del edificio). Devuelve el punto de la puerta.
    // En OrigenNoche / OrigenDia queda dibujada en la escena (se puede ver y editar);
    // en cualquier otro lugar (recuerdos) se arma solo para la cinemática.
    public static Vector2 Construir(Cine c, Vector2 o, bool noche)
    {
        bool enEscena = (noche && (o - OrigenNoche).sqrMagnitude < 0.01f) || (!noche && (o - OrigenDia).sqrMagnitude < 0.01f);
        cine = enEscena ? null : c;
        if (enEscena)
        {
            Horno.Empezar("Fachada_" + (noche ? "noche" : "dia"));
            GameObject g = new GameObject("Entrada Unifranz (" + (noche ? "noche, menú" : "día, llegada") + ")");
            g.transform.SetParent(Juego.I != null && Juego.I.Raiz != null ? Juego.I.Raiz : null, false);
            padre = g.transform;
        }
        else
        {
            Horno.Suspender();
            padre = c.transform;
        }
        try
        {
            // Cielo con el Illimani
            Plano("Cielo", Cielo(noche), o + new Vector2(0f, -2f), -3100).transform.localScale = new Vector3(3.2f, 3.2f, 1f);

            // Edificios vecinos (siluetas) y el de la U
            R(Util.Rect("Edificio vecino", padre, o + new Vector2(-12.5f, 3f), new Vector2(9f, 6f), Util.Hex(noche ? "#2a2f45" : "#c9b79c"), -3060));
            R(Util.Rect("Edificio vecino", padre, o + new Vector2(14f, 1.5f), new Vector2(8f, 3f), Util.Hex(noche ? "#262b3f" : "#b9a58a"), -3060));
            Plano("Fachada Unifranz", Edificio(noche), o, -3050);

            // Letreros
            TextMesh t1 = Util.Texto("Universidad Franz Tamayo", padre, o + new Vector2(1.2f, 2.32f), 0.6f, Util.Hex("#f1faee"), -3000);
            if (cine != null) cine.Registrar(t1.gameObject);
            TextMesh t2 = Util.Texto("UNIFRANZ", padre, o + new Vector2(-5.25f, 0.62f), 0.9f, Util.Hex("#0b3d91"), -3000);
            t2.fontStyle = FontStyle.Bold;
            if (cine != null) cine.Registrar(t2.gameObject);

            // Vereda y calle (Av. Héroes del Acre)
            R(Util.Mosaico("Vereda", padre, Arte.Suelo(TipoSuelo.Piedra, Util.Hex(noche ? "#7a7872" : "#b5b0a5")), o + new Vector2(0f, -1f), new Vector2(40f, 2f), -3040));
            R(Util.Rect("Cordón", padre, o + new Vector2(0f, -2.05f), new Vector2(40f, 0.15f), Util.Hex("#d9d9d9"), -3039));
            R(Util.Mosaico("Calle", padre, Arte.Suelo(TipoSuelo.Calle, Util.Hex(noche ? "#3a3a40" : "#55555c")), o + new Vector2(0f, -4.1f), new Vector2(40f, 4f), -3041));
            for (float x = -18f; x < 20f; x += 4f)
                R(Util.Rect("Línea", padre, o + new Vector2(x, -4.1f), new Vector2(1.8f, 0.14f), Util.Hex("#e9c46a"), -3038));

            // Árboles a la derecha y faroles
            Plano("Árbol", Arte.Arbol(), o + new Vector2(8.6f, -0.3f), OrdenPorY.Orden(o.y - 0.3f));
            Plano("Árbol", Arte.Arbol(), o + new Vector2(9.8f, -0.5f), OrdenPorY.Orden(o.y - 0.5f));
            Farol(o + new Vector2(5.6f, -1.6f), noche);
            Farol(o + new Vector2(-7.2f, -1.6f), noche);
        }
        finally
        {
            if (enEscena) Horno.Terminar(); else Horno.Reanudar();
        }
        return o + new Vector2(1.7f, 0f);
    }

    static SpriteRenderer Plano(string nombre, Sprite s, Vector2 pos, int orden) { return R(Util.Plano(nombre, padre, s, pos, orden)); }

    // Lo que no queda en la escena se borra al terminar la cinemática
    static SpriteRenderer R(SpriteRenderer sr)
    {
        if (cine != null && sr != null) cine.Registrar(sr.gameObject);
        return sr;
    }

    static void Farol(Vector2 pie, bool noche)
    {
        R(Util.Rect("Poste", padre, pie + new Vector2(0f, 1.7f), new Vector2(0.14f, 3.4f), Util.Hex("#2b2d42"), OrdenPorY.Orden(pie.y)));
        R(Util.Rect("Lámpara", padre, pie + new Vector2(0f, 3.45f), new Vector2(0.55f, 0.22f), Util.Hex(noche ? "#ffe8a3" : "#adb5bd"), OrdenPorY.Orden(pie.y) + 1));
        if (!noche) return;
        SpriteRenderer luz = Plano("Luz del farol", Arte.Luz(), pie + new Vector2(0f, 2.2f), 30000);
        luz.color = new Color(1f, 0.85f, 0.55f, 0.28f);
        luz.transform.localScale = new Vector3(2.6f, 3.2f, 1f);
    }

    // =================================================================
    //  ARTE
    // =================================================================
    static float Azar(int n)
    {
        unchecked { uint x = (uint)n * 2654435761u; x ^= x >> 15; x *= 2246822519u; x ^= x >> 13; return (x & 0xFFFF) / 65535f; }
    }

    public static Sprite Edificio(bool noche)
    {
        Sprite s;
        string k = "fachada_unifranz_" + noche;
        if (Arte.EnCache(k, out s)) return s;
        Lienzo L = new Lienzo(256, 180);
        Color pared = Col.H(noche ? "#b4bdd0" : "#f4f5f7"), paredS = Col.H(noche ? "#8d97ad" : "#d5d9e0");
        Color marco = Col.H(noche ? "#2d3446" : "#5c6675");
        Color vidOsc = Col.H(noche ? "#1a2336" : "#86b2d3"), vidBrillo = Col.H(noche ? "#2b3a58" : "#c8e2f3");
        Color luz = Col.H("#ffd27a"), luzB = Col.H("#fff1c1");
        Color torre = Col.H(noche ? "#4b5260" : "#7d848f"), torreS = Col.H(noche ? "#3a404c" : "#646b75");
        int baseY = 42, piso = 18, pisos = 7, top = baseY + pisos * piso;      // 168

        // Planta baja y alas
        L.R(16, 0, 240, baseY, pared);
        L.R(16, baseY, 112, top, pared);
        L.R(140, baseY, 240, top, pared);
        L.R(228, 0, 240, top, paredS);                    // sombra de la esquina
        L.R(14, top, 114, top + 3, paredS);               // cornisas
        L.R(138, top, 242, top + 3, paredS);
        for (int f = 0; f <= pisos; f++)
        {
            int y = baseY + f * piso;
            L.R(16, y, 112, y, paredS);
            L.R(140, y, 240, y, paredS);
        }

        // Ventanas de las dos alas
        int semilla = 0;
        int[][] alas = { new[] { 18, 110 }, new[] { 142, 228 } };
        foreach (int[] ala in alas)
            for (int f = 0; f < pisos; f++)
                for (int x = ala[0] + 4; x + 10 <= ala[1]; x += 14)
                {
                    int y = baseY + f * piso + 4;
                    L.R(x - 1, y - 1, x + 10, y + 10, marco);
                    bool encendida = noche && Azar(semilla++ * 7 + f) < 0.38f;
                    L.R(x, y, x + 9, y + 9, encendida ? luz : vidOsc);
                    L.R(x, y + 7, x + 9, y + 9, encendida ? luzB : vidBrillo);
                    L.R(x + 4, y, x + 5, y + 9, marco);               // parante
                }

        // Torre gris del medio (sobresale arriba)
        L.R(112, baseY - 2, 140, top + 10, torre);
        L.R(112, baseY - 2, 114, top + 10, torreS);
        L.R(138, baseY - 2, 140, top + 10, torreS);
        L.R(118, baseY + 2, 133, top + 4, marco);
        for (int f = 0; f < pisos; f++)
        {
            int y = baseY + f * piso + 3;
            bool encendida = noche && Azar(900 + f) < 0.5f;
            L.R(119, y, 132, y + 13, encendida ? luz : vidOsc);
            L.R(119, y + 10, 132, y + 13, encendida ? luzB : vidBrillo);
        }

        // Marquesina oscura con el letrero
        L.R(56, 33, 238, 41, Col.H("#1f2633"));
        L.R(56, 33, 238, 33, Col.H("#3b4456"));
        L.R(56, 41, 238, 41, Col.H("#3b4456"));

        // Banderas sobre la marquesina
        string[][] banderas =
        {
            new[] { "#d52b1e", "#f9e300", "#007934" },    // Bolivia
            new[] { "#0b3d91", "#ffffff", "#0b3d91" },
            new[] { "#d52b1e", "#ffffff", "#d52b1e" },
            new[] { "#f9e300", "#0038a8", "#ce1126" },
            new[] { "#d52b1e", "#f9e300", "#007934" },
            new[] { "#75aadb", "#ffffff", "#75aadb" },
            new[] { "#009b3a", "#fedf00", "#002776" },
            new[] { "#d52b1e", "#f9e300", "#007934" },
        };
        for (int i = 0; i < banderas.Length; i++)
        {
            int x = 146 + i * 10;
            L.R(x, 42, x, 60, Col.H("#d9d9d9"));
            for (int k2 = 0; k2 < 3; k2++) L.R(x + 1, 58 - k2 * 2, x + 7, 59 - k2 * 2, Col.H(banderas[i][k2]));
        }

        // Entrada de vidrio con columnas
        Color interior = Col.H(noche ? "#ffe3a1" : "#6f8fa6"), interiorB = Col.H(noche ? "#fff4d6" : "#9fbfd6");
        L.R(110, 0, 204, 31, interior);
        L.R(110, 22, 204, 31, interiorB);
        for (int x = 110; x <= 204; x += 12) L.R(x, 0, x, 31, marco);
        L.R(150, 0, 151, 24, marco); L.R(162, 0, 163, 24, marco);        // puertas
        L.R(104, 0, 110, 32, pared); L.R(204, 0, 210, 32, pared);
        L.R(104, 0, 105, 32, paredS); L.R(209, 0, 210, 32, paredS);

        // Local a la derecha y panel de UNIFRANZ a la izquierda
        L.R(214, 4, 236, 28, marco); L.R(215, 5, 235, 27, vidOsc); L.R(215, 22, 235, 27, vidBrillo);
        L.R(20, 4, 68, 16, Col.H("#ffffff"));
        L.R(20, 4, 68, 4, paredS);
        L.R(70, 0, 102, 31, marco); L.R(71, 1, 101, 30, vidOsc); L.R(71, 24, 101, 30, vidBrillo);

        L.Borde(Col.H("#14161c"));
        return Arte.Crear(k, L, Arte.PivotPies);
    }

    // Cielo de La Paz con el Illimani al fondo
    public static Sprite Cielo(bool noche)
    {
        Sprite s;
        string k = "cielo_lapaz_" + noche;
        if (Arte.EnCache(k, out s)) return s;
        Lienzo L = new Lienzo(160, 100);
        Color arriba = Col.H(noche ? "#070b1d" : "#4ea3dc"), abajo = Col.H(noche ? "#25335e" : "#cfe8f7");
        for (int y = 0; y < L.h; y++) L.R(0, y, L.w - 1, y, Col.Mezcla(abajo, arriba, y / (float)L.h));
        if (noche)
        {
            for (int i = 0; i < 70; i++)
            {
                int x = (int)(Azar(i * 3 + 1) * 159), y = 40 + (int)(Azar(i * 5 + 2) * 59);
                L.P(x, y, new Color(1f, 1f, 1f, 0.4f + Azar(i) * 0.6f));
            }
            L.Circulo(24f, 60f, 5f, Col.H("#f1f1e6"));
            L.Circulo(26f, 62f, 4.2f, Col.Mezcla(abajo, arriba, 62f / L.h));   // luna creciente
        }
        else
        {
            for (int i = 0; i < 5; i++)
            {
                float cx = 15f + i * 33f, cy = 78f + Azar(i) * 14f;
                L.Elipse(cx, cy, 10f, 3f, new Color(1f, 1f, 1f, 0.85f));
                L.Elipse(cx + 5f, cy + 2f, 6f, 3f, new Color(1f, 1f, 1f, 0.9f));
            }
        }
        // Illimani (tres picos nevados)
        Color roca = Col.H(noche ? "#3d4a70" : "#7f8fa8"), nieve = Col.H(noche ? "#c9d3ea" : "#ffffff");
        float[] picos = { 112f, 128f, 146f };
        float[] altos = { 30f, 38f, 33f };
        for (int x = 0; x < L.w; x++)
        {
            float h = 6f;
            for (int p = 0; p < picos.Length; p++) h = Mathf.Max(h, altos[p] - Mathf.Abs(x - picos[p]) * 0.75f);
            for (int y = 0; y < (int)h; y++) L.P(x, y, y > h - 9f ? nieve : roca);
        }
        return Arte.Crear(k, L, Arte.PivotPies);
    }
}
