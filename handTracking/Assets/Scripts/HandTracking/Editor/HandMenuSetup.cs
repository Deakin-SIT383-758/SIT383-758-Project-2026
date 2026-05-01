using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using OAS.HandTracking;

namespace OAS.HandTracking.Editor
{
    public static class HandMenuSetup
    {
        private const float BtnW     = 0.13f;
        private const float BtnH     = 0.032f;
        private const float BtnThick = 0.003f;
        private const float BtnGap   = 0.012f;

        [MenuItem("OAS/3 - Setup Hand Menu")]
        public static void Setup()
        {
            var ovrRig = Object.FindFirstObjectByType<OVRCameraRig>();
            if (ovrRig == null)
            {
                Debug.LogError("[OAS] OVRCameraRig not found. " +
                               "Run 'OAS/1 - Setup Tabletop Scene' first.");
                return;
            }

            var allHands      = ovrRig.GetComponentsInChildren<OVRHand>(true);
            var leftHand      = allHands.FirstOrDefault(h => h.GetHand() == OVRPlugin.Hand.HandLeft);
            var rightHand     = allHands.FirstOrDefault(h => h.GetHand() == OVRPlugin.Hand.HandRight);
            var leftSkeleton  = leftHand?.GetComponent<OVRSkeleton>();
            var rightSkeleton = rightHand?.GetComponent<OVRSkeleton>();

            if (leftHand == null)
            {
                Debug.LogError("[OAS] Left OVRHand not found in scene.");
                return;
            }

            if (Object.FindFirstObjectByType<HandMenuController>() != null)
            {
                Debug.Log("[OAS] HandMenuController already in scene — skipping.");
                return;
            }

            Build(leftHand, leftSkeleton, rightHand, rightSkeleton);

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        }

        // ── Construction ─────────────────────────────────────────────────────────

        private static void Build(
            OVRHand leftHand, OVRSkeleton leftSkeleton,
            OVRHand rightHand, OVRSkeleton rightSkeleton)
        {
            var root       = new GameObject("HandMenu");
            var controller = root.AddComponent<HandMenuController>();

            var rayLine = MakeRayLine(root.transform);
            var trigger = MakeTriggerButton(root.transform);
            var (overlay, closeBtn, opt1, opt3) =
                MakeOverlay(root.transform, leftSkeleton, rightSkeleton);

            var cso = new SerializedObject(controller);
            cso.FindProperty("leftHand").objectReferenceValue      = leftHand;
            cso.FindProperty("leftSkeleton").objectReferenceValue  = leftSkeleton;
            cso.FindProperty("rightHand").objectReferenceValue     = rightHand;
            cso.FindProperty("rightSkeleton").objectReferenceValue = rightSkeleton;
            cso.FindProperty("triggerButton").objectReferenceValue = trigger;
            cso.FindProperty("overlayMenu").objectReferenceValue   = overlay;
            cso.FindProperty("menuRayLine").objectReferenceValue   = rayLine;
            cso.FindProperty("palmNormalAxis").vector3Value        = Vector3.down;
            cso.FindProperty("invertPalmNormal").boolValue         = false;
            cso.FindProperty("handPointer").objectReferenceValue   = Object.FindFirstObjectByType<TabletopHandPointer>();
            cso.FindProperty("teleportInteractor").objectReferenceValue =
                GameObject.Find("TeleportHandInteractor");
            cso.ApplyModifiedPropertiesWithoutUndo();

            UnityEventTools.AddPersistentListener(closeBtn.onClick, controller.CloseMenu);
            UnityEventTools.AddPersistentListener(opt1.onClick,     controller.OnOption1Pressed);
            UnityEventTools.AddPersistentListener(opt3.onClick,     controller.OnOption3Pressed);
        }

        // ── Ray line ─────────────────────────────────────────────────────────────

