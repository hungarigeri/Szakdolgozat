using UnityEngine;
using UnityEngine.UI;
using TMPro; // Ha TextMeshPro-t használsz

public class CraftingButton : MonoBehaviour
{
    [Header("Referenciák")]
    public CraftingRecipe recipe;
    public TextMeshProUGUI buttonText; // A szövegnek
    public Image resultIcon;          // ÚJ: A készülő tárgy ikonjának

    private Button button;

    void Awake()
    {
        button = GetComponent<Button>();
    }

    void Start()
    {
        // Az elején beállítjuk az ikont és az alap szöveget
        if (recipe != null)
        {
            if (resultIcon != null) resultIcon.sprite = recipe.resultItem.image;
            UpdateText();
        }
    }

    void Update()
    {
        // Minden képkockánál ellenőrizzük, hogy le tudjuk-e gyártani
        if (recipe != null && CraftingManager.instance != null)
        {
            bool canCraft = CraftingManager.instance.CanCraft(recipe);

            // Ha nem tudjuk legyártani, a gomb nem kattintható
            button.interactable = canCraft;

            // Színvisszajelzés: ha nem tudjuk legyártani, a szöveg pirosas lesz
            buttonText.color = canCraft ? Color.white : new Color(1f, 0.5f, 0.5f);
        }
    }

    void UpdateText()
    {
        string info = recipe.resultItem.name + "\nÁr: ";
        for (int i = 0; i < recipe.ingredients.Count; i++)
        {
            var ing = recipe.ingredients[i];
            info += ing.count + "x " + ing.item.name;
            if (i < recipe.ingredients.Count - 1) info += ", ";
        }
        buttonText.text = info;
    }

    public void OnClickCraft()
    {
        CraftingManager.instance.CraftItem(recipe);
    }
}