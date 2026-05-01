using Game;
using GameCore;
using UnityEngine;

public class InputBehaviour : CoreBehaviour
{
	[Inject]
	protected GameStater gameStater;

	public void Forward()
    {
        gameStater.Command("Forward", true);
	}
    public void Backward()
    {
		gameStater.Command("Backward", true);
	}
    public void Jump()
    {
		gameStater.Command("Jump", true);
	}
	public void ForwardStop()
	{
		gameStater.Command("Forward", false);
	}
	public void BackwardStop()
	{
		gameStater.Command("Backward", false);
	}
	public void JumpStop()
	{
		gameStater.Command("Jump", false);
	}
}
