using System.Collections;
using UnityEngine;
using DG.Tweening;
namespace GameCore
{
	public class View : ViewBase
	{
		[SerializeField] protected RectTransform content;
		[SerializeField] protected float m_ContentShowTime = 0.25f;
		public override IEnumerator ExecuteOpen()
		{
			OnBeforeOpen();
			background.alpha = 0;
			background.blocksRaycasts = true;
			content.localScale = Vector3.zero;
			gameObject.SetActive(true);
			background.DOFade(1, m_BackgroundFadeDuration);
			yield return content.DOScale(1.1f, m_ContentShowTime * .75f).WaitForCompletion();
			yield return content.DOScale(1f, m_ContentShowTime * .25f).WaitForCompletion();
			OnAfterOpen();
		}

		public override IEnumerator ExecuteClose()
		{
			OnBeforeClose();
			background.blocksRaycasts = false;
			background.DOFade(0, m_BackgroundFadeDuration);
			yield return content.DOScale(1.1f, m_ContentShowTime * .25f).WaitForCompletion();
			yield return content.DOScale(0f, m_ContentShowTime * .75f).WaitForCompletion();
			gameObject.SetActive(false);
			OnAfterClose();
		}
	}
}