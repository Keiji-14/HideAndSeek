using Scene;
using Photon.Pun;
using Photon.Realtime;
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
        private bool isMatchingStart = false;
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
            // "Room"という名前のルームに参加する（ルームが存在しなければ作成して参加する）
            PhotonNetwork.JoinOrCreateRoom("Room", new RoomOptions { MaxPlayers = 2 }, TypedLobby.Default);
        }

        /// <summary>
        /// ゲームサーバーへの接続が成功した時に呼ばれるコールバック
        /// </summary>
        public override void OnJoinedRoom()
        {
            if (!isMatchingStart)
            {
                isMatchingStart = true;
                matchingController.MatchingStart();
            }
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
        }

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
        {
            // プレイヤーの役割が割り当てられたかどうかを確認
            if (PhotonNetwork.IsMasterClient && isMatchingStart)
            {
                bool allRolesAssigned = true;
                foreach (var player in PhotonNetwork.PlayerList)
                {
                    if (!player.CustomProperties.ContainsKey("Role"))
                    {
                        allRolesAssigned = false;
                        break;
                    }
                }

                if (allRolesAssigned)
                {
                    isMatchingStart = false;
                    SceneLoader.Instance().PhotonNetworkLoad(SceneLoader.SceneName.Game);
                }
            }
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
                    ExitGames.Client.Photon.Hashtable customProperties = new ExitGames.Client.Photon.Hashtable();
                    if (playerIndex == seekerIndex)
                    {
                        customProperties["Role"] = "Seeker";
                    }
                    else
                    {
                        customProperties["Role"] = "Hider";
                    }
                    player.SetCustomProperties(customProperties);
                    playerIndex++;
                }
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