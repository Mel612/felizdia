using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
#endif

// Teclas que usa el juego. Funciona con el Input System nuevo (por defecto en Unity 6)
// y también con el Input Manager antiguo.
public enum Tecla
{
    Arriba, Abajo, Izquierda, Derecha,
    Interactuar,   // E
    Accion,        // Espacio
    Cuaderno,      // Tab
    Confirmar,     // Enter
    Correr,        // Shift
    N1, N2, N3, N4, N5, N6,
    Reiniciar,     // R
    Silencio,      // M (sonido)
    Saltar,        // Esc (saltar cinemática)
    Pausa,         // P (pausa: opciones, volver al menú)
    Mapa           // Q (mapa grande)
}

public static class Entrada
{
    public static bool Presionada(Tecla t) { return Revisar(t, false); }
    public static bool Bajada(Tecla t) { return Revisar(t, true); }

    public static Vector2 Movimiento()
    {
        float x = 0f, y = 0f;
        if (Presionada(Tecla.Derecha)) x += 1f;
        if (Presionada(Tecla.Izquierda)) x -= 1f;
        if (Presionada(Tecla.Arriba)) y += 1f;
        if (Presionada(Tecla.Abajo)) y -= 1f;
        Vector2 v = new Vector2(x, y);
        return v.sqrMagnitude > 1f ? v.normalized : v;
    }

    // Devuelve 1..5 si se presionó una tecla numérica este frame, si no 0.
    public static int NumeroBajado()
    {
        if (Bajada(Tecla.N1)) return 1;
        if (Bajada(Tecla.N2)) return 2;
        if (Bajada(Tecla.N3)) return 3;
        if (Bajada(Tecla.N4)) return 4;
        if (Bajada(Tecla.N5)) return 5;
        if (Bajada(Tecla.N6)) return 6;
        return 0;
    }

    // Flecha (o WASD) presionada este frame. Devuelve Arriba/Abajo/Izquierda/Derecha o -1.
    public static int DireccionBajada()
    {
        if (Bajada(Tecla.Arriba)) return (int)Tecla.Arriba;
        if (Bajada(Tecla.Abajo)) return (int)Tecla.Abajo;
        if (Bajada(Tecla.Izquierda)) return (int)Tecla.Izquierda;
        if (Bajada(Tecla.Derecha)) return (int)Tecla.Derecha;
        return -1;
    }

    public static bool Avanzar()
    {
        return Bajada(Tecla.Interactuar) || Bajada(Tecla.Accion) || Bajada(Tecla.Confirmar);
    }

#if ENABLE_INPUT_SYSTEM
    static bool B(KeyControl c, bool bajada)
    {
        if (c == null) return false;
        return bajada ? c.wasPressedThisFrame : c.isPressed;
    }

    static bool Revisar(Tecla t, bool bajada)
    {
        Keyboard k = Keyboard.current;
        if (k == null) return false;
        switch (t)
        {
            case Tecla.Arriba: return B(k.wKey, bajada) || B(k.upArrowKey, bajada);
            case Tecla.Abajo: return B(k.sKey, bajada) || B(k.downArrowKey, bajada);
            case Tecla.Izquierda: return B(k.aKey, bajada) || B(k.leftArrowKey, bajada);
            case Tecla.Derecha: return B(k.dKey, bajada) || B(k.rightArrowKey, bajada);
            case Tecla.Interactuar: return B(k.eKey, bajada);
            case Tecla.Accion: return B(k.spaceKey, bajada);
            case Tecla.Cuaderno: return B(k.tabKey, bajada);
            case Tecla.Confirmar: return B(k.enterKey, bajada) || B(k.numpadEnterKey, bajada);
            case Tecla.Correr: return B(k.leftShiftKey, bajada) || B(k.rightShiftKey, bajada);
            case Tecla.N1: return B(k.digit1Key, bajada) || B(k.numpad1Key, bajada);
            case Tecla.N2: return B(k.digit2Key, bajada) || B(k.numpad2Key, bajada);
            case Tecla.N3: return B(k.digit3Key, bajada) || B(k.numpad3Key, bajada);
            case Tecla.N4: return B(k.digit4Key, bajada) || B(k.numpad4Key, bajada);
            case Tecla.N5: return B(k.digit5Key, bajada) || B(k.numpad5Key, bajada);
            case Tecla.N6: return B(k.digit6Key, bajada) || B(k.numpad6Key, bajada);
            case Tecla.Reiniciar: return B(k.rKey, bajada);
            case Tecla.Silencio: return B(k.mKey, bajada);
            case Tecla.Saltar: return B(k.escapeKey, bajada);
            case Tecla.Pausa: return B(k.pKey, bajada);
            case Tecla.Mapa: return B(k.qKey, bajada);
        }
        return false;
    }
#else
    static bool K(KeyCode c, bool bajada)
    {
        return bajada ? Input.GetKeyDown(c) : Input.GetKey(c);
    }

    static bool Revisar(Tecla t, bool bajada)
    {
        switch (t)
        {
            case Tecla.Arriba: return K(KeyCode.W, bajada) || K(KeyCode.UpArrow, bajada);
            case Tecla.Abajo: return K(KeyCode.S, bajada) || K(KeyCode.DownArrow, bajada);
            case Tecla.Izquierda: return K(KeyCode.A, bajada) || K(KeyCode.LeftArrow, bajada);
            case Tecla.Derecha: return K(KeyCode.D, bajada) || K(KeyCode.RightArrow, bajada);
            case Tecla.Interactuar: return K(KeyCode.E, bajada);
            case Tecla.Accion: return K(KeyCode.Space, bajada);
            case Tecla.Cuaderno: return K(KeyCode.Tab, bajada);
            case Tecla.Confirmar: return K(KeyCode.Return, bajada) || K(KeyCode.KeypadEnter, bajada);
            case Tecla.Correr: return K(KeyCode.LeftShift, bajada) || K(KeyCode.RightShift, bajada);
            case Tecla.N1: return K(KeyCode.Alpha1, bajada) || K(KeyCode.Keypad1, bajada);
            case Tecla.N2: return K(KeyCode.Alpha2, bajada) || K(KeyCode.Keypad2, bajada);
            case Tecla.N3: return K(KeyCode.Alpha3, bajada) || K(KeyCode.Keypad3, bajada);
            case Tecla.N4: return K(KeyCode.Alpha4, bajada) || K(KeyCode.Keypad4, bajada);
            case Tecla.N5: return K(KeyCode.Alpha5, bajada) || K(KeyCode.Keypad5, bajada);
            case Tecla.N6: return K(KeyCode.Alpha6, bajada) || K(KeyCode.Keypad6, bajada);
            case Tecla.Reiniciar: return K(KeyCode.R, bajada);
            case Tecla.Silencio: return K(KeyCode.M, bajada);
            case Tecla.Saltar: return K(KeyCode.Escape, bajada);
            case Tecla.Pausa: return K(KeyCode.P, bajada);
            case Tecla.Mapa: return K(KeyCode.Q, bajada);
        }
        return false;
    }
#endif

    public static string Simbolo(int direccion)
    {
        switch ((Tecla)direccion)
        {
            case Tecla.Arriba: return "↑";
            case Tecla.Abajo: return "↓";
            case Tecla.Izquierda: return "←";
            case Tecla.Derecha: return "→";
        }
        return "?";
    }
}
