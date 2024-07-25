using GameData;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class PlayerNameDisplay : MonoBehaviourPunCallbacks
{
    #region PublicMethod
    /// <summary>
    /// 初期化
    /// </summary>
    /// <param name="isBot">ボットかどうか</param>
    public void Init(bool isBot)
    {
        var nameLabel = gameObject.GetComponent<Text>();
        var playerName = GameDataManager.Instance().GetPlayerData().name;

        if (isBot)
        {
            playerName = "bot";
        }
        else
        {
            PhotonNetwork.NickName = playerName;
        }

        
        // プレイヤー名を設定
        nameLabel.text = $"{playerName}";

        // 自分の役割に基づいて名前の表示を設定
        SetNameVisibility(nameLabel);
    }
    #endregion

    #region PrivateMethod
    /// <summary>
    /// ゲーム開始後のタイマー処理を行う
    /// </summary>
    /// <param name="nameLabel">自身のプレイヤー名</param>
    private void SetNameVisibility(Text nameLabel)
    {
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