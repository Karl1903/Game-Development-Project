using System.Collections.Generic;
using Utils;

namespace NPC {
    [System.Serializable]
    public struct NPCBrainInputs {
        [ReadOnly] public List<NPCBrainSoundInput> recentSoundInputs; // recent perceived sounds
        [ReadOnly] public bool playerInAttackRange; // is the player in attack range?
        [ReadOnly] public bool playerInSightRange;
    }
}