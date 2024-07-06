using UnityEngine;
using TMPro;

namespace Game
{
    /// <summary>
    /// ゲーム画面のUI
    /// </summary>
    public class GameUI : MonoBehaviour
    {
        #region SerializeField
        /// <summary>ゲームの制限時間テキスト</summary>
        [SerializeField] private TextMeshProUGUI timerText;
        #endregion

        #region PublicMethod
        /// <summary>
        /// 残り時間を更新する
        /// </summary>
        /// <param name="time">残り時間</param>
        public void UpdateTimer(float time)
        {
            timerText.text = $"Time Left: {Mathf.Ceil(time)}s";
        }
        #endregion
    }
}