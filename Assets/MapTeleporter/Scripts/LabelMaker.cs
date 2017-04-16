using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LabelMaker : MonoBehaviour
{
	
	public TextMeshPro m_text;
	public Animator m_animator;
	private bool hasALabel = true;


	void Start ()
	{
		Invoke ("SetupNames", 2);
	}

	void SetupNames ()
	{

		if (m_text != null && hasALabel) {
			//m_text.text = m_teleporter.goToMarker.m_info.m_buildingName;
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
