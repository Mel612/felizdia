using System.Collections.Generic;

// Textos del cuaderno de pistas. La clave es el id que se guarda en Memoria.Aprender(id).
// Si un recuerdo no está aquí, no aparece en el cuaderno (sirve para "recuerdos internos").
public static class Pistas
{
    public struct Pista
    {
        public string titulo;
        public string texto;
        public Pista(string t, string x) { titulo = t; texto = x; }
    }

    public static readonly Dictionary<string, Pista> Catalogo = new Dictionary<string, Pista>
    {
        { "escaleras", new Pista("Las escaleras",
            "Antes de las 9:00 alguien me empujó por el hueco de las escaleras. No debo subir sola. " +
            "¿Diego podría acompañarme? Don Teodoro, el guardia, tiene tarjetas del ascensor.") },

        { "med_maniqui", new Pista("Medicina: el maniquí de más",
            "Después del sello se corta la luz y aparece un maniquí que no estaba. Hay una camilla con sábana: " +
            "puedo esconderme y CONTENER LA RESPIRACIÓN (mantener ESPACIO) cuando se acerque.") },

        { "quemaduras", new Pista("PRUEBA: Historia clínica",
            "Hace un año, la noche de la Bienvenida, una estudiante llegó a la clínica con quemaduras graves en brazos y cuello. El nombre está tachado.") },

        { "der_estrado", new Pista("Derecho: el juez",
            "El 'juez' del juicio simulado es el Pepino. Cierra las puertas. Debajo del estrado me puedo esconder " +
            "si me muevo justo cuando no mira (seguir las flechas rápido).") },

        { "motivo", new Pista("PRUEBA: El expediente",
            "Denuncia por novatada en la Bienvenida del año pasado. Víctima: CAMILA ROJAS. Acusado: SEBASTIÁN QUISPE... mi hermano. La denuncia fue retirada.") },

        { "password", new Pista("El post-it",
            "Pegado al expediente había un post-it: 'Sistema U — clave: cachimbo2025'.") },

        { "per_reflector", new Pista("Periodismo: el reflector",
            "En pleno 'al aire' el teleprompter escribe MIRA ARRIBA. Tengo que apretar la tecla que aparezca AL INSTANTE.") },

        { "cicatriz", new Pista("PRUEBA: La grabación",
            "La cámara del set grabó al Pepino en las vigas. Se quitó un guante: tiene CICATRICES DE QUEMADURAS en la mano.") },

        { "sis_ups", new Pista("Sistemas: el UPS",
            "Si no desconecto el UPS a tiempo, el cable pelado me electrocuta. Orden correcto de los cables: ROJO, AZUL, AMARILLO, VERDE.") },

        { "usuario", new Pista("PRUEBA: Los registros",
            "En el sistema de la U: el usuario 'c.rojas' modificó la lista de la Bienvenida para ponerme en la ruta del Pepino.") },

        { "anuario", new Pista("El anuario",
            "En el anuario de egresados 2025 (Biblioteca), alguien rayó la cara de Sebastián con bolígrafo rojo hasta romper el papel.") },

        { "presupuesto", new Pista("Cinco disfraces",
            "Presupuesto de la Bienvenida (Administración, Piso 1): se compraron 5 disfraces de Pepino... pero solo 4 personas se disfrazan.") },

        { "afiche", new Pista("La firma del afiche",
            "El afiche de la Bienvenida (Diseño Gráfico, Piso 2) lo encargó \"C. Rojas\".") },

        { "vargas", new Pista("Ing. Vargas",
            "El docente de Sistemas me dijo: 'Hay cosas que esta universidad prefiere olvidar'. Él sabe algo.") },

        { "andrea_disfraz", new Pista("Andrea",
            "Andrea tiene un disfraz de Pepino en la mochila. ¿Es ella?") },

        { "andrea_inocente", new Pista("Andrea (otra vez)",
            "Andrea dice que el disfraz se lo DIO CAMILA para la fiesta. Alguien la quiere incriminar.") },

        { "diego_primo", new Pista("Diego",
            "Diego es primo de Camila. Ella le pidió que me 'acompañara' a ciertos lugares... sin decirle por qué.") },

        { "teodoro_camaras", new Pista("Don Teodoro",
            "El guardia borró las cámaras porque Camila le dijo que eran de una broma de la Bienvenida.") },

        { "quien_es", new Pista("La Bienvenida",
            "En la fiesta hay CUATRO Pepinos. El de verdad tiene cicatrices en la mano. Sin la grabación de Periodismo, es adivinar.") },

        { "final_malo", new Pista("Error",
            "Acusé a Andrea frente a todos. Nadie me creyó y morí. Necesito las 4 PRUEBAS para que me crean.") },

        { "camila_1", new Pista("Camila (1)",
            "Le pregunté a Camila por las quemaduras. Se tapó el cuello con la bufanda y cambió de tema. Le temblaban las manos.") },

        { "camila_2", new Pista("Camila (2)",
            "Le hablé del expediente. Me dijo: 'Tu hermano tiene tus mismos ojos'. Retiró la denuncia porque nadie le creyó.") },

        { "camila_3", new Pista("Camila (3)",
            "Le conté de la grabación. Se quitó un guante: las cicatrices son reales. 'Lo grabaron riéndose mientras me quemaba.'") },

        { "camila_4", new Pista("Camila (4)",
            "Le dije que vi los registros. No lo negó: 'Solo quería que alguien de su familia sintiera miedo un día. Uno solo.'") },

        // ---------- Grabaciones de Camila (coleccionables) ----------
        { "audio_1", new Pista("Grabación 1/8: Primer día",
            "(Patio) \"Hola, yo del futuro. Hoy es mi primer día en Unifranz. Quiero estudiar Periodismo. Estoy nerviosa pero feliz. No te olvides de esta sensación, ¿ya?\"") },
        { "audio_2", new Pista("Grabación 2/8: Sebastián",
            "(Biblioteca) \"Hay un chico de último año, Sebastián Quispe. Todos lo quieren. Me invitó a la Bienvenida... dice que a los cachimbos nos tienen una 'sorpresa'.\"") },
        { "audio_3", new Pista("Grabación 3/8: La noche",
            "(Psicología) (Voz rota) \"Me echaron alcohol en el pelo, 'para el bautizo'. Alguien sacó un encendedor. Todos tenían el celular en la mano. Nadie me ayudó.\"") },
        { "audio_4", new Pista("Grabación 4/8: Retiré la denuncia",
            "(Administración) \"La U dijo que no había pruebas. Las cámaras 'no grababan'. Me dijeron que era mi palabra contra la suya. Hoy la retiré.\"") },
        { "audio_5", new Pista("Grabación 5/8: 8 de noviembre",
            "(Radio) \"Hoy fui al Cementerio General con mi abuela, por la Fiesta de las Ñatitas. Dice que las ñatitas cumplen favores si les llevas flores, coca y velas. " +
            "Le pedí uno a la ñatita de Odontología (Piso 3).\"") },
        { "audio_6", new Pista("Grabación 6/8: Funcionó",
            "(Bioquímica) \"Le pedí que alguien de la familia Quispe sintiera mi miedo. Un día. Y hoy... el día se repitió. Su hermanita vino a la U. Valeria. Y el día no termina.\"") },
        { "audio_7", new Pista("Grabación 7/8: Yo también recuerdo",
            "(Servidores) \"Recuerdo cada vez que la mato. CADA VEZ. Ya no quiero. Pero si me detengo, el día vuelve a empezar igual. La ñatita no me deja ir.\"") },
        { "audio_8", new Pista("Grabación 8/8: Ella se rió",
            "(Ing. Comercial) \"Sebastián mandó el video al grupo de su familia. Lo vi en su celular. Su hermanita respondió con un emoji de risa. Valeria. Ella también se rió.\"") },

        // ---------- La ñatita y el secreto de Valeria ----------
        { "secreto", new Pista("Mi secreto",
            "Hace un año Sebastián mandó un video al grupo de la familia: una 'broma' de la Bienvenida. Yo tenía 17. " +
            "Lo vi... y respondí con un emoji de risa. Nunca pregunté qué pasó después.") },
        { "natita", new Pista("La ñatita de Odontología",
            "En Odontología (Piso 3) hay una ñatita: un cráneo con chulo, lentes oscuros y flores secas. Para que te escuche hay que llevarle una OFRENDA en el mismo día: " +
            "FLORES (jardinera del Patio), HOJAS DE COCA (Don Teodoro) y una VELA (Hotelería, Piso 2).") },
        { "natita_ofrenda", new Pista("Lo que dijo la ñatita",
            "\"El favor se pidió con rabia, y la rabia no se cansa. Para cerrar el día tienen que pedirlo LAS DOS: la que se quemó y la que se rió.\" " +
            "Necesito las 4 pruebas, las 4 charlas con Camila, conocer mi secreto... y llevar la ofrenda ese mismo día.") },
        { "camila_recuerda", new Pista("Camila también recuerda",
            "Camila lo admitió: recuerda cada bucle, cada vez que me mata. Está tan atrapada como yo.") },

        // ---------- Historia de Mateo ----------
        { "m_vargas", new Pista("La clave de Vargas",
            "El Ing. Vargas me dio la clave del servidor de respaldos (Piso 4). Hay 4 videos de la Bienvenida pasada escondidos en 4 computadoras: " +
            "Biblioteca (PB), Radio (Piso 2), Bioquímica (Piso 3) y Servidores (Piso 4).") },
        { "m_aviso", new Pista("NO USES LAS ESCALERAS",
            "La advertencia del directorio la escribí yo, en un bucle anterior. Siempre fui yo.") },
        { "m_archivo_1", new Pista("Video 1: La fiesta",
            "Celular de un cachimbo: la Bienvenida pasada empieza. Camila llega sonriendo, con su vestido amarillo.") },
        { "m_archivo_2", new Pista("Video 2: El encendedor",
            "Otro celular: un chico de último año se acerca a Camila con un encendedor. Está de espaldas. Todos se ríen.") },
        { "m_archivo_3", new Pista("Video 3: Historia clínica digital",
            "Sistemas + Medicina: en la base de datos de la clínica estaba el nombre que tacharon en papel. CAMILA ROJAS. Acompañante: nadie.") },
        { "m_archivo_4", new Pista("Video 4: La cara",
            "El último video: el chico del encendedor se quita la máscara y se ríe mirando a la cámara. Es SEBASTIÁN QUISPE.") },
        { "m_quinto", new Pista("El quinto disfraz",
            "El quinto disfraz de Pepino desapareció del depósito. Lo tiene Sebastián: vuelve a la U para borrar las grabaciones... y a quien las tenga.") },
        { "m_aliados", new Pista("Aliados",
            "Para que la verdad salga necesito ayuda: JOAQUÍN (Radio) transmite si le muestro 2 videos; PAOLA (Psicología) acompaña a Camila si le muestro la historia clínica; " +
            "DON TEODORO me da la llave de la cabina del auditorio si le muestro el video 4.") },

        // ---------- El cuarto secreto ----------
        { "cuarto_secreto", new Pista("El cuarto de monitoreo",
            "Detrás de una puerta sin letrero en Seguridad está el viejo cuarto de monitoreo. Don Teodoro me dio la llave. " +
            "Dentro: cierra las puertas (A / D) cuando un disfraz llegue, mira las cámaras (W y 1-6) y no gastes toda la energía. Sobrevive hasta las 6 AM.") },
        { "camara_vieja", new Pista("La cinta roja",
            "En el cuarto de monitoreo quedó congelada una imagen: el Pepino de verdad se quita un guante y lleva una CINTA ROJA en la muñeca.") },

        // ---------- Misiones ----------
        { "misiones", new Pista("Indumentaria",
            "Cada laboratorio pide su ropa: TOGA para Derecho (Doña Rosa, Biblioteca), CREDENCIAL DE PRENSA para Periodismo (Joaquín, Radio), " +
            "BATA Y BARBIJO para Medicina (Ariel, Bioquímica) y TARJETA para Sistemas (Iván, Servidores). Cada uno pide un favor a cambio.") },

        // ---------- El Pepino aprende / eventos ----------
        { "pepino_aprende", new Pista("El Pepino aprende",
            "El Pepino recuerda dónde me escondí en otros días. No debo usar el mismo casillero dos veces.") },
        { "simulacro", new Pista("El simulacro",
            "Cuando suena la alarma del simulacro, todos salen del edificio. Si me quedo arriba, me quedo sola con el Pepino. Hay que bajar a la Planta Baja.") },
    };

    public static bool Existe(string id) { return Catalogo.ContainsKey(id); }
}
