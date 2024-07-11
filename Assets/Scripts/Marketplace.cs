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
    public AudioClip Choose;
    public List<ShopItem> ShopItemList = new List<ShopItem>();
void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion
    [System.Serializable]
    public class ShopItem
{
    public string name;
    public Sprite Image;
    public int price;
    public bool isAvailable = false;
    public bool isSelected = false;  // Track if the item is currently selected
}
    public static List<Sprite> PurchasedSkins = new List<Sprite>();
    [SerializeField]GameObject ItemTemplate;
    GameObject g;
    [SerializeField] Transform StoreSurf;
    [SerializeField] GameObject StoreMenu;
    [SerializeField] Text Balance;
    Button buyBtn;
    public GameObject NE_Message;
    public static Sprite selectedSkin;
void Start()
{
    NE_Message.SetActive(false);
    LoadItemAvailability();
    // Ensure the first item is always available
    if (ShopItemList.Count > 0) {
        ShopItemList[0].isAvailable = true;
    }
    // Set up UI for each item
    foreach (var item in ShopItemList)
    {
        GameObject g = Instantiate(ItemTemplate, StoreSurf);
        SetupItemButton(g, item);
        UpdateItemUI(g, item);
    }
}
void SetupItemButton(GameObject itemGameObject, ShopItem item)
{
    Button itemButton = itemGameObject.GetComponent<Button>();
    if (itemButton != null)
    {
        itemButton.onClick.RemoveAllListeners();  // Clear existing listeners
        itemButton.onClick.AddListener(() => {
            if (item.isAvailable) {
                SelectItem(item);  // Select item if it is available
            } else {
                PurchaseItem(item);  // Purchase item if it is not yet available
            }
        });
    }
    else
    {
        Debug.LogError("Button component is missing on the item prefab.");
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
public void LoadItemAvailability()
{
    Debug.Log("Loading item availability...");
    PurchasedSkins.Clear(); // Clear the list to avoid duplicating skins on multiple calls.
    for (int i = 0; i < ShopItemList.Count; i++)
    {
        bool isAvailable = PlayerPrefs.GetInt("ItemAvailable_" + i, 0) == 1;
        ShopItemList[i].isAvailable = isAvailable;

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
void UpdateItemUI(GameObject itemGameObject, ShopItem item)
{
    if (itemGameObject != null)
    {
        // Set the icon
        Image iconImage = itemGameObject.transform.GetChild(0).GetComponent<Image>();
        if (iconImage != null)
        {
            iconImage.sprite = item.Image; // Assign the icon sprite
        }
        else
        {
            Debug.LogError("Icon Image component is missing on the item prefab.");
        }

        // Handling the visibility and text of NotBought and Bought buttons
        GameObject notBoughtButton = itemGameObject.transform.GetChild(1).gameObject;
        Text priceText = itemGameObject.transform.GetChild(1).GetChild(1).GetComponent<Text>(); // Assuming this is the correct child index for the price text

        if (notBoughtButton != null && priceText != null)
        {
            notBoughtButton.SetActive(!item.isAvailable);
            priceText.text = item.price.ToString(); // Set the price
        }
        else
        {
            Debug.LogError("NotBoughtButton or Price Text is missing or not accessible in the prefab structure.");
        }

        // Adjust visibility based on purchase state
        GameObject boughtButton = itemGameObject.transform.GetChild(2).gameObject;
        if (boughtButton != null)
        {
            boughtButton.SetActive(item.isAvailable);
        }
        else
        {
            Debug.LogError("BoughtButton is missing in the prefab structure.");
        }
        
        // Update select indicators within the BoughtButton
        UpdateSelectIndicators(boughtButton, item);
    }
    else
    {
        Debug.LogError("Item GameObject is null.");
    }
}

void UpdateSelectIndicators(GameObject boughtButton, ShopItem item)
{
    if (boughtButton != null)
    {
        GameObject select = boughtButton.transform.GetChild(0).gameObject;
        GameObject selected = boughtButton.transform.GetChild(1).gameObject;
        if (select != null && selected != null)
        {
            select.SetActive(item.isAvailable && !item.isSelected);
            selected.SetActive(item.isSelected);
        }
        else
        {
            Debug.LogError("Select or Selected GameObjects are missing in the BoughtButton structure.");
        }
    }
}

public void PurchaseItem(ShopItem item)
{
    if (Product.Instance.HasEnoughCoins(item.price))
    {
        Product.Instance.UseCoins(item.price);
        item.isAvailable = true;
        PlayerPrefs.SetInt("ItemAvailable_" + ShopItemList.IndexOf(item), 1);
        PlayerPrefs.Save();
        Sounds.PlayOneShot(Purchase);
    }
    else
    {
        NE_Message.SetActive(true);
        StartCoroutine("MessageHang");
        Sounds.PlayOneShot(NoBuy);
    }

    UpdateShopUI();  // Refresh the UI for all items
}
void SelectItem(ShopItem selectedItem)
{
    foreach (var item in ShopItemList)
    {
        item.isSelected = false;  // Deselect all items
    }
    Sounds.PlayOneShot(Choose);
    PlayerPrefs.SetString("SelectedSkin", selectedItem.name);
    selectedItem.isSelected = true;  // Select the clicked item
    selectedSkin = selectedItem.Image; // Update the selected skin to use in the game
    PlayerPrefs.Save();

    UpdateShopUI();  // Refresh the UI for all items
}
public void UpdateShopUI()
{
    foreach (var item in ShopItemList)
    {
        int index = ShopItemList.IndexOf(item);
        GameObject itemGameObject = StoreSurf.GetChild(index).gameObject;

        // Set UI elements based on item state
        itemGameObject.transform.GetChild(1).gameObject.SetActive(!item.isAvailable); // NotBoughtButton
        itemGameObject.transform.GetChild(2).gameObject.SetActive(item.isAvailable); // BoughtButton

        // Set selection state
        itemGameObject.transform.GetChild(2).GetChild(1).gameObject.SetActive(item.isSelected); // Selected
        itemGameObject.transform.GetChild(2).GetChild(0).gameObject.SetActive(item.isAvailable && !item.isSelected); // Select
    }
}

public void ResetPurchases()
{
    for (int i = 0; i < ShopItemList.Count; i++)
    {
        if (i != 0) {  // Skip the first item
            ShopItemList[i].isAvailable = false;
            ShopItemList[i].isSelected = false;
            PlayerPrefs.SetInt("ItemAvailable_" + i, 0);
        }
    }
    PlayerPrefs.SetInt("Total",2000);
    PlayerPrefs.SetInt("ItemAvailable_0", 1);  // Ensure first item is available
    PlayerPrefs.Save();
    UpdateShopUI();  // Refresh the UI
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