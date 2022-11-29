using UnityEngine;
using System;
using System.Collections;

namespace GunsArcade.Ragdoll
{
    public class MuscleScript : MonoBehaviour
    {
        RagdollSystem ragdollSystem;

        public BodyParts BodyPart = BodyParts.None;
        public bool Critical = false;
        public bool groundedJoint = false;

        private void Start() 
        {
            ragdollSystem = GetComponentInParent<RagdollSystem>();
        }

        // Use this method in the bullet to register a hit reaction.
        // force should be direction of the bullet! something like bullet.transform.forward * bulletForce
        // Check InputGun as reference
        public void GetDamage(Vector3 force)
        {
            ragdollSystem.Damage(BodyPart, force);
        }
    }
}