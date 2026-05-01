using System.Collections.Generic;
using UnityEngine;
namespace Game

{
	using GameCore;
	using System.Collections.Generic;
	using UnityEngine;
	public class GravityObject : MonoBehaviour
	{
		[SerializeField] protected Collider2D targetCollider;
		[Header("Настройки гравитации")]
		[SerializeField] protected float gravityForce = 45f;
		[SerializeField] protected float rotationSpeed = 30f;

		[Tooltip("Слой, на котором лежат платформы")]
		[SerializeField] protected LayerMask groundLayer;
		protected float rayDistance = float.MaxValue;
		protected Collider2D triggerCollider;
		protected List<GravityObject> gravityObjects;


		virtual protected void OnEnable()
		{
			if (Hub.Get<List<GravityObject>>() == null)
			{
				Hub.Add(new List<GravityObject>());
			}
			gravityObjects = Hub.Get<List<GravityObject>>();
			gravityObjects.Add(this);
		}
		virtual protected void OnDisable()
		{
			gravityObjects.Remove(this);
		}


		virtual protected void Awake()
		{
			triggerCollider = GetComponent<Collider2D>();
		}

		virtual protected void OnTriggerStay2D(Collider2D other)
		{
			Rigidbody2D rb = other.attachedRigidbody;
			if (rb != null && rb.bodyType != RigidbodyType2D.Kinematic)
			{
				if (IsDominant(rb.position))
				{
					ApplyGravity(rb);
				}
			}
		}

		virtual protected bool IsDominant(Vector2 playerPosition)
		{
			float distanceToPlayer = Vector2.Distance(playerPosition, targetCollider.ClosestPoint(playerPosition));

			foreach (var obj in gravityObjects)
			{
				if (obj == this) continue;
				float otherDist = Vector2.Distance(playerPosition, obj.targetCollider.ClosestPoint(playerPosition));

				if (otherDist < distanceToPlayer - 0.01f)
					return false;

				if (Mathf.Abs(otherDist - distanceToPlayer) < 0.02f)
				{
					if (obj.GetInstanceID() > GetInstanceID())
						return false;
				}
			}
			return true;
		}

		virtual protected void ApplyGravity(Rigidbody2D rb)
		{
			Vector2 playerPos = rb.position;
			Vector2 downDirection = targetCollider.ClosestPoint(playerPos) - playerPos;
			if (downDirection == Vector2.zero)
				downDirection = (Vector2)transform.position - playerPos;
			Vector2 gravityDir = downDirection;

			RaycastHit2D hit = Physics2D.Raycast(playerPos, downDirection, rayDistance, groundLayer);

			Debug.DrawRay(playerPos, downDirection * rayDistance, hit.collider ? Color.green : Color.red);

			if (hit.collider != null)
			{
				gravityDir = -hit.normal;
			}
			else
			{
				gravityDir = ((Vector2)transform.position - playerPos).normalized;
			}

			float appliedForce = gravityForce;
			if (Vector2.Dot(rb.linearVelocity, -gravityDir) > 1f)
			{
				appliedForce *= 0.5f;
			}

			rb.AddForce(gravityDir * appliedForce);

			float targetAngle = Mathf.Atan2(gravityDir.y, gravityDir.x) * Mathf.Rad2Deg + 90f;
			float smoothedAngle = Mathf.LerpAngle(rb.rotation, targetAngle, rotationSpeed * Time.fixedDeltaTime);

			rb.MoveRotation(smoothedAngle);
		}
	}
}