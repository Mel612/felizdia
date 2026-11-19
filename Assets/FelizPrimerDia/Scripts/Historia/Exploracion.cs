using System.Collections.Generic;
using UnityEngine;

// =====================================================================
//  SALAS PARA EXPLORAR: otras carreras de la sede La Paz.
//  Cada sala tiene un guía que explica la carrera y un objeto para revisar
//  (algunos dan pistas nuevas). Para agregar otra sala: añade una entrada aquí
//  con el id de la sala de Mapa.cs.
// =====================================================================
public static class Exploracion
{
    public class Sala
    {
        public string id, carrera;
        public string guia;
        public Apariencia apariencia;
        public Vector2 posGuia;
        public string[] explicacion;
        public string objeto;            // texto del botón "Revisar ..."
        public Vector2 posObjeto;
        public string[] textoObjeto;
        public string pista;             // recuerdo que se aprende al revisar (o null)
    }

    public static readonly List<Sala> Salas = new List<Sala>();
    static readonly HashSet<string> ids = new HashSet<string>();

    public static bool EsExplorable(string id) { return ids.Contains(id); }

    public static Sala Buscar(string id)
    {
        foreach (Sala s in Salas) if (s.id == id) return s;
        return null;
    }

    static void Agregar(string id, string carrera, string guia, Apariencia ap, Vector2 posGuia, string[] explicacion,
                        string objeto, Vector2 posObjeto, string[] textoObjeto, string pista)
    {
        Sala s = new Sala();
        s.id = id; s.carrera = carrera; s.guia = guia; s.apariencia = ap; s.posGuia = posGuia;
        s.explicacion = explicacion; s.objeto = objeto; s.posObjeto = posObjeto; s.textoObjeto = textoObjeto; s.pista = pista;
        Salas.Add(s);
        ids.Add(id);
    }

