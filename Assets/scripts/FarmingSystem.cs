using UnityEngine;

public class FarmingSystem : MonoBehaviour
{

    void Update()
    {
        

        if (Input.GetKeyDown(KeyCode.E)) // E gomb vetéshez
        {
            Vector3Int position = new Vector3Int((int)transform.position.x, (int)transform.position.y, 0);

            if (GameManager.Instance.tileManager.IsInteractable(position))
            {
                GameManager.Instance.tileManager.SetInteracted(position);
            }
        }
    }


}