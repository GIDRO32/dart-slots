using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class Marketplace : MonoBehaviour
{
    #region SingleTon:Marketplace
    public static Marketplace Instance;
    public AudioSource Sounds;
    public AudioClip Purchase;
    public AudioClip NoBuy;
    void Awake()
    {
        if(Instance == null)
        Instance = this;
        else
        Destroy(gameObject);
    }
    #endregion
    [System.Serializable]
    public class ShopItem
{
    public Sprite Image;
    public int price;
    public bool isAvailable = false;
}
    public List<ShopItem> ShopItemList;
    public static List<Sprite> PurchasedSkins = new List<Sprite>();
    [SerializeField]GameObject ItemTemplate;
    GameObject g;
    [SerializeField] Transform StoreSurf;
    [SerializeField] GameObject StoreMenu;
    [SerializeField] Text Balance;
    Button buyBtn;
    public GameObject NE_Message;
void Start()
{
    LoadItemAvailability();
    NE_Message.SetActive(false);
    int len = ShopItemList.Count;
    for (int i = 0; i < len; i++)
    {
        g = Instantiate(ItemTemplate, StoreSurf);
        g.transform.GetChild(1).GetComponent<Image>().sprite = ShopItemList[i].Image; // Frame
        g.transform.GetChild(2).GetComponent<Text>().text = ShopItemList[i].price.ToString(); // Price
        buyBtn = g.transform.GetChild(3).GetComponent<Button>(); // Buy button
        if (ShopItemList[i].isAvailable)
        {
            DisableBuyButton(buyBtn);
        }
        else
        {
            buyBtn.AddEventListener(i, OnBuyBtnClicked);
        }
    }
}

    IEnumerator MessageHang()
    {
            for(float f = 1f; f >= -0.05f; f -= 0.05f)
            {
                yield return new WaitForSeconds(0.1f);
            }
            NE_Message.SetActive(false);
    }
void LoadItemAvailability()
{
    Debug.Log("Loading item availability...");
    PurchasedSkins.Clear(); // Clear the list to avoid duplicating skins on multiple calls.
    for (int i = 0; i < ShopItemList.Count; i++)
    {
        bool isAvailable = PlayerPrefs.GetInt("ItemAvailable_" + i, 0) == 1;
        ShopItemList[i].isAvailable = isAvailable;
        Debug.Log($"Item {i}: {ShopItemList[i].Image.name}, Available: {isAvailable}");

        // If the item is marked as available, add its sprite to the PurchasedSkins list
        if (isAvailable)
        {
            if (!PurchasedSkins.Contains(ShopItemList[i].Image))
            {
                PurchasedSkins.Add(ShopItemList[i].Image);
            }
        }
    }
}


void OnBuyBtnClicked(int itemIndex)
{
    if (Product.Instance.HasEnoughCoins(ShopItemList[itemIndex].price))
    {
        Product.Instance.UseCoins(ShopItemList[itemIndex].price);
        ShopItemList[itemIndex].isAvailable = true;
        PlayerPrefs.SetInt("ItemAvailable_" + itemIndex, 1); // Save item as available

        // Debugging the skin added
        Debug.Log("Adding skin to PurchasedSkins: " + ShopItemList[itemIndex].Image.name);

        PurchasedSkins.Add(ShopItemList[itemIndex].Image); // Assuming this is a Sprite
        PlayerPrefs.Save();

        UpdateShopUI();
        Sounds.PlayOneShot(Purchase);
    }
    else
    {
        NE_Message.SetActive(true);
        StartCoroutine("MessageHang");
        Sounds.PlayOneShot(NoBuy);
    }
}

void UpdateShopUI()
{
    for (int i = 0; i < ShopItemList.Count; i++)
    {
        Transform itemTransform = StoreSurf.GetChild(i);
        Button buyButton = itemTransform.GetChild(3).GetComponent<Button>();  // Make sure index is correct
        if (ShopItemList[i].isAvailable)
        {
            buyButton.interactable = false;
            buyButton.transform.GetChild(0).GetComponent<Text>().text = "Owned";
        }
        else
        {
            buyButton.interactable = true;
            buyButton.transform.GetChild(0).GetComponent<Text>().text = "Buy";
        }
    }
}

public void ResetPurchases()
{
    PurchasedSkins.Clear();
    for (int i = 0; i < ShopItemList.Count; i++)
    {
        ShopItemList[i].isAvailable = false;  // Make items unavailable
        PlayerPrefs.SetInt("ItemAvailable_" + i, 0);  // Reset PlayerPrefs state

        // Update UI immediately
        Transform itemTransform = StoreSurf.GetChild(i);
        Button buyButton = itemTransform.GetChild(3).GetComponent<Button>();  // Assuming button is at index 3
        buyButton.interactable = true;
        buyButton.transform.GetChild(0).GetComponent<Text>().text = "Buy";  // Update button text
    }

    PlayerPrefs.SetInt("Total", 2000);  // Reset coin balance
    PlayerPrefs.Save();  // Save changes to PlayerPrefs

    UpdateShopUI();  // Refresh the UI if necessary
}


void DisableBuyButton(Button buyButton)
{
    buyButton.interactable = false;
    buyButton.transform.GetChild(0).GetComponent<Text>().text = "Owned";
}

    void Update()
    {
        Balance.text = Product.Instance.Coins.ToString();
        // LoadItemAvailability();
    }
    public void OpenShop()
    {
        StoreMenu.SetActive(true);
    }
    public void CloseShop()
    {
        StoreMenu.SetActive(false);
    }
}