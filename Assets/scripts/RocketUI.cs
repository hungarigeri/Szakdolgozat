using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RocketUI : MonoBehaviour
{
    public static RocketUI instance;

    [Header("Fő UI Elemek")]
    public GameObject rocketPanel;
    public TextMeshProUGUI phaseTitleText;

    [Header("Hibaüzenet")]
    public TextMeshProUGUI warningText;
    private float warningTimer = 0f;

    [Header("Előre elkészített sorok (Rows)")]
    public GameObject[] requirementRows;
    public TextMeshProUGUI[] itemNameTexts;
    public TextMeshProUGUI[] progressTexts;
    public Slider[] progressBars;
    public Button[] insertButtons;

    private DeliveryRocket activeRocket;

    // --- ÚJ: Időzítés védő változók ---
    private int frameOpened = -1;
    private int frameClosed = -1;

    void Awake()
    {
        instance = this;
        if (rocketPanel != null) rocketPanel.SetActive(false);
        if (warningText != null) warningText.gameObject.SetActive(false);
    }

    void Update()
    {
        // Kilépés ESC vagy E gombbal
        if (rocketPanel.activeSelf && (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.E)))
        {
            // ÚJ: Csak akkor zárjuk be, ha nem pont ebben a képkockában nyitottuk ki!
            if (Time.frameCount != frameOpened)
            {
                ToggleUI();
                return;
            }
        }

        // Hibaüzenet eltüntetése
        if (warningTimer > 0)
        {
            warningTimer -= Time.deltaTime;
            if (warningTimer <= 0 && warningText != null) warningText.gameObject.SetActive(false);
        }
    }

    public void OpenUI(DeliveryRocket rocket)
    {
        // ÚJ: Ha pont most zártuk be az 'E'-vel, ne nyissa azonnal újra a FarmingSystem!
        if (Time.frameCount == frameClosed) return;

        // Ha már nyitva van, és újra rányomunk, akkor bezárja (Toggle)
        if (rocketPanel.activeSelf && activeRocket == rocket)
        {
            ToggleUI();
            return;
        }

        activeRocket = rocket;
        rocketPanel.SetActive(true);
        frameOpened = Time.frameCount; // Megjegyezzük a nyitás pillanatát

        UpdateDisplay();
    }

    public void ToggleUI()
    {
        if (rocketPanel != null)
        {
            bool isOpen = !rocketPanel.activeSelf;
            rocketPanel.SetActive(isOpen);

            if (!isOpen)
            {
                activeRocket = null;
                frameClosed = Time.frameCount; // Megjegyezzük a bezárás pillanatát
            }
        }
    }

    public void UpdateDisplay()
    {
        if (activeRocket == null || activeRocket.currentPhaseIndex >= activeRocket.phases.Length)
        {
            phaseTitleText.text = "MINDEN FÁZIS TELJESÍTVE!";
            foreach (var row in requirementRows) row.SetActive(false);
            return;
        }

        RocketPhase currentPhase = activeRocket.phases[activeRocket.currentPhaseIndex];
        phaseTitleText.text = currentPhase.phaseName;

        for (int i = 0; i < requirementRows.Length; i++)
        {
            if (i < currentPhase.requirements.Length)
            {
                requirementRows[i].SetActive(true);

                RocketRequirement req = currentPhase.requirements[i];
                itemNameTexts[i].text = req.requiredItem.name;
                progressTexts[i].text = $"{req.currentAmount} / {req.targetAmount}";
                progressBars[i].value = (float)req.currentAmount / req.targetAmount;

                if (insertButtons.Length > i && insertButtons[i] != null)
                {
                    int reqIndex = i;
                    insertButtons[i].onClick.RemoveAllListeners();
                    insertButtons[i].onClick.AddListener(() => OnInsertButtonClicked(reqIndex));
                    insertButtons[i].interactable = (req.currentAmount < req.targetAmount);
                }
            }
            else
            {
                requirementRows[i].SetActive(false);
            }
        }
    }

    public void OnInsertButtonClicked(int reqIndex)
    {
        if (activeRocket != null)
        {
            bool success = activeRocket.TryManualInsert(reqIndex);

            if (success)
            {
                UpdateDisplay();
            }
            else
            {
                ShowWarning("Nincs nálad elég nyersanyag!");
            }
        }
    }

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