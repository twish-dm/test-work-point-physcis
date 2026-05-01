using GameCore;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{


	public class KayboardInputBehaviour : InputBehaviour, ITick
	{
		protected override void Awake()
		{
			base.Awake();
			Hub.Add(this);
		}
		[SerializeField] protected KeyboardInputSettings input;
		public void Tick(float deltaTime)
		{
			if (deltaTime == 0) return;

			if (Input.GetKeyDown(input.Forward))
				Forward();
			else if (Input.GetKeyUp(input.Forward))
				ForwardStop();
			if (Input.GetKeyDown(input.Backward))
				Backward();
			else if (Input.GetKeyUp(input.Backward))
				ForwardStop();
			if (Input.GetKeyDown(input.Jump))
				Jump();
			else if (Input.GetKeyUp(input.Jump))
				JumpStop();
		}
	}
}
