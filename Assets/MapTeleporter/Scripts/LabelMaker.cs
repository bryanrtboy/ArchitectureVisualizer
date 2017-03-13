using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent (typeof(Animator))]
public class LabelMaker : MonoBehaviour
{
	public TextMeshPro m_text;
	public Animator m_animator;



	void Start ()
	{
		m_animator = this.GetComponent<Animator> () as Animator;
		m_text.transform.parent.gameObject.SetActive (false);
	}


	public void UpdateLabel (BuildingInfo b)
	{
		m_text.text = b.m_buildingName;
		m_animator.SetTrigger ("Open");
	}

	public void TurnOff ()
	{
		if (m_animator.GetCurrentAnimatorStateInfo (0).IsTag ("on"))
			m_animator.SetTrigger ("Close");
	}
}
