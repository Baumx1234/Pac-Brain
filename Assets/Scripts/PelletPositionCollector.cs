using UnityEngine;
using System.Collections.Generic;

public class PelletPositionCollector : MonoBehaviour
{
    // Liste zur Speicherung der Positionen der Pellets
    private List<Vector3> pelletPositions = new List<Vector3>();

    // Methode zum Sammeln der Pellet-Positionen aus den Spielobjekten
    public void CollectPelletPositions()
    {
        pelletPositions.Clear();

        // Suchen Sie nach allen Spielobjekten mit dem Tag "Pellet" und erfassen Sie ihre Positionen
        GameObject[] pellets = GameObject.FindGameObjectsWithTag("Pellet");
        foreach (GameObject pellet in pellets)
        {
            pelletPositions.Add(pellet.transform.position);
        }
    }

    // Methode zum Abrufen der gesammelten Pellet-Positionen
    public List<Vector3> GetPelletPositions()
    {
        return pelletPositions;
    }
}