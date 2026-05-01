using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using OAS.HandTracking;

namespace OAS.HandTracking.Editor
{
    public static class EmergencyDoorSetup
    {
        private const float DoorZ         = -1.8f;
        private const float DoorWidth     = 1.0f;
        private const float DoorHeight    = 2.2f;
        private const float FrameBorder   = 0.08f;
        private const float DoorDepth     = 0.08f;

        [MenuItem("OAS/4 - Add Emergency Door")]
        public static void Setup()
        {
            if (GameObject.Find("EmergencyDoor") != null)
            {
                Debug.Log("[OAS] EmergencyDoor already in scene — skipping.");
                return;
            }

            var door = BuildDoor();
            WireAlarmAnchor(door);
            ExtendPointerRange();

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            Debug.Log("[OAS] Emergency door added behind the user at Z = " + DoorZ);
        }

        // ── Door geometry ─────────────────────────────────────────────────────

        private static GameObject BuildDoor()
        {
            float centerY = DoorHeight * 0.5f;

            var root = new GameObject("EmergencyDoor");
            root.transform.position = new Vector3(0f, centerY, DoorZ);

            // Frame — dark red surround
            var frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frame.name = "DoorFrame";
            frame.transform.SetParent(root.transform, false);
            frame.transform.localPosition = Vector3.zero;
            frame.transform.localScale = new Vector3(
                DoorWidth  + FrameBorder * 2f,
                DoorHeight + FrameBorder * 2f,
                DoorDepth  * 0.5f);
            Object.DestroyImmediate(frame.GetComponent<Collider>());
            frame.GetComponent<Renderer>().sharedMaterial =
                new Material(Shader.Find("Universal Render Pipeline/Lit"))
                    { color = new Color(0.6f, 0.05f, 0.05f) };

            // Door panel — green (international emergency exit colour), selectable
            var panel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            panel.name = "DoorPanel";
            panel.transform.SetParent(root.transform, false);
            panel.transform.localPosition = new Vector3(0f, 0f, -DoorDepth * 0.3f);
            panel.transform.localScale = new Vector3(DoorWidth, DoorHeight, DoorDepth);
            panel.GetComponent<Renderer>().sharedMaterial =
                new Material(Shader.Find("Universal Render Pipeline/Lit"))
                    { color = new Color(0.08f, 0.55f, 0.15f) };

            // Attach hotspot to the panel — this makes it selectable and anchors the alarm
            var hotspot = panel.AddComponent<TabletopHotspot>();
            var so      = new SerializedObject(hotspot);
            so.FindProperty("hotspotType").enumValueIndex = (int)HotspotType.EmergencyExit;
            so.ApplyModifiedPropertiesWithoutUndo();

            // Exit sign above the door
            MakeExitSign(root.transform, centerY);

            return root;
        }

        private static void MakeExitSign(Transform parent, float doorCenterY)
        {
            float signY = doorCenterY + DoorHeight * 0.5f + 0.15f;  // just above door top

            // Sign backing — bright red
            var sign = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sign.name = "ExitSign";
            sign.transform.SetParent(parent, false);
            sign.transform.localPosition = new Vector3(0f, signY - doorCenterY, -DoorDepth * 0.5f);
            sign.transform.localScale    = new Vector3(0.7f, 0.18f, 0.04f);
            Object.DestroyImmediate(sign.GetComponent<Collider>());
            sign.GetComponent<Renderer>().sharedMaterial =
                new Material(Shader.Find("Universal Render Pipeline/Unlit"))
                    { color = new Color(0.9f, 0.1f, 0.1f) };

            // TMP label "EMERGENCY EXIT"
            var labelGO = new GameObject("ExitLabel");
            labelGO.transform.SetParent(parent, false);
            labelGO.transform.localPosition = new Vector3(0f, signY - doorCenterY, -DoorDepth * 0.55f);
            labelGO.transform.localRotation = Quaternion.identity;
            labelGO.transform.localScale    = Vector3.one * 0.001f;

            var canvas = labelGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            var rt = labelGO.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(700f, 180f);

            var textGO = new GameObject("Text");
            textGO.transform.SetParent(labelGO.transform, false);
            var tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.text      = "EMERGENCY EXIT";
            tmp.fontSize  = 60;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color     = Color.white;

            var trt = textGO.GetComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = trt.offsetMax = Vector2.zero;
        }

        // ── Wire alarm anchor ─────────────────────────────────────────────────

        private static void WireAlarmAnchor(GameObject door)
        {
            var audioMgr = Object.FindFirstObjectByType<CabinAudioManager>();
            if (audioMgr == null)
            {
                Debug.LogWarning("[OAS] CabinAudioManager not found — assign Alarm Anchor manually.");
                return;
            }

            var panel = door.transform.Find("DoorPanel");
            if (panel == null) return;

            var so = new SerializedObject(audioMgr);
            so.FindProperty("alarmAnchor").objectReferenceValue = panel;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ── Extend pointer ray so the door (behind the user) is reachable ────

        private static void ExtendPointerRange()
        {
            var pointer = Object.FindFirstObjectByType<TabletopHandPointer>();
            if (pointer == null) return;

            var so       = new SerializedObject(pointer);
            var distProp = so.FindProperty("maxRayDistance");
            if (distProp != null && distProp.floatValue < 4f)
            {
                distProp.floatValue = 4f;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }
    }
}
