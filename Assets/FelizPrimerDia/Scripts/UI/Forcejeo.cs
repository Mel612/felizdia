using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// =====================================================================
//  FORCEJEO: cuando el Pepino te atrapa en un pasillo.
//  "¡Presiona E rápido!" — cada E llena la barra, el Pepino la vacía.
//  Si la llenas antes de que acabe el tiempo, te sueltas y lo dejas aturdido.
//  Si no, empieza la persecución de lado (como antes).
// =====================================================================
public class Forcejeo : MonoBehaviour
{
    public static IEnumerator Jugar(Juego j, Transform pepino, Resultado<bool> r)
    {
        r.valor = false;
        if (j == null || j.ui == null) yield break;
        j.Bloquear("forcejeo");
        GameObject go = new GameObject("Forcejeo", typeof(RectTransform));
        go.transform.SetParent(j.ui.transform, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = rt.offsetMax = Vector2.zero;
        Util.Panel(go.transform, "Rojo", new Color(0.35f, 0f, 0f, 0.35f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        Vector2 cero = Vector2.zero;
        Text titulo = Util.TextoUI(go.transform, "¡TE ATRAPÓ!", 96, Util.Hex("#ff4d4d"), TextAnchor.MiddleCenter, new Vector2(0, 0.6f), new Vector2(1, 0.78f), cero, cero);
        titulo.fontStyle = FontStyle.Bold;
        titulo.gameObject.AddComponent<Outline>().effectColor = Color.black;
        Text tecla = Util.TextoUI(go.transform, "Presiona  E  rápido", 48, Color.white, TextAnchor.MiddleCenter, new Vector2(0, 0.5f), new Vector2(1, 0.6f), cero, cero);
        tecla.gameObject.AddComponent<Outline>().effectColor = Color.black;

        RectTransform marco = Util.PanelArteCentro(go.transform, "Barra", new Color(0.05f, 0.03f, 0.05f, 0.95f), Util.Hex("#ffd166"), new Vector2(0, -60), new Vector2(820, 70));
        RectTransform relleno = Util.Panel(marco, "Fuerza", Util.Hex("#7bd389"), Vector2.zero, new Vector2(0.25f, 1f), new Vector2(8, 8), new Vector2(-4, -8));

        float fuerza = 0.25f;
        float tiempo = 3.2f * Dificultad.FactorTiempo;
        float baja = 0.32f / Mathf.Max(0.5f, Dificultad.FactorTiempo);   // el Pepino tira de vuelta
        float sube = 0.085f * Mathf.Clamp(Dificultad.FactorTiempo, 0.85f, 1.2f);
        float t = 0f;
        Sonido.Efecto("grito", 0.5f, 1.3f);
        Vector3 pepinoBase = pepino != null ? pepino.position : Vector3.zero;
        while (t < tiempo && fuerza < 1f)
        {
            t += Time.deltaTime;
            fuerza -= baja * Time.deltaTime;
            if (Entrada.Bajada(Tecla.Interactuar) || Entrada.Bajada(Tecla.Accion))
            {
                fuerza += sube;
                j.ui.Golpe(0.08f);
                Sonido.Efecto("golpe", 0.35f, Random.Range(0.9f, 1.2f));
                tecla.transform.localScale = Vector3.one * 1.15f;
            }
            tecla.transform.localScale = Vector3.Lerp(tecla.transform.localScale, Vector3.one, Time.deltaTime * 10f);
            fuerza = Mathf.Clamp01(fuerza);
            relleno.anchorMax = new Vector2(Mathf.Max(0.02f, fuerza), 1f);
            relleno.GetComponent<Image>().color = Color.Lerp(Util.Hex("#e63946"), Util.Hex("#7bd389"), fuerza);
            titulo.text = "¡TE ATRAPÓ!   " + Mathf.CeilToInt(tiempo - t);
            if (pepino != null) pepino.position = pepinoBase + (Vector3)(Random.insideUnitCircle * 0.06f);
            yield return null;
        }
        if (pepino != null) pepino.position = pepinoBase;
        r.valor = fuerza >= 1f;
        Sonido.Efecto(r.valor ? "golpe" : "susto", 0.7f, r.valor ? 0.7f : 1f);
        Destroy(go);
        j.Desbloquear("forcejeo");
    }
}
