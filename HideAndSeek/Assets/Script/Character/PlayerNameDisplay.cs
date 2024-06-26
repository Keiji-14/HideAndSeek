using Photon.Pun;
using TMPro;

namespace Player
{
    public class PlayerNameDisplay : MonoBehaviourPunCallbacks
    {
        private void Start()
        {
            var nameLabel = GetComponent<TextMeshPro>();
            // �v���C���[���ƃv���C���[ID��\������
            nameLabel.text = $"{photonView.Owner.NickName}({photonView.OwnerActorNr})";
        }
    }
}