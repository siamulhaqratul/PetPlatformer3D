using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityExt;
using UnityExt.DataSaving;

namespace UnityExt.Sound
{
    public sealed class MusicController : AudioController
    {
        [SerializeField] AudioClip backgroundMusic;
        static MusicController _instance;
        AudioClip defaultMusic;
        public static MusicController Instance { get { return _instance; } }
        protected override PlayerStorage<bool> DefineSettingConfig(bool defaultValue)
        {
            return new PlayerStorage<bool>("_IS_GAME_MUSIC_ON", defaultValue);
        }
        public static void PlayDefaultMusic()
        {
            if (!IsValid<MusicController>()) { return; }
            Tons.MultitonBase<AudioController>.Instances.ExForEachSafeRO((i) =>
            {
                var inst = i as MusicController;
                if (inst != null)
                {
                    inst.backgroundMusic = inst.defaultMusic;
                    if (inst.audioSource != null)
                    {
                        inst.audioSource.clip = inst.defaultMusic;
                        inst.audioSource.Stop();
                        inst.audioSource.loop = true;
                        inst.audioSource.Play();
                    }
                }
            });
        }
        protected override void OnAwake()
        {
            base.OnAwake();
            _instance = GetInstance<MusicController>();
            this.defaultMusic = backgroundMusic;
        }
        public static void EnableBackgroundMusic(bool enable)
        {
            Tons.MultitonBase<AudioController>.Instances.ExForEachSafeRO((i) =>
            {
                var inst = i as MusicController;
                if (inst != null) { inst.SetEnableInstance(enable, false); }
            });
        }
        public static void ChangeMusic(AudioClip clip)
        {
            if (!IsValid<MusicController>()) { return; }
            Tons.MultitonBase<AudioController>.Instances.ExForEachSafeRO((i) =>
            {
                var inst = i as MusicController;
                if (inst != null)
                {
                    inst.backgroundMusic = clip;
                    if (inst.audioSource != null)
                    {
                        inst.audioSource.clip = clip;
                        inst.audioSource.Stop();
                        inst.audioSource.loop = true;
                        inst.audioSource.Play();
                    }
                }
            });
        }
        protected override void OnChangeSoundSetting(bool isOn)
        {
            base.OnChangeSoundSetting(isOn);
            if (isOn)
            {
                if (backgroundMusic != null)
                {
                    if (audioSource != null)
                    {
                        audioSource.clip = backgroundMusic;
                        audioSource.Stop();
                        audioSource.loop = true;
                        audioSource.Play();
                    }
                }
            }
        }
    }
}