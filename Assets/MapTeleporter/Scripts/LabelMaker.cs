using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

using Valve.VR.InteractionSystem;

[RequireComponent (typeof(TeleportPoint))]
public class LabelMaker : MonoBehaviour
{
	
	public TextMeshPro m_text;
	public Animator m_animator;

	private TeleportPoint m_teleporter;
	private bool hasALabel = true;


    void Start()
    {
        Invoke("SetupNames", 2);
    }

	void SetupNames ()
	{
		m_teleporter = this.GetComponent<TeleportPoint> () as TeleportPoint;

		if (m_teleporter == null || m_teleporter.goToMarker.m_buildingId == Marker.id.Zero) {
			Debug.LogWarning ("No markers for you, " + this.transform.parent.transform.name + "!");
			hasALabel = false;
		}

		if (m_text != null && hasALabel) {
			m_text.text = m_teleporter.goToMarker.m_info.m_buildingName;
         //   Debug.Log("updating building name to " + m_teleporter.goToMarker.m_info.m_buildingName);
            m_teleporter.UpdateVisuals();
		} 
	}

	void OnEnable ()
	{
		if (hasALabel)
			ShowLabel ();
	}

	public void ShowLabel ()
	{
//		if (m_animator.GetCurrentAnimatorStateInfo (0).IsTag ("off"))
		m_animator.SetTrigger ("Open");
	}

	public void HideLabel ()
	{
		m_animator.SetTrigger ("Close");
	}
}
