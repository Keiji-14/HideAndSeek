using Photon.Pun;
using System.Collections;
using UniRx;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// ゲーム画面の処理管理
    /// </summary>
    public class GameController : MonoBehaviour
    {
        #region PrivateField
        private bool gameStarted = false;
        private float remainingTime;
        #endregion

        #region SerializeField
        /// <summary>隠れる時間</summary>
        [SerializeField] private float gracePeriodSeconds;
        /// <summary>ゲームの制限時間</summary>
        [SerializeField] private float gameTimeSeconds;
        [Header("Player Prefab")]
        /// <summary>探す側のプレイヤーオブジェクト</summary>
        [SerializeField] private GameObject seekerPrefab;
        /// <summary>隠れる側のプレイヤーオブジェクト</summary>
        [SerializeField] private GameObject hiderPrefab;
        [Header("Component")]
        /// <summary>ゲームUI</summary>
        [SerializeField] private GameUI gameUI;
        #endregion

        #region PublicMethod
        /// <summary>
        /// 初期化
        /// </summary>
        public void Init()
        {
            if (PhotonNetwork.LocalPlayer.CustomProperties["Role"].ToString() == "Seeker")
            {
                SpawnPlayer(seekerPrefab);
            }
            else
            {
                SpawnPlayer(hiderPrefab);
            }

            // 隠れる側の時間を開始
            StartCoroutine(GracePeriodCoroutine());
        }
        #endregion

        #region PrivateMethod
        private void SpawnPlayer(GameObject prefab)
        {
            var position = new Vector3(Random.Range(-3f, 3f), 3f, Random.Range(-3f, 3f));
            PhotonNetwork.Instantiate($"Prefabs/{prefab.name}", position, Quaternion.identity);
        }

        private IEnumerator GracePeriodCoroutine()
        {
            // 猶予時間中のカウントダウン表示
            remainingTime = gracePeriodSeconds;
            while (remainingTime > 0)
            {
                remainingTime -= Time.deltaTime;
                gameUI.UpdateTimer(remainingTime); // GameUIに残り時間を渡す
                yield return null;
            }

            // ゲーム開始
            gameStarted = true;
            remainingTime = gameTimeSeconds;
            Observable.EveryUpdate().Subscribe(_ =>
            {
                if (gameStarted)
                {
                    remainingTime -= Time.deltaTime;

                    // GameUIに残り時間を渡す
                    gameUI.UpdateTimer(remainingTime);
                    if (remainingTime <= 0)
                    {
                        gameStarted = false;
                        GameOver(false);
                    }
                }
            }).AddTo(this);
        }

        public void OnPlayerCaught()
        {
            if (gameStarted)
            {
                gameStarted = false;
                GameOver(true);
            }
        }

        private void GameOver(bool isSeekerWin)
        {
            // ゲーム終了処理
            if (isSeekerWin)
            {
                Debug.Log("Seeker Wins!");
            }
            else
            {
                Debug.Log("Hiders Win!");
            }

            // ここでシーン遷移やリザルト表示を行う
        }
        #endregion

    }
}