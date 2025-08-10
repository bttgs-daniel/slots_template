using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

/*
 17.02.2021
 21.02.2021
 23.02.2021
 */
namespace Mkey
{
	public class SoundGUIController : MonoBehaviour
	{
        [Header("Load sound, music prefs")]
        [SerializeField]
        private UnityEvent<float> LoadVolumeEvent;
        [SerializeField]
        private UnityEvent<float> LoadMusicVolumeEvent;
        [SerializeField]
        private UnityEvent<bool>  LoadSoundOnEvent;
        [SerializeField]
        private UnityEvent<bool> LoadMusicOnEvent;

        [Header("Sound, music events")]
        [SerializeField]
        private UnityEvent <float> ChangeVolumeEvent;
        [SerializeField]
        private UnityEvent<float> ChangeMusicVolumeEvent;
        [SerializeField]
        private UnityEvent <bool> SoundOnOffEvent;
        [SerializeField]
        private UnityEvent <bool> MusicOnOffEvent;

        #region temp vars
        private SoundMaster MSound => SoundMaster.Instance;
        #endregion temp vars

        #region regular
        private IEnumerator Start()
        {
            while (!MSound) yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            MSound.ChangeVolumeEvent += ChangeVolumeEventHandler;
            MSound.ChangeVolumeMusicEvent += ChangeMusicVolumeEventHandler;

            MSound.VolumeOnEvent += VolumeOnOffEventHandler;
            MSound.VolumeMusicOnEvent += VolumeMusikOnOffEventHandler;

            MSound.ChangeSoundOnEvent += SoundOnOffEventHandler;
            MSound.ChangeMusicOnEvent += MusicOnOffEventHandler;

            LoadVolumeEvent?.Invoke(MSound.Volume);
            LoadMusicVolumeEvent?.Invoke(MSound.VolumeMusic);
            LoadSoundOnEvent?.Invoke(MSound.SoundOn && MSound.Volume>0);
            LoadMusicOnEvent?.Invoke(MSound.MusicOn && MSound.VolumeMusic>0);
        }

        private void OnDestroy()
        {
            if (MSound)
            {
                MSound.ChangeVolumeEvent -= ChangeVolumeEventHandler;
                MSound.ChangeVolumeMusicEvent -= ChangeMusicVolumeEventHandler;
            }
        }
        #endregion regular

        public void ToggleMusik()
        {
            MSound.SetMusic(!MSound.MusicOn);
        }

        public void ToggleSound()
        {
            MSound.SetSound(!MSound.SoundOn);
        }

        public void SetVolume(Single volume)
        {
            MSound.SetVolume((float) volume);
        }

        public void SetMusicVolume(Single volume)
        {
            MSound.SetVolumeMusic((float)volume);
        }

        #region handlers
        private void ChangeVolumeEventHandler(float volume)
        {
            ChangeVolumeEvent?.Invoke(volume);
        }

        private void ChangeMusicVolumeEventHandler(float volume)
        {
            ChangeMusicVolumeEvent?.Invoke(volume);
        }

        private void VolumeOnOffEventHandler(bool on)
        {
             SoundOnOffEvent?.Invoke(on && MSound.SoundOn);
        }

        private void VolumeMusikOnOffEventHandler(bool on)
        {
            MusicOnOffEvent?.Invoke(on && MSound.MusicOn);
        }

        private void MusicOnOffEventHandler(bool on)
        {
            MusicOnOffEvent?.Invoke(on && MSound.VolumeMusic > 0);
        }

        private void SoundOnOffEventHandler(bool on)
        {
            SoundOnOffEvent?.Invoke(on && MSound.Volume > 0);
        }
        #endregion handlers

    }
}
