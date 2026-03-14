/*******************************************************************************
Author:    Benjamin Ellinger
DP Email:  bellinge@digipen.edu
Date:      3/1/2021
Course:    DES 212

Description:
	This file is part of the framework for the 1D Combat Simulator assignment in
	DES 212 (System Design Methods). It can be freely used and modified by students
	for that assignment.
	
    This component will "throw" any game object it is added to with the specified
	starting velocities (with some random variation if desired).

*******************************************************************************/

//Standard Unity component libraries
using System.Collections; //Not needed in this file, but here just in case.
using System.Collections.Generic; //Not needed in this file, but here just in case.
using UnityEngine; //The library that lets your access all of the Unity functionality.

//The following statement telling the Unity editor to not allow this component to be
//added to a game object that does not also have a RigidBody2D component.
[RequireComponent(typeof(Rigidbody2D))]
//Inherents from MonoBehavior like all normal Unity components do...
//Remember that the class name MUST be identical to the file name!
public class ThrowObject : MonoBehaviour
{
	//By making these member variables public, they can automatically be edited in the
	//property inspector.
    public float StartingXVelocity = 0.0f;
    public float StartingYVelocity = 1.0f;
    public float RandomXVariation = 0.0f;
    public float RandomYVariation = 0.0f;

    //Start is called before the first frame update
    void Start()
    {
		//The GetComponent<>() function will get the designated component if this
		//component's parent game object has that type of component. Note we don't
		//need to check for a null reference here because of the RequireComponent()
		//statement above.
        GetComponent<Rigidbody2D>().velocity = new Vector2(StartingXVelocity + Random.Range(-RandomXVariation, RandomXVariation), StartingYVelocity + Random.Range(-RandomYVariation, RandomYVariation));
		//The RigidBody2D component has a velocity property we can set.
		//We use a Vector2 here because this is 2D rigid body, but in many other places
		//a Vector3 will be needed even thoigh this is a 2D game.
	}
	
	//Don't need an Update() function for this component--the physics engine will handle
	//it from here.
}
