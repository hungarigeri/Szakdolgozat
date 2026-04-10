using UnityEngine;
using UnityEngine.UI;

public class RecipeButton : MonoBehaviour
{
    // Ezzel hozunk létre egy lenyíló menüt a Unity Inspectorban
    public enum StationType { Workbench, Furnace }

    [Header("Beállítások")]
    public StationType targetStation; // Ide kerül a lenyíló menü
    public CraftingRecipe recipe;     // Ide húzod a receptet

    private Image buttonImage;
    private Button buttonComponent;

    void Start()
    {
        buttonImage = GetComponent<Image>();
        buttonComponent = GetComponent<Button>();

        // AUTOMATIKUS KÉP BEÁLLÍTÁS
        if (recipe != null && recipe.resultItem != null && recipe.resultItem.image != null)
        {
            buttonImage.sprite = recipe.resultItem.image;
            buttonImage.color = Color.white; // Biztosítjuk, hogy látszódjon
        }

        // KATTINTÁS BEKÖTÉSE
        if (buttonComponent != null)
        {
            buttonComponent.onClick.AddListener(OnClickRecipe);
        }
    }

    void OnClickRecipe()
    {
        if (recipe == null) return;

        // A lenyíló menü alapján döntjük el, kinek küldjük a receptet!
        if (targetStation == StationType.Workbench)
        {
            if (WorkbenchUI.instance != null) WorkbenchUI.instance.SelectRecipe(recipe);
        }
        else if (targetStation == StationType.Furnace)
        {
            if (FurnaceUI.instance != null) FurnaceUI.instance.SelectRecipe(recipe);
        }
    }
}