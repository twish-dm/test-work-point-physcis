using GameCore;
using System;
using UnityEngine;
namespace GameCore
{
	public class Saver
	{
		private readonly Storer m_Data;
		private const string SaveKeyPrefix = "Save_";

		public Saver(Storer storer) => m_Data = storer;

		/// <summary>
		/// Сохраняет все данные из Storer в PlayerPrefs. Сложные типы конвертирует в JSON.
		/// </summary>
		public void SaveAll()
		{
			foreach (var item in m_Data)
			{
				string key = SaveKeyPrefix + item.Key;
				object value = item.Value.UntypedValue;

				if (value == null) continue;

				if (value is int i) PlayerPrefs.SetInt(key, i);
				else if (value is float f) PlayerPrefs.SetFloat(key, f);
				else if (value is string s) PlayerPrefs.SetString(key, s);
				else if (value is bool b) PlayerPrefs.SetInt(key, b ? 1 : 0);
				else
				{
					// Сериализация классов в JSON
					string json = JsonUtility.ToJson(value);
					PlayerPrefs.SetString(key, json);
				}
			}
			PlayerPrefs.Save();
		}

		/// <summary>
		/// Загружает данные из PlayerPrefs обратно в Storer, соблюдая типы.
		/// </summary>
		public void LoadAll()
		{
			foreach (var item in m_Data)
			{
				string key = SaveKeyPrefix + item.Key;
				if (!PlayerPrefs.HasKey(key)) continue;

				var prop = item.Value;
				Type targetType = prop.GetType().GetGenericArguments()[0];

				if (targetType == typeof(int)) prop.UntypedValue = PlayerPrefs.GetInt(key);
				else if (targetType == typeof(float)) prop.UntypedValue = PlayerPrefs.GetFloat(key);
				else if (targetType == typeof(string)) prop.UntypedValue = PlayerPrefs.GetString(key);
				else if (targetType == typeof(bool)) prop.UntypedValue = PlayerPrefs.GetInt(key) == 1;
				else
				{
					// Десериализация JSON в объект
					string json = PlayerPrefs.GetString(key);
					prop.UntypedValue = JsonUtility.FromJson(json, targetType);
				}
			}
		}

		public void DeleteSaves() => PlayerPrefs.DeleteAll();
	}
}