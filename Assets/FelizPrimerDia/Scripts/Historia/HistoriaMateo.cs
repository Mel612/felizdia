using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// =====================================================================
//  NIVEL EXTRA: LA HISTORIA DE MATEO CHOQUE ("El que miraba")
//  Cachimbo de Ing. de Sistemas. Vio morir a Valeria por las cámaras del
//  cuarto secreto y no hizo nada: ahora él también está en el bucle.
//
//  Objetivo de cada día (antes de las 18:00):
//    · Recuperar los 4 VIDEOS de la novatada en 4 computadoras.
//    · Conseguir 3 ALIADOS: Joaquín (Radio), Paola (Psicología), Don Teodoro.
//    · En la Bienvenida: subir a la cabina de luces y proyectar la verdad.
//  Habilidad: en cualquier computadora puede HACKEAR LAS CÁMARAS y ver
//  dónde está el quinto disfraz (Sebastián).
// =====================================================================
public partial class Historia
{
    public static readonly string[] ArchivosMateo = { "archivo_1", "archivo_2", "archivo_3", "archivo_4" };
    public static readonly string[] AliadosMateo = { "aliado_joaquin", "aliado_paola", "aliado_teodoro" };
    public static readonly string[] LugarArchivo = { "Biblioteca (PB)", "Cabina de Radio (Piso 2)", "Bioquímica (Piso 3)", "Sala de Servidores (Piso 4)" };

    void MontarMateo(Mapa m, Transform t, int piso)
    {
        Memoria.Obtener("tarjeta");                       // Mateo ayuda al Ing. Vargas: tiene tarjeta del ascensor
        if (piso == 0) MontarPlantaBajaMateo(m, t);

        Interactuable.Crear(t, Mapa.Escaleras(piso), "Usar las escaleras", delegate { Lanzar(UsarEscaleras(piso)); }, 1.6f);
        Interactuable.Crear(t, Mapa.Ascensor(piso), "Usar el ascensor", delegate { Lanzar(UsarAscensor(piso)); }, 1.4f);

        // Casilleros (mismo orden que en la historia de Valeria: son objetos dibujados) y el quinto disfraz
        if (piso > 0)
        {
            float[] xs = { -13f, 3f, 15f };
            foreach (float x in xs) CrearCasillero(t, Mapa.Origen(piso) + new Vector2(x, -2.1f));
            List<Vector2> ruta = new List<Vector2>
            {
                Mapa.Origen(piso) + new Vector2(-12f, 0.9f), Mapa.Origen(piso) + new Vector2(17f, 0.9f),
                Mapa.Origen(piso) + new Vector2(17f, -1.6f), Mapa.Origen(piso) + new Vector2(-12f, -1.6f)
            };
            Pepino pep = Pepino.Crear(t, ruta[1 + (piso % 2) * 2], ruta);
            pep.velocidadPatrulla = 1.7f + piso * 0.1f;
            pep.HacerQuinto();
            pepinoPiso = pep;
        }

        // Los guías de las carreras (Joaquín y Paola pueden ser aliados)
        foreach (Exploracion.Sala ex in Exploracion.Salas)
        {
            Sala s;
            if (m.Salas.TryGetValue(ex.id, out s)) MontarExploracion(t, s, ex);
        }

        // Computadoras: hackear cámaras y recuperar los videos
        Sala sa;
        if (EnEstePiso(m, "biblioteca", out sa)) Computadora(t, sa.P(2.2f, 2.3f), 0);
        if (EnEstePiso(m, "radio", out sa)) Computadora(t, sa.P(0.8f, -0.9f), 1);
        if (EnEstePiso(m, "bioquimica", out sa)) Computadora(t, sa.P(1f, 1.8f), 2);
        if (EnEstePiso(m, "servidores", out sa)) Computadora(t, sa.P(2.5f, 1.6f), 3);
        if (EnEstePiso(m, "sistemas", out sa))
        {
            Computadora(t, sa.P(-7f, -3f), -1);
            Personaje.Crear(t, "Ing. Vargas (Sistemas)", sa.P(-9.2f, -3f), Arte.IngVargas(), delegate { Lanzar(HablarVargas()); });
        }
        if (EnEstePiso(m, "seguridad", out sa)) Computadora(t, sa.P(-3f, 1.6f), -1);
    }

