/*******************************************************************************
Author:    Benjamin Ellinger
DP Email:  bellinge@digipen.edu
Date:      3/1/2021
Course:    DES 212

Description:
	This file is part of the framework for the 1D Combat Simulator assignment in
	DES 212 (System Design Methods). It can be freely used and modified by students
	for that assignment.
	
    This component will control the X scale any game object it is added to (even if it
	isn't actually a bar). In particular, it will keep track of the original scale and
	allow other code to just give it the percentage of that original scale that is
	desired (and can interpolate the scale to the new value over time if desired).

*******************************************************************************/

//Standard Unity component libraries
using System.Collections; //Not needed in this file, but here just in case.
using System.Collections.Generic; //Not needed in this file, but here just in case.
using UnityEngine; //The library that lets your access all of the Unity functionality.

//Inherents from MonoBehavior like all normal Unity components do...
//Remember that the class name MUST be identical to the file name!
public class BarScaler : MonoBehaviour
{
	//By making this member variable public, it can automatically be edited in the
	//property inspector.
    public bool Rotated = false; //If the object is rotated, this flag needs to be set

	//All of these variables are used to track the scaling, position, and interpolation
    private float MaxRealScale;
    private Vector3 OriginalPosition;
    private float CurrentScale = 1.0f;
    private float TargetScale = 1.0f;
    private float ScaleTime = 0.0f;
    private float InterpolationTime = 0.0f;

    //Start is called before the first frame update
    void Start()
    {
		//We need to store the original "width" of the object. Note that "localScale" will
		//usually be the property we need for any type of dynamic scaling.
        MaxRealScale = gameObject.transform.localScale.x;

		//We also need to store the original position of the object. Note that "localPosition" will
		//usually be the property we need for any type of dynamic alteration of the position.
        OriginalPosition = gameObject.transform.localPosition;
    }

    //Update is called once per frame
    void Update()
    {
		//Are we actually interpolating to a new scale?
        if (InterpolationTime > 0.0f)
        {
			//Reduce the interpolation time by the delta time, but don't let it go below zero.
            InterpolationTime = Mathf.Clamp(InterpolationTime - Time.deltaTime, 0.0f, 1.0f);
			//If this object is flipped, we need to do things slighty differently, but either way
			//we pass in the interpolated scale (note that the lerp() function could be used here).
            if (Rotated == true)
                SetScaleRotated(TargetScale + ((CurrentScale - TargetScale) * InterpolationTime) / ScaleTime);
            else
                SetScaleStandard(TargetScale + ((CurrentScale - TargetScale) * InterpolationTime) / ScaleTime);
        }
    }

	//Pass in a 0.0 to 1.0 value for the new scale to be set for this frame.
    void SetScaleStandard(float newScale)
    {
		//We create a new Vector3 and only "replace" the X value, then assign the new vector to
		//the localScale property because the individual values are not writable (a common pattern in Unity).
        gameObject.transform.localScale = new Vector3(newScale, gameObject.transform.localScale.y, gameObject.transform.localScale.z);
		//The actual position of the object needs to be moved to the left by half the reduction in scale
		//because we want it to be anchor on the left (while the position is the center).
        float positionAdjustment = (MaxRealScale - newScale) / 2.0f;
		//We create a new Vector3 and only "replace" the X value, then assign the new vector to
		//the localPosition property because the individual values are not writable (a common pattern in Unity).
        gameObject.transform.localPosition = new Vector3(OriginalPosition.x - positionAdjustment, OriginalPosition.y, OriginalPosition.z);
    }

	//Pass in a 0.0 to 1.0 value for the new scale to be set for this frame.
    void SetScaleRotated(float newScale)
    {
 		//We create a new Vector3 and only "replace" the X value, then assign the new vector to
		//the localScale property because the individual values are not writable (a common pattern in Unity).
        gameObject.transform.localScale = new Vector3(newScale, gameObject.transform.localScale.y, gameObject.transform.localScale.z);
		//The actual position of the object needs to be moved to the left by half the reduction in scale
		//because we want it to be anchor on the left (while the position is the center). But because this
		//object (or usually its parent) is rotated, we need to move it in the Y direction (in local space)
		//instead of the X direction.
        float positionAdjustment = (MaxRealScale - newScale) / 2.0f;
 		//We create a new Vector3 and only "replace" the Y value, then assign the new vector to
		//the localPosition property because the individual values are not writable (a common pattern in Unity).
        gameObject.transform.localPosition = new Vector3(OriginalPosition.x, OriginalPosition.y - positionAdjustment, OriginalPosition.z);
    }

	//Pass in a 0.0 to 1.0 value for the new scale and a time in seconds for the interpolation.
	//This function is public because it is designed to be called from other code.
    public void InterpolateToScale(float percent, float time)
    {
		//Store the curren scale for interpolation purposes.
        CurrentScale = gameObject.transform.localScale.x;
		//Determine the non-normalized target scale.
        TargetScale = percent * MaxRealScale;
		//Store the total interpolation time (with some padding in case a 0.0 value is used).
        ScaleTime = time + 0.001f;
		//Set the interpolation time counter.
        InterpolationTime = time + 0.001f;
    }

 	//Pass in a 0.0 to 1.0 value for the new scale which will be set immediately.
	//This function is public because it is designed to be called from other code.
    public void InterpolateImmediate(float percent)
    {
		//Store the curren scale for interpolation purposes.
        CurrentScale = gameObject.transform.localScale.x;
		//Determine the non-normalized target scale.
        TargetScale = percent * MaxRealScale;
		//The total interpolation time is 0 for an immediate call.
        ScaleTime = 0.0f;
		//Make sure this value is 0 as well (in case another interpolation was in progress).
        InterpolationTime = 0.0f;
		//Just set the new scale right now, don't wait for an Update() call.
        if (Rotated == true)
            SetScaleRotated(TargetScale);
        else
            SetScaleStandard(TargetScale);
    }
}
