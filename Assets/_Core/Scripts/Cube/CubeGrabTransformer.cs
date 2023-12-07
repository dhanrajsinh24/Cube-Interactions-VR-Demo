using Oculus.Interaction;
using UnityEngine;

    /// <summary>
    /// Used to transform Puzzle Cubes by keeping physics alive
    /// </summary>
    public class CubeGrabTransformer : MonoBehaviour, ITransformer
    {
        /// <summary>
        /// Grabbable reference
        /// </summary>
        private Grabbable _grabbable;
        private Pose _grabDeltaInLocalSpace;
        private Rigidbody _rigidbody;

        public bool IsGrabbed { get; set; }
        private Quaternion _toRot;
        private Vector3 _toPos;

        public void Initialize(IGrabbable grabbable)
        {
            _grabbable = (Grabbable)grabbable;
            _rigidbody = _grabbable.Transform.GetComponent<Rigidbody>();
        }

        public void BeginTransform()
        {
            var grabPoint = _grabbable.GrabPoints[0];
            var targetTransform = _grabbable.Transform;
            _grabDeltaInLocalSpace = new Pose(
                targetTransform.InverseTransformVector(grabPoint.position - targetTransform.position),
                Quaternion.Inverse(grabPoint.rotation) * targetTransform.rotation);
        }

        public void UpdateTransform()
        {
            var grabPoint = _grabbable.GrabPoints[0];
            var targetTransform = _grabbable.Transform;
            _toPos = grabPoint.position - 
                      targetTransform.TransformVector(_grabDeltaInLocalSpace.position);
            _toRot = grabPoint.rotation * 
                      _grabDeltaInLocalSpace.rotation;
        }

        private void FixedUpdate()
        {
            MoveGrabbable();
        }

        private void MoveGrabbable()
        {
            //if the box is grabbed Then only this cube can be moved
            if (BoxManager.isBoxCreated)
            {
                if (!BoxManager.isBoxHeld) return;
            }

            if (!IsGrabbed) return;
            
            _rigidbody.MoveRotation(_toRot);
            _rigidbody.MovePosition(_toPos);
        }

        public void EndTransform() { }

        public void Grabbed(bool enable)
        {
            IsGrabbed = enable;
        }
    }