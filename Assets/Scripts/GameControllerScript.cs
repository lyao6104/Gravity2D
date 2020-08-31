using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameControllerScript : MonoBehaviour
{
	public GameObject dustPrefab;
	public List<SpeckScript> dustSpecks;
	public SpriteRenderer backgroundSprite;
	public Color noCollideColour, collideColour;
	public bool enableDustCollisions;

	public float speckSpawnRadius = 1f;
	public float gravitationalConstant = 6.674e-11f;
	public float maxGravityDistance = 25f;
	public int maxSpecks = 200;

	// Start is called before the first frame update
	private void Start()
	{
		Time.fixedDeltaTime = 0.05f;
		Application.targetFrameRate = 60;
		backgroundSprite = GameObject.Find("Background").GetComponent<SpriteRenderer>();
		if (enableDustCollisions)
		{
			backgroundSprite.color = collideColour;
		}
		else
		{
			backgroundSprite.color = noCollideColour;
		}
		ToggleCollisionMode();
		Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Specks"), LayerMask.NameToLayer("Specks"), !enableDustCollisions);
	}

	private void FixedUpdate()
	{
		// Simulate gravity for dust specks. This code is really unoptimized, and I'd like to find a better method of doing this.
		for (int i = 0; i < dustSpecks.Count; i++)
		{
			for (int j = 0; j < i; j++)
			{
				if (i == j)
				{
					continue;
				}

				Rigidbody2D rb1 = dustSpecks[i].GetComponent<Rigidbody2D>(), rb2 = dustSpecks[j].GetComponent<Rigidbody2D>();
				float distance = Vector2.Distance(rb1.position, rb2.position);

				if (distance > maxGravityDistance)
				{
					continue;
				}

				Vector2 direction = (rb2.position - rb1.position).normalized;
				float fg = gravitationalConstant * (rb1.mass * rb2.mass) / distance;
				rb1.AddForce(direction * fg);
				rb2.AddForce(direction * -fg);
			}
		}

		// Clicking/tapping will add specks to the simulation, up to a certain amount (performance reasons)
		if (dustSpecks.Count < maxSpecks)
		{
			if (Input.mousePresent && Input.GetMouseButton(0))
			{
				float offsetX = Random.Range(-speckSpawnRadius, speckSpawnRadius);
				float offsetY = Random.Range(-speckSpawnRadius, speckSpawnRadius);
				Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
				Vector2 spawnPos = new Vector2(mousePos.x + offsetX, mousePos.y + offsetY);

				CreateDust(spawnPos);
			}
			else if (Input.touchSupported && Input.touchCount > 0)
			{
				foreach (Touch touch in Input.touches)
				{
					float offsetX = Random.Range(-speckSpawnRadius, speckSpawnRadius);
					float offsetY = Random.Range(-speckSpawnRadius, speckSpawnRadius);
					Vector2 touchPos = Camera.main.ScreenToWorldPoint(touch.position);
					Vector2 spawnPos = new Vector2(touchPos.x + offsetX, touchPos.y + offsetY);

					CreateDust(spawnPos);
				}
			}
		}
	}

	private void Update()
	{
		if (Application.platform != RuntimePlatform.WebGLPlayer && Input.GetButtonDown("Exit"))
		{
			Application.Quit();
		}
		else if (Input.GetButtonDown("ToggleCollide"))
		{
			if (enableDustCollisions)
			{
				enableDustCollisions = false;
				backgroundSprite.color = noCollideColour;
			}
			else
			{
				enableDustCollisions = true;
				backgroundSprite.color = collideColour;
			}
			ToggleCollisionMode();
			Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Specks"), LayerMask.NameToLayer("Specks"), !enableDustCollisions);
		}
	}

	// This is done because, when collisions are toggled, there's a chance that specks will go flying through the background barrier.
	private void ToggleCollisionMode()
	{
		for (int i = 0; i < dustSpecks.Count; i++)
		{
			if (enableDustCollisions)
			{
				dustSpecks[i].GetComponent<Rigidbody2D>().collisionDetectionMode = CollisionDetectionMode2D.Discrete;
			}
			else
			{
				dustSpecks[i].GetComponent<Rigidbody2D>().collisionDetectionMode = CollisionDetectionMode2D.Continuous;
			}
		}
	}

	private void CreateDust(Vector2 pos)
	{
		SpeckScript newDust = Instantiate(dustPrefab, pos, Quaternion.identity, GameObject.Find("Specks").transform).GetComponent<SpeckScript>();
		dustSpecks.Add(newDust);
	}
}
