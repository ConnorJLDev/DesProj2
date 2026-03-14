/*******************************************************************************
Author:    Benjamin Ellinger
DP Email:  bellinge@digipen.edu
Date:      3/1/2021
Course:    DES 212

Description:
	This file is part of the framework for the 1D Combat Simulator assignment in
	DES 212 (System Design Methods). It can be freely used and modified by students
	for that assignment.
	
    This component will destroy any game object it is added to in the specified
	amount of time.

*******************************************************************************/

//Standard Unity component libraries
using System.Collections; //Not needed in this file, but here just in case.
using System.Collections.Generic; //Not needed in this file, but here just in case.
using UnityEngine; //The library that lets your access all of the Unity functionality.

//Inherents from MonoBehavior like all normal Unity components do...
//Remember that the class name MUST be identical to the file name!
public class TimeToDie : MonoBehaviour
{
	//By making this member variable public, it can automatically be edited in the
	//property inspector.
    public float TimeToLive = 10.0f;

	//Don't need a Start() function for this component.

    //Update is called once per frame
    void Update()
    {
		//Time.deltaTime is the amount of time passed since the last update in seconds
		//and is from the UnityEngine library.
        TimeToLive -= Time.deltaTime;

		//Not dead yet.
        if (TimeToLive > 0.0f)
            return;
		
		//gameObject is the parent game object of the component. Game objects can be
		//destroyed by calling the Destroy() function, which is from the UnityEngine
		//library.
        Destroy(gameObject);
    }
}
