using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace GameCore
{
	/// <summary>
	/// Центральное ядро архитектуры. Сочетает в себе Service Locator и DI Container.
	/// </summary>
	public static class Hub
	{
		// Кэш для хранения сервисов. Ключ формируется из типа и имени.
		private static readonly Dictionary<string, object> m_Services = new();

		// Кэш метаданных полей для оптимизации Inject (чтобы не искать атрибуты каждый раз).
		private static readonly Dictionary<Type, FieldInfo[]> m_InjectCache = new();

		#region --- Dependency Injection ---

		/// <summary>
		/// Автоматически заполняет поля, помеченные атрибутом [Inject], соответствующими сервисами.
		/// </summary>
		public static void Inject(object target)
		{
			if (target == null) return;
			var type = target.GetType();

			// Ищем поля в кэше или сканируем тип, если его там нет
			if (!m_InjectCache.TryGetValue(type, out var fields))
			{
				var allFields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				var injectList = new List<FieldInfo>();

				foreach (var f in allFields)
				{
					if (Attribute.IsDefined(f, typeof(InjectAttribute)))
						injectList.Add(f);
				}

				fields = injectList.ToArray();
				m_InjectCache[type] = fields;
			}

			// Впрыскиваем зависимости
			foreach (var field in fields)
			{
				var attr = (InjectAttribute)Attribute.GetCustomAttribute(field, typeof(InjectAttribute));

				// Если имя в атрибуте не указано, используем имя типа поля
				string serviceName = attr.Name ?? field.FieldType.Name;

				var service = Get(field.FieldType, serviceName);
				if (service != null)
				{
					field.SetValue(target, service);
				}
			}
		}

		#endregion

		#region --- Регистрация (Add) ---

		/// <summary>
		/// Регистрация сервиса по типу и уникальному имени.
		/// </summary>
		public static void Add<T>(string name, T service)
		{
			if (service == null) return;
			string key = GetKey(typeof(T), name);
			m_Services[key] = service;
		}

		/// <summary>
		/// Регистрация сервиса по типу (имя берется из названия типа).
		/// </summary>
		public static void Add<T>(T service) => Add(typeof(T).Name, service);

		#endregion

		#region --- Получение (Get) ---

		/// <summary>
		/// Дженерик-версия получения сервиса по имени.
		/// </summary>
		public static T Get<T>(string name)
		{
			string key = GetKey(typeof(T), name);
			if (m_Services.TryGetValue(key, out var service))
				return (T)service;

			Debug.LogError($"[Hub] Service '{key}' not found!");
			return default;
		}

		/// <summary>
		/// Дженерик-версия получения сервиса по типу.
		/// </summary>
		public static T Get<T>() => Get<T>(typeof(T).Name);

		/// <summary>
		/// Не-дженерик версия получения сервиса (используется системой Inject).
		/// </summary>
		public static object Get(Type type, string name)
		{
			string key = GetKey(type, name);
			if (m_Services.TryGetValue(key, out var service))
				return service;

			//Debug.LogError($"[Hub] Service of type {type.Name} with name '{name}' not found!");
			return null;
		}

		#endregion

		#region --- Удаление и Утилиты ---

		public static void Remove<T>(string name) => m_Services.Remove(GetKey(typeof(T), name));

		public static void Remove<T>() => Remove<T>(typeof(T).Name);

		public static bool Has<T>(string name) => m_Services.ContainsKey(GetKey(typeof(T), name));

		/// <summary>
		/// Полная очистка всех сервисов и кэша инъекций.
		/// </summary>
		public static void Clear()
		{
			m_Services.Clear();
			m_InjectCache.Clear();
		}

		/// <summary>
		/// Внутренний метод генерации ключа для словаря.
		/// </summary>
		private static string GetKey(Type type, string name) => $"{type.FullName}_{name}";

		#endregion
	}

	/// <summary>
	/// Атрибут для автоматического внедрения зависимостей через Hub.Inject.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field)]
	public class InjectAttribute : Attribute
	{
		public string Name { get; }
		public InjectAttribute(string name = null) => Name = name;
	}
}