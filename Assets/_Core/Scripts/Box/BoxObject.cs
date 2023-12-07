using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Oculus.Interaction;
using UnityEngine;

public class BoxObject : MonoBehaviour
{
    public GameObject Renderer;
    
    private const float CubeSize = 0.1f;

    /// <summary>
    /// Extra box size in both side of cubes
    /// </summary>
    private const float ExtraBoxSize = 0.06f;
    
    /// <summary>
    /// All cubes the box currently have
    /// </summary>
    [SerializeField]private List<CubeObject> _cubesInBox = new ();

    private enum Side { None, X, Y, Z }

    private class SideContainer
    {
        public readonly Side MySide;
        public readonly float MyPos;

        public SideContainer(Side mySide, float myPos)
        {
            MySide = mySide;
            MyPos = myPos;
        }
    }

    private readonly Dictionary<Transform, SideContainer> _cubeSides = new ();

    private Collider Collider { get; set; }

    private Grabbable Grabbable { get; set; }
    
    private Coroutine _makeSizeCoroutine;

    private void Start()
    {
        Collider = GetComponent<Collider>();
        Grabbable = GetComponent<Grabbable>();
    }

    public void ActivateBox(bool show = true)
    {
        //isBoxHeld = show;
        Renderer.gameObject.SetActive(show);
        Grabbable.enabled = show;
        Collider.enabled = show;
    }

    //If the box does not already contain 
    //The cube, add it
    public IEnumerator AddCubeInBox(CubeObject cube)
    {
        //This is a problem should not happen
        if (_cubesInBox.Contains(cube))
        {
            Debug.Log("Oops, why is the AddCubeInBox called for this cube?");
            yield break;
        }

        _cubesInBox.Add(cube);

        IgnoreCollision(cube);
        
        //#8 - TODO - box size and position calculation
        yield return StartCoroutine(MakeSize(cube.transform));
    }

    private void IgnoreCollision(CubeObject cube)
    {
        //ignore collision between new cube to the box cubes
        foreach (var item in 
                 _cubesInBox.Where(item => !item.Equals(cube)))
        {
            cube.IgnoreCollisionWithOther(item, true);
        }
    }
    
    /// <summary>
    /// Determines which side the box will expand
    /// </summary>
    private void DetermineBoxSizeSide(Transform cubeTransform)
    {
        //calculate box size on the anchor side cubes
        if (_cubeSides.Count == 0)
        {
            //This is the first cube so transform side is None
            _cubeSides.Add(cubeTransform, new SideContainer(Side.None, 0));
        }
        else
        {
            var cubePosition = cubeTransform.localPosition;
            var refPosition = _cubeSides.First().Key.localPosition;

            //X side
            var xDiff = Mathf.Abs(cubePosition.x - refPosition.x);
            var yDiff = Mathf.Abs(cubePosition.y - refPosition.y);
            var zDiff = Mathf.Abs(cubePosition.z - refPosition.z);
            
            var sideFinal = false;
            if (xDiff > 0.09f)
            {
                var isThisPosAdded = IsThisPositionAlreadyAdded(Side.X, xDiff);
                Debug.Log("x: "+isThisPosAdded);
                
                if (!isThisPosAdded)
                {
                    sideFinal = true;
                    _cubeSides.Add(cubeTransform, new SideContainer(Side.X, xDiff));
                }
            }

            if (sideFinal) return;
            //Y side
            if (yDiff > 0.09f)
            {
                var isThisPosAdded = IsThisPositionAlreadyAdded(Side.Y, yDiff);
                Debug.Log("y: "+isThisPosAdded);
                
                if (!isThisPosAdded)
                {
                    sideFinal = true;
                    _cubeSides.Add(cubeTransform, new SideContainer(Side.Y, yDiff));
                }
            }
            if (sideFinal) return;
            //Z side
            if (zDiff > 0.09f)
            {
                var isThisPosAdded = IsThisPositionAlreadyAdded(Side.Z, zDiff);
                Debug.Log("z: "+isThisPosAdded);
                if (!isThisPosAdded)
                {
                    _cubeSides.Add(cubeTransform, new SideContainer(Side.Z, zDiff));
                }
            }
        }
    }

