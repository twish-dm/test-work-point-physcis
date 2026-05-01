namespace GameCore
{
	using System;
	using System.Collections.Generic;

	public class Eventer : IDisposable
	{
		// События без параметров
		private readonly Dictionary<string, Action> m_Events = new Dictionary<string, Action>();

		// События с параметром <T> (храним как Delegate для поддержки разных типов)
		private readonly Dictionary<string, Delegate> m_TypedEvents = new Dictionary<string, Delegate>();

		#region Subscribe

		public void Subscribe(string eventName, Action callback)
		{
			if (callback == null) return;
			if (!m_Events.ContainsKey(eventName)) m_Events[eventName] = delegate { };
			m_Events[eventName] += callback;
		}

		public void Subscribe<T>(string eventName, Action<T> callback)
		{
			if (callback == null) return;
			if (!m_TypedEvents.ContainsKey(eventName))
			{
				m_TypedEvents[eventName] = callback;
			}
			else
			{
				m_TypedEvents[eventName] = Delegate.Combine(m_TypedEvents[eventName], callback);
			}
		}

		#endregion

		#region Unsubscribe

		public void Unsubscribe(string eventName, Action callback)
		{
			if (m_Events.ContainsKey(eventName))
			{
				m_Events[eventName] -= callback;
				if (m_Events[eventName] == null) m_Events.Remove(eventName);
			}
		}

		public void Unsubscribe<T>(string eventName, Action<T> callback)
		{
			if (m_TypedEvents.TryGetValue(eventName, out var existingDelegate))
			{
				var newDelegate = Delegate.Remove(existingDelegate, callback);
				if (newDelegate == null) m_TypedEvents.Remove(eventName);
				else m_TypedEvents[eventName] = newDelegate;
			}
		}

		#endregion

		#region Invoke

		/// <summary>
		/// Вызов события без данных.
		/// </summary>
		public void Invoke(string eventName)
		{
			if (m_Events.TryGetValue(eventName, out var action))
			{
				action?.Invoke();
			}
		}

		/// <summary>
		/// Вызов события с данными. Также оповещает подписчиков без параметров.
		/// </summary>
		public void Invoke<T>(string eventName, T data)
		{
			// Оповещаем тех, кто подписан просто на факт события
			Invoke(eventName);

			if (m_TypedEvents.TryGetValue(eventName, out var existingDelegate))
			{
				if (existingDelegate is Action<T> action)
				{
					action.Invoke(data);
				}
				// Если нужно, здесь можно добавить LogError о несовпадении типов
			}
		}

		#endregion

		public void Dispose()
		{
			m_Events.Clear();
			m_TypedEvents.Clear();
		}
	}
}