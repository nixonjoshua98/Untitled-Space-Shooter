using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEngine.UI;

public class GameScript : MonoBehaviour
{
	public static GameScript instance = null;

	// Game tags
	private string[] enemyBulletTags = { "EnemyABullet", "EnemyBBullet", "EnemyCBullet" };
    public string[] enemyTags;
	public string[] badItems = new string[] {"EnemyA", "EnemyB", "EnemyC", "EnemyD", "EnemyCBullet", "EnemyBBullet", "EnemyABullet"};
	private string[] itemTags        = { "ShieldItem", "HealthItem", "LifeItem", "DamageItem", "MovementItem", "FireRateItem" };

	public Dictionary<string, int> itemWeights = new Dictionary<string, int>()
	{
		{"ShieldItem"  , 17},
		{"HealthItem"  , 13},
		{"LifeItem"    , 10},
		{"DamageItem"  , 15},
		{"MovementItem", 20},
		{"FireRateItem", 25},
	};

	// Interface objects
	private GameObject[] playerlifesObj = new GameObject[5];

	public Slider healthSlider, shieldSlider;
	public Text scoreText, continueText, middleScreenText;

	// Game objects
	public GameObject playerObj;
	public GameObject shieldItem, healthItem, lifeItem, damageItem, movementSpeedItem, fireRateItem;

    // Game vars
	public bool gameActive = true;
	public GameObject[] droppableItems;
	public List<GameObject> droppableItemsWeighted;
	private GameObject playerShield = null;

	// Player status
	public float playerDamage       = 1;
	public float playerBulletSpeed  = 600.0f;
	public float playerShootDelay   = 0.5f;
	public float playerMoveSpeed    = 5.0f;
	public int currentLevel         = 0;

    private int playerHealth        = 100;
	private int playerLifes         = 3;
    private int playerScore         = 0;
	private int shieldHealth        = 50;
	private bool shieldActivated    = false;

