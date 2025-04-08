using NUnit.Framework;
using System;
using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace Inventory.Model {
    [CreateAssetMenu]
    public class ConsumableItemSO : ItemSO, IDestroyableItem, IItemAction {
        [SerializeField]
        private List<ModifierData> modifiersData = new List<ModifierData>();

        public string ActionName => "Consume";

        [field: SerializeField]
        public AudioClip actionSFX {get; private set;}

        public bool PerformAction(GameObject character, List<ItemParameter> itemState = null) {
            foreach (ModifierData data in modifiersData) {
                data.statModifier.AffectCharacter(character, data.value);
            }

            return true;
        }
    }

    public interface IDestroyableItem {

    }

    public interface IItemAction {
        public string ActionName { get; }
        public bool PerformAction(GameObject character, List<ItemParameter> itemState);
        public AudioClip actionSFX { get; }
    }

    [Serializable]
    public class ModifierData {
        public CharacterStatModifierSO statModifier;
        public int value;
    }
}


