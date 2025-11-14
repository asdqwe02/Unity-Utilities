using System;
using System.Collections;
using UnityEngine;

namespace Mint.Gdk.Utilities.Runtime
{
	// Static class for Coroutine handling outside of MonoBehaviour
	public static class CoroutineHandler
	{
		private static CoroutineRunner runner;

		public static CoroutineRunner GetStaticCoroutineRunner()
		{
			if (runner == null)
			{
				GameObject obj = new("CoroutineRunner");
				runner = obj.AddComponent<CoroutineRunner>();
				UnityEngine.Object.DontDestroyOnLoad(obj);
			}
			return runner;
		}

		public static void CreateCoroutine(Action action, float delay = 0f)
		{
			GameObject obj = new("CoroutineRunner");
			CoroutineRunner runner = obj.AddComponent<CoroutineRunner>();
			runner.StartCoroutine(action, delay);
		}

		// Internal MonoBehaviour class to handle coroutines
		public class CoroutineRunner : MonoBehaviour
		{
			Action action;
			float delay;

			public void StartCoroutine(Action action, float delay = 0f)
			{
				this.action = action;
				this.delay = delay;
				StartCoroutine(DelayedAction());
			}

			private IEnumerator DelayedAction()
			{
				yield return new WaitForSeconds(delay);
				action?.Invoke();
				Destroy(gameObject);
			}
		}
	}
}
