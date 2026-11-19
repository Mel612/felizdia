using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// =====================================================================
//  ESCENA 10_Pantallas: todas las pantallas del juego dibujadas una al lado
//  de la otra, como "cuadros" en el mundo (Canvas en World Space).
//  Se ven en la pestaña Scene (doble clic en un cuadro de la Hierarchy y F
//  para acercarte) y también con Play (flechas: mover · E/Espacio: zoom).
//  Son dibujos sin lógica: el juego de verdad las arma solo.
//  Para cambiar cómo se ve una pantalla, cambia su archivo (MenuInicio.cs,
//  Ajustes.cs, MiniMapa.cs, Creditos.cs...) y vuelve a Construir escenas.
// =====================================================================
public static class Pantallas
{
    const int Columnas = 4;
    const float AnchoCuadro = 21f, AltoCuadro = 13.5f, Escala = 0.01f;   // 1920 px = 19.2 unidades

    public static Vector2 Centro { get { return new Vector2((Columnas - 1) * AnchoCuadro / 2f, -AltoCuadro / 2f); } }
    public const float Zoom = 24f;

    public static GameObject Construir()
    {
        GameObject raiz = new GameObject("Pantallas del juego (dibujadas)");

        // Un juego "de mentira" para el mapa (necesita el plano de los pisos)
        GameObject tmp = new GameObject("tmp");
        Juego j = tmp.AddComponent<Juego>();
        j.tipoEscena = TipoEscena.Piso;
        j.piso = 0;
        j.FijarHora(10.25f);
        Mapa mapa = tmp.AddComponent<Mapa>();
        mapa.PrepararPlano();
        j.mapa = mapa;

        try
        {
            int n = 0;
            MenuInicio.ArmarEn(Cuadro(raiz, n++, "1 · MENÚ DE INICIO  (escena 00_Menu)", true));
            PantallaTexto(Cuadro(raiz, n++, "2 · PERSONAJE Y DIFICULTAD  (Nueva partida)", true), "DIFICULTAD", TextoDificultad(), "Presiona 1, 2, 3 o 4");
            PanelPausa.Crear(Cuadro(raiz, n++, "3 · PAUSA  (tecla P)", false), true, "");
            PanelOpciones.Crear(Cuadro(raiz, n++, "4 · OPCIONES  (volumen, brillo, cámara)", false));
            PanelEspacios.Crear(Cuadro(raiz, n++, "5 · GUARDAR PARTIDA  (máquina de escribir o P)", false), "GUARDAR PARTIDA");
            MiniMapa.ArmarEn(Cuadro(raiz, n++, "6 · MAPA  (tecla Q)", true), j, Mapa.Origen(0) + new Vector2(-6f, -1.5f));
            Creditos.Crear(Cuadro(raiz, n++, "7 · CRÉDITOS  (suben por la pantalla)", true)).Desplazar(1050f);
            PantallaTexto(Cuadro(raiz, n++, "8 · LOGROS  (menú de inicio)", true), "LOGROS", "<size=24>" + Logros.Texto() + "</size>", "E para volver");
        }
        finally
        {
            if (Application.isPlaying) Object.Destroy(tmp); else Object.DestroyImmediate(tmp);
        }

        // Quitar la lógica: solo quedan los dibujos
        List<MonoBehaviour> quitar = new List<MonoBehaviour>();
        foreach (MonoBehaviour mb in raiz.transform.GetComponentsInChildren<MonoBehaviour>(true))
            if (mb is PanelPausa || mb is PanelOpciones || mb is PanelEspacios || mb is Creditos || mb is MiniMapa || mb is MenuInicio)
                quitar.Add(mb);
        foreach (MonoBehaviour mb in quitar)
            if (Application.isPlaying) Object.Destroy(mb); else Object.DestroyImmediate(mb);
        return raiz;
    }

    // Un cuadro de 1920x1080 con su título arriba. Devuelve el área de la pantalla.
    static Transform Cuadro(GameObject raiz, int i, string titulo, bool fondoOscuro)
    {
        GameObject c = new GameObject(titulo, typeof(RectTransform));
        c.transform.SetParent(raiz.transform, false);
        Canvas cv = c.AddComponent<Canvas>();
        cv.renderMode = RenderMode.WorldSpace;
        RectTransform rt = c.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(1920f, 1200f);
        rt.localScale = new Vector3(Escala, Escala, 1f);
        rt.position = new Vector3((i % Columnas) * AnchoCuadro, -(i / Columnas) * AltoCuadro, 0f);

        Text t = Util.TextoUI(c.transform, titulo, 64, Util.Hex("#ffd166"), TextAnchor.MiddleLeft, new Vector2(0, 1), new Vector2(1, 1), new Vector2(10, -110), new Vector2(0, 0));
        t.fontStyle = FontStyle.Bold;

        RectTransform area = Util.Panel(c.transform, "Pantalla", fondoOscuro ? Util.Hex("#07060b") : new Color(0.12f, 0.16f, 0.22f, 1f),
                                        Vector2.zero, Vector2.one, Vector2.zero, new Vector2(0, -120));
        area.gameObject.AddComponent<RectMask2D>();
        // Borde
        Util.Panel(c.transform, "Borde arriba", Util.Hex("#ffd166"), new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -124), new Vector2(0, -118));
        return area;
    }

    // Pantalla de texto grande (como las de elegir dificultad)
    static void PantallaTexto(Transform padre, string titulo, string cuerpo, string pie)
    {
        Vector2 cero = Vector2.zero, uno = Vector2.one;
        Text tt = Util.TextoUI(padre, titulo, 72, Color.white, TextAnchor.UpperCenter, new Vector2(0, 0.82f), new Vector2(1, 0.97f), cero, cero);
        tt.fontStyle = FontStyle.Bold;
        Util.TextoUI(padre, cuerpo, 34, Color.white, TextAnchor.UpperCenter, new Vector2(0.08f, 0.12f), new Vector2(0.92f, 0.8f), cero, cero);
        Util.TextoUI(padre, pie, 28, Util.Hex("#ffd166"), TextAnchor.LowerCenter, new Vector2(0, 0.03f), new Vector2(1, 0.1f), cero, cero);
    }

    public static string TextoDificultad()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        for (int i = 0; i < 4; i++)
            sb.Append("<color=").Append(Dificultad.Colores[i]).Append("><b>").Append(i + 1).Append(".  ").Append(Dificultad.Nombres[i])
              .Append("</b></color>\n<size=24><color=#bbbbbb>").Append(Dificultad.Descripciones[i]).Append("</color></size>\n\n");
        return sb.ToString();
    }
}
