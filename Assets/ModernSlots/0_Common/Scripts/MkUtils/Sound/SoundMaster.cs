using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif
/*
   021218 
   add stopallclip

    160119
    set clips as private
    add StopAllClip(bool stopBackgroundMusik)

    240119
    add   public void SetNewBackGroundClipAndPlay(AudioClip newBackGroundClip)
    fix  void PlayBkgMusik(bool play)
            // set clip if failed
            if (aSbkg && !aSbkg.clip && bkgMusic) aSbkg.clip = bkgMusic;

    020619
        add SetSound (bool on)
        add SetMusic (bool on)
        add SetFeatMusic (bool on)
        add SetVolume(float volume)

    100219
        replace OLD
              if (aSclick && aC)
                {
                    aSclick.clip = aC;
                    aSclick.Play();
                }
                while (aSclick.isPlaying)
                    yield return wff;
       new
        if (aSclick && aC)
                {
                    aSclick.clip = aC;
                    aSclick.Play();
                    while (aSclick.isPlaying)
                        yield return wff;
                }
           remove
            GetComponet<AudioSource>

    21.02.19
        base class for game soun masters
    25.06.2019
     change  
      public void StopAllClip(bool stopMusic)
        {
            if (musicSource && stopMusic) StopMusic();

            AudioSource[] aSs = GetComponentsInChildren<AudioSource>();
            if (aSs != null )
            {
                foreach (var item in aSs)
                {
                    if (item && (item != musicSource)) item.Stop();
                }
            }
        }
        
        private void ApplyVolume()
        {
            if (musicSource)
            {
                musicSource.volume = Volume * musicVolumeMult;
            }

            AudioSource[] aSs = GetComponentsInChildren<AudioSource>();
            if (aSs != null)
            {
                foreach (var item in aSs)
                {
                    if (item && (item != musicSource)) item.volume = Volume;
                }
            }
        }

        27.06.2019  public void PlayCurrentMusic()
        28.06.2019   - make accessible       
        [SerializeField]
        private int audioSoucesMaxCount = 10;
        
        31.03.2020
            - public Action <float> ChangeVolumeEvent;
        
        16.07.2020 
            -fix editor play mode

        04.01.2021 
            - improve music tween, SetSound
            - add events:
                public Action<bool> ChangeMusicOnEvent;
                public Action<bool> ChangeSoundOnEvent;

        17.02.2021 
            - add Music Volume

        04.05.2021 - add  public void StopClips(AudioClip clip),  play loop -  public void PlayClip(float delay, bool loop, AudioClip clip)

        27.06.2023 - add public List<AudioSource> AudioSources => tempAudioSources;
        change:  
        protected IEnumerator PlayClipAtPoint(float playDelay, AudioClip aC, Vector3 pos, float volumeMultiplier, Action completeCallBack)
        {
            if (SoundOn && tempAudioSources.Count >= audioSourcesMaxCount)
            {

                // AudioSource[] aSs = GetComponentsInChildren<AudioSource>();
                if (tempAudioSources[0]) tempAudioSources[0].Stop();
            }

*/

namespace Mkey
{
    public class SoundMaster : MonoBehaviour
    {
        #region basic clips
        [Space(8, order = 0)]
        [Header("Basic audio clips", order = 1)]
        [SerializeField]
        private AudioClip music;
        [SerializeField]
        private AudioClip buttonClick;
        [SerializeField]
        private AudioClip openWindow;
        [SerializeField]
        private AudioClip closeWindow;
        #endregion basic clips
        [SerializeField]
        private uint audioSourcesMaxCount = 10;

        #region save keys
        [SerializeField]
        private bool saveSettings = true;
        private string saveNameSound = "mk_soundon";
        private string saveNameMusic = "mk_musicon";
        private string saveNameVolume = "mk_volume";
        private string saveNameVolumeMusic = "mk_volume_music";
        #endregion save keys

        #region private
        private AudioSource musicSource; // for background musik
        private AudioClip currentMusic;
        private WaitForEndOfFrame wff; // new WaitForEndOfFrame()
        private WaitForSeconds wfs0_1; // new WaitForSeconds(0.1f);
        private List<AudioSource> tempAudioSources; // not loop
        private int musicVolumeTween = -1;
        #endregion private

        #region properties
        public static SoundMaster Instance { get; private set; }

        public bool SoundOn
        {
            get; private set;
        }

        public bool MusicOn
        {
            get; private set;
        }

        public float Volume
        {
            get; private set;
        }

        public float VolumeMusic
        {
            get; private set;
        }

        public List<AudioSource> AudioSources => tempAudioSources;
        #endregion properties

        #region events
        public Action<float> ChangeVolumeEvent;
        public Action<float> ChangeVolumeMusicEvent;
        public Action<bool> VolumeOnEvent;
        public Action<bool> VolumeMusicOnEvent;
        public Action<bool> ChangeSoundOnEvent;
        public Action<bool> ChangeMusicOnEvent;
        #endregion events

