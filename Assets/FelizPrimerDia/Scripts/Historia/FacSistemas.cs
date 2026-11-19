using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 💻 INGENIERÍA DE SISTEMAS — Laboratorio de Computación (Piso 4)
// Minijuego: Compila o muere · Terror: tu foto en todas las pantallas + apagón
// Salvarse: desconectar el UPS en el orden correcto · Prueba 4: los registros (hackeo).
public class FacSistemas : Facultad
{
    protected override string SelloId { get { return Ids.SelloSistemas; } }
    protected override string NombreCarrera { get { return "Ing. de Sistemas"; } }

    Personaje guia, vargas;
    SpriteRenderer ups;
    readonly List<SpriteRenderer> pantallas = new List<SpriteRenderer>();
    bool ventanaAbierta, tocoUps;

    protected override void Montar()
    {
        // Filas de computadoras
        for (int fila = 0; fila < 2; fila++)
            for (int i = 0; i < 5; i++)
            {
                Vector2 b = sala.P(-5f + i * 2.4f, 1.0f - fila * 2.6f);
                pantallas.Add(Obj(Arte.Escritorio(true, "#4cc9f0"), b, new Vector2(1.8f, 0.6f)));
                Obj(Arte.Silla("#2a9d8f"), b + new Vector2(0f, -0.75f));
            }
        Obj(Arte.Rack(), sala.P(10.5f, 2.2f), new Vector2(1.4f, 0.6f));
        Obj(Arte.Rack(), sala.P(11.3f, -3.9f), new Vector2(1.4f, 0.6f));
        ups = Obj(Arte.UPS(), sala.P(8.2f, 2.9f));
        ups.color = Color.white;
        foreach (SpriteRenderer p in pantallas) p.sprite = Arte.Escritorio(true, "#4cc9f0");
        Util.Texto("UPS", transform, sala.P(8.2f, 4.0f), 0.6f, Util.Hex("#2b2433"), 0);
        Obj(Arte.Escritorio(true, "#7bd389"), sala.P(-9.5f, 2.6f), new Vector2(1.8f, 0.6f));
        Util.Texto("Terminal", transform, sala.P(-9.5f, 4.0f), 0.6f, Util.Hex("#2b2433"), 0);
        Util.Plano("Pizarra", transform, Arte.Pizarra(), sala.P(-2f, 3.9f), -1480);

        guia = Personaje.Crear(transform, "Bruno (Sistemas)", sala.P(-9f, -1.5f),
            Arte.Personaje(Arte.Ap("#2a9d8f", "#b5835a", "#111111", "#1d3557", false, Accesorio.Lentes, null)),
            delegate { Lanzar(HablarGuia()); });

        vargas = Personaje.Crear(transform, "Ing. Ramiro Vargas", sala.P(-6.6f, 2.6f),
            Arte.Personaje(Arte.Ap("#495057", "#c08f63", "#6c757d", "#212529", false, Accesorio.Lentes, null)),
            delegate { Lanzar(HablarVargas()); });

        Interactuable iUps = Interactuable.Crear(transform, sala.P(8.2f, 2.3f), "Desconectar el UPS", delegate { tocoUps = true; }, 1.6f);
        iUps.disponible = delegate { return ventanaAbierta; };

        Interactuable terminal = Interactuable.Crear(transform, sala.P(-9.5f, 1.8f), "Usar la terminal",
            delegate { Lanzar(Terminal()); }, 1.5f);
        terminal.disponible = delegate { return !Memoria.Tiene(Ids.PruebaRegistros) && !enEvento; };
    }

