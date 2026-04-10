using UnityEngine;
using TMPro;

public class StorageUI : MonoBehaviour
{
    public static StorageUI instance;

    public GameObject storagePanel; // Maga az ablak
    public TextMeshProUGUI contentsText; // Ide írjuk ki a tartalmát (egyelőre csak szövegként)

    private StorageContainer currentStorage; // Melyik hordóba nézünk épp?

    void Awake()
    {
        instance = this;
        if (storagePanel != null) storagePanel.SetActive(false);
    }

    public void OpenStorage(StorageContainer container)
    {
        currentStorage = container;
        storagePanel.SetActive(true);
        RefreshUI();
    }

    public void CloseStorage()
    {
        storagePanel.SetActive(false);
        currentStorage = null;
    }

    public void RefreshUI()
    {
        if (currentStorage == null) return;

        string info = "<b>Hordó tartalma:</b>\n\n";

        if (currentStorage.contents.Count == 0)
        {
            info += "<i>Üres</i>";
        }
        else
        {
            foreach (var stored in currentStorage.contents)
            {
                info += $"{stored.count}x {stored.item.name}\n";
            }
        }

        contentsText.text = info;
    }

    // Ezt a gombot nyomjuk meg, hogy betegyük a kezünkben lévő dolgot
    public void DepositHeldItem()
    {
        if (currentStorage == null) return;

        Item heldItem = InventoryManager.instance.GetSelectedItem(false);
        if (heldItem != null)
        {
            // Elvesszük a játékostól (GetSelectedItem(true) fogyaszt egyet)
            InventoryManager.instance.GetSelectedItem(true);

            // Betesszük a hordóba
            currentStorage.DepositItem(heldItem, 1);

            RefreshUI();
        }
    }
}