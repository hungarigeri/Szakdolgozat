using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


public class TileManager : MonoBehaviour
{
    [SerializeField] private Tilemap interactableMap;

    [SerializeField] private Tile hiddenInteractableTile;

    [SerializeField] private Tile InterectedTile;

    void Start()
    {
        foreach (var position in interactableMap.cellBounds.allPositionsWithin)
        {
            TileBase tile = interactableMap.GetTile(position);
            if (tile != null && tile.name == "Interactable_visible")
                {
                     interactableMap.SetTile(position, hiddenInteractableTile);
                }
           
        }
    }
    public bool IsInteractable(Vector3Int position)
    {
        TileBase tileAtPosition = interactableMap.GetTile(position);

        if(tileAtPosition.name != null)
        {
            if (tileAtPosition.name == "Interactable_Hidden"){
           return true;
        }
        }
        return false;
    }

   public void SetInteracted(Vector3Int position)
    {
        interactableMap.SetTile(position, InterectedTile);
    }
}
