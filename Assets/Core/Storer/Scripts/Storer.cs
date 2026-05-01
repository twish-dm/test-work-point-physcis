using System.Collections;
using System.Collections.Generic;
namespace GameCore
{
	public class Storer : IEnumerable<KeyValuePair<string, IObservable>>
	{
		private readonly Dictionary<string, IObservable> m_Map = new Dictionary<string, IObservable>();

		public int Length => m_Map.Count;

		/// <summary>
		/// Устанавливает значение. Если ключа нет — создает новое свойство.
		/// </summary>
		public void Set<T>(string key, T value)
		{
			if (m_Map.TryGetValue(key, out var prop))
			{
				((Property<T>)prop).Value = value;
			}
			else
			{
				m_Map.Add(key, new Property<T>(value));
			}
		}

		/// <summary>
		/// Возвращает объект свойства для подписки или чтения.
		/// </summary>
		public Property<T> Get<T>(string key, T defaultValue = default)
		{
			if (!m_Map.TryGetValue(key, out var prop))
			{
				var newProp = new Property<T>(defaultValue);
				m_Map.Add(key, newProp);
				return newProp;
			}
			return (Property<T>)prop;
		}

		public void Remove(string key)
		{
			if (m_Map.TryGetValue(key, out var prop))
			{
				prop.ClearListeners();
				m_Map.Remove(key);
			}
		}

		public void Clear()
		{
			foreach (var pair in m_Map) pair.Value.ClearListeners();
			m_Map.Clear();
		}

		// Поддержка итерации (для Saver)
		public IEnumerator<KeyValuePair<string, IObservable>> GetEnumerator() => m_Map.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		// Быстрый доступ через индексатор storer["Key"]
		public object this[string key]
		{
			get => m_Map.TryGetValue(key, out var prop) ? prop.UntypedValue : null;
			set
			{
				if (m_Map.TryGetValue(key, out var prop))
					prop.UntypedValue = value;
				else
					m_Map.Add(key, new Property<object>(value));
			}
		}
	}
}