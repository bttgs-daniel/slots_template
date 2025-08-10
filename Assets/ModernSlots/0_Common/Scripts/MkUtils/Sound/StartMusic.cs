using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
  12.08.2024
  20.10.2020 - first
 */
namespace Mkey
{
    public class StartMusic : MonoBehaviour
    {
        [SerializeField]
        private AudioClip startClip;

        #region temp vars
        private SoundMaster MSound { get { return SoundMaster.Instance; } }
        #endregion temp vars

        IEnumerator Start()
        {
            while (!MSound) yield return null;
            yield return null;
            MSound.SetMusicAndPlay(startClip);
        }
    }
}
