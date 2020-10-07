using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class GameControllerScript : MonoBehaviour
{
	public GameObject dustPrefab;
	public List<SpeckScript> dustSpecks;
	public SpriteRenderer backgroundSprite;
	public Color noCollideColour, collideColour;
	public bool enableDustCollisions;

	public bool startWithVelocity = false;
	public float maxVelocity = 50f;

	public float speckSpawnRadius = 1f, minSpawnRadius = 0f, maxSpawnRadius = 10f;
	private Bounds speckSpawnBounds;
	public int spawnVisualizerVertices = 36;
	private LineRenderer radiusRenderer;
	public float gravitationalConstant = 6.674e-11f;
	public float maxGravityDistance = 25f;
	public int maxSpecks = 200;

	public CinemachineTargetGroup targetGroup;

	// Start is called before the first frame update
	private void Start()
	{
		Application.targetFrameRate = 60;
		GameObject bgObject = GameObject.Find("Background");
		backgroundSprite = bgObject.GetComponent<SpriteRenderer>();
		speckSpawnBounds = bgObject.GetComponent<Collider2D>().bounds;
		float edgeRadius = bgObject.GetComponent<EdgeCollider2D>().edgeRadius;
		speckSpawnBounds.Expand(new Vector3(-edgeRadius, -edgeRadius)); // Shrink the bounds by the edge radius
		radiusRenderer = GetComponent<LineRenderer>();
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

		radiusRenderer.positionCount = spawnVisualizerVertices + 1;
		if (Input.mousePresent)
		{
			UpdateSpawnVisualizer();
		}

	}

	private void FixedUpdate()
	{
		// Simulate gravity for dust specks. This code is really unoptimized, and I'd like to find a better method of doing this.
		for (int i = 0; i < dustSpecks.Count; i++)
		{
			for (int j = 0; j < i; j++)
			{
				//if (i == j)
				//{
				//	continue;
				//}

				Rigidbody2D rb1 = dustSpecks[i].GetComponent<Rigidbody2D>(), rb2 = dustSpecks[j].GetComponent<Rigidbody2D>();
				float distance = Vector2.Distance(rb1.position, rb2.position);

				//if (distance > maxGravityDistance)
				//{
				//	continue;
				//}

				Vector2 direction = (rb2.position - rb1.position).normalized;
				float fg = gravitationalConstant * (rb1.mass * rb2.mass) / Mathf.Clamp(distance * distance, 1e-3f, float.MaxValue);
				rb1.AddForce(direction * fg);
				rb2.AddForce(direction * -fg);
			}
		}

		// Clicking/tapping will add specks to the simulation, up to a certain amount (performance reasons)
		if (dustSpecks.Count < maxSpecks)
		{
			if (Input.mousePresent && Input.GetMouseButton(0))
			{
				Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

				if (!speckSpawnBounds.Contains(mousePos))
				{
					return;
				}

				float offsetX = Random.Range(-speckSpawnRadius, speckSpawnRadius);
				float offsetY = Random.Range(-speckSpawnRadius, speckSpawnRadius);
				Vector2 spawnPos = new Vector2(mousePos.x + offsetX, mousePos.y + offsetY);

				CreateDust(spawnPos);
			}
			else if (Input.touchSupported && Input.touchCount > 0)
			{
				foreach (Touch touch in Input.touches)
				{
					Vector2 touchPos = Camera.main.ScreenToWorldPoint(touch.position);

					if (!speckSpawnBounds.Contains(touchPos))
					{
						continue;
					}

					float offsetX = Random.Range(-speckSpawnRadius, speckSpawnRadius);
					float offsetY = Random.Range(-speckSpawnRadius, speckSpawnRadius);
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

		if (Input.mousePresent)
		{
			if (Mathf.Abs(Input.GetAxis("Mouse ScrollWheel")) > 1e-6)
			{
				float newSpawnRadius = Mathf.Clamp(speckSpawnRadius + Input.GetAxis("Mouse ScrollWheel"), minSpawnRadius, maxSpawnRadius);
				speckSpawnRadius = newSpawnRadius;
			}

			UpdateSpawnVisualizer();
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

	private void UpdateSpawnVisualizer()
	{
		float angleIncrement = 2f * Mathf.PI / spawnVisualizerVertices;

		Vector2 offset = Vector3.zero;
		if (Input.mousePresent)
		{
			offset = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		}

		for (int i = 0; i < spawnVisualizerVertices + 1; i++)
		{
			radiusRenderer.SetPosition(i, new Vector3(Mathf.Cos(angleIncrement * i) * speckSpawnRadius + offset.x, Mathf.Sin(angleIncrement * i) * speckSpawnRadius + offset.y, 0f));
		}
	}

	private void CreateDust(Vector2 pos)
	{
		SpeckScript newDust = Instantiate(dustPrefab, pos, Quaternion.identity, GameObject.Find("Specks").transform).GetComponent<SpeckScript>();

		if (startWithVelocity)
		{
			Vector2 direction = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
			newDust.GetComponent<Rigidbody2D>().velocity = direction * Random.Range(-maxVelocity, maxVelocity);
		}

		targetGroup.AddMember(newDust.transform, 1, 5);
		dustSpecks.Add(newDust);
	}
}
