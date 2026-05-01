using UnityEngine;
namespace Game
{
	[CreateAssetMenu(fileName = "InputSettings", menuName = "Game/InputSettings")]
	public class KeyboardInputSettings : ScriptableObject
	{
		[SerializeField] public KeyCode Forward, Backward, Jump;
	}
}