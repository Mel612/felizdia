using UnityEngine;

// Un "lienzo" de píxeles para dibujar pixel art por código.
// (0,0) es la esquina inferior izquierda, igual que las texturas de Unity.
public class Lienzo
{
    public readonly int w, h;
    public readonly Color[] px;

    public Lienzo(int ancho, int alto)
    {
        w = ancho;
        h = alto;
        px = new Color[w * h];
    }

    public bool Dentro(int x, int y) { return x >= 0 && y >= 0 && x < w && y < h; }

    public Color G(int x, int y) { return Dentro(x, y) ? px[y * w + x] : new Color(0, 0, 0, 0); }

    public void P(int x, int y, Color c)
    {
        if (!Dentro(x, y)) return;
        int i = y * w + x;
        if (c.a >= 0.999f) { px[i] = c; return; }
        Color b = px[i];
        float a = c.a + b.a * (1f - c.a);
        if (a <= 0.0001f) { px[i] = new Color(0, 0, 0, 0); return; }
        px[i] = new Color(
            (c.r * c.a + b.r * b.a * (1f - c.a)) / a,
            (c.g * c.a + b.g * b.a * (1f - c.a)) / a,
            (c.b * c.a + b.b * b.a * (1f - c.a)) / a, a);
    }

    public void Borrar(int x, int y) { if (Dentro(x, y)) px[y * w + x] = new Color(0, 0, 0, 0); }

    // Rectángulo (inclusive) de (x0,y0) a (x1,y1).
    public void R(int x0, int y0, int x1, int y1, Color c)
    {
        if (x1 < x0) { int t = x0; x0 = x1; x1 = t; }
        if (y1 < y0) { int t = y0; y0 = y1; y1 = t; }
        for (int y = y0; y <= y1; y++)
            for (int x = x0; x <= x1; x++) P(x, y, c);
    }

    public void Elipse(float cx, float cy, float rx, float ry, Color c)
    {
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float dx = (x + 0.5f - cx) / rx, dy = (y + 0.5f - cy) / ry;
                if (dx * dx + dy * dy <= 1f) P(x, y, c);
            }
    }

    public void Circulo(float cx, float cy, float r, Color c) { Elipse(cx, cy, r, r, c); }

    // Contorno: pinta los píxeles vacíos que tocan un píxel lleno.
    public void Borde(Color c)
    {
        bool[] lleno = new bool[px.Length];
        for (int i = 0; i < px.Length; i++) lleno[i] = px[i].a > 0.5f;
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                if (lleno[y * w + x]) continue;
                bool toca = (x > 0 && lleno[y * w + x - 1]) || (x < w - 1 && lleno[y * w + x + 1]) ||
                            (y > 0 && lleno[(y - 1) * w + x]) || (y < h - 1 && lleno[(y + 1) * w + x]);
                if (toca) px[y * w + x] = c;
            }
    }

    // Variación de brillo aleatoria (textura).
    public void Ruido(float cantidad, int semilla)
    {
        System.Random r = new System.Random(semilla);
        for (int i = 0; i < px.Length; i++)
        {
            if (px[i].a <= 0f) continue;
            float f = 1f + ((float)r.NextDouble() - 0.5f) * cantidad;
            Color c = px[i];
            px[i] = new Color(Mathf.Clamp01(c.r * f), Mathf.Clamp01(c.g * f), Mathf.Clamp01(c.b * f), c.a);
        }
    }

    public static bool Igual(Color a, Color b)
    {
        return Mathf.Abs(a.r - b.r) < 0.004f && Mathf.Abs(a.g - b.g) < 0.004f && Mathf.Abs(a.b - b.b) < 0.004f && a.a > 0.5f;
    }
}

// Ayudas de color.
public static class Col
{
    public static Color H(string hex)
    {
        if (string.IsNullOrEmpty(hex)) return Color.magenta;
        if (hex[0] == '#') hex = hex.Substring(1);
        if (hex.Length == 3) hex = "" + hex[0] + hex[0] + hex[1] + hex[1] + hex[2] + hex[2];
        if (hex.Length < 6) return Color.magenta;
        float r = System.Convert.ToInt32(hex.Substring(0, 2), 16) / 255f;
        float g = System.Convert.ToInt32(hex.Substring(2, 2), 16) / 255f;
        float b = System.Convert.ToInt32(hex.Substring(4, 2), 16) / 255f;
        float a = hex.Length >= 8 ? System.Convert.ToInt32(hex.Substring(6, 2), 16) / 255f : 1f;
        return new Color(r, g, b, a);
    }

    public static Color Oscuro(Color c, float f) { return new Color(c.r * f, c.g * f, c.b * f, c.a); }

    public static Color Claro(Color c, float t) { return Mezcla(c, Color.white, t); }

    public static Color Mezcla(Color a, Color b, float t)
    {
        return new Color(a.r + (b.r - a.r) * t, a.g + (b.g - a.g) * t, a.b + (b.b - a.b) * t, a.a + (b.a - a.a) * t);
    }

    public static Color Alfa(Color c, float a) { return new Color(c.r, c.g, c.b, a); }
}
