using GameData;
using Photon.Pun;
using UnityEngine.UI;

namespace Player
{
    /// <summary>
    /// 頭上にプレイヤー名を表示する処理
    /// </summary>
    public class PlayerNameDisplay : MonoBehaviourPunCallbacks
    {
        #region PublicMethod
        /// <summary>
        /// 初期化
        /// </summary>
        /// <param name="isBot">ボットかどうか</param>
        /// <param name="name">名前<param>
        public void Init(bool isBot, string name = null)
        {
            // プレイヤー名を取得
            var playerName = GameDataManager.Instance().GetPlayerData().name;

            if (isBot)
            {
                playerName = "bot";
            }
            else
            {
                PhotonNetwork.NickName = playerName;
            }

            // プレイヤー名を設定し、他のプレイヤーに通知
            photonView.RPC("RPC_SyncPlayerName", RpcTarget.AllBuffered, playerName);

            // 自分の役割に基づいて名前の表示を設定
            SetNameVisibility();
        }
        #endregion

        #region PrivateMethod
        /// <summary>
        /// プレイヤー名を同期する処理
        /// </summary>
        /// <param name="playerName">同期するプレイヤー名</param>
        [PunRPC]
        private void RPC_SyncPlayerName(string playerName)
        {
            var nameLabel = gameObject.GetComponent<Text>();
            nameLabel.text = $"{playerName}";
        }

        /// <summary>
        /// 自分の役割に基づいて名前の表示を設定
        /// </summary>
        private void SetNameVisibility()
        {
            var nameLabel = gameObject.GetComponent<Text>();
            // 自分の役割を取得
            string myRole = (string)PhotonNetwork.LocalPlayer.CustomProperties["Role"];

            if (photonView.IsMine)
            {
                if (myRole == "Seeker")
                {
                    // 自分自身の名前を非表示
                    nameLabel.enabled = false;
                }
                else if (myRole == "Hider")
                {
                    // 自分自身には名前を表示
                    nameLabel.enabled = true;
                }
            }
            else
            {
                // 他プレイヤーの役割を取得
                string otherPlayerRole = (string)photonView.Owner.CustomProperties["Role"];

                if (myRole == "Seeker")
                {
                    // 鬼の場合、鬼のみの名前を表示
                    nameLabel.enabled = (otherPlayerRole == "Seeker");
                }
                else if (myRole == "Hider")
                {
                    // 隠れる側の場合、全てのプレイヤーの名前を表示
                    nameLabel.enabled = true;
                }
            }
        }
        #endregion
    }
}