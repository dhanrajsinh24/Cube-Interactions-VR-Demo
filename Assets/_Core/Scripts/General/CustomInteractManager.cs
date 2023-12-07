using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

/// <summary>
/// Managers all interactions that the controllers / hands can do
/// </summary>
public class CustomInteractManager : MonoBehaviour
{
    public static CustomInteractManager Instance;

    //Used to detect the attachment by detecting which side is near which side
    private readonly List<AnchorSide> _anchorSidePair1 = new ();
    private readonly List<AnchorSide> _anchorSidePair2 = new ();
    
    public static event Action<CubeObject, CubeObject> OnCubeStuckToBox;
    public static Action<CubeObject> CubeUnstuck;
    
    private Vector3 _targetPositionToStick;
    private CubeObject _parentCube;
    private CubeObject _childCube;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// Adding anchor pair when it's near other achor pair
    /// </summary>
    /// <param name="one"></param>
    /// <param name="two"></param>
    public void TryAddingAnchorPair(AnchorSide one, AnchorSide two)
    {
        //If the same anchor are triggered again then below two conditions 
        //Can be true
        if (_anchorSidePair1.Count != 0 && 
            _anchorSidePair1[0].Equals(two) && 
            _anchorSidePair1[0].ID.Equals(two.ID))
        {
            //Debug.Log("The pair is already added");
            return;
        }
        if (_anchorSidePair2.Count != 0 && 
            _anchorSidePair2[0].Equals(two)  && 
            _anchorSidePair2[0].ID.Equals(two.ID))
        {
            //Debug.Log("The pair is already added");
            return;
        }
        
        //If the cubes are already stuck there is no need to stick
        //them again
        if (one.CubeObject.IsStuckInTheBox && two.CubeObject.IsStuckInTheBox)
        {
            //Debug.Log("Cubes are already stuck!");
            _anchorSidePair1.Clear();
            _anchorSidePair2.Clear();
            return;
        }
        
        //Add to first list
        if (_anchorSidePair1.Count == 0)
        {
            _anchorSidePair1.Add(one);
            _anchorSidePair1.Add(two);
            
            //Disable all anchor points except the current side
            one.CubeObject.DisableOtherAnchorPoints(one);
            two.CubeObject.DisableOtherAnchorPoints(two);
            
            //Debug.Log(one.CubeObject.gameObject.name+one.gameObject.name + " collided with "
            //          + two.CubeObject.gameObject.name+two.gameObject.name);
        }
        else if (_anchorSidePair2.Count == 0) //add to second list
        {
            _anchorSidePair2.Add(one);
            _anchorSidePair2.Add(two);
            
            //Disable all anchor points except the current side
            one.CubeObject.DisableOtherAnchorPoints(one);
            two.CubeObject.DisableOtherAnchorPoints(two);
            
            //Debug.Log(one.CubeObject.gameObject.name+one.gameObject.name + " collided with "
            //          + two.CubeObject.gameObject.name+two.gameObject.name);
        }

        //#3 - TODO
        //If two pairs are added then check for sticking
        if (_anchorSidePair1.Count != 0 && _anchorSidePair2.Count != 0)
        {
            ValidateAnchorMagnet();
        }
    }
    
    /// <summary>
    /// Removed anchor pair which are far after they were near
    /// </summary>
    /// <param name="one"></param>
    /// <param name="two"></param>
    public void TryRemovingAnchorPair(AnchorSide one, AnchorSide two)
    {
        if (_anchorSidePair1.Contains(one) && _anchorSidePair1.Contains(two))
        {
            _anchorSidePair1.Clear();
            //Debug.Log( one.gameObject.name+ " separated with "+two.gameObject.name);
            one.CubeObject.ToggleAnchorPoints(true);
            two.CubeObject.ToggleAnchorPoints(true);
            one.CubeObject.IsReadyToBeStuck = false;
            two.CubeObject.IsReadyToBeStuck = false;
            _stopVibrate = true;
        }
        else if(_anchorSidePair2.Contains(one) && _anchorSidePair2.Contains(two))
        {
            _anchorSidePair2.Clear();
            //Debug.Log( one.gameObject.name+ " separated with "+two.gameObject.name);
            one.CubeObject.ToggleAnchorPoints(true);
            two.CubeObject.ToggleAnchorPoints(true);
            one.CubeObject.IsReadyToBeStuck = false;
            two.CubeObject.IsReadyToBeStuck = false;
            _stopVibrate = true;
        }
    }

