using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// =====================================================================
//  MISIONES DE INDUMENTARIA
//  Cada facultad pide una indumentaria para entrar a su laboratorio.
//  Se consigue con una misión corta: un guía de otra carrera te la presta
//  si le llevas algo. Las indumentarias son del DÍA (se pierden al morir),
//  pero si ya hiciste la misión una vez, en los siguientes bucles basta con
//  decirle al guía lo que ya sabes.
// =====================================================================
public static class Misiones
{
    public class Mision
    {
        public string id;              // toga, prensa, bata, tarjeta
        public string sello;           // sello que la pide
        public string prenda;          // "Toga y corbata"
        public string facultad;        // "Derecho (Piso 1)"
        public string guiaSala;        // sala del guía que la presta (Exploracion)
        public string guiaNombre;
        public string objeto;          // lo que pide el guía
        public string dondeObjeto;     // dónde se consigue
        public string[] pedido;        // lo que dice el guía al pedirlo
        public string[] entrega;       // lo que dice al entregarla
    }

    public static readonly List<Mision> Lista = new List<Mision>
    {
        new Mision {
            id = "toga", sello = Ids.SelloDerecho, prenda = "Toga y corbata", facultad = "Derecho (Piso 1)",
            guiaSala = "biblioteca", guiaNombre = "Doña Rosa",
            objeto = "el Código Penal", dondeObjeto = "la Cafetería (Planta Baja)",
            pedido = new[] {
                "¿Una toga para el juicio simulado? Las guardo aquí, de las graduaciones.",
                "Pero primero devuélveme algo: alguien dejó un CÓDIGO PENAL de la biblioteca en la Cafetería. Tráemelo y te presto la toga." },
            entrega = new[] { "¡Mi Código Penal! Gracias, hija. Toma: toga y corbata. Que no se te arrugue." } },

        new Mision {
            id = "prensa", sello = Ids.SelloPeriodismo, prenda = "Credencial de prensa", facultad = "Periodismo (Piso 2)",
            guiaSala = "radio", guiaNombre = "Joaquín",
            objeto = "un café", dondeObjeto = "la Cafetería (Planta Baja)",
            pedido = new[] {
                "¿Credencial de prensa para entrar al set? Yo las imprimo.",
                "Pero estoy en vivo desde las seis de la mañana... Tráeme UN CAFÉ de la Cafetería y es tuya." },
            entrega = new[] { "¡Café! Me salvaste el programa. Aquí tienes tu credencial de PRENSA. Cuélgatela bien visible." } },

        new Mision {
            id = "bata", sello = Ids.SelloMedicina, prenda = "Bata y barbijo", facultad = "Medicina (Piso 3)",
            guiaSala = "bioquimica", guiaNombre = "Ariel",
            objeto = "el Manual de Bioseguridad", dondeObjeto = "la Biblioteca (Planta Baja), con Doña Rosa",
            pedido = new[] {
                "Sin bata y barbijo nadie entra a Simulación. Bioseguridad, cachimba.",
                "Te presto los míos si me traes el MANUAL DE BIOSEGURIDAD de la Biblioteca. Doña Rosa lo tiene." },
            entrega = new[] { "Perfecto. Bata, barbijo... y lávate las manos antes de entrar, ¿ya?" } },

        new Mision {
            id = "tarjeta", sello = Ids.SelloSistemas, prenda = "Tarjeta del laboratorio", facultad = "Ing. de Sistemas (Piso 4)",
            guiaSala = "servidores", guiaNombre = "Iván",
            objeto = "una memoria USB", dondeObjeto = "Diseño Gráfico (Piso 2)",
            pedido = new[] {
                "El laboratorio tiene cerradura con tarjeta. Te activo una, pero necesito un favor.",
                "Olvidé mi MEMORIA USB en Diseño Gráfico, en el Piso 2. Tiene el respaldo de los servidores. Tráemela." },
            entrega = new[] { "¡Mi USB! Toma, tu tarjeta ya está activada. Pásala por el lector de la puerta." } },
    };

    public static Mision DeSello(string sello) { foreach (Mision m in Lista) if (m.sello == sello) return m; return null; }
    public static Mision DeGuia(string sala) { foreach (Mision m in Lista) if (m.guiaSala == sala) return m; return null; }

    public static string Prenda(Mision m) { return "ind_" + m.id; }
    public static string Activa(Mision m) { return "mis_" + m.id; }
    public static string Objeto(Mision m) { return "obj_" + m.id; }
    public static string Hecha(Mision m) { return "mision_" + m.id + "_hecha"; }

    public static bool TienePrenda(string sello)
    {
        Mision m = DeSello(sello);
        return m == null || Memoria.Tiene(Prenda(m));
    }

    public static int PrendasHoy()
    {
        int n = 0;
        foreach (Mision m in Lista) if (Memoria.Tiene(Prenda(m))) n++;
        return n;
    }

