using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using NPC;
using SoundWaves;
using Player;
using Utils;

namespace AI.Traditional {
    public class TraditionalAgent : MonoBehaviour {
        [Header("Stats")]
        [SerializeField] private float speedWalk = 2.5f;
        [SerializeField] private float speedChase = 6;
        
        // if an enemy is in close distance to another enemy, they will ignore each others sounds (because otherwise the ai will walk to it's own sounds)
        [SerializeField] private float minimumSoundDistance = 2f;
        [SerializeField, Min(0)] private float waitingTime = 1f;

        [Header("Setup")]
        [SerializeField] private Transform[] waypoints;

        [Header("Debug")]
        [SerializeField, ReadOnly] private AIStates currentState;
        [SerializeField, ReadOnly] private bool isHearingSounds;

        private FloatingEnemy floatingEnemy;
        private NavMeshAgent navMeshAgent;
        private List<NPCBrainSoundInput> sounds;
        private Transform player;

        private int currentWaypointIndex;

        // chaseSound state
        private Dictionary<string, float> soundProbabilities;
        [SerializeField] private float timesBeforeLearned = 3;
        private NPCBrainSoundInput currentlyFollowingSound;

        // standStill state
        private bool isStandingStill;
        private float startedStanding;

        void Start() {
            currentState = AIStates.Idle;
            
            currentWaypointIndex = 0;

            navMeshAgent = GetComponent<NavMeshAgent>();
            floatingEnemy = GetComponent<FloatingEnemy>();

            navMeshAgent.isStopped = false;
            navMeshAgent.speed = speedWalk;
            navMeshAgent.SetDestination(waypoints[currentWaypointIndex].position);

            // chaseSound state
            soundProbabilities = new Dictionary<string, float>();
            soundProbabilities.Add("General", 0);
            soundProbabilities.Add("Debug", 0);
            soundProbabilities.Add("Player", 100);
            soundProbabilities.Add("Environment", 0);
            soundProbabilities.Add("Resonance", 95);
            soundProbabilities.Add("Lyre", 100);
            soundProbabilities.Add("NPC", 90);
            soundProbabilities.Add("Stone", 100);
            soundProbabilities.Add("Bell", 80);
            soundProbabilities.Add("Door", 100);

            player = PlayerController.Current.transform;
        }

        private void Update() {
            InstantlyTurn();

            // update the current sate
            currentState = ChangeState();

            // reset the speed if not in idle, because the speed might be 0
            if (currentState != AIStates.Idle && currentState != AIStates.StandStill) {
                
                if (currentState is AIStates.ChasePlayer or AIStates.ChaseSound) { 
                    navMeshAgent.speed = speedChase;
                } else { 
                    navMeshAgent.speed = speedWalk;  
                }
                
            }

            // do something depending on the current state
            switch (currentState) {
                case AIStates.Idle:
                    Idle();
                    break;
                case AIStates.ChaseSound:
                    ChaseSound();
                    break;
                case AIStates.ChasePlayer:
                    ChasePlayer();
                    break;
                case AIStates.AttackPlayer:
                    AttackPlayer();
                    break;
                case AIStates.StandStill:
                    StandingStill();
                    break;
            }
        }

        private void InstantlyTurn() {
            navMeshAgent.angularSpeed = 999f;
            navMeshAgent.acceleration = 999f;
        }

        private AIStates ChangeState() {
            if (floatingEnemy.Inputs.playerInAttackRange) {
                return AIStates.AttackPlayer;
            }

            if (floatingEnemy.Inputs.playerInSightRange) {
                return AIStates.ChasePlayer;
            }

            if (isHearingSounds) {
                return AIStates.ChaseSound;
            }

            if (isStandingStill) {
                return AIStates.StandStill;
            }

            return AIStates.Idle;
        }

        // Enemies move on preset paths set as waypoints
        public void NextPoint() {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            navMeshAgent.SetDestination(waypoints[currentWaypointIndex].position);
        }

        private void Idle() {
            navMeshAgent.SetDestination(waypoints[currentWaypointIndex].position);
            if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance) {
                isStandingStill = true;
                startedStanding = Time.time;

                NextPoint();
            }
        }

        private void ChaseSound() {
            navMeshAgent.SetDestination(currentlyFollowingSound.soundOrigin);

            // update if the agent can hear sounds
            if (Vector3.Distance(navMeshAgent.destination, transform.position) < 1.1f) {
                ReducePlayerProbability(currentlyFollowingSound.soundTag.ToString());
                isHearingSounds = false;
                isStandingStill = true;
                startedStanding = Time.time;
            }
        }

        private void OnTriggerEnter(Collider other) {
            if (other.tag.Equals("SoundNotifier")) {
                String soundType = other.name[Range.StartAt(15)];
                if (soundProbabilities[soundType] != 0) {
                    other.gameObject.TryGetComponent(out SoundNotifier soundNotifier);

                    if (soundType.Equals("Player") || Vector3.Distance(soundNotifier.SoundOrigin, transform.position) > minimumSoundDistance) {
                        floatingEnemy.ReceiveSound(soundNotifier);

                        AnalyzeSounds();
                        isHearingSounds = true;
                    }
                }
            }
        }

        private float CalculateSoundScore(float playerProbability, float distance, float timeElapsed) {
            return (100 - playerProbability) + distance + (timeElapsed * 10);
        }

        private NPCBrainSoundInput CompareSoundScore(NPCBrainSoundInput soundOne, NPCBrainSoundInput soundTwo) {
            float soundOneScore = CalculateSoundScore(
                soundProbabilities[soundOne.soundTag.ToString()],
                soundOne.soundDistance,
                Time.time - soundOne.timestamp);

            float soundTwoScore = CalculateSoundScore(
                soundProbabilities[soundTwo.soundTag.ToString()],
                soundTwo.soundDistance,
                Time.time - soundTwo.timestamp);

            return soundOneScore < soundTwoScore ? soundOne : soundTwo;
        }

        private void AnalyzeSounds() {
            sounds = floatingEnemy.Inputs.recentSoundInputs;
            if (sounds.Count == 1) {
                currentlyFollowingSound = sounds[0];
            } else {
                for (int i = 1; i < sounds.Count; i++) {
                    NPCBrainSoundInput soundOne;
                    NPCBrainSoundInput soundTwo;

                    if (i == 1) {
                        soundOne = sounds[0];
                        soundTwo = sounds[i];

                        currentlyFollowingSound = CompareSoundScore(soundOne, soundTwo);
                    } else {
                        soundTwo = sounds[i];

                        currentlyFollowingSound = CompareSoundScore(currentlyFollowingSound, soundTwo);
                    }
                }
            }
        }

        private void ReducePlayerProbability(String soundTag) {
            if (soundTag.Equals("Stone")) {
                soundProbabilities[soundTag] -= (100 / timesBeforeLearned);
                if (soundProbabilities[soundTag] < 0) {
                    soundProbabilities[soundTag] = 0;
                }
            }
        }

        private void ChasePlayer() {
            navMeshAgent.SetDestination(player.position);
        }

        private void AttackPlayer() {
            // Currently calls the routine in FloatingEnemy directly, since we have our own attack range check
            if (!floatingEnemy.isAttacking) {
                floatingEnemy.StartCoroutine("AttackRoutine");
            }
        }

        private void StandingStill() {
            navMeshAgent.speed = 0;
            if (Time.time - startedStanding >= waitingTime) {
                isStandingStill = false;
                navMeshAgent.speed = speedWalk;
            }
        }
    }
}