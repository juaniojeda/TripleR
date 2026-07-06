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

    [Serializable]
    private sealed class SkinSlot
    {
        public RecyclingSkinData skin;
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

    [Header("Sections")]
    [SerializeField] private Button licensesTabButton;
    [SerializeField] private Button skinsTabButton;
    [SerializeField] private GameObject licensesPanel;
    [SerializeField] private GameObject skinsPanel;
    [SerializeField] private ShopSection defaultSection = ShopSection.Licenses;

    [Header("Profile UI")]
    [SerializeField] private MainMenuProfileUI profileUI;

    [Header("Licenses")]
    [SerializeField] private LicenseSlot[] licenseSlots;

    [Header("Skins")]
    [SerializeField] private SkinSlot[] skinSlots;

    private enum ShopSection
    {
        Licenses,
        Skins
    }

    private ShopSection currentSection;

    private void Awake()
    {
        currentSection = defaultSection;
        RegisterButtons();

        if (shopPanel != null)
            shopPanel.SetActive(false);
    }

    public void OpenShop()
    {
        if (shopPanel != null)
            shopPanel.SetActive(true);

        ClearFeedback();
        ShowSection(defaultSection);
        Refresh();
    }

    public void CloseShop()
    {
        if (shopPanel != null)
            shopPanel.SetActive(false);

        if (licensesPanel != null && licensesPanel != shopPanel)
            licensesPanel.SetActive(false);

        if (skinsPanel != null && skinsPanel != shopPanel)
            skinsPanel.SetActive(false);
    }

    private void RegisterButtons()
    {
        if (licensesTabButton != null)
            licensesTabButton.onClick.AddListener(ShowLicenses);

        if (skinsTabButton != null)
            skinsTabButton.onClick.AddListener(ShowSkins);

        if (licenseSlots != null)
        {
            for (int i = 0; i < licenseSlots.Length; i++)
            {
                int index = i;
                LicenseSlot slot = licenseSlots[index];

                if (slot != null && slot.buyButton != null)
                    slot.buyButton.onClick.AddListener(() => TryBuyLicense(index));
            }
        }

        if (skinSlots != null)
        {
            for (int i = 0; i < skinSlots.Length; i++)
            {
                int index = i;
                SkinSlot slot = skinSlots[index];

                if (slot != null && slot.buyButton != null)
                    slot.buyButton.onClick.AddListener(() => TryBuySkin(index));
            }
        }
    }

    public void ShowLicenses()
    {
        ShowSection(ShopSection.Licenses);
    }

    public void ShowSkins()
    {
        ShowSection(ShopSection.Skins);
    }

    public void BackToShop()
    {
        ShowLicenses();
    }

    private void ShowSection(ShopSection section)
    {
        currentSection = section;

        bool showingLicenses = currentSection == ShopSection.Licenses;

        SetSectionPanelActive(licensesPanel, showingLicenses);
        SetSectionPanelActive(skinsPanel, !showingLicenses);

        if (licensesTabButton != null)
            licensesTabButton.interactable = !showingLicenses;

        if (skinsTabButton != null)
            skinsTabButton.interactable = showingLicenses;

        ClearFeedback();
    }

    private void SetSectionPanelActive(GameObject sectionPanel, bool active)
    {
        if (sectionPanel == null)
            return;

        sectionPanel.SetActive(active);
    }

    private void TryBuyLicense(int index)
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

    private void TryBuySkin(int index)
    {
        if (skinSlots == null)
            return;

        if (index < 0 || index >= skinSlots.Length)
            return;

        SkinSlot slot = skinSlots[index];

        if (slot == null || slot.skin == null)
            return;

        string skinId = slot.skin.SkinId;
        int price = slot.skin.Price;

        if (PlayerProfile.HasSkin(skinId))
        {
            SetFeedback("Esta skin ya esta comprada.");
            Refresh();
            return;
        }

        bool bought = PlayerProfile.TryBuySkin(skinId, price);

        if (!bought)
        {
            SetFeedback("No tenes monedas suficientes.");
            Refresh();
            return;
        }

        SetFeedback("Skin comprada.");
        Refresh();

        if (profileUI != null)
            profileUI.Refresh();
    }

    private void Refresh()
    {
        if (coinsText != null)
            coinsText.text = $"Monedas: {PlayerProfile.Coins}";

        if (licenseSlots != null)
        {
            for (int i = 0; i < licenseSlots.Length; i++)
            {
                RefreshSlot(licenseSlots[i]);
            }
        }

        if (skinSlots != null)
        {
            for (int i = 0; i < skinSlots.Length; i++)
            {
                RefreshSkinSlot(skinSlots[i]);
            }
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

    private void RefreshSkinSlot(SkinSlot slot)
    {
        if (slot == null || slot.skin == null)
            return;

        bool owned = PlayerProfile.HasSkin(slot.skin.SkinId);

        if (slot.priceText != null)
        {
            slot.priceText.text = owned
                ? "Comprada"
                : $"Costo: {slot.skin.Price}";
        }

        if (slot.stateText != null)
        {
            slot.stateText.text = owned
                ? "Skin activa"
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
