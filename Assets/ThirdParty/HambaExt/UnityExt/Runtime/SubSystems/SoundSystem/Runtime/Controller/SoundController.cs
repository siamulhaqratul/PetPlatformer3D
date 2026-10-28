using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityExt;
using DG.Tweening;
using UnityExt.DataSaving;

namespace UnityExt.Sound
{
    public sealed class SoundController : AudioController
    {
        [Header("Single SFX entries")]
        [SerializeField] List<SFXEntry> Sound_Entries;
        [Header("Set SFX entries. Play clips of a set sequentially on each call")]
        [SerializeField] List<SFXSet> soundSets;
        [Header("Smooth SFX entries")]
        [SerializeField] List<SmoothSFXEntry> Smooth_Sound_Entries;
        [SerializeField] GameObject smoothEntryHolder;

        static AudioSource smoothAudioSource = null;
        static SmoothSFXEntry currentlyPlayingSmoothEntry = null;
        static Coroutine mixingHandle = null;
        static Tween mixingTw = null;
        static SoundController _instance;
        public static SoundController Instance { get { return _instance; } }

        protected override PlayerStorage<bool> DefineSettingConfig(bool defaultValue)
        {
            return new PlayerStorage<bool>("_IS_GAME_SOUND_ON", defaultValue);
        }
        protected override void OnAwake()
        {
            base.OnAwake();
            currentlyPlayingSmoothEntry = null;
            mixingHandle = null;
            mixingTw = null;
            soundSets.ExForEach((set) => { set.Reset(); });
            _instance = GetInstance<SoundController>();
        }
        protected override void OnChangeSoundSetting(bool isOn)
        {
            base.OnChangeSoundSetting(isOn);
            if (isOn)
            {
                smoothAudioSource = smoothEntryHolder.EnsureComponent<AudioSource>();
                smoothAudioSource.playOnAwake = false;
            }
            else
            {
                if (smoothAudioSource != null)
                {
                    smoothAudioSource.Stop();
                    Destroy(smoothAudioSource);
                }
            }
        }
        public static void PlaySound(string soundName, float volume = 1.0f)
        {
            if (!IsValid<SoundController>()) { return; }
            var entries = GetInstance<SoundController>().Sound_Entries;
            SFXEntry entry = entries.GetDataWithName(soundName);
            Tons.MultitonBase<AudioController>.Instances.ExForEachSafeRO((i) =>
            {
                var inst = i as SoundController;
                if (inst != null)
                {
                    if (inst.audioSource == null) { return; }
                    inst.audioSource.playOnAwake = false;
                    inst.audioSource.volume = volume;
                    inst.audioSource.PlayOneShot(entry.Clip);
                }
            });
        }
        public static void PlaySoundSetEntry(string setName)
        {
            if (!IsValid<SoundController>()) { return; }
            var entries = GetInstance<SoundController>().soundSets;
            SFXSet entry = entries.GetDataWithName(setName);
            Tons.MultitonBase<AudioController>.Instances.ExForEachSafeRO((i) =>
            {
                var inst = i as SoundController;
                if (inst != null)
                {
                    if (inst.audioSource == null) { return; }
                    inst.audioSource.playOnAwake = false;
                    entry.Play(inst.audioSource);
                }
            });
        }

        public static void ResetSoundSet(string setName)
        {
            if (!IsValid<SoundController>()) { return; }
            var entries = GetInstance<SoundController>().soundSets;
            SFXSet entry = entries.GetDataWithName(setName);
            entry.Reset();
        }

