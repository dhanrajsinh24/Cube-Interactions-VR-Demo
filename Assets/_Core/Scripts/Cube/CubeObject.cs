using UnityEngine;
using UnityEngine.Animations;
using Grabbable = Oculus.Interaction.Grabbable;

public class CubeObject : MonoBehaviour
{
    /// <summary>
    /// indicates what is the priority of this object
    /// (Higher number means higher priority)
    /// </summary>
    public int Weight { get; set; } = 1;
    public ParentConstraint ParentConstraint { get; private set; }
    public Rigidbody Rigidbody { get; private set; }
    public Collider Collider { get; private set; }
    public Grabbable Grabbable { get; private set; }
    public AnchorPoint[] anchorPoints { get; private set; }
    public bool IsGrabbed { get; set; }
    public bool IsStuckInTheBox { get; set; }
    
    public bool IsReadyToBeStuck { get; set; }

    private CubeGrabTransformer _cubeGrabTransformer;
  
    private void Awake()
    {
        ParentConstraint = GetComponent<ParentConstraint>();
        Rigidbody = GetComponent<Rigidbody>();
        Collider = GetComponent<Collider>();
        Grabbable = GetComponent<Grabbable>();
        anchorPoints = GetComponentsInChildren<AnchorPoint>();
        _cubeGrabTransformer = GetComponent<CubeGrabTransformer>();
    }

    private void Start()
    {
        //Ignore collisions for all colliders of the same object
        for (var i = 0; i < anchorPoints.Length; i++)
        {
            for (var j = i+1; j < anchorPoints.Length; j++)
            {
                if(i.Equals(j)) continue;
                Physics.IgnoreCollision(anchorPoints[i].MyCollider, 
                    anchorPoints[j].MyCollider);
            }
        }

        foreach (var t in anchorPoints)
        {
            t.MyCollider.enabled = true;
        }
    }

    /// <summary>
    /// Disable all anchor points except one side mentioned
    /// </summary>
    /// <param name="side"></param>
    public void DisableOtherAnchorPoints(AnchorSide side)
    {
        foreach (var item in anchorPoints)
        {
            if(item.anchorSide.Equals(side)) continue;
            item.MyCollider.enabled = false;
        }
    }

    public void ToggleAnchorPoints(bool enable)
    {
        foreach (var item in anchorPoints)
        {
            item.MyCollider.enabled = enable;
        }
    }

    /// <summary>
    /// Settings needed when the cube is stuck/unstuck from the box
    /// </summary>
    /// <param name="isStuck"></param>
    public void ToggleStuckInBox(bool isStuck)
    {
        Debug.Log("ToggleStuckInBox: "+isStuck);
        //Make it stuck
        IsStuckInTheBox = isStuck;
        
        //Disable physics
        Rigidbody.isKinematic = isStuck;
        
        //Disable collider
        Collider.enabled = !isStuck;

        if (isStuck)
        {
            Rigidbody.velocity = Vector3.zero;
            Rigidbody.angularVelocity = Vector3.zero;
            IsGrabbed = false;
            _cubeGrabTransformer.IsGrabbed = false;
            ToggleAnchorPoints(true);
        }
    }

    /// <summary>
    /// Used to update grabing of cubes inside box
    /// </summary>
    /// <param name="grabbed"></param>
    public void ToggleGrab(bool grabbed)
    {
        Collider.enabled = grabbed;
        Grabbable.enabled = grabbed;
    }

    public void Grabbed(bool grabbed)
    {
        IsGrabbed = grabbed;

        if (grabbed)
        {
            //Debug.Log("Cube "+gameObject.name + " Grabbed again");
            Rigidbody.isKinematic = true;
            Rigidbody.velocity = Vector3.zero;
            Rigidbody.angularVelocity = Vector3.zero;
 
            //#9 - TODO - if cube is grabbed check if it was in the box
            CheckForUnstuck();
        }
        else
        {
            //#5 - TODO
            //If this cube is ungrabed and was ready to be stuck
            //Then stick it
            if (IsReadyToBeStuck)
            {
                CustomInteractManager.Instance.ConnectCubes();
            }
        }
    }

    private void CheckForUnstuck()
    {
        //Check for unstuck
        if (!BoxManager.isBoxCreated) return;
        if (!BoxManager.isBoxHeld) return;
        if (!IsStuckInTheBox) return;
            
        Debug.Log("Now the cube can be unstuck");
        ToggleStuckInBox(false);
        CustomInteractManager.CubeUnstuck?.Invoke(this);
    }

    /// <summary>
    /// Ignore collision of this cube with other
    /// </summary>
    /// <param name="other"></param>
    /// <param name="ignore"></param>
    public void IgnoreCollisionWithOther(CubeObject other, bool ignore)
    {
        var otherAnchorPoints = other.anchorPoints;

        for (var i = 0; i < anchorPoints.Length; i++)
        {
            for (var j = 0; j < otherAnchorPoints.Length; j++)
            {
                Physics.IgnoreCollision(anchorPoints[i].MyCollider,
                    otherAnchorPoints[j].MyCollider, ignore);
            }
        }
    }
}
