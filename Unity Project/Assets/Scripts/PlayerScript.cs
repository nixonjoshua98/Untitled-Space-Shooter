using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlayerScript : MonoBehaviour
{
	public GameObject bullet;
    private float lastShotTime = 0.0f;

	private void Start() 
	{
		GameScript.instance.ActivateShield (transform);
	}

	private void FixedUpdate()
    {
        this.MovePlayer();
		if (Input.GetKey ("space") == false && Time.time > lastShotTime + GameScript.instance.playerShootDelay)
		{
			this.Shoot ();
		}
    }

	private void MovePlayer()
    {
        float horizontalAxis   = Input.GetAxis("Horizontal");
        float verticalAxis     = Input.GetAxis("Vertical");
		verticalAxis = verticalAxis > 0 ? verticalAxis * 1.2f : verticalAxis * 0.8f; // Forwards > backwards

		Vector2 currentPosition = transform.position;
        Vector2 movementVector  = new Vector2(horizontalAxis, verticalAxis);
		Vector2 endPosition     = currentPosition + (movementVector * Time.fixedDeltaTime * GameScript.instance.playerMoveSpeed);
		this.GetComponent<Rigidbody2D>().MovePosition (Vector2.Lerp (currentPosition, endPosition, 1.0f));
    }

	private void Shoot()
    {
		SoundScript.instance.PlaySFX (GetComponent<AudioSource> (), GetComponent<AudioSource> ().volume);
        lastShotTime                  = Time.time;
        GameObject bulletShot         = Instantiate(bullet);
        bulletShot.transform.position = transform.position;
        Rigidbody2D bulletShotRd2D    = bulletShot.GetComponent<Rigidbody2D>();
		bulletShotRd2D.AddForce(Vector2.up * GameScript.instance.playerBulletSpeed);
    }

	private GameObject HitItem(Collider2D coll) // return null if didn't hit item
	{
		GameObject[] droppableItems = GameScript.instance.droppableItems;
		for(int x = 0 ; x < droppableItems.Length ; x++) 
		{
			if (droppableItems[x].tag == coll.tag)
			{
				return droppableItems[x];
			}
		}
		return null;
	}

	// Hit by enemy
	private void HitEnemy(Collider2D coll) {
		int i = coll.gameObject.tag.IndexOf ("Bullet");
		if (i == -1) {
			GameScript.instance.HitByEnemy (coll.gameObject.tag, transform);
			EnemyScript enemyScript = coll.GetComponent<EnemyScript> ();
			if (enemyScript.formation > 0) {
				EnemySpawnScript.instance.FormationDead (enemyScript.formation);
			}
		} else {
			GameScript.instance.HitByEnemy (coll.gameObject.tag.Remove(coll.gameObject.tag.IndexOf("Bullet")), transform);
		}
		Destroy(coll.gameObject);
	}
		

	private void OnTriggerEnter2D(Collider2D coll)
    {
		GameObject itemHit = this.HitItem (coll);

		if (itemHit != null) // Player hit a item
		{
			GameScript.instance.ActivateItem (transform, itemHit);
			Destroy (coll.gameObject);
		}

		// Got Hit
		else if (GameScript.instance.badItems.Contains(coll.gameObject.tag)) {
			this.HitEnemy (coll);
        }
	}
}