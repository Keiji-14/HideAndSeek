using GameData;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// ゲーム画面の処理管理
    /// </summary>
    public class GameController : MonoBehaviourPunCallbacks
    {
        #region PrivateField
        /// <summary>ゲームが開始されたかどうかのフラグ</summary>
        private bool gameStarted = false;
        /// <summary>サーバー時間を保持する変数</summary>
        private double startTime;
        /// <summary>残りの猶予時間</summary>
        private float graceRemainingTime;
        /// <summary>ゲームの残り時間</summary>
        private float gameRemainingTime;
        /// <summary>上空視点カメラ</summary>
        private Camera overheadCamera;
        /// <summary>隠れる側のIDリスト</summary>
        private List<int> capturedHiderIDs = new List<int>();
        /// <summary>生成した隠れる側のプレイヤーリスト</summary>
        private List<GameObject> hiderPlayerList = new List<GameObject>();
        #endregion

        #region SerializeField
        /// <summary>隠れる時の猶予時間</summary>
        [SerializeField] private float gracePeriodSeconds;
        /// <summary>ゲーム全体の制限時間</summary>
        [SerializeField] private float gameTimeSeconds;
        [Header("Player Prefab")]
        /// <summary>探す側のプレイヤーオブジェクト</summary>
        [SerializeField] private GameObject seekerPrefab;
        /// <summary>隠れる側のプレイヤーオブジェクト</summary>
        [SerializeField] private GameObject hiderPrefab;
        /// <summary>隠れる側のボットのプレイヤーオブジェクト</summary>
        [SerializeField] private GameObject hiderBotPrefab;
        /// <summary>上空カメラのオブジェクト</summary>
        [SerializeField] private GameObject overheadCameraPrefab;
        /// <summary>上空カメラのオブジェクトステージ情報</summary>
        [SerializeField] private StageData stageData;
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
            if (PhotonNetwork.IsMasterClient)
            {
                Debug.Log("PhotonNetwork.IsMasterClient");
                // マスタークライアントがステージデータを生成して他のプレイヤーに共有する
                //photonView.RPC("RPC_SetStageData", RpcTarget.AllBuffered, stageData.stageID);
            }
            else
            {
                Debug.Log("PhotonNetwork.IsMasterClient not");
            }

            StartCoroutine(WaitForCustomProperties());
        }
        #endregion

        #region PrivateMethod
        /// <summary>
        /// RPCでステージデータを設定する処理
        /// </summary>
        [PunRPC]
        private void RPC_SetStageData(int stageID)
        {
            // ステージオブジェクトの生成
            if (stageData != null && stageData.stageObj != null)
            {
                GameDataManager.Instance().SetStagerData(stageData);

                // ネットワーク経由でインスタンス化
                Instantiate(stageData.stageObj, Vector3.zero, Quaternion.identity);
            }
        }

        private IEnumerator WaitForCustomProperties()
        {
            while (!PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("Role"))
            {
                yield return null;
            }

            Debug.Log(PhotonNetwork.LocalPlayer.CustomProperties["Role"].ToString());

            if (PhotonNetwork.LocalPlayer.CustomProperties["Role"].ToString() == "Seeker")
            {
                SpawnSeekerPlayer(seekerPrefab);
                StartCoroutine(GracePeriodSeekerCoroutine());
            }
            else
            {
                SpawnHiderPlayer(hiderPrefab);
                StartCoroutine(GracePeriodHiderCoroutine());
            }

            /*if (PhotonNetwork.IsMasterClient)
            {
                int numberOfBots = 3; // 生成するボットの数を指定
                SpawnHiderBots(numberOfBots); // ボットを生成
            }*/
        }

        /// <summary>
        /// 鬼側のプレイヤーオブジェクトを生成する処理
        /// </summary>
        /// <param name="prefab">生成するプレイヤーオブジェクト</param>
        private void SpawnSeekerPlayer(GameObject prefab)
        {
            if (!HasSpawnedPlayer(PhotonNetwork.LocalPlayer))
            {
                var position = new Vector3(Random.Range(-3f, 3f), 3f, Random.Range(-3f, 3f));
                var playerObject = PhotonNetwork.Instantiate($"Prefabs/{prefab.name}", position, Quaternion.identity);

                // TagObjectに生成したプレイヤーオブジェクトを設定
                PhotonNetwork.LocalPlayer.TagObject = playerObject;
            }
        }

        /// <summary>
        /// 隠れる側のプレイヤーオブジェクトを生成する処理
        /// </summary>
        /// <param name="prefab">生成するプレイヤーオブジェクト</param>
        private void SpawnHiderPlayer(GameObject prefab)
        {
            if (!HasSpawnedPlayer(PhotonNetwork.LocalPlayer))
            {
                var position = new Vector3(Random.Range(-3f, 3f), 3f, Random.Range(-3f, 3f));
                var playerObject = PhotonNetwork.Instantiate($"Prefabs/{prefab.name}", position, Quaternion.identity);
                // TagObjectに生成したプレイヤーオブジェクトを設定
                PhotonNetwork.LocalPlayer.TagObject = playerObject;
                hiderPlayerList.Add(playerObject);
            }
        }

        /// <summary>
        /// ボットを生成する処理
        /// </summary>
        private void SpawnHiderBots(int numberOfBots)
        {
            for (int i = 0; i < numberOfBots; i++)
            {
                var position = new Vector3(Random.Range(-3f, 3f), 3f, Random.Range(-3f, 3f));
                var botObject = PhotonNetwork.Instantiate($"Prefabs/{hiderBotPrefab.name}", position, Quaternion.identity);
                hiderPlayerList.Add(botObject);
            }
        }

        private bool HasSpawnedPlayer(Player player)
        {
            return player.TagObject != null;
        }

        /// <summary>
        /// 猶予時間中の鬼側の処理
        /// </summary>
        /// <returns></returns>
        private IEnumerator GracePeriodSeekerCoroutine()
        {
            // 上空視点カメラ
            overheadCamera = Instantiate(overheadCameraPrefab).GetComponent<Camera>();
            // プレイヤー生成待ちのため少し待つ
            yield return new WaitForSeconds(0.5f);

            // 自プレイヤーのSeekerControllerを取得して移動を無効にする
            GameObject playerObject = PhotonNetwork.LocalPlayer.TagObject as GameObject;
            if (playerObject != null)
            {
                SeekerController seekerController = playerObject.GetComponent<SeekerController>();
                if (seekerController != null)
                {
                    seekerController.enabled = false;
                }
            }

            var hiders = GameObject.FindGameObjectsWithTag("Hider");
            // 隠れる側のプレイヤーを見えなくする
            foreach (var hider in hiders)
            {
                // オブジェクトごと非表示にする
                SetActiveRecursively(hider, false);
            }

            // MasterClientが基準時間を送信
            if (PhotonNetwork.IsMasterClient)
            {
                double masterStartTime = PhotonNetwork.Time;
                photonView.RPC("RPC_StartGracePeriod", RpcTarget.All, masterStartTime);
            }

            yield return null;
        }

        [PunRPC]
        private void RPC_StartGracePeriod(double masterStartTime)
        {
            startTime = masterStartTime;
            graceRemainingTime = gracePeriodSeconds - (float)(PhotonNetwork.Time - masterStartTime);
            gameUI.UpdateGraceTimer(graceRemainingTime);

            Observable.EveryUpdate().Subscribe(_ =>
            {
                graceRemainingTime = gracePeriodSeconds - (float)(PhotonNetwork.Time - startTime);
                gameUI.UpdateGraceTimer(graceRemainingTime);
                if (graceRemainingTime <= 0)
                {
                    if (PhotonNetwork.IsMasterClient)
                    {
                        photonView.RPC("RPC_StartGame", RpcTarget.All, startTime);
                    }
                }
            }).AddTo(this);
        }

        [PunRPC]
        private void RPC_StartGame(double masterStartTime)
        {
            // 猶予時間終了後の処理
            var hiders = GameObject.FindGameObjectsWithTag("Hider");
            foreach (var hider in hiders)
            {
                // オブジェクトを再表示
                SetActiveRecursively(hider, true);
                var hiderController = hider.GetComponent<HiderController>();

                if (hiderController != null)
                {
                    hiderController.SetCamera();
                }
            }

            // 自プレイヤーのSeekerControllerを有効にする
            GameObject playerObject = PhotonNetwork.LocalPlayer.TagObject as GameObject;
            if (playerObject != null)
            {
                SeekerController seekerController = playerObject.GetComponent<SeekerController>();
                if (seekerController != null)
                {
                    seekerController.enabled = true;
                }
            }

            // ゲーム開始
            StartGameTimer(masterStartTime);
        }

        /// <summary>
        /// 猶予時間中の隠れる側の処理
        /// </summary>
        /// <returns></returns>
        private IEnumerator GracePeriodHiderCoroutine()
        {
            // MasterClientが基準時間を送信
            if (PhotonNetwork.IsMasterClient)
            {
                double masterStartTime = PhotonNetwork.Time;
                photonView.RPC("RPC_StartGracePeriod", RpcTarget.All, masterStartTime);
            }

            yield return null;
        }

        private void SetActiveRecursively(GameObject obj, bool state)
        {
            obj.SetActive(state);
            foreach (Transform child in obj.transform)
            {
                SetActiveRecursively(child.gameObject, state);
            }
        }

        /// <summary>
        /// ゲーム開始後のタイマー処理を行う
        /// </summary>
        private void StartGameTimer(double masterStartTime)
        {
            gameStarted = true;
            gameRemainingTime = gameTimeSeconds;
            startTime = masterStartTime;

            Observable.EveryUpdate().Subscribe(_ =>
            {
                if (gameStarted)
                {
                    gameRemainingTime = gameTimeSeconds - (float)(PhotonNetwork.Time - startTime);
                    gameUI.UpdateGameTimer(gameRemainingTime);
                    if (gameRemainingTime <= 0)
                    {
                        gameStarted = false;
                        GameOver(false);
                    }
                }
            }).AddTo(this);
        }

        /// <summary>
        /// プレイヤーが捕まった時の処理
        /// </summary>
        public void OnPlayerCaught(int hiderViewID)
        {
            if (gameStarted)
            {
                if (!capturedHiderIDs.Contains(hiderViewID))
                {
                    capturedHiderIDs.Add(hiderViewID);
                    photonView.RPC("RPC_OnPlayerCaught", RpcTarget.All, hiderViewID);

                    if (capturedHiderIDs.Count >= hiderPlayerList.Count)
                    {
                        gameStarted = false;
                        GameOver(true);
                    }
                }
            }
        }

        [PunRPC]
        private void RPC_OnPlayerCaught(int hiderViewID)
        {
            GameObject hider = PhotonView.Find(hiderViewID).gameObject;
            if (hider != null)
            {
                if (PhotonNetwork.LocalPlayer.ActorNumber == hider.GetComponent<PhotonView>().Owner.ActorNumber)
                {
                    SetSpectatorMode();
                }

                // Hiderを削除
                PhotonNetwork.Destroy(hider);
            }
        }

        /// <summary>
        /// 観戦モードにする
        /// </summary>
        private void SetSpectatorMode()
        {
            overheadCamera = Instantiate(overheadCameraPrefab).GetComponent<Camera>();
        }

        /// <summary>
        /// ゲームオーバー時の処理
        /// </summary>
        /// <param name="isSeekerWin">鬼の勝利かどうかのフラグ</param>
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
        }
        #endregion

    }
}