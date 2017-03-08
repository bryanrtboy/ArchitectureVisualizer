using UnityEngine;
using System.Collections;

public class TakeScreenShot : MonoBehaviour
{


	public string		m_hotKey = "s";
	public GameObject[] m_thingsToHide;
	public int			m_resolutionMultiplier = 3;
	public string		m_savePrefix = "Screenshot";
	public int			m_maxNumberOfPics = 10;


	private bool m_isPaused = false;
	private int m_screenCount = 0;
	
	void Awake ()
	{
		m_screenCount = PlayerPrefs.GetInt ("Count");
		
		if (m_screenCount > m_maxNumberOfPics)
			m_screenCount = 0;
	}
	
	void Update ()
	{
		if (!m_isPaused && Input.GetKeyDown (m_hotKey)) {
			StartCoroutine (TakeAScreenShot ());
		}
		
		
	}


	IEnumerator TakeAScreenShot ()
	{	
		m_isPaused = true;
		
		m_screenCount = PlayerPrefs.GetInt ("Count");
		
		if (m_screenCount > m_maxNumberOfPics)
			m_screenCount = 0;
		
		
		if (m_resolutionMultiplier >= 6)
			m_resolutionMultiplier = 6;
		
		m_savePrefix = "_" + Application.loadedLevelName + "_" + m_screenCount.ToString () + ".png";
		
		Debug.Log ("Taking a Screenshot, " + m_savePrefix + " at " + m_resolutionMultiplier + "X resolution!");
		yield return new WaitForEndOfFrame ();
		Application.CaptureScreenshot (m_savePrefix, m_resolutionMultiplier);
		m_screenCount++;
		m_isPaused = false;
		
		PlayerPrefs.SetInt ("Count", m_screenCount);
	}
}
