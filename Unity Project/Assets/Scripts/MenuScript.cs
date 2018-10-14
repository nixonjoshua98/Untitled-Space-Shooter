using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour {
	public Button musicButton;
	public Button SFXButton;

	public GameObject credits;

	private GameObject creditsObj;

	public void Start() {
		UpdateButtonText();
	}

	public void StartGame() {
		SceneManager.LoadScene (1);
	}
	public void ExitGame() {
		UnityEditor.EditorApplication.isPlaying = false;
		Application.Quit ();
	}

	public void OpenCredits() {
		GameObject.Find ("MainMenu").transform.GetChild (0).gameObject.SetActive (false);	
		creditsObj = Instantiate (credits);
		InvokeRepeating ("CheckLeftClick", 0.0f, 0.05f);
	}

	public void ToggleSFX() {
		SoundScript.instance.ToggleSFX ();
		UpdateButtonText();
	}
	public void ToggleMusic() {
		SoundScript.instance.ToggleMusic ();
		UpdateButtonText();
	}

	public void UpdateButtonText() {
		if (SFXButton != null) {
			SFXButton.GetComponentInChildren<Text>().text   = SoundScript.instance._playSFX == true ? "SFX [ON]" : "SFX [OFF]";
			musicButton.GetComponentInChildren<Text>().text = SoundScript.instance._playMusic == true ? "MUSIC [ON]" : "MUSIC [OFF]";
		}
	}

	public void CheckLeftClick() {
		if (Input.GetMouseButtonDown (0)) {
			GameObject.Find ("MainMenu").transform.GetChild (0).gameObject.SetActive (true);
			Destroy (creditsObj.gameObject);
		}
	}
}