    void MontarPlantaBajaMateo(Mapa m, Transform t)
    {
        Sala ingreso = m.Salas["ingreso"];
        Interactuable dir = Interactuable.Crear(t, ingreso.Centro + new Vector2(-3.4f, 2.6f), "Leer el directorio", delegate { Lanzar(DirectorioMateo()); }, 1.4f);
        dir.textoDinamico = delegate { return Memoria.Tiene("aviso_escrito") ? "Leer el directorio" : "Escribir en el directorio"; };

        Sala seg = m.Salas["seguridad"];
        Personaje.Crear(t, "Don Teodoro (Guardia)", seg.Centro + new Vector2(0f, -1.4f), Arte.Teodoro(), delegate { Lanzar(TeodoroMateo()); });

        Sala aud = m.Salas["auditorio"];
        Interactuable puerta = Interactuable.Crear(t, aud.FrentePuerta, "Entrar al Auditorio", delegate { Lanzar(PuertaAuditorioMateo()); }, 1.6f);
        puerta.textoDinamico = delegate { return juego.Hora < juego.horaBienvenida ? "Auditorio (abre a las 18:00)" : "Entrar a la Bienvenida"; };
    }

    // ---------------------------------------------------------------
    //  PERSONAJES
    // ---------------------------------------------------------------
    IEnumerator HablarVargas()
    {
        if (!Memoria.Tiene("clave_vargas"))
        {
            yield return D.DecirCo("Ing. Vargas",
                "Choque, ¿no? El que me ayuda con los respaldos. Bienvenido a Ingeniería de Sistemas.",
                "Aquí aprendes a construir lo que mueve todo: redes, bases de datos, seguridad informática, programas.",
                "Las clínicas guardan las historias clínicas en sistemas. Los bancos, la plata. La U, sus cámaras. Todo pasa por nosotros.");
            if (Memoria.Aprender("carrera_sistemas_mateo"))
                UI.Toast("Aprendiste sobre Ingeniería de Sistemas", 3f);
            yield return D.DecirCo("Ing. Vargas",
                "Hace un año desaparecieron grabaciones de la Bienvenida. Hay cosas que esta universidad prefiere olvidar.",
                "Yo no. Toma la clave del servidor de respaldos. Úsala con cuidado: alguien más las está buscando.");
            Memoria.Obtener("clave_vargas");
            Memoria.Aprender("m_aliados");
            if (Memoria.Aprender("m_vargas")) UI.Toast("Nueva pista: " + Pistas.Catalogo["m_vargas"].titulo, 3.5f);
        }
        else yield return D.DecirCo("Ing. Vargas", "Cuatro videos, Choque. Uno en cada piso donde estaban los respaldos. Y no te quedes mirando.");
    }

    IEnumerator TeodoroMateo()
    {
        List<string> ops = new List<string>();
        List<int> ids = new List<int>();
        ops.Add("Saludar"); ids.Add(0);
        if (!Memoria.Tiene("llave_cabina")) { ops.Add("Pedir la llave de la cabina de luces del auditorio"); ids.Add(1); }
        ops.Add("Nada, gracias"); ids.Add(2);
        Resultado<int> r = new Resultado<int>();
        yield return D.ElegirCo("Don Teodoro", "¿Usted es el chico de las computadoras? El que ayuda al ingeniero.", ops.ToArray(), r);
        if (ids[r.valor] == 0)
            yield return D.DecirCo("Don Teodoro", "No se quede hasta tarde en el cuarto de abajo, joven. De noche ahí se ven cosas.");
        else if (ids[r.valor] == 1)
        {
            if (!Memoria.Tiene(ArchivosMateo[3]))
            {
                yield return D.DecirCo("Don Teodoro", "¿La cabina? No, joven. Esa llave no se presta. Solo si me da una buena razón.");
                yield break;
            }
            yield return D.DecirCo("Mateo", "(Le muestras el video 4 en la laptop: Sebastián Quispe sin máscara, riéndose.)");
            yield return D.DecirCo("Don Teodoro", "...Ese chico. Yo lo vi esa noche. Y borré las cámaras porque me dijeron que era una broma.",
                "Tome la llave. Esta vez no voy a mirar para otro lado.");
            Memoria.Obtener("llave_cabina");
            Memoria.Obtener(AliadosMateo[2]);
            UI.Toast("ALIADO: Don Teodoro  (" + Memoria.Cuantos(AliadosMateo) + "/3)  ·  Tienes la llave de la cabina", 4f);
            Sonido.Efecto("sello", 0.7f, 1.1f);
        }
    }

