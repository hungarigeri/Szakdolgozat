using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FurnaceUI : MonoBehaviour
{
    public static FurnaceUI instance;

    [Header("Fő UI Elemek")]
    public GameObject furnacePanel;
    public TextMeshProUGUI recipeNameText;
    public TextMeshProUGUI inventoryText;
    public Slider progressBar;
    public Image recipeIcon;

    [Header("Mód Specifikus Panelek")]
    public GameObject manualControlsPanel;
    public GameObject autoControlsPanel;

    [Header("Összes Sütési Recept (Minecraft módhoz)")]
    public CraftingRecipe[] allSmeltingRecipes;

    private Furnace activeFurnace;
    private int frameOpened = -1;
    private int frameClosed = -1; // --- 1. ÚJ VÉDELEM: Mikor zártuk be? ---

    void Awake()
    {
        instance = this;
        if (furnacePanel != null) furnacePanel.SetActive(false);
    }

    void Update()
    {
        // KILÉPÉS LOGIKA ('E' vagy 'ESC' gombbal)
        if (furnacePanel != null && furnacePanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Escape))
            {
                if (Time.frameCount != frameOpened)
                {
                    ToggleUI();
                    return;
                }
            }
        }

        if (!furnacePanel.activeSelf || activeFurnace == null) return;

        if (activeFurnace.currentRecipe != null)
        {
            progressBar.value = activeFurnace.currentProgress / activeFurnace.currentRecipe.requiredWork;
            if (inventoryText != null) inventoryText.text = $"Bemenet: {activeFurnace.storedInput} db\nKimenet: {activeFurnace.storedOutput} db";
        }
        else
        {
            progressBar.value = 0f;
            if (inventoryText != null) inventoryText.text = "A kemence üres.";
        }
    }

    public void OpenUIForFurnace(Furnace furnaceObj)
    {
        // --- 2. ÚJ VÉDELEM: Ha pont most zártuk be az 'E'-vel, ne nyissa azonnal újra! ---
        if (Time.frameCount == frameClosed) return;

        // --- 3. ÚJ VÉDELEM: Ha már nyitva van ez a gép, az 'E' gomb bezárja (Toggle)! ---
        if (furnacePanel.activeSelf && activeFurnace == furnaceObj)
        {
            ToggleUI();
            return;
        }

        activeFurnace = furnaceObj;
        furnacePanel.SetActive(true);

        frameOpened = Time.frameCount;

        if (activeFurnace.isAutomated)
        {
            if (manualControlsPanel != null) manualControlsPanel.SetActive(false);
            if (autoControlsPanel != null) autoControlsPanel.SetActive(true);
        }
        else
        {
            if (manualControlsPanel != null) manualControlsPanel.SetActive(true);
            if (autoControlsPanel != null) autoControlsPanel.SetActive(false);
        }

        if (activeFurnace.currentRecipe != null) UpdateVisuals(activeFurnace.currentRecipe);
        else ClearVisuals();
    }

    // --- RECEPT KIVÁLASZTÁSA ---
    public void SelectRecipe(CraftingRecipe recipe)
    {
        if (activeFurnace != null && activeFurnace.isAutomated)
        {
            activeFurnace.currentRecipe = recipe;
            UpdateVisuals(recipe);
        }
    }

    // --- KÉZI BEPAKOLÁS ---
    public void InsertMaterialButton()
    {
        if (activeFurnace == null || activeFurnace.isAutomated) return;

        Item itemInHand = InventoryManager.instance.GetSelectedItem(false);
        if (itemInHand == null) return;

        if (activeFurnace.currentRecipe != null)
        {
            if (itemInHand == activeFurnace.currentRecipe.ingredients[0].item)
            {
                InventoryManager.instance.GetSelectedItem(true);
                activeFurnace.storedInput++;
            }
        }
        else
        {
            foreach (CraftingRecipe recipe in allSmeltingRecipes)
            {
                if (recipe.ingredients[0].item == itemInHand)
                {
                    activeFurnace.currentRecipe = recipe;
                    InventoryManager.instance.GetSelectedItem(true);
                    activeFurnace.storedInput++;
                    UpdateVisuals(recipe);
                    break;
                }
            }
        }
    }

    // --- KÉZI KIVÉTEL ---
    public void ExtractProductButton()
    {
        if (activeFurnace != null && activeFurnace.storedOutput > 0)
        {
            activeFurnace.storedOutput--;
            InventoryManager.instance.Additem(activeFurnace.currentRecipe.resultItem);

            if (activeFurnace.storedInput <= 0 && activeFurnace.storedOutput <= 0)
            {
                activeFurnace.currentRecipe = null;
                ClearVisuals();
            }
        }
    }

    void UpdateVisuals(CraftingRecipe recipe)
    {
        if (recipeNameText != null) recipeNameText.text = recipe.resultItem.name;
        if (recipeIcon != null)
        {
            recipeIcon.gameObject.SetActive(true);
            recipeIcon.sprite = recipe.resultItem.image;
        }
    }

    void ClearVisuals()
    {
        if (recipeNameText != null) recipeNameText.text = "Válassz receptet / Tegyél be anyagot!";
        if (recipeIcon != null) recipeIcon.gameObject.SetActive(false);
    }

    public void ToggleUI()
    {
        if (furnacePanel == null) return;
        bool isOpen = !furnacePanel.activeSelf;
        furnacePanel.SetActive(isOpen);

        if (!isOpen)
        {
            activeFurnace = null;
            frameClosed = Time.frameCount; // --- 4. ÚJ VÉDELEM: Megjegyezzük a bezárás idejét! ---
        }
    }


}