        public static void StopAnySmoothSound()
        {
            if (!IsValid<SoundController>() || currentlyPlayingSmoothEntry == null) { return; }
            var instance = GetInstance<SoundController>();
            var entries = instance.Smooth_Sound_Entries;
            smoothAudioSource.loop = false;
            if (mixingHandle != null) { instance.StopCoroutine(mixingHandle); }
            smoothAudioSource.playOnAwake = false;
            smoothAudioSource.Stop();
            mixingHandle = instance.StartCoroutine(COR());
            IEnumerator COR()
            {
                var endClip = currentlyPlayingSmoothEntry.EndClip;
                smoothAudioSource.clip = endClip;
                smoothAudioSource.Play();
                var endClipDone = false;
                mixingTw.Kill();
                mixingTw = DOTween.To(() => { return smoothAudioSource.volume; }, (volume) =>
                {
                    smoothAudioSource.volume = volume;
                }, 0.0f,1f).OnComplete(() => { endClipDone = true; });
                while (!endClipDone) { yield return null; }
                currentlyPlayingSmoothEntry = null;
                smoothAudioSource.Stop();
            }
        }
        public static void SmoothPlaySound(string soundName, bool freshPlay = false)
        {
            if (!IsValid<SoundController>()) { return; }
            var instance = GetInstance<SoundController>();
            if (instance == null) { Debug.Log("yap null"); }
            mixingTw.Kill();
            smoothAudioSource.volume = 1;
            smoothAudioSource.pitch = 0.8f;
            smoothAudioSource.loop = true;
            var entries = GetInstance<SoundController>().Smooth_Sound_Entries;
            SmoothSFXEntry entry = entries.GetDataWithName(soundName);
            //if (currentlyPlayingSmoothEntry == entry && !freshPlay) { return; }

            smoothAudioSource.loop = false;
            if (mixingHandle != null) { instance.StopCoroutine(mixingHandle); }
            smoothAudioSource.playOnAwake = false;
            smoothAudioSource.Stop();
            mixingHandle = instance.StartCoroutine(COR());
            IEnumerator COR()
            {
                if (currentlyPlayingSmoothEntry != null)
                {
                    var endClip = currentlyPlayingSmoothEntry.EndClip;
                    smoothAudioSource.clip = endClip;
                    smoothAudioSource.Play();
                    var endClipDone = false;
                    mixingTw.Kill();
                    mixingTw = DOTween.To(() => { return smoothAudioSource.volume; }, (volume) =>
                    {
                        smoothAudioSource.volume = volume;
                    }, 0.0f, 0.5f).OnComplete(() => { endClipDone = true; });
                    while (!endClipDone) { yield return null; }
                }
                currentlyPlayingSmoothEntry = entry;
                var startClip = currentlyPlayingSmoothEntry.StartClip;
                if (smoothAudioSource.isPlaying) { smoothAudioSource.Stop(); }
                smoothAudioSource.clip = startClip;
                smoothAudioSource.Play();

                var startClipDone = false;
                mixingTw.Kill();
                mixingTw = DOTween.To(() => { return smoothAudioSource.volume; }, (volume) =>
                {
                    smoothAudioSource.volume = volume;
                }, 1.0f, 0.1f).OnComplete(() => { startClipDone = true; });
                while (!startClipDone) { yield return null; }

                if (smoothAudioSource.isPlaying) { smoothAudioSource.Stop(); }
                smoothAudioSource.clip = currentlyPlayingSmoothEntry.LoopClip;
                smoothAudioSource.Play();
                smoothAudioSource.loop = true;
            }
        }

        public static void PlayLoopSound(string soundName, bool freshPlay = false)
        {
            if (!IsValid<SoundController>()) { return; }
            var instance = GetInstance<SoundController>();
            if (instance == null) { Debug.Log("yap null"); }
            var entries = GetInstance<SoundController>().Sound_Entries;
            SFXEntry entry = entries.GetDataWithName(soundName);

            smoothAudioSource.loop = true;
            smoothAudioSource.playOnAwake = false;
            smoothAudioSource.volume = 1;
            smoothAudioSource.clip = entry.Clip;
            smoothAudioSource.Play();
        }

        public static void StopLoopSound()
        {
            smoothAudioSource.loop = false;
            smoothAudioSource.playOnAwake = false;
            smoothAudioSource.volume = 0;
            smoothAudioSource.Stop();
        }
    }
}