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

        private static void Build(
            OVRHand leftHand, OVRSkeleton leftSkeleton,
            OVRHand rightHand, OVRSkeleton rightSkeleton)
        {
            var root       = new GameObject("HandMenu");
            var controller = root.AddComponent<HandMenuController>();

            var rayLine = MakeRayLine(root.transform);
            var trigger = MakeTriggerButton(root.transform);
            var (overlay, closeBtn, opt1, opt3, opt4) =
                MakeOverlay(root.transform, leftSkeleton, rightSkeleton);

            var controllerSO = new SerializedObject(controller);
            controllerSO.FindProperty("leftHand").objectReferenceValue      = leftHand;
            controllerSO.FindProperty("leftSkeleton").objectReferenceValue  = leftSkeleton;
            controllerSO.FindProperty("rightHand").objectReferenceValue     = rightHand;
            controllerSO.FindProperty("rightSkeleton").objectReferenceValue = rightSkeleton;
            controllerSO.FindProperty("triggerButton").objectReferenceValue = trigger;
            controllerSO.FindProperty("overlayMenu").objectReferenceValue   = overlay;
            controllerSO.FindProperty("menuRayLine").objectReferenceValue   = rayLine;
            controllerSO.FindProperty("palmNormalAxis").vector3Value        = Vector3.down;
            controllerSO.FindProperty("invertPalmNormal").boolValue         = false;
            controllerSO.FindProperty("handPointer").objectReferenceValue   = Object.FindFirstObjectByType<TabletopHandPointer>();
            controllerSO.FindProperty("teleportInteractor").objectReferenceValue =
                GameObject.Find("TeleportHandInteractor");
            controllerSO.FindProperty("passthroughToggle").objectReferenceValue =
                Object.FindFirstObjectByType<MRPassthroughToggle>();
            controllerSO.ApplyModifiedPropertiesWithoutUndo();

            UnityEventTools.AddPersistentListener(closeBtn.onClick, controller.CloseMenu);
            UnityEventTools.AddPersistentListener(opt1.onClick,     controller.OnOption1Pressed);
            UnityEventTools.AddPersistentListener(opt3.onClick,     controller.OnOption3Pressed);
            UnityEventTools.AddPersistentListener(opt4.onClick,     controller.OnOption4Pressed);
        }

        private static LineRenderer MakeRayLine(Transform parent)
        {
            var go = new GameObject("MenuRay");
            go.transform.SetParent(parent, false);
            var rayLine = go.AddComponent<LineRenderer>();
            rayLine.positionCount = 2;
            rayLine.startWidth    = 0.004f;
            rayLine.endWidth      = 0.002f;
            rayLine.useWorldSpace = true;
            rayLine.material = new Material(Shader.Find("Universal Render Pipeline/Unlit"))
                { color = new Color(0.5f, 0.85f, 1f, 0.75f) };
            return rayLine;
        }

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

        private static (GameObject overlay,
                         HandMenuButton close,
                         HandMenuButton opt1,
                         HandMenuButton opt3,
                         HandMenuButton opt4)
            MakeOverlay(Transform parent, OVRSkeleton leftSkeleton, OVRSkeleton rightSkeleton)
        {
            var overlay = new GameObject("OverlayMenu");
            overlay.transform.SetParent(parent, false);

            var background = GameObject.CreatePrimitive(PrimitiveType.Cube);
            background.name = "Background";
            background.transform.SetParent(overlay.transform, false);
            background.transform.localPosition = Vector3.zero;
            background.transform.localScale    = new Vector3(0.16f, 0.27f, 0.002f);
            Object.DestroyImmediate(background.GetComponent<Collider>());
            background.GetComponent<Renderer>().sharedMaterial =
                new Material(Shader.Find("Universal Render Pipeline/Unlit"))
                    { color = new Color(0.07f, 0.07f, 0.08f) };

            MakeLabel(overlay.transform, "Title", "MENU",
                      localY: 0.112f, canvasW: 160f, canvasH: 22f, fontSize: 18);

            const float startY = 0.068f;
            var close = MakeButton(overlay.transform, "CloseBtn",   "Close",
                                   new Color(0.75f, 0.18f, 0.18f), startY);
            var opt1  = MakeButton(overlay.transform, "Option1Btn", "Toggle Ray",
                                   new Color(0.18f, 0.38f, 0.75f), startY - (BtnH + BtnGap));

            MakeSoundRangeSlider(overlay.transform,
                                 centerY: startY - (BtnH + BtnGap) * 2,
                                 leftSkeleton, rightSkeleton);

            var opt3  = MakeButton(overlay.transform, "Option3Btn", "Toggle Teleport",
                                   new Color(0.18f, 0.38f, 0.75f), startY - (BtnH + BtnGap) * 3);

            var opt4  = MakeButton(overlay.transform, "Option4Btn", "Toggle MR",
                                   new Color(0.18f, 0.55f, 0.38f), startY - (BtnH + BtnGap) * 4);

            overlay.SetActive(false);
            return (overlay, close, opt1, opt3, opt4);
        }

        private static void MakeSoundRangeSlider(Transform overlayParent, float centerY,
                                                  OVRSkeleton leftSkeleton, OVRSkeleton rightSkeleton)
        {
            const float halfLen = 0.055f;

            MakeLabel(overlayParent, "SoundRange_Title", "Sound Range",
                      localY: centerY + 0.018f, canvasW: 130f, canvasH: 20f, fontSize: 11);

            var track = GameObject.CreatePrimitive(PrimitiveType.Cube);
            track.name = "SoundRangeTrack";
            track.transform.SetParent(overlayParent, false);
            track.transform.localPosition = new Vector3(0f, centerY, -0.003f);
            track.transform.localScale    = new Vector3(halfLen * 2f, 0.006f, 0.006f);
            track.GetComponent<Renderer>().sharedMaterial =
                new Material(Shader.Find("Universal Render Pipeline/Lit"))
                    { color = new Color(0.25f, 0.25f, 0.27f) };

            var fillObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fillObject.name = "SoundRangeFill";
            fillObject.transform.SetParent(overlayParent, false);
            fillObject.transform.localPosition = new Vector3(-halfLen, centerY, -0.002f);
            fillObject.transform.localScale    = new Vector3(0.001f, 0.006f, 0.005f);
            Object.DestroyImmediate(fillObject.GetComponent<Collider>());
            fillObject.GetComponent<Renderer>().sharedMaterial =
                new Material(Shader.Find("Universal Render Pipeline/Lit"))
                    { color = new Color(0.18f, 0.65f, 1f) };

            var handleObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            handleObject.name = "SoundRangeHandle";
            handleObject.transform.SetParent(overlayParent, false);
            handleObject.transform.localPosition = new Vector3(-halfLen, centerY, -0.001f);
            handleObject.transform.localScale    = Vector3.one * 0.011f;
            Object.DestroyImmediate(handleObject.GetComponent<Collider>());
            handleObject.GetComponent<Renderer>().sharedMaterial =
                new Material(Shader.Find("Universal Render Pipeline/Lit"))
                    { color = Color.white };

            var valueLabel = MakeLabel(overlayParent, "SoundRange_Value", "Range: 5.00 m",
                                       localY: centerY - 0.018f, canvasW: 130f, canvasH: 20f, fontSize: 10);

            var slider   = overlayParent.gameObject.AddComponent<AudioRangeSlider>();
            var sliderSO = new SerializedObject(slider);
            sliderSO.FindProperty("leftSkeleton").objectReferenceValue   = leftSkeleton;
            sliderSO.FindProperty("rightSkeleton").objectReferenceValue  = rightSkeleton;
            sliderSO.FindProperty("trackTransform").objectReferenceValue = track.transform;
            sliderSO.FindProperty("fill").objectReferenceValue           = fillObject.transform;
            sliderSO.FindProperty("handle").objectReferenceValue         = handleObject.transform;
            sliderSO.FindProperty("valueLabel").objectReferenceValue     = valueLabel;
            sliderSO.FindProperty("trackHalfLen").floatValue             = halfLen;
            sliderSO.FindProperty("minRange").floatValue                 = 0.5f;
            sliderSO.FindProperty("maxRange").floatValue                 = 10.0f;
            sliderSO.FindProperty("initialRange").floatValue             = 5.0f;

            var audioManager = Object.FindFirstObjectByType<CabinAudioManager>();
            if (audioManager != null)
                sliderSO.FindProperty("audioManager").objectReferenceValue = audioManager;
            else
                Debug.LogWarning("[OAS] CabinAudioManager not found — assign it manually on the AudioRangeSlider.");

            sliderSO.ApplyModifiedPropertiesWithoutUndo();
        }

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
            var rectTransform = go.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(canvasW, canvasH);

            var textObject = new GameObject("Text");
            textObject.transform.SetParent(go.transform, false);
            var tmp       = textObject.AddComponent<TextMeshProUGUI>();
            tmp.text      = text;
            tmp.fontSize  = fontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color     = Color.white;

            var textRect      = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = textRect.offsetMax = Vector2.zero;

            return tmp;
        }
    }
}
