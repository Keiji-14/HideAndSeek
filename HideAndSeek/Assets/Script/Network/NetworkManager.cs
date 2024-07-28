using Scene;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UniRx;
using UnityEngine;

namespace NetWork
{
    public class NetworkManager : MonoBehaviourPunCallbacks
    {
        #region PublicField
        public static NetworkManager instance = null;
        #endregion

        #region PrivateField
        private const string ROOM_PREFIX = "Room_";
        private int maxRoomSuffix = 10000; // ランダムなルーム名のサフィックスの最大値
        #endregion

        #region SerializeField
        [SerializeField] private MatchingController matchingController;
        #endregion

        #region UnityEvent
        private void Awake()
        {
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
        public void ConnectUsingSettings()
        {
            // PhotonServerSettingsの設定内容を使ってマスターサーバーへ接続する
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

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            CreateRoom();
        }

        /// <summary>
        /// ゲームサーバーから退出する処理
        /// </summary>
        public void LeaveRoom()
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
            PhotonNetwork.AutomaticallySyncScene = true;

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

        private void JoinRandomRoom()
        {
            PhotonNetwork.JoinRandomRoom();
        }

        private void CreateRoom()
        {
            string randomRoomName = GenerateRandomRoomName();
            RoomOptions roomOptions = new RoomOptions { MaxPlayers = 2 };
            roomOptions.CustomRoomProperties = new Hashtable() { { "IsOpen", true } };
            roomOptions.CustomRoomPropertiesForLobby = new string[] { "IsOpen" };
            PhotonNetwork.CreateRoom(randomRoomName, roomOptions, TypedLobby.Default);
        }

        private string GenerateRandomRoomName()
        {
            int randomSuffix = Random.Range(0, maxRoomSuffix);
            return $"{ROOM_PREFIX}{randomSuffix}";
        }

        /// <summary>
        /// 役割を割り当ててゲームシーンに移行する処理
        /// </summary>
        private void AssignRolesAndLoadGameScene()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                int seekerIndex = Random.Range(0, PhotonNetwork.PlayerList.Length);
                int playerIndex = 0;

                foreach (var player in PhotonNetwork.PlayerList)
                {
                    Hashtable customProperties = new Hashtable();
                    if (playerIndex == seekerIndex)
                    {
                        customProperties["Role"] = "Seeker";
                        Debug.Log("Assigned Seeker role to player: " + player.NickName);
                    }
                    else
                    {
                        customProperties["Role"] = "Hider";
                        Debug.Log("Assigned Hider role to player: " + player.NickName);
                    }
                    player.SetCustomProperties(customProperties);
                    playerIndex++;
                }

                Debug.Log("All roles assigned. Loading game scene...");
                PhotonNetwork.CurrentRoom.IsOpen = false;

                SceneLoader.Instance().PhotonNetworkLoad(SceneLoader.SceneName.Game);
            }
        }

        /// <summary>
        /// ルームを閉じる処理
        /// </summary>
        private void CloseRoom()
        {
            Debug.Log("CloseRoom");

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