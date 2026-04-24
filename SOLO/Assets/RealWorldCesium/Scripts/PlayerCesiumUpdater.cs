using CesiumForUnity;
using Meta.WitAi.Attributes;
using MM.WorldInteraction;
using UnityEngine;


namespace MM.WorldInteraction.Cesium
{

    public class PlayerCesiumUpdater : MonoBehaviour
    {
        [SerializeField] private CesiumGeoreference cesiumGeoreference;
        [SerializeField] private WorldManager worldManager;

        private void Awake()
        {
            if (worldManager == null)
            {
                worldManager = GameObject.FindFirstObjectByType<WorldManager>();
                if (worldManager == null)
                {
                    Debug.LogError("No WorldManager in scene. Disabling CesiumUpdater");
                    this.enabled = false;
                    return;
                }
            }
            if (cesiumGeoreference == null)
            {
                cesiumGeoreference = GameObject.FindFirstObjectByType<CesiumGeoreference>();
                if (cesiumGeoreference == null)
                {
                    Debug.LogError("No CesiumGeoreference in scene. Disabling CesiumUpdater");
                    this.enabled = false;
                    return;
                }
            }
        }

        public double originHeight = 50;
        private void Start()
        {

        }

        public void UpdateCesiumGeoreference()
        {
            var playerPos = worldManager.playerPosition;
            cesiumGeoreference.SetOriginLongitudeLatitudeHeight(playerPos.Longitude, playerPos.Latitude, originHeight);
        }

        [ContextMenu("Update georeference")]
        public void UpdateCesiumGeoreferenceEditor()
        {
            if (ConfirmEditorModeSetup())
                UpdateCesiumGeoreference();
        }

        private bool ConfirmEditorModeSetup()
        {
            if (worldManager == null)
            {
                worldManager = GameObject.FindFirstObjectByType<WorldManager>();
                if (worldManager == null)
                {
                    return false;
                }
            }
            if (cesiumGeoreference == null)
            {
                cesiumGeoreference = GameObject.FindFirstObjectByType<CesiumGeoreference>();
                if (cesiumGeoreference == null)
                {
                    return false;
                }
            }

            return true;
        }
    }
}