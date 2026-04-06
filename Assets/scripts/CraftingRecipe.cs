using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class CraftingIngredient
{
    public Item item;
    public int count;
}

[CreateAssetMenu(fileName = "New Recipe", menuName = "Inventory/Crafting Recipe")]
public class CraftingRecipe : ScriptableObject
{
    [Header("Hol készíthető?")]
    public bool requiresCraftingTable = false;

    [Header("Satisfactory Crafting")]
    // ÚJ: Hány egységnyi "munka" kell hozzá? (Kicsi dolog: 1, Nagy dolog: 5)
    public float requiredWork = 1f;

    [Header("Mit kapunk a végén?")]
    public Item resultItem;
    public int resultCount = 1;

    [Header("Mi kell hozzá? (Nyersanyagok)")]
    public List<CraftingIngredient> ingredients;
}