    IEnumerator HablarGuia()
    {
        string[] explicacion =
        {
            "¡Hola! Este es el Laboratorio de Computación de INGENIERÍA DE SISTEMAS.",
            "Aquí programamos, armamos redes, administramos servidores y bases de datos, y desarrollamos software.",
            "De hecho, el sistema de inscripciones de la U lo mantienen estudiantes y docentes de aquí.",
            "Tu prueba: este programa en C++ está desordenado. Ordena las líneas (1-5) para que compile. ¡Rápido!"
        };
        yield return FlujoGuia("Bruno", explicacion, delegate
        {
            MJOrdenar mj = Minijuego.Crear<MJOrdenar>("COMPILA O MUERE", "Ordena las líneas de código para que el programa compile.");
            mj.ordenCorrecto.Add("int main() {");
            mj.ordenCorrecto.Add("    string nombre = \"Valeria\";");
            mj.ordenCorrecto.Add("    cout << \"Hola, \" << nombre;");
            mj.ordenCorrecto.Add("    return 0;");
            mj.ordenCorrecto.Add("}");
            mj.pista = "Pista: primero se abre la función, al final se cierra la llave.";
            mj.tiempoLimite = 30f;
            mj.maxErrores = 2;
            mj.textoExito = "COMPILACIÓN EXITOSA";
            mj.textoFallo = "ERROR DE COMPILACIÓN";
            mj.tituloIzquierda = "LÍNEAS";
            mj.tituloDerecha = "main.cpp";
            return mj;
        }, "dom_sistemas");
    }

    IEnumerator HablarVargas()
    {
        if (Memoria.Tiene(Ids.PruebaRegistros))
        {
            yield return D.DecirCo("Ing. Vargas",
                "...Viste los registros. Entonces ya sabes.",
                "Yo los encontré hace una semana. Camila me pidió silencio. Me dijo que si hablaba, la próxima sería yo.",
                "Hay cosas que esta universidad prefiere olvidar, Quispe. Tú no las olvides.");
        }
        else
        {
            yield return D.DecirCo("Ing. Vargas",
                "Inducción, ¿no? Mira, mira. No toques mi terminal.",
                "...Y un consejo: hay cosas que esta universidad prefiere olvidar.");
        }
        Memoria.Aprender("vargas");
    }

    protected override IEnumerator EventoTerror()
    {
        juego.Bloquear(this);
        foreach (SpriteRenderer p in pantallas) p.sprite = Arte.Escritorio(true, "#d00000");
        UI.Golpe(0.35f);
        yield return Decir("Valeria",
            "Todas las pantallas se encienden solas con la misma imagen:",
            "MI FOTO DE INSCRIPCIÓN. Con una X roja encima.",
            "Las impresoras empiezan a imprimir mi nombre. Una y otra y otra vez.");
        yield return Decir("Bruno", "¿Es una broma de la Bienvenida? Voy a ver quién es...");
        guia.Mostrar(false);
        vargas.Mostrar(false);
        yield return Decir("Valeria", "El Ing. Vargas tampoco está. Su café sigue caliente.");

        UI.Oscuridad(0.85f);
        UI.Toast("Se fue la luz...", 2f);
        yield return Esperar(1.2f);
        SpriteRenderer pepino = Figura("Pepino", sala.P(-2f, 2.6f), Util.SpritePepino());
        UI.Oscuridad(0.65f);
        juego.Desbloquear(this);

        UI.Toast(Memoria.Sabe("sis_ups")
            ? "¡El UPS (arriba a la derecha)! Desconéctalo en orden: ROJO, AZUL, AMARILLO, VERDE."
            : "El UPS del rincón parpadea... y huele a cable quemado.", 5f);

        tocoUps = false;
        ventanaAbierta = true;
        StartCoroutine(ParpadeoUps());
        yield return Ventana(6f, delegate { return tocoUps; });
        ventanaAbierta = false;
        if (juego.Terminado) yield break;

        if (!tocoUps)
        {
            yield return Abalanzarse(pepino);
            Morir("Te empujó contra el rack. Un cable pelado.\nChispas, humo... y tu cara parpadeando en todas las pantallas.", "sis_ups", TipoMuerte.Electrocucion);
            yield break;
        }

        MJOrdenar mj = Minijuego.Crear<MJOrdenar>("¡DESCONECTA EL UPS!", "Desconecta los cables en el orden correcto. Si te equivocas, la descarga te mata.");
        mj.ordenCorrecto.Add("Cable ROJO");
        mj.ordenCorrecto.Add("Cable AZUL");
        mj.ordenCorrecto.Add("Cable AMARILLO");
        mj.ordenCorrecto.Add("Cable VERDE");
        mj.coloresHex = new List<string> { "#e63946", "#3a86ff", "#ffd60a", "#2dc653" };
        mj.pista = Memoria.Sabe("sis_ups") ? "Lo anotaste: ROJO, AZUL, AMARILLO, VERDE." : "¿En qué orden...? No hay tiempo para pensar.";
        mj.tiempoLimite = 10f;
        mj.maxErrores = 0;
        mj.textoExito = "SIN CORRIENTE";
        mj.textoFallo = "¡DESCARGA!";
        mj.tituloIzquierda = "CABLES";
        mj.tituloDerecha = "DESCONECTADOS";
        Resultado<bool> ok = new Resultado<bool>();
        yield return mj.Jugar(ok);

        if (!ok.valor)
        {
            yield return Abalanzarse(pepino);
            Morir("Desconectaste el cable equivocado. La descarga te lanzó contra el rack.\nEl Pepino solo miraba.", "sis_ups", TipoMuerte.Electrocucion);
            yield break;
        }

        Destroy(pepino.gameObject);
        foreach (SpriteRenderer p in pantallas) p.sprite = Arte.Escritorio(true, "#4cc9f0");
        UI.Oscuridad(0f);
        sobrevivio = true;
        Memoria.Aprender("sis_ups");
        yield return Decir("Valeria",
            "El cable pelado quedó sin corriente. El Pepino me buscó a ciegas y se fue por la puerta trasera.",
            "La terminal del docente sigue encendida...");
    }

