using UnityEngine;
using UnityEngine.UI;

// Herramientas para crear sprites, textos y UI por código.
// Todo el arte es provisional (cuadrados y pixel art generado): reemplázalo con tus sprites.
public static class Util
{
    static Sprite cuadrado;
    static Material matSprite;
    static Font fuente;

    // Capa "Ignore Raycast" (2): personajes y zonas. Así la visión del Pepino solo choca con paredes.
    public const int CapaSinVision = 2;

    public static Font Fuente
    {
        get
        {
            if (fuente == null) fuente = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return fuente;
        }
    }

    public static Sprite Cuadrado
    {
        get
        {
            if (cuadrado == null)
            {
                Texture2D t = new Texture2D(4, 4);
                Color32[] px = new Color32[16];
                for (int i = 0; i < 16; i++) px[i] = new Color32(255, 255, 255, 255);
                t.SetPixels32(px);
                t.filterMode = FilterMode.Point;
                t.Apply();
                cuadrado = Sprite.Create(t, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f, 0, SpriteMeshType.FullRect);
                cuadrado.name = "cuadrado";
            }
            return cuadrado;
        }
    }

    // Material "sin luces" para que los sprites se vean aunque la escena no tenga luces 2D (URP).
    public static Material MatSprite
    {
        get
        {
            if (matSprite == null)
            {
                Shader sh = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
                if (sh == null) sh = Shader.Find("Sprites/Default");
                if (sh != null) matSprite = new Material(sh);
            }
            return matSprite;
        }
    }

    public static void AplicarMaterial(SpriteRenderer sr)
    {
        if (MatSprite != null) sr.sharedMaterial = MatSprite;
    }

    public static Color Hex(string hex) { return Col.H(hex); }

    // Rectángulo de color en el mundo (1 unidad = 1 metro aprox.).
    public static SpriteRenderer Rect(string nombre, Transform padre, Vector2 centro, Vector2 tam, Color c, int orden)
    {
        string id;
        SpriteRenderer ya = Horno.Reusar<SpriteRenderer>(out id);
        if (ya != null) return ya;
        GameObject go = new GameObject(nombre);
        Horno.Marcar(go, id);
        go.transform.SetParent(padre, false);
        go.transform.position = new Vector3(centro.x, centro.y, 0f);
        go.transform.localScale = new Vector3(tam.x, tam.y, 1f);
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Cuadrado;
        sr.color = c;
        sr.sortingOrder = orden;
        AplicarMaterial(sr);
        return sr;
    }

    public static GameObject Muro(Transform padre, Vector2 centro, Vector2 tam, Color c)
    {
        SpriteRenderer sr = Rect("Muro", padre, centro, tam, c, 0);
        Colision(sr.gameObject);
        return sr.gameObject;
    }

    // Devuelve el BoxCollider2D del objeto (lo crea si no tiene). Evita duplicados en objetos de la escena.
    public static BoxCollider2D Colision(GameObject go)
    {
        BoxCollider2D bc = go.GetComponent<BoxCollider2D>();
        if (bc == null) bc = go.AddComponent<BoxCollider2D>();
        return bc;
    }

    // Texto flotante en el mundo (carteles, nombres).
    public static TextMesh Texto(string texto, Transform padre, Vector2 pos, float tam, Color c, int orden)
    {
        string id;
        TextMesh ya = Horno.Reusar<TextMesh>(out id);
        if (ya != null) return ya;
        GameObject go = new GameObject("Texto: " + (texto.Length > 24 ? texto.Substring(0, 24) : texto));
        Horno.Marcar(go, id);
        go.transform.SetParent(padre, false);
        go.transform.position = new Vector3(pos.x, pos.y, 0f);
        TextMesh tm = go.AddComponent<TextMesh>();
        tm.text = texto;
        tm.font = Fuente;
        tm.fontSize = 48;
        tm.characterSize = 0.06f * tam;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = c;
        MeshRenderer mr = go.GetComponent<MeshRenderer>();
        if (Fuente != null) mr.sharedMaterial = Fuente.material;
        mr.sortingOrder = 3000 + orden;
        return tm;
    }

    // ---------- Pixel art (ver carpeta Arte/) ----------
    public static Sprite SpritePersonaje(Color ropa, Color piel, Color pelo, Color pantalon)
    {
        Apariencia a = new Apariencia();
        a.ropa = ropa; a.piel = piel; a.pelo = pelo; a.pantalon = pantalon;
        a.zapatos = Hex("#2b2230"); a.acc = UnityEngine.Color.white;
        return Arte.Personaje(a).Frente;
    }

    public static Sprite SpritePepino() { return Arte.Pepino().Frente; }

    // Coloca un objeto con sprite en el mundo. Si "colision" no es cero, le pone un collider
    // de ese tamaño en la base (para que se pueda caminar "detrás" de los muebles altos).
    public static SpriteRenderer Objeto(string nombre, Transform padre, Sprite sprite, Vector2 pos, Vector2 colision)
    {
        string id;
        SpriteRenderer ya = Horno.Reusar<SpriteRenderer>(out id);
        if (ya != null) return ya;
        GameObject go = new GameObject(nombre);
        Horno.Marcar(go, id);
        go.transform.SetParent(padre, false);
        go.transform.position = new Vector3(pos.x, pos.y, 0f);
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        AplicarMaterial(sr);
        sr.sortingOrder = OrdenPorY.Orden(pos.y);
        go.AddComponent<OrdenPorY>();
        if (colision.x > 0f && colision.y > 0f)
        {
            BoxCollider2D bc = go.AddComponent<BoxCollider2D>();
            bc.size = colision;
            bc.offset = new Vector2(0f, colision.y / 2f);
        }
        return sr;
    }

