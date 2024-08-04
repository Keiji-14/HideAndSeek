using System.Collections.Generic;
using UnityEngine;

namespace Audio
{    
    /// <summary>
    /// 効果音の再生処理
    /// </summary>
    public class SE : MonoBehaviour
    {
        #region PublicField
        /// <summary>シングルトン</summary>
        public static SE instance = null;
        #endregion

        #region SerializeField
        /// <summary>効果音</summary>
        [SerializeField] private AudioSource audioSource;
        /// <summary>各効果音のLリスト</summary>
        [SerializeField] private List<AudioClip> seClipList;
        #endregion

        #region UnityEvent
        private void Awake()
        {
            // シングルトンの実装
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        #endregion

        #region PublicMethod
        public enum SEName
        {
            ButtonSE,
            AttackSE,
            MatchingSE,
            CorrectSE,
            WhistleSE,
        }

        /// <summary>
        /// SEを再生
        /// </summary>
        /// <param name="seName">効果音名</param>
        public void Play(SEName seName)
        {
            audioSource.PlayOneShot(seClipList[(int)seName]);
        }

        /// <summary>
        /// SEを再生
        /// </summary>
        /// <param name="audioClip">効果音</param>
        public void Play(AudioClip audioClip)
        {
            audioSource.PlayOneShot(audioClip);
        }
        #endregion
    }
}