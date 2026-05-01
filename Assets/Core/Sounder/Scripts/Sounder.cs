using GameCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace GameCore
{
	public class Sounder : MonoBehaviour
	{
		private AudioSource m_MusicSource;
		private AudioSource m_SfxSource;

		// Кэшируем клипы, чтобы не дергать диск каждый раз
		private readonly Dictionary<string, AudioClip> m_CachedClips = new Dictionary<string, AudioClip>();

		private const string m_Path = "Audio/"; // Путь: Resources/Audio/
		private Coroutine m_MusicCoroutine;

		private void Awake()
		{
			Hub.Add("Audio", this);

			m_MusicSource = gameObject.AddComponent<AudioSource>();
			m_MusicSource.loop = true;

			m_SfxSource = gameObject.AddComponent<AudioSource>();

			DontDestroyOnLoad(gameObject);
		}

		/// <summary>
		/// Проигрывает эффект. Загружает из Resources/Audio/NAME
		/// </summary>
		public void PlaySfx(string soundName, float volume = 1f)
		{
			AudioClip clip = GetClip(soundName);
			if (clip != null)
			{
				m_SfxSource.PlayOneShot(clip, volume);
			}
		}

		/// <summary>
		/// Запускает музыку с фейдом.
		/// </summary>
		public void PlayMusic(string musicName, float volume = 0.5f, float fadeDuration = 1f)
		{
			AudioClip clip = GetClip(musicName);
			if (clip == null) return;

			if (m_MusicCoroutine != null) StopCoroutine(m_MusicCoroutine);
			m_MusicCoroutine = StartCoroutine(MusicFadeRoutine(clip, volume, fadeDuration));
		}

		private IEnumerator MusicFadeRoutine(AudioClip nextClip, float targetVolume, float duration)
		{
			// 1. Уводим текущую музыку в ноль
			if (m_MusicSource.isPlaying)
			{
				float startVol = m_MusicSource.volume;
				for (float t = 0; t < duration; t += Time.deltaTime)
				{
					m_MusicSource.volume = Mathf.Lerp(startVol, 0, t / duration);
					yield return null;
				}
			}

			m_MusicSource.clip = nextClip;
			m_MusicSource.Play();

			// 2. Выводим новую музыку из нуля
			for (float t = 0; t < duration; t += Time.deltaTime)
			{
				m_MusicSource.volume = Mathf.Lerp(0, targetVolume, t / duration);
				yield return null;
			}
			m_MusicSource.volume = targetVolume;
		}

		private AudioClip GetClip(string clipName)
		{
			if (m_CachedClips.TryGetValue(clipName, out var clip)) return clip;

			clip = Resources.Load<AudioClip>(m_Path + clipName);
			if (clip == null)
			{
				Debug.LogWarning($"[Sounder] Clip '{clipName}' not found in Resources/{m_Path}");
				return null;
			}

			m_CachedClips.Add(clipName, clip);
			return clip;
		}

		// Очистка кэша, если нужно освободить память
		public void ClearCache() => m_CachedClips.Clear();
	}
}