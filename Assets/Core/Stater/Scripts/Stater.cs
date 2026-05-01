using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace GameCore
{
	public class Stater : MonoBehaviour
	{
		private IBaseState m_CurrentState;
		private bool m_IsChangingState;

		private readonly Dictionary<string, IBaseState> m_States = new Dictionary<string, IBaseState>();
		private readonly Queue<IBaseState> m_TransitionsQueue = new Queue<IBaseState>();

		public IBaseState CurrentState => m_CurrentState;

		/// <summary>
		/// Добавляет состояние, привязывает Stater и вызывает инициализацию.
		/// </summary>
		public void AddState(IBaseState state)
		{
			if (state == null) return;

			string name = state.GetType().Name;
			if (!m_States.ContainsKey(name))
			{
				state.Stater = this;
				state.Initialize();
				m_States.Add(name, state);
			}
		}

		/// <summary>
		/// Удаляет состояние из словаря и освобождает ресурсы.
		/// </summary>
		public void RemoveState(string stateName)
		{
			if (m_States.TryGetValue(stateName, out IBaseState state))
			{
				state.Dispose();
				m_States.Remove(stateName);
			}
		}

		/// <summary>
		/// Переход в состояние по экземпляру (добавляет в словарь автоматически).
		/// </summary>
		public void Change(IBaseState state)
		{
			if (state == null) return;
			AddState(state);
			EnqueueTransition(state);
		}

		/// <summary>
		/// Переход в состояние по имени класса.
		/// </summary>
		public void Change(string stateName)
		{
			if (m_States.TryGetValue(stateName, out IBaseState state))
			{
				EnqueueTransition(state);
			}
			else
			{
				Debug.LogError($"[Stater] State '{stateName}' not found. Add it first via AddState().");
			}
		}

		private void EnqueueTransition(IBaseState state)
		{
			m_TransitionsQueue.Enqueue(state);

			if (!m_IsChangingState)
			{
				StartCoroutine(ProcessQueueRoutine());
			}
		}

		private IEnumerator ProcessQueueRoutine()
		{
			m_IsChangingState = true;

			while (m_TransitionsQueue.Count > 0)
			{
				IBaseState nextState = m_TransitionsQueue.Dequeue();

				if (m_CurrentState == nextState) continue;

				if (m_CurrentState != null)
				{
					yield return StartCoroutine(m_CurrentState.Exit());
				}

				m_CurrentState = nextState;

				if (m_CurrentState != null)
				{
					yield return StartCoroutine(m_CurrentState.Enter());
				}
			}

			m_IsChangingState = false;
		}

		private static readonly Dictionary<Type, Dictionary<string, MethodInfo>> _methodCache =
			new Dictionary<Type, Dictionary<string, MethodInfo>>();

		public void Command(string commandName, params object[] args)
		{
			if (m_CurrentState == null) return;

			var type = m_CurrentState.GetType();

			// 1. Получаем/создаем карту методов для этого типа
			if (!_methodCache.TryGetValue(type, out var methods))
			{
				methods = new Dictionary<string, MethodInfo>();
				_methodCache[type] = methods;	
			}

			// 2. Ищем метод
			if (!methods.TryGetValue(commandName, out var method))
			{
				var foundMethod = type.GetMethod(commandName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

				// Проверяем атрибут [StateCommand]
				if (foundMethod != null && foundMethod.GetCustomAttribute<StateCommandAttribute>() != null)
				{
					method = foundMethod;
				}
				methods[commandName] = method;
			}

			// 3. Вызываем
			if (method != null && args!=null && args.Length>0)
			{
				method.Invoke(m_CurrentState, args);
			}
			else if (method != null)
			{
				method.Invoke(m_CurrentState, null);
			}
			else
			{
				Debug.LogWarning($"[Stater] Command '{commandName}' not found in {type.Name}");
			}
		}

		#region Unity Lifecycle Dispatching

		private void Update()
		{
			if (!m_IsChangingState) m_CurrentState?.Update();
		}

		private void FixedUpdate()
		{
			if (!m_IsChangingState) m_CurrentState?.FixedUpdate();
		}

		private void OnCollisionEnter(Collision col) => m_CurrentState?.OnCollisionEnter(col);
		private void OnCollisionExit(Collision col) => m_CurrentState?.OnCollisionExit(col);
		private void OnTriggerEnter(Collider other) => m_CurrentState?.OnTriggerEnter(other);
		private void OnTriggerExit(Collider other) => m_CurrentState?.OnTriggerExit(other);

		private void OnDestroy()
		{
			StopAllCoroutines();
			foreach (var state in m_States.Values)
			{
				state.Dispose();
			}
			m_States.Clear();
			m_TransitionsQueue.Clear();
		}

		#endregion
	}
}