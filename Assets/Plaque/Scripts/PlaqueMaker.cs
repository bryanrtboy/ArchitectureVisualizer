using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlaqueMaker : MonoBehaviour
{

	public TextMeshPro m_architectName;
	public TextMeshPro m_buildingName;
	public TextMeshPro m_quote;
	public SpriteRenderer m_colorBar;
	public SpriteRenderer m_icon;

    private Marker m_marker;

	void Start ()
	{
        Invoke("GetBuildingInfo", Random.Range(.1f, 2f));//Stagger the building of plaques so it is not so heavy on start
	}

    void GetBuildingInfo()
    {

        m_marker = GetComponentInParent<Marker>() as Marker;

        MakePlaques();
    }

    void MakePlaques()
    {

       BuildingInfo m_info = m_marker.m_info;

        if (m_architectName)
            m_architectName.text = m_info.m_architect;
        if (m_buildingName)
            m_buildingName.text = m_info.m_buildingName;
        if (m_quote)
            m_quote.text = '\u201c' + m_info.m_quote + '\u201d';
        if (m_colorBar)
            m_colorBar.color = m_info.m_color;
        if (m_icon && m_info.m_sprite)
            m_icon.sprite = m_info.m_sprite;
    }
}
