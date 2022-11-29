using System;
using System.Collections.Generic;
using UnityEngine;
using MyBox;
using GunsArcade.Ragdoll;

public class RagdollSystem : MonoBehaviour 
{
    // Components
    List<MuscleComponent> muscles = new List<MuscleComponent>();
    [ReadOnly] PlayerState playerState = PlayerState.Animated;

    // Parameters
    public int maxHealth = 10;
    [ReadOnly] int health = 10;

    public float minHitSpeed = 15f;
    public float waitForGetUp = 2f;
    public float recoverSpeed = 2f;
    public float maxForceTolerance = 20f; // Use this as max force until entire body ragdolls. Most certainly, this value will be over nine thousand for us
    public bool canDie;

    bool isRagdoll;
    bool isDamage;
    Vector3 hitVelocity;
    BodyParts hitPart;
    Vector3 hitForce;
    
    float timer;
    float blendAmount;
    // The animator should be set up properly. Check the video https://www.youtube.com/watch?v=zoVk53D3Z4Y&ab_channel=UnityCity

    // References
    public MonoBehaviour[] disableScripts; // add behaviours that should stop during a body ragdoll here. like movement, maybe health?
    Animator animator;
    Transform b_hips;
    Transform b_hipsParent;

    void Start () 
    {
        animator = GetComponent<Animator>();
        GetBonesTransform();
        GetAllMuscleComponents();
        SetRagdollPart(true, true);
    }
    void OnEnable()
    {
        
    }
    void OnDisable()
    {
        foreach (MuscleComponent muscle in muscles)
        {
            muscle.Transform.localPosition = muscle.InitialPosition;
            muscle.Transform.localRotation = muscle.InitialRotation;
        }

        playerState = PlayerState.Animated;
        isRagdoll = false;
        isDamage = false;
        hitForce = Vector3.zero;
        hitPart = BodyParts.None;
        blendAmount = 0;
        health = maxHealth;
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
    }
    void Update()
    {
        RagdollRoutine_Update();
    }
    private void LateUpdate() 
    {
        RagdollRoutine_LateUpdate();      
    }
    private void OnCollisionEnter(Collision other) 
    {
        if(other.contacts.Length > 0 && other.contacts[0].otherCollider.transform != transform.parent)
        {
            hitVelocity = other.relativeVelocity;
        }
    }

    /// <summary>
    /// Gets the hips bone and its parent.
    /// </summary>
    void GetBonesTransform()
    {
        b_hips = animator.GetBoneTransform(HumanBodyBones.Hips);
        b_hipsParent = b_hips.parent;
    }
    /// <summary>
    /// Gets all the MuscleComponents inside the body
    /// </summary>
    void GetAllMuscleComponents() 
    {
        foreach (Rigidbody rb in GetComponentsInChildren<Rigidbody>()) 
        {
            muscles.Add(new MuscleComponent(rb));
        }
        SetRagdollPart(true, true);
    }
    /// <summary>
    /// Sets if the muscles should be kinematic and trigger. If these parameters are ON, the "reaction" will override the animation,
    /// thus creating the "ragdoll" effect.
    /// Also turns off/on the scripts to avoid unwanted behaviours like moving while lied down.
    /// </summary>
    void SetRagdollPart(bool state, bool gravity) 
    {
        animator.enabled = state;
        foreach(MuscleComponent muscle in muscles)
        {
            muscle.Rigidbody.useGravity = gravity;
            if(muscle.Transform == transform)
            {
                muscle.Collider.isTrigger = !state;
                muscle.Rigidbody.isKinematic = !state;
                continue;
            }
            muscle.Collider.isTrigger = state;
            muscle.Rigidbody.isKinematic = state;
        }
        foreach(MonoBehaviour script in disableScripts)
        {
            script.enabled = state;
        }
    }

