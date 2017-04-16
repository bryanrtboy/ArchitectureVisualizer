//Bryan Leister, March 2017
//
//A script to create map markers, taking markers from a larger world, making a copy of them and placing them on a small
//map. Mostly tested on a curved map. For reasons, not known, this script needs to be on a game object with a position
//of 0,0,0 or the markers will be offset.

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
	public MeshFilter m_meshFilterMap;
	public GameObject m_mapMarkerPrefab;
	public Vector3 m_offsetBy = Vector3.zero;
	public float m_heightMultiplier = .2f;
	public Dictionary<int, Marker> m_worldMarkers;

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

		GetMarkersAndNormalizedPositions ();
		PlaceMarkersOnMesh (m_meshFilterMap);	
	}

	void GetMarkersAndNormalizedPositions ()
	{
		//Store the positions and rotations so we can zero out the object for calculations
		Vector3 tempPos = m_markersGroup.transform.position;
		Quaternion tempRot = m_markersGroup.transform.rotation;

		m_markersGroup.transform.position = Vector3.zero;
		m_markersGroup.transform.rotation = Quaternion.identity;


		Marker[] temp = m_markersGroup.GetComponentsInChildren<Marker> ();
		m_worldMarkers = new Dictionary<int, Marker> ();

		for (int i = 0; i < temp.Length; i++) {
			Marker m = temp [i];
			m.transform.name = "TelePorter_" + i.ToString ();

			//Get the coordinates as a percent of the world size plane (normalize) when laying flat. 
			//This may require that the world map be at position 0,0,0 for it to work right
			float xPos = Mathf.InverseLerp (0, m_worldArea.bounds.size.x, temp [i].transform.position.x + m_worldArea.bounds.extents.x);
			float yPos = Mathf.InverseLerp (0, m_worldArea.bounds.size.z, temp [i].transform.position.z + m_worldArea.bounds.extents.z);

			m.m_normalizedPosition = new Vector3 (xPos, yPos, 0);
			m_worldMarkers.Add (i, m);
		}

		m_markersGroup.transform.position = tempPos;
		m_markersGroup.transform.rotation = tempRot;
	}

	void PlaceMarkersOnMesh (MeshFilter mf)
	{
		if (mf.transform.parent != null) {
			mf.transform.parent.position = Vector3.zero;
			mf.transform.parent.rotation = Quaternion.identity;
		}
		
		GameObject marker = new GameObject ("Marker Parent");
		marker.transform.SetParent (mf.transform);

		for (int i = 0; i < m_worldMarkers.Count; i++) {
			GameObject go = Instantiate (m_mapMarkerPrefab, marker.transform);
			go.transform.position = UvTo3D (new Vector2 (m_worldMarkers [i].m_normalizedPosition.x, m_worldMarkers [i].m_normalizedPosition.y), mf);
			InteractableMarker markerScript = go.GetComponent<InteractableMarker> () as InteractableMarker;
			if (markerScript)
				markerScript.m_markerInfo = m_worldMarkers [i];
		}

		marker.transform.position += m_offsetBy;
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
			//p3D = new Vector3 (p3D.x * meshFilter.transform.localScale.x, p3D.y * meshFilter.transform.localScale.y, p3D.z * meshFilter.transform.localScale.z);
			p3D = new Vector3 (-p3D.x, -p3D.y, p3D.z);
			// and return it in world coordinates:
			return meshFilter.transform.TransformPoint (p3D);

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


}


