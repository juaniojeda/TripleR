using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class MainMenuShopUI : MonoBehaviour
{
    [Serializable]
    private sealed class LicenseSlot
    {
        public RecyclingLicenseData license;
        public Button buyButton;
        public TMP_Text priceText;
        public TMP_Text stateText;
        public GameObject ownedMark;
    }

    [Header("Panel")]
    [SerializeField] private GameObject shopPanel;

    [Header("Texts")]
    [SerializeField] private TMP_Text coinsText;
    [SerializeField] private TMP_Text feedbackText;

    [Header("Profile UI")]
    [SerializeField] private MainMenuProfileUI profileUI;

    [Header("Licenses")]
    [SerializeField] private LicenseSlot[] licenseSlots;

    private void Awake()
    {
        RegisterButtons();

        if (shopPanel != null)
            shopPanel.SetActive(false);
    }

    public void OpenShop()
    {
        if (shopPanel != null)
            shopPanel.SetActive(true);

        ClearFeedback();
        Refresh();
    }

    public void CloseShop()
    {
        if (shopPanel != null)
            shopPanel.SetActive(false);
    }

    private void RegisterButtons()
    {
        if (licenseSlots == null)
            return;

        for (int i = 0; i < licenseSlots.Length; i++)
        {
            int index = i;
            LicenseSlot slot = licenseSlots[index];

            if (slot != null && slot.buyButton != null)
                slot.buyButton.onClick.AddListener(() => TryBuy(index));
        }
    }

    private void TryBuy(int index)
    {
        if (licenseSlots == null)
            return;

        if (index < 0 || index >= licenseSlots.Length)
            return;

        LicenseSlot slot = licenseSlots[index];

        if (slot == null || slot.license == null)
            return;

        string licenseId = slot.license.LicenseId;
        int price = slot.license.Price;

        if (PlayerProfile.HasLicense(licenseId))
        {
            SetFeedback("Esta licencia ya está comprada.");
            Refresh();
            return;
        }

        bool bought = PlayerProfile.TryBuyLicense(licenseId, price);

        if (!bought)
        {
            SetFeedback("No tenés monedas suficientes.");
            Refresh();
            return;
        }

        SetFeedback("Licencia comprada.");
        Refresh();

        if (profileUI != null)
            profileUI.Refresh();
    }

    private void Refresh()
    {
        if (coinsText != null)
            coinsText.text = $"Monedas: {PlayerProfile.Coins}";

        if (licenseSlots == null)
            return;

        for (int i = 0; i < licenseSlots.Length; i++)
        {
            RefreshSlot(licenseSlots[i]);
        }
    }

    private void RefreshSlot(LicenseSlot slot)
    {
        if (slot == null || slot.license == null)
            return;

        bool owned = PlayerProfile.HasLicense(slot.license.LicenseId);

        if (slot.priceText != null)
        {
            slot.priceText.text = owned
                ? "Comprada"
                : $"Costo: {slot.license.Price}";
        }

        if (slot.stateText != null)
        {
            slot.stateText.text = owned
                ? "Licencia activa"
                : "Bloqueada";
        }

        if (slot.buyButton != null)
            slot.buyButton.interactable = !owned;

        if (slot.ownedMark != null)
            slot.ownedMark.SetActive(owned);
    }

    private void SetFeedback(string message)
    {
        if (feedbackText != null)
            feedbackText.text = message;
    }

    private void ClearFeedback()
    {
        if (feedbackText != null)
            feedbackText.text = "";
    }
}