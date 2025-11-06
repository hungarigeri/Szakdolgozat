using UnityEngine;

public class FarmingSystem : MonoBehaviour
{
    public GameObject cropPrefab; // Inspector-ban húzd ide a Crop prefab-ot

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E)) // E gomb vetéshez
        {
            PlantCrop();
        }
    }

    void PlantCrop()
    {
        Vector3 plantPosition = transform.position; // Karakter pozíciója
        Instantiate(cropPrefab, plantPosition, Quaternion.identity); // Növény létrehozása
    }
}