using System.Runtime.CompilerServices;
using UnityEngine;

[CreateAssetMenu]
public class CharacterStatMovementModifierSO : CharacterStatModifierSO {
    public override void AffectCharacter(GameObject character, float val) {
        MonsterAI monsterAI = character.GetComponent<MonsterAI>();

        if (monsterAI != null) {
            monsterAI.tempStop(val);
        }
    }
}
