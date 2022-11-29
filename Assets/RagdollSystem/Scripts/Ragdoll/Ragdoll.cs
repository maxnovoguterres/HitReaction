using System;
using UnityEngine;

namespace GunsArcade.Ragdoll
{
    [Serializable]
    public enum PlayerState
    {
        Animated,
        HitReaction,
        DamageRecover,
        RagdollMode,
        WaitForStable,
        RagdollToAnim
    }

    [Serializable]
    public enum BodyParts 
    {
        Spine,
        Chest,
        LeftHip,
        LeftKnee, 
        RightHip,
        RightKnee,
        Head,
        LeftArm,
        LeftElbow,
        RightArm,
        RightElbow,
        None
    }

    [Serializable]
    public class MuscleComponent 
    {
        public BodyParts BodyPart = BodyParts.None;
        public Transform Transform;
        public Rigidbody Rigidbody;
        public Collider Collider;
        public ConfigurableJoint Joint;
        public Vector3 InitialPosition;
        public Quaternion InitialRotation;
        public Vector3 StoredPosition;
        public Quaternion StoredRotation;
    
        public MuscleComponent (Rigidbody rb)
        {
            Rigidbody = rb;
            Transform = rb.transform;
            Collider = rb.GetComponent<Collider>();
            InitialPosition = rb.transform.localPosition;
            InitialRotation = rb.transform.localRotation;

            MuscleScript muscleScript = rb.GetComponent<MuscleScript>();
            if(muscleScript != null)
            {
                BodyPart = muscleScript.BodyPart;
                if(muscleScript.groundedJoint)
                {
                    Joint = rb.gameObject.AddComponent<ConfigurableJoint>();
                    Joint.configuredInWorldSpace = true;
                    Debug.Log($"created Joint");
                }
            }
        }
    }
}
