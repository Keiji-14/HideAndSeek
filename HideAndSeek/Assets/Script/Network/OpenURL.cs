using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class OpenURL : MonoBehaviour
{
    #region PublicField
    /// <summary>プライバシーポリシーボタンを押した時の処理</summary>
    public IObservable<Unit> OnClickPrivacyPolicyButtonObservable => privacyPolicyBtn.OnClickAsObservable();
    #endregion

    #region SerializeField
    /// <summary>プライバシーポリシー</summary>
    [SerializeField] private Button privacyPolicyBtn;
    #endregion

    #region UnityEvent
    void Start()
    {
        OnClickPrivacyPolicyButtonObservable.Subscribe(_ =>
        {
            OpenPrivacyPolicy();
        }).AddTo(this);
    }
    #endregion

    #region PublicMethod
    public void OpenPrivacyPolicy()
    {
        string url = "https://akeiji14.wixsite.com/privacy";
        Application.OpenURL(url);
    }
    #endregion
}