        [Tooltip("Music volume multiplier")]
        [SerializeField]
        private float musicVolumeMult = 0.1f;

        [Tooltip("Music volume multiplier")]
        [SerializeField]
        private bool useMusicVolumeControl = false;

        #region regular
        private void Awake()
        {
            Debug.Log("sound base awake");
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
            wff = new WaitForEndOfFrame();
            wfs0_1 = new WaitForSeconds(0.1f);
        }

        private void Start()
        {
            SoundOn = true;
            MusicOn = true;
            Volume = 1;
            VolumeMusic = 1;

            if (saveSettings)
            {
                SoundOn = (PlayerPrefs.GetInt(saveNameSound, 1) > 0) ? true : false;
                MusicOn = (PlayerPrefs.GetInt(saveNameMusic, 1) > 0) ? true : false;
                Volume = PlayerPrefs.GetFloat(saveNameVolume, 1);
                VolumeMusic = (useMusicVolumeControl) ? PlayerPrefs.GetFloat(saveNameVolumeMusic, 1) : Volume;
                ChangeVolumeEvent?.Invoke(Volume);
                ChangeVolumeMusicEvent?.Invoke(VolumeMusic);
                ChangeMusicOnEvent?.Invoke(MusicOn);
                ChangeSoundOnEvent?.Invoke(SoundOn);
                VolumeMusicOnEvent?.Invoke(VolumeMusic > 0);
                VolumeOnEvent?.Invoke(Volume > 0);
            }

            musicSource = CreateAudioSourceAtPos(transform.position);
            musicSource.loop = true;
            musicSource.playOnAwake = false;
            musicSource.name = "music";

            tempAudioSources = new List<AudioSource>();
            currentMusic = music;
            ApplyVolume();
            PlayCurrentMusic();
        }

        protected virtual void OnDestroy()
        {
            SimpleTween.Cancel(musicVolumeTween, false);
        }

        protected virtual void OnValidate()
        {
            musicVolumeMult = Mathf.Clamp01(musicVolumeMult);
        }
        #endregion regular

        #region sound settings countrol
        public virtual void SetSound(bool on)
        {
            bool changed = (on != SoundOn);
            if (changed)
            {
                SoundOn = on;
                if (saveSettings) PlayerPrefs.SetInt(saveNameSound, (SoundOn) ? 1 : 0);
                if (!on) StopAllClip(false);
                ChangeSoundOnEvent?.Invoke(on);
            }
        }

        public virtual void SetMusic(bool on)
        {
            bool changed = (on != MusicOn);
            if (changed)
            {
                MusicOn = on;
                if (saveSettings) PlayerPrefs.SetInt(saveNameMusic, (MusicOn) ? 1 : 0);
                PlayCurrentMusic();
                ChangeMusicOnEvent?.Invoke(on);
            }
        }

        public void SetVolume(float volume)
        {
            bool onV1 = (Volume > 0);
            float vol = Mathf.Clamp(volume, 0, 1);
            bool changed = (vol != Volume);
            Volume = vol;
            bool onV2 = (Volume > 0);

            if (changed) ChangeVolumeEvent?.Invoke(Volume);
            if (onV1 != onV2) VolumeOnEvent?.Invoke(Volume > 0);

            if (!useMusicVolumeControl)
            {
                VolumeMusic = volume;
                if (changed) ChangeVolumeMusicEvent?.Invoke(VolumeMusic);
                if (onV1 != onV2) VolumeMusicOnEvent?.Invoke(VolumeMusic > 0);
            }
            if (saveSettings) PlayerPrefs.SetFloat(saveNameVolume, Volume);
            ApplyVolume();
        }

        public void SetVolumeMusic(float volume)
        {
            if (!useMusicVolumeControl) return;

            bool onV1 = (VolumeMusic > 0);
            float vol = Mathf.Clamp(volume, 0, 1);
            bool changed = (vol != VolumeMusic);
            VolumeMusic = vol;
            bool onV2 = (VolumeMusic > 0);

            if (changed) ChangeVolumeMusicEvent?.Invoke(VolumeMusic);
            if (onV1 != onV2) VolumeMusicOnEvent?.Invoke(VolumeMusic > 0);

            if (saveSettings) PlayerPrefs.SetFloat(saveNameVolumeMusic, VolumeMusic);
            ApplyVolume();

        }
        #endregion sound settings countrol

        #region play basic clips
        public void SoundPlayClipAtPos(float playDelay, AudioClip aC, Vector3 pos, float volumeMultiplier, Action callBack)
        {
            StartCoroutine(PlayClipAtPoint(playDelay, aC, pos, volumeMultiplier, callBack));
        }

