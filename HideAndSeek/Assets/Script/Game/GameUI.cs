using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Collections;

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
        [Header("Hider UI")]
        /// <summary>隠れる側のCanvas</summary>
        [SerializeField] private GameObject hiderCanvasObj;
        [Header("Common UI")]
        /// <summary>待機時間中に表示するキャンバス</summary>
        [SerializeField] private GameObject standbyCanvas;
        /// <summary>ゲーム中に表示するキャンバス</summary>
        [SerializeField] private GameObject gameCanvas;
        /// <summary>鬼側の勝利バー</summary>
        [SerializeField] private GameObject seekerWinBar;
        /// <summary>隠れる側の勝利バー</summary>
        [SerializeField] private GameObject hiderWinBar;
        /// <summary>ライフアイコン</summary>
        [SerializeField] private List<GameObject> lifeIconList;
        /// <summary>開始時の待機時間テキスト</summary>
        [SerializeField] private TextMeshProUGUI standbyTimerText;
        /// <summary>ゲームの制限時間テキスト</summary>
        [SerializeField] private TextMeshProUGUI timerText;
        /// <summary>ゲーム開始の猶予時間テキスト</summary>
        [SerializeField] private TextMeshProUGUI graceTimerText;
        /// <summary>猶予時間時に表示するテキスト</summary>
        [SerializeField] private TextMeshProUGUI graceAssistText;
        /// <summary>隠れる側の人数テキスト</summary>
        [SerializeField] private TextMeshProUGUI hiderCountText;
        /// <summary>捕まえた時の表示テキスト</summary>
        [SerializeField] private TextMeshProUGUI caughtPlayerText;
        #endregion

        #region PublicMethod
        /// <summary>
        /// ゲーム中のCanavsを切り替える処理
        /// </summary>
        /// <param name="isStandby">待機中かどうか</param>
        public void SwicthGameCanvas(bool isStandby)
        {
            standbyCanvas.SetActive(isStandby);
            gameCanvas.SetActive(!isStandby);
        }

        /// <summary>
        /// 役割によって表示するCanavsを切り替える処理
        /// </summary>
        /// <param name="role">役割</param>
        public void SwicthRoleCanvas(string role)
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
        /// 鬼側の勝利時に表示する処理
        /// </summary>
        public void ViewSeekerWin()
        {
            seekerWinBar.SetActive(true);
        }

        /// <summary>
        /// 隠れる側の勝利時に表示する処理
        /// </summary>
        public void ViewHiderWin()
        {
            hiderWinBar.SetActive(true);
        }

        /// <summary>
        /// 残りの待機時間を更新する処理
        /// </summary>
        /// <param name="time">残り時間</param>
        public void UpdateStandbyTimer(float time)
        {
            standbyTimerText.text = $"{Mathf.Ceil(time)}";
            standbyCanvas.SetActive(time >= 0);
        }

        /// <summary>
        /// 残りの猶予時間を更新する処理
        /// </summary>
        /// <param name="time">残り時間</param>
        public void UpdateGraceTimer(float time)
        {
            graceTimerText.text = $"{Mathf.Ceil(time)}";
            graceTimerText.gameObject.SetActive(time >= 0);
        }

        /// <summary>
        /// 残りのゲーム時間を更新する処理
        /// </summary>
        /// <param name="time">残り時間</param>
        public void UpdateGameTimer(float time)
        {
            timerText.text = $"{Mathf.Ceil(time)}";
        }

        /// <summary>
        /// 残り体力を更新する処理
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
        /// 猶予時間中に表示する処理
        /// </summary>
        /// <param name="graceAssistStr">表示内容</param>
        public IEnumerator ViewGraceText(string graceAssistStr)
        {
            graceAssistText.text = graceAssistStr;
            // 10秒間表示
            yield return new WaitForSeconds(10f);
            graceAssistText.text = "";
        }

        /// <summary>
        /// 捕まえた情報を表示する処理
        /// </summary>
        /// <param name="caughtStr">表示内容</param>
        public IEnumerator ViewCaughtPlayerName(string caughtStr)
        {
            caughtPlayerText.text = caughtStr;
            // 5秒間表示
            yield return new WaitForSeconds(5f);
            caughtPlayerText.text = "";
        }

        /// <summary>
        /// 隠れる側のプレイヤー数を更新する処理
        /// </summary>
        /// <param name="value">プレイヤー数</param>
        public void UpdateHider(int value)
        {
            hiderCountText.text = $"{value}";
        }
        #endregion
    }
}