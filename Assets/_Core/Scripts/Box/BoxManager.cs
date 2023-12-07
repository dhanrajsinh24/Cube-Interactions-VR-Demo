using System;
using System.Collections;
using UnityEngine;

public class BoxManager : MonoBehaviour
{
    [SerializeField] private BoxObject boxObject;
    private Transform _boxTransform;

    /// <summary>
    /// Specifies if the box is available or not
    /// </summary>
    public static bool isBoxCreated;
    
    /// <summary>
    /// Specifies if the box is grabbed or not
    /// </summary>
    public static bool isBoxHeld;

    private Vector3 _startPosition;
    private Quaternion _startRotation;
    private Vector3 _startScale;
    
    private Transform _firstCubeTransform;

    private void Awake()
    {
        _boxTransform = boxObject.transform;
        _startPosition = _boxTransform.position;
        _startRotation = _boxTransform.rotation;
        _startScale = _boxTransform.localScale;
    }

    private void OnEnable()
    {
        CustomInteractManager.OnCubeStuckToBox += CubeStuckToBox;
        CustomInteractManager.CubeUnstuck += CubeUnstuckFromBox;
    }

    private void OnDisable()
    {
        CustomInteractManager.OnCubeStuckToBox -= CubeStuckToBox;
        CustomInteractManager.CubeUnstuck -= CubeUnstuckFromBox;
    }
    
    private void CubeStuckToBox(CubeObject parentCube, CubeObject childCube)
    {
        //#6 - Cube should be added to box
        Debug.Log("Box has cube stuck");
        StartCoroutine(StartAddingToBox(parentCube, childCube));
    }
    
    private IEnumerator StartAddingToBox(CubeObject parentCube, CubeObject childCube)
    {
        //First deactivate the box so it is ungrabed if grabed
        boxObject.ActivateBox(false);
        
        yield return null;

        if (!isBoxCreated)
        {
            isBoxCreated = true;
            
            AssignFirstParentPose(parentCube.transform);
            
            //#7 - TODO - box is not created so create it
            InitializeBox();

            yield return StartCoroutine(boxObject.AddCubeInBox(parentCube));
            yield return StartCoroutine(boxObject.AddCubeInBox(childCube));
        }
        else
        {
            Debug.Log("Box already created");
   
            InitializeBox();
        
            //Only add child because the parent should already be in the box
            yield return StartCoroutine(boxObject.AddCubeInBox(childCube));
        }

        //Remove parent child relationship completely
        yield return StartCoroutine(RemoveParentSource(childCube));

        //Every actions are done to stick the cube
        //Now we can show the box and activate it
        boxObject.ActivateBox();
    }

    private void AssignFirstParentPose(Transform cubeTransform)
    {
        if (_firstCubeTransform == null)
        {
            _firstCubeTransform = cubeTransform;
        }
    }

    private static IEnumerator RemoveParentSource(CubeObject childCube)
    {
        yield return null;
        
        //Parent-child relationship should be off now
        var parentConstraint = childCube.ParentConstraint;
   
        parentConstraint.constraintActive = false;
        //Remove parent source
        parentConstraint.RemoveSource(0);

        yield return null;
    }

    private void CubeUnstuckFromBox(CubeObject cubeObject)
    {
        Debug.Log("Cube unstuck from the Box");

        var isBox = boxObject.RemoveCubeFromBox(cubeObject);
        
        //#10 - TODO - remove box if no cubes are inside it
        if(isBox) RemoveBox();
    }

    /// <summary>
    /// It re positions the box and then
    /// enables it
    /// </summary>
    private void InitializeBox()
    {
        var boxTransform = boxObject.transform;
        
        //Set box position and rotation to the first cube so
        //it can be scaled
        boxTransform.SetPositionAndRotation(_firstCubeTransform.position, _firstCubeTransform.rotation);
    }

    private void RemoveBox()
    {
        Debug.Log("Hide box");
        //(when two remaining cubes are separated)
        //Make sure all cubes are deparented before calling this func
        isBoxCreated = false;
        isBoxHeld = false;
        
        _boxTransform.SetPositionAndRotation(_startPosition, _startRotation);
        _boxTransform.localScale = _startScale;

        boxObject.ActivateBox(false);

        _firstCubeTransform = null;
    }
}
