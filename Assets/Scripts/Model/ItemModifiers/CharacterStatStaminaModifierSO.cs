using UnityEngine;

[CreateAssetMenu]
public class CharacterStatStaminaModifierSO : CharacterStatModifierSO {
    public override void AffectCharacter(GameObject character, float val) {
        PlayerMovement playerMovement = character.GetComponent<PlayerMovement>();

        if(playerMovement!=null) {
            playerMovement.InfiniteStamina(val);
        }
    }
}
