using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class DartSlot : MonoBehaviour
{
    public GameObject slotPrefab;  // The Slot prefab to be instantiated
    public Sprite[] icons;         // Array of sprites to be used as icons

    public float spawnInterval = 0.3f;  // Interval between spawns
    public float moveSpeed = 3f;        // Speed at which slots move down
    public float destroyY = -2f;        // Y position at which slots destroy themselves

    public float[] spawnPositionsX = { -1.5f, 0f, 1.5f };  // Array of X positions for spawning

    private List<SlotMover> activeSlots = new List<SlotMover>();  // List to keep track of active slots
    private Dictionary<float, Coroutine> spawnCoroutines = new Dictionary<float, Coroutine>();  // Reference to the spawn coroutines for each X position
public GameObject targetCircle;  // Reference to the target circle
public GameObject dartPrefab;    // Reference to the dart prefab

public float targetCircleSpeed = 2f;  // Speed of the target circle movement
public Vector2 targetCircleBounds = new Vector2(-3f, 3f);  // Bounds for target circle movement

private bool isMovingRight = true;  // Direction flag for the target circle
private HashSet<float> hitRows = new HashSet<float>();

    public int freeDarts = 3;
    public int dartPrice = 30;
    public int coins = 1000;
    public Text balance;
    public GameObject BasicGame;
    public GameObject Starter;
    public string diff_mark;
    public int gameprice;
    public int multiplier;
    private int winnings;
    public GameObject[] boxes = new GameObject[3];  // Array to store references to the box sprites
    public GameObject resultsPopup;  // Reference to the results popup GameObject
    private int hits = 0;  // Track the number of successful hits
    public int[] iconValues = { 100, 150, 200, 250, 300 };
    public List<SpriteRenderer> boxRenderers;  // List to store references to the box sprite renderers
    public List<Sprite> hitIcons = new List<Sprite>();
    public Text result_text;
    public List<Sprite> dartSkins = new List<Sprite>(); // List to hold purchased dart skins
    public Sprite defaultDartSkin; // Default dart skin

private void Start()
    {
        LoadPurchasedSkins();
        Starter.SetActive(true);
        BasicGame.SetActive(false);
        resultsPopup.SetActive(false);
        foreach (float xPos in spawnPositionsX)
        {
            spawnCoroutines[xPos] = StartCoroutine(SpawnSlots(xPos));
        }
        coins = PlayerPrefs.GetInt("Total", coins);
    }
    void Update()
    {
        balance.text = coins.ToString();
        if(coins < dartPrice && freeDarts == 0)
        {
            result_text.text = "No coins left\nTry lower difficulty";
            ShowResultsPopup();
        }
        else if(coins < dartPrice && freeDarts == 0 && diff_mark == "Easy")
        {
            result_text.text = "No free darts left\nTry again";
            ShowResultsPopup();
        }
    }
void LoadPurchasedSkins()
{
    dartSkins.Clear(); // Clear to avoid duplication
    dartSkins.Add(defaultDartSkin); // Add default first

    Debug.Log("Loading purchased skins, count: " + Marketplace.PurchasedSkins.Count);

    dartSkins.AddRange(Marketplace.PurchasedSkins);

    // Debug each loaded skin
    foreach (var skin in dartSkins)
    {
        Debug.Log("Loaded skin: " + skin.name);
    }
}

public bool CanHitRow(float yPos)
    {
        return !hitRows.Contains(yPos);
    }

    public void RegisterHitRow(float yPos)
    {
        hitRows.Add(yPos);
    }
public void SpawnDart()
{
    if (freeDarts > 0)
    {
        freeDarts--;  // Use a free dart
    }
    else
    {
        if (coins >= dartPrice)
        {
            coins -= dartPrice;  // Deduct coins for the dart
        }
    }
GameObject dart = Instantiate(dartPrefab, targetCircle.transform.position, Quaternion.identity);
        Dart dartComponent = dart.AddComponent<Dart>();

        // Randomly choose a skin from the available skins
        Sprite chosenSkin = dartSkins[UnityEngine.Random.Range(0, dartSkins.Count)];
        dart.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = chosenSkin; // Assume the child is the visual part

        dartComponent.Initialize(this, spawnPositionsX);
}
public void startGame(string difficulty)
{
    diff_mark = difficulty;
    if(difficulty == "Easy")
    {
        gameprice = 0;
        dartPrice = 100;
        BasicGame.SetActive(true);
        Starter.SetActive(false);
        multiplier = 1;
        StartCoroutine(MoveTargetCircleLinear());
    }
    else if(difficulty == "Medium")
    {
        gameprice = 2500;
        if(coins < gameprice)
        {
            Debug.Log("Not enough coins to play");
        }
        else
        {
            coins -= gameprice;
            dartPrice = 250;
            multiplier = 2;
            BasicGame.SetActive(true);
            Starter.SetActive(false);
            StartCoroutine(MoveTargetCircleInfinity());
        }
    }
    else if(difficulty == "Hard")
    {
        gameprice = 5000;
        if(coins < gameprice)
        {
            Debug.Log("Not enough coins to play");
        }
        else
        {
            coins -= gameprice;
            dartPrice = 500;
            multiplier = 3;
            BasicGame.SetActive(true);
            Starter.SetActive(false);
            StartCoroutine(MoveTargetCircleRandom());
        }
    }
}
private void CalculateWinnings()
    {
        Dictionary<Sprite, int> iconCount = new Dictionary<Sprite, int>();

        // Count each icon
        foreach (Sprite icon in hitIcons)
        {
            if (iconCount.ContainsKey(icon))
            {
                iconCount[icon]++;
            }
            else
            {
                iconCount.Add(icon, 1);
            }
        }

        foreach (var pair in iconCount)
        {
            if (pair.Value >= 2) // At least two icons must be the same
            {
                int iconIndex = Array.IndexOf(icons, pair.Key);
                winnings += iconValues[iconIndex] * pair.Value * multiplier;
            }
        }

        result_text.text = ($"Winnings\n {winnings}");
        coins += winnings;
        PlayerPrefs.SetInt("Total", coins);
    }
    private IEnumerator SpawnSlots(float xPos)
    {
        while (true)
        {
            // Instantiate the slot prefab at position (xPos, 2)
            GameObject newSlot = Instantiate(slotPrefab, new Vector3(xPos, 2, 0), Quaternion.identity);

            // Assign a random icon to the slot
            Sprite randomIcon = icons[UnityEngine.Random.Range(0, icons.Length)];
            newSlot.GetComponent<SpriteRenderer>().sprite = randomIcon;

            // Add the SlotMover component to the new slot to handle movement and destruction
            SlotMover slotMover = newSlot.AddComponent<SlotMover>();
            slotMover.Initialize(moveSpeed, destroyY, this, xPos);
            activeSlots.Add(slotMover);

            // Wait for the specified interval before spawning the next slot
            yield return new WaitForSeconds(spawnInterval);
        }
    }

public void StopAllSlots(float xPosition)
{
    hits++;  // Increment hits
        if (hits == boxes.Length)
        {
            CalculateWinnings();
            ShowResultsPopup();
            if(diff_mark == "Easy")
            {
                StopCoroutine(MoveTargetCircleLinear());
            }
            else if(diff_mark == "Medium")
            {
                StopCoroutine(MoveTargetCircleInfinity());
            }
            else if(diff_mark == "Hard")
            {
                StopCoroutine(MoveTargetCircleRandom());
            }
        }
}
private void ShowResultsPopup()
{
    resultsPopup.SetActive(true);
    BasicGame.SetActive(false);
    Debug.Log($"You won {winnings} coins!");
}
public void ProcessHitIcon(Sprite hitIcon)
{
    hitIcons.Add(hitIcon); // Add the hit icon to the list

    // Update the corresponding box with the new icon
    if (hitIcons.Count <= boxRenderers.Count)
    {
        boxRenderers[hitIcons.Count - 1].sprite = hitIcon;  // Set the sprite of the corresponding box
    }

    // Check if three icons have been hit
    if (hitIcons.Count == 3)
    {
        ShowResultsPopup(); // Show results and end the game
    }
}


public bool CanHitIcon(Sprite icon)
{
    // Check each box to see if the icon is already displayed
    foreach (var box in boxes)
    {
        SpriteRenderer renderer = box.GetComponent<SpriteRenderer>();
        if (renderer.sprite == icon)
        {
            return false;  // Icon already displayed
        }
    }
    return true;  // Icon not yet displayed
}

    public void RemoveSlot(SlotMover slot)
    {
        // Remove slot from the list
        if (activeSlots.Contains(slot))
        {
            activeSlots.Remove(slot);
        }
    }
    private IEnumerator MoveTargetCircleRandom()
{
    while (true)
    {
        Vector3 newPos = new Vector3(UnityEngine.Random.Range(-2f, 2f), UnityEngine.Random.Range(-2f, 2f), 0);
        float time = 0;
        Vector3 startPos = targetCircle.transform.position;
        while (time < 0.3f)
        {
            targetCircle.transform.position = Vector3.Lerp(startPos, newPos, time / 0.5f);
            time += Time.deltaTime;
            yield return null;
        }
        yield return new WaitForSeconds(0.01f);  // Wait for 0.5 seconds before moving again
    }
}
private IEnumerator MoveTargetCircleInfinity()
{
    float time = 0;
    while (true)
    {
        float x = Mathf.Sin(time * targetCircleSpeed) * 2f;
        float y = Mathf.Sin(time * targetCircleSpeed) * Mathf.Cos(time * targetCircleSpeed);
        targetCircle.transform.position = new Vector2(x, y);
        time += Time.deltaTime;
        yield return null;
    }
}
private IEnumerator MoveTargetCircleLinear()
{
    while (true)
    {
        if (isMovingRight)
        {
            targetCircle.transform.Translate(Vector3.right * targetCircleSpeed * Time.deltaTime);
            if (targetCircle.transform.position.x >= targetCircleBounds.y)
            {
                isMovingRight = false;
            }
        }
        else
        {
            targetCircle.transform.Translate(Vector3.left * targetCircleSpeed * Time.deltaTime);
            if (targetCircle.transform.position.x <= targetCircleBounds.x)
            {
                isMovingRight = true;
            }
        }
        yield return null;
    }
}

}

public class SlotMover : MonoBehaviour
{
    private float moveSpeed;  // Speed at which the slot moves down
    private float destroyY;   // Y position at which the slot destroys itself
    private DartSlot dartSlot;  // Reference to DartSlot script
    private bool isMoving = true;  // Flag to check if the slot is moving
    private float xPos;  // X position where the slot was spawned

    public void Initialize(float speed, float destroyPosition, DartSlot dartSlotReference, float spawnX)
    {
        moveSpeed = speed;
        destroyY = destroyPosition;
        dartSlot = dartSlotReference;
        xPos = spawnX;
    }

    private void Update()
    {
        if (isMoving)
        {
            // Move the slot downwards
            transform.Translate(Vector3.down * moveSpeed * Time.deltaTime);

            // Check if the slot has reached the destroy position
            if (transform.position.y <= destroyY)
            {
                // Notify DartSlot to remove this slot from the list
                dartSlot.RemoveSlot(this);
                Destroy(gameObject);  // Destroy the slot
            }
        }
    }

    public void StopMoving()
    {
        isMoving = false;  // Stop the slot from moving
    }

    public float GetXPosition()
    {
        return xPos;  // Return the X position where the slot was spawned
    }
}