    /// <summary>
    /// Function check if the position and side (x / y / z)
    /// is already added to cubeSides
    /// </summary>
    /// <param name="side"></param>
    /// <param name="diff"></param>
    /// <returns></returns>
    private bool IsThisPositionAlreadyAdded(Side side, float diff)
    {
        foreach (var item in _cubeSides)
        {
            var thresholdDistance = CubeSize * 0.1f;
            bool thresholdMet;
            var netDiff = item.Value.MyPos - diff;
            if (netDiff < 0)
            {
                thresholdDistance *= -1;
                thresholdMet = netDiff > thresholdDistance;
            }
            else
            {
                thresholdMet = netDiff < thresholdDistance;
            }
            if (item.Value.MySide == side && thresholdMet) return true;
        }

        return false;
    }

    public bool RemoveCubeFromBox(CubeObject cube)
    {
        Debug.Log("RemoveCubeFromBox: "+cube.gameObject.name);
        Deparent(cube);

        if (_cubesInBox.Count != 1)
        {
            return false;
        }
        
        //Only one element so
        //Box will need to be disabled
        Debug.Log("Box will be disabled");
        Deparent(_cubesInBox[0]);
        return true;
    }

    private void Deparent(CubeObject cube)
    {
        Debug.Log("De-parent cube: "+cube.gameObject.name);
        _cubesInBox.Remove(cube);
        _cubesInBox.TrimExcess();
        
        _cubeSides.Remove(cube.transform);
        _cubeSides.TrimExcess();
        
        //start collision between this cube to the box cubes
        foreach (var item in 
                 _cubesInBox.Where(item => !item.Equals(cube)))
        {
            cube.IgnoreCollisionWithOther(item, false);
        }
        
        //Un-parent from box
        cube.transform.SetParent(null);
    }

    private IEnumerator MakeSize(Transform cubeTransform)
    {
        //Hide the box
        Renderer.gameObject.SetActive(false);
        
        //first get all cubes out of the box
        foreach (var item in _cubesInBox)
        { 
            item.transform.SetParent(null);
        }

        //Make the box size 1
        transform.localScale = Vector3.one;
 
        //Put cubes in the box
        foreach (var item in _cubesInBox)
        {
            item.transform.SetParent(transform);
        }

        yield return null;
 
        //Check which side this cube is from the first cube
        DetermineBoxSizeSide(cubeTransform);
  
        //Out of the box
        foreach (var item in _cubesInBox)
        { 
            item.transform.SetParent(null);
        }
        
        //Skip the frame
        yield return null;

        //Set pose of the box (pos and rot)
        ApplyNewPoseToBox();
  
        //Put cubes in the box again
        foreach (var item in _cubesInBox)
        {
            item.transform.SetParent(transform);
        }

        //Show the box now
        Renderer.gameObject.SetActive(true);
    }

    /// <summary>
    /// Sets the box position and rotation based on saved parameters
    /// </summary>
    private void ApplyNewPoseToBox()
    {
        const float thresholdDistance = CubeSize * 0.1f;
        List<float> xPos = new (), yPos = new (), zPos = new ();
        foreach (var pos in 
                 _cubeSides.Select(side => side.Key.position))
        {
            //Check if the list already has the position
            if (!xPos.Any(x => Mathf.Abs(x - pos.x) < thresholdDistance)) 
                xPos.Add(pos.x);
            if (!yPos.Any(y => Mathf.Abs(y - pos.y) < thresholdDistance)) 
                yPos.Add(pos.y);
            if (!zPos.Any(z => Mathf.Abs(z - pos.z) < thresholdDistance)) 
                zPos.Add(pos.z);
        }

        transform.localPosition = new Vector3(xPos.Sum() / xPos.Count,
            yPos.Sum() / yPos.Count, zPos.Sum() / zPos.Count);
        
        //Scale
        transform.localScale = new Vector3(BoxSideSize(Side.X), 
            BoxSideSize(Side.Y), BoxSideSize(Side.Z));
    }

    private float BoxSideSize(Side side)
    {
        var numOfCubes = _cubeSides.Count(x => x.Value.MySide.Equals(side));
        var sideSize = CubeSize * numOfCubes + ExtraBoxSize + CubeSize;
        if (sideSize <= 0.12f) sideSize = 0.12f;
        return sideSize;
    }

    //Box grab / un-grab with hand
    public void GrabUpdate(bool grabbed)
    {
        Debug.Log("Box grabbed: "+grabbed);
        BoxManager.isBoxHeld = grabbed;

        //toggle colliders to update grab for cubes
        foreach (var item in _cubesInBox)
        {
            item.ToggleGrab(grabbed);
        }
    }
} 
