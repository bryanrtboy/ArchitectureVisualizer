//Bryan Leister, March 2017
//Portal script
//
//This script uses the Turbulent Library noise from https://github.com/jesta88/Turbulence-Library  to generate a portal
//texture effect. As the player approaches the portal, it fades away and the collider is disabled, if there is one.
//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(Renderer))]
public class FadeOnApproach : MonoBehaviour
{

	public bool m_isActive = true;
    public GameObject m_target;
	public float m_beginFadeAt = 2;
	public float m_fadedOutCompletelyAt = 1;
	public string m_materialFadeProperty = "_Transparency";
	public bool m_useAlphaColor = false;
	//if the shader does not have a _Transparency option, it's best to use the color's alpha value instead.

	private Collider m_collider;
	private GameObject m_player;
	private Renderer m_renderer;

	void Start ()
	{
		m_player = GameObject.FindGameObjectWithTag ("MainCamera");

		if (m_player == null)
			Debug.LogError ("No player found, " + this.name + " will not work. Have you tagged the Player with the Player tag?");

        m_collider = this.GetComponent<Collider>() as Collider;

        m_renderer = this.GetComponent<Renderer> () as Renderer;

        if (m_target == null) 
            m_target = new GameObject("target");



	}

	void Update ()
	{
		if (!m_isActive || m_player == null)
			return;

		float distanceToPlayer = Vector3.Distance (m_target.transform.position, m_player.transform.position);

 //       Debug.Log("distance is" + distanceToPlayer);

		float transparency = Mathf.InverseLerp (m_fadedOutCompletelyAt, m_beginFadeAt, distanceToPlayer);

		if (transparency == 1)
			return;

        if (m_collider != null)
        {
            if (m_collider.enabled && transparency < .1f)
                m_collider.enabled = false;
            else if (!m_collider.enabled && transparency > .1f)
                m_collider.enabled = true;
        }

        if (!m_useAlphaColor) {
			m_renderer.material.SetFloat (m_materialFadeProperty, transparency);
		} else {
			Color c = m_renderer.material.GetColor (m_materialFadeProperty);
			c.a = transparency;
			m_renderer.material.SetColor (m_materialFadeProperty, c);
		}
	}

}
