using System;
using Oculus.Interaction;
using UnityEngine;

    /// <summary>
    /// Used to transform Puzzle Cubes by keeping physics alive
    /// </summary>
    public class BoxGrabTransformer : MonoBehaviour, ITransformer
    {
        /// <summary>
        /// Grabbable reference
        /// </summary>
        private IGrabbable _grabbable;
        private Pose _grabDeltaInLocalSpace;
        private Rigidbody _rigidbody;

        private bool _isGrabbed;
        private Quaternion _toRot;
        private Vector3 _toPos;

        private void Awake()
        {
            _toPos = transform.position;
            _toRot = transform.rotation;
        }

        public void Initialize(IGrabbable grabbable)
        {
            _grabbable = grabbable;
            _rigidbody = _grabbable.Transform.GetComponent<Rigidbody>();
        }

        public void BeginTransform()
        {
            var grabPoint = _grabbable.GrabPoints[0];
            var targetTransform = _grabbable.Transform;
            _grabDeltaInLocalSpace = new Pose(
                targetTransform.InverseTransformVector(grabPoint.position - targetTransform.position),
                Quaternion.Inverse(grabPoint.rotation) * targetTransform.rotation);
            _isGrabbed = true;
        }

        public void UpdateTransform()
        {
            _isGrabbed = true;
            var grabPoint = _grabbable.GrabPoints[0];
            var targetTransform = _grabbable.Transform;
            _toPos = grabPoint.position - 
                      targetTransform.TransformVector(_grabDeltaInLocalSpace.position);
            _toRot = grabPoint.rotation * 
                      _grabDeltaInLocalSpace.rotation;
        }

        private void Update()
        {
            //if (!_isGrabbed) return;
            //transform.SetPositionAndRotation(_toPos, _toRot);
        }

        private void FixedUpdate()
        {
            MoveGrabbable();
        }

        private void MoveGrabbable()
        {
            if (!_isGrabbed) return;
            
            _rigidbody.MoveRotation(_toRot);
            _rigidbody.MovePosition(_toPos);
        }

        public void EndTransform()
        {
            _isGrabbed = false;
        }
    }