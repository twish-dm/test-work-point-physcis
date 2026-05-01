using GameCore;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace GameCore
{
	public class Pooler : MonoBehaviour
	{
		private readonly Dictionary<int, Stack<GameObject>> m_Pools = new Dictionary<int, Stack<GameObject>>();
		private readonly Dictionary<int, GameObject> m_Prefabs = new Dictionary<int, GameObject>();
		private readonly Dictionary<int, Transform> m_Groups = new Dictionary<int, Transform>();

		// НОВОЕ: Храним активные объекты, чтобы иметь к ним доступ
		private readonly Dictionary<int, HashSet<GameObject>> m_ActiveObjects = new Dictionary<int, HashSet<GameObject>>();


		public void AddMapping(string tag, GameObject prefab, int preload = 0)
		{
			int hash = tag.GetHashCode();
			if (m_Prefabs.ContainsKey(hash)) return;

			m_Prefabs[hash] = prefab;
			m_Pools[hash] = new Stack<GameObject>();
			m_ActiveObjects[hash] = new HashSet<GameObject>(); // Инициализируем список активных

			var group = new GameObject($"[Group] {tag}").transform;
			group.SetParent(this.transform);
			m_Groups[hash] = group;

			for (int i = 0; i < preload; i++) CreateNew(hash, true);
		}

		public GameObject Spawn(string tag, Vector3 pos, Quaternion rot)
		{
			int hash = tag.GetHashCode();
			if (!m_Pools.ContainsKey(hash)) return null;

			GameObject obj = (m_Pools[hash].Count > 0) ? m_Pools[hash].Pop() : CreateNew(hash, false);

			obj.transform.SetPositionAndRotation(pos, rot);
			obj.SetActive(true);

			// Добавляем в список активных
			m_ActiveObjects[hash].Add(obj);

			if (obj.TryGetComponent(out IPoolable p)) p.OnSpawn();
			return obj;
		}

		public void Despawn(string tag, GameObject obj)
		{
			int hash = tag.GetHashCode();

			// Убираем из активных
			if (m_ActiveObjects.ContainsKey(hash))
				m_ActiveObjects[hash].Remove(obj);

			if (obj.TryGetComponent(out IPoolable p)) p.OnDespawn();

			obj.SetActive(false);
			obj.transform.SetParent(m_Groups[hash]);
			m_Pools[hash].Push(obj);
		}

		// НОВОЕ: Массовая очистка конкретного типа
		public void DespawnAll(string tag)
		{
			int hash = tag.GetHashCode();
			if (!m_ActiveObjects.ContainsKey(hash)) return;

			// Копируем список, так как Despawn будет его изменять (защита от ошибки коллекции)
			var toRemove = new List<GameObject>(m_ActiveObjects[hash]);

			foreach (var obj in toRemove)
			{
				Despawn(tag, obj);
			}

			Debug.Log($"<color=yellow>All {tag} objects returned to pool.</color>");
		}

		private GameObject CreateNew(int hash, bool inactive)
		{
			var obj = Instantiate(m_Prefabs[hash], m_Groups[hash]);
			if (inactive)
			{
				obj.SetActive(false);
				m_Pools[hash].Push(obj);
			}
			return obj;
		}

		private void OnDestroy()
		{
			// При уничтожении пуллера зачищаем всё
			foreach (var hash in m_ActiveObjects.Keys)
			{
				DespawnAllByHash(hash);
			}
		}

		// Вспомогательный метод для внутренней зачистки
		private void DespawnAllByHash(int hash)
		{
			var toRemove = new List<GameObject>(m_ActiveObjects[hash]);
			foreach (var obj in toRemove)
			{
				if (obj.TryGetComponent(out IPoolable p)) p.OnDespawn();
				Destroy(obj);
			}
		}
	}
}