    static Exploracion()
    {
        // ---------- PLANTA BAJA ----------
        Agregar("biblioteca", "Biblioteca", "Doña Rosa (Biblioteca)",
            Arte.Ap("#6d597a", "#c08f63", "#8d8d8d", "#3d405b", true, Accesorio.Lentes, null), new Vector2(-4f, 1.5f),
            new[] {
                "¡Shhh! Bienvenida a la Biblioteca. Aquí prestamos libros, tenemos salas de estudio y computadoras para investigar.",
                "Todas las carreras pasan por aquí: los de Derecho por sus códigos, los de Medicina por sus atlas de anatomía...",
                "Hoy está tranquilo. Todos están en la Inducción... o escondiéndose." },
            "Revisar el anuario de egresados", new Vector2(0f, -0.2f),
            new[] {
                "\"Anuario de egresados 2025\". Fotos de los graduados del año pasado.",
                "En primera fila, sonriendo: SEBASTIÁN QUISPE. Mi hermano.",
                "Alguien rayó su cara con bolígrafo rojo. Tantas veces que el papel está roto." },
            "anuario");

        // ---------- PISO 1 ----------
        Agregar("psicologia", "Psicología", "Paola (Psicología)",
            Arte.Ap("#3b3550", "#d4a373", "#3d2b1f", "#22223b", true, Accesorio.Ninguno, null), new Vector2(3.5f, -1.5f),
            new[] {
                "Esta es la Cámara Gesell de PSICOLOGÍA. Ese espejo es una ventana por el otro lado.",
                "Aquí practicamos entrevistas y observamos el comportamiento sin interrumpir.",
                "En la carrera estudias cómo pensamos, sentimos y actuamos: psicología clínica, educativa, organizacional..." },
            "Mirar el espejo", new Vector2(0f, 2.6f),
            new[] {
                "Te miras en el espejo de la Cámara Gesell.",
                "Tu reflejo tarda medio segundo en moverse.",
                "...Y cuando parpadeas, él no." },
            null);

        Agregar("administracion", "Administración de Empresas", "Marcos (Administración)",
            Arte.Ap("#1d3557", "#a47148", "#111111", "#2b2d42", false, Accesorio.Ninguno, null), new Vector2(4f, -2.5f),
            new[] {
                "Bienvenida a ADMINISTRACIÓN DE EMPRESAS. Aquí aprendes a dirigir organizaciones.",
                "Planificación, finanzas, marketing, talento humano... y mucho emprendimiento.",
                "De hecho, nosotros llevamos las cuentas de la Bienvenida de esta noche." },
            "Revisar el presupuesto de la Bienvenida", new Vector2(-2f, 0.9f),
            new[] {
                "\"Presupuesto — Bienvenida a los Cachimbos. Organizadora: Camila Rojas.\"",
                "\"Disfraces de Pepino: 5 unidades.\"",
                "Pero en la fiesta solo se van a disfrazar 4 personas... ¿Quién tiene el quinto?" },
            "presupuesto");

        // ---------- PISO 2 ----------
        Agregar("diseno", "Diseño Gráfico", "Nico (Diseño Gráfico)",
            Arte.Ap("#ff70a6", "#c68a5a", "#2b1d14", "#14213d", false, Accesorio.Gorra, "#3a86ff"), new Vector2(-4f, -2.5f),
            new[] {
                "¡Hola! Esto es DISEÑO GRÁFICO Y PRODUCCIÓN CROSSMEDIA.",
                "Hacemos identidad de marca, ilustración, animación y contenido para redes, video e impresos.",
                "Todos los afiches de la Bienvenida salieron de aquí. ¿Viste el Pepino? Quedó bien tenebroso, ¿no?" },
            "Mirar el afiche de la Bienvenida", new Vector2(4.5f, 2.6f),
            new[] {
                "El afiche oficial de la Bienvenida: un Pepino sonriente sobre fondo rojo.",
                "En la esquina, muy chiquito, la firma de quien lo encargó: \"C. Rojas\"." },
            "afiche");

        Agregar("hoteleria", "Hotelería y Turismo", "Gabriela (Hotelería y Turismo)",
            Arte.Ap("#2a9d8f", "#e0ac69", "#6f1d1b", "#1d3557", true, Accesorio.Bufanda, "#ffd166"), new Vector2(1f, 1.5f),
            new[] {
                "¡Bienvenida al hotel de práctica de HOTELERÍA Y TURISMO!",
                "Aprendemos a gestionar hoteles, restaurantes y servicios turísticos. Bolivia tiene muchísimo para mostrar.",
                "Y salimos de viaje de práctica: el Salar, el lago Titicaca, los Yungas..." },
            "Tocar el timbre de recepción", new Vector2(-2.5f, -0.2f),
            new[] {
                "Ding.",
                "Desde algún lugar del edificio, alguien toca otro timbre de vuelta.",
                "Ding. Ding. Ding." },
            null);

        Agregar("radio", "Periodismo (Radio)", "Joaquín (Radio)",
            Arte.Ap("#e76f51", "#8d5524", "#111111", "#264653", false, Accesorio.Ninguno, null), new Vector2(3.5f, 1.5f),
            new[] {
                "Esta es la cabina de radio de PERIODISMO. Aquí hacemos programas en vivo y podcasts.",
                "Se practica locución, entrevistas, edición de audio y producción.",
                "Ojo, que el micrófono está encendido... aunque no hay nadie transmitiendo." },
            "Escuchar el micrófono", new Vector2(-1.5f, -0.6f),
            new[] {
                "Te acercas al micrófono. Los parlantes hacen un zumbido.",
                "Una voz susurra: \"Valeria... ¿cuántas veces más?\"",
                "La luz roja de AL AIRE se apaga sola." },
            null);

        // ---------- PISO 3 ----------
        Agregar("odontologia", "Odontología", "Lucero (Odontología)",
            Arte.Ap("#4cc9f0", "#c68a5a", "#1b1414", "#4cc9f0", true, Accesorio.Bata, null), new Vector2(-4.5f, -2f),
            new[] {
                "Bienvenida a la clínica de práctica de ODONTOLOGÍA.",
                "Estudias anatomía, prevención y tratamientos de la boca y los dientes.",
                "Primero practicamos con simuladores; después atendemos pacientes reales con supervisión." },
            "Revisar el sillón dental", new Vector2(2.5f, -0.3f),
            new[] {
                "El torno del sillón está encendido. Zumba.",
                "Nadie lo está usando.",
                "En el apoyacabezas hay un pelo largo, negro... como el tuyo." },
            null);

        Agregar("bioquimica", "Bioquímica y Farmacia", "Ariel (Bioquímica y Farmacia)",
            Arte.Ap("#06d6a0", "#a47148", "#2b1d14", "#22223b", false, Accesorio.Bata, null), new Vector2(-4.5f, -2.5f),
            new[] {
                "Este es el laboratorio de BIOQUÍMICA Y FARMACIA.",
                "Estudiamos los medicamentos, los análisis clínicos y la química del cuerpo.",
                "Regla número uno: nunca toques un frasco que no tenga bien puesta la etiqueta." },
            "Revisar los frascos", new Vector2(2.5f, -2.4f),
            new[] {
                "Un frasco tiene la etiqueta torcida: \"ÁCIDO SULFÚRICO\".",
                "Está casi vacío. Alguien lo usó hace poco.",
                "En el mesón hay un guante de látex derretido." },
            null);

        // ---------- PISO 4 ----------
        Agregar("servidores", "Sala de Servidores (Sistemas)", "Iván (Técnico de redes)",
            Arte.Ap("#264653", "#b5835a", "#111111", "#1d3557", false, Accesorio.Lentes, null), new Vector2(-4.5f, -2.5f),
            new[] {
                "Esta es la sala de servidores. Aquí vive la red de toda la sede.",
                "Los de INGENIERÍA DE SISTEMAS practican aquí: redes, seguridad informática y administración de servidores.",
                "Hoy los monitores de las cámaras andan raros. Mira tú misma." },
            "Mirar los monitores de las cámaras", new Vector2(0f, -1f),
            new[] {
                "Los monitores muestran las cámaras de todos los pisos.",
                "En la del Piso 3, alguien con máscara de Pepino mira directo a la cámara.",
                "Levanta la mano... y te saluda." },
            null);

        Agregar("comercial", "Ingeniería Comercial", "Fernanda (Ingeniería Comercial)",
            Arte.Ap("#ffb703", "#d4a373", "#3d2b1f", "#3d405b", true, Accesorio.Ninguno, null), new Vector2(-4f, -2.5f),
            new[] {
                "¡Hola! Esto es INGENIERÍA COMERCIAL.",
                "Negocios, comercio exterior, finanzas, logística... cómo se mueven los productos y el dinero.",
                "Es como Administración, pero con más números y más ingeniería." },
            "Revisar el mapa de la pared", new Vector2(2f, 0.9f),
            new[] {
                "Un mapa de Bolivia con rutas de exportación marcadas con hilos.",
                "Alguien clavó una chinche roja sobre La Paz... justo en la Av. Héroes del Acre.",
                "Tiene escrito: \"Aquí empieza\"." },
            null);
    }
}