    // Joaquín y Paola: aliados de Mateo
    IEnumerator AliadoMateo(Exploracion.Sala ex, Resultado<bool> resuelto)
    {
        resuelto.valor = false;
        bool esJoaquin = ex.id == "radio" && !Memoria.Tiene(AliadosMateo[0]);
        bool esPaola = ex.id == "psicologia" && !Memoria.Tiene(AliadosMateo[1]);
        if (!esJoaquin && !esPaola) yield break;
        string nombre = esJoaquin ? "Joaquín" : "Paola";
        Resultado<int> r = new Resultado<int>();
        yield return D.ElegirCo(nombre, "¿Qué necesitas?", new[] { "Preguntar por la carrera", esJoaquin ? "Pedirle que transmita en vivo desde la Bienvenida" : "Pedirle ayuda para hablar con Camila", "Nada" }, r);
        if (r.valor != 1) { resuelto.valor = r.valor == 2; yield break; }
        resuelto.valor = true;
        if (esJoaquin)
        {
            if (Memoria.Cuantos(ArchivosMateo) < 2)
            {
                yield return D.DecirCo("Joaquín", "¿Transmitir qué? Sin pruebas es un rumor, y un rumor no sale al aire. Tráeme al menos dos videos.");
                yield break;
            }
            yield return D.DecirCo("Joaquín", "(Mira los videos en tu laptop. Se queda pálido.)", "Esto tiene que verlo todo La Paz. A las 18:00 conecto la radio a la cabina del auditorio. Cuenta conmigo.");
        }
        else
        {
            if (!Memoria.Tiene(ArchivosMateo[2]))
            {
                yield return D.DecirCo("Paola", "¿Camila? Para ayudarla necesito entender qué le pasó de verdad. ¿Tienes algo... un informe, una historia clínica?");
                yield break;
            }
            yield return D.DecirCo("Paola", "(Lee la historia clínica digital.) Quemaduras... y nadie la acompañó al hospital.",
                "Alguien que vivió algo así no necesita que la expongan. Necesita que alguien le crea. Yo voy contigo.");
        }
        Memoria.Obtener(esJoaquin ? AliadosMateo[0] : AliadosMateo[1]);
        UI.Toast("ALIADO: " + nombre + "  (" + Memoria.Cuantos(AliadosMateo) + "/3)", 3.5f);
        Sonido.Efecto("sello", 0.7f, 1.1f);
    }

    IEnumerator DirectorioMateo()
    {
        if (Memoria.Tiene("aviso_escrito"))
        {
            yield return D.DecirCo("Directorio", "<color=#ff4d4d>\"NO USES LAS ESCALERAS\"</color> — con mi letra.");
            yield break;
        }
        Resultado<int> r = new Resultado<int>();
        yield return D.ElegirCo("Mateo", "Valeria siempre sube sola por las escaleras antes de las 9... y ahí la empujan.",
            new[] { "Escribir con marcador: \"NO USES LAS ESCALERAS\"", "Mejor no" }, r);
        if (r.valor != 0) yield break;
        Memoria.Obtener("aviso_escrito");
        Sonido.Efecto("estatica", 0.3f, 2.4f);
        yield return D.DecirCo("Mateo", "...Un momento. Esta frase ya estaba escrita el primer día.", "Fui yo. Siempre fui yo.");
        if (Memoria.Aprender("m_aviso")) UI.Toast("Nueva pista: " + Pistas.Catalogo["m_aviso"].titulo, 3.5f);
        Logros.Desbloquear("aviso");
    }

    // ---------------------------------------------------------------
    //  COMPUTADORAS: hackear cámaras y recuperar videos
    // ---------------------------------------------------------------
    void Computadora(Transform t, Vector2 pos, int archivo)
    {
        int a = archivo;
        Horno.Suspender();
        Util.Objeto("Laptop de Mateo", t, Arte.Laptop(), pos + new Vector2(0f, -0.15f));
        Horno.Reanudar();
        Interactuable.Crear(t, pos, "Usar la computadora", delegate { Lanzar(UsarComputadora(a)); }, 1.2f);
    }

    IEnumerator UsarComputadora(int archivo)
    {
        List<string> ops = new List<string>();
        List<int> ids = new List<int>();
        ops.Add("Hackear las cámaras de este piso"); ids.Add(0);
        if (archivo >= 0 && !Memoria.Tiene(ArchivosMateo[archivo])) { ops.Add("Recuperar el video " + (archivo + 1)); ids.Add(1); }
        ops.Add("Salir"); ids.Add(2);
        Resultado<int> r = new Resultado<int>();
        yield return D.ElegirCo("Computadora", "<color=#2d6a4f>root@unifranz:~$</color>  " + (archivo >= 0 ? "respaldo_bienvenida_" + (archivo + 1) + ".mp4 (dañado)" : "listo"), ops.ToArray(), r);
        int op = ids[r.valor];
        if (op == 0)
        {
            Sonido.Efecto("click", 0.7f, 1.2f);
            yield return StartCoroutine(MapaHack.Mostrar(juego, juego.piso, 7f));
            juego.AvanzarReloj(0.05f);
        }
        else if (op == 1)
        {
            if (archivo == 3 && !Memoria.Tiene("clave_vargas"))
            {
                yield return D.DecirCo("Computadora", "<color=#ff4d4d>ACCESO DENEGADO.</color> Servidor de respaldos: se necesita la clave del administrador.", "(El Ing. Vargas, en el laboratorio del Piso 4, tiene la clave.)");
                yield break;
            }
            Minijuego mj = MinijuegoArchivo(archivo);
            Resultado<bool> ok = new Resultado<bool>();
            yield return mj.Jugar(ok);
            juego.AvanzarReloj(0.15f);
            if (!ok.valor) { yield return D.DecirCo("Mateo", "Se corrompió... Lo intento otra vez."); yield break; }
            Memoria.Obtener(ArchivosMateo[archivo]);
            UI.Toast("VIDEO RECUPERADO  " + Memoria.Cuantos(ArchivosMateo) + "/4", 3.5f);
            yield return StartCoroutine(Recuerdos.Reproducir("m_video_" + (archivo + 1)));
            Memoria.Aprender("m_archivo_" + (archivo + 1));
            if (archivo == 3 && Memoria.Aprender("m_quinto")) UI.Toast("Nueva pista: " + Pistas.Catalogo["m_quinto"].titulo, 4f);
        }
    }

