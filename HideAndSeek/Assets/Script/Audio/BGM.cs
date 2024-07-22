using Scene;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Audio
{
    /// <summary>
    /// BGMの再生処理
    /// </summary>
    public class BGM : MonoBehaviour
    {
        #region PublicField
        public static BGM instance = null;
        #endregion

        #region PrivateField
        private Dictionary<string, AudioClip> sceneBGMMap;
        #endregion

        #region SerializeField
        [SerializeField] private AudioSource bgm;
        /// <summary>各シーンのBGM</summary>
        [SerializeField] private List<SceneBGM> sceneBGMList;
        #endregion

        #region UnityEvent
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeSceneBGMMap();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        void Start()
        {
            // シーン開始時にBGMを再生する処理
            PlayBGMForCurrentScene();
        }

        /// <summary>
        /// シーンが有効化された時にイベント登録
        /// </summary>
        private void OnEnable()
        {
            SceneLoader.Instance().OnSceneLoaded += OnSceneLoaded;
        }

        /// <summary>
        /// シーンが無効化された時にイベント解除
        /// </summary>
        private void OnDisable()
        {
            if (SceneLoader.Instance() != null)
            {
                SceneLoader.Instance().OnSceneLoaded -= OnSceneLoaded;
            }
        }
        #endregion

        #region PrivateMethod
        /// <summary>
        /// シーンごとにBGMをマップに初期化する処理
        /// </summary>
        private void InitializeSceneBGMMap()
        {
            sceneBGMMap = new Dictionary<string, AudioClip>();
            foreach (var sceneBGM in sceneBGMList)
            {
                if (!sceneBGMMap.ContainsKey(sceneBGM.sceneName))
                {
                    sceneBGMMap.Add(sceneBGM.sceneName, sceneBGM.bgmClip);
                }
            }
        }

        /// <summary>
        /// シーンが読み込まれた時に呼び出される処理
        /// </summary>
        /// <param name="sceneName">シーンの名前</param>
        private void OnSceneLoaded(string sceneName)
        {
            PlayBGMForScene(sceneName);
        }

        /// <summary>
        /// 現在のシーンに対応するBGMを再生する処理
        /// </summary>
        private void PlayBGMForCurrentScene()
        {
            PlayBGMForScene(SceneManager.GetActiveScene().name);
        }

        /// <summary>
        /// 指定されたシーンのBGMを再生する処理
        /// </summary>
        /// <param name="sceneName">シーンの名前</param>
        private void PlayBGMForScene(string sceneName)
        {
            if (sceneBGMMap.TryGetValue(sceneName, out var bgmClip))
            {
                if (bgm.clip != bgmClip)
                {
                    bgm.clip = bgmClip;
                    bgm.Play();
                }
            }
        }
        #endregion

        /// <summary>
        /// シーンごとのBGM情報を格納する
        /// </summary>
        [System.Serializable]
        public struct SceneBGM
        {
            public string sceneName;
            public AudioClip bgmClip;
        }
    }
}