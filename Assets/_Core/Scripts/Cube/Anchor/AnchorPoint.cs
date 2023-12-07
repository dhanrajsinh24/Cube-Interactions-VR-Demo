using UnityEngine;

public class AnchorPoint : MonoBehaviour
{
    public CubeObject CubeObject { get; private set; }
    public AnchorSide anchorSide;
    public Collider MyCollider { get; private set; }

    private void Awake()
    {
        CubeObject = transform.root.GetComponent<CubeObject>();
        MyCollider = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("AnchorPoint")) return;

        if (!other.TryGetComponent(out AnchorPoint point)) return;
        
        //#1 - TODO - When the anchor point touches another anchor point
        //Add anchor pair to be checked later
        CustomInteractManager.Instance.
            TryAddingAnchorPair(anchorSide, point.anchorSide);
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("AnchorPoint")) return;

        if (!other.TryGetComponent(out AnchorPoint point)) return;

        //#2 - TODO - When the anchor point un-touches another anchor point
        //Remove the pair if needed
        CustomInteractManager.Instance.
            TryRemovingAnchorPair(anchorSide, point.anchorSide);
    }
}
