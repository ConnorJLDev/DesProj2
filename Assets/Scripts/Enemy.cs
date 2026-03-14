/*******************************************************************************
Author:    Benjamin Ellinger
DP Email:  bellinge@digipen.edu
Date:      2/1/2022
Course:    DES 212

Description:
	This file is part of the framework for the 1D Combat Simulator assignment in
	DES 212 (System Design Methods). It can be freely used and modified by students
	for that assignment.
	
    This component makes a game object an enemy. These are dynamically spawned at
    the start of each round by the SimControl script.
	
*******************************************************************************/

//Standard Unity component libraries
using System.Collections; //Not needed in this file, but here just in case.
using System.Collections.Generic; //Not needed in this file, but here just in case.
using UnityEngine; //The library that lets your access all of the Unity functionality.
using UnityEngine.UI; //This is here so we don't have to type out longer names for UI components.

//Inherits from MonoBehavior like all normal Unity components do...
//Remember that the class name MUST be identical to the file name!
public class Enemy : MonoBehaviour
{
    //Properties for maximum hit points, movement speed, and optimal range.
    //Note that enemies are not as complex as the hero.
    public float MaxHitPoints = 200;
    public float MoveSpeed = 0.1f;
    public float OptimalRange = 5.0f;

    [HideInInspector]
    public float HitPoints = 200; //Current hit points.

    [HideInInspector]
    public Hero Target; //Current target (always the hero in this case).

    [HideInInspector]
    public BarScaler HealthBar; //Reference to the health bar, so we don't have to look it up all the time.

    //References to the abilities, so we don't have to look them up all the time.
    [HideInInspector]
    public EnemyAbility AbilityOne; //Always need this to be a "real" ability.
    [HideInInspector]
    public EnemyAbility AbilityTwo; //This one might be inactive for simple enemies.

    [HideInInspector]
    public float FreezeTimeLeft = 0.0f;

    [HideInInspector]
    public SpriteRenderer SpriteRendererRef;

    public Color NormalColor = Color.white;
    public Color FrozenColor = Color.blue;

    //Start is called before the first frame update
    void Start()
    {
        //Find() will get the first child game object of that name.
        //Use GetComponent so we don't have to use it later to access the functionality we want.
        HealthBar = transform.Find("EnemyHealth").GetComponent<BarScaler>();
        AbilityOne = transform.Find("AbilityOne").GetComponent<EnemyAbility>();
        AbilityTwo = transform.Find("AbilityTwo").GetComponent<EnemyAbility>();
        SpriteRendererRef = GetComponent<SpriteRenderer>();
        if (SpriteRendererRef != null)
            SpriteRendererRef.color = NormalColor;
    }

    //Update is called once per frame
    void Update()
    {
        if (SimControl.RoundOver)
            return;

        if (Target == null)
            Initialize();

        if (FreezeTimeLeft > 0.0f)
        {
            FreezeTimeLeft -= SimControl.DT;

            if (SpriteRendererRef != null)
                SpriteRendererRef.color = FrozenColor;

            if (FreezeTimeLeft <= 0.0f)
            {
                FreezeTimeLeft = 0.0f;

                if (SpriteRendererRef != null)
                    SpriteRendererRef.color = NormalColor;
            }

            return; // frozen enemies do nothing
        }

        DoMovement();
        UseRandomAbility();
    }

    //Try to stay close to optimal range.
    public void DoMovement()
    {
        if (HitPoints <= 0.0f || Target == null) //If the enemy or the player is dead, no need to move.
            return;
        //Calculate distance to target along the X axis (1D not 2D).
        float distanceToTarget = transform.position.x - Target.transform.position.x;
        //If we are between 80% and 100% of optimal range, that's good enough.
        if (Mathf.Abs(distanceToTarget) <= OptimalRange && Mathf.Abs(distanceToTarget) >= OptimalRange * 0.8f)
            return;
        //If we are too close, flip the "distance" so we will move away instead of towards.
        if (Mathf.Abs(distanceToTarget) < OptimalRange * 0.8f)
            distanceToTarget = -distanceToTarget;
        //We need to move, so get our current X position.
        float newX = transform.position.x;
        if (distanceToTarget > 0) //Move to the left.
            newX -= MoveSpeed * SimControl.DT; //Make sure to use the simulated DT.
        else //Move to the right.
            newX += MoveSpeed * SimControl.DT; //Make sure to use the simulated DT.
        //Don't go past the edge of the arena.
        newX = Mathf.Clamp(newX, -SimControl.EdgeDistance, SimControl.EdgeDistance);
        //Update the transform.
        transform.position = new Vector3(newX, transform.position.y, transform.position.z);
    }

    //This is NOT a Start() function because we need to be able to call Initialize() whenever a new
    //round starts, not just when the object is created.
    public void Initialize()
    {
        //No resetting of position because we don't want to override the position they were instansiated at.
        //(Unlike the player who is never actually deleted and needs to be reset every round.)
        //Reset hit points.
        HitPoints = MaxHitPoints;
        FreezeTimeLeft = 0.0f;
        //Reset all the cooldowns.
        if (AbilityOne != null) AbilityOne.ResetCooldown();
        if (AbilityTwo != null) AbilityTwo.ResetCooldown();
        //Find the target.
        Target = GameObject.Find("Hero").GetComponent<Hero>();
        //Make sure the health bar gets reset as well.
        HealthBar.InterpolateImmediate(HitPoints / MaxHitPoints);
        if (SpriteRendererRef != null)
            SpriteRendererRef.color = NormalColor;
    }

    //Try to use a random ability.
    public bool UseRandomAbility()
    {
        //Get a random number between 1 and 2. Yes, the integer version of this function is not
        //inclusive. This is wrong and Unity should feel bad for doing this.
        return UseAbility(Random.Range(1, 3));
    }

    //Try to use a specific ability. Returns whether it actually was used.
    public bool UseAbility(int abilityNumber)
    {
        //We could do this with an array of abilities,
        //but there are only two and we are lazy.
        if (abilityNumber == 1 && AbilityOne != null)
            return AbilityOne.Use();
        if (abilityNumber == 2 && AbilityTwo != null)
            return AbilityTwo.Use();
        return false;
    }

    //Take damage from any source.
    public bool TakeDamage(float damage)
    {
        if (damage != 0.0f) //Don't bother if the damage is 0
        {
            //Accumulate the telemetry data.
            SimControl.DamageDone += Mathf.Min(Mathf.Max(damage,0.0f), HitPoints); //Can't do more damage than the target has HP, and negative damage is actually healing.
            //Make sure hit points do not go negative (or above max, because the "damage" could be negative, i.e., healing).
            HitPoints = Mathf.Clamp(HitPoints - damage, 0.0f, MaxHitPoints);
            //Interpolate the hit point UI bar over half a second.
            HealthBar.InterpolateToScale(HitPoints / MaxHitPoints, 0.5f);
            //Create a temporary InfoText object to show the damage using the static Instantiate() function.
            Text damageText = Object.Instantiate(SimControl.InfoTextPrefab, transform.position, Quaternion.identity, SimControl.Canvas.transform).GetComponent<Text>();
            //Set the damage text to just the integer amount of the damage done.
            //Uses the "empty string plus number" trick to make it a string.
            damageText.text = "" + Mathf.Floor(damage);
        }
        //Return true if dead.
        return (HitPoints <= 0.0f);
    }
    public void ApplyFreeze(float duration)
    {
        FreezeTimeLeft = Mathf.Max(FreezeTimeLeft, duration);

        if (SpriteRendererRef != null)
            SpriteRendererRef.color = FrozenColor;
    }
}
