using Inventory.Model;
using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System;

public class AgentWeapon : MonoBehaviour
{
    [SerializeField] private EquiappableItemSO weapon;
    [SerializeField] private InventorySO inventoryData;
    [SerializeField] private List<ItemParameter> parameterToModify, itemCurrentState;

    public void SetWeapon(EquiappableItemSO weaponItemSO, List<ItemParameter> itemState) {
        if(weapon!=null) {
            inventoryData.AddItem(weapon, 1, itemCurrentState);
        }

        this.weapon = weaponItemSO;
        this.itemCurrentState = new List<ItemParameter>(itemState);
        ModifyParameters();
    }

    private void ModifyParameters() {
        foreach (var parameter in parameterToModify) {
            if(itemCurrentState.Contains(parameter)) {
                int index = itemCurrentState.IndexOf(parameter);
                float newValue = itemCurrentState[index].value + parameter.value;
                itemCurrentState[index] = new ItemParameter {
                    itemParameter = parameter.itemParameter,
                    value = newValue
                };
            }
        }
    }
}