    /// <summary>
    /// Main logic for the ragdoll system.
    /// </summary>
    void RagdollRoutine_Update()
    {
        switch (playerState)
        {
            case PlayerState.Animated:
                // Controls when the entirety of the body should ragdoll. Maybe add an (health <= 0) here as well 
                if(hitVelocity.magnitude > minHitSpeed)
                {
                    playerState = PlayerState.RagdollMode;
                    timer = waitForGetUp;
                    isRagdoll = true;
                }
            break;

            case PlayerState.HitReaction:
                SetRagdollPart(false, isRagdoll);
                isDamage = !isRagdoll;
                foreach(MuscleComponent muscle in muscles)
                {
                    if(muscle.BodyPart == hitPart)
                    {
                    muscle.Rigidbody.AddForce(hitForce, ForceMode.VelocityChange);
                    }
                    else
                    {
                        if(muscle.Joint)
                        {
                            muscle.Joint.xMotion = ConfigurableJointMotion.Locked;
                            muscle.Joint.yMotion = ConfigurableJointMotion.Locked;
                            muscle.Joint.zMotion = ConfigurableJointMotion.Locked;
                            if(muscle.Transform.childCount > 0)
                            {
                                muscle.Joint.anchor = muscle.Transform.GetChild(0).transform.localPosition;
                            }
                        }
                    }
                }
                playerState = PlayerState.DamageRecover;
            break;

            case PlayerState.DamageRecover:
                foreach(MuscleComponent muscle in muscles)
                {
                    if(muscle.Joint != null)
                    {
                        muscle.Joint.xMotion = ConfigurableJointMotion.Free;
                        muscle.Joint.yMotion = ConfigurableJointMotion.Free;
                        muscle.Joint.zMotion = ConfigurableJointMotion.Free;
                    }
                }
                playerState = PlayerState.RagdollMode;
            break;

            case PlayerState.RagdollMode:
                SetRagdollPart(false, isRagdoll);
                if(isRagdoll)
                {
                    foreach(MuscleComponent muscle in muscles)
                    {
                        muscle.Rigidbody.AddForce(-hitVelocity, ForceMode.Impulse);
                    }
                    hitVelocity = Vector3.zero;
                    b_hips.parent = null;
                    transform.position = b_hips.position;
                }
                if(b_hips.GetComponent<Rigidbody>().velocity.magnitude < 0.1f)
                {
                    timer = Mathf.MoveTowards(timer, 0, Time.deltaTime);
                }
                if((isRagdoll || isDamage) && timer == 0f)
                {
                    playerState = PlayerState.WaitForStable;
                }
            break;

            case PlayerState.WaitForStable:
                blendAmount = 1f;
                b_hips.parent = b_hipsParent;
                if (isRagdoll)
                {
                    if (gameObject.activeSelf)
                    {
                        gameObject.SetActive(false);
                    }
                }
                else
                {
                    playerState = PlayerState.RagdollToAnim;
                }
                foreach (MuscleComponent muscle in muscles)
                {
                    muscle.StoredPosition = muscle.Transform.localPosition;
                    muscle.StoredRotation = muscle.Transform.localRotation;
                }
                SetRagdollPart(true, true);
            break;
        }
    }
    /// <summary>
    /// Takes care of the logic for the ragdoll system in the LateUpdate.
    /// More or less resets the body state to the default (Animated).
    /// </summary>
    void RagdollRoutine_LateUpdate()
    {
        if(playerState == PlayerState.RagdollToAnim) 
        {
            blendAmount = Mathf.MoveTowards(blendAmount, 0, isDamage ?  Time.deltaTime * recoverSpeed : Time.deltaTime);

            foreach(MuscleComponent muscle in muscles)
            {
                muscle.Transform.localPosition = Vector3.Slerp(muscle.Transform.localPosition, muscle.StoredPosition, blendAmount);
                muscle.Transform.localRotation = Quaternion.Slerp(muscle.Transform.localRotation, muscle.StoredRotation, blendAmount);
            }
            if(blendAmount <= 0)
            {
                playerState = PlayerState.Animated;
                isRagdoll = false;
                isDamage = false;
                hitForce = Vector3.zero;
                hitPart = BodyParts.None;
            }
        }
    }

    /// <summary>
    /// Assigns to which body part, the force should be added to.
    /// </summary>
    public void Damage(BodyParts hitPart, Vector3 force)
    {
        if(hitPart == this.hitPart)
        {
            //    if((force.magnitude + hitForce.magnitude) > maxForceTolerance)
            //    {
            //        timer = waitForGetUp;
            //        isRagdoll = true;
            //        hitVelocity = -force;
            //    }
        }
        if (canDie)
        {
            health--;
        }
        if (health <= 0)
        {
            timer = waitForGetUp;
            isRagdoll = true;
            hitVelocity = -force;
        }
        else
        {
            playerState = PlayerState.HitReaction;
            this.hitPart = hitPart;
            hitForce = force;
        }
        Debug.Log("Force: "+ force);
    }
}