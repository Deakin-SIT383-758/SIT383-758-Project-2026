using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace OAS.HandTracking.Editor
{
    public static class CabinAudioSetup
    {
        private static readonly Vector3[] PassengerPositions =
        {
            new(-0.05f, 0.01f,  0.06f), new(0.05f, 0.01f,  0.06f),
            new(-0.05f, 0.01f,  0.00f), new(0.05f, 0.01f,  0.00f),
            new(-0.05f, 0.01f, -0.06f), new(0.05f, 0.01f, -0.06f),
        };

        [MenuItem("OAS/2 - Setup 3D Audio")]
        public static void Setup()
        {
            if (Object.FindFirstObjectByType<CabinAudioManager>() != null)
            {
                Debug.Log("[OAS] CabinAudioManager already in scene — skipping.");
                return;
            }

            Build();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        }

        private static void Build()
        {
            var root    = new GameObject("CabinAudio");
            var manager = root.AddComponent<CabinAudioManager>();

            OVRSkeleton leftSkeleton  = null;
            OVRSkeleton rightSkeleton = null;
            var ovrRig = Object.FindFirstObjectByType<OVRCameraRig>();
            if (ovrRig != null)
            {
                var allHands  = ovrRig.GetComponentsInChildren<OVRHand>(true);
                var leftHand  = allHands.FirstOrDefault(h => h.GetHand() == OVRPlugin.Hand.HandLeft);
                var rightHand = allHands.FirstOrDefault(h => h.GetHand() == OVRPlugin.Hand.HandRight);
                leftSkeleton  = leftHand?.GetComponent<OVRSkeleton>();
                rightSkeleton = rightHand?.GetComponent<OVRSkeleton>();
            }

            var ambient = MakeSource(root.transform, "Ambient",
                                     loop: true, spatialBlend: 0f,
                                     minDist: 1f, maxDist: 20f, volume: 0.35f);

            var alarm = MakeSource(root.transform, "Alarm",
                                   loop: true, spatialBlend: 1f,
                                   minDist: 0.04f, maxDist: 0.5f, volume: 1f);

            var passengerParent = new GameObject("Passengers");
            passengerParent.transform.SetParent(root.transform, false);

            var passengerSources = new AudioSource[PassengerPositions.Length];
            for (int i = 0; i < PassengerPositions.Length; i++)
            {
                var source = MakeSource(passengerParent.transform, $"Passenger_{i}",
                                        loop: true, spatialBlend: 1f,
                                        minDist: 0.02f, maxDist: 0.25f, volume: 0.6f);
                source.transform.localPosition = PassengerPositions[i];
                passengerSources[i] = source;
            }

            var managerSO = new SerializedObject(manager);
            managerSO.FindProperty("ambientSource").objectReferenceValue = ambient;
            managerSO.FindProperty("alarmSource").objectReferenceValue   = alarm;

            var passengerProp = managerSO.FindProperty("passengerSources");
            passengerProp.arraySize = passengerSources.Length;
            for (int i = 0; i < passengerSources.Length; i++)
                passengerProp.GetArrayElementAtIndex(i).objectReferenceValue = passengerSources[i];

            managerSO.ApplyModifiedPropertiesWithoutUndo();

            var training = Object.FindFirstObjectByType<TabletopTrainingManager>();
            if (training != null)
            {
                var trainingSO = new SerializedObject(training);
                var prop       = trainingSO.FindProperty("audioManager");
                if (prop != null)
                {
                    prop.objectReferenceValue = manager;
                    trainingSO.ApplyModifiedPropertiesWithoutUndo();
                    Debug.Log("[OAS] CabinAudioManager wired to TabletopTrainingManager.");
                }
            }
        }

        private static void MakeSliderPanel(Transform parent, CabinAudioManager manager,
                                             OVRSkeleton leftSkeleton, OVRSkeleton rightSkeleton)
        {
            var panel = new GameObject("AudioRangePanel");
            panel.transform.SetParent(parent, false);
            panel.transform.localPosition = new Vector3(0.2f, 0.04f, 0f);

            var background = GameObject.CreatePrimitive(PrimitiveType.Cube);
            background.name = "Background";
            background.transform.SetParent(panel.transform, false);
            background.transform.localPosition = Vector3.zero;
            background.transform.localScale    = new Vector3(0.165f, 0.06f, 0.002f);
            Object.DestroyImmediate(background.GetComponent<Collider>());
            background.GetComponent<Renderer>().sharedMaterial =
                new Material(Shader.Find("Universal Render Pipeline/Unlit"))
                    { color = new Color(0.07f, 0.07f, 0.08f) };

            const float trackHalf = 0.06f;
            var track = GameObject.CreatePrimitive(PrimitiveType.Cube);
            track.name = "Track";
            track.transform.SetParent(panel.transform, false);
            track.transform.localPosition = new Vector3(0f, -0.008f, -0.002f);
            track.transform.localScale    = new Vector3(trackHalf * 2f, 0.006f, 0.006f);
            track.GetComponent<Renderer>().sharedMaterial =
                new Material(Shader.Find("Universal Render Pipeline/Lit"))
                    { color = new Color(0.25f, 0.25f, 0.27f) };

            var fillObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fillObject.name = "Fill";
            fillObject.transform.SetParent(panel.transform, false);
            fillObject.transform.localPosition = new Vector3(-trackHalf, -0.008f, -0.003f);
            fillObject.transform.localScale    = new Vector3(0.001f, 0.006f, 0.005f);
            Object.DestroyImmediate(fillObject.GetComponent<Collider>());
            fillObject.GetComponent<Renderer>().sharedMaterial =
                new Material(Shader.Find("Universal Render Pipeline/Lit"))
                    { color = new Color(0.18f, 0.65f, 1f) };

            var handleObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            handleObject.name = "Handle";
            handleObject.transform.SetParent(panel.transform, false);
            handleObject.transform.localPosition = new Vector3(-trackHalf, -0.008f, -0.005f);
            handleObject.transform.localScale    = Vector3.one * 0.013f;
            Object.DestroyImmediate(handleObject.GetComponent<Collider>());
            handleObject.GetComponent<Renderer>().sharedMaterial =
                new Material(Shader.Find("Universal Render Pipeline/Lit"))
                    { color = Color.white };

            MakeLabel(panel.transform, "Title",  "Sound Range",  new Vector3(0f,  0.018f, -0.002f), 12);
            var valueLabel = MakeLabel(panel.transform, "Value", "Range: 0.30 m", new Vector3(0f, -0.022f, -0.002f), 11);

            var slider   = panel.AddComponent<AudioRangeSlider>();
            var sliderSO = new SerializedObject(slider);
            sliderSO.FindProperty("audioManager").objectReferenceValue   = manager;
            sliderSO.FindProperty("leftSkeleton").objectReferenceValue   = leftSkeleton;
            sliderSO.FindProperty("rightSkeleton").objectReferenceValue  = rightSkeleton;
            sliderSO.FindProperty("trackTransform").objectReferenceValue = track.transform;
            sliderSO.FindProperty("fill").objectReferenceValue           = fillObject.transform;
            sliderSO.FindProperty("handle").objectReferenceValue         = handleObject.transform;
            sliderSO.FindProperty("valueLabel").objectReferenceValue     = valueLabel;
            sliderSO.FindProperty("trackHalfLen").floatValue             = trackHalf;
            sliderSO.ApplyModifiedPropertiesWithoutUndo();
        }

        private static TMP_Text MakeLabel(Transform parent, string goName, string text,
                                           Vector3 localPos, int fontSize)
        {
            var go = new GameObject(goName);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale    = Vector3.one * 0.001f;

            var canvas        = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            var rectTransform = go.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(160f, 20f);

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

        private static AudioSource MakeSource(Transform parent, string goName,
                                               bool loop, float spatialBlend,
                                               float minDist, float maxDist, float volume)
        {
            var go           = new GameObject(goName);
            go.transform.SetParent(parent, false);
            var source          = go.AddComponent<AudioSource>();
            source.loop         = loop;
            source.spatialBlend = spatialBlend;
            source.minDistance  = minDist;
            source.maxDistance  = maxDist;
            source.rolloffMode  = AudioRolloffMode.Linear;
            source.playOnAwake  = false;
            source.volume       = volume;
            return source;
        }
    }
}
