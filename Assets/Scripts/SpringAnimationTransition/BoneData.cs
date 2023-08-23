using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SpringAnimationTransition
{
    [Serializable]
    public class BoneData
    {
        public Transform transform;
        public Vector3 position;
        public Vector3 velocity;
        public Quaternion rotation;
        public Vector3 angularVelocity;

        public Vector3 offsetPosition;
        public Quaternion offsetRotation;
        public Vector3 offsetVelocity;
        public Vector3 offsetAngularVelocity;

        public Vector3 animatorPosition;
        public Quaternion animatorRotation;

        public Vector3 lastPosition;
        public Quaternion lastRotation;
        public Vector3 lastAnimatorPosition;
        public Quaternion lastAnimatorRotation;

        public float deltaTime = 0.02f;

        public BoneData(Transform transform, CoordinateSpace coordinateSpace)
        {
            this.transform = transform;
            if (coordinateSpace == CoordinateSpace.Local)
            {
                position = transform.localPosition;
                velocity = Vector3.zero;
                rotation = transform.localRotation;
                angularVelocity = Vector3.zero;
            }
            else if (coordinateSpace == CoordinateSpace.World)
            {
                position = transform.position;
                velocity = Vector3.zero;
                rotation = transform.rotation;
                angularVelocity = Vector3.zero;
            }
        }
    }
}

