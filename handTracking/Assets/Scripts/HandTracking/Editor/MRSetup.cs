using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Meta.XR.MRUtilityKit;

namespace OAS.HandTracking.Editor
{
    public static class MRSetup
    {
        [MenuItem("OAS/5 - Setup MR (Passthrough + Table Anchor)")]
        public static void Setup()
        {
            SetupPassthrough();
            SetupMRUK();
            SetupTableAnchor();
            WirePassthroughToMenu();

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        }

        private static void SetupPassthrough()
        {
            if (Object.FindFirstObjectByType<OVRPassthroughLayer>() != null)
            {
                Debug.Log("[OAS] OVRPassthroughLayer already exists — skipping.");
                return;
            }

            var go = new GameObject("Passthrough");
            var layer = go.AddComponent<OVRPassthroughLayer>();

            var so = new SerializedObject(layer);
            var placement = so.FindProperty("projectionSurfaceType");
            if (placement != null) placement.enumValueIndex = 0; 
            var overlay = so.FindProperty("overlayType");
            if (overlay != null) overlay.enumValueIndex = 1; 
            so.ApplyModifiedPropertiesWithoutUndo();

            var cam = Camera.main;
            if (cam != null)
            {
                cam.clearFlags      = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0f, 0f, 0f, 0f);
                EditorUtility.SetDirty(cam);
            }

            var ovrManager = Object.FindFirstObjectByType<OVRManager>();
            if (ovrManager != null)
            {
                var mso = new SerializedObject(ovrManager);
                foreach (var propName in new[] { "_passthroughSupport", "passthroughSupport",
                                                  "_isInsightPassthroughEnabled" })
                {
                    var prop = mso.FindProperty(propName);
                    if (prop == null) continue;
                    if (prop.propertyType == SerializedPropertyType.Enum)
                        prop.enumValueIndex = 1; // Supported
                    else
                        prop.boolValue = true;
                    break;
                }
                mso.ApplyModifiedPropertiesWithoutUndo();
            }

            var toggle = go.AddComponent<MRPassthroughToggle>();
            var tso    = new SerializedObject(toggle);
            var floor  = GameObject.Find("Floor");
            tso.FindProperty("virtualFloor").objectReferenceValue = floor;
            tso.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetupMRUK()
        {
            if (Object.FindFirstObjectByType<MRUK>() != null)
            {
                return;
            }

            var go = new GameObject("MRUK");
            go.AddComponent<MRUK>();
            go.SetActive(false); 
        }

        private static void SetupTableAnchor()
        {
            if (GameObject.Find("TableAnchor") != null)
            {
                Debug.Log("[OAS] TableAnchor already in scene — skipping.");
                return;
            }

            var anchorGO = new GameObject("TableAnchor");
            anchorGO.transform.position = Vector3.zero;

            var table = GameObject.Find("Table");
            if (table != null)
                table.transform.SetParent(anchorGO.transform, true);
            else
                Debug.LogWarning("[OAS] 'Table' GameObject not found — reparent it under TableAnchor manually.");

            var ui = GameObject.Find("TrainingUI");
            if (ui != null)
                ui.transform.SetParent(anchorGO.transform, true);
            else
                Debug.LogWarning("GameObject not found — reparent it under TableAnchor manually.");

            var anchor = anchorGO.AddComponent<QRTableAnchor>();
            var aso = new SerializedObject(anchor);
            aso.FindProperty("tableRoot").objectReferenceValue         = anchorGO.transform;
            aso.FindProperty("tableSurfaceLocalY").floatValue          = 0.785f;
            // FindFirstObjectByType with Include finds inactive objects too
            var mruk = Object.FindFirstObjectByType<MRUK>(FindObjectsInactive.Include);
            aso.FindProperty("mrukGameObject").objectReferenceValue  = mruk != null ? mruk.gameObject : null;
            aso.FindProperty("virtualFloor").objectReferenceValue    = GameObject.Find("Floor");
            aso.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void WirePassthroughToMenu()
        {
            var controller = Object.FindFirstObjectByType<HandMenuController>();
            if (controller == null)
            {
                Debug.LogWarning("[OAS] HandMenuController not found — run OAS/3 first.");
                return;
            }

            var toggle = Object.FindFirstObjectByType<MRPassthroughToggle>();
            if (toggle == null)
            {
                return;
            }

            var cso = new SerializedObject(controller);
            cso.FindProperty("passthroughToggle").objectReferenceValue = toggle;
            cso.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
