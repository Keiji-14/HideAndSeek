using Photon.Pun;
using TMPro;

namespace Player
{
    public class PlayerNameDisplay : MonoBehaviourPunCallbacks
    {
        private void Start()
        {
            var nameLabel = GetComponent<TextMeshPro>();
            // プレイヤー名とプレイヤーIDを表示する
            nameLabel.text = $"{photonView.Owner.NickName}({photonView.OwnerActorNr})";
        }
    }
}