        private static LineRenderer MakeRayLine(Transform parent)
        {
            var go = new GameObject("MenuRay");
            go.transform.SetParent(parent, false);
            var lr = go.AddComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.startWidth    = 0.004f;
            lr.endWidth      = 0.002f;
            lr.useWorldSpace = true;
            lr.material = new Material(Shader.Find("Universal Render Pipeline/Unlit"))
                { color = new Color(0.5f, 0.85f, 1f, 0.75f) };
            return lr;
        }

        // ── Trigger button ────────────────────────────────────────────────────────

        private static GameObject MakeTriggerButton(Transform parent)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "TriggerButton";
            go.transform.SetParent(parent, false);
            go.transform.localScale = Vector3.one * 0.025f;
            go.GetComponent<Renderer>().sharedMaterial =
                new Material(Shader.Find("Universal Render Pipeline/Unlit"))
                    { color = new Color(0.25f, 0.85f, 1f) };
            go.SetActive(false);
            return go;
        }

        // ── Overlay panel ─────────────────────────────────────────────────────────

        private static (GameObject overlay,
                         HandMenuButton close,
                         HandMenuButton opt1,
                         HandMenuButton opt3)
            MakeOverlay(Transform parent, OVRSkeleton leftSk, OVRSkeleton rightSk)
        {
            var overlay = new GameObject("OverlayMenu");
            overlay.transform.SetParent(parent, false);

            // Dark background slab
            var bg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bg.name = "Background";
            bg.transform.SetParent(overlay.transform, false);
            bg.transform.localPosition = Vector3.zero;
            bg.transform.localScale    = new Vector3(0.16f, 0.225f, 0.002f);
            Object.DestroyImmediate(bg.GetComponent<Collider>());
            bg.GetComponent<Renderer>().sharedMaterial =
                new Material(Shader.Find("Universal Render Pipeline/Unlit"))
                    { color = new Color(0.07f, 0.07f, 0.08f) };

            // Title
            MakeLabel(overlay.transform, "Title", "MENU",
                      localY: 0.092f, canvasW: 160f, canvasH: 22f, fontSize: 18);

            // Buttons — stacked downward from startY
            const float startY = 0.045f;
            var close = MakeButton(overlay.transform, "CloseBtn",  "Close",
                                   new Color(0.75f, 0.18f, 0.18f), startY);
            var opt1  = MakeButton(overlay.transform, "Option1Btn","Toggle Ray",
                                   new Color(0.18f, 0.38f, 0.75f), startY - (BtnH + BtnGap));

            // Option 2 slot → sound range slider
            MakeSoundRangeSlider(overlay.transform,
                                 centerY: startY - (BtnH + BtnGap) * 2,
                                 leftSk, rightSk);

            var opt3  = MakeButton(overlay.transform, "Option3Btn","Toggle Teleport",
                                   new Color(0.18f, 0.38f, 0.75f), startY - (BtnH + BtnGap) * 3);

            overlay.SetActive(false);
            return (overlay, close, opt1, opt3);
        }

        // ── Sound range slider (replaces Option 2 button slot) ───────────────────

