using UnityEngine;
using UnityEngine.UI;

namespace Scene
{
	/// <summary>
	/// フェードイン・フェードアウトの処理
	/// </summary>
	public class FadeController : MonoBehaviour
	{
		#region PublicField
		/// <summary>フェードインを行うかどうか</summary>
		public bool fadeIn = false;
		/// <summary>フェードアウトを行うかどうか</summary>
		public bool fadeOut = false;
		#endregion

		#region PrivateField
		/// <summary>RGBAの値</summary>
		private float red, green, blue, alfa;
		/// <summary>フェードする画像</summary>
		private Image fadeImage;
		#endregion

		#region PublicField
		/// <summary>フェードインの速度</summary>
		[SerializeField] private float fadeInSpeed;
		/// <summary>フェードアウトの速度</summary>
		[SerializeField] private float fadeOutSpeed;
		#endregion

		#region UnityEvent
		void Start()
		{
			fadeImage = GetComponent<Image>();
			red = fadeImage.color.r;
			green = fadeImage.color.g;
			blue = fadeImage.color.b;
			alfa = fadeImage.color.a;

			fadeIn = true;
		}

		void Update()
		{
			if (fadeIn)
			{
				StartFadeIn();
			}

			if (fadeOut)
			{
				StartFadeOut();
			}
		}
		#endregion

		#region PublicMethod
		/// <summary>
		/// フェードインを行う処理
		/// </summary>
		public void StartFadeIn()
		{
			fadeIn = true;
			alfa -= fadeInSpeed;
			SetAlpha();
			if (alfa <= 0)
			{
				fadeIn = false;
				fadeImage.enabled = false;
			}
		}

		/// <summary>
		/// フェードアウトを行う処理
		/// </summary>
		public void StartFadeOut()
		{
			fadeOut = true;
			fadeImage.enabled = true;
			alfa += fadeOutSpeed;
			SetAlpha();
			if (alfa >= 1)
			{
				fadeOut = false;
			}
		}
		#endregion

		#region PrivateMethod
		/// <summary>
		/// アルファ値の設定を行う処理
		/// </summary>
		private void SetAlpha()
		{
			fadeImage.color = new Color(red, green, blue, alfa);
		}
		#endregion
	}
}