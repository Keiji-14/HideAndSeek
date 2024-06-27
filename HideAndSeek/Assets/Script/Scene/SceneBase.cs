using UnityEngine;
using UnityEngine.EventSystems;

namespace Scene
{
    /// <summary>
    /// 画面のベースクラス
    /// </summary>
    public class SceneBase : MonoBehaviour
    {
        #region UnityEvent
        public virtual void Start()
        {
            // EventSystemの有無で生成する処理
            if (EventSystem.current == null)
            {
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            }
        }
        #endregion
    }
}