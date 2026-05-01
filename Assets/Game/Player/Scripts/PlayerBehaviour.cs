using GameCore;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game
{
	public class PlayerBehaviour : CoreBehaviour, IFixedTick
	{
		[Header("Настройки")]
		public float speed = 10f;
		public float jumpForce = 15f;
		public float friction = 10f;

		protected Rigidbody2D rb;
		protected float moveDirection;

		protected override void Awake()
		{
			rb = GetComponent<Rigidbody2D>();
			Hub.Add(this);
		}

		public void Forward()
		{
			moveDirection = 1f;
		}

		public void Backward()
		{
			moveDirection = -1f;
		}

		public void Stop()
		{
			moveDirection = 0f;
		}

		public void Jump()
		{
			rb.AddForce(transform.up * jumpForce, ForceMode2D.Impulse);
		}

		protected void HandleMovement(float deltaTime)
		{
			Vector2 targetVelocity = transform.right * moveDirection * speed;
			float currentXVel = Vector2.Dot(rb.linearVelocity, transform.right);
			float newXVel = Mathf.Lerp(currentXVel, moveDirection * speed, deltaTime * friction);
			float currentYVel = Vector2.Dot(rb.linearVelocity, transform.up);
			rb.linearVelocity = (Vector2)transform.right * newXVel + (Vector2)transform.up * currentYVel;
		}

		public void FixedTick(float deltaTime)
		{
			HandleMovement(deltaTime);
		}
	}
}