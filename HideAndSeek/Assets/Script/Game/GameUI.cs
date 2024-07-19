using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    /// <summary>
    /// ゲーム画面のUI
    /// </summary>
    public class GameUI : MonoBehaviour
    {
        #region SerializeField
        /// <summary>ゲームの制限時間テキスト</summary>
        [SerializeField] private Text timerText;
        /// <summary>ゲーム開始の猶予時間テキスト</summary>
        [SerializeField] private Text countTimerText;
        #endregion

        #region PublicMethod
        /// <summary>
        /// 残り時間を更新する
        /// </summary>
        /// <param name="time">残り時間</param>
        public void UpdateTimer(float time)
        {
            timerText.text = $"{Mathf.Ceil(time)}";
        }
        #endregion
    }
}