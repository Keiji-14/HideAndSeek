using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Title
{
    /// <summary>
    /// マイページの処理
    /// </summary>
    public class MyPage : MonoBehaviour
    {
        /*#region PrivateField
        /// <summary>決定ボタンを選択した時の処理 </summary>
        private IObservable<Unit> InputEnterBtnObservable =>
            enterBtn.OnClickAsObservable();
        /// <summary>閉じるボタンを選択した時の処理 </summary>
        private IObservable<Unit> InputCloseBtnObservable =>
            closeBtn.OnClickAsObservable();
        #endregion

        #region SerializeField
        /// <summary>決定ボタン</summary>
        [SerializeField] private Button enterBtn;
        /// <summary>閉じるボタン</summary>
        [SerializeField] private Button closeBtn;
        /// <summary>名前を表示</summary>
        [SerializeField] private TextMeshProUGUI nameText;
        /// <summary>名前入力場所</summary>
        [SerializeField] private InputField nameInputField;
        /// <summary>マイページウィンドウ</summary>
        [SerializeField] private GameObject myPageWindow;
        #endregion

        #region PublicMethod
        /// <summary>
        /// 初期化
        /// </summary>
        public void Init()
        {
            nameInputField.text = "";

            nameInputField.onValueChanged.AddListener(OnInputFieldValueChanged);

            InputEnterBtnObservable.Subscribe(_ =>
            {
                if (nameInputField.text.Length > 0)
                {
                    PlayerPrefs.SetInt("FirstTime", 1);
                    PlayerPrefs.SetString("UserName", nameInputField.text);

                    PlayerData playerData = new PlayerData(nameInputField.text);
                    GameDataManager.instance.SetPlayerData(playerData);
                }
                myPageWindow.SetActive(false);

                SE.instance.Play(SE.SEName.ButtonSE);
            }).AddTo(this);

            InputCloseBtnObservable.Subscribe(_ =>
            {
                myPageWindow.SetActive(false);

                SE.instance.Play(SE.SEName.ButtonSE);
            }).AddTo(this);
        }

        /// <summary>
        /// マイページを開く処理
        /// </summary>
        public void OpenMyPage()
        {
            nameInputField.text = "";

            nameText.text = GameDataManager.instance.GetPlayerData().name;

            myPageWindow.SetActive(true);
        }
        #endregion

        #region PrivateMethod
        /// <summary>
        /// ひらがな、カタカナ、英語、一部の記号以外の文字を削除する処理
        /// </summary>
        private void OnInputFieldValueChanged(string value)
        {
            string filteredText = System.Text.RegularExpressions.Regex.Replace(value, "[^ぁ-んァ-ンa-zA-Z0-9!\"#$%&'()*+,./:;<=>?@[\\]^_`{|}ー~]+", "");

            // テキストを更新する
            nameInputField.text = filteredText;
        }
        #endregion*/
    }
}