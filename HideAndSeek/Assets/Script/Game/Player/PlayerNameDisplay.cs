using GameData;
using Photon.Pun;
using UnityEngine.UI;

namespace Player
{
    public class PlayerNameDisplay : MonoBehaviourPunCallbacks
    {
        #region UnityEvent
        private void Start()
        {
            var nameLabel = gameObject.GetComponent<Text>();
            var playerName = GameDataManager.Instance().GetPlayerData().name;
            PhotonNetwork.NickName = playerName;

            // ÉvÉåÉCÉÑÅ[ñºÇê›íË
            nameLabel.text = $"{photonView.Owner.NickName}";
        }
        #endregion
    }
}