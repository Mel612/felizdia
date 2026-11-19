using UnityEngine;

public static partial class Arte
{
    // Panel con esquinas pixeladas y borde (se estira con 9-slice).
    public static Sprite Panel(Color fondo, Color borde)
    {
        string k = "panel_" + ColorUtility.ToHtmlStringRGBA(fondo) + ColorUtility.ToHtmlStringRGBA(borde);
        Sprite s;
        if (EnCache(k, out s)) return s;
        Lienzo L = new Lienzo(24, 24);
        Color sombra = Col.Alfa(Color.black, 0.35f);
        for (int y = 0; y < 24; y++)
            for (int x = 0; x < 24; x++)
            {
                int dx = Mathf.Min(x, 23 - x), dy = Mathf.Min(y, 23 - y);
                bool esquina = dx + dy < 3;
                if (esquina) continue;
                bool enBorde = dx < 2 || dy < 2 || dx + dy < 5;
                Color c = enBorde ? borde : fondo;
                if (!enBorde && (dx == 2 || dy == 2) && y > 12) c = Col.Mezcla(fondo, borde, 0.25f);   // brillo arriba
                L.P(x, y, c);
            }
        // Sombra inferior
        for (int x = 3; x < 21; x++) if (L.G(x, 0).a < 0.1f) L.P(x, 0, sombra);
        return Crear(k, L, PivotCentro, 100f, new Vector4(8, 8, 8, 8));
    }

    // Hoja de cuaderno con líneas (se repite).
    public static Sprite PapelCuaderno()
    {
        Sprite s;
        if (EnCache("papel", out s)) return s;
        Lienzo L = new Lienzo(64, 32);
        L.R(0, 0, 63, 31, Col.H("#f3ead2"));
        L.Ruido(0.025f, 41);
        L.R(0, 0, 63, 0, Col.H("#9fb7d9"));
        return Crear("papel", L, PivotCentro, 100f, Vector4.zero);
    }

    // Viñeta: transparente en el centro y oscura en los bordes.
    public static Sprite Vineta(string clave, float radioInterior, float radioExterior)
    {
        Sprite s;
        if (EnCache(clave, out s)) return s;
        int n = 128;
        Lienzo L = new Lienzo(n, n);
        for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float dx = (x + 0.5f) / n - 0.5f, dy = (y + 0.5f) / n - 0.5f;
                float d = Mathf.Sqrt(dx * dx + dy * dy) * 2f;
                float a = Mathf.Clamp01((d - radioInterior) / Mathf.Max(0.001f, radioExterior - radioInterior));
                a = a * a * (3f - 2f * a);
                L.P(x, y, new Color(0, 0, 0, a));
            }
        Sprite sp = Crear(clave, L, PivotCentro, 100f, Vector4.zero);
        sp.texture.filterMode = FilterMode.Bilinear;
        return sp;
    }

    // Gotas de sangre que chorrean desde arriba (pantalla de muerte).
    public static Sprite Chorreado()
    {
        Sprite s;
        if (EnCache("chorreado", out s)) return s;
        Lienzo L = new Lienzo(128, 40);
        System.Random r = new System.Random(77);
        Color c = Col.H("#8a0010"), cO = Col.H("#5c000b");
        L.R(0, 30, 127, 39, c);
        int x = 0;
        while (x < 128)
        {
            int ancho = 2 + r.Next(5), largo = 4 + r.Next(26);
            L.R(x, 30 - largo, x + ancho - 1, 30, c);
            L.Circulo(x + ancho / 2f, 30 - largo + 1, ancho / 2f + 0.6f, c);
            L.R(x, 31, x + ancho - 1, 31, cO);
            x += ancho + 1 + r.Next(6);
        }
        return Crear("chorreado", L, PivotCentro, 100f, Vector4.zero);
    }
}
