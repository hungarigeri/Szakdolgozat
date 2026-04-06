using UnityEngine;

public class CraftingManager : MonoBehaviour
{
    public static CraftingManager instance;

    private void Awake()
    {
        instance = this;
    }

    // 1. ELLENŐRZÉS: Végignézi az inventory-t, hogy megvan-e minden a recepthez
    public bool CanCraft(CraftingRecipe recipe)
    {
        foreach (CraftingIngredient ingredient in recipe.ingredients)
        {
            // Lekérdezzük az InventoryManager-től, hogy van-e elég ebből az itemből
            if (InventoryManager.instance.GetItemCount(ingredient.item) < ingredient.count)
            {
                return false; // Ha csak egy dologból is hiány van, nem tudjuk legyártani!
            }
        }
        return true; // Minden szükséges anyag a zsebünkben van.
    }

    // 2. LEGYÁRTÁS: Kiveszi az anyagokat és beteszi az eredményt
    public void CraftItem(CraftingRecipe recipe)
    {
        if (CanCraft(recipe))
        {
            // 1. Nyersanyagok levonása
            foreach (CraftingIngredient ingredient in recipe.ingredients)
            {
                InventoryManager.instance.RemoveItem(ingredient.item, ingredient.count);
            }

            // 2. Kész termék hozzáadása (akár többet is, pl. 1 fából 4 deszka)
            for (int i = 0; i < recipe.resultCount; i++)
            {
                InventoryManager.instance.Additem(recipe.resultItem);
            }

            Debug.Log("Sikeresen legyártottad: " + recipe.resultItem.name);
        }
        else
        {
            Debug.Log("Nincs elég nyersanyagod ehhez a recepthez!");
        }
    }
}