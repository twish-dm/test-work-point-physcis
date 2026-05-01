using System;
using System.Collections.Generic;

namespace GameCore
{
	public class Property<T> : IObservable
	{
		private T m_Value;
		public event Action<T> OnChanged;

		// Словарь для хранения связей между Action<object> и Action<T>
		// Это нужно, чтобы Unsubscribe работал корректно
		private readonly Dictionary<Action<object>, Action<T>> m_Wrappers = new();

		public T Value
		{
			get => m_Value;
			set
			{
				if (Equals(m_Value, value)) return;
				m_Value = value;
				OnChanged?.Invoke(m_Value);
			}
		}

		public object UntypedValue
		{
			get => Value;
			set => Value = (T)value;
		}

		public Property(T defaultValue = default) => m_Value = defaultValue;

		// Реализация интерфейса для CoreBehaviour
		public void Subscribe(Action<object> callback)
		{
			Action<T> wrapper = (val) => callback(val);
			m_Wrappers[callback] = wrapper;
			OnChanged += wrapper;
		}

		public void Unsubscribe(Action<object> callback)
		{
			if (m_Wrappers.TryGetValue(callback, out var wrapper))
			{
				OnChanged -= wrapper;
				m_Wrappers.Remove(callback);
			}
		}

		public void ClearListeners()
		{
			OnChanged = null;
			m_Wrappers.Clear();
		}

		public static implicit operator T(Property<T> prop) => prop != null ? prop.Value : default;
		public override string ToString() => m_Value?.ToString() ?? "null";
	}
}