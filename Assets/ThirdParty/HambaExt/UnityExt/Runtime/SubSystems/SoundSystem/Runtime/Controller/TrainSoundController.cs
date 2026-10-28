using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityExt.Sound
{
    public class TrainSoundController : MonoBehaviour
    {
        // Start is called before the first frame update
        [SerializeField] private AudioSource _audioSource;

        void Start()
        {
            var isOn = AudioController.Instances[0].IsOn;
            ToggleSound(isOn.Value);
            isOn.OnChangeData.AddListener(() =>
            {
                ToggleSound(isOn.Value);
            });

        }

        private void ToggleSound(bool isOnBool)
        {
            if (isOnBool)
            {
                _audioSource.volume = 1f;
            }
            else
            {
                _audioSource.volume = 0f;
            }
        }
    }
}