    IEnumerator ParpadeoUps()
    {
        while (ventanaAbierta)
        {
            ups.color = Util.Hex("#ff4d4d");
            yield return new WaitForSeconds(0.2f);
            ups.color = Color.white;
            yield return new WaitForSeconds(0.2f);
        }
        ups.color = Color.white;
    }

    IEnumerator Terminal()
    {
        if (!sobrevivio)
        {
            yield return Decir("Terminal", "\"Sesión del Ing. Vargas. Bloqueada.\"");
            yield break;
        }
        bool sabeClave = Memoria.Sabe("password");
        MJPreguntas mj = Minijuego.Crear<MJPreguntas>("HACKEO", "Entra al sistema de la U y encuentra quién te puso en la ruta del Pepino.");
        mj.maxErrores = 1;
        mj.etiquetaRonda = "Paso";
        mj.textoExito = "ACCESO CONCEDIDO";
        mj.textoFallo = "ACCESO DENEGADO";
        mj.AgregarRonda("> ¿Qué comando muestra los archivos de la Bienvenida?", 1,
            "rm -rf /", "ls /bienvenida", "shutdown now", "cd ..");
        if (sabeClave)
            mj.AgregarRonda("> Usuario: admin_bienvenida\n> Contraseña:", 2, "123456", "unifranz", "cachimbo2025", "password");
        else
            mj.AgregarRonda("> Usuario: admin_bienvenida\n> Contraseña:", 99, "123456", "unifranz", "password", "admin");
        mj.AgregarRonda("> ¿Qué registro quieres revisar?", 2,
            "wifi_conexiones.log", "fotos_de_gatitos/", "cambios_lista_bienvenida.log", "notas_2018.txt");
        mj.AgregarRonda("> ¿Restaurar respaldo de cámaras?", 0,
            "restore --camaras --fecha=hoy", "delete --camaras", "format C:", "exit");
        Resultado<bool> ok = new Resultado<bool>();
        yield return mj.Jugar(ok);
        if (!ok.valor)
        {
            if (!sabeClave) yield return Decir("Valeria", "No sé la contraseña... ¿Alguien la habrá anotado en algún lado?");
            else yield return Decir("Valeria", "Me equivoqué de comando. Otra vez, con calma.");
            yield break;
        }
        yield return Decir("Terminal", "cambios_lista_bienvenida.log  ·  respaldo de cámaras restaurado.");
        yield return Recuerdos.Reproducir("usuario");
        yield return Decir("Valeria", "c.rojas... Camila Rojas. La delegada. La que organiza la Bienvenida.");
        Memoria.Obtener(Ids.PruebaRegistros);
        Memoria.Aprender("usuario");
        UI.Toast("PRUEBA OBTENIDA: Los registros  (" + Memoria.Cuantos(Ids.Pruebas) + "/4)", 4f);
    }
}
