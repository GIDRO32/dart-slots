using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Dart : MonoBehaviour
{
    private DartSlot dartSlot;
    private Rigidbody2D rb;
    public float[] xPositions = {-1.2f,0,1.2f};  // Array of X positions corresponding to slot positions
    private HashSet<float> hitRows = new HashSet<float>();
    private SpriteRenderer spriteRenderer;

    public void Initialize(DartSlot dartSlotReference, float[] positions)
    {
        dartSlot = dartSlotReference;
        xPositions = positions;
        rb = gameObject.GetComponent<Rigidbody2D>() ?? gameObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.isKinematic = true;
        GetComponent<Collider2D>().isTrigger = true;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

private void OnTriggerEnter2D(Collider2D collision)
{
    if (collision.CompareTag("Icon")) 
    {
        StopCoroutine(FadeOutAndDestroy());  // Stop any ongoing fade out
        GetComponent<Collider2D>().enabled = false;  // Disable the collider immediately upon hitting an icon

        SpriteRenderer collidedIconSpriteRenderer = collision.GetComponent<SpriteRenderer>();
        if (collidedIconSpriteRenderer != null && dartSlot.CanHitRow(collision.transform.position.y))  // Valid target hit
        {
            Sprite collidedSprite = collidedIconSpriteRenderer.sprite;
            dartSlot.ProcessHitIcon(collidedSprite);  // Process the icon that was hit
            Debug.Log($"Dart hit icon with sprite: {collidedSprite.name}");
            float closestPosition = FindClosestXPosition(collision.transform.position.x);
            dartSlot.StopAllSlots(closestPosition);
            dartSlot.RegisterHitRow(collision.transform.position.y);
        }
    }
    else if(collision.CompareTag("Area") && collision.CompareTag("Area"))
    {
        Debug.Log("Miss!");
    }
    StartCoroutine(FadeOutAndDestroy());  // Start fading out if the hit was not valid
}


IEnumerator FadeOutAndDestroy()
{
    float fadeDuration = 1f; // Duration in seconds over which the dart will fade out
    float startTime = Time.time;
    Color originalColor = spriteRenderer.color;

    // Access the child SpriteRenderer
    SpriteRenderer childSpriteRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
    Color childOriginalColor = childSpriteRenderer.color;

    while (Time.time < startTime + fadeDuration)
    {
        float t = (Time.time - startTime) / fadeDuration;
        float alpha = Mathf.Lerp(1, 0, t);

        // Apply fading to the main dart's sprite
        spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);

        // Apply the same fading to the child dart's sprite
        childSpriteRenderer.color = new Color(childOriginalColor.r, childOriginalColor.g, childOriginalColor.b, alpha);

        yield return null;
    }

    Destroy(gameObject);
}

    private float FindClosestXPosition(float hitX)
    {
        float closest = xPositions[0];
        float minDistance = Mathf.Abs(hitX - closest);

        foreach (float pos in xPositions)
        {
            float distance = Mathf.Abs(hitX - pos);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = pos;
            }
        }
        return closest;
    }
}


