using UnityEngine;
using UnityEngine.UI;
using TMPro; // Ha TextMeshPro-t használsz a szövegekhez

public class WorkbenchUI : MonoBehaviour
{
    public static WorkbenchUI instance;

    [Header("UI Elemek")]
    public GameObject workbenchPanel; // Maga a teljes Munkaasztal ablak
    public TextMeshProUGUI recipeNameText;
    public TextMeshProUGUI costText;
    public Slider progressBar; // A Satisfactory csík!

    [Header("Crafting Logika")]
    public float craftSpeed = 2f; // Milyen gyorsan telik a csík (lehet állítani)
    private CraftingRecipe selectedRecipe;

    private bool isHoldingButton = false;
    private float currentProgress = 0f;

    void Awake()
    {
        instance = this;
        workbenchPanel.SetActive(false); // Zárva indul
    }

    void Update()
    {
        if (!workbenchPanel.activeSelf || selectedRecipe == null) return;

        // Satisfactory logika: Nyomva tartjuk a gombot
        if (isHoldingButton)
        {
            // Ellenőrizzük, hogy van-e elég anyag
            if (CraftingManager.instance.CanCraft(selectedRecipe))
            {
                currentProgress += Time.deltaTime * craftSpeed;
                progressBar.value = currentProgress / selectedRecipe.requiredWork;

                if (currentProgress >= selectedRecipe.requiredWork)
                {
                    CraftItem();
                }
            }
            else
            {
                // Ha nincs elég anyag, a csík nem mozog, és küldhetünk egy üzenetet (opcionális)
                // Debug.Log("Nincs elég nyersanyag a gyártáshoz!");
                currentProgress = 0f;
                progressBar.value = 0f;
            }
        }
        else
        {
            // Ha elengedjük, azonnal visszaesik (Satisfactory stílus)
            currentProgress = 0f;
            progressBar.value = 0f;
        }
    }

    // Ezt hívja meg az E gomb a FarmingSystem-ből
    public void ToggleUI()
    {
        bool isOpen = !workbenchPanel.activeSelf;
        workbenchPanel.SetActive(isOpen);

        if (isOpen)
        {
            currentProgress = 0f;
            if (progressBar != null) progressBar.value = 0f;
        }
    }

    // Ezt a függvényt hívják meg a bal oldali Recept gombok
    public void SelectRecipe(CraftingRecipe recipe)
    {
        selectedRecipe = recipe;
        recipeNameText.text = recipe.resultItem.name;

        string info = "Szükséges:\n";
        foreach (var ing in recipe.ingredients)
        {
            int hasAmount = InventoryManager.instance.GetItemCount(ing.item);

            // Ha kevesebb van nálunk, mint amennyi kell, pirossal írjuk ki (Rich Text kell a TMPro-nál!)
            if (hasAmount < ing.count)
            {
                info += "<color=red>" + ing.count + "x " + ing.item.name + " (" + hasAmount + ")</color>\n";
            }
            else
            {
                info += ing.count + "x " + ing.item.name + " (" + hasAmount + ")\n";
            }
        }
        costText.text = info;

        currentProgress = 0f;
        progressBar.value = 0f;
    }

    // --- EZEKET HÍVJA A "CRAFT" GOMB ---
    public void OnCraftButtonDown()
    {
        isHoldingButton = true;
    }

    public void OnCraftButtonUp()
    {
        isHoldingButton = false;
    }

    void CraftItem()
    {
        CraftingManager.instance.CraftItem(selectedRecipe);
        currentProgress = 0f; // Visszaállítjuk a csíkot a következőhöz
        progressBar.value = 0f;
    }
}