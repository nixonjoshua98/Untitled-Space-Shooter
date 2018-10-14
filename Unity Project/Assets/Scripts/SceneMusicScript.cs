using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneMusicScript : MonoBehaviour {
	void Start () {
		SoundScript.instance.PlayMusic (GetComponent<AudioSource> (), GetComponent<AudioSource> ().volume);
	}

}
