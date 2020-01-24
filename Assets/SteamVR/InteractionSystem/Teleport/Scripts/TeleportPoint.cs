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
			SwitchToNewScene
		};

		//Public variables
		public TeleportPointType teleportType = TeleportPointType.MoveToLocation;
		public string title;
		public string switchToScene;
		public Color titleVisibleColor;
		public Color titleHighlightedColor;
		public Color titleLockedColor;
		public bool playerSpawnPoint = false;

		public Material VisibleMaterial;
		public Material LockedMaterial;
		public Material HighlightedMaterial;

		//Private data
		private bool gotReleventComponents = false;
		private MeshRenderer markerMesh;
		private MeshRenderer switchSceneIcon;
		private MeshRenderer moveLocationIcon;
		private MeshRenderer lockedIcon;
		private MeshRenderer pointIcon;
		private Transform lookAtJointTransform;
		private new Animation animation;
		private Transform player;
		private Vector3 lookAtPosition = Vector3.zero;
		private int tintColorID = 0;
		private Color tintColor = Color.clear;
		private Color titleColor = Color.clear;
		private float fullTitleAlpha = 0.0f;

		//Constants
		private const string switchSceneAnimation = "switch_scenes_idle";
		private const string moveLocationAnimation = "move_location_idle";
		private const string lockedAnimation = "locked_idle";


		//-------------------------------------------------
		public override bool showReticle
		{
			get
			{
				return false;
			}
		}


		//-------------------------------------------------
		void Awake()
		{
			GetRelevantComponents();

			animation = GetComponent<Animation>();

			tintColorID = Shader.PropertyToID( "_TintColor" );

			moveLocationIcon.gameObject.SetActive( false );
			switchSceneIcon.gameObject.SetActive( false );
			lockedIcon.gameObject.SetActive( false );

			UpdateVisuals();
		}


		//-------------------------------------------------
		void Start()
		{
			player = Camera.main.transform;
		}


		//-------------------------------------------------
		void Update()
		{
			if ( Application.isPlaying )
			{
				lookAtPosition.x = player.position.x;
				lookAtPosition.y = lookAtJointTransform.position.y;
				lookAtPosition.z = player.position.z;

				lookAtJointTransform.LookAt( lookAtPosition );
			}
		}


		//-------------------------------------------------
		public override bool ShouldActivate( Vector3 playerPosition )
		{
			return ( Vector3.Distance( transform.position, playerPosition ) > 1.0f );
		}


		//-------------------------------------------------
		public override bool ShouldMovePlayer()
		{
			return true;
		}


		//-------------------------------------------------
		public override void Highlight( bool highlight )
		{
			if ( !locked )
			{
				if ( highlight )
				{
					SetMeshMaterials( HighlightedMaterial, titleHighlightedColor );
				}
				else
				{
					SetMeshMaterials( VisibleMaterial, titleVisibleColor );
				}
			}

			if ( highlight )
			{
				pointIcon.gameObject.SetActive( true );
				animation.Play();
			}
			else
			{
				pointIcon.gameObject.SetActive( false );
				animation.Stop();
			}
		}


		//-------------------------------------------------
		public override void UpdateVisuals()
		{
			if ( !gotReleventComponents )
			{
				return;
			}

			if ( locked )
			{
				SetMeshMaterials( LockedMaterial, titleLockedColor );

				pointIcon = lockedIcon;

				animation.clip = animation.GetClip( lockedAnimation );
			}
			else
			{
				SetMeshMaterials( VisibleMaterial, titleVisibleColor );

				switch ( teleportType )
				{
					case TeleportPointType.MoveToLocation:
						{
							pointIcon = moveLocationIcon;

							animation.clip = animation.GetClip( moveLocationAnimation );
						}
						break;
					case TeleportPointType.SwitchToNewScene:
						{
							pointIcon = switchSceneIcon;

							animation.clip = animation.GetClip( switchSceneAnimation );
						}
						break;
				}
			}
		}


		//-------------------------------------------------
		public override void SetAlpha( float tintAlpha, float alphaPercent )
		{
			tintColor = markerMesh.material.GetColor( tintColorID );
			tintColor.a = tintAlpha;

			markerMesh.material.SetColor( tintColorID, tintColor );
			switchSceneIcon.material.SetColor( tintColorID, tintColor );
			moveLocationIcon.material.SetColor( tintColorID, tintColor );
			lockedIcon.material.SetColor( tintColorID, tintColor );

			titleColor.a = fullTitleAlpha * alphaPercent;
		}


		//-------------------------------------------------
		public void SetMeshMaterials( Material material, Color textColor )
		{
			markerMesh.material = material;
			switchSceneIcon.material = material;
			moveLocationIcon.material = material;
			lockedIcon.material = material;

			titleColor = textColor;
			fullTitleAlpha = textColor.a;
		}


		//-------------------------------------------------
		public void TeleportToScene()
		{
			if ( !string.IsNullOrEmpty( switchToScene ) )
			{
				Debug.Log("<b>[SteamVR Interaction]</b> TeleportPoint: Hook up your level loading logic to switch to new scene: " + switchToScene );
			}
			else
			{
				Debug.LogError("<b>[SteamVR Interaction]</b> TeleportPoint: Invalid scene name to switch to: " + switchToScene );
			}
		}


		//-------------------------------------------------
		public void GetRelevantComponents()
		{
			markerMesh = transform.Find( "teleport_marker_mesh" ).GetComponent<MeshRenderer>();
			switchSceneIcon = transform.Find( "teleport_marker_lookat_joint/teleport_marker_icons/switch_scenes_icon" ).GetComponent<MeshRenderer>();
			moveLocationIcon = transform.Find( "teleport_marker_lookat_joint/teleport_marker_icons/move_location_icon" ).GetComponent<MeshRenderer>();
			lockedIcon = transform.Find( "teleport_marker_lookat_joint/teleport_marker_icons/locked_icon" ).GetComponent<MeshRenderer>();
			lookAtJointTransform = transform.Find( "teleport_marker_lookat_joint" );

			gotReleventComponents = true;
		}


		//-------------------------------------------------
		public void ReleaseRelevantComponents()
		{
			markerMesh = null;
			switchSceneIcon = null;
			moveLocationIcon = null;
			lockedIcon = null;
			lookAtJointTransform = null;
		}


		//-------------------------------------------------
		public void UpdateVisualsInEditor()
		{
			if ( Application.isPlaying )
			{
				return;
			}

			GetRelevantComponents();

			if ( locked )
			{
				lockedIcon.gameObject.SetActive( true );
				moveLocationIcon.gameObject.SetActive( false );
				switchSceneIcon.gameObject.SetActive( false );

				markerMesh.sharedMaterial = LockedMaterial;
				lockedIcon.sharedMaterial = LockedMaterial;

			}
			else
			{
				lockedIcon.gameObject.SetActive( false );

				markerMesh.sharedMaterial = VisibleMaterial;
				switchSceneIcon.sharedMaterial = VisibleMaterial;
				moveLocationIcon.sharedMaterial = VisibleMaterial;

				switch ( teleportType )
				{
					case TeleportPointType.MoveToLocation:
						{
							moveLocationIcon.gameObject.SetActive( true );
							switchSceneIcon.gameObject.SetActive( false );
						}
						break;
					case TeleportPointType.SwitchToNewScene:
						{
							moveLocationIcon.gameObject.SetActive( false );
							switchSceneIcon.gameObject.SetActive( true );
						}
						break;
				}
			}

			ReleaseRelevantComponents();
		}
	}


#if UNITY_EDITOR
	//-------------------------------------------------------------------------
	[CustomEditor( typeof( TeleportPoint ) )]
	public class TeleportPointEditor : Editor
	{
		//-------------------------------------------------
		void OnEnable()
		{
			if ( Selection.activeTransform )
			{
				TeleportPoint teleportPoint = Selection.activeTransform.GetComponent<TeleportPoint>();
                if (teleportPoint != null)
				    teleportPoint.UpdateVisualsInEditor();
			}
		}


		//-------------------------------------------------
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			if ( Selection.activeTransform )
			{
				TeleportPoint teleportPoint = Selection.activeTransform.GetComponent<TeleportPoint>();
				if ( GUI.changed )
				{
					teleportPoint.UpdateVisualsInEditor();
				}
			}
		}
	}
#endif
}
