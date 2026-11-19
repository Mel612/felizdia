using UnityEngine;

// Arte de los objetos de la historia extra: grabaciones de Camila, la ñatita y la vela.
public static partial class Arte
{
    // Celular olvidado en el piso (las grabaciones de Camila). Tiene la funda roja de Camila.
    public static Sprite Celular()
    {
        Sprite s;
        if (EnCache("celular", out s)) return s;
        Lienzo L = new Lienzo(10, 8);
        Color funda = Col.H("#e63946"), pant = Col.H("#1b2a41"), brillo = Col.H("#4cc9f0");
        L.Elipse(4.5f, 1f, 4.5f, 1.2f, new Color(0f, 0f, 0f, 0.35f));    // sombra
        L.R(1, 1, 8, 6, funda);
        L.R(2, 2, 7, 5, pant);
        L.R(3, 4, 6, 4, brillo);                                          // onda de audio
        L.P(3, 3, brillo); L.P(5, 3, brillo); L.P(4, 5, brillo); L.P(6, 5, brillo);
        L.P(7, 5, Col.H("#ff3b3b"));                                      // punto de "grabando"
        L.Borde(Col.H("#120d16"));
        return Crear("celular", L, PivotPies);
    }

    // La ñatita: un cráneo sobre un pedestal, con chulo, lentes oscuros, flores y velas.
    public static Sprite Natita()
    {
        Sprite s;
        if (EnCache("natita", out s)) return s;
        Lienzo L = new Lienzo(22, 30);
        Color madera = Col.H("#6b4a2f"), maderaO = Col.H("#4a3220"), hueso = Col.H("#efe6cf"), huesoO = Col.H("#c9bc99");
        Color negro = Col.H("#14110f");
        // Pedestal / vitrina
        L.R(1, 0, 20, 11, maderaO);
        L.R(2, 1, 19, 10, madera);
        L.R(2, 10, 19, 11, Col.H("#8d6748"));
        L.R(5, 4, 16, 6, maderaO);                                        // cajón
        // Velas
        L.R(2, 12, 3, 16, Col.H("#fff3c4")); L.P(2, 17, Col.H("#ffb703")); L.P(2, 18, Col.H("#ff7b00"));
        L.R(18, 12, 19, 15, Col.H("#fff3c4")); L.P(19, 16, Col.H("#ffb703")); L.P(19, 17, Col.H("#ff7b00"));
        // Cráneo
        L.Elipse(10.5f, 18f, 5.5f, 5f, hueso);
        L.R(7, 12, 14, 14, hueso);                                        // mandíbula
        L.R(8, 12, 13, 12, huesoO);
        for (int x = 8; x <= 13; x += 2) L.P(x, 13, negro);              // dientes
        L.R(10, 15, 11, 16, huesoO);                                      // nariz
        // Lentes oscuros
        L.R(6, 17, 9, 19, negro); L.R(12, 17, 15, 19, negro); L.R(9, 18, 12, 18, negro);
        L.P(7, 19, Col.H("#4a4a4a")); L.P(13, 19, Col.H("#4a4a4a"));
        // Chulo andino de colores
        Color[] franjas = { Col.H("#e63946"), Col.H("#ffb703"), Col.H("#2a9d8f"), Col.H("#8338ec") };
        for (int y = 22; y <= 27; y++)
        {
            int ancho = 6 - (y - 22) / 2;
            L.R(10 - ancho, y, 11 + ancho, y, franjas[(y - 22) % 4]);
        }
        L.P(10, 28, Col.H("#e63946")); L.P(11, 28, Col.H("#e63946"));
        L.R(4, 20, 5, 23, franjas[1]); L.R(16, 20, 17, 23, franjas[2]);   // orejeras
        // Flores secas
        L.Circulo(5f, 12.5f, 1.3f, Col.H("#ff70a6")); L.Circulo(16f, 12.5f, 1.3f, Col.H("#ffd166"));
        L.P(5, 12, Col.H("#6a040f")); L.P(16, 12, Col.H("#e76f51"));
        L.Borde(Col.H("#120d16"));
        return Crear("natita", L, PivotPies);
    }

    // Vela del centro de mesa (Hotelería).
    public static Sprite Vela()
    {
        Sprite s;
        if (EnCache("vela", out s)) return s;
        Lienzo L = new Lienzo(6, 11);
        L.R(0, 0, 5, 1, Col.H("#b08968"));                                // platito
        L.R(1, 2, 4, 7, Col.H("#fff3c4"));
        L.R(1, 2, 1, 7, Col.H("#e9d8a6"));
        L.P(2, 8, Col.H("#3d2b1f"));
        L.R(2, 9, 3, 9, Col.H("#ffb703")); L.P(2, 10, Col.H("#ff7b00"));
        L.Borde(Col.H("#120d16"));
        return Crear("vela", L, PivotPies);
    }

    // ---------- Personajes de las cinemáticas (mismas apariencias que en el juego) ----------
    public static SpritesPersonaje Camila() { return Personaje(Ap("#ffb703", "#e0ac69", "#6f1d1b", "#3d405b", true, Accesorio.Bufanda, "#e63946")); }
    public static SpritesPersonaje Andrea() { return Personaje(Ap("#6a040f", "#d4a373", "#111111", "#343a40", true, Accesorio.Mochila, "#ff70a6")); }
    public static SpritesPersonaje Teodoro() { return Personaje(Ap("#264653", "#a47148", "#555555", "#1d3557", false, Accesorio.Gorra, "#1d3557")); }
    public static SpritesPersonaje Guardia() { return Personaje(Ap("#1d3557", "#8d5524", "#111111", "#1d3557", false, Accesorio.Gorra, "#111111")); }
    public static SpritesPersonaje Diego() { return Personaje(Ap("#e76f51", "#c68a5a", "#2b1d14", "#264653", false, Accesorio.Mochila, "#2a9d8f")); }

