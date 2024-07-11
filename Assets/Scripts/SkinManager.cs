using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class SkinManager : MonoBehaviour
{
    public Marketplace Reference;
    public List<Sprite> dartSkins = new List<Sprite>();
    private string selectedSkinName;
    public GameObject Menu;
    public GameObject Shop;
    void Start()
    {
        InstantShopUpdate();
        Reference.LoadItemAvailability();
        Reference.UpdateShopUI();
        selectedSkinName = PlayerPrefs.GetString("SelectedSkin", "");
    }
    void InstantShopUpdate()
    {
        if(Shop.activeSelf)
        {
        Menu.SetActive(true);
        Shop.SetActive(false);
        }
    }
    void LoadPurchasedSkins()
{
    dartSkins.Clear(); // Clear to avoid duplication

    Debug.Log("Loading purchased skins, count: " + Marketplace.PurchasedSkins.Count);

    dartSkins.AddRange(Marketplace.PurchasedSkins);

    // Debug each loaded skin
    foreach (var skin in dartSkins)
    {
        Debug.Log("Loaded skin: " + skin.name);
    }
}
}