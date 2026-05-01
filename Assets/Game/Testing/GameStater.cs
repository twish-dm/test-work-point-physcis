using GameCore;
namespace Game
{
	public class GameStater : Stater
	{
		protected void Awake()
		{
			AddState(new InitialState());
			AddState(new StartGameState());
			AddState(new LogicGameState());
			AddState(new EndGameState());

			Change("InitialState");
		}
	}
}