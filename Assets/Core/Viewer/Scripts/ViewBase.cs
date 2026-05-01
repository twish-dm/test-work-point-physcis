using System.Collections;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
namespace GameCore
{

	[RequireComponent(typeof(CanvasGroup))]
	public abstract class ViewBase : MonoBehaviour
	{
		[SerializeField] protected CanvasGroup background;
		public string ViewName { get; private set; }
		[SerializeField] protected float m_BackgroundFadeDuration = 0.25f;

		public virtual void Initialize(string viewName)
		{
			background = background?? GetComponent<CanvasGroup>();
			ViewName = viewName;
			background.alpha = 0;
			background.blocksRaycasts = false;
			gameObject.SetActive(false);
		}

		public virtual IEnumerator ExecuteOpen()
		{
			OnBeforeOpen();
			background.alpha = 0;
			background.blocksRaycasts = true;
			gameObject.SetActive(true);
			yield return background.DOFade(1, m_BackgroundFadeDuration).WaitForCompletion();
			OnAfterOpen();
		}

		public virtual IEnumerator ExecuteClose()
		{
			OnBeforeClose();
			background.blocksRaycasts = false;
			yield return background.DOFade(0, m_BackgroundFadeDuration).WaitForCompletion();
			gameObject.SetActive(false);
			OnAfterClose();
		}

		protected virtual void OnBeforeOpen() { }
		protected virtual void OnAfterOpen() { }
		protected virtual void OnBeforeClose() { }
		protected virtual void OnAfterClose() { }
	}
}