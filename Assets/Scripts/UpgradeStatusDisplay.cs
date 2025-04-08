using UnityEngine;
using UnityEngine.UI;

public class UpgradeStatusDisplay : MonoBehaviour
{
    [SerializeField] private Image[] upgradeIcons;
    [SerializeField] private Color activeUpgradeColor = Color.green;
    [SerializeField] private Color inactiveUpgradeColor = Color.red;

    private void Start() {
        for(int i=0; i<upgradeIcons.Length; i++) {
            if (upgradeIcons[i]!=null) {
                upgradeIcons[i].color = inactiveUpgradeColor;
            }
        }
    }

    public void UpdateUpgradeStatus(int currentLevel) {
        for(int i=0; i<upgradeIcons.Length; i++) {
            if (upgradeIcons[i]!=null) {
                upgradeIcons[i].color = (i<currentLevel) ? activeUpgradeColor : inactiveUpgradeColor;
            }
        }
    }
}
