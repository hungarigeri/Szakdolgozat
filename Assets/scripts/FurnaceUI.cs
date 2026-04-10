using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FurnaceUI : MonoBehaviour
{
    public static FurnaceUI instance;

    [Header("UI Elemek")]
    public GameObject furnacePanel;
    public TextMeshProUGUI recipeNameText;
    public TextMeshProUGUI costText;
    public Slider progressBar;
    public Image recipeIcon;

    [Header("Smelting Logika")]
    public float smeltSpeed = 0.5f; // Lassabb, mint a kézi craftolás
    private CraftingRecipe selectedRecipe;
    private float currentProgress = 0f;

    void Awake()
    {
        instance = this;
        if (furnacePanel != null) furnacePanel.SetActive(false);
    }

    void Update()
    {
        if (furnacePanel == null || !furnacePanel.activeSelf || selectedRecipe == null) return;

        // AUTOMATA MÓD: Ha van elég anyag, magától elindul a csík
        if (CraftingManager.instance.CanCraft(selectedRecipe))
        {
            currentProgress += Time.deltaTime * smeltSpeed;
            progressBar.value = currentProgress / selectedRecipe.requiredWork;

            if (currentProgress >= selectedRecipe.requiredWork)
            {
                SmeltItem();
            }
        }
        else
        {
            // Ha elfogy az anyag, megáll és nullázódik
            currentProgress = 0f;
            progressBar.value = 0f;
        }
    }

    public void ToggleUI()
    {
        bool isOpen = !furnacePanel.activeSelf;
        furnacePanel.SetActive(isOpen);

        if (isOpen)
        {
            currentProgress = 0f;
            if (progressBar != null) progressBar.value = 0f;
        }
    }

    public void SelectRecipe(CraftingRecipe recipe)
    {
        selectedRecipe = recipe;
        recipeNameText.text = recipe.resultItem.name;

        // JAVÍTÁS: .image helyett .icon, és hozzáadtuk a színfrissítést!
        if (recipeIcon != null && recipe.resultItem != null)
        {
            recipeIcon.sprite = recipe.resultItem.image;
            recipeIcon.color = Color.white; // Láthatóvá tesszük a fehér négyzet helyén
        }

        UpdateCostText(recipe);
        currentProgress = 0f;
        if (progressBar != null) progressBar.value = 0f;
    }

    void UpdateCostText(CraftingRecipe recipe)
    {
        if (recipe == null) return;

        string info = "Szükséges:\n";
        foreach (var ing in recipe.ingredients)
        {
            int count = InventoryManager.instance.GetItemCount(ing.item);
            string color = count >= ing.count ? "white" : "red";
            info += $"<color={color}>{ing.count}x {ing.item.name} ({count})</color>\n";
        }
        costText.text = info;
    }

    void SmeltItem()
    {
        CraftingManager.instance.CraftItem(selectedRecipe);
        currentProgress = 0f;
        progressBar.value = 0f;
        UpdateCostText(selectedRecipe); // Frissítjük a számokat a UI-on
    }
}