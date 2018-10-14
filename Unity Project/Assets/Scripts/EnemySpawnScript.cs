using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class EnemyInfo {
	public GameObject ship = null;
	public int currentNode = 0;
	public int nodeStartLoop; // Which node to loop back to afterwards
	public int health;
	public int damage;
	public int speed;
	public int itemDropChance;
    public int scoreWhenKilled;
    public int shotCooldown;
	public int bulletForce;
	public int formationSize;
	public int spawnDelay;
	public Vector2[] movementPath;
}

public class EnemySpawnScript : MonoBehaviour {
	public static EnemySpawnScript instance = null;

	// Enemy dict - took the idea from here
	// https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/how-to-initialize-a-dictionary-with-a-collection-initializer
	public Dictionary<string, EnemyInfo> enemies = new Dictionary<string, EnemyInfo>()
	{
		{"EnemyA", new EnemyInfo {
				nodeStartLoop = 1,
				health = 2,
				damage = 20,
				speed = 6,
                scoreWhenKilled = 15,
				itemDropChance = 10,
                shotCooldown = 2, 
				bulletForce = 150,
				formationSize = 5,
				spawnDelay = 2,
				movementPath = new Vector2[]
				{ 
					new Vector2(10.0f,   5.0f), 
					new Vector2(8.0f ,   2.0f),
					new Vector2(2.5f ,   5.0f), 
					new Vector2(-2.5f,   2.0f), 
					new Vector2(-8.0f,   3.5f),
					new Vector2(-5.0f,  -3.0f), 
					new Vector2(5.0f ,  -2.0f),
				}
			}
		},

		{"EnemyB", new EnemyInfo {
				nodeStartLoop = 1,
				health = 3,
				damage = 30,
				speed = 8,
                scoreWhenKilled = 30,
				itemDropChance = 60,
                shotCooldown = 2, 
				bulletForce = 250,
				formationSize = 3,
				spawnDelay = 3,
				movementPath = new Vector2[]
				{ 
					new Vector2(-10.0f,  5.0f), 
					new Vector2(-5.0f ,  3.5f),
					new Vector2(-7.5f ,  0.0f),
					new Vector2(-5.0f , -3.5f),
					new Vector2(-2.5f ,  0.0f),
					new Vector2(5.0f  , -3.5f),
					new Vector2(7.5f  ,  0.0f),
					new Vector2(5.0f  ,  3.5f),
					new Vector2(2.5f  ,  0.0f),
				}
			}
		},

		{"EnemyC", new EnemyInfo {
				nodeStartLoop = 1,
				health = 5,
				damage = 60,
                scoreWhenKilled = 50,
                speed = 3,
				shotCooldown = 3,
				itemDropChance = 60,
				bulletForce = 300,
				formationSize = 1,
				spawnDelay = 1,
				movementPath = new Vector2[]
				{ 
					new Vector2(10.0f,  0.0f), 
					new Vector2(-8.0f,  2.0f),
					new Vector2(-8.0f, -2.0f),
					new Vector2(8.0f , -2.0f),
					new Vector2(8.0f ,  2.0f),
				}
			}
		},

		{"EnemyD", new EnemyInfo {
				nodeStartLoop = 1,
				health = 15,
				damage = 0,
                scoreWhenKilled = 30,
                speed = 1,
				shotCooldown = 0, 
				itemDropChance = 100,
				bulletForce = 0,
				formationSize = 1,
				spawnDelay = 1,
				movementPath = new Vector2[]
				{ 
					new Vector2(0.0f,  5.5f), 
					new Vector2(0.0f,  4.0f),
				}
			}
		},
	};

	public GameObject enemyA, enemyB, enemyC, enemyD;

	private int[] formations;
	private object[] gamePath;
	private int currentGameNode   = 0;
	public bool gameLoopRunning   = true;
	public bool levelEnded        = false;
	private int oldGameNode;
	private int currentFormationNum = 0;

