﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR;

using Valve.VR;

[RequireComponent (typeof(AudioSource))]
public class MapInputHandler : MonoBehaviour
{
	public GameObject m_player;
	public GameObject m_leftHand;
	public Animator m_map;
	public Vector3 m_mapPositionOffset = Vector3.one;
	public AudioClip m_selected;
	public AudioClip m_clicked;
	public AudioClip m_openMap;
	public AudioClip m_closeMap;
	public Block[] m_blocks;
	public LabelMaker m_buildingLabel;

	public int _currentNumber { get; private set; }

	public int _lastNumber  { get; private set; }

	private Vector3 m_startPosition;
	private Vector3 m_startRotation;
	private AudioSource m_audioSource;
	private bool m_isShowingMap = false;
//	private bool m_fadingUp = false;

	SteamVR_Controller.Device device;
//	SteamVR_TrackedObject trackedobj;

	Vector2 touchpad;
	Transform cam;

	void Start ()
	{
		m_audioSource = this.GetComponent<AudioSource> () as AudioSource;
		cam = Camera.main.transform;
        m_startPosition = m_player.transform.position;
    }

	void Update ()
	{

		if (!MarkerManager.instance.m_isReady)
			return;
		HandleScrolling ();
		if (Input.GetKeyDown (KeyCode.Space)) {
			if (m_map == null)
				return;

			if (!m_isShowingMap) {
				
				m_map.transform.position = cam.position;
				m_map.transform.parent = cam;
				m_map.transform.localPosition = Vector3.zero + m_mapPositionOffset;
				m_map.transform.localEulerAngles = Vector3.zero;
				m_map.transform.Rotate (new Vector3 (0, 180, 0));
				m_map.transform.parent = null;
				m_map.SetTrigger ("Open");
				m_isShowingMap = true;

				if (m_openMap) {
					m_audioSource.clip = m_openMap;
					m_audioSource.Play ();
				}

			} else {
				m_map.SetTrigger ("Close");
				m_isShowingMap = false;

				if (m_closeMap) {
					m_audioSource.clip = m_closeMap;
					m_audioSource.Play ();
				}
			}
		}

		if (Input.GetKeyDown (KeyCode.Escape) && m_player != null) {
            Application.Quit();

		}

        if (Input.GetButtonDown("Fire1"))
        {
            if (m_map == null)
                return;

            if (!m_isShowingMap)
            {

				ShowMap ("Open", true, m_openMap);

            }
            else
            {
				ShowMap ("Close", false, m_closeMap);
            }
        }

		if (m_isShowingMap && Input.GetButtonUp ("Fire2")) {

			ShowMap("Close", false, m_closeMap);
		}

        if (Input.GetKeyDown (KeyCode.B) && m_player != null) {
            m_player.transform.position = m_startPosition;
		}

	}

	void ShowMap(string message, bool toShow, AudioClip soundEffect)
	{
		m_map.transform.position = cam.position + -(Vector3.forward*2) ;
		m_map.SetTrigger(message);
		m_isShowingMap = toShow;
		m_audioSource.clip = soundEffect;
		m_audioSource.Play();
	}

	void HandleScrolling ()
	{


		float f = Input.GetAxisRaw ("Mouse ScrollWheel");
		if (f > 0) {
			_currentNumber++;
			if (_currentNumber >= MarkerManager.instance.m_mapMarkers.Length)
				_currentNumber = MarkerManager.instance.m_mapMarkers.Length - 1;
		} else if (f < 0) {
			_currentNumber--;
			if (_currentNumber < 0) {
				_currentNumber = 0;
			}
		}

		if (_currentNumber != _lastNumber) {
			MarkerManager.instance.m_mapMarkers [_currentNumber].SetTrigger ("Select");
			MarkerManager.instance.m_mapMarkers [_lastNumber].SetTrigger ("Deselect");
			_lastNumber = _currentNumber;
		} 

	}

	void HandleTouchpad ()
	{

		float f = 0;

		if (device.GetPress (SteamVR_Controller.ButtonMask.Touchpad)) {
			f = device.GetAxis (EVRButtonId.k_EButton_SteamVR_Touchpad).x;
		}

		if (f > 0) {
			_currentNumber++;
			if (_currentNumber >= MarkerManager.instance.m_mapMarkers.Length)
				_currentNumber = MarkerManager.instance.m_mapMarkers.Length - 1;
		} else if (f < 0) {
			_currentNumber--;
			if (_currentNumber < 0) {
				_currentNumber = 0;
			}
		}

		if (_currentNumber != _lastNumber) {

			Marker selectedWorldMarker = MarkerManager.instance.m_worldMarkers [_currentNumber];

			foreach (Block b in m_blocks)
				b.UpdateState (selectedWorldMarker.m_area);//Do the rollover effect if we are on top of that block
		}
			
		_lastNumber = _currentNumber;
	}

}
