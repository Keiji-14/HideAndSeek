using UnityEngine;
using UnityEngine.UI;

namespace Title
{
    /// <summary>
    /// タイトル画面のUI
    /// </summary>
    public class TitleUI : MonoBehaviour
    {
        #region SerializeField
        /// <summary>マッチング中の経過時間UI</summary>
        [SerializeField] private GameObject timeCountUIObj;
        /// <summary>マッチング完了UI</summary>
        [SerializeField] private GameObject matchedUIObj;
        /// <summary>マッチング中の経過時間テキスト</summary>
        [SerializeField] private Text timeCountText;
        #endregion

        #region PublicMethod
        /// <summary>
        /// マッチング中の経過時間の表示切り替えの処理
        /// </summary>
        public void ViewMatchingTimeUI(bool isView)
        {
            timeCountUIObj.SetActive(isView);
        }

        /// <summary>
        /// マッチング中の経過時間の表示処理
        /// </summary>
        /// <param name="minutes">マッチング中の分を計測する値</param>
        /// <param name="seconds">マッチング中の秒を計測する値</param>
        public void MatchingTimeUI(float minutes, float seconds)
        {
            timeCountText.text = $"{minutes:00}:{seconds:00}";
        }

        // <summary>
        /// マッチング完了UIの表示処理
        /// </summary>
        public void MatchingCompletedUI()
        {
            matchedUIObj.SetActive(true);
            timeCountUIObj.SetActive(false);
        }
        #endregion
    }
}