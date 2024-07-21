using Photon.Pun;
using System.Collections.Generic;
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
        /// <summary>鬼側のCanvas</summary>
        [SerializeField] private GameObject seekerCanvasObj;
        /// <summary>隠れる側のCanvas</summary>
        [SerializeField] private GameObject hiderCanvasObj;
        /// <summary>隠れる側のCanvas</summary>
        [SerializeField] private List<GameObject> lifeIconList;
        /// <summary>ゲームの制限時間テキスト</summary>
        [SerializeField] private Text timerText;
        /// <summary>ゲーム開始の猶予時間テキスト</summary>
        [SerializeField] private Text graceTimerText;
        /// <summary>テキスト</summary>
        [SerializeField] private Text hiderCountText;
        #endregion

        #region PublicMethod
        public void ToggleCanvas(string role)
        {
            if (role == "Seeker")
            {
                seekerCanvasObj.gameObject.SetActive(true);
                hiderCanvasObj.gameObject.SetActive(false);
            }
            else if (role == "Hider")
            {
                seekerCanvasObj.gameObject.SetActive(false);
                hiderCanvasObj.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// 残りの猶予時間を更新する
        /// </summary>
        /// <param name="time">残り時間</param>
        public void UpdateGraceTimer(float time)
        {
            graceTimerText.text = $"{Mathf.Ceil(time)}";
            graceTimerText.gameObject.SetActive(time >= 0);
        }

        /// <summary>
        /// 残りのゲーム時間を更新する
        /// </summary>
        /// <param name="time">残り時間</param>
        public void UpdateGameTimer(float time)
        {
            timerText.text = $"{Mathf.Ceil(time)}";
        }

        /// <summary>
        /// 残り体力を更新する
        /// </summary>
        /// <param name="life">残り体力</param>
        public void UpdateLife(float life)
        {
            for (int i = 0; i < lifeIconList.Count; i++)
            {
                lifeIconList[i].SetActive(i < life);
            }
        }

        /// <summary>
        /// 隠れる側のプレイヤー数を更新する
        /// </summary>
        /// <param name="value">プレイヤー数</param>
        public void UpdateHider(int value)
        {
            hiderCountText.text = $"{value}";
        }
        #endregion
    }
}