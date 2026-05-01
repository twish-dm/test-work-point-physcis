using System;
using System.Collections;
using UnityEngine;

namespace GameCore
{
	public interface IBaseState : IInitialize, IDisposable
	{
		Stater Stater { get; set; }
		IEnumerator Enter();
		void Update();
		void FixedUpdate();
		void OnCollisionEnter(Collision collision);
		void OnCollisionExit(Collision collision);
		void OnTriggerEnter(Collider other);
		void OnTriggerExit(Collider other);
		IEnumerator Exit();
	}


	[AttributeUsage(AttributeTargets.Method, Inherited = true)]
	public class StateCommandAttribute : Attribute { }
}