    /// <summary>
    /// Checks whether the cubes can attach or not
    /// </summary>
    private void ValidateAnchorMagnet()
    {
        Debug.Log("ValidateAnchorMagnet");

        //Check if the two anchor side pairs represents the same sides for
        //both the cubes which are near
        var firstSame = _anchorSidePair1[0].Equals(_anchorSidePair2[0]) &&
                        _anchorSidePair1[1].Equals(_anchorSidePair2[1]);
        var secondSame = _anchorSidePair1[0].Equals(_anchorSidePair2[1]) &&
                        _anchorSidePair1[1].Equals(_anchorSidePair2[0]);

        //Side pairs mismatch 
        if (!firstSame && !secondSame)
        {
            //Debug.Log("Pairs are not eligible for attaching.");
            _anchorSidePair1.Clear();
            _anchorSidePair2.Clear();
            return;
        }
        
        var cube1 = _anchorSidePair1[0].CubeObject;
        var cube2 = _anchorSidePair1[1].CubeObject;
        
        //If both the cubes are already stuck in the box then
        //don't do anything
        if (cube1.IsStuckInTheBox && cube2.IsStuckInTheBox)
        {
            //Debug.Log("Cubes are already stuck!");
            _anchorSidePair1.Clear();
            _anchorSidePair2.Clear();
            return;
        }

        var firstCubeIsParent = false;
   
        //If the box is already created
        if (cube1.IsStuckInTheBox || cube2.IsStuckInTheBox)
        {
            firstCubeIsParent = cube1.IsStuckInTheBox;
        }

        _parentCube = firstCubeIsParent ? cube1 : cube2;
        _childCube = firstCubeIsParent ? cube2 : cube1;

        _childCube.IsReadyToBeStuck = true;
        
        //Target position to stick
        var targetSide = firstCubeIsParent ? _anchorSidePair1[0] : _anchorSidePair1[1];
        _targetPositionToStick = targetSide.transform.localPosition;

        //These are not needed anymore so must be cleared
        _anchorSidePair1.Clear();
        _anchorSidePair2.Clear();

        //#4 - TODO - Ready to stick
        //Start vibration
        StartCoroutine(InvokeVibrate());
        
        //Debug.Log("Cubes are now ready to be stuck");
    }

    public void ConnectCubes()
    {
        _stopVibrate = true;
        
        Debug.Log(_childCube.gameObject.name + " is sticking with "
                                             +_parentCube.gameObject.name);

        //Change parameters for Stuck cubes
        PutCubeInBox(_parentCube, _childCube);

        //Start the connecting process now
        StartCoroutine(StartConnectingCubes(_parentCube, _childCube, _targetPositionToStick));
    }

    private bool _stopVibrate;
    private IEnumerator InvokeVibrate()
    {
        _stopVibrate = false;
        //starts vibration on controllers and hands
        OVRInput.SetControllerVibration(1, 0.5f, OVRInput.Controller.Touch);
        //OVRInput.SetControllerVibration(1, 0.5f, OVRInput.Controller.Hands);

        while (!_stopVibrate) yield return null;

        //stops vibration on controllers and hands
        OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.Touch);
        //OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.Hands);
    }

    /// <summary>
    /// Start connecting cubes by using Parent Constraint
    /// </summary>
    /// <param name="parentCube"></param>
    /// <param name="childCube"></param>
    /// <param name="position"></param>
    /// <returns></returns>
    private static IEnumerator StartConnectingCubes(CubeObject parentCube, 
        CubeObject childCube, Vector3 position)
    {
        //Rotation subtraction
        var rotOffset = Quaternion.Inverse(parentCube.transform.rotation) * childCube.transform.rotation;
        
        var rotEuler = rotOffset.eulerAngles;
        
        //Rotation is rounded to *90
        var rotEulerRounded = new Vector3(RoundToNintyRotation(rotEuler.x),
            RoundToNintyRotation(rotEuler.y), RoundToNintyRotation(rotEuler.z));

        //Parent constraint is used to position and rotation child
        //Then it is disabled once the cube is in the box
        var parentConstraint = childCube.ParentConstraint;
        //1 is parent, 2 is child
        parentConstraint.AddSource(new ConstraintSource
        {
            sourceTransform = parentCube.transform, weight = 1
        });

        //Set position and rotation offset for the child
        parentConstraint.SetTranslationOffset(0, new Vector3(
            position.x / 5f,position.y / 5f,position.z / 5f));
        
        //Set rotation offset
        parentConstraint.SetRotationOffset(0, rotEulerRounded);
        
        parentConstraint.constraintActive = true;
        
        yield return null;
        yield return null;
        
        OnCubeStuckToBox?.Invoke(parentCube, childCube);
    }

    /// <summary>
    /// Set all parameters needed when the cubes joins / detaches
    /// </summary>
    /// <param name="parentCube"></param>
    /// <param name="childCube"></param>
    private static void PutCubeInBox(CubeObject parentCube, CubeObject childCube)
    {
        parentCube.ToggleStuckInBox(true);
        childCube.ToggleStuckInBox(true);
    }

    /// <summary>
    /// Used to make Rotation rounded to near to multiple of 90
    /// </summary>
    /// <param name="inputRot"></param>
    /// <returns></returns>
    private static float RoundToNintyRotation(float inputRot)
    {
        return Mathf.Round(inputRot / 90) * 90;
    }

    private void OnApplicationQuit()
    {
        Instance = null;
    }
}
