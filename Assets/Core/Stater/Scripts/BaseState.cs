using System.Collections;
using UnityEngine;

namespace GameCore
{
	public abstract class BaseState : IBaseState
	{
		public Stater Stater { get; set; }

		public virtual void Initialize() { }
		public virtual void Dispose() { }

		public virtual IEnumerator Enter() { yield break; }
		public virtual void Update() { }
		public virtual void FixedUpdate() { }

		public virtual void OnCollisionEnter(Collision collision) { }
		public virtual void OnCollisionExit(Collision collision) { }
		public virtual void OnTriggerEnter(Collider other) { }
		public virtual void OnTriggerExit(Collider other) { }

		public virtual IEnumerator Exit() { yield break; }
	}
}