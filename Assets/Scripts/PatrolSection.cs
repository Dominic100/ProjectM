using UnityEngine;

[System.Serializable]
public class PatrolSection {
    public string sectionName;
    public Collider2D sectionBounds;
    public Transform patrolPointsParent;
    [SerializeField] private float switchCooldown = 2f;
    private float lastSwitchTime;
    private Transform[] _patrolPoints;

    public Transform[] GetPatrolPoints() {
        if (_patrolPoints == null || _patrolPoints.Length == 0) {
            if (patrolPointsParent != null) {
                _patrolPoints = new Transform[patrolPointsParent.childCount];
                Debug.Log(patrolPointsParent.childCount);
                for (int i = 0; i < patrolPointsParent.childCount; i++) {
                    _patrolPoints[i] = patrolPointsParent.GetChild(i);
                    Debug.Log(_patrolPoints[i].name);
                }
            }
        }
        return _patrolPoints;
    }

    public bool CanSwitchTo() {
        return Time.time - lastSwitchTime >= switchCooldown;
    }

    public void MarkSwitched() {
        lastSwitchTime = Time.time;
    }
}
