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

    [Header("ÚJ: Hibaüzenet")]
    public TextMeshProUGUI warningText;
    private float warningTimer = 0f;

    [Header("Mód Specifikus Panelek")]
    public GameObject manualControlsPanel;
    public GameObject autoControlsPanel;

    [Header("Összes Sütési Recept")]
    public CraftingRecipe[] allSmeltingRecipes;

    private Furnace activeFurnace;
    private int frameOpened = -1;
    private int frameClosed = -1;

    void Awake()
    {
        instance = this;
        if (furnacePanel != null) furnacePanel.SetActive(false);
        if (warningText != null) warningText.gameObject.SetActive(false);
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

        // Hibaüzenet eltüntetése
        if (warningTimer > 0)
        {
            warningTimer -= Time.deltaTime;
            if (warningTimer <= 0 && warningText != null) warningText.gameObject.SetActive(false);
        }

        if (!furnacePanel.activeSelf || activeFurnace == null) return;

        // UI Frissítése
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
        if (Time.frameCount == frameClosed) return;

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

    // --- ÚJ: RECEPT KIVÁLASZTÁSA (Már a sima kemencénél is engedi!) ---
    public void SelectRecipe(CraftingRecipe recipe)
    {
        if (activeFurnace != null)
        {
            // Biztonsági védelem: Ha már van benne MÁSIK anyag, ne engedjük átváltani
            if (activeFurnace.storedInput > 0 && activeFurnace.currentRecipe != null && activeFurnace.currentRecipe != recipe)
            {
                ShowWarning("Előbb vedd ki a bent lévő anyagot!");
                return;
            }

            activeFurnace.currentRecipe = recipe;
            UpdateVisuals(recipe);
        }
    }

    // --- ÚJ: KÉZI BEPAKOLÁS (A teljes táskát nézi) ---
    public void InsertMaterialButton()
    {
        if (activeFurnace == null || activeFurnace.isAutomated) return;

        if (activeFurnace.currentRecipe == null)
        {
            ShowWarning("Előbb válassz egy receptet a listából!");
            return;
        }

        // Megnézzük, mi kell a recepthez
        Item requiredItem = activeFurnace.currentRecipe.ingredients[0].item;

        // Ellenőrizzük az egész inventoryt
        int playerHasAmount = InventoryManager.instance.GetItemCount(requiredItem);

        if (playerHasAmount > 0)
        {
            // FIGYELEM: Ha nálad máshogy hívják a törlést, írd át a RemoveItem-et!
            InventoryManager.instance.RemoveItem(requiredItem, 1);
            activeFurnace.storedInput++;
        }
        else
        {
            ShowWarning($"Nincs nálad: {requiredItem.name}!");
        }
    }

    // --- KÉZI KIVÉTEL ---
    public void ExtractProductButton()
    {
        if (activeFurnace != null && activeFurnace.storedOutput > 0)
        {
            activeFurnace.storedOutput--;
            InventoryManager.instance.Additem(activeFurnace.currentRecipe.resultItem);

            // A Minecraft logikát (ClearVisuals) kivettük, így megjegyzi a receptet!
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
        if (recipeNameText != null) recipeNameText.text = "Válassz receptet a listából!";
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
            frameClosed = Time.frameCount;
        }
    }

    // --- ÚJ: Hibaüzenet megjelenítése ---
    void ShowWarning(string msg)
    {
        if (warningText != null)
        {
            warningText.text = msg;
            warningText.color = Color.red;
            warningText.gameObject.SetActive(true);
            warningTimer = 2f;
        }
    }
}