        public void SoundPlayClick(float playDelay, Action callBack)
        {
            PlayClip(playDelay, buttonClick, callBack);
        }

        public void SoundPlayOpenWindow(float playDelay, Action callBack)
        {
            PlayClip(playDelay, openWindow, callBack);
        }

        public void SoundPlayCloseWindow(float playDelay, Action callBack)
        {
            PlayClip(playDelay, closeWindow, callBack);
        }
        #endregion play basic clips

        #region play clips

        /// <summary>
        /// Play clip at audiosource point
        /// </summary>
        /// <param name="delay"></param>
        /// <param name="clip"></param>
        /// <param name="completeCallBack"></param>
        public void PlayClip(float delay, AudioClip clip)
        {
            StartCoroutine(PlayClipAtPoint(delay, clip, transform.position, 1, null));
        }

        public void PlayClip(float delay, bool loop, AudioClip clip)
        {
            StartCoroutine(PlayClipAtPoint(delay, clip, transform.position, 1, loop, null));
        }

        /// <summary>
        /// Play clip at audiosource point
        /// </summary>
        /// <param name="delay"></param>
        /// <param name="clip"></param>
        /// <param name="completeCallBack"></param>
        public void PlayClip(float delay, AudioClip clip, Action completeCallBack)
        {
            StartCoroutine(PlayClipAtPoint(delay, clip, transform.position, 1, completeCallBack));
        }

        /// <summary>
        /// Play clip at position
        /// </summary>
        /// <param name="delay"></param>
        /// <param name="clip"></param>
        /// <param name="completeCallBack"></param>
        public void PlayClip(float delay, AudioClip clip, Vector3 position, Action completeCallBack)
        {
            StartCoroutine(PlayClipAtPoint(delay, clip, position, 1, completeCallBack));
        }

        protected IEnumerator PlayClipAtPoint(float playDelay, AudioClip aC, Vector3 pos, float volumeMultiplier, Action completeCallBack)
        {
            if (SoundOn && tempAudioSources.Count >= audioSourcesMaxCount)
            {

                // AudioSource[] aSs = GetComponentsInChildren<AudioSource>();
                if (tempAudioSources[0]) tempAudioSources[0].Stop();
            }

            if (SoundOn)
            {
                AudioSource aSt = CreateAudioSourceAtPos(pos);
                tempAudioSources.Add(aSt);
                aSt.volume = Volume * volumeMultiplier;

                float delay = 0f;
                while (delay < playDelay)
                {
                    delay += Time.deltaTime;
                    yield return wff;
                }

                if (aC)
                {
                    aSt.clip = aC;
                    aSt.Play();
                }

                while (aSt && aSt.isPlaying)
                    yield return wff;

                tempAudioSources.Remove(aSt);
                if (aSt) Destroy(aSt.gameObject);
                completeCallBack?.Invoke();
            }
            else
            {
                completeCallBack?.Invoke();
            }
        }

        protected IEnumerator PlayClipAtPoint(float playDelay, AudioClip aC, Vector3 pos, float volumeMultiplier, bool loop, Action completeCallBack)
        {
            if (SoundOn && tempAudioSources.Count >= audioSourcesMaxCount)
            {

                //  AudioSource[] aSs = GetComponentsInChildren<AudioSource>();
                if (tempAudioSources[0]) tempAudioSources[0].Stop();
            }

            if (SoundOn)
            {
                AudioSource aSt = CreateAudioSourceAtPos(pos);
                tempAudioSources.Add(aSt);
                aSt.volume = Volume * volumeMultiplier;
                aSt.loop = loop;

                float delay = 0f;
                while (delay < playDelay)
                {
                    delay += Time.deltaTime;
                    yield return wff;
                }

                if (aC)
                {
                    aSt.clip = aC;
                    aSt.Play();
                }

                while (aSt && aSt.isPlaying)
                    yield return wff;

                tempAudioSources.Remove(aSt);
                if (aSt) Destroy(aSt.gameObject);
                completeCallBack?.Invoke();
            }
            else
            {
                completeCallBack?.Invoke();
            }
        }

        /// <summary>
        /// Set new music and play
        /// </summary>
        /// <param name="newMusic"></param>
        public void SetMusicAndPlay(AudioClip newMusic)
        {
            if (!newMusic) return;
            currentMusic = newMusic;
            PlayCurrentMusic();
        }

