using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Product : MonoBehaviour
{
    public int Coins;
    #region Singleton:Product
    public static Product Instance;
    public LevelHandle levelHandle;
    void Awake()
    {
        if(Instance == null)
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

    public void UseCoins(int amount)
    {
        Coins -= amount;
        PlayerPrefs.SetInt("Total", Coins);
    }
    public bool HasEnoughCoins(int amount)
    {
        return(Coins >= amount);
    }
    void Update()
    {
        Coins = PlayerPrefs.GetInt("Total", Coins);
        PlayerPrefs.SetInt("Total", Coins);
    }
    public void ResetData()
    {
        Coins = 2000;
        PlayerPrefs.SetInt("Total", Coins);
    }
}
