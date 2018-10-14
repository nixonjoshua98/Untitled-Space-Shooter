using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundScript : MonoBehaviour {
	public static SoundScript instance = null;

	public bool playSFX = true;
	public bool playMusic = true;

	public bool _playSFX   {get { return this.playSFX;  } }
	public bool _playMusic {get { return this.playMusic;} }

	private AudioSource currentMusic;

	private void Awake()
	{
		if (instance == null) {
			instance = this;
			DontDestroyOnLoad (this);
		} else {
			Destroy (this.gameObject);
		}
	}

	public bool ToggleSFX() {
		playSFX = !playSFX;
		return playSFX;
	}

	public bool ToggleMusic() {
		playMusic = !playMusic;

		if (!playMusic) {
			StopMusic ();
		} else {
			PlayMusic (GameObject.Find ("Music").GetComponent<AudioSource> (), (GameObject.Find ("Music").GetComponent<AudioSource> ().volume));
		}
		return playMusic;
	}

	public void PlayMusic(AudioSource myAudio, float vol = 1.0f)
	{
		if (playMusic) {
			this.StopMusic();
			currentMusic = myAudio;
			currentMusic.volume = vol;
			currentMusic.loop = true;
			currentMusic.Play ();
		}
	}
	public void PlaySFX(AudioSource myAudio, float vol = 1.0f)
	{
		if (playSFX) {
			myAudio.volume = vol;
			myAudio.Play ();
		}
	}

	public void StopMusic() {
		if (currentMusic != null) {
			currentMusic.Stop ();
		}
	}
}
