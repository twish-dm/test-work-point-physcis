using System;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace GameCore
{
	public abstract class CoreBehaviour : MonoBehaviour
	{
		protected virtual void Awake()
		{
			// Автоматически впрыскиваем зависимости из Hub (Eventer, Pooler и т.д.)
			Hub.Inject(this);
		}

		private readonly List<(IObservable property, Action<object> handler)> m_ActiveSubscriptions = new();

		/// <summary>
		/// Подписывается на изменение данных в Storer по ключу.
		/// </summary>
		protected void Bind<T>(string key, Action<T> callback, bool triggerImmediately = true)
		{
			// Получаем свойство из нашего Storer
			var property = Hub.Get<Storer>().Get<T>(key);

			// Создаем обертку для колбэка, чтобы хранить её в списке IObservable
			Action<object> wrappedHandler = (val) => callback((T)val);

			// Подписываемся через интерфейс свойства
			property.Subscribe(wrappedHandler);

			// Сохраняем для авто-отписки
			m_ActiveSubscriptions.Add((property, wrappedHandler));

			// Если нужно — сразу выполняем колбэк с текущим значением
			if (triggerImmediately)
			{
				callback(property.Value);
			}
		}

		public virtual void SetActive(bool value)
		{
			gameObject.SetActive(value);
		}

		protected virtual void OnEnable()
		{
			if (this is ITick || this is IFixedTick || this is ILateTick)
				Hub.Get<Ticker>()?.Add(this);
		}

		protected virtual void OnDisable()
		{
			Hub.Get<Ticker>()?.Remove(this);
		}
		protected virtual void OnDestroy()
		{
			// Автоматически чистим все подписки на данные
			foreach (var sub in m_ActiveSubscriptions)
			{
				sub.property.Unsubscribe(sub.handler);
			}
			m_ActiveSubscriptions.Clear();
		}
	}
}