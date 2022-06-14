using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PowerTools;
using UnityEngine.UI;

/// <summary>
/// This class controls the inderival Shield objects that exist within the Stamina Manager and The HUD element above the player.
/// When the player takes damage and they block it, we may want to break a shield.
/// StaminaManager tells each inderivudal shield to break, so it is possible for multiple shields to break at once.
/// This code deals with only a single Stamina Shield. 
/// </summary>

public class StaminaShield : MonoBehaviour
{

    public Image image;
    public Sprite[] BreakShieldSprites;
    public float animationSpeed;
    public Sprite[] ResetShieldSprites;
    public bool broken = false;
    public bool animating = false;

    private void Start()
    {
        image = GetComponent<Image>();
        image.enabled = false;
    }


    public void ToggleVisability()
    {
        image.enabled = !image.enabled;
    }
    
    
    public void BreakShield()
    {
        StopAllCoroutines();
        StartCoroutine("BreakShieldRoutine");
    }

    public void ResetShield()
    {
        StopAllCoroutines();
        StartCoroutine("ResetShieldRoutine");
    }

    protected IEnumerator BreakShieldRoutine()
    {
        broken = true;
        animating = true;
        
        for (int i = 0; i < BreakShieldSprites.Length; i++)
        {
            image.sprite = BreakShieldSprites[i];
            yield return new WaitForSeconds(animationSpeed/BreakShieldSprites.Length);
        }
        animating = false;
    }

    protected IEnumerator ResetShieldRoutine()
    {
        
        broken = false;
        animating = true;
        for (int i = 0; i < ResetShieldSprites.Length; i++)
        {
            image.sprite = ResetShieldSprites[i];
            yield return new WaitForSeconds(animationSpeed / ResetShieldSprites.Length);
        }
        animating = false;


    }

}