    Minijuego MinijuegoArchivo(int archivo)
    {
        switch (archivo)
        {
            case 0:
            {
                MJPreguntas mj = Minijuego.Crear<MJPreguntas>("RECUPERAR VIDEO 1", "El archivo está dañado. Responde bien para repararlo.");
                mj.maxErrores = 1; mj.etiquetaRonda = "Sector"; mj.textoExito = "VIDEO REPARADO"; mj.textoFallo = "ARCHIVO CORRUPTO";
                mj.AgregarRonda("¿Qué es un 'respaldo' (backup)?", 2, "Un virus", "Una contraseña", "Una copia de seguridad de los datos", "Un cable de red");
                mj.AgregarRonda("¿Qué extensión tiene un archivo de video?", 0, ".mp4", ".exe", ".docx", ".sql");
                mj.AgregarRonda("Un byte tiene...", 1, "4 bits", "8 bits", "16 bits", "1000 bits");
                return mj;
            }
            case 1:
            {
                MJOrdenar mj = Minijuego.Crear<MJOrdenar>("RECUPERAR VIDEO 2", "El video llegó en paquetes desordenados. Ordena las capas de la red (de abajo hacia arriba).");
                mj.ordenCorrecto.Add("Física (cables y señales)");
                mj.ordenCorrecto.Add("Enlace (tarjeta de red)");
                mj.ordenCorrecto.Add("Red (direcciones IP)");
                mj.ordenCorrecto.Add("Transporte (TCP)");
                mj.ordenCorrecto.Add("Aplicación (el reproductor)");
                mj.pista = "Pista: primero lo físico, al final lo que ve el usuario.";
                return mj;
            }
            case 2:
            {
                MJPreguntas mj = Minijuego.Crear<MJPreguntas>("HISTORIA CLÍNICA DIGITAL",
                    "Sistemas + Medicina: busca en la base de datos de la clínica la historia clínica de la noche de la Bienvenida.");
                mj.maxErrores = 1; mj.etiquetaRonda = "Consulta"; mj.textoExito = "REGISTRO ENCONTRADO"; mj.textoFallo = "CONSULTA INVÁLIDA";
                mj.AgregarRonda("> ¿Qué comando SQL sirve para LEER datos?", 1, "DELETE", "SELECT", "DROP TABLE", "UPDATE");
                mj.AgregarRonda("> SELECT * FROM pacientes WHERE ...", 2, "nombre = rojo", "fecha < hospital", "fecha = '2025-02-07'", "* = *");
                mj.AgregarRonda("> El nombre está tachado. ¿Dónde más puede estar?", 0, "En la tabla de acompañantes y contactos", "En la papelera de reciclaje", "En el monitor", "En el teclado");
                return mj;
            }
            default:
            {
                MJSecuencia mj = Minijuego.Crear<MJSecuencia>("SERVIDOR DE RESPALDOS", "Inyecta los comandos con las flechas antes de que el sistema te bloquee.");
                mj.largo = 8;
                mj.ventana = 1.1f;
                mj.maxErrores = 1;
                mj.textoArriba = "sudo restaurar --video 4";
                mj.textoExito = "ACCESO CONCEDIDO";
                mj.textoFallo = "BLOQUEADO";
                return mj;
            }
        }
    }

    // ---------------------------------------------------------------
    IEnumerator PuertaAuditorioMateo()
    {
        if (juego.Hora < juego.horaBienvenida)
        {
            yield return D.DecirCo("Auditorio", "Cerrado hasta las 18:00. Detrás, alguien prueba las luces del escenario.");
            yield break;
        }
        yield return D.DecirCo("Mateo", "Ya empezó la Bienvenida. Valeria está adentro... y Sebastián también.");
        Partida.IrABienvenida();
    }
}
