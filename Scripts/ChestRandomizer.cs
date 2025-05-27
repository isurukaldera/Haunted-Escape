using UnityEngine;

public class ChestRandomizer : MonoBehaviour
{
public GameObject chest; // Drag your chest GameObject here in the inspector
public Transform[] spawnPoints; // Drag your spawn points here

void Start()
{
// Choose a random spawn point
int randomIndex = Random.Range(0, spawnPoints.Length);

// Move the chest to that point
chest.transform.position = spawnPoints[randomIndex].position;
chest.transform.rotation = spawnPoints[randomIndex].rotation;
}
}