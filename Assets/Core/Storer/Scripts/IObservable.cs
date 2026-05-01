using System;

namespace GameCore
{
	public interface IObservable
	{
		object UntypedValue { get; set; }
		void Subscribe(Action<object> callback);
		void Unsubscribe(Action<object> callback);
		void ClearListeners();
	}
}