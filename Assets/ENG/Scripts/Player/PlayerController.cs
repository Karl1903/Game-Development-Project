using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Audio;
using Random = UnityEngine.Random;
using Interactions;
using SoundWaves;
using SoundWaves.SoundMaterials;
using World.Pickupables;
using Sounds;
using Utils;

namespace Player {
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour {
        /// <summary>
        /// Do not use this in Awake. Start is fine.
        /// </summary>
        public static PlayerController Current { get; private set; }

        public const string PLAYER_TAG = "Player";

        [Header("Setup")]
        [SerializeField] private Transform pickupHandTransform;
        [SerializeField] private TrajectoryPreview trajectoryPreview;
        [SerializeField] private int trajectoryLength = 20;
        [SerializeField] private GameObject lyre;
        [SerializeField] private Transform lyreHandTransform;
        [SerializeField] private Transform lyreStowedTransform;

        [Header("Inputs")]
        [SerializeField] private InputActionAsset inputActionAsset;

        [Header("Stats")]
        [SerializeField, Range(0f, 6.3f)] private float sneakSpeed = 1.7f;
        [SerializeField, Range(0f, 6.3f)] private float walkSpeed = 3f;
        [SerializeField, Range(0f, 6.3f)] private float sprintSpeed = 6.3f;
        [SerializeField, Min(0f)] private float acceleration = 8f;
        [SerializeField, Min(0f)] private float deceleration = 16f;
        [SerializeField, Range(0f, 1f)] private float rotationSpeed = 0.9f;
        [SerializeField, Min(0f), Tooltip("Threshold for the fall animation when moving downwards, so walking down stairs or hills doesn't cause the falling animation. If the player seems to be sliding down hills, increase this value. If the player seems to be walking when falling, lower this value.")]
        private float fallThreshold = 4f;
        [SerializeField] private float jumpHeight = 7f;
        [SerializeField] private float throwForce = 20f;
        [SerializeField] private float throwRotation = 25f;

        [Header("Gravity")]
        [SerializeField] private float gravity = -Physics.gravity.y;
        [SerializeField] private float gravityMultiplier = 2f;

        [Header("Sound Wave Generation")]
        [SerializeField] private Transform leftFootPosition;
        [SerializeField] private Transform rightFootPosition;
        [SerializeField, Tooltip("Sound Wave that originates on every footstep of the player. Faster walking speed leads to a stronger wave (radius, brightness, width).")]
        private SWParams footstepSoundWave = SWParams.DEFAULT;
        [SerializeField, Range(0f, 1f), Tooltip("footstep waves will spawn infront of the foot by the given amount")]
        private float footstepSoundWaveOffset = 0f;
        [SerializeField, Tooltip("Sound Wave that originates when the players lands after jumping or falling. The greater the height, the stronger the wave (radius, brightness, width).")]
        private SWParams landingSoundWave = SWParams.DEFAULT;
        [SerializeField, Tooltip("Sound Wave Pattern when playing the lyre")]
        private SWPattern lyreSoundWavePattern = SWPattern.DEFAULT;
        [SerializeField, Min(0f)] private float floorInfluence = 1f;
        [SerializeField] private LayerMask floorLayerMask;

        [Header("Audio")]
        [SerializeField] private AudioMixerGroup mixerGroup;
        [SerializeField] private RandomSound defaultFootstepSounds;
        [SerializeField, Range(0f, 1f), Tooltip("The maximum volume of the footstep sound. The sound becomes quieter when walking slower.")]
        private float footstepVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float footstepPitchRandomizeIntensity = 0.1f;
        [SerializeField] private RandomSound landingSounds;
        [SerializeField, Range(0f, 1f), Tooltip("The maximum volume of the landing sound. The sound becomes quieter when falling from a lower height than the jump height. It will not become louder when falling from a greater height than the jump height.")]
        private float landingVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float landingPitchRandomizeIntensity = 0.1f;
        [SerializeField] private RandomSound lyreSounds;
        [SerializeField, Min(0f)] private float lyrePlayDelay = 0.33f;
        [SerializeField] private RandomSound pickupSounds;
        [SerializeField] private RandomSound throwSounds;

        [SerializeField] private AudioMixerGroup mixerGroupNarrator;
        [SerializeField] private RandomSound deathSounds;
        [SerializeField] private AudioClip fallingDeathSound;

        [Header("Debug")]
        [SerializeField, ReadOnly] private Vector2 moveInput;
        [SerializeField, ReadOnly] private float currAngle;
        [SerializeField, ReadOnly] private float currAngleVelocity;
        [SerializeField, ReadOnly] private float currWalkVelocity;
        [SerializeField, ReadOnly] private float currVelocityY;
        [SerializeField, ReadOnly] private float lastVelocityY; // saves the y velocity while falling, so it can be used for the landing wave strength
        [SerializeField, ReadOnly] private StaticSoundMaterial currFloorMaterial = null;
        [SerializeField, ReadOnly] private bool aiming = false;
        [SerializeField, ReadOnly] private bool isDead = false;
        [SerializeField] private List<Interaction> interactions = new List<Interaction>();

        private Camera cam;
        private CharacterController characterController;
        public CharacterController CharacterController => characterController;
        private float canJumpTimer;
        private bool canJump;

        private Animator animator;
        private readonly int walkVelocityId = Animator.StringToHash("Walking Velocity");
        private readonly int isJumpingId = Animator.StringToHash("Is Jumping");
        private readonly int isGroundedId = Animator.StringToHash("Is Grounded");
        private readonly int isFallingId = Animator.StringToHash("Is Falling");
        private readonly int dropStoneId = Animator.StringToHash("DropRock");
        private readonly int playLyreId = Animator.StringToHash("playLyre");
        private readonly int canThrowId = Animator.StringToHash("CanThrow");
        private readonly int threwRockId = Animator.StringToHash("ThrewRock");
        private readonly int pickupRockId = Animator.StringToHash("PickupRock");
        private readonly int pullLeverId = Animator.StringToHash("PullLever");
        private readonly int deadId = Animator.StringToHash("Dead");

        private bool movementDisabled = false;
        private bool handsAvailable = true; // indicates whether an action requiring the hands can be performed

        private InputActionReference sneakActionRef;
        private InputActionReference moveActionRef;
        private InputActionReference sprintActionRef;
        private InputActionReference jumpActionRef;
        private InputActionReference interactActionRef;
        private InputActionReference dropActionRef;
        private InputActionReference throwAimActionRef;
        private InputActionReference throwActionRef;
        private InputActionReference lyreActionRef;
        private InputActionReference menuActionRef;

        private IPickupable pickupable = null;
        private Lever lever = null;

        private void Awake() {
            Current = this;
            cam = Camera.main;
            characterController = GetComponent<CharacterController>();
            animator = GetComponentInChildren<Animator>();
            if (!trajectoryPreview) Debug.LogWarning("PlayerController: no trajectoryPreview assigned");
            if (!animator) Debug.LogWarning("PlayerController: no animator found in component or children");

            sneakActionRef = ScriptableObject.CreateInstance<InputActionReference>();
            moveActionRef = ScriptableObject.CreateInstance<InputActionReference>();
            sprintActionRef = ScriptableObject.CreateInstance<InputActionReference>();
            jumpActionRef = ScriptableObject.CreateInstance<InputActionReference>();
            interactActionRef = ScriptableObject.CreateInstance<InputActionReference>();
            dropActionRef = ScriptableObject.CreateInstance<InputActionReference>();
            throwAimActionRef = ScriptableObject.CreateInstance<InputActionReference>();
            throwActionRef = ScriptableObject.CreateInstance<InputActionReference>();
            lyreActionRef = ScriptableObject.CreateInstance<InputActionReference>();
            menuActionRef = ScriptableObject.CreateInstance<InputActionReference>();

            sneakActionRef.Set(inputActionAsset, "Player", "Sneak");
            moveActionRef.Set(inputActionAsset, "Player", "Move");
            sprintActionRef.Set(inputActionAsset, "Player", "Sprint");
            jumpActionRef.Set(inputActionAsset, "Player", "Jump");
            interactActionRef.Set(inputActionAsset, "Player", "Interact");
            dropActionRef.Set(inputActionAsset, "Player", "Drop");
            throwAimActionRef.Set(inputActionAsset, "Player", "Throw Aim");
            throwActionRef.Set(inputActionAsset, "Player", "Throw");
            lyreActionRef.Set(inputActionAsset, "Player", "Lyre");
            menuActionRef.Set(inputActionAsset, "Player", "Menu");
        }

        private void Start() {
            currAngle = transform.rotation.eulerAngles.y;
        }

        private void OnEnable() {
            inputActionAsset.Enable();
        }

        private void OnDisable() {
            inputActionAsset.Disable();
        }

        private void Update() {
            if (!GameManager.Inst.PauseMenuShown) {
                canJump = characterController.isGrounded;
                if (canJump)
                    canJumpTimer = 0.2f; // reset timer
                if (canJumpTimer > 0) {
                    canJump = true;
                    canJumpTimer -= Time.deltaTime; // count down timer
                }

                if (!movementDisabled) Move(canJump);

                if (!canJump) lastVelocityY = Mathf.Min(0, currVelocityY);

                UpdateInteractibleHighlight();

                // Interaction handling
                if (interactActionRef.action.triggered && canJump) {
                    // Only trigger the nearest interaction
                    Interaction nearestInteractible = GetNearestInteractible(interactions);

                    if (handsAvailable && nearestInteractible != null) {
                        bool successfullyInteracted = nearestInteractible.Interact();

                        if (successfullyInteracted && nearestInteractible.TeleportPlayerTo != null) {
                            // teleport player
                            transform.position = nearestInteractible.TeleportPlayerTo.position;
                            transform.rotation = nearestInteractible.TeleportPlayerTo.rotation;
                        }
                    }
                }

                // pickupable
                if (pickupable != null) {
                    // Throwing
                    if (throwAimActionRef.action.triggered) {
                        ThrowAim();
                    }

                    if (aiming) {
                        // Trajectory update
                        trajectoryPreview.SimulateTrajectory(pickupable, throwForce, throwRotation, trajectoryLength);

                        if (throwActionRef.action.triggered) {
                            animator.SetBool(threwRockId, true);
                            SoundManager.Inst.Play(throwSounds.GetRandom(), mixerGroup);
                            trajectoryPreview.Dispose();
                            aiming = false;
                        }
                    }

                    // Dropping
                    if (dropActionRef.action.triggered && !aiming && canJump) {
                        // start drop animation, but do not let go of the stone yet
                        DisableMovement();
                        animator.SetBool(dropStoneId, true);
                    }
                }

                // lyre
                if (handsAvailable) {
                    if (canJump) {
                        // Lyre
                        if (lyreActionRef.action.triggered) {
                            PlayLyre();
                        }
                    }
                }

                // Floor material handling
                DetectFloorMaterial();

                // Update animation variables
                animator.SetFloat(walkVelocityId, currWalkVelocity);
                animator.SetBool(isGroundedId, characterController.isGrounded);
                bool isFalling = !characterController.isGrounded && currVelocityY < -fallThreshold;
                animator.SetBool(isFallingId, isFalling);
                bool isJumping = !characterController.isGrounded && currVelocityY > 0;
                animator.SetBool(isJumpingId, isJumping);
            }

            // Check for menu button
            if (menuActionRef.action.triggered) GameManager.Inst.TogglePauseMenu();
        }

        private void Move(bool canJump) {
            // Read movement input value and convert it to 3D space
            moveInput = moveActionRef.action.ReadValue<Vector2>();
            Vector3 movement = new Vector3(moveInput.x, 0f, moveInput.y);

            bool hasDirectionInput = moveInput.magnitude > 0f;

            // Rotation handling
            float targetAngle = Mathf.Atan2(movement.x, movement.z) * Mathf.Rad2Deg + cam.transform.eulerAngles.y;
            if (hasDirectionInput || aiming) {
                currAngle = Mathf.SmoothDampAngle(currAngle, targetAngle, ref currAngleVelocity, 1f - rotationSpeed) % 360f;
                transform.rotation = Quaternion.Euler(0, currAngle, 0);
            }

            // Walking / Sprinting
            if (hasDirectionInput) {
                // Accelerate
                float targetVelocity;

                // if sprint and sneak is pressed, use sneak speed
                if (sneakActionRef.action.IsPressed()) targetVelocity = sneakSpeed;
                else if (sprintActionRef.action.IsPressed()) targetVelocity = sprintSpeed;
                else targetVelocity = walkSpeed * moveInput.magnitude;

                currWalkVelocity = Mathf.MoveTowards(currWalkVelocity, targetVelocity, acceleration * Time.deltaTime);
            } else {
                // Decelerate
                currWalkVelocity = Mathf.MoveTowards(currWalkVelocity, 0f, deceleration * Time.deltaTime);
            }

            // Gravity
            if (!characterController.isGrounded) {
                currVelocityY -= gravity * gravityMultiplier * Time.deltaTime;
            } else {
                currVelocityY = -0.1f; // Stick to the ground -> isGrounded returns correct values
            }

            // Jumping
            if (canJump && jumpActionRef.action.triggered) {
                currVelocityY = jumpHeight;
                canJumpTimer = 0;
            }

            // Final movement
            Vector3 finalVelocity = Quaternion.Euler(0, currAngle, 0) * Vector3.forward * currWalkVelocity;
            finalVelocity.y = currVelocityY;
            characterController.Move(finalVelocity * Time.deltaTime);
        }

        private Interaction GetNearestInteractible(List<Interaction> interactions) {
            float nearestDist = float.MaxValue;
            Interaction nearestInteractible = null;
            for (int i = 0; i < interactions.Count; i++) {
                Interaction ia = interactions[i];
                if (ia == null) {
                    // Fixes a rare issue where interaction objects are null
                    interactions.RemoveAt(i--);
                } else {
                    float dist = Vector3.Distance(transform.position, ia.transform.position);
                    if (dist < nearestDist) {
                        nearestDist = dist;
                        nearestInteractible = ia;
                    }

                }
            }
            return nearestInteractible;
        }

        private void UpdateInteractibleHighlight() {
            foreach (Interaction i in interactions) i.SetHighlight(false);
            if (handsAvailable) GetNearestInteractible(interactions)?.SetHighlight(true);
        }

        private void OnTriggerEnter(Collider other) {
            // Interaction handling
            Interaction interaction = other.GetComponent<Interaction>();
            // Check if it is an interaction opbject and is not the current picked up object in hand
            if (interaction && interaction.gameObject != pickupable?.GetTransform().gameObject) {
                interactions.Add(interaction);
                UpdateInteractibleHighlight();
            }
        }

        private void OnTriggerExit(Collider other) {
            // Interaction handling
            Interaction interaction = other.GetComponent<Interaction>();
            interaction?.SetHighlight(false);
            interactions.Remove(interaction);
            UpdateInteractibleHighlight();
        }

        public void LeftFootstepEvent(AnimationEvent evt) {
            FootstepEvent(evt, leftFootPosition.position);
        }

        public void RightFootstepEvent(AnimationEvent evt) {
            FootstepEvent(evt, rightFootPosition.position);
        }

        public void LandingEvent() {
            float landingStrength = -lastVelocityY / jumpHeight;

            SWParams swParams = landingSoundWave;
            float floorImpact = (1f - (currFloorMaterial ? currFloorMaterial.objectSoftness : StaticSoundMaterial.DEFAULT_SOFTNESS)) * floorInfluence;
            swParams.soundRadius *= landingStrength * floorImpact;
            swParams.waveColor.a *= landingStrength * floorImpact;
            swParams.waveWidth *= landingStrength;

            // spawn at left foot, because the exact position is not noticeable anyway
            SoundWaveManager.Inst.AddSoundWave(gameObject, leftFootPosition.position, SoundTag.Player, swParams);

            float volume = landingVolume * landingStrength;
            float pitch = Random.Range(1f - landingPitchRandomizeIntensity, 1f + landingPitchRandomizeIntensity);
            SoundManager.Inst.PlayAtPosition("Player Landing", leftFootPosition.position, landingSounds.GetRandom(), mixerGroup, volume, false, 0f, pitch);
        }

        public void Pickup(IPickupable p) {
            DisableMovement();
            handsAvailable = false;
            StowLyre();

            if (pickupable == null) {
                pickupable = p;
                SoundManager.Inst.Play(pickupSounds.GetRandom(), mixerGroup);
                animator.SetBool(pickupRockId, true);
            }
        }

        public void PullLever(Lever lever) {
            this.lever = lever;

            DisableMovement();
            handsAvailable = false;
            StowLyre();

            animator.SetBool(pullLeverId, true);
        }

        public void Die() {
            if (DebugManager.InvincibleMode || isDead) return;
            isDead = true;
            DisableMovement();
            handsAvailable = false;
            animator.Play(deadId);
            SoundManager.Inst.Play(deathSounds.GetRandom(), mixerGroupNarrator);
            GameManager.Inst.Respawn();
        }

        public void DieFalling() {
            if (DebugManager.InvincibleMode || isDead) return;
            isDead = true;
            handsAvailable = false;
            SoundManager.Inst.Play(fallingDeathSound, mixerGroupNarrator);
            GameManager.Inst.Respawn(0.5f);
        }

        private void FootstepEvent(AnimationEvent evt, Vector3 position) {
            // if 2 animations are blended, skip the less important one -> otherwise 2 events are triggered
            if (evt.animatorClipInfo.weight < 0.5) return;

            // offset the wave spawn in the walk direction
            Quaternion rotation = Quaternion.Euler(0, currAngle, 0);
            Vector3 direction = rotation * Vector3.forward;
            float offsetStrength = currWalkVelocity / sprintSpeed; // [0, 1]
            position += direction * footstepSoundWaveOffset * offsetStrength;

            SWParams swParams = footstepSoundWave;

            // change wave strength
            float footstepStrength = currWalkVelocity / sprintSpeed;
            float floorImpact = (1f - (currFloorMaterial ? currFloorMaterial.objectSoftness : StaticSoundMaterial.DEFAULT_SOFTNESS)) * floorInfluence;
            swParams.soundRadius *= footstepStrength * floorImpact;
            swParams.waveColor.a *= footstepStrength * floorImpact;
            swParams.waveWidth *= footstepStrength;

            SoundWaveManager.Inst.AddSoundWave(gameObject, position, SoundTag.Player, swParams);

            float volume = footstepVolume * footstepStrength;
            float pitch = Random.Range(1f - footstepPitchRandomizeIntensity, 1f + footstepPitchRandomizeIntensity);
            SoundManager.Inst.PlayAtPosition("Player Footstep", position, (currFloorMaterial ? currFloorMaterial.GetFootstepSoundsOverride(defaultFootstepSounds) : defaultFootstepSounds).GetRandom(), mixerGroup, volume, false, 0f, pitch);
        }

        private void Drop() {
            if (aiming) {
                trajectoryPreview.Dispose();
                aiming = false;
            }
            if (pickupable != null) {
                pickupable.Drop();
                pickupable = null;
            }
        }

        private void ThrowAim() {
            animator.SetBool(canThrowId, true);
            aiming = true;
            trajectoryPreview.Init();
        }

        private void Throw() {
            pickupable.Throw(throwForce, throwRotation);
            pickupable = null;
        }

        private void PickupRock() {
            Transform t = pickupable.GetTransform();
            t.parent = pickupHandTransform;
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
        }

        private void PlayLyre() {
            SoundWaveManager.Inst.AddSoundWavePattern(lyre, lyre.transform, SoundTag.Lyre, lyreSoundWavePattern);
            SoundManager.Inst.Play(lyreSounds.GetRandom(), mixerGroup, 1f, false, lyrePlayDelay);
            animator.SetBool(playLyreId, true);

            DisableMovement();
            handsAvailable = false;
        }

        private void DetectFloorMaterial() {
            if (Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, 1f, floorLayerMask, QueryTriggerInteraction.Ignore)) {
                StaticSoundMaterial floorMaterial = hit.collider.gameObject.GetComponent<StaticSoundMaterial>();
                if (floorMaterial != currFloorMaterial) {
                    currFloorMaterial = floorMaterial;
                }
            }
        }

        // called from animation at the time the lever movement starts
        private void StartLeverPullAnimation() {
            lever.StartPullAnimation();
        }

        private void StowLyre() {
            lyre.transform.SetParent(lyreStowedTransform);
            lyre.transform.localPosition = Vector3.zero;
            lyre.transform.localRotation = Quaternion.identity;
        }

        private void UnstowLyre() {
            lyre.transform.SetParent(lyreHandTransform);
            lyre.transform.localPosition = Vector3.zero;
            lyre.transform.localRotation = Quaternion.identity;
        }

        private void DisableMovement() {
            currWalkVelocity = 0;
            currVelocityY = 0;
            movementDisabled = true;
        }

        private void EnableMovement() {
            movementDisabled = false;
        }

        // gets called from animation events
        private void EndAnimation(String animationName) {
            EnableMovement();
            animator.SetBool(animationName, false);
        }

        // gets called from animation events
        private void FreeHands() {
            handsAvailable = true;
            UnstowLyre();
        }
    }
}
