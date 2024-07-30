using Scene;
using GameData;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UniRx;
using UnityEngine;

namespace NetWork
{
    /// <summary>
    /// ネットワークの管理を行うクラス
    /// </summary>
    public class NetworkManager : MonoBehaviourPunCallbacks
    {
        #region PublicField
        /// <summary>シングルトン</summary>
        public static NetworkManager instance = null;
        #endregion

        #region PrivateField
        /// <summary>ルーム名</summary>
        private const string roomPrefix = "Room_";
        /// <summary>ランダムなルーム名の最大値</summary>
        private int maxRoomSuffix = 10000;
        /// <summary>マッチング処理のコンポーネント</summary>
        private MatchingController matchingController;
        #endregion

        #region UnityEvent
        private void Awake()
        {
            // シングルトンの実装
            if (instance == null)
            {
                instance = this;

                Init();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        #endregion

        #region PublicMethod
        /// <summary>
        /// PhotonServerSettingsの設定内容を使ってマスターサーバーへ接続する処理
        /// </summary>
        public void ConnectUsingSettings()
        {
            PhotonNetwork.ConnectUsingSettings();
        }

        /// <summary>
        /// マスターサーバーへの接続が成功した時に呼ばれるコールバック
        /// </summary>
        public override void OnConnectedToMaster()
        {
            JoinRandomRoom();
        }

        /// <summary>
        /// ゲームサーバーへの接続が成功した時に呼ばれるコールバック
        /// </summary>
        public override void OnJoinedRoom()
        {
            matchingController.MatchingStart();
        }

        /// <summary>
        /// ランダムルームへの参加に失敗したときに呼ばれるコールバックメソッド
        /// </summary>
        /// <param name="returnCode">失敗の原因を示すコード</param>
        /// <param name="message">失敗の詳細メッセージ</param>
        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            CreateRoom();
        }

        /// <summary>
        /// ゲームサーバーから退出する処理
        /// </summary>
        public void LeaveRoom()
        {
            if (PhotonNetwork.InRoom)
            {
                // PhotonのLeaveRoomメソッドを使用してゲームサーバーから退出する
                matchingController.MatchingFinish();

                if (PhotonNetwork.IsMasterClient)
                {
                    CloseRoom();
                }
                else
                {
                    PhotonNetwork.LeaveRoom();
                }

                if (PhotonNetwork.LocalPlayer != null)
                {
                    PhotonNetwork.LocalPlayer.TagObject = null;
                }
            }

            // Photonサーバーとの接続を切断する
            PhotonNetwork.Disconnect();
        }
        #endregion

        #region PrivateMethod
        /// <summary>
        /// 初期化
        /// </summary>
        private void Init()
        {
            // シーン自動同期の有効化
            PhotonNetwork.AutomaticallySyncScene = true;

            matchingController = FindObjectOfType<MatchingController>();

            // 既にルームに入っている場合は退出する
            if (PhotonNetwork.InRoom)
            {
                LeaveRoom();
            }

            matchingController.MatchingCompletedSubject.Subscribe(_ =>
            {
                var tilteController = FindObjectOfType<Title.TitleController>();
                if (tilteController != null)
                {
                    tilteController.MatchingCompleted();
                }

                AssignRolesAndLoadGameScene();
            }).AddTo(this);
        }

        /// <summary>
        /// ランダムルームに参加する
        /// </summary>
        private void JoinRandomRoom()
        {
            PhotonNetwork.JoinRandomRoom();
        }

        /// <summary>
        /// 新しいルームを作成する
        /// </summary>
        private void CreateRoom()
        {
            // ランダムなルーム名を生成する
            string randomRoomName = GenerateRandomRoomName();
            // ルームのオプションを設定する
            RoomOptions roomOptions = new RoomOptions { MaxPlayers = 2 };
            roomOptions.CustomRoomProperties = new Hashtable() { { "IsOpen", true } };
            roomOptions.CustomRoomPropertiesForLobby = new string[] { "IsOpen" };
            // ルームを作成する
            PhotonNetwork.CreateRoom(randomRoomName, roomOptions, TypedLobby.Default);
        }

        /// <summary>
        /// ランダムなルーム名を生成する
        /// </summary>
        /// <returns>生成されたランダムなルーム名</returns>
        private string GenerateRandomRoomName()
        {
            int randomSuffix = Random.Range(0, maxRoomSuffix);
            return $"{roomPrefix}{randomSuffix}";
        }

        /// <summary>
        /// 役割を割り当ててゲームシーンに移行する処理
        /// </summary>
        private void AssignRolesAndLoadGameScene()
        {
            Hashtable customProperties = new Hashtable();
            customProperties["Role"] = GameDataManager.Instance().GetPlayerRole();

            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.CurrentRoom.IsOpen = false;

                SceneLoader.Instance().PhotonNetworkLoad(SceneLoader.SceneName.Game);
            }
        }

        /// <summary>
        /// ルームを閉じる処理
        /// </summary>
        private void CloseRoom()
        {
            foreach (var player in PhotonNetwork.PlayerList)
            {
                if (!player.IsLocal)
                {
                    PhotonNetwork.CloseConnection(player);
                }
            }
            PhotonNetwork.LeaveRoom();
        }
        #endregion
    }
}