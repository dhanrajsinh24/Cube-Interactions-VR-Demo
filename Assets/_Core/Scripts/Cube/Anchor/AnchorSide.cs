using UnityEngine;

public class AnchorSide : MonoBehaviour
{
    //unique id to identify this 
    public string ID { get; private set; }
    
    /// <summary>
    /// Side of this object
    /// </summary>
    private enum Side
    {
        Front,
        Back,
        Left,
        Right,
        Top,
        Bottom
    }
    
    [SerializeField] private Side mySide;
    public CubeObject CubeObject { get; private set; }

    private void Start()
    { 
        CubeObject = transform.root.GetComponent<CubeObject>();
        ID = CubeObject.gameObject.name + mySide;
    }
}
