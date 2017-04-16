using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;
using TMPro;

[RequireComponent (typeof(Interactable))]
public class InteractableMarker : MonoBehaviour
{
	public Marker m_markerInfo;
	public Animator m_label;
	public Animator m_marker;
	public TextMeshPro m_text;
	public LineRenderer m_line;

	private Hand.AttachmentFlags attachmentFlags = Hand.defaultAttachmentFlags & (~Hand.AttachmentFlags.SnapOnAttach) & (~Hand.AttachmentFlags.DetachOthers);
	private float m_wantedZPosition;
	private float m_startZPosition;
	private bool m_isSetUp = false;
	private Vector3 m_startPos;

	void Start ()
	{
		Invoke ("SetupNames", 1);

	}

	void OnEnable ()
	{
		if (m_isSetUp) {
			StartCoroutine (MoveToPosition ());
		}
	}

	void OnDisable ()
	{
		StopAllCoroutines ();
		//transform.position = new Vector3 (transform.localPosition.x, transform.localPosition.y, m_startZPosition);
	}

	IEnumerator MoveToPosition ()
	{
		float currentZPosition = transform.localPosition.z;
		while (m_wantedZPosition - currentZPosition > .01f) {
			
			currentZPosition = Mathf.Lerp (currentZPosition, m_wantedZPosition, Time.deltaTime * 1f);
			transform.localPosition = new Vector3 (transform.localPosition.x, transform.localPosition.y, currentZPosition);
			if (m_line) {
				m_line.SetPosition (1, new Vector3 (0, 0, -currentZPosition));
			}
				
			yield return null;
		}
	}

	void SetupNames ()
	{
		m_isSetUp = true;
		m_startZPosition = transform.localPosition.z;

		if (m_text != null && m_markerInfo != null) {
			m_text.text = m_markerInfo.m_info.m_buildingName;
		} 
		if (m_markerInfo != null)
			m_wantedZPosition = m_markerInfo.transform.position.y * .005f;

	}


	//-------------------------------------------------
	// Called when a Hand starts hovering over this object
	//-------------------------------------------------
	private void OnHandHoverBegin (Hand hand)
	{
		//Debug.Log ("Hovering hand: " + hand.name + " at " + Time.time);
		if (m_label)
			m_label.SetTrigger ("Open");
		if (m_marker)
			m_marker.SetTrigger ("Select");
	}


	//-------------------------------------------------
	// Called when a Hand stops hovering over this object
	//-------------------------------------------------
	private void OnHandHoverEnd (Hand hand)
	{
		//Debug.Log ("No Hand Hovering" + hand.name + " at " + Time.time);
		if (m_label)
			m_label.SetTrigger ("Close");
		if (m_marker)
			m_marker.SetTrigger ("Deselect");
	}


	//-------------------------------------------------
	// Called every Update() while a Hand is hovering over this object
	//-------------------------------------------------
	private void HandHoverUpdate (Hand hand)
	{
		if (hand.GetStandardInteractionButtonDown () || ((hand.controller != null) && hand.controller.GetPressDown (Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger))) {
			if (hand.currentAttachedObject != gameObject) {

				// Call this to continue receiving HandHoverUpdate messages,
				// and prevent the hand from hovering over anything else
				hand.HoverLock (GetComponent<Interactable> ());

				if (m_markerInfo != null) {
					Player.instance.transform.position = m_markerInfo.transform.position;
				}

				// Attach this object to the hand
				hand.AttachObject (gameObject, attachmentFlags);
			} else {
				// Detach this object from the hand
				hand.DetachObject (gameObject);

				// Call this to undo HoverLock
				hand.HoverUnlock (GetComponent<Interactable> ());

			}
		}
	}


	//-------------------------------------------------
	// Called when this GameObject becomes attached to the hand
	//-------------------------------------------------
	private void OnAttachedToHand (Hand hand)
	{
		Debug.Log (gameObject.name + " is attached at " + Time.time);
	}


	//-------------------------------------------------
	// Called when this GameObject is detached from the hand
	//-------------------------------------------------
	private void OnDetachedFromHand (Hand hand)
	{
		Debug.Log (gameObject.name + " is detached at " + Time.time);
	}


	//-------------------------------------------------
	// Called every Update() while this GameObject is attached to the hand
	//-------------------------------------------------
	private void HandAttachedUpdate (Hand hand)
	{

	}


	//-------------------------------------------------
	// Called when this attached GameObject becomes the primary attached object
	//-------------------------------------------------
	private void OnHandFocusAcquired (Hand hand)
	{
		Debug.Log (gameObject.name + " is focused at " + Time.time);
	}


	//-------------------------------------------------
	// Called when another attached GameObject becomes the primary attached object
	//-------------------------------------------------
	private void OnHandFocusLost (Hand hand)
	{
		Debug.Log (gameObject.name + " lost focus at " + Time.time);
	}
}