        private static void MakeSoundRangeSlider(Transform overlayParent, float centerY,
                                                  OVRSkeleton leftSk, OVRSkeleton rightSk)
        {
            const float halfLen = 0.055f;

            // Title label above track
            MakeLabel(overlayParent, "SoundRange_Title", "Sound Range",
                      localY: centerY + 0.018f, canvasW: 130f, canvasH: 20f, fontSize: 11);

            // Track — keeps BoxCollider for finger detection
            var track = GameObject.CreatePrimitive(PrimitiveType.Cube);
            track.name = "SoundRangeTrack";
            track.transform.SetParent(overlayParent, false);
            track.transform.localPosition = new Vector3(0f, centerY, -0.003f);
            track.transform.localScale    = new Vector3(halfLen * 2f, 0.006f, 0.006f);
            track.GetComponent<Renderer>().sharedMaterial =
                new Material(Shader.Find("Universal Render Pipeline/Lit"))
                    { color = new Color(0.25f, 0.25f, 0.27f) };

            // Fill — cyan bar, no collider
            var fillGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fillGO.name = "SoundRangeFill";
            fillGO.transform.SetParent(overlayParent, false);
            fillGO.transform.localPosition = new Vector3(-halfLen, centerY, -0.002f);
            fillGO.transform.localScale    = new Vector3(0.001f, 0.006f, 0.005f);
            Object.DestroyImmediate(fillGO.GetComponent<Collider>());
            fillGO.GetComponent<Renderer>().sharedMaterial =
                new Material(Shader.Find("Universal Render Pipeline/Lit"))
                    { color = new Color(0.18f, 0.65f, 1f) };

            // Handle — white sphere, no collider
            var handleGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            handleGO.name = "SoundRangeHandle";
            handleGO.transform.SetParent(overlayParent, false);
            handleGO.transform.localPosition = new Vector3(-halfLen, centerY, -0.001f);
            handleGO.transform.localScale    = Vector3.one * 0.011f;
            Object.DestroyImmediate(handleGO.GetComponent<Collider>());
            handleGO.GetComponent<Renderer>().sharedMaterial =
                new Material(Shader.Find("Universal Render Pipeline/Lit"))
                    { color = Color.white };

            // Value label below track
            var valueTmp = MakeLabel(overlayParent, "SoundRange_Value", "Range: 0.30 m",
                                     localY: centerY - 0.018f, canvasW: 130f, canvasH: 20f, fontSize: 10);

            // AudioRangeSlider on the overlay — disabled when menu closed (overlay inactive)
            var slider = overlayParent.gameObject.AddComponent<AudioRangeSlider>();
            var sso    = new SerializedObject(slider);
            sso.FindProperty("leftSkeleton").objectReferenceValue   = leftSk;
            sso.FindProperty("rightSkeleton").objectReferenceValue  = rightSk;
            sso.FindProperty("trackTransform").objectReferenceValue = track.transform;
            sso.FindProperty("fill").objectReferenceValue           = fillGO.transform;
            sso.FindProperty("handle").objectReferenceValue         = handleGO.transform;
            sso.FindProperty("valueLabel").objectReferenceValue     = valueTmp;
            sso.FindProperty("trackHalfLen").floatValue             = halfLen;
            sso.FindProperty("minRange").floatValue                 = 0.5f;
            sso.FindProperty("maxRange").floatValue                 = 10.0f;
            sso.FindProperty("initialRange").floatValue             = 5.0f;

            var audioMgr = Object.FindFirstObjectByType<CabinAudioManager>();
            if (audioMgr != null)
                sso.FindProperty("audioManager").objectReferenceValue = audioMgr;
            else
                Debug.LogWarning("[OAS] CabinAudioManager not found — assign it manually on the AudioRangeSlider.");

            sso.ApplyModifiedPropertiesWithoutUndo();
        }

        // ── Button factory ────────────────────────────────────────────────────────

        private static HandMenuButton MakeButton(
            Transform parent, string goName, string label, Color color, float localY)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = goName;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(0f, localY, -0.003f);
            go.transform.localScale    = new Vector3(BtnW, BtnH, BtnThick);
            go.GetComponent<Renderer>().sharedMaterial =
                new Material(Shader.Find("Universal Render Pipeline/Lit")) { color = color };

            MakeLabel(parent, goName + "_Label", label,
                      localY: localY, canvasW: 130f, canvasH: 32f, fontSize: 14);

            return go.AddComponent<HandMenuButton>();
        }

        // ── TMP label helper ──────────────────────────────────────────────────────

        private static TMP_Text MakeLabel(Transform parent, string goName, string text,
                                           float localY, float canvasW, float canvasH, int fontSize)
        {
            var go = new GameObject(goName);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(0f, localY, -0.006f);
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale    = Vector3.one * 0.001f;

            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(canvasW, canvasH);

            var textGO = new GameObject("Text");
            textGO.transform.SetParent(go.transform, false);
            var tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.text      = text;
            tmp.fontSize  = fontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color     = Color.white;

            var trt = textGO.GetComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = trt.offsetMax = Vector2.zero;

            return tmp;
        }
    }
}