        /// <summary>
        /// Play current music clip
        /// </summary>
        public void PlayCurrentMusic()
        {
            if (!musicSource || !currentMusic) return;
            SimpleTween.Cancel(musicVolumeTween, true);
            float volume = (useMusicVolumeControl) ? VolumeMusic * musicVolumeMult : Volume * musicVolumeMult;
            if (MusicOn)
            {
                if ((currentMusic == musicSource.clip) && musicSource.isPlaying) // check volume
                {
                    float vol = musicSource.volume;
                    if (vol != volume) musicVolumeTween = SimpleTween.Value(gameObject, vol, volume, 1f).
                            SetOnUpdate((float val) => { if (musicSource) musicSource.volume = val; }).
                            AddCompleteCallBack(() => { if (musicSource) musicSource.volume = volume; }).ID;

                }

                if ((currentMusic != musicSource.clip) && musicSource.isPlaying)
                {
                    musicSource.Stop();
                    musicSource.clip = currentMusic;
                    musicSource.Play();
                    float vol = musicSource.volume;
                    if (vol != volume) musicVolumeTween = SimpleTween.Value(gameObject, vol, volume, 1f).
                            SetOnUpdate((float val) => { if (musicSource) musicSource.volume = val; }).
                            AddCompleteCallBack(() => { if (musicSource) musicSource.volume = volume; }).ID;
                }

                if (!musicSource.isPlaying)
                {
                    musicSource.clip = currentMusic;
                    musicSource.volume = 0;
                    musicSource.Play();
                    musicVolumeTween = SimpleTween.Value(gameObject, 0.0f, volume, 1f).
                            SetOnUpdate((float val) => { if (musicSource) musicSource.volume = val; }).
                            AddCompleteCallBack(() => { if (musicSource) musicSource.volume = volume; }).ID;
                }
            }
            else
            {
                StopMusic();
            }
        }
        #endregion play clips

        #region stop clips
        /// <summary>
        /// Stop all clips with or without backround music
        /// </summary>
        /// <param name="stopMusic"></param>
        public void StopAllClip(bool stopMusic)
        {
            if (musicSource && stopMusic) StopMusic();

            AudioSource[] aSs = GetComponentsInChildren<AudioSource>();
            if (aSs != null)
            {
                foreach (var item in aSs)
                {
                    if (item && (item != musicSource)) item.Stop();
                }
            }
        }

        /// <summary>
        /// Stop music audiosource
        /// </summary>
        public void StopMusic()
        {
            SimpleTween.Cancel(musicVolumeTween, true);
            if (musicSource && musicSource.isPlaying)
            {
                musicVolumeTween = SimpleTween.Value(gameObject, Volume * musicVolumeMult, 0.0f, 1f).
                    SetOnUpdate((float val) => { if (musicSource) musicSource.volume = val; }).
                    AddCompleteCallBack(() => { if (musicSource) musicSource.Stop(); musicSource.volume = 0; }).ID;

            }
        }

        public void ForceStopMusic()
        {
            SimpleTween.Cancel(musicVolumeTween, true);
            if (musicSource && musicSource.isPlaying)
            {
                musicSource.Stop();
                musicSource.volume = 0;
            }
        }

        /// <summary>
        /// Stop all audiosources with clip
        /// </summary>
        /// <param name="clip"></param>
        public void StopClips(AudioClip clip)
        {
            if (clip == null) return;
            AudioSource[] aSs = GetComponentsInChildren<AudioSource>();
            if (aSs != null)
            {
                foreach (var item in aSs)
                {
                    if (item && (item != musicSource) && (item.clip == clip)) item.Stop();
                }
            }
        }
        #endregion stop clips

        #region private
        private void ApplyVolume()
        {
            if (musicSource)
            {
                musicSource.volume = (useMusicVolumeControl) ? VolumeMusic * musicVolumeMult : Volume * musicVolumeMult;
            }

            AudioSource[] aSs = GetComponentsInChildren<AudioSource>();
            if (aSs != null)
            {
                foreach (var item in aSs)
                {
                    if (item && (item != musicSource)) item.volume = Volume;
                }
            }
        }

        private AudioSource CreateAudioSourceAtPos(Vector3 pos)
        {
            GameObject aS = new GameObject();
            aS.transform.position = pos;
            aS.transform.parent = transform;
            return aS.AddComponent<AudioSource>();
        }
        #endregion private
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(SoundMaster))]
    public class SoundMasterEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            if (EditorApplication.isPlaying)
            {
                SoundMaster script = (SoundMaster)target;
                if (script)
                {
                    GUILayout.Label("Testing, music - " + script.MusicOn + ", sound - " + script.SoundOn);
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Play click"))
                    {
                        script.SoundPlayClick(0, null);
                    }
                    if (GUILayout.Button(!script.MusicOn ? "MusicOn" : "MusicOff"))
                    {
                        script.SetMusic(!script.MusicOn);
                    }
                    if (GUILayout.Button(!script.SoundOn ? "SoundOn" : "SoundOff"))
                    {
                        script.SetSound(!script.SoundOn);
                    }
                    GUILayout.EndHorizontal();
                }
            }
            else
            {
                GUILayout.Label("Goto play mode for test sounds");
            }
        }
    }
#endif
}