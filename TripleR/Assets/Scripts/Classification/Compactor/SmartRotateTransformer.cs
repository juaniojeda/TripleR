using System;
using System.Collections;
using UnityEngine;

namespace Oculus.Interaction
{
    /// Versión modificada del OneGrabRotateTransformer de Meta que incluye 
    /// auto-retorno al ángulo 0, manteniendo sincronizada la matemática interna.
    public class SmartRotateTransformer : MonoBehaviour, ITransformer
    {
        public enum Axis
        {
            Right = 0,
            Up = 1,
            Forward = 2
        }

        [SerializeField, Optional]
        private Transform _pivotTransform = null;

        public Transform Pivot => _pivotTransform != null ? _pivotTransform : transform;

        [SerializeField]
        private Axis _rotationAxis = Axis.Up;

        public Axis RotationAxis => _rotationAxis;

        [Serializable]
        public class OneGrabRotateConstraints
        {
            public FloatConstraint MinAngle;
            public FloatConstraint MaxAngle;
        }

        [SerializeField]
        private OneGrabRotateConstraints _constraints =
            new OneGrabRotateConstraints()
            {
                MinAngle = new FloatConstraint(),
                MaxAngle = new FloatConstraint()
            };

        public OneGrabRotateConstraints Constraints
        {
            get => _constraints;
            set => _constraints = value;
        }

        [Header("Configuración de Retorno")]
        [SerializeField] private float returnSpeed = 5f;
        [SerializeField] private float returnDelay = 1f;

        private Coroutine _returnRoutine;

        private float _relativeAngle = 0.0f;
        private float _constrainedRelativeAngle = 0.0f;

        private IGrabbable _grabbable;
        private Vector3 _grabPositionInPivotSpace;
        private Pose _transformPoseInPivotSpace;

        private Pose _worldPivotPose;
        private Vector3 _previousVectorInPivotSpace;

        private Quaternion _localRotation;
        private float _startAngle = 0;


        public void Initialize(IGrabbable grabbable)
        {
            _grabbable = grabbable;
        }

        public Pose ComputeWorldPivotPose()
        {
            if (_pivotTransform != null)
            {
                return _pivotTransform.GetPose();
            }

            var targetTransform = _grabbable.Transform;

            Vector3 worldPosition = targetTransform.position;
            Quaternion worldRotation = targetTransform.parent != null
                ? targetTransform.parent.rotation * _localRotation
                : _localRotation;

            return new Pose(worldPosition, worldRotation);
        }

        public void BeginTransform()
        {
            StopAutoReturn();

            var grabPoint = _grabbable.GrabPoints[0];
            var targetTransform = _grabbable.Transform;

            if (_pivotTransform == null)
            {
                _localRotation = targetTransform.localRotation;
            }

            Vector3 localAxis = Vector3.zero;
            localAxis[(int)_rotationAxis] = 1f;

            _worldPivotPose = ComputeWorldPivotPose();
            Vector3 rotationAxis = _worldPivotPose.rotation * localAxis;

            Quaternion inverseRotation = Quaternion.Inverse(_worldPivotPose.rotation);

            Vector3 grabDelta = grabPoint.position - _worldPivotPose.position;
            if (Mathf.Abs(grabDelta.magnitude) < 0.001f)
            {
                Vector3 localAxisNext = Vector3.zero;
                localAxisNext[((int)_rotationAxis + 1) % 3] = 0.001f;
                grabDelta = _worldPivotPose.rotation * localAxisNext;
            }

            _grabPositionInPivotSpace = inverseRotation * grabDelta;

            Vector3 worldPositionDelta =
                inverseRotation * (targetTransform.position - _worldPivotPose.position);

            Quaternion worldRotationDelta = inverseRotation * targetTransform.rotation;
            _transformPoseInPivotSpace = new Pose(worldPositionDelta, worldRotationDelta);

            Vector3 initialOffset = _worldPivotPose.rotation * _grabPositionInPivotSpace;
            Vector3 initialVector = Vector3.ProjectOnPlane(initialOffset, rotationAxis);
            _previousVectorInPivotSpace = Quaternion.Inverse(_worldPivotPose.rotation) * initialVector;

            _startAngle = _constrainedRelativeAngle;
            _relativeAngle = _startAngle;

            float parentScale = targetTransform.parent != null ? targetTransform.parent.lossyScale.x : 1f;
            _transformPoseInPivotSpace.position /= parentScale;
        }