	private void Start()
    {
        enemyTags = new string[] { "EnemyA", "EnemyB", "EnemyC", "EnemyD" };
        droppableItems     = new GameObject[] {lifeItem, shieldItem, healthItem, damageItem, movementSpeedItem, fireRateItem};

		foreach (GameObject item in droppableItems) {
			droppableItemsWeighted.AddRange(Enumerable.Repeat(item, itemWeights[item.tag]));
		}

		this.CreateInitialPlayerLifes ();

		InvokeRepeating ("UpdateInterface", 0.0f, 0.5f);

	}

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
		else if (instance != this)
		{
			Destroy(gameObject);
		}
	}

	private void UpdateInterface()
	{
		healthSlider.value = playerHealth;
		shieldSlider.value = shieldHealth;
		scoreText.text = "SCORE: " + playerScore;
	}

	private void CreateInitialPlayerLifes()
	{
		// Create empty game object to hold lifes
		GameObject heartContainer = new GameObject();
		heartContainer.name = "Player Heart Container";

		for (int x = 0; x < 5; x++) 
		{
			GameObject heart = Instantiate (lifeItem);
			heart.gameObject.tag   = "Untagged";
			heart.gameObject.name  = "Heart " + (x + 1);
			heart.transform.parent = heartContainer.transform;
			heart.transform.position   = new Vector3 (-8 + x, 6, 0);
			heart.transform.localScale = new Vector2 (0.1f, 0.1f);
			if (x + 1> this.playerLifes) {
				heart.gameObject.SetActive (false);
			}
			playerlifesObj [x] = heart;
		}
	}

	private void AddPlayerLife()
	{
		if (playerLifes < 5) 
		{
			playerLifes++;
			playerlifesObj [playerLifes - 1].gameObject.SetActive (true);
		}
	}

	private void RemovePlayerLife()
	{
		if (playerLifes > 0)
		{
			playerLifes--;
			playerlifesObj [playerLifes].gameObject.SetActive (false);
		}
	}

    private void ManagePlayerStats(int damageAmount = 0, Transform playerTransform = null)
    {
        if (!shieldActivated)
        {
			playerHealth -= Mathf.Min(damageAmount, playerHealth);
			if (playerHealth == 0)
			{
				if (playerLifes > 1) 
				{
					this.OnDeath (playerTransform);
				}
				else if (playerLifes == 1) 
				{
					this.OnGameOver ();
				}
			}
        }
        else if (shieldActivated)
        {
			shieldHealth -= Mathf.Min(damageAmount, shieldHealth);
            if (shieldHealth == 0)
            {
				this.DeactivateShield ();
            }
        }
    }

	private void ContinueCheck() {
		if (Input.GetKey ("space") || Input.GetMouseButtonDown(0)) {
			CancelInvoke ("ContinueCheck");
			SceneManager.LoadScene (0);
		}
	}

	private void OnGameOver(string text = "GAME OVER")
	{
		if (text == "GAME OVER") {
			SoundScript.instance.PlaySFX (GetComponents<AudioSource> () [5], GetComponents<AudioSource> () [5].volume);
		} else {
			SoundScript.instance.PlaySFX (GetComponents<AudioSource> () [6], GetComponents<AudioSource> () [6].volume);
		}
		SoundScript.instance.StopMusic();

		gameActive = false;
        EnemySpawnScript.instance.gameLoopRunning = false;
		continueText.text = "Press space to return to the main menu";
		InvokeRepeating ("ContinueCheck", 0.0f, 0.05f);
        middleScreenText.text  = text;
		middleScreenText.color = Color.red;
        scoreText.rectTransform.localPosition =  new Vector3(0, -50.0f, 0);

        this.RemovePlayerLife ();
		this.RemovePlayerBuffs();
		this.DestroyAll (enemyBulletTags);
		this.DestroyAll (enemyTags);
		this.DestroyAll (itemTags);

		playerHealth = 0;
		// Lifes get set to 0 in RemovePlayerLife
		healthSlider.gameObject.SetActive (false);
		Destroy (playerObj.gameObject);
	}

	public void OnGameWon()
	{
		this.OnGameOver ("YOU WIN!");
		for (int x = 0; x < 5; x++)
		{
			this.RemovePlayerLife ();
		}
	}
	public IEnumerator NewLevelUI(int duration) 
	{
        if (gameActive)
        {
            currentLevel++;
			this.ChangeAudio ();
			BackgroundScript.instance.UpdateBackground ();
            middleScreenText.text = "LEVEL " + currentLevel;
            yield return new WaitForSeconds(duration);
            if (gameActive)
            {
                middleScreenText.text = "";
            }
        }
	}

	private void ChangeAudio() {
		switch (currentLevel) {
		case 1:
			SoundScript.instance.PlayMusic (GetComponents<AudioSource> ()[0], 0.3f);
			break;
		case 2:
			SoundScript.instance.PlayMusic (GetComponents<AudioSource> ()[1]);
			break;
		case 3:
			SoundScript.instance.PlayMusic (GetComponents<AudioSource> ()[2]);
			break;
        }
	}

	private void OnDeath(Transform playerTransform)
	{
		SoundScript.instance.PlaySFX (GetComponents<AudioSource> ()[3], GetComponents<AudioSource> ()[3].volume);
		this.RemovePlayerBuffs ();
		this.RemovePlayerLife ();
		this.DestroyAll (enemyBulletTags);
		this.DestroyAll (itemTags);
		this.ActivateShield(playerTransform);
		playerHealth = 100;
		playerTransform.position = new Vector3 (0, -4, 0);
	}

	private void DestroyAll(string[] tags)
	{
		GameObject[] objects;

        foreach (string tag in tags)
		{
			objects = GameObject.FindGameObjectsWithTag(tag);
			foreach (GameObject obj in objects) 
			{
				Destroy (obj.gameObject);
			}
		}
	}

	private void RemovePlayerBuffs()
	{
		this.DeactivateShield ();
		playerDamage      = 1.0f;
		playerMoveSpeed   = 5.0f;
		playerShootDelay  = 0.5f;
		playerBulletSpeed = 600.0f;
	}

	private void ActivateHealthItem()
	{
		playerHealth = 100;
	}

	private void ActivateLifeItem()
	{
		this.AddPlayerLife ();
	}

	private void ActivateDamageItem()
	{
		playerDamage = 2.0f;
	}

	private void ActivateSpeedItem()
	{
		playerMoveSpeed = 10.0f;
	}

	private void ActivateFireRateItem()
	{
		playerShootDelay = 0.25f;
		playerBulletSpeed = 900.0f;
    }

	public void ActivateShield(Transform playerTrans)
	{
		if (!shieldActivated)
		{
			playerShield = Instantiate(shieldItem);
			playerShield.transform.name = "Shield Item";
			playerShield.transform.tag = "PlayerShield";
			playerShield.transform.parent   = playerTrans;
			Destroy(playerShield.GetComponent<Rigidbody2D>());
            Destroy(playerShield.GetComponent<CircleCollider2D>());
            playerShield.transform.position = playerTrans.position;
		}

		shieldActivated = true;
		shieldHealth    = 50;
		shieldSlider.gameObject.SetActive (true);
	}

	public void DeactivateShield()
	{
		if (shieldActivated)
		{
			Destroy (playerShield.gameObject);
			shieldHealth    = 0;
			shieldActivated = false;
			shieldSlider.gameObject.SetActive (false);
		}
	}

	public void DropItem (Transform trans, int dropChance)
	{
		if (UnityEngine.Random.Range (1, 100) <= dropChance)
		{
			GameObject chosenItem  = droppableItemsWeighted [UnityEngine.Random.Range (0, droppableItemsWeighted.Count)];
			GameObject itemDropped = Instantiate (chosenItem);
			itemDropped.transform.position = trans.position;
			if (chosenItem.tag == "ShieldItem") 
			{
				itemDropped.transform.localScale = new Vector2 (1.5f, 1.5f);
			}
			itemDropped.GetComponent<Rigidbody2D> ().AddForce (Vector2.down * 100);
		}
	}

	public void ActivateItem(Transform trans, GameObject item)
	{
		SoundScript.instance.PlaySFX (GetComponents<AudioSource> ()[4], GetComponents<AudioSource> ()[4].volume);
		if (item.tag == "HealthItem") 
		{
			this.ActivateHealthItem ();
		}
		else if (item.tag == "LifeItem") 
		{
			this.ActivateLifeItem ();
		}
		else if (item.tag == "ShieldItem") 
		{
			this.ActivateShield (trans);
		}
		else if (item.tag == "DamageItem") 
		{
			this.ActivateDamageItem ();
		}
		else if (item.tag == "MovementItem") 
		{
			this.ActivateSpeedItem ();
		}
		else if (item.tag == "FireRateItem") 
		{
			this.ActivateFireRateItem ();
		}
	}

	public void HitByEnemy(string enemyTag, Transform playerTransform)
    {
		this.ManagePlayerStats(EnemySpawnScript.instance.enemies[enemyTag].damage, playerTransform);
		playerScore = Math.Max (0, playerScore - 10);
    }

    public void EnemyKilled(string enemyTag)
    {
		SoundScript.instance.PlaySFX (GetComponents<AudioSource> ()[3], GetComponents<AudioSource> ()[3].volume);
        playerScore += EnemySpawnScript.instance.enemies[enemyTag].scoreWhenKilled;
    }
}