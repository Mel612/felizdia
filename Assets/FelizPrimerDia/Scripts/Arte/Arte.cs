using System.Collections.Generic;
using UnityEngine;

// =====================================================================
//  ARTE: todo el pixel art del juego se genera por código (16 píxeles = 1 metro).
//  Está dividido en: Arte.cs (base), ArtePersonajes.cs, ArteMundo.cs, ArteUI.cs.
//  Si más adelante tienes sprites dibujados a mano, solo reemplaza lo que devuelven
//  estas funciones.
// =====================================================================
public static partial class Arte
{
    public const float PPU = 16f;

    static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();

    public static readonly Vector2 PivotCentro = new Vector2(0.5f, 0.5f);
    public static readonly Vector2 PivotPies = new Vector2(0.5f, 0f);

    // Convierte un lienzo en Sprite (con caché para no repetir texturas cada día del loop).
    public static Sprite Crear(string clave, Lienzo L, Vector2 pivot)
    {
        return Crear(clave, L, pivot, PPU, Vector4.zero);
    }

    public static Sprite Crear(string clave, Lienzo L, Vector2 pivot, float ppu, Vector4 borde)
    {
        Sprite s;
        if (clave != null && cache.TryGetValue(clave, out s) && s != null) return s;
        Texture2D t = new Texture2D(L.w, L.h, TextureFormat.RGBA32, false);
        t.filterMode = FilterMode.Point;
        t.wrapMode = TextureWrapMode.Clamp;
        Color32[] c32 = new Color32[L.px.Length];
        for (int i = 0; i < c32.Length; i++) c32[i] = L.px[i];
        t.SetPixels32(c32);
        t.Apply();
        s = Sprite.Create(t, new Rect(0, 0, L.w, L.h), pivot, ppu, 0, SpriteMeshType.FullRect, borde);
        s.name = clave ?? "sprite";
        if (clave != null) cache[clave] = s;
        return s;
    }

    public static bool EnCache(string clave, out Sprite s)
    {
        return cache.TryGetValue(clave, out s) && s != null;
    }

    static int Semilla(string s)
    {
        int h = 17;
        foreach (char ch in s) h = h * 31 + ch;
        return h;
    }
}
