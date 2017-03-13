using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(Animator))]
public class Block : MonoBehaviour
{

	public MapArea m_area;

	private Animator m_animator;
	private string m_areaAsString;

	void Start ()
	{
		m_animator = this.GetComponent<Animator> () as Animator;
		m_areaAsString = m_area.ToString ();
	}

	public void UpdateState (MapArea m)
	{
		string n = m.ToString ();

		//Check that we are matching the state of the current mapMarker
		if (n == m_areaAsString) {
			//If so, then check that we are not already animating, make sure the states in the Animator have a tag that matches the MapArea enum...
			if (!m_animator.GetCurrentAnimatorStateInfo (0).IsTag (n))
				m_animator.SetTrigger (n);
		} else if (!m_animator.GetCurrentAnimatorStateInfo (0).IsTag ("Default")) {
			m_animator.SetTrigger ("Default");
		}
	}

}

[System.Serializable]
public enum MapArea
{
	Sapphire,
	Emerald,
	Ruby,
	Amber
}
