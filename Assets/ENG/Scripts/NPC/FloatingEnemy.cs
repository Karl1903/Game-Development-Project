using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using Player;
using SoundWaves;
using Sounds;
using Utils;

namespace NPC {
    [RequireComponent(typeof(CharacterController))]
    public class FloatingEnemy : MonoBehaviour {
        [Header("Stats")]
        [SerializeField, Min(0f)] private float attackRange = 2f;
        [SerializeField, Min(0f)] private float attackSpeed = 1f;
        [SerializeField, Min(0f)] private float sightRange = 5f;
        [SerializeField, Range(0, 360)] private float angle = 100f;

        [Header("Gravity")]
        [SerializeField] private bool enableGravity = false;
        [SerializeField, ShowIf(nameof(enableGravity), true)]
        private float gravity = -Physics.gravity.y;
        [SerializeField, ShowIf(nameof(enableGravity), true)]
        private float gravityMultiplier = 0.25f;

        [Header("Sound Waves")]
        [SerializeField] private SWPattern attackWavePattern = SWPattern.DEFAULT;
        [SerializeField] private SWParams groanWaveParams = SWParams.DEFAULT;
        [SerializeField, Min(0f)] private float minGroanInterval = 5f;
        [SerializeField, Min(0f)] private float maxGroanInterval = 10f;

        [Header("Audio")]
        [SerializeField] private AudioMixerGroup mixerGroup;
        [SerializeField] private RandomSound groanSounds;
        [SerializeField, Range(0f, 1f)] private float groanVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float groanPitchRandomizeIntensity = 0.1f;
        [SerializeField] private RandomSound attackSounds;
        [SerializeField, Range(0f, 1f)] private float attackVolume = 1f;
        [SerializeField, Range(0f, 3f)] private float attackPitchRandomizeIntensity = 0.1f;

        [Header("Debug")]
        [SerializeField] private Vector2 moveDirection;
        [SerializeField, ReadOnly] private float velocityY;

        // ##############
        // ### INPUTS ###
        // ##############

        [Header("AI Inputs")]
        [SerializeField, Min(0f)] private float soundInputsLifetime = 3f;
        [SerializeField, ReadOnly]
        private NPCBrainInputs inputs = new NPCBrainInputs() {
            recentSoundInputs = new List<NPCBrainSoundInput>(),
            playerInAttackRange = false,
            playerInSightRange = false
        };
        public NPCBrainInputs Inputs => inputs;

        public bool isAttacking = false;

        private CharacterController characterController;
        private PlayerController playerController;
        private Animator anim;

        private Vector3 lastPos;
        private float lastGroanTimestamp = 0f;
        private float currGroanInterval = 0f;

        // #################
        // ### BEHAVIOUR ###
        // #################

        private void Awake() {
            characterController = GetComponent<CharacterController>();
            anim = GetComponentInChildren<Animator>();
        }

        private void Start() {
            playerController = PlayerController.Current;
            currGroanInterval = Random.Range(minGroanInterval, maxGroanInterval);

            lastPos = transform.position;
        }

        private void Update() {
            // Gravity
            if (enableGravity) {
                if (!characterController.isGrounded)
                    velocityY -= gravity * gravityMultiplier * Time.deltaTime;
                else
                    velocityY = -0.1f; // Stick to the ground -> isGrounded returns correct values
            } else velocityY = 0f;

            UpdateInputs();

            // Groaning
            if (Time.time >= lastGroanTimestamp + currGroanInterval) {
                SoundWaveManager.Inst.AddSoundWave(gameObject, transform.position, SoundTag.NPC, groanWaveParams);
                lastGroanTimestamp = Time.time;
                currGroanInterval = Random.Range(minGroanInterval, maxGroanInterval);

                float pitch = Random.Range(1f - groanPitchRandomizeIntensity, 1f + groanPitchRandomizeIntensity);
                SoundManager.Inst.PlayAtPosition("NPC Groan", transform.position, groanSounds.GetRandom(), mixerGroup, groanVolume, false, 0f, pitch);
            }

            anim.SetFloat("speed", Vector3.Distance(transform.position, lastPos));
            lastPos = transform.position;
        }

        // #######################
        // ### INPUTS HANDLING ###
        // #######################

        private void UpdateInputs() {
            // Update recent sound inputs, remove old ones
            for (int i = 0; i < inputs.recentSoundInputs.Count; i++) {
                if (Time.time >= inputs.recentSoundInputs[i].timestamp + soundInputsLifetime) inputs.recentSoundInputs.RemoveAt(i--);
            }

            // Is the player in attack range?
            Vector3 directionToTarget = (playerController.transform.position - transform.position).normalized;

            if (Vector3.Angle(transform.forward, directionToTarget) < angle / 2) {
                float distanceToTarget = Vector3.Distance(transform.position, playerController.transform.position);
                RaycastHit hit;

                if (!Physics.Raycast(transform.position, directionToTarget, out hit, distanceToTarget, LayerMask.GetMask("Default"))) {
                    inputs.playerInSightRange = Vector3.Distance(transform.position, playerController.transform.position) <= sightRange;
                    inputs.playerInAttackRange = Vector3.Distance(transform.position, playerController.transform.position) <= attackRange;
                } else {
                    inputs.playerInSightRange = false;
                    inputs.playerInAttackRange = false;
                }
            } else {
                //The enemy won't update those two variables, if the player is not within its FOV, so this should fix it
                inputs.playerInSightRange = false;
                inputs.playerInAttackRange = false;
            }
        }

        public void ReceiveSound(SoundNotifier soundNotifier) {
            inputs.recentSoundInputs.Add(new NPCBrainSoundInput() {
                timestamp = Time.time,
                soundOrigin = soundNotifier.SoundOrigin,
                soundDistance = Vector3.Distance(transform.position, soundNotifier.SoundOrigin),
                soundTag = soundNotifier.SoundTag
            });
        }

        // ###############
        // ### OUTPUTS ###
        // ###############

        /// <summary>
        /// Set the direction of the movement during update. If the vector is (0, 0) no movement will happen.
        /// </summary>
        /// <param name="direction">Movement direction</param>
        public void SetMoveDirection(Vector2 direction) {
            moveDirection = direction;
        }

        /// <summary>
        /// Attack the player if one is in range.
        /// </summary>
        [ContextMenu("Attack")]
        public void Attack() {
            if (inputs.playerInAttackRange) {
                StartCoroutine(AttackRoutine());
            }
        }

        private IEnumerator AttackRoutine() {
            isAttacking = true;

            anim.SetTrigger("attack");
            SoundWaveManager.Inst.AddSoundWavePattern(gameObject, transform, SoundTag.NPC, attackWavePattern);

            float pitch = Random.Range(1f - attackPitchRandomizeIntensity, 1f + attackPitchRandomizeIntensity);
            SoundManager.Inst.PlayAtPosition("NPC Attack", transform.position, attackSounds.GetRandom(), mixerGroup, attackVolume, false, 0f, pitch);

            yield return new WaitForSeconds(attackSpeed);

            // if (inputs.playerInAttackRange) GameManager.Inst.Respawn(); --> sword has a trigger to kill player

            isAttacking = false;
        }
    }
}
