using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeckScript : MonoBehaviour
{
	public Color colour;

	private SpriteRenderer sprR;

	// Setup colour for the new speck
	private void Start()
	{
		sprR = GetComponent<SpriteRenderer>();
		colour = Color.HSVToRGB(Random.Range(0f, 1f), 0.15f, 0.9f);
		sprR.color = colour;
		Color hdrColour = new Color(colour.r * 3, colour.g * 3, colour.b * 3);
		Material newMat = sprR.material;
		newMat.SetColor("_TintColor", hdrColour);
		sprR.material = newMat;
	}
}
