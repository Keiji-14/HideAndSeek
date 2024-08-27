using GameData;
using Scene;
using Audio;
using Player;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// ゲーム画面の処理を管理する
    /// </summary>
    public class GameController : MonoBehaviourPunCallbacks
    {
        #region PrivateField
        /// <summary>隠れる側のプレイヤー数</summary>
        private int hiderPlayerCount;
        /// <summary>猶予時間が開始されたかどうか</summary>
        private bool isGracePeriodStarted = false;
        /// <summary>ゲームが開始されたかどうか</summary>
        private bool isGameStarted = false;
        /// <summary>ゲームが終了されたかどうか</summary>
        private bool isGameFinished = false;
        /// <summary>サーバー時間を保持する変数</summary>
        private double startTime;
        /// <summary>残りの待機時間</summary>
        private float standbyRemainingTime;
        /// <summary>残りの猶予時間</summary>
        private float graceRemainingTime;
        /// <summary>ゲームの残り時間</summary>
        private float gameRemainingTime;
        /// <summary>上空視点カメラ</summary>
        private Camera overheadCamera;
        /// <summary>捕まえたプレイヤーのIDリスト</summary>
        private List<int> capturedHiderIDList = new List<int>();
        /// <summary>隠れる側のプレイヤーリスト</summary>
        private List<GameObject> hiderPlayerObjectList = new List<GameObject>();
        #endregion

        #region SerializeField
        /// <summary>遷移後のプレイヤー待機時間</summary>
        [SerializeField] private float standbySeconds;
        /// <summary>隠れる時の猶予時間</summary>
        [SerializeField] private float gracePeriodSeconds;
        /// <summary>ゲーム全体の制限時間</summary>
        [SerializeField] private float gameTimeSeconds;
        [Header("Player Prefab")]
        /// <summary>鬼側のプレイヤーオブジェクト</summary>
        [SerializeField] private GameObject seekerPrefab;
        /// <summary>隠れる側のプレイヤーオブジェクト</summary>
        [SerializeField] private GameObject hiderPrefab;
        /// <summary>隠れる側のボットのプレイヤーオブジェクト</summary>
        [SerializeField] private GameObject hiderBotPrefab;
        /// <summary>上空カメラのオブジェクト</summary>
        [SerializeField] private GameObject overheadCameraPrefab;
        /// <summary>消滅時のエフェクト</summary>
        [SerializeField] private GameObject destroyEffectObj;
        [Header("Component")]
        /// <summary>待機中のカメラ</summary>
        [SerializeField] private Camera standbyCamera;
        /// <summary>ゲームUI</summary>
        [SerializeField] private GameUI gameUI;
        #endregion

        #region PublicMethod
        /// <summary>
        /// 初期化
        /// </summary>
        public void Init()
        {
            // MasterClientがステージを設定
            if (PhotonNetwork.IsMasterClient)
            {
                double masterStartTime = PhotonNetwork.Time;

                // ランダムにステージを選出
                var stageDataList = GameDataManager.Instance().GetStageDatabase().stageDataList;
                var stageNum = Random.Range(0, stageDataList.Count);

                PhotonNetwork.Instantiate($"Prefabs/Stage/{stageDataList[stageNum].name}/{stageDataList[stageNum].name}", Vector3.zero, Quaternion.identity);

                photonView.RPC("RPC_SetStageData", RpcTarget.All, stageNum);

                photonView.RPC("RPC_PlayerStandby", RpcTarget.All, masterStartTime);
            }
        }

        /// <summary>
        /// プレイヤーが捕まえた時の処理
        /// </summary>
        /// <param name="seekerViewID">捕まえた鬼のPhotonViewID</param>
        /// <param name="hiderViewID">捕まったプレイヤーのPhotonViewID</param>
        public void OnPlayerCaught(int seekerViewID, int hiderViewID)
        {
            if (isGameStarted && !capturedHiderIDList.Contains(hiderViewID))
            {
                capturedHiderIDList.Add(hiderViewID);

                GameObject hiderPlayer = PhotonView.Find(hiderViewID).gameObject;
                var bot = hiderPlayer.GetComponent<HiderBotController>();

                if (bot == null)
                {
                    if (PhotonNetwork.LocalPlayer.ActorNumber == hiderPlayer.GetComponent<PhotonView>().Owner.ActorNumber)
                    {
                        SetSpectatorMode();
                    }
                }

                // 捕まえた通知を送信
                string hiderName = hiderPlayer.GetComponent<PhotonView>().Owner.NickName;

                photonView.RPC("RPC_DisplayCaughtMessage", RpcTarget.All, seekerViewID, hiderViewID, hiderName);

                // 全プレイヤーに隠れ側の数を更新
                var subtractHider = -1;
                photonView.RPC("RPC_UpdateHiderCount", RpcTarget.All, subtractHider);
                // 全プレイヤーに捕まえた事を通知
                photonView.RPC("RPC_OnPlayerCaught", RpcTarget.All, hiderViewID);
            }
        }

        /// <summary>
        /// 鬼プレイヤーのライフが0になった時の処理
        /// </summary>
        /// <param name="seekerViewID">鬼プレイヤーのPhotonViewID</param>
        public void SeekerFailed(int seekerViewID)
        {
            if (isGameStarted)
            {
                photonView.RPC("RPC_DestroySeeker", RpcTarget.All, seekerViewID);
            }
        }
        #endregion

        #region PrivateMethod
        /// <summary>
        /// RPCでステージデータを設定する処理
        /// </summary>
        /// <param name="stageNum">ステージ番号</param>
        [PunRPC]
        private void RPC_SetStageData(int stageNum)
        {
            var stageDataList = GameDataManager.Instance().GetStageDatabase().stageDataList;
            // ステージ情報の設定
            GameDataManager.Instance().SetStagerData(stageDataList[stageNum]);
        }

        /// <summary>
        /// RPCで全プレイヤーに一定時間待機させる
        /// </summary>
        /// <param name="masterStartTime">マスタークライアントの開始時間</param>
        [PunRPC]
        private void RPC_PlayerStandby(double masterStartTime)
        {
            // 各時間の設定を初期化
            startTime = masterStartTime;
            standbyRemainingTime = standbySeconds;
            graceRemainingTime = gracePeriodSeconds;
            gameRemainingTime = gameTimeSeconds;

            // 待機中のCanvasを表示する
            gameUI.SwicthGameCanvas(true);
            // 各時間のUIを設定
            gameUI.UpdateStandbyTimer(standbyRemainingTime);
            gameUI.UpdateGraceTimer(graceRemainingTime);
            gameUI.UpdateGameTimer(gameRemainingTime);

            Observable.EveryUpdate().Subscribe(_ =>
            {
                if (!isGracePeriodStarted)
                {
                    standbyRemainingTime = standbySeconds - (float)(PhotonNetwork.Time - (startTime));
                    gameUI.UpdateStandbyTimer(standbyRemainingTime);

                    if (standbyRemainingTime <= 0)
                    {
                        isGracePeriodStarted = true;

                        StartCoroutine(SpawnSeekerPlayers());
                    }
                }
            }).AddTo(this);
        }

        /// <summary>
        /// プレイヤー達を生成する処理
        /// </summary>
        /// <returns></returns>
        private IEnumerator SpawnSeekerPlayers()
        {
            while (!PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("Role"))
            {
                yield return null;
            }

            gameUI.SwicthRoleCanvas(PhotonNetwork.LocalPlayer.CustomProperties["Role"].ToString());

            // 鬼側か隠れる側かを判定する処理
            if (PhotonNetwork.LocalPlayer.CustomProperties["Role"].ToString() == "Seeker")
            {
                SpawnSeekerPlayer(seekerPrefab);
                StartCoroutine(GracePeriodSeeker());
            }
            else if (PhotonNetwork.LocalPlayer.CustomProperties["Role"].ToString() == "Hider")
            {
                SpawnHiderPlayer(hiderPrefab);
            }

            if (PhotonNetwork.IsMasterClient)
            {
                // 隠れる側のプレイヤー数に応じてボットの数を決定
                int hiderCount = PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("HiderCount", out object hiderCountObj) ? (int)hiderCountObj : 0;
                // 足りない分のボットを生成
                int numberOfBots = Mathf.Max(0, 4 - hiderCount);
                SpawnHiderBots(numberOfBots);
                photonView.RPC("RPC_StartGracePeriod", RpcTarget.All);
            }
        }

        /// <summary>
        /// 鬼側のプレイヤーオブジェクトを生成する処理
        /// </summary>
        /// <param name="prefab">生成するプレイヤーオブジェクト</param>
        private void SpawnSeekerPlayer(GameObject prefab)
        {
            if (!HasSpawnedPlayer(PhotonNetwork.LocalPlayer))
            {
                var stageData = GameDataManager.Instance().GetStageData();

                var position = stageData.seekerStartArea.position;
                var playerObject = PhotonNetwork.Instantiate($"Prefabs/{prefab.name}", position, Quaternion.identity);
                // TagObjectに生成したプレイヤーオブジェクトを設定
                PhotonNetwork.LocalPlayer.TagObject = playerObject;

                SeekerController seekerController = playerObject.GetComponent<SeekerController>();
                 
                if (seekerController != null)
                {
                    // 鬼のAudioListenerを無効化にする（上空視点のAudioListenerを使用）
                    seekerController.SwitchAudioListener(false);
                }

                StartCoroutine(gameUI.ViewGraceText("他の人が隠れている間にマップを覚えましょう"));
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
                var position = new Vector3(Random.Range(-4f, 4f), 3f, Random.Range(-8f, 8f));
                var playerObject = PhotonNetwork.Instantiate($"Prefabs/{prefab.name}", position, Quaternion.identity);
                // TagObjectに生成したプレイヤーオブジェクトを設定
                PhotonNetwork.LocalPlayer.TagObject = playerObject;

                var addHider = 1;

                StartCoroutine(gameUI.ViewGraceText("鬼から見つからないように隠れましょう"));
                photonView.RPC("RPC_UpdateHiderCount", RpcTarget.All, addHider);
            }
        }

        /// <summary>
        /// ボットを生成する処理
        /// </summary>
        /// <param name="botNum">生成するボットの数</param>
        private void SpawnHiderBots(int botNum)
        {
            for (int i = 0; i < botNum; i++)
            {
                var position = new Vector3(Random.Range(-3f, 3f), 3f, Random.Range(-3f, 3f));
                var botObject = PhotonNetwork.Instantiate($"Prefabs/{hiderBotPrefab.name}", position, Quaternion.identity);
                var botPlayerObject = botObject.GetComponent<HiderBotController>();
                botPlayerObject.Init("bot");

                var addHider = 1;
                photonView.RPC("RPC_UpdateHiderCount", RpcTarget.All, addHider);
            }
        }

        /// <summary>
        /// 隠れる側のプレイヤー数を更新する処理
        /// </summary>
        /// <param name="changeValue">変更する数値</param>
        [PunRPC]
        private void RPC_UpdateHiderCount(int changeValue)
        {
            hiderPlayerCount += changeValue;
            gameUI.UpdateHider(hiderPlayerCount);
        }

        /// <summary>
        /// プレイヤーが既に生成されているかを判定する処理
        /// </summary>
        /// <param name="player">チェックするプレイヤー</param>
        /// <returns>生成されている場合はtrue</returns>
        private bool HasSpawnedPlayer(Photon.Realtime.Player player)
        {
            return player.TagObject != null;
        }

        /// <summary>
        /// 猶予時間中の鬼側の処理
        /// </summary>
        /// <returns></returns>
        private IEnumerator GracePeriodSeeker()
        {
            // 上空視点カメラ
            overheadCamera = Instantiate(overheadCameraPrefab).GetComponent<Camera>();
            // プレイヤー生成待ちのため少し待つ
            yield return new WaitForSeconds(1f);

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

            // リストをクリア
            hiderPlayerObjectList.Clear();
            var hiders = GameObject.FindGameObjectsWithTag("Hider");
            Debug.Log("Hiders found: " + hiders.Length);
            // 隠れる側のプレイヤーを見えなくする
            foreach (var hider in hiders)
            {
                HiderController hiderController = hider.GetComponent<HiderController>();
                HiderBotController hiderBotController = hider.GetComponent<HiderBotController>();
                
                // プレイヤーかボットの場合は非表示にする
                if (hiderController != null)
                {
                    hiderController.HidePlayer();
                }
                else if (hiderBotController != null)
                {
                    hiderBotController.HideBotPlayer();
                }
                
                hiderPlayerObjectList.Add(hider);
            }
        }

        /// <summary>
        /// RPCで猶予時間を開始する
        /// </summary>
        [PunRPC]
        private void RPC_StartGracePeriod()
        {
            // ゲーム中のCanvasを表示する
            gameUI.SwicthGameCanvas(false);

            if (standbyCamera != null)
            {
                // 待機中のカメラを削除する
                Destroy(standbyCamera.gameObject);
                standbyCamera = null;
            }

            Observable.EveryUpdate().Subscribe(_ =>
            {
                if (!isGameStarted)
                {
                    graceRemainingTime = gracePeriodSeconds - (float)(PhotonNetwork.Time - (startTime + standbySeconds));
                    gameUI.UpdateGraceTimer(graceRemainingTime);

                    if (graceRemainingTime <= 0)
                    {
                        isGameStarted = true;

                        if (PhotonNetwork.IsMasterClient)
                        {
                            photonView.RPC("RPC_StartGame", RpcTarget.All);
                        }
                    }
                }
            }).AddTo(this);
        }

        /// <summary>
        /// RPCでゲームを開始する処理
        /// </summary>
        [PunRPC]
        private void RPC_StartGame()
        {
            SE.instance.Play(SE.SEName.WhistleSE);

            // 開始時の鬼の処理
            if (PhotonNetwork.LocalPlayer.CustomProperties["Role"].ToString() == "Seeker")
            {
                StarGameSeekerInit();
            }

            Observable.EveryUpdate().Subscribe(_ =>
            {
                if (!isGameFinished)
                {
                    gameRemainingTime = gameTimeSeconds - (float)(PhotonNetwork.Time - (startTime + standbySeconds + gracePeriodSeconds));
                    gameUI.UpdateGameTimer(gameRemainingTime);

                    if (gameRemainingTime <= 0)
                    {
                        isGameStarted = true;
                        GameOver(false);
                    }
                }
            }).AddTo(this);
        }

        /// <summary>
        /// ゲーム開始直後の鬼の処理
        /// </summary>
        private void StarGameSeekerInit()
        {
            if (overheadCamera != null)
            {
                // 上空カメラを削除する
                Destroy(overheadCamera.gameObject);
                overheadCamera = null;
            }

            // 自プレイヤーのSeekerControllerを有効にする
            GameObject playerObject = PhotonNetwork.LocalPlayer.TagObject as GameObject;
            if (playerObject != null)
            {
                SeekerController seekerController = playerObject.GetComponent<SeekerController>();
                if (seekerController != null)
                {
                    seekerController.enabled = true;
                    seekerController.SwitchAudioListener(true);
                }
            }

            // 猶予時間終了後の処理
            foreach (var hider in hiderPlayerObjectList)
            {
                var hiderController = hider.GetComponent<HiderController>();
                var hiderBotController = hider.GetComponent<HiderBotController>();

                if (hiderController != null)
                {
                    // オブジェクトを再表示
                    hiderController.ShowPlayer();
                    hiderController.SetCamera();
                }
                else if (hiderBotController != null)
                {
                    hiderBotController.ShowBotPlayer();
                }
            }
        }

        /// <summary>
        /// RPCで鬼側が消滅したことを処理する
        /// </summary>
        /// <param name="seekerViewID">鬼のプレイヤーID</param>
        [PunRPC]
        private void RPC_DestroySeeker(int seekerViewID)
        {
            GameObject seekerPlayer = PhotonView.Find(seekerViewID).gameObject;
            if (seekerPlayer != null)
            {
                if (PhotonNetwork.LocalPlayer.ActorNumber == seekerPlayer.GetComponent<PhotonView>().Owner.ActorNumber)
                {
                    SetSpectatorMode();
                    PhotonNetwork.Destroy(seekerPlayer);
                }
                GameOver(false);
            }
        }

        /// <summary>
        /// RPCでプレイヤーが捕まったことを処理する
        /// </summary>
        /// <param name="hiderViewID">捕まったプレイヤーのPhotonViewID</param>
        [PunRPC]
        private void RPC_OnPlayerCaught(int hiderViewID)
        {
            GameObject hiderPlayer = PhotonView.Find(hiderViewID).gameObject;
            if (hiderPlayer != null)
            {
                var bot = hiderPlayer.GetComponent<HiderBotController>();
                if (bot == null)
                {
                    if (PhotonNetwork.LocalPlayer.ActorNumber == hiderPlayer.GetComponent<PhotonView>().Owner.ActorNumber)
                    {
                        SetSpectatorMode();
                    }
                }

                // MasterClientが消滅演出を行う
                if (PhotonNetwork.IsMasterClient)
                {
                    // エフェクトの生成座標
                    var pos = new Vector3(hiderPlayer.transform.position.x, hiderPlayer.transform.position.y + 0.5f, hiderPlayer.transform.position.z);

                    PhotonNetwork.Instantiate($"Effect/{destroyEffectObj.name}", pos, Quaternion.identity);
                }

                // 所有者が消滅処理を行う
                PhotonView hiderPhotonView = hiderPlayer.GetComponent<PhotonView>();
                if (hiderPhotonView.IsMine)
                {
                    // オブジェクトを削除
                    PhotonNetwork.Destroy(hiderPlayer);
                }
            }

            Debug.Log($"hiderPlayerCount:{hiderPlayerCount}");
            // 隠れる側のプレイヤーが全て捕まった場合
            if (hiderPlayerCount <= 0)
            {
                isGameStarted = false;
                GameOver(true);
            }
        }

        /// <summary>
        /// 捕まえたプレイヤーと鬼のメッセージを表示するRPC
        /// </summary>
        /// <param name="seekerViewID">捕まえた鬼のPhotonViewID</param>
        /// <param name="hiderViewID">捕まったプレイヤーのPhotonViewID</param
        /// <param name="hiderName">捕まったプレイヤーの名前</param>
        [PunRPC]
        private void RPC_DisplayCaughtMessage(int seekerViewID, int hiderViewID, string hiderName)
        {
            GameObject seekerPlayer = PhotonView.Find(seekerViewID).gameObject;
            GameObject hiderPlayer = PhotonView.Find(hiderViewID).gameObject;

            if (seekerPlayer != null && hiderPlayer != null)
            {
                var bot = hiderPlayer.GetComponent<HiderBotController>();
                bool isBot = bot != null;

                string caughtHiderName = isBot ? bot.GetBotName() : hiderName;

                // 隠れる側のプレイヤーかどうか
                if (PhotonNetwork.LocalPlayer.ActorNumber == hiderPlayer.GetComponent<PhotonView>().Owner.ActorNumber && !isBot)
                {
                    StartCoroutine(gameUI.ViewCaughtPlayerName($"{seekerPlayer.GetComponent<PhotonView>().Owner.NickName}に捕まりました"));
                }
                else if (PhotonNetwork.LocalPlayer.ActorNumber == seekerPlayer.GetComponent<PhotonView>().Owner.ActorNumber)
                {
                    StartCoroutine(gameUI.ViewCaughtPlayerName($"{caughtHiderName}を捕まえました"));
                }
            }
        }

        /// <summary>
        /// 観戦モードにする処理
        /// </summary>
        private void SetSpectatorMode()
        {
            if (overheadCameraPrefab != null)
            {
                overheadCamera = Instantiate(overheadCameraPrefab).GetComponent<Camera>();
                overheadCamera = Camera.main;
            }
        }

        /// <summary>
        /// ゲームオーバー時の処理
        /// </summary>
        /// <param name="isSeekerWin">鬼の勝利かどうかの判定</param>
        private void GameOver(bool isSeekerWin)
        {
            SE.instance.Play(SE.SEName.WhistleSE);

            // ゲーム終了処理
            if (isSeekerWin)
            {
                gameUI.ViewSeekerWin();
            }
            else
            {
                gameUI.ViewHiderWin();
            }
            // タイトル画面に戻る
            StartCoroutine(ReturnToTitle());
        }

        /// <summary>
        /// タイトル画面に戻る処理
        /// </summary>
        private IEnumerator ReturnToTitle()
        {
            // 数秒待機
            yield return new WaitForSeconds(5.0f);

            // シーンをロード
            SceneLoader.Instance().Load(SceneLoader.SceneName.Title);
        }
        #endregion
    }
}