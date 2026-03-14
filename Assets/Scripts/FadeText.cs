/*******************************************************************************
Author:    Benjamin Ellinger
DP Email:  bellinge@digipen.edu
Date:      3/1/2021
Course:    DES 212

Description:
	This file is part of the framework for the 1D Combat Simulator assignment in
	DES 212 (System Design Methods). It can be freely used and modified by students
	for that assignment.
	
    This component will fade out the text on any game object it is added to over the
	specified time frame, after a specified delay.

*******************************************************************************/

//Standard Unity component libraries
using System.Collections; //Not needed in this file, but here just in case.
using System.Collections.Generic; //Not needed in this file, but here just in case.
using UnityEngine; //The library that lets your access all of the Unity functionality.
using UnityEngine.UI; //This is here so we don't have to type out longer names for UI compoenents.

//The following statement telling the Unity editor to not allow this component to be
//added to a game object that does not also have a Text component.
[RequireComponent(typeof(Text))]

//Inherents from MonoBehavior like all normal Unity components do...
//Remember that the class name MUST be identical to the file name!
public class FadeText : MonoBehaviour
{
	//By making these member variables public, they can automatically be edited in the
	//property inspector.
    public float TimeUntilFade = 0.0f;
    public float TimeToFade = 1.0f;

	//This varialbe just tracks how long the object has been fading, so it doesn't needed
	//to be public.
    private float TimeFading = 0.0f;

	//We don't need a StartUp() function for this component.

    //Update is called once per frame
    void Update()
    {
		//Time.deltaTime is the amount of time passed since the last update in seconds
		//and is from the UnityEngine library.
        TimeUntilFade -= Time.deltaTime;

		//Not time to start fading yet.
        if (TimeUntilFade > 0.0f)
            return;
		
		//Fading is done, so don't keep trying to fade.
        if (TimeFading > TimeToFade)
            return;

		//The GetComponent<>() function will get the designated component if this
		//component's parent game object has that type of component. Note we don't
		//need to check for a null reference here because of the RequireComponent()
		//statement above.
        Color fadingColor = GetComponent<Text>().color;
		//The color property of the text component has an alpha value "a". Here we
		//we do a simple linear interpolation (there is a lerp() function that could
		//be used instead, if desired).
        fadingColor.a = Mathf.Clamp(1.0f - (TimeFading / TimeToFade), 0.0f, 1.0f);
		//We have to create our own Color variable, change it's alpha value, then
		//assign the whole thing back to the color property of the text object,
		//because the alpha value on the component is not writable. This is a common
		//pattern for Unity components.
        GetComponent<Text>().color = fadingColor;
		//Increment the time while fading.
        TimeFading += Time.deltaTime;
    }
}
