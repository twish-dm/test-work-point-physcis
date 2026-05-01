using GameCore;
using System.Collections;
using UnityEngine;
namespace Game
{
	[RequireComponent(typeof(Ticker))]
	public class Bootstrap : MonoBehaviour
	{
		[SerializeField] protected GameObject[] activeAfterInitObjects;
		private IEnumerator Start()
		{

			// 1. »Õ»÷»¿À»«¿÷»ﬂ —»—“≈Ã

			var eventer = new Eventer();
			var storer = new Storer();
			var saver = new Saver(storer);
			var loader = new Loader();
			var pooler = new Pooler();
			var ticker = gameObject.AddComponent<Ticker>();
			var viewer = GameObject.FindFirstObjectByType<Viewer>();
			var gameStater =  GameObject.FindFirstObjectByType<GameStater>();
			Hub.Add(loader);
			Hub.Add(storer);
			Hub.Add(saver);
			Hub.Add(pooler);
			Hub.Add(ticker);
			Hub.Add(viewer);
			Hub.Add(eventer);
			Hub.Add(gameStater);
			Hub.Add(Camera.main);
			for (int i = 0; i < activeAfterInitObjects.Length; i++)
			{
				activeAfterInitObjects[i].SetActive(true);
			}
			yield break;
		}
	}
}