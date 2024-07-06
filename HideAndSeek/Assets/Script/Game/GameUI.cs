using UnityEngine;
using TMPro;

namespace Game
{
    /// <summary>
    /// �Q�[����ʂ�UI
    /// </summary>
    public class GameUI : MonoBehaviour
    {
        #region SerializeField
        /// <summary>�Q�[���̐������ԃe�L�X�g</summary>
        [SerializeField] private TextMeshProUGUI timerText;
        #endregion

        #region PublicMethod
        /// <summary>
        /// �c�莞�Ԃ��X�V����
        /// </summary>
        /// <param name="time">�c�莞��</param>
        public void UpdateTimer(float time)
        {
            timerText.text = $"Time Left: {Mathf.Ceil(time)}s";
        }
        #endregion
    }
}