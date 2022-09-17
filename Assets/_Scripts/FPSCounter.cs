using System.Collections;
using UnityEngine;

public class FPSCounter : MonoBehaviour
{
	string text = "";
	float count;

	IEnumerator Start()
	{
		GUI.depth = 2;
		while (true)
		{
			if (Time.timeScale == 1)
			{
				yield return new WaitForSeconds(0.1f);
				count = (1 / Time.deltaTime);
				text = "FPS :" + (Mathf.Round(count));
			}
			else
			{
				text = "Pause";
			}
			yield return new WaitForSeconds(0.5f);
		}
	}

	void OnGUI()
	{
		GUI.contentColor = Color.cyan;
		GUI.skin.label.fontSize = 30;
		GUI.Label(new Rect(50, 50, 200, 50), text);
	}
}