    // Máscara de Pepino suelta (en el piso, en la mochila...)
    public static Sprite Mascara()
    {
        Sprite s;
        if (EnCache("mascara", out s)) return s;
        Lienzo L = new Lienzo(14, 16);
        Color verde = Col.H("#7bd389"), verdeO = Col.H("#40916c");
        L.Elipse(6.5f, 8f, 6f, 7.4f, verdeO);
        L.Elipse(6.5f, 8.4f, 5.2f, 6.6f, verde);
        L.R(3, 9, 4, 11, Col.H("#14110f")); L.R(9, 9, 10, 11, Col.H("#14110f"));        // ojos
        L.P(4, 11, Color.white); L.P(10, 11, Color.white);
        for (int x = 3; x <= 10; x++) L.P(x, 5 - (x == 3 || x == 10 ? -1 : 0), Col.H("#d62828"));  // sonrisa
        L.R(4, 4, 9, 4, Col.H("#d62828"));
        L.P(1, 12, Col.H("#ffd166")); L.P(12, 12, Col.H("#ffd166"));                      // rombos
        L.Borde(Col.H("#120d16"));
        return Crear("mascara", L, PivotPies);
    }

    // Cama vista de lado (para los dormitorios)
    public static Sprite CamaLado(string colchaHex)
    {
        Sprite s;
        string k = "cama_lado" + colchaHex;
        if (EnCache(k, out s)) return s;
        Lienzo L = new Lienzo(48, 22);
        Color madera = Col.H("#6d4c35"), maderaO = Col.H("#4a3220"), colcha = Col.H(colchaHex);
        L.R(0, 0, 2, 21, maderaO);                    // cabecera
        L.R(45, 0, 47, 12, maderaO);                  // pie
        L.R(2, 3, 45, 7, madera);
        L.R(3, 8, 44, 11, Col.H("#f8f9fa"));          // colchón
        L.R(3, 10, 12, 14, Col.H("#ffffff"));         // almohada
        L.R(12, 9, 44, 13, colcha);
        L.R(12, 9, 44, 9, Col.Oscuro(colcha, 0.7f));
        L.Borde(Col.H("#120d16"));
        return Crear(k, L, PivotPies);
    }

    public static Sprite Mochila()
    {
        Sprite s;
        if (EnCache("mochila", out s)) return s;
        Lienzo L = new Lienzo(16, 18);
        Color c = Col.H("#7b2cbf"), o = Col.H("#5a189a");
        L.R(1, 0, 14, 14, o);
        L.R(2, 1, 13, 13, c);
        L.R(4, 3, 11, 7, o);                          // bolsillo
        L.R(5, 14, 10, 17, o);                        // asa
        for (int x = 6; x <= 9; x++) for (int y = 15; y <= 16; y++) L.Borrar(x, y);
        L.Borde(Col.H("#120d16"));
        return Crear("mochila", L, PivotPies);
    }

    public static Sprite Ramo()
    {
        Sprite s;
        if (EnCache("ramo", out s)) return s;
        Lienzo L = new Lienzo(14, 14);
        L.R(5, 0, 8, 6, Col.H("#2d6a4f"));
        L.R(3, 3, 10, 5, Col.H("#f1faee"));           // papel
        L.Circulo(4f, 9f, 2.4f, Col.H("#e63946")); L.Circulo(9.5f, 9.5f, 2.4f, Col.H("#ffd166"));
        L.Circulo(7f, 11.5f, 2.2f, Col.H("#f8f9fa")); L.Circulo(6.5f, 8f, 1.6f, Col.H("#ff70a6"));
        L.Borde(Col.H("#120d16"));
        return Crear("ramo", L, PivotPies);
    }

    // ---------- Mateo (nivel extra) ----------
    // Cachimbo de Ing. de Sistemas: polera negra, lentes, pelo corto.
    public static SpritesPersonaje Mateo() { return Personaje(Ap("#1b1b1f", "#a47148", "#111111", "#3d405b", false, Accesorio.Lentes, null)); }
    // Sebastián Quispe, el hermano de Valeria
    public static SpritesPersonaje Sebastian() { return Personaje(Ap("#3a0ca3", "#c68a5a", "#1b1b1b", "#14213d", false, Accesorio.Ninguno, null)); }
    // Valeria un año después, estudiante de Medicina
    public static SpritesPersonaje ValeriaMedicina() { return Personaje(Ap("#7b2cbf", "#c68a5a", "#1b1414", "#2b2d42", true, Accesorio.Bata, null)); }
    public static SpritesPersonaje IngVargas() { return Personaje(Ap("#495057", "#c08f63", "#6c757d", "#212529", false, Accesorio.Lentes, null)); }

    // Laptop con stickers (Mateo)
    public static Sprite Laptop()
    {
        Sprite s;
        if (EnCache("laptop", out s)) return s;
        Lienzo L = new Lienzo(16, 12);
        L.R(1, 0, 14, 1, Col.H("#6c757d"));            // base
        L.R(2, 2, 13, 11, Col.H("#343a40"));           // tapa
        L.R(3, 3, 12, 10, Col.H("#1f6f4a"));           // pantalla
        for (int y = 4; y < 10; y += 2) L.R(4, y, 4 + (y * 3) % 7 + 2, y, Col.H("#7bd389"));
        L.P(3, 10, Col.H("#ff70a6")); L.P(12, 3, Col.H("#ffd166"));
        L.Borde(Col.H("#120d16"));
        return Crear("laptop", L, PivotPies);
    }
}
