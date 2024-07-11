using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class LevelHandle : MonoBehaviour
{
    public GameObject slotPrefab;  // The Slot prefab to be instantiated
    public Sprite[] icons;         // Array of sprites to be used as icons

    public float spawnInterval = 0.3f;  // Interval between spawns
    public float moveSpeed = 3f;        // Speed at which slots move down
    public float destroyY = -2f;        // Y position at which slots destroy themselves

    public float[] spawnPositionsX = { -1.5f, 0f, 1.5f };  // Array of X positions for spawning

    private List<SlotSpinner> activeSlots = new List<SlotSpinner>();  // List to keep track of active slots
    private Dictionary<float, Coroutine> spawnCoroutines = new Dictionary<float, Coroutine>();  // Reference to the spawn coroutines for each X position
public GameObject targetCircle;  // Reference to the target circle
public GameObject dartPrefab;    // Reference to the dart prefab

public float targetCircleSpeed = 2f;  // Speed of the target circle movement
public Vector2 targetCircleBounds = new Vector2(-3f, 3f);  // Bounds for target circle movement

private bool isMovingRight = true;  // Direction flag for the target circle
private HashSet<float> hitRows = new HashSet<float>();

    public GameObject BasicGame;
    public GameObject Starter;
    public GameObject[] boxes = new GameObject[3];  // Array to store references to the box sprites
    public GameObject resultsPopup;  // Reference to the results popup GameObject
    private int hits = 0;  // Track the number of successful hits
    public List<SpriteRenderer> boxRenderers;  // List to store references to the box sprite renderers
    public List<Sprite> hitIcons = new List<Sprite>();
    public Text result_text;
    public List<Sprite> dartSkins = new List<Sprite>(); // List to hold purchased dart skins
    public Button[] levelButtons;
public Sprite[] icon_objectives;
public GameObject objectivePopup;  // Popup that shows the level objective
public Image objectiveIcon;  // Icon displayed in the objective popup
public Sprite currentObjectiveIcon; // Variable to hold the current level's objective icon
public AudioSource sound_effects;
public AudioClip jingle;
public Sprite selectedSkin;
public Sprite defaultSkin;
private void Start()
    {
        Time.timeScale = 1f;
        LoadPurchasedSkins();
        LoadSelectedSkin();
        Starter.SetActive(true);
        BasicGame.SetActive(false);
        resultsPopup.SetActive(false);
        foreach (float xPos in spawnPositionsX)
        {
            spawnCoroutines[xPos] = StartCoroutine(SpawnSlots(xPos));
        }
    }
void LoadPurchasedSkins()
{
    dartSkins.Clear(); // Clear to avoid duplication

    Debug.Log("Loading purchased skins, count: " + Marketplace.PurchasedSkins.Count);

    dartSkins.AddRange(Marketplace.PurchasedSkins);
}
void LoadSelectedSkin()
{
    string selectedSkinName = PlayerPrefs.GetString("SelectedSkin", "");
    if (!string.IsNullOrEmpty(selectedSkinName))
    {
        foreach (var item in Marketplace.Instance.ShopItemList)
        {
            if (item.name == selectedSkinName && item.isAvailable)
            {
                selectedSkin = item.Image;
                break;
            }
        }
    }
    else
    {
        selectedSkin = defaultSkin;
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
    GameObject dart = Instantiate(dartPrefab, targetCircle.transform.position, Quaternion.identity);
    Dart dartComponent = dart.AddComponent<Dart>();

    // Use the selected skin if available, otherwise use default
    SpriteRenderer dartRenderer = dart.transform.GetChild(0).GetComponent<SpriteRenderer>();
    dartRenderer.sprite = selectedSkin;

    dartComponent.Initialize2(this, spawnPositionsX);
    if (selectedSkin != null)
    {
        dartRenderer.sprite = selectedSkin;
    }
    else
    {
        Debug.LogWarning("No skin selected, using default settings.");
        // Optionally apply a default sprite or handle this case as needed
    }
}

public void ShowObjective(int levelIndex)
{
    if (levelIndex < icon_objectives.Length)
    {
        objectiveIcon.sprite = icon_objectives[levelIndex]; // Update the UI with the objective icon
        currentObjectiveIcon = icon_objectives[levelIndex];  // Set the current objective
        Starter.SetActive(false);
        objectivePopup.SetActive(true);
    }
}
public void startGame()
{
        BasicGame.SetActive(true);
        StartCoroutine(MoveTargetCircleLinear());
}
void LoadLevels()
{
    for (int i = 1; i < levelButtons.Length; i++)
    {
        if (PlayerPrefs.GetInt("LevelUnlocked_" + i, 0) == 1)
        {
            levelButtons[i].enabled = true;
        }
    }
}

public void ResetLevelProgress()
{
    for (int i = 1; i < levelButtons.Length; i++)
    {
        levelButtons[i].enabled = false;
        PlayerPrefs.SetInt("LevelUnlocked_" + i, 0);
    }
    PlayerPrefs.Save();
}
public void CheckLevelCompletion()
{
    bool allMatch = true;
    foreach (SpriteRenderer renderer in boxRenderers)
    {
        if (renderer.sprite != currentObjectiveIcon)
        {
            result_text.text = "Level Failed\nTry again!";
            allMatch = false;
            break;
        }
    }

    if (allMatch)
    {
        sound_effects.PlayOneShot(jingle);
        result_text.text = "Well Done!";
        int currentLevel = Array.IndexOf(icon_objectives, currentObjectiveIcon);
        if (currentLevel + 1 < levelButtons.Length)
        {
            levelButtons[currentLevel + 1].enabled = true;
            PlayerPrefs.SetInt("LevelUnlocked_" + (currentLevel + 1), 1);
            PlayerPrefs.Save();
        }
        ShowResultsPopup(true); // Assuming a method that handles showing a success popup
    }
    else
    {
        ShowResultsPopup(false); // Assuming a method that handles showing a failure popup
    }
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

            // Add the SlotSpinner component to the new slot to handle movement and destruction
            SlotSpinner slotMover = newSlot.AddComponent<SlotSpinner>();
            slotMover.Initialize(moveSpeed, destroyY, this, xPos);
            activeSlots.Add(slotMover);

            // Wait for the specified interval before spawning the next slot
            yield return new WaitForSeconds(spawnInterval);
        }
    }

public void StopAllSlots(float xPosition)
{
    if (spawnCoroutines.ContainsKey(xPosition))
        {
            StopCoroutine(spawnCoroutines[xPosition]);
            spawnCoroutines.Remove(xPosition);
        }
    foreach (SlotSpinner slot in activeSlots)
        {
            if (slot != null && Mathf.Approximately(slot.GetXPosition(), xPosition))
            {
                slot.StopMoving();
            }
        }
    // hits++;  // Increment hits
}
private void ShowResultsPopup(bool success)
{
    resultsPopup.SetActive(true);
    BasicGame.SetActive(false);
    Time.timeScale = 0f;
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
        CheckLevelCompletion();
        ShowResultsPopup(true); // Show results and end the game
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

    public void RemoveSlot(SlotSpinner slot)
    {
        // Remove slot from the list
        if (activeSlots.Contains(slot))
        {
            activeSlots.Remove(slot);
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

public class SlotSpinner : MonoBehaviour
{
    private float moveSpeed;  // Speed at which the slot moves down
    private float destroyY;   // Y position at which the slot destroys itself
    private LevelHandle dartSlot;  // Reference to DartSlot script
    public bool isMoving = true;  // Flag to check if the slot is moving
    private float xPos;  // X position where the slot was spawned

    public void Initialize(float speed, float destroyPosition, LevelHandle dartSlotReference, float spawnX)
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
