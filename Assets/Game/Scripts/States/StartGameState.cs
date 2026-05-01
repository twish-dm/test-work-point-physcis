using GameCore;
using System.Collections;
namespace Game
{
	public class StartGameState : BaseState
	{
		public override IEnumerator Enter()
		{
			Hub.Get<Viewer>().Push<View>("ControlView", ViewLayer.Screen);
			Stater.Change("LogicGameState");
			return base.Enter();
		}
	}
}