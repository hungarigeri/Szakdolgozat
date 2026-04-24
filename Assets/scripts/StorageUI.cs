using UnityEngine;
using TMPro;

public class StorageUI : MonoBehaviour
{
    public static StorageUI instance;

    public GameObject storagePanel;
    public TextMeshProUGUI contentsText;

    private StorageContainer currentStorage;

    // --- ÚJ: Időzítés védő változók (mint a Kemencénél) ---
    private int frameOpened = -1;
    private int frameClosed = -1;

    void Awake()
    {
        instance = this;
        if (storagePanel != null) storagePanel.SetActive(false);
    }

    void Update()
    {
        // --- ÚJ: Kilépés ESC vagy E gombbal ---
        if (storagePanel != null && storagePanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.E))
            {
                // Csak akkor zárjuk be, ha nem pont ebben a képkockában nyitottuk ki
                if (Time.frameCount != frameOpened)
                {
                    CloseStorage();
                }
            }
        }
    }

    public void OpenStorage(StorageContainer container)
    {
        // Ha pont most zártuk be, ne nyíljon újra
        if (Time.frameCount == frameClosed) return;

        // Ha már nyitva van, és újra rányomunk, zárja be (Toggle)
        if (storagePanel.activeSelf && currentStorage == container)
        {
            CloseStorage();
            return;
        }

        currentStorage = container;
        storagePanel.SetActive(true);
        frameOpened = Time.frameCount; // Megjegyezzük a nyitás pillanatát

        UpdateUI();
    }

    public void CloseStorage()
    {
        if (storagePanel != null) storagePanel.SetActive(false);
        currentStorage = null;
        frameClosed = Time.frameCount; // Megjegyezzük a bezárás pillanatát
    }

    // --- ÁTNEVEZTÜK: RefreshUI -> UpdateUI, hogy passzoljon a StorageContainer-hez! ---
    public void UpdateUI()
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

        if (contentsText != null) contentsText.text = info;
    }

    // Beteszünk 1 db-ot a kezünkben lévő tárgyból
    public void DepositHeldItem()
    {
        if (currentStorage == null) return;

        Item heldItem = InventoryManager.instance.GetSelectedItem(false);
        if (heldItem != null)
        {
            InventoryManager.instance.GetSelectedItem(true); // Elveszünk 1-et
            currentStorage.DepositItem(heldItem, 1); // Beteszünk 1-et
            UpdateUI();
        }
    }

    // --- ÚJ: Kiveszünk 1 db-ot a hordóból (A legelső tárgyból) ---
    public void WithdrawItemButton()
    {
        if (currentStorage != null && currentStorage.contents.Count > 0)
        {
            // Mindig a lista legelső elemét próbáljuk kivenni
            Item itemToTake = currentStorage.contents[0].item;

            // Betesszük a táskába
            InventoryManager.instance.Additem(itemToTake);

            // Kivesszük a hordóból
            currentStorage.WithdrawItem(itemToTake, 1);

            UpdateUI();
        }
    }
}