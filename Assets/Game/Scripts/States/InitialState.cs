using GameCore;
using System.Collections;
namespace Game
{
	public class InitialState : BaseState
	{
		public override IEnumerator Enter()
		{
			Hub.Get<PlayerBehaviour>().SetActive(false);
			Stater.Change("StartGameState");
			return base.Enter();
		}
	}
}