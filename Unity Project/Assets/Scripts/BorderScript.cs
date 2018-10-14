using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BorderScript : MonoBehaviour {

	void OnTriggerEnter2D(Collider2D coll)
	{
		Destroy (coll.gameObject);
	}
}