namespace GameCore
{
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using UnityEngine;
	using Object = UnityEngine.Object;

	// Условная компиляция для работы с Addressables
#if USE_ADDRESSABLES
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
#endif

	public class Loader
	{
		private readonly Dictionary<string, Object> m_ResourceAssets = new Dictionary<string, Object>();

#if USE_ADDRESSABLES
    private readonly Dictionary<string, AsyncOperationHandle> m_AddressableHandles = new Dictionary<string, AsyncOperationHandle>();
#endif


		/// <summary>
		/// Загружает ассет и сразу регистрирует его в Hub под указанным именем (или путем).
		/// </summary>
		public async Task<T> LoadAndRegister<T>(string key, string hubName = null) where T : Object
		{
			var asset = await LoadInternal(key);
			if (asset is T typedAsset)
			{
				// Если hubName не указан, используем ключ пути как имя
				Hub.Add<T>(hubName ?? key, typedAsset);
				return typedAsset;
			}
			return null;
		}

		/// <summary>
		/// Возвращает уже загруженный ассет из кэша.
		/// </summary>
		public T Get<T>(string key) where T : Object
		{
			// Ищем в Resources
			if (m_ResourceAssets.TryGetValue(key, out var res) && res is T tRes)
				return tRes;

#if USE_ADDRESSABLES
            // Ищем в Addressables
            if (m_AddressableHandles.TryGetValue(key, out var handle) && handle.Result is T aRes)
                return aRes;
#endif

			Debug.LogWarning($"[Loader] Asset '{key}' is not loaded yet!");
			return null;
		}


		/// <summary>
		/// Универсальный метод загрузки: принимает пути/ключи, прогресс и колбэк.
		/// </summary>
		public async void Load(Action<float> onProgress, Action onComplete, params string[] keys)
		{
			if (keys == null || keys.Length == 0)
			{
				onComplete?.Invoke();
				return;
			}

			for (int i = 0; i < keys.Length; i++)
			{
				await LoadInternal(keys[i]);
				onProgress?.Invoke((i + 1f) / keys.Length);
			}

			onComplete?.Invoke();
		}

		public void Load(Action onComplete, params string[] keys) => Load(null, onComplete, keys);

		private async Task<Object> LoadInternal(string key)
		{
#if USE_ADDRESSABLES
        // 1. Попытка через Addressables
        if (await IsAddressable(key))
        {
            if (m_AddressableHandles.TryGetValue(key, out var handle))
                return handle.Result as Object;

            var newHandle = Addressables.LoadAssetAsync<Object>(key);
            m_AddressableHandles[key] = newHandle;
            await newHandle.Task;
            return newHandle.Result;
        }
#endif

			// 2. Попытка через Resources
			if (m_ResourceAssets.TryGetValue(key, out var cachedResource))
				return cachedResource;

			var request = Resources.LoadAsync<Object>(key);
			while (!request.isDone) await Task.Yield();

			if (request.asset != null)
			{
				m_ResourceAssets[key] = request.asset;
				return request.asset;
			}

			Debug.LogWarning($"[Loader] Asset '{key}' not found.");
			return null;
		}

#if USE_ADDRESSABLES
    private async Task<bool> IsAddressable(string key)
    {
        // Проверка, есть ли такой ключ в каталогах Addressables
        foreach (var locator in Addressables.ResourceLocators)
        {
            if (locator.Locate(key, typeof(Object), out _)) return true;
        }
        return false;
    }
#endif

		/// <summary>
		/// Очистка памяти. Выгружает и бандлы, и ресурсы.
		/// </summary>
		public void Unload()
		{
#if USE_ADDRESSABLES
        foreach (var handle in m_AddressableHandles.Values)
            Addressables.Release(handle);
        m_AddressableHandles.Clear();
#endif

			foreach (var asset in m_ResourceAssets.Values)
			{
				if (asset is not GameObject)
					Resources.UnloadAsset(asset);
			}
			m_ResourceAssets.Clear();

			Resources.UnloadUnusedAssets();
			Debug.Log("<color=cyan>[Loader] Memory Flushed</color>");
		}
	}
}