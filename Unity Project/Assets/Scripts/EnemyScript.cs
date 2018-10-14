using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyScript : MonoBehaviour
{
	public GameObject bullet = null;

	private float health;
	private int currentNode;
	public int formation;

	void Start() {
		health        = EnemySpawnScript.instance.enemies [gameObject.tag].health;
		currentNode   = 0;

		transform.position = EnemySpawnScript.instance.enemies [gameObject.tag].movementPath [EnemySpawnScript.instance.enemies [gameObject.tag].currentNode];

		InvokeRepeating ("Move", 0.0f, 0.05f);
		if (EnemySpawnScript.instance.enemies [gameObject.tag].shotCooldown > 0) 
		{
			InvokeRepeating ("Shoot", 2, EnemySpawnScript.instance.enemies [gameObject.tag].shotCooldown);
		}
	}

	void Move()
    {
		Vector2 endPos;
		bool resetPath = false;
		Vector2 currentPos = transform.position;

		if (currentNode + 1 == EnemySpawnScript.instance.enemies [gameObject.tag].movementPath.Length) 
		{
			endPos = EnemySpawnScript.instance.enemies [gameObject.tag].movementPath [EnemySpawnScript.instance.enemies [gameObject.tag].nodeStartLoop];
			resetPath = true;
		} 
		else
		{
			endPos = EnemySpawnScript.instance.enemies [gameObject.tag].movementPath[currentNode + 1];
		}


		if (Mathf.Abs (transform.position.x - endPos.x) < 0.1 && Mathf.Abs (transform.position.y - endPos.y) < 0.1) 
		{

			if (resetPath) 
			{
				currentNode = EnemySpawnScript.instance.enemies [gameObject.tag].nodeStartLoop;
			} 
			else 
			{
				currentNode++;
			}
		}
		transform.position = Vector2.MoveTowards(transform.position, endPos,   EnemySpawnScript.instance.enemies [gameObject.tag].speed * Time.deltaTime);
    }

    void Shoot()
    {
		SoundScript.instance.PlaySFX (GetComponent<AudioSource> (), GetComponent<AudioSource> ().volume);
		GameObject bulletShot         = Instantiate(bullet);
        bulletShot.transform.position = transform.position;
        Rigidbody2D bulletShotRd2D    = bulletShot.GetComponent<Rigidbody2D>();
		bulletShotRd2D.AddForce(Vector2.down * EnemySpawnScript.instance.enemies[gameObject.tag].bulletForce);
    }

	void ShotByPlayer(Collider2D coll)
	{
		Destroy(coll.gameObject);
		health -= GameScript.instance.playerDamage;

		if (health <= 0)
		{
            GameScript.instance.EnemyKilled(gameObject.tag);
			if (EnemySpawnScript.instance.FormationDead (formation) || EnemySpawnScript.instance.enemies [gameObject.tag].itemDropChance == 100 || formation == -1) {
				GameScript.instance.DropItem (transform, EnemySpawnScript.instance.enemies [gameObject.tag].itemDropChance);
			}				
			Destroy(this.gameObject);
		}
	}

	void OnTriggerEnter2D(Collider2D coll)
	{
		if (coll.gameObject.CompareTag ("PlayerBullet")) 
		{
			this.ShotByPlayer (coll);
		}
	}
}