        public void UpdateTransform()
        {
            var grabPoint = _grabbable.GrabPoints[0];
            var targetTransform = _grabbable.Transform;

            Vector3 localAxis = Vector3.zero;
            localAxis[(int)_rotationAxis] = 1f;
            _worldPivotPose = ComputeWorldPivotPose();
            Vector3 rotationAxis = _worldPivotPose.rotation * localAxis;

            Vector3 targetOffset = grabPoint.position - _worldPivotPose.position;
            Vector3 targetVector = Vector3.ProjectOnPlane(targetOffset, rotationAxis);

            Vector3 previousVectorInWorldSpace =
                _worldPivotPose.rotation * _previousVectorInPivotSpace;

            _previousVectorInPivotSpace = Quaternion.Inverse(_worldPivotPose.rotation) * targetVector;

            float signedAngle =
                Vector3.SignedAngle(previousVectorInWorldSpace, targetVector, rotationAxis);

            _relativeAngle += signedAngle;
            _constrainedRelativeAngle = _relativeAngle;

            if (Constraints.MinAngle.Constrain)
            {
                _constrainedRelativeAngle = Mathf.Max(_constrainedRelativeAngle, Constraints.MinAngle.Value);
            }
            if (Constraints.MaxAngle.Constrain)
            {
                _constrainedRelativeAngle = Mathf.Min(_constrainedRelativeAngle, Constraints.MaxAngle.Value);
            }

            Quaternion deltaRotation = Quaternion.AngleAxis(_constrainedRelativeAngle - _startAngle, rotationAxis);

            float parentScale = targetTransform.parent != null ? targetTransform.parent.lossyScale.x : 1f;
            Pose transformDeltaInWorldSpace =
                new Pose(
                    _worldPivotPose.rotation * (parentScale * _transformPoseInPivotSpace.position),
                    _worldPivotPose.rotation * _transformPoseInPivotSpace.rotation);

            Pose transformDeltaRotated = new Pose(
                deltaRotation * transformDeltaInWorldSpace.position,
                deltaRotation * transformDeltaInWorldSpace.rotation);

            targetTransform.position = _worldPivotPose.position + transformDeltaRotated.position;
            targetTransform.rotation = transformDeltaRotated.rotation;
        }

        public void EndTransform() { }

        public void StartAutoReturn()
        {
            if (_returnRoutine != null) StopCoroutine(_returnRoutine);
            _returnRoutine = StartCoroutine(ReturnToAngleRoutine(0f));
        }

        public void StopAutoReturn()
        {
            if (_returnRoutine != null)
            {
                StopCoroutine(_returnRoutine);
                _returnRoutine = null;
            }
        }

        private IEnumerator ReturnToAngleRoutine(float targetAngle)
        {
            yield return new WaitForSeconds(returnDelay);

            var targetTransform = _grabbable.Transform;
            Vector3 localAxis = Vector3.zero;
            localAxis[(int)_rotationAxis] = 1f;

            while (Mathf.Abs(_constrainedRelativeAngle - targetAngle) > 0.1f)
            {
                _relativeAngle = Mathf.Lerp(_relativeAngle, targetAngle, Time.deltaTime * returnSpeed);
                _constrainedRelativeAngle = _relativeAngle;

                _worldPivotPose = ComputeWorldPivotPose();
                Vector3 rotationAxis = _worldPivotPose.rotation * localAxis;

                Quaternion deltaRotation = Quaternion.AngleAxis(_constrainedRelativeAngle - _startAngle, rotationAxis);

                float parentScale = targetTransform.parent != null ? targetTransform.parent.lossyScale.x : 1f;
                Pose transformDeltaInWorldSpace = new Pose(
                    _worldPivotPose.rotation * (parentScale * _transformPoseInPivotSpace.position),
                    _worldPivotPose.rotation * _transformPoseInPivotSpace.rotation);

                Pose transformDeltaRotated = new Pose(
                    deltaRotation * transformDeltaInWorldSpace.position,
                    deltaRotation * transformDeltaInWorldSpace.rotation);

                targetTransform.position = _worldPivotPose.position + transformDeltaRotated.position;
                targetTransform.rotation = transformDeltaRotated.rotation;

                yield return null;
            }

            _relativeAngle = targetAngle;
            _constrainedRelativeAngle = targetAngle;
            _returnRoutine = null;
        }

        #region Inject
        public void InjectOptionalPivotTransform(Transform pivotTransform) => _pivotTransform = pivotTransform;
        public void InjectOptionalRotationAxis(Axis rotationAxis) => _rotationAxis = rotationAxis;
        public void InjectOptionalConstraints(OneGrabRotateConstraints constraints) => _constraints = constraints;
        #endregion
    }
}