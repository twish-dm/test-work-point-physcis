namespace GameCore
{
	public interface IPoolable
	{
		void OnSpawn();   // Сброс здоровья, запуск анимаций
		void OnDespawn(); // Остановка звуков, очистка эффектов
	}
}