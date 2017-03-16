using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarkerLineEffect : MonoBehaviour
{
	public LineRenderer line;

	float f = 0;
	float rand = 1;
	// Use this for initialization
	void Start ()
	{
		if (line == null) {
			Debug.LogWarning ("No lines for you, " + this.name + "!");
			Destroy (this);
		}

		line.useWorldSpace = false;

		rand = Random.Range (.1f, 2f);
	}


	// Update is called once per frame
	void Update ()
	{
		line.SetPosition (1, new Vector3 (0, 0, -transform.localPosition.z));

		line.material.mainTextureOffset += new Vector2 (Time.deltaTime * (transform.localPosition.z * 4f), 0);
		//line.material.mainTexture = new Vector2 (f + transform.localPosition.z, 0);
	}
}
