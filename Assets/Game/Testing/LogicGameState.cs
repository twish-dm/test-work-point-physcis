using GameCore;
using System.Collections;
using UnityEngine;
namespace Game
{
	public class LogicGameState : BaseState
	{
		protected int currentTurn;
		public override IEnumerator Enter()
		{
			Hub.Get<PlayerBehaviour>().SetActive(true);
			return base.Enter();
		}
		[StateCommand]
		protected void Forward(bool value)
		{
			if(value)
			{
				Hub.Get<PlayerBehaviour>().Forward();
			}
			else
			{
				Hub.Get<PlayerBehaviour>().Stop();
			}
		}

		[StateCommand]
		protected void Backward(bool value)
		{
			if (value)
			{
				Hub.Get<PlayerBehaviour>().Backward();
			}
			else
			{
				Hub.Get<PlayerBehaviour>().Stop();
			}
		}

		[StateCommand]
		protected void Jump(bool value)
		{
			if (value)
			{
				Hub.Get<PlayerBehaviour>().Jump();
			}
		}
	}
}