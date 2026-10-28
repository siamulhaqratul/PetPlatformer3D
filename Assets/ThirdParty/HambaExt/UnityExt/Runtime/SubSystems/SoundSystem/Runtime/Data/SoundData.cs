using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityExt.Sound
{
    internal interface ISoundData
    {
        internal string DataName { get; }
    }

    [System.Serializable]
    internal class SFXEntry : ISoundData
    {
        [SerializeField] string soundName;
        [SerializeField] AudioClip clip;
        internal AudioClip Clip { get { return clip; } }
        string ISoundData.DataName => soundName;
    }

    [System.Serializable]
    internal class SmoothSFXEntry : ISoundData
    {
        [SerializeField] string entryName;
        [SerializeField] AudioClip startClip, loopClip, endClip;
        internal AudioClip StartClip { get { return startClip; } }
        internal AudioClip LoopClip { get { return loopClip; } }
        internal AudioClip EndClip { get { return endClip; } }
        string ISoundData.DataName => entryName;
    }

    [System.Serializable]
    internal class SFXSet : ISoundData
    {
        [SerializeField] string setName;
        [SerializeField] List<AudioClip> clips;
        int currentIndex;
        string ISoundData.DataName => setName;
        internal void Play(AudioSource src)
        {
            src.playOnAwake = false;
            var clip = clips[currentIndex];
            src.PlayOneShot(clip);
            currentIndex++;
            if (currentIndex > clips.Count - 1) { currentIndex = 0; }
        }

        internal void Reset() { currentIndex = 0; }
    }
}