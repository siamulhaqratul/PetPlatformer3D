using UnityEngine;

// Switches off coins and obstacles once the camera has passed them, and clears everything behind
// the pet when a run starts or resumes. Objects are deactivated, not destroyed, so a restart can bring them back.
// It measures from the pet, not the camera: after a restart the pet is moved at once, but Cinemachine only moves
// the camera later in the frame, and a stale camera position would switch off the course the pet was sent back to.
public class CourseCuller : MonoBehaviour
{
    // ================= SERIALIZED FIELDS (INSPECTOR) =================

    [Tooltip("Pet whose position decides what has been passed.")]
    [SerializeField] private Transform _pet;
    [Tooltip("Coins and obstacles, sorted from the start of the course to the end.")]
    [SerializeField] private Transform[] _targets;
    [Tooltip("How far behind the pet an object must be before it is switched off: the camera's follow distance plus a margin, so it is already out of view.")]
    [SerializeField] private float _cullDistance = 11f;

    // =================== PRIVATE/PROTECTED FIELDS ====================

    private int _nextIndex;

    // ======================= UNITY CALLBACKS =========================

    private void Update()
    {
        float cutoffZ = _pet.position.z - _cullDistance;
        while (_nextIndex < _targets.Length && _targets[_nextIndex].position.z < cutoffZ)
        {
            _targets[_nextIndex].gameObject.SetActive(false);
            _nextIndex++;
        }
    }

    // ========================= PUBLIC METHODS ========================

    // Everything from startZ onward is shown again; everything behind it is hidden.
    public void ShowFrom(float startZ)
    {
        _nextIndex = _targets.Length;
        for (int index = 0; index < _targets.Length; index++)
        {
            bool isAhead = _targets[index].position.z >= startZ;
            _targets[index].gameObject.SetActive(isAhead);

            if (isAhead && index < _nextIndex)
            {
                _nextIndex = index;
            }
        }
    }
}