	// Use this for initialization
	void Start () {
		enemies ["EnemyA"].ship = enemyA;
		enemies ["EnemyB"].ship = enemyB;
		enemies ["EnemyC"].ship = enemyC;
		enemies ["EnemyD"].ship = enemyD;

		gamePath = new object[] 
		{
			"NextLevel",  2, // Level One
			   "EnemyA",  3,
			   "EnemyB",  12,
               "EnemyA",  6,
			   "EnemyB",  1,

            "NextLevel",  2, // Level Two
               "EnemyA",  5,
			   "EnemyC",  3,
			   "EnemyB",  2,
			   "EnemyD",  2,
			   "EnemyC",  6,
			   "EnemyA",  5,
			   "EnemyB",  7,
			   "EnemyA",  1,

			"NextLevel",  2, // Level three
			   "EnemyD",  1,
			   "EnemyC",  2,
			   "EnemyA",  4,
			   "EnemyB",  3,
			   "EnemyC",  5,
			   "EnemyA",  7,
			   "EnemyB",  4,
			   "EnemyC",  3,
			   "EnemyA",  1,
		};

		formations = new int[gamePath.Length];

		StartCoroutine (Spawner ());
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

	private IEnumerator Spawner()
	{
		while (gameLoopRunning) 
		{
			if (GameScript.instance.enemyTags.Contains(gamePath [currentGameNode]) && !levelEnded)
			{
				StartCoroutine (SpawnEnemyFormation (enemies [gamePath [currentGameNode].ToString ()]));
			} 
			else if (((string) gamePath [currentGameNode] == "NextLevel") && !levelEnded) 
			{
				if (GameScript.instance.currentLevel == 0)
				{
					StartCoroutine (GameScript.instance.NewLevelUI ((int)gamePath [currentGameNode + 1]));

				} 
				else 
				{
					levelEnded = true;
					InvokeRepeating ("CheckLevelCompleted", 0.0f, 0.5f);
					oldGameNode = currentGameNode + 1;
					currentGameNode += 2;
				}
			} 
				
			if (currentGameNode + 2 < gamePath.Length && !levelEnded)
			{
				currentGameNode++;
				yield return new WaitForSeconds ((int)gamePath [currentGameNode]);
				currentGameNode++;
			} 
			else if (currentGameNode + 2 >= gamePath.Length && !levelEnded) 
			{ 
				gameLoopRunning = false;
				InvokeRepeating ("CheckGameCompletion", 0.0f, 0.5f);
			}
			else
			{
				yield return new WaitForSeconds (1);	
			}
		}
	}

	private void CheckGameCompletion()
	{
		if (this.AllShipsDestroyed () && !gameLoopRunning && currentGameNode + 2 >= gamePath.Length) 
		{
			if (GameScript.instance.gameActive) 
			{
				GameScript.instance.OnGameWon ();
			} 
			CancelInvoke ("CheckGameCompletion");
		}
	}

	private void CheckLevelCompleted()
	{
		if (this.AllShipsDestroyed ()) 
		{
			levelEnded = false;
			CancelInvoke ("CheckLevelCompleted");
			StartCoroutine (GameScript.instance.NewLevelUI ((int) gamePath[oldGameNode]));
		}
	}

	public bool FormationDead(int formationNum) {
		if (formationNum >= 0) {
			formations [formationNum]--;
			if (formations [formationNum] == 0) {
				return true;
			}
		}
		return false;
	}

	private bool AllShipsDestroyed()
	{
		foreach (string enemyTag in GameScript.instance.enemyTags)
		{
			if (GameObject.FindGameObjectsWithTag (enemyTag).Length > 0) 
			{
				return false;
			}
		}
		return true;
	}

	private IEnumerator SpawnEnemyFormation(EnemyInfo enemy)
	{
		int enemiesSpawned = 0;
		int formationNum = -1;
		if (enemy.ship.tag == "EnemyA" || enemy.ship.tag == "EnemyB") {
			formationNum = currentFormationNum;
			currentFormationNum++;
			formations [formationNum] = 0;
		}

		while (enemiesSpawned < enemy.formationSize && GameScript.instance.gameActive) {
			GameObject anEnemy = Instantiate (enemy.ship);
			EnemyScript anEnemyScript = anEnemy.GetComponent<EnemyScript> ();
			anEnemyScript.formation = formationNum;
			enemiesSpawned++;
			if (formationNum > 0) {
				formations [formationNum]++;
			}
			yield return new WaitForSeconds (enemy.spawnDelay);
		}
	}
}