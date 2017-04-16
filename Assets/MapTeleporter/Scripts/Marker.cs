using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Marker : MonoBehaviour
{


	public enum id
	{
		Zero,
		One,
		Two,
		Three,
		Four,
		Five,
		Six,
		Seven,
		Eight,
		Nine,
		Ten,
		Eleven,
		Twelve,
		Thirteen,
		Fourteen,
		Fifteen,
		Sixteen,
		Seventeen,
		Eighteen,
		Nineteen,
		Twenty
	}

	public id m_buildingId = id.Zero;
	public MapArea m_area = MapArea.Amber;
	public bool m_isPortal = false;
	public Vector3 m_normalizedPosition;
	public BuildingInfo m_info;

	void Start ()
	{
		//If we are not a building, don't get any building data...
		if (m_buildingId == id.Zero)
			return;

	

		if (DataReaderMap.instance == null) {
			Debug.LogError ("No database found, not going to import");
			Destroy (this);
		} else {
			m_info = DataReaderMap.instance.m_data [(int)m_buildingId];
		}
	}

	public Marker GetMarker ()
	{
		return this;
	}


}
