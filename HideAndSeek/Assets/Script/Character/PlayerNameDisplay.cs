using GameData;
using Photon.Pun;
using TMPro;
using UnityEngine;

namespace Player
{
    public class PlayerNameDisplay : MonoBehaviourPunCallbacks
    {
        #region UnityEvent
        private void Start()
        {
            var nameLabel = gameObject.GetComponent<TextMeshProUGUI>();
            var playerName = GameDataManager.Instance().GetPlayerData().name;
            PhotonNetwork.NickName = playerName;

            // �v���C���[����ݒ�
            nameLabel.text = $"{photonView.Owner.NickName}";
        }
        #endregion
    }
}