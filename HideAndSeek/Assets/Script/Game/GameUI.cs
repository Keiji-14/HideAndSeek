using System.Collections.Generic;
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
        [Header("Seeker UI")]
        /// <summary>鬼側のCanvas</summary>
        [SerializeField] private GameObject seekerCanvasObj;
        /// <summary>鬼側の開始時UI</summary>
        [SerializeField] private GameObject seekerStartUIObj;
        [Header("Hider UI")]
        /// <summary>隠れる側のCanvas</summary>
        [SerializeField] private GameObject hiderCanvasObj;
        /// <summary>隠れる側の開始時UI</summary>
        [SerializeField] private GameObject hiderStartUIObj;
        [Header("Common UI")]
        /// <summary>鬼側の勝利バー</summary>
        [SerializeField] private GameObject seekerWinBar;
        /// <summary>隠れる側の勝利バー</summary>
        [SerializeField] private GameObject hiderWinBar;
        /// <summary>隠れる側のCanvas</summary>
        [SerializeField] private List<GameObject> lifeIconList;
        /// <summary>ゲームの制限時間テキスト</summary>
        [SerializeField] private TextMeshProUGUI timerText;
        /// <summary>ゲーム開始の猶予時間テキスト</summary>
        [SerializeField] private TextMeshProUGUI graceTimerText;
        /// <summary>隠れる側の人数テキスト</summary>
        [SerializeField] private TextMeshProUGUI hiderCountText;
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

        public void ViewSeekerWin()
        {
            seekerWinBar.SetActive(true);
        }

        public void ViewHiderWin()
        {
            hiderWinBar.SetActive(true);
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
        /// 捕まえたプレイヤー名を表示する
        /// </summary>
        /// <param name="name">捕まえたプレイヤー名</param>
        public void ViewCaughtPlayerName(string name)
        {
            hiderCountText.text = $"{name}を捕まえました";
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