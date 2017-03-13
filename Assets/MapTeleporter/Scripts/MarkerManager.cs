using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MarkerManager : MonoBehaviour
{
	//Static instance of GameManager which allows it to be accessed by any other script.
	public static MarkerManager instance = null;

    public GameObject m_markersGroup;
	public Renderer m_worldArea;
	public SpriteRenderer m_spriteMap;
	public MeshFilter m_meshFilterMap;
	public GameObject m_mapMarkerPrefab;
	public Vector3 m_offsetBy = Vector3.zero;
	public float m_heightMultiplier = .2f;
	public bool m_flipY = false;

	[HideInInspector]
	public Dictionary<int, Marker> m_worldMarkers;
	[HideInInspector]
	public Animator[] m_mapMarkers;
	float m_markerBoundsHeight;

	public bool m_isReady { get; private set; }

	void Awake ()
	{
		//Make sure this is in fact the only instance (Singleton pattern)
		if (instance == null)
			instance = this;
		else if (instance != this)
			Destroy (gameObject);    
            
		//Sets this to not be destroyed when reloading scene
		DontDestroyOnLoad (gameObject);

		CalculateLocalBounds ();

	}

	void Start ()
	{
		GetMarkersAndNormalizedPositions ();

		if (m_spriteMap != null)
			PlaceMarkersOnSprite (m_spriteMap);

		if (m_meshFilterMap != null)
			PlaceMarkersOnMesh (m_meshFilterMap);

		
	}

	void GetMarkersAndNormalizedPositions ()
	{
		Marker[] temp = m_markersGroup.GetComponentsInChildren<Marker> ();
		m_worldMarkers = new Dictionary<int, Marker> ();

		for (int i = 0; i < temp.Length; i++) {
			Marker m = temp [i];
			m.m_telePortTo = m.gameObject;
			m.transform.name = "TelePorter_" + i.ToString ();

			//Get the coordinates as a percent of the world size plane (normalize) when laying flat. This may require that the world map be at position 0,0,0 for it to work right
			float xPos = Mathf.InverseLerp (0, m_worldArea.bounds.size.x, m.transform.position.x + m_worldArea.bounds.extents.x);
			float yPos = Mathf.InverseLerp (0, m_worldArea.bounds.size.z, m.transform.position.z + m_worldArea.bounds.extents.z);
			float zPos = Mathf.InverseLerp (0, m_markerBoundsHeight, m.transform.position.y);

			if (m_flipY)
				zPos = -zPos;

			m.m_normalizedPosition = new Vector3 (xPos, yPos, zPos);
			m_worldMarkers.Add (i, m);

		}
	}

	void PlaceMarkersOnSprite (SpriteRenderer sprite)
	{
		m_mapMarkers = new Animator[m_worldMarkers.Count];

		//make a parent for the map in case the map is scaled non-uniformly
		GameObject parent = new GameObject ("Map and Markers");
		parent.transform.position = sprite.transform.position;
		parent.transform.rotation = sprite.transform.rotation;
		sprite.gameObject.transform.SetParent (parent.transform);

		GameObject marker = new GameObject ("Markers");
		marker.transform.SetParent (parent.transform);
		marker.transform.localPosition = Vector3.zero;
		marker.transform.localEulerAngles = Vector3.zero;

		for (int i = 0; i < m_worldMarkers.Count; i++) {

			float xPos = Mathf.Lerp (-sprite.bounds.extents.x, sprite.bounds.extents.x, m_worldMarkers [i].m_normalizedPosition.x);
			float yPos = Mathf.Lerp (-sprite.bounds.extents.y, sprite.bounds.extents.y, m_worldMarkers [i].m_normalizedPosition.y);
			float zPos = Mathf.Lerp (-sprite.bounds.extents.z, sprite.bounds.extents.z, m_worldMarkers [i].m_normalizedPosition.z);

			GameObject go = Instantiate (m_mapMarkerPrefab, marker.transform);
			go.transform.localPosition = new Vector3 (xPos, yPos, zPos);
			go.transform.localPosition += m_offsetBy;

			m_mapMarkers [i] = go.GetComponentInChildren<Animator> () as Animator;
		}

		m_isReady = true;
	}

	void PlaceMarkersOnMesh (MeshFilter mf)
	{

		m_mapMarkers = new Animator[m_worldMarkers.Count];

		GameObject marker = new GameObject ("Markers");

		for (int i = 0; i < m_worldMarkers.Count; i++) {
			
			GameObject go = Instantiate (m_mapMarkerPrefab, marker.transform);
			go.transform.position = UvTo3D (new Vector2 (m_worldMarkers [i].m_normalizedPosition.x, m_worldMarkers [i].m_normalizedPosition.y), mf);
			go.transform.localPosition += m_offsetBy;
			go.transform.Translate (new Vector3 (0, 0, m_worldMarkers [i].m_normalizedPosition.z * m_heightMultiplier));
			m_mapMarkers [i] = go.GetComponentInChildren<Animator> () as Animator;
		}
		//if the mesh was deformed, we need to now move the group into the root
		marker.transform.SetParent (mf.transform.root.transform);
		marker.transform.localPosition = Vector3.zero;
		marker.transform.localEulerAngles = Vector3.zero;

		m_isReady = true;

	}

	Vector3 UvTo3D (Vector2 uv, MeshFilter meshFilter)
	{


		int[] tris = meshFilter.mesh.triangles;
		Vector2[] uvs = meshFilter.mesh.uv;
		Vector3[] verts = meshFilter.mesh.vertices;

		for (int i = 0; i < tris.Length; i += 3) {
			Vector2 u1 = uvs [tris [i]]; // get the triangle UVs
			Vector2 u2 = uvs [tris [i + 1]];
			Vector2 u3 = uvs [tris [i + 2]];
			// calculate triangle area - if zero, skip it
			float a = Area (u1, u2, u3);
			if (a == 0)
				continue;
			// calculate barycentric coordinates of u1, u2 and u3
			// if anyone is negative, point is outside the triangle: skip it
			float a1 = Area (u2, u3, uv) / a;
			if (a1 < 0)
				continue;
			float a2 = Area (u3, u1, uv) / a;
			if (a2 < 0)
				continue;
			float a3 = Area (u1, u2, uv) / a;
			if (a3 < 0)
				continue;
			// point inside the triangle - find mesh position by interpolation...
			Vector3 p3D = a1 * verts [tris [i]] + a2 * verts [tris [i + 1]] + a3 * verts [tris [i + 2]];

			//correct for scaled mesh
			p3D = new Vector3 (p3D.x * meshFilter.transform.localScale.x, p3D.y * meshFilter.transform.localScale.y, p3D.z * meshFilter.transform.localScale.z);
			// and return it in world coordinates:
			return transform.TransformPoint (p3D);

		}
		// point outside any uv triangle: return Vector3.zero
		return Vector3.zero;

	}
 
	// calculate signed triangle area using a kind of "2D cross product":
	float Area (Vector2 p1, Vector2 p2, Vector2 p3)
	{
		Vector2 v1 = p1 - p3;
		Vector2 v2 = p2 - p3;
		return (v1.x * v2.y - v1.y * v2.x) / 2;
	}

	private void CalculateLocalBounds ()
	{
		Quaternion currentRotation = this.transform.rotation;
		this.transform.rotation = Quaternion.Euler (0f, 0f, 0f);
		Bounds bounds = new Bounds (this.transform.position, Vector3.zero);
		foreach (Renderer renderer in GetComponentsInChildren<Renderer>()) {
			bounds.Encapsulate (renderer.bounds);
		}
		Vector3 localCenter = bounds.center - this.transform.position;
		bounds.center = localCenter;

		m_markerBoundsHeight = bounds.extents.y * 2;

//		Debug.Log ("The local bounds of this model is " + bounds);
		this.transform.rotation = currentRotation;


	}
}