    // Texto para el pasaporte
    public static string Resumen()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder("<b>ROPA</b>  ");
        string[] cortos = { "Toga", "Prensa", "Bata", "Tarjeta" };
        for (int i = 0; i < Lista.Count; i++)
        {
            bool ok = Memoria.Tiene(Prenda(Lista[i]));
            bool activa = !ok && Memoria.Tiene(Activa(Lista[i]));
            sb.Append(ok ? "<color=#7bd389>" : activa ? "<color=#ffd166>" : "<color=#777777>").Append(cortos[i]).Append("</color> ");
        }
        return sb.ToString();
    }

    // Lo que dice el guía de la facultad si llegas sin la indumentaria
    public static IEnumerator Rechazo(Dialogo D, string guia, string sello)
    {
        Mision m = DeSello(sello);
        yield return D.DecirCo(guia,
            "¡Alto! Sin " + m.prenda.ToUpper() + " no puedes entrar al laboratorio de " + m.facultad + ". Reglas de la U.",
            m.guiaNombre + " te puede prestar una. Búscalo en " + NombreSala(m.guiaSala) + ".");
        if (Memoria.Aprender("misiones"))
            Juego.I.ui.Toast("Nueva pista: " + Pistas.Catalogo["misiones"].titulo, 3.5f);
    }

    public static string NombreSala(string sala)
    {
        switch (sala)
        {
            case "biblioteca": return "la Biblioteca (Planta Baja)";
            case "radio": return "la Cabina de Radio (Piso 2)";
            case "bioquimica": return "Bioquímica y Farmacia (Piso 3)";
            case "servidores": return "la Sala de Servidores (Piso 4)";
        }
        return sala;
    }

    // Opciones extra cuando hablas con un guía que presta indumentaria.
    // Devuelve true si la conversación ya se resolvió aquí.
    public static IEnumerator HablarGuia(Dialogo D, Exploracion.Sala ex, Resultado<bool> resuelto)
    {
        resuelto.valor = false;
        Mision m = DeGuia(ex.id);
        // Doña Rosa además presta el Manual de Bioseguridad (misión de la bata)
        Mision bata = DeSello(Ids.SelloMedicina);
        bool pideManual = ex.id == "biblioteca" && Memoria.Tiene(Activa(bata)) && !Memoria.Tiene(Objeto(bata)) && !Memoria.Tiene(Prenda(bata));

        List<string> ops = new List<string>();
        List<int> ids = new List<int>();
        ops.Add("Preguntar por la carrera"); ids.Add(0);
        if (m != null && !Memoria.Tiene(Prenda(m)))
        {
            if (Memoria.Tiene(Objeto(m))) { ops.Add("Entregar " + m.objeto); ids.Add(2); }
            else if (Memoria.Sabe(Hecha(m))) { ops.Add("\"Ya sé: necesitas " + m.objeto + ". Confía en mí.\""); ids.Add(3); }
            else { ops.Add("Pedir: " + m.prenda); ids.Add(1); }
        }
        if (pideManual) { ops.Add("Pedir el Manual de Bioseguridad"); ids.Add(4); }
        if (ops.Count == 1) yield break;
        ops.Add("Nada"); ids.Add(5);

        string nombre = ex.guia.Contains("(") ? ex.guia.Substring(0, ex.guia.IndexOf('(')).Trim() : ex.guia;
        Resultado<int> r = new Resultado<int>();
        yield return D.ElegirCo(nombre, "¿Qué necesitas, cachimba?", ops.ToArray(), r);
        int op = ids[r.valor];
        if (op == 0) yield break;                 // sigue la charla normal de la carrera
        resuelto.valor = true;
        UIJuego ui = Juego.I.ui;
        switch (op)
        {
            case 1:
                yield return D.DecirCo(nombre, m.pedido);
                Memoria.Obtener(Activa(m));
                ui.Toast("MISIÓN: lleva " + m.objeto + " a " + m.guiaNombre + "  (está en " + m.dondeObjeto + ")", 5f);
                break;
            case 2:
                yield return D.DecirCo(nombre, m.entrega);
                Entregar(m);
                break;
            case 3:
                yield return D.DecirCo(nombre, "¿Cómo sabías que...? Bueno, se nota que eres de confiar. Toma: " + m.prenda.ToLower() + ".");
                Entregar(m);
                break;
            case 4:
                yield return D.DecirCo(nombre, "¿El Manual de Bioseguridad? Para Ariel, seguro. Siempre lo pierde. Toma, y que me lo devuelva.");
                Memoria.Obtener(Objeto(bata));
                ui.Toast("Tienes el MANUAL DE BIOSEGURIDAD. Llévaselo a Ariel (Piso 3).", 4f);
                break;
        }
    }

    static void Entregar(Mision m)
    {
        Memoria.Obtener(Prenda(m));
        Memoria.Quitar(Objeto(m));
        Memoria.Aprender(Hecha(m));
        Sonido.Efecto("sello", 0.7f, 1.2f);
        Juego.I.ui.Toast("INDUMENTARIA: " + m.prenda.ToUpper() + "  ·  ya puedes entrar a " + m.facultad, 4.5f);
        if (PrendasHoy() >= 4) Logros.Desbloquear("indumentaria");
    }
}
