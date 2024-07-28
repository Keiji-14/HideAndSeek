using System.Collections;
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
        /// <summary>マッチング中のテキスト</summary>
        [SerializeField] private GameObject matchingUIObj;
        /// <summary>マッチング完了UI</summary>
        [SerializeField] private GameObject matchedUIObj;
        /// <summary>マッチングロードUI</summary>
        [SerializeField] private GameObject matchingLoadingUI;
        /// <summary>マッチング中のテキスト</summary>
        [SerializeField] private Text matchingText;
        /// <summary>マッチング中の経過時間テキスト</summary>
        [SerializeField] private Text timeCountText;
        #endregion

        #region PublicMethod
        /// <summary>
        /// マッチング中のUI表示切り替えの処理
        /// </summary>
        public void ViewMatchingUI(bool isView)
        {
            timeCountUIObj.SetActive(isView);
            matchingLoadingUI.SetActive(isView);

            if (isView)
            {
                matchingUIObj.SetActive(true);
                matchingText.text = "マッチング中";
                StartCoroutine(UpdateMatchingText());
            }
            else
            {
                StopCoroutine(UpdateMatchingText());
            }
            //matchingLoadingUI.SetActive(isView);
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
            matchingUIObj.SetActive(false);
            timeCountUIObj.SetActive(false);
        }
        #endregion

        #region PrivateMethod
        /// <summary>
        /// マッチング中のテキストを動的に更新するコルーチン
        /// </summary>
        private IEnumerator UpdateMatchingText()
        {
            string baseText = "マッチング中";
            int dotCount = 0;
            while (true)
            {
                matchingText.text = baseText + new string('.', dotCount);
                dotCount = (dotCount + 1) % 4;
                yield return new WaitForSeconds(0.5f);
            }
        }
        #endregion
    }
}