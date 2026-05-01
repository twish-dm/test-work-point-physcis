using GameCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace GameCore
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;


	public class Viewer : MonoBehaviour
	{
		[Header("Roots")]
		[SerializeField] private RectTransform m_ScreenRoot;
		[SerializeField] private RectTransform m_ModalRoot;
		[SerializeField] private RectTransform m_TutorialRoot;
		[SerializeField] private RectTransform m_NotificationRoot;
		[SerializeField] private RectTransform m_OverlayRoot;

		private readonly Dictionary<string, ViewBase> m_CachedViews = new Dictionary<string, ViewBase>();
		private readonly Dictionary<ViewLayer, ViewBase> m_ActiveViews = new Dictionary<ViewLayer, ViewBase>();

		// Очереди и флаги состояний для каждого слоя отдельно
		private readonly Dictionary<ViewLayer, Queue<IEnumerator>> m_Queues = new Dictionary<ViewLayer, Queue<IEnumerator>>();
		private readonly Dictionary<ViewLayer, bool> m_IsProcessing = new Dictionary<ViewLayer, bool>();

		private const string m_Path = "Views/";

		private void Awake()
		{
			// Инициализируем структуры для каждого слоя
			foreach (ViewLayer layer in System.Enum.GetValues(typeof(ViewLayer)))
			{
				m_Queues[layer] = new Queue<IEnumerator>();
				m_IsProcessing[layer] = false;
			}

		}

		#region Public API

		/// <summary>
		/// Открывает новое окно на слое. Если на слое что-то было, оно закроется первым.
		/// </summary>
		public void Push<T>(string viewName, ViewLayer layer = ViewLayer.Screen) where T : ViewBase
		{
			m_Queues[layer].Enqueue(PushRoutine<T>(viewName, layer));
			TryProcess(layer);
		}

		/// <summary>
		/// Закрывает текущее активное окно на слое.
		/// </summary>
		public void Pop(ViewLayer layer)
		{
			m_Queues[layer].Enqueue(PopRoutine(layer));
			TryProcess(layer);
		}

		#endregion

		private void TryProcess(ViewLayer layer)
		{
			if (!m_IsProcessing[layer] && m_Queues[layer].Count > 0)
			{
				StartCoroutine(ProcessLayerRoutine(layer));
			}
		}

		private IEnumerator ProcessLayerRoutine(ViewLayer layer)
		{
			m_IsProcessing[layer] = true;
			var queue = m_Queues[layer];

			while (queue.Count > 0)
			{
				yield return StartCoroutine(queue.Dequeue());
			}

			m_IsProcessing[layer] = false;
		}

		private IEnumerator PushRoutine<T>(string viewName, ViewLayer layer) where T : ViewBase
		{
			// Закрываем старое на ЭТОМ слое
			if (m_ActiveViews.TryGetValue(layer, out var current))
			{
				if (current.ViewName == viewName) yield break;
				yield return StartCoroutine(current.ExecuteClose());
				m_ActiveViews.Remove(layer);
			}

			// Создаем/берем новое
			T view = GetOrCreate<T>(viewName, layer);
			if (view != null)
			{
				m_ActiveViews[layer] = view;
				yield return StartCoroutine(view.ExecuteOpen());
			}
		}

		private IEnumerator PopRoutine(ViewLayer layer)
		{
			if (m_ActiveViews.TryGetValue(layer, out var current))
			{
				yield return StartCoroutine(current.ExecuteClose());
				m_ActiveViews.Remove(layer);
			}
		}

		private T GetOrCreate<T>(string viewName, ViewLayer layer) where T : ViewBase
		{
			if (m_CachedViews.TryGetValue(viewName, out var cached)) return (T)cached;

			GameObject prefab = Resources.Load<GameObject>(m_Path + viewName);
			if (!prefab)
			{
				Debug.LogError($"[Viewer] Prefab {viewName} not found!");
				return null;
			}

			Transform root = layer switch
			{
				ViewLayer.Tutorial => m_TutorialRoot,
				ViewLayer.Notification => m_NotificationRoot,
				ViewLayer.Modal => m_ModalRoot,
				ViewLayer.Overlay => m_OverlayRoot,
				_ => m_ScreenRoot
			};

			T instance = Instantiate(prefab, root, false).GetComponent<T>();
			instance.Initialize(viewName);
			m_CachedViews.Add(viewName, instance);
			return instance;
		}
	}
}