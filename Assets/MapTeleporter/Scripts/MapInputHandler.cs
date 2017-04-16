using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR;

using Valve.VR;
using Valve.VR.InteractionSystem;

[RequireComponent (typeof(AudioSource))]
public class MapInputHandler : MonoBehaviour
{
	public Animator m_map;
	public GameObject m_placeHolder;
	public AudioClip m_selected;
	public AudioClip m_clicked;
	public AudioClip m_openMap;
	public AudioClip m_closeMap;

	private Vector3 m_startPosition;
	private Vector3 m_startRotation;
	private AudioSource m_audioSource;
	private bool m_isShowingMap = false;
	private bool m_isPlacingMap = false;

	void Start ()
	{
		m_audioSource = this.GetComponent<AudioSource> () as AudioSource;
		m_startPosition = Player.instance.transform.position;
	}

	void Update ()
	{

		if (!MarkerManager.instance.m_isReady)
			return;

		if (m_isPlacingMap && !m_placeHolder.activeSelf)
			m_placeHolder.SetActive (true);
		
		if (!m_isPlacingMap && m_placeHolder.activeSelf)
			m_placeHolder.SetActive (false);

		if (Input.GetKeyDown (KeyCode.Escape)) {
			Application.Quit ();

		}

		if (Player.instance.rightHand.controller != null && Player.instance.rightHand.controller.GetPressDown (Valve.VR.EVRButtonId.k_EButton_Grip)) {
			if (!m_isPlacingMap) {
				m_isPlacingMap = true;
			}

			if (m_isShowingMap) {
				m_isPlacingMap = false;
				return;
			}	
			
			if (m_placeHolder.transform.parent != Player.instance.rightHand.transform) {
				m_placeHolder.transform.SetParent (Player.instance.rightHand.transform);
				m_placeHolder.transform.localPosition = Vector3.zero;
				m_placeHolder.transform.localEulerAngles = new Vector3 (0, 180, 0);
				m_placeHolder.transform.Translate (-Vector3.forward);
			}
		}

		if (Player.instance.rightHand.controller != null && Player.instance.rightHand.controller.GetPressUp (Valve.VR.EVRButtonId.k_EButton_Grip)) {
			if (m_map == null)
				return;

			if (!m_isShowingMap) {
				ShowMap ("Open", true, m_openMap);
			} else {
				ShowMap ("Close", false, m_closeMap);
			}
		}
			
		if (m_isShowingMap && Input.GetButtonUp ("Fire2")) {

			ShowMap ("Close", false, m_closeMap);
		}

		if (Input.GetKeyDown (KeyCode.B)) {
			Player.instance.transform.position = m_startPosition;
		}

	}

	void ShowMap (string message, bool toShow, AudioClip soundEffect)
	{
		if (toShow) {
			m_placeHolder.transform.parent = null;
			m_map.transform.position = m_placeHolder.transform.position;
			m_map.transform.rotation = m_placeHolder.transform.rotation;
			m_map.transform.Translate (Vector3.forward * .1f);
		} else {
			m_isPlacingMap = false;
		}

		m_map.SetTrigger (message);
		m_isShowingMap = toShow;
		m_audioSource.clip = soundEffect;
		m_audioSource.Play ();
	}

}
