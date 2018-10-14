using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundScript : MonoBehaviour {
	public static BackgroundScript instance = null;

	public GameObject levelOneBackground;
	public GameObject levelTwoBackground;
	public GameObject levelThreeBackground;
	public GameObject currentBackground;

	private void Awake(){
		if (instance == null) {
			instance = this;
		} else if (instance != this) {
			Destroy (gameObject);
		}
	}

	public void UpdateBackground() {
		GameObject newBackground = null;
		switch (GameScript.instance.currentLevel) {
		case 1:
			newBackground = levelOneBackground;
			break;
		case 2:
			newBackground = levelTwoBackground;
			break;
		case 3:
			newBackground = levelThreeBackground;
			break;
		}
		Destroy (currentBackground.gameObject);
		currentBackground = Instantiate (newBackground);
	}
}
