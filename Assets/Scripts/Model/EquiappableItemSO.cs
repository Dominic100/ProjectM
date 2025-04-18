using System;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory.Model {
    [CreateAssetMenu]
    public class EquiappableItemSO : ItemSO, IDestroyableItem, IItemAction {
        public string ActionName => "Equip";

        [field: SerializeField]
        public AudioClip actionSFX { get; private set; }

        public virtual bool PerformAction(GameObject character, List<ItemParameter> itemState = null) {
            AgentWeapon weaponSystem = character.GetComponent<AgentWeapon>();

            if (weaponSystem != null) {
                weaponSystem.SetWeapon(this, itemState == null ? DefaultParametersList : itemState);
                return true;
            }

            return false;
        }
    }
}
