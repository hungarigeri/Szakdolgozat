using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class RocketRequirement
{
    public Item requiredItem; // Mit kér? (pl. Vasérc)
    public int targetAmount;  // Mennyi kell belőle?
    public int currentAmount; // Mennyit töltöttünk be eddig?
}

[System.Serializable]
public class RocketPhase
{
    public string phaseName; // Pl. "1. Fázis: Túlélés"
    public RocketRequirement[] requirements; // Mik kellenek ehhez a fázishoz
}

public class DeliveryRocket : MonoBehaviour
{
    [Header("Progresszió")]
    public RocketPhase[] phases; // Az összes fázis listája (Inspectorban állíthatod be!)
    public int currentPhaseIndex = 0;

    // Ezt hívja meg a Futószalag (ConveyorBelt.cs), amikor beletol valamit
    public bool AcceptItem(Item item)
    {
        if (currentPhaseIndex >= phases.Length) return false;
        RocketPhase currentPhase = phases[currentPhaseIndex];

        foreach (RocketRequirement req in currentPhase.requirements)
        {
            if (req.requiredItem == item && req.currentAmount < req.targetAmount)
            {
                req.currentAmount++;
                CheckPhaseComplete();

                // --- ÚJ: FRISSÍTJÜK A UI-T, MERT KAPTUNK VALAMIT! ---
                if (RocketUI.instance != null && RocketUI.instance.rocketPanel.activeSelf)
                {
                    RocketUI.instance.UpdateDisplay();
                }

                return true;
            }
        }
        return false;
    }

    void CheckPhaseComplete()
    {
        RocketPhase currentPhase = phases[currentPhaseIndex];

        // Ellenőrizzük, van-e olyan követelmény, ami még NINCS kész
        foreach (RocketRequirement req in currentPhase.requirements)
        {
            if (req.currentAmount < req.targetAmount)
            {
                return; // Még nincs kész minden, kilépünk
            }
        }

        // --- HA IDE ELJUTUNK, MINDEN ÖSSZEGYŰLT! KILÖVÉS! ---
        LaunchRocket();
    }

    void LaunchRocket()
    {
        Debug.Log($"--- RAKÉTA KILÖVE: {phases[currentPhaseIndex].phaseName} KÉSZ! ---");
        currentPhaseIndex++;

        // --- ÚJ: FRISSÍTJÜK A UI-T, HOGY MUTASSA AZ ÚJ FÁZIST! ---
        if (RocketUI.instance != null && RocketUI.instance.rocketPanel.activeSelf)
        {
            RocketUI.instance.UpdateDisplay();
        }
    }

    // --- ÚJ: Kézi beadás a UI-ból ---
    public bool TryManualInsert(int reqIndex)
    {
        if (currentPhaseIndex >= phases.Length) return false;

        RocketPhase currentPhase = phases[currentPhaseIndex];
        RocketRequirement req = currentPhase.requirements[reqIndex];

        // 1. Kell-e egyáltalán ebből a tárgyból még?
        if (req.currentAmount >= req.targetAmount) return false;

        // 2. Megnézzük a játékos Inventory-ját (a GetItemCount-ot használtad a Workbenchben is!)
        int playerHasAmount = InventoryManager.instance.GetItemCount(req.requiredItem);

        if (playerHasAmount > 0)
        {
            // 3. Levonunk 1 db-ot a játékostól 
            // FIGYELEM: Ha az InventoryManageredben a törlés máshogy van elnevezve (pl. ConsumeItem), itt írd át!
            InventoryManager.instance.RemoveItem(req.requiredItem, 1);

            // 4. Beletesszük a rakétába
            req.currentAmount++;
            CheckPhaseComplete();
            return true; // Sikeres beadás!
        }

        return false; // Nincs a játékosnál ilyen tárgy
    }
}