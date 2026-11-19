using UnityEngine;

// Figuras de muestra para ver la escena en el editor (personajes, el Pepino...).
// Al darle Play se borran solas y el código crea los personajes de verdad
// (con sus diálogos, animaciones e inteligencia).
public class SoloEnEditor : MonoBehaviour
{
    void Awake()
    {
        gameObject.SetActive(false);
        Destroy(gameObject);
    }
}
