using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using VTL.IO;

public class DataReaderMap : MonoBehaviour
{

    public static DataReaderMap instance = null;
    // specify the location of your csv file relative to a Resources folder
    // leave off the extension
    public string resourceLocation = "data";

    //[HideInInspector]
    public Dictionary<int, BuildingInfo> m_data;

    void Awake ()
	{


		//Make sure this is in fact the only instance (Singleton pattern)
		if (instance == null)
			instance = this;
		else if (instance != this)

            Destroy(gameObject);

        //Sets this to not be destroyed when reloading scene
        DontDestroyOnLoad(gameObject);

		DictReader dictReader = new DictReader (resourceLocation);
		m_data = new Dictionary<int, BuildingInfo> ();

		foreach (var row in dictReader) {
			Debug.Log (row ["ID"] + ", " +
			row ["IMAGE"] + ", " +
			row ["ARCHITECT"] + ", " +
			row ["BUILDING"] + ", " +
			row ["HEX"] + ", " +
			row ["RGB"] + ", " +
			row ["QUOTE"] + ", " +
			row ["BLOCK"]
			);

			BuildingInfo info = new BuildingInfo ();
			info.m_image = row ["IMAGE"];
			info.m_architect = row ["ARCHITECT"];
			info.m_buildingName = row ["BUILDING"];
			Color c = new Color ();
			ColorUtility.TryParseHtmlString ("#" + row ["HEX"], out c);
			info.m_color = c;
			//Debug.Log (c.ToString ());
			info.m_quote = row ["QUOTE"];
			info.m_block = row ["BLOCK"];
			info.m_sprite = Resources.Load<Sprite> ("BuildingSprites/" + info.m_image);

			m_data.Add (Convert.ToInt32 (row ["ID"]), info);
		}


		Debug.Log (dictReader);
	}


}

[System.Serializable]
public class BuildingInfo
{
	public string m_image;
	public string m_buildingName;
	public string m_architect;
	public Color m_color;
	public string m_quote;
	public string m_block;
	public Sprite m_sprite;
}
