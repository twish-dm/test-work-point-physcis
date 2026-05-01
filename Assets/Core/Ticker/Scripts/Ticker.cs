using System.Collections.Generic;
using UnityEngine;

namespace GameCore
{
	public enum TickPriority
	{
		High = 0,   // Системы логики, детекторы, поиск пути
		Normal = 1, // Враги, башни, игроки
		Low = 2     // Снаряды, эффекты, UI, камера
	}

	public interface ITick { void Tick(float deltaTime); }
	public interface IFixedTick { void FixedTick(float deltaTime); }
	public interface ILateTick { void LateTick(float deltaTime); }

	public class Ticker : MonoBehaviour
	{
		private readonly Dictionary<TickPriority, List<ITick>> m_Tickers = new()
		{
			{ TickPriority.High, new List<ITick>() },
			{ TickPriority.Normal, new List<ITick>() },
			{ TickPriority.Low, new List<ITick>() }
		};

		private readonly Dictionary<TickPriority, List<IFixedTick>> m_FixedTickers = new()
		{
			{ TickPriority.High, new List<IFixedTick>() },
			{ TickPriority.Normal, new List<IFixedTick>() },
			{ TickPriority.Low, new List<IFixedTick>() }
		};

		private readonly Dictionary<TickPriority, List<ILateTick>> m_LateTickers = new()
		{
			{ TickPriority.High, new List<ILateTick>() },
			{ TickPriority.Normal, new List<ILateTick>() },
			{ TickPriority.Low, new List<ILateTick>() }
		};

		private float m_TimeScale = 1f;
		private bool m_IsPaused = false;

		private void OnDestroy() => Hub.Remove<Ticker>();

		public void SetPause(bool isPaused) => m_IsPaused = isPaused;
		public void SetTimeScale(float scale) => m_TimeScale = Mathf.Max(0, scale);

		public void Add(object obj, TickPriority priority = TickPriority.Normal)
		{
			if (obj is ITick t) m_Tickers[priority].Add(t);
			if (obj is IFixedTick ft) m_FixedTickers[priority].Add(ft);
			if (obj is ILateTick lt) m_LateTickers[priority].Add(lt);
		}

		public void Remove(object obj)
		{
			foreach (var list in m_Tickers.Values) if (obj is ITick t) list.Remove(t);
			foreach (var list in m_FixedTickers.Values) if (obj is IFixedTick ft) list.Remove(ft);
			foreach (var list in m_LateTickers.Values) if (obj is ILateTick lt) list.Remove(lt);
		}

		private void Update()
		{
			if (m_IsPaused) return;
			float dt = Time.deltaTime * m_TimeScale;

			ExecuteTick(m_Tickers[TickPriority.High], dt);
			ExecuteTick(m_Tickers[TickPriority.Normal], dt);
			ExecuteTick(m_Tickers[TickPriority.Low], dt);
		}

		private void FixedUpdate()
		{
			if (m_IsPaused) return;
			float dt = Time.fixedDeltaTime * m_TimeScale;

			ExecuteFixedTick(m_FixedTickers[TickPriority.High], dt);
			ExecuteFixedTick(m_FixedTickers[TickPriority.Normal], dt);
			ExecuteFixedTick(m_FixedTickers[TickPriority.Low], dt);
		}

		private void LateUpdate()
		{
			if (m_IsPaused) return;
			float dt = Time.deltaTime * m_TimeScale;

			ExecuteLateTick(m_LateTickers[TickPriority.High], dt);
			ExecuteLateTick(m_LateTickers[TickPriority.Normal], dt);
			ExecuteLateTick(m_LateTickers[TickPriority.Low], dt);
		}

		// Вспомогательные методы прохода по спискам (итерируемся с конца для безопасности удаления)
		private void ExecuteTick(List<ITick> list, float dt)
		{
			for (int i = list.Count - 1; i >= 0; i--) list[i].Tick(dt);
		}

		private void ExecuteFixedTick(List<IFixedTick> list, float dt)
		{
			for (int i = list.Count - 1; i >= 0; i--) list[i].FixedTick(dt);
		}

		private void ExecuteLateTick(List<ILateTick> list, float dt)
		{
			for (int i = list.Count - 1; i >= 0; i--) list[i].LateTick(dt);
		}
	}
}