    public static SpriteRenderer Objeto(string nombre, Transform padre, Sprite sprite, Vector2 pos)
    {
        return Objeto(nombre, padre, sprite, pos, Vector2.zero);
    }

    // Sprite plano pegado al suelo o a la pared (no se ordena por profundidad).
    public static SpriteRenderer Plano(string nombre, Transform padre, Sprite sprite, Vector2 centro, int orden)
    {
        string id;
        SpriteRenderer ya = Horno.Reusar<SpriteRenderer>(out id);
        if (ya != null) return ya;
        GameObject go = new GameObject(nombre);
        Horno.Marcar(go, id);
        go.transform.SetParent(padre, false);
        go.transform.position = new Vector3(centro.x, centro.y, 0f);
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = orden;
        AplicarMaterial(sr);
        return sr;
    }

    // Superficie con textura repetida (suelos, paredes, cortinas).
    public static SpriteRenderer Mosaico(string nombre, Transform padre, Sprite sprite, Vector2 centro, Vector2 tam, int orden)
    {
        string id;
        SpriteRenderer ya = Horno.Reusar<SpriteRenderer>(out id);
        if (ya != null) return ya;
        Horno.Suspender();
        SpriteRenderer sr = Plano(nombre, padre, sprite, centro, orden);
        Horno.Reanudar();
        Horno.Marcar(sr.gameObject, id);
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.size = tam;
        return sr;
    }

    // Sombra + animación + orden por profundidad para un personaje.
    public static AnimadorPersonaje Animar(GameObject go, SpriteRenderer sr, SpritesPersonaje sprites)
    {
        Horno.Suspender();
        AnimadorPersonaje an = go.AddComponent<AnimadorPersonaje>();
        an.Iniciar(sprites, sr);
        OrdenPorY o = go.AddComponent<OrdenPorY>();
        o.dinamico = true;
        SpriteRenderer sombra = Plano("Sombra", go.transform, Arte.Sombra(), (Vector2)go.transform.position + new Vector2(0f, -0.72f), -900);
        sombra.color = UnityEngine.Color.white;
        Horno.Reanudar();
        return an;
    }

    // ---------- UI por código ----------
    public static RectTransform Panel(Transform padre, string nombre, Color c, Vector2 anclaMin, Vector2 anclaMax, Vector2 offMin, Vector2 offMax)
    {
        GameObject go = new GameObject(nombre);
        go.transform.SetParent(padre, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anclaMin;
        rt.anchorMax = anclaMax;
        rt.offsetMin = offMin;
        rt.offsetMax = offMax;
        Image img = go.AddComponent<Image>();
        img.color = c;
        img.raycastTarget = false;
        return rt;
    }

    // Panel con borde pixelado (9-slice).
    public static RectTransform PanelArte(Transform padre, string nombre, Color fondo, Color borde, Vector2 anclaMin, Vector2 anclaMax, Vector2 offMin, Vector2 offMax)
    {
        RectTransform rt = Panel(padre, nombre, UnityEngine.Color.white, anclaMin, anclaMax, offMin, offMax);
        Image img = rt.GetComponent<Image>();
        img.sprite = Arte.Panel(fondo, borde);
        img.type = Image.Type.Sliced;
        img.pixelsPerUnitMultiplier = 0.3f;
        return rt;
    }

    public static RectTransform PanelArteCentro(Transform padre, string nombre, Color fondo, Color borde, Vector2 pos, Vector2 tam)
    {
        RectTransform rt = PanelArte(padre, nombre, fondo, borde, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        rt.sizeDelta = tam;
        rt.anchoredPosition = pos;
        return rt;
    }

    public static Image Imagen(Transform padre, string nombre, Sprite sprite, Vector2 anclaMin, Vector2 anclaMax, Vector2 offMin, Vector2 offMax)
    {
        RectTransform rt = Panel(padre, nombre, UnityEngine.Color.white, anclaMin, anclaMax, offMin, offMax);
        Image img = rt.GetComponent<Image>();
        img.sprite = sprite;
        return img;
    }

    public static RectTransform PanelCentro(Transform padre, string nombre, Color c, Vector2 pos, Vector2 tam)
    {
        RectTransform rt = Panel(padre, nombre, c, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        rt.sizeDelta = tam;
        rt.anchoredPosition = pos;
        return rt;
    }

    public static Text TextoUI(Transform padre, string texto, int tam, Color c, TextAnchor alineacion,
                               Vector2 anclaMin, Vector2 anclaMax, Vector2 offMin, Vector2 offMax)
    {
        GameObject go = new GameObject("Texto");
        go.transform.SetParent(padre, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anclaMin;
        rt.anchorMax = anclaMax;
        rt.offsetMin = offMin;
        rt.offsetMax = offMax;
        Text t = go.AddComponent<Text>();
        t.font = Fuente;
        t.text = texto;
        t.fontSize = tam;
        t.color = c;
        t.alignment = alineacion;
        t.supportRichText = true;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;
        return t;
    }

    // Texto que ocupa todo su padre con un margen.
    public static Text TextoRelleno(Transform padre, string texto, int tam, Color c, TextAnchor alineacion, float margen)
    {
        return TextoUI(padre, texto, tam, c, alineacion, Vector2.zero, Vector2.one,
                       new Vector2(margen, margen), new Vector2(-margen, -margen));
    }

    public static string Pintar(string texto, string hex)
    {
        return "<color=" + hex + ">" + texto + "</color>";
    }
}
