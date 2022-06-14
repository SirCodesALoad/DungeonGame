using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class controls the player's Stamina it recieves calls to break shields, which it then excutes. 
/// It also hands the Stamina bar, and how long it takes for the player's Stamina bar to refrill, as long as when the Stamina bar is visible and when it is hidden.
/// Afterall we don't always want the Player's Stamina bar on the screen.
/// </summary>

public class StaminaManager : MonoBehaviour
{

    public Player p;
    public StaminaShield[] shields;
    public StaminaShield shield;
    public int stamina = 2;
    public int staminaMax = 2;
    public float staminaRefreshTime = 6f;
    public bool regening = false;

    bool visible = false;

    void Start()
    {

        p = GetComponentInParent<Transform>().GetComponentInParent<Player>();
        shields = new StaminaShield[staminaMax]; //make this a get - return function...

        for(int i = 0; i <= shields.Length-1; i++)
        {
            StaminaShield x = Instantiate(shield, GetComponentInParent<Transform>());
            shields[i] = x;
            
        }
    }


    public bool OutOfStaminaCheck()
    {
        if(stamina <= 0)
        {
            return true;
        }
        return false;
    }

    public void ToggleDisplay()
    {
        visible = !visible;
        if (regening == true)
        {
            //stops the shields from disappearing if they're regenerating..
            return;
        }
        

        for (int i = 0; i <= shields.Length-1; i++)
        {
            shields[i].ToggleVisability();
        }
    }

    void UpdateStaminaMax(int newStamina)
    {
        //Only called when a player gets an increase to their max stamina might never be called
        StopAllCoroutines();
        staminaMax = newStamina;
        for (int i = 0; i >= shields.Length; i++)
        {
            Destroy(shields[i].gameObject);
        }
        
        shields = new StaminaShield[(int)stamina];
        for (int i = 0; i <= shields.Length; i++)
        {
            StaminaShield x = Instantiate(shield, GetComponentInParent<Transform>());
            shields[i] = x;
            shields[i].gameObject.SetActive(visible);
        }
    }

    public void BreakShields(int amount)
    {
        //caused whenever the player takes daamage to their stamina.
        int complete = 0;
        
        StopAllCoroutines();
        Debug.Log("Boop!");
        for (int i = (int)shields.Length-1; i >= -1; i--)
        {
            if (shields[i].broken == false)
            {
                complete++;
                stamina--;
                shields[i].BreakShield();
                
                
            }
            if (complete == amount) {
                StartCoroutine("ShieldRefresh");
                return;
            }
        }

    }

    IEnumerator ShieldRefresh()
    {
        //After a Refresh Peroid Gain Back Stamina..

        regening = true;
        while (stamina < staminaMax)
        {
           
            yield return new WaitForSeconds(staminaRefreshTime);
            if (stamina < staminaMax)
            {
                stamina++;
            }

            for (int i = 0; i <= shields.Length-1; i++)
            {
                if (shields[i].broken == true)
                {
                    
                    shields[i].ResetShield();
                    break;
                }
               
            }
            
        }
        regening = false;

        bool exit = false;

        while (exit == false)
        {
            yield return new WaitForSeconds(2);
            //checks if each stamina shield is animating.
            exit = true;
            foreach (StaminaShield s in  shields)
            {
                if (s.animating == true)
                {
                    exit = false;
                }
            }

        }
        if (visible == false)
        {
            for (int i = 0; i <= shields.Length-1; i++)
            {
                shields[i].ToggleVisability();
            }
        }

    }



}
