using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityExt;
using UnityExt.DataSaving;
using UnityExt.Tons;

namespace UnityExt.Sound
{
    public abstract class AudioController : MultitonBehaviour<AudioController>
    {
        [SerializeField] bool defaultOn = true;
        [SerializeField] GameObject holder;
        protected AudioSource audioSource = null;
        PlayerStorage<bool> isOn = null;
        protected abstract PlayerStorage<bool> DefineSettingConfig(bool defaultValue);
        public AudioSource AudioSource => audioSource;
        private bool _onlyOnce;
        protected static bool IsValid<T>() where T : AudioController 
        {
            var instance = GetInstance<T>();
            return instance != null && instance.isOn != null && instance.isOn.Value;
        }
        protected override void OnAwake()
        {
            base.OnAwake();
            isOn = DefineSettingConfig(defaultOn);
            DontDestroyOnLoad(this);
        }
        protected override void OnLoadUnityScene(Scene scene, LoadSceneMode mode)
        {
            if (_onlyOnce) return;
            base.OnLoadUnityScene(scene, mode);
            if (scene.name == "boot" || scene.buildIndex == 0)
            {
                audioSource = holder.GetComponent<AudioSource>();
                if (audioSource != null)
                {
                    audioSource.Stop();
                    Destroy(audioSource);
                }
            }
            else
            {
                StartCoroutine(OnSceneStart());
                IEnumerator OnSceneStart()
                {
                    yield return null;
                    OnChangeSoundSetting(isOn.Value);
                    _onlyOnce = true;
                }
            }
        }
        public static void SetEnable(bool isOn, bool saveToDevice = true)
        {
            MultitonBase<AudioController>.Instances.ExForEachSafeRO((i) => 
            {
                if (saveToDevice)
                {
                    i.isOn.Value = isOn;
                }
                i.OnChangeSoundSetting(isOn);
            });
        }
        public void SetEnableInstance(bool isOn, bool saveToDevice = true)
        {
            if (saveToDevice)
            {
                this.isOn.Value = isOn;
            }
            this.OnChangeSoundSetting(isOn);
        }

        public static void SetEnableFlagsOnly(bool enable)
        {
            MultitonBase<AudioController>.Instances.ExForEachSafeRO((i) => { i.isOn.Value = enable; });
        }
        public PlayerStorage<bool> IsOn { get { return isOn; } }
        protected virtual void OnChangeSoundSetting(bool isOn)
        {
            if (isOn)
            {
                audioSource = holder.EnsureComponent<AudioSource>();
                audioSource.playOnAwake = false;
            }
            else
            {
                audioSource = holder.GetComponent<AudioSource>();
                if (audioSource != null)
                {
                    audioSource.Stop();
                    Destroy(audioSource);
                }
            }
        }
    }
}