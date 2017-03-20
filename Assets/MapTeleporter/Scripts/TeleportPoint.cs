//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Single location that the player can teleport to
//
//=============================================================================

using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Valve.VR.InteractionSystem
{
	//-------------------------------------------------------------------------
	public class TeleportPoint : TeleportMarkerBase
	{
		public enum TeleportPointType
		{
			MoveToLocation,
			SwitchToNewScene}

		;

		//Public variables
		public TeleportPointType teleportType = TeleportPointType.MoveToLocation;
		public string title;
		public string switchToScene;
		public Color titleVisibleColor;
		public Color titleHighlightedColor;
		public Color titleLockedColor;
		public bool playerSpawnPoint = false;

		//Private data
		private bool gotReleventComponents = false;
		private MeshRenderer markerMesh;
		private MeshRenderer switchSceneIcon;
		private MeshRenderer moveLocationIcon;
		private MeshRenderer lockedIcon;
		private MeshRenderer pointIcon;
		private Transform lookAtJointTransform;
		private new Animation animation;
		private Text titleText;
		private Player player;
		private Vector3 lookAtPosition = Vector3.zero;
		private int tintColorID = 0;
		private Color tintColor = Color.clear;
		private Color titleColor = Color.clear;
		private float fullTitleAlpha = 0.0f;
		private Animator _animator;
        
		//Constants
		private const string switchSceneAnimation = "switch_scenes_idle";
		private const string moveLocationAnimation = "move_location_idle";
		private const string lockedAnimation = "locked_idle";

		private float startElevation;
		private float currentElevation;
		private float desiredElevation;
		//-------------------------------------------------
		public override bool showReticle {
			get {
				return false;
			}
		}


		//-------------------------------------------------
		void Awake ()
		{
			GetRelevantComponents ();

			animation = GetComponent<Animation> ();
			_animator = GetComponent<Animator> ();

			tintColorID = Shader.PropertyToID ("_TintColor");
			if (!isMapMarker) {
				moveLocationIcon.gameObject.SetActive (false);
				switchSceneIcon.gameObject.SetActive (false);
				lockedIcon.gameObject.SetActive (false);
			}
			UpdateVisuals ();
		}


		//-------------------------------------------------
		void Start ()
		{
			player = Player.instance;

			startElevation = transform.localPosition.z;
			currentElevation = startElevation;
			desiredElevation = (teleportToPosition.y * .01f) + startElevation;
		}

		void OnDisable ()
		{
			if (isMapMarker)
				transform.localPosition = new Vector3 (transform.localPosition.x, transform.localPosition.y, startElevation);
		}


		//-------------------------------------------------
		void Update ()
		{
			if (Application.isPlaying) {
				if (!isMapMarker) {
					lookAtPosition.x = player.hmdTransform.position.x;
					lookAtPosition.y = lookAtJointTransform.position.y;
					lookAtPosition.z = player.hmdTransform.position.z;

					lookAtJointTransform.LookAt (lookAtPosition);
				} else if (isMapMarker && desiredElevation - transform.localPosition.z > .001f) {
					float heightMultiplier = .01f;

					currentElevation = Mathf.Lerp (transform.localPosition.z, desiredElevation, Time.deltaTime * 2f);

					transform.localPosition = new Vector3 (transform.localPosition.x, transform.localPosition.y, currentElevation);
				}
			}
		}


		//-------------------------------------------------
		public override bool ShouldActivate (Vector3 playerPosition)
		{
			return (Vector3.Distance (transform.position, playerPosition) > 1.0f);
		}


		//-------------------------------------------------
		public override bool ShouldMovePlayer ()
		{
			return true;
		}


		//-------------------------------------------------
		public override void Highlight (bool highlight)
		{
			if (!locked) {
				if (highlight) {
					if (isMapMarker)
						_animator.SetTrigger ("Select");
					else
						SetMeshMaterials (Teleport.instance.pointHighlightedMaterial, titleHighlightedColor);
				} else {
					if (isMapMarker)
						_animator.SetTrigger ("Deselect");
					else
						SetMeshMaterials (Teleport.instance.pointVisibleMaterial, titleVisibleColor);
				}
			}

			if (highlight) {
				if (isMapMarker) {
					_animator.SetTrigger ("Select");
				} else {
					pointIcon.gameObject.SetActive (true);
					animation.Play ();
				}
			} else {
				if (isMapMarker) {
					_animator.SetTrigger ("Deselect");
				} else {
					pointIcon.gameObject.SetActive (false);
					animation.Stop ();
				}
			}
		}


		//-------------------------------------------------
		public override void UpdateVisuals ()
		{
			if (!gotReleventComponents) {
				return;
			}

			if (locked && !isMapMarker) {

				SetMeshMaterials (Teleport.instance.pointLockedMaterial, titleLockedColor);
				pointIcon = lockedIcon;
				animation.clip = animation.GetClip (lockedAnimation);
				
			} else if (!isMapMarker) {

				SetMeshMaterials (Teleport.instance.pointVisibleMaterial, titleVisibleColor);

				switch (teleportType) {
				case TeleportPointType.MoveToLocation:
					{
						pointIcon = moveLocationIcon;

						animation.clip = animation.GetClip (moveLocationAnimation);
					}
					break;
				case TeleportPointType.SwitchToNewScene:
					{
						pointIcon = switchSceneIcon;

						animation.clip = animation.GetClip (switchSceneAnimation);
					}
					break;
				}
				
			}
			if (titleText != null)
				titleText.text = title;
		}


		//-------------------------------------------------
		public override void SetAlpha (float tintAlpha, float alphaPercent)
		{
			if (isMapMarker)
				return;

			tintColor = markerMesh.material.GetColor (tintColorID);
			tintColor.a = tintAlpha;

			markerMesh.material.SetColor (tintColorID, tintColor);
			switchSceneIcon.material.SetColor (tintColorID, tintColor);
			moveLocationIcon.material.SetColor (tintColorID, tintColor);
			lockedIcon.material.SetColor (tintColorID, tintColor);

			titleColor.a = fullTitleAlpha * alphaPercent;
			titleText.color = titleColor;
		}


		//-------------------------------------------------
		public void SetMeshMaterials (Material material, Color textColor)
		{

			if (isMapMarker)
				return;
			markerMesh.material = material;
			switchSceneIcon.material = material;
			moveLocationIcon.material = material;
			lockedIcon.material = material;

			titleColor = textColor;
			fullTitleAlpha = textColor.a;
			titleText.color = titleColor;
		}


		//-------------------------------------------------
		public void TeleportToScene ()
		{
			if (!string.IsNullOrEmpty (switchToScene)) {
				Debug.Log ("TeleportPoint: Hook up your level loading logic to switch to new scene: " + switchToScene);
			} else {
				Debug.LogError ("TeleportPoint: Invalid scene name to switch to: " + switchToScene);
			}
		}


		//-------------------------------------------------
		public void GetRelevantComponents ()
		{
			if (!isMapMarker) {
				markerMesh = transform.Find ("teleport_marker_mesh").GetComponent<MeshRenderer> ();
				switchSceneIcon = transform.Find ("teleport_marker_lookat_joint/teleport_marker_icons/switch_scenes_icon").GetComponent<MeshRenderer> ();
				moveLocationIcon = transform.Find ("teleport_marker_lookat_joint/teleport_marker_icons/move_location_icon").GetComponent<MeshRenderer> ();
				lockedIcon = transform.Find ("teleport_marker_lookat_joint/teleport_marker_icons/locked_icon").GetComponent<MeshRenderer> ();
				lookAtJointTransform = transform.Find ("teleport_marker_lookat_joint");

				titleText = transform.Find ("teleport_marker_lookat_joint/teleport_marker_canvas/teleport_marker_canvas_text").GetComponent<Text> ();
			}

			gotReleventComponents = true;
		}


		//-------------------------------------------------
		public void ReleaseRelevantComponents ()
		{
			if (isMapMarker)
				return;

			markerMesh = null;
			switchSceneIcon = null;
			moveLocationIcon = null;
			lockedIcon = null;
			lookAtJointTransform = null;
			titleText = null;
		}


		//-------------------------------------------------
		public void UpdateVisualsInEditor ()
		{
			if (Application.isPlaying) {
				return;
			}

			GetRelevantComponents ();

			if (locked && !isMapMarker) {

				lockedIcon.gameObject.SetActive (true);
				moveLocationIcon.gameObject.SetActive (false);
				switchSceneIcon.gameObject.SetActive (false);

			
				markerMesh.sharedMaterial = Teleport.instance.pointLockedMaterial;
				lockedIcon.sharedMaterial = Teleport.instance.pointLockedMaterial;
				

				titleText.color = titleLockedColor;
                
			} else if (!isMapMarker) {

				lockedIcon.gameObject.SetActive (false);
				markerMesh.sharedMaterial = Teleport.instance.pointVisibleMaterial;
				switchSceneIcon.sharedMaterial = Teleport.instance.pointVisibleMaterial;
				moveLocationIcon.sharedMaterial = Teleport.instance.pointVisibleMaterial;

				switch (teleportType) {
				case TeleportPointType.MoveToLocation:
					{
						moveLocationIcon.gameObject.SetActive (true);
						switchSceneIcon.gameObject.SetActive (false);
					}
					break;
				case TeleportPointType.SwitchToNewScene:
					{
						moveLocationIcon.gameObject.SetActive (false);
						switchSceneIcon.gameObject.SetActive (true);
					}
					break;
				}
				
				titleText.color = titleVisibleColor;

                
				
			}

			if (titleText != null)
				titleText.text = title;

			ReleaseRelevantComponents ();
		}
	}

}
