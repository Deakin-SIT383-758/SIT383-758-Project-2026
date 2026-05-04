using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace OAS.HandTracking.Editor
{
    public static class CabinAudioSetup
    {
        // Tabletop-scale seat positions (metres) — 3 rows of 2 seats
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

            // Find hand skeletons for the range slider
            OVRSkeleton leftSk = null, rightSk = null;
            var ovrRig = Object.FindFirstObjectByType<OVRCameraRig>();
            if (ovrRig != null)
            {
                var allHands = ovrRig.GetComponentsInChildren<OVRHand>(true);
                var lh = allHands.FirstOrDefault(h => h.GetHand() == OVRPlugin.Hand.HandLeft);
                var rh = allHands.FirstOrDefault(h => h.GetHand() == OVRPlugin.Hand.HandRight);
                leftSk  = lh?.GetComponent<OVRSkeleton>();
                rightSk = rh?.GetComponent<OVRSkeleton>();
            }

            // Ambient — non-spatial, heard everywhere (engine hum)
            var ambient = MakeSource(root.transform, "Ambient",
                                     loop: true, spatialBlend: 0f,
                                     minDist: 1f, maxDist: 20f, volume: 0.35f);

            // Alarm — fully spatial so user locates the emergency exit by ear
            var alarm = MakeSource(root.transform, "Alarm",
                                   loop: true, spatialBlend: 1f,
                                   minDist: 0.04f, maxDist: 0.5f, volume: 1f);

            // Passenger voices — spatial, arranged at seat positions
            var paxParent = new GameObject("Passengers");
            paxParent.transform.SetParent(root.transform, false);

            var paxSources = new AudioSource[PassengerPositions.Length];
            for (int i = 0; i < PassengerPositions.Length; i++)
            {
                var src = MakeSource(paxParent.transform, $"Passenger_{i}",
                                     loop: true, spatialBlend: 1f,
                                     minDist: 0.02f, maxDist: 0.25f, volume: 0.6f);
                src.transform.localPosition = PassengerPositions[i];
                paxSources[i] = src;
            }

            // Wire fields on CabinAudioManager
            var so = new SerializedObject(manager);
            so.FindProperty("ambientSource").objectReferenceValue = ambient;
            so.FindProperty("alarmSource").objectReferenceValue   = alarm;

            var paxProp = so.FindProperty("passengerSources");
            paxProp.arraySize = paxSources.Length;
            for (int i = 0; i < paxSources.Length; i++)
                paxProp.GetArrayElementAtIndex(i).objectReferenceValue = paxSources[i];

            so.ApplyModifiedPropertiesWithoutUndo();

            // Wire to training manager if present
            var training = Object.FindFirstObjectByType<TabletopTrainingManager>();
            if (training != null)
            {
                var tso  = new SerializedObject(training);
                var prop = tso.FindProperty("audioManager");
                if (prop != null)
                {
                    prop.objectReferenceValue = manager;
                    tso.ApplyModifiedPropertiesWithoutUndo();
                    Debug.Log("[OAS] CabinAudioManager wired to TabletopTrainingManager.");
                }
            }
        }

        // ── Range slider panel ───────────────────────────────────────────────────

        private static void MakeSliderPanel(Transform parent, CabinAudioManager manager,
                                             OVRSkeleton leftSk, OVRSkeleton rightSk)
        {
            // Panel root — positioned to the right of the table, upright and facing forward
            var panel = new GameObject("AudioRangePanel");
            panel.transform.SetParent(parent, false);
            panel.transform.localPosition = new Vector3(0.2f, 0.04f, 0f);

            // Dark background slab
            var bg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bg.name = "Background";
            bg.transform.SetParent(panel.transform, false);
            bg.transform.localPosition = Vector3.zero;
            bg.transform.localScale    = new Vector3(0.165f, 0.06f, 0.002f);
            Object.DestroyImmediate(bg.GetComponent<Collider>());
            bg.GetComponent<Renderer>().sharedMaterial =
                new Material(Shader.Find("Universal Render Pipeline/Unlit"))
                    { color = new Color(0.07f, 0.07f, 0.08f) };

            // Grey track bar — keeps its BoxCollider for finger touch detection
            const float trackHalf = 0.06f;
            var track = GameObject.CreatePrimitive(PrimitiveType.Cube);
            track.name = "Track";
            track.transform.SetParent(panel.transform, false);
            track.transform.localPosition = new Vector3(0f, -0.008f, -0.002f);
            track.transform.localScale    = new Vector3(trackHalf * 2f, 0.006f, 0.006f);
            track.GetComponent<Renderer>().sharedMaterial =
                new Material(Shader.Find("Universal Render Pipeline/Lit"))
                    { color = new Color(0.25f, 0.25f, 0.27f) };

            // Cyan fill — starts at minimum, grows right as range increases
            var fillGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fillGO.name = "Fill";
            fillGO.transform.SetParent(panel.transform, false);
            fillGO.transform.localPosition = new Vector3(-trackHalf, -0.008f, -0.003f);
            fillGO.transform.localScale    = new Vector3(0.001f, 0.006f, 0.005f);
            Object.DestroyImmediate(fillGO.GetComponent<Collider>());
            fillGO.GetComponent<Renderer>().sharedMaterial =
                new Material(Shader.Find("Universal Render Pipeline/Lit"))
                    { color = new Color(0.18f, 0.65f, 1f) };

            // White handle sphere — slides along track
            var handleGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            handleGO.name = "Handle";
            handleGO.transform.SetParent(panel.transform, false);
            handleGO.transform.localPosition = new Vector3(-trackHalf, -0.008f, -0.005f);
            handleGO.transform.localScale    = Vector3.one * 0.013f;
            Object.DestroyImmediate(handleGO.GetComponent<Collider>());
            handleGO.GetComponent<Renderer>().sharedMaterial =
                new Material(Shader.Find("Universal Render Pipeline/Lit"))
                    { color = Color.white };

            // Title and value label
            MakeLabel(panel.transform, "Title",  "Sound Range",  new Vector3(0f,  0.018f, -0.002f), 12);
            var valueTmp = MakeLabel(panel.transform, "Value", "Range: 0.30 m", new Vector3(0f, -0.022f, -0.002f), 11);

            // AudioRangeSlider component
            var slider = panel.AddComponent<AudioRangeSlider>();
            var sso    = new SerializedObject(slider);
            sso.FindProperty("audioManager").objectReferenceValue   = manager;
            sso.FindProperty("leftSkeleton").objectReferenceValue   = leftSk;
            sso.FindProperty("rightSkeleton").objectReferenceValue  = rightSk;
            sso.FindProperty("trackTransform").objectReferenceValue = track.transform;
            sso.FindProperty("fill").objectReferenceValue           = fillGO.transform;
            sso.FindProperty("handle").objectReferenceValue         = handleGO.transform;
            sso.FindProperty("valueLabel").objectReferenceValue     = valueTmp;
            sso.FindProperty("trackHalfLen").floatValue             = trackHalf;
            sso.ApplyModifiedPropertiesWithoutUndo();
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
            var rt            = go.GetComponent<RectTransform>();
            rt.sizeDelta      = new Vector2(160f, 20f);

            var textGO = new GameObject("Text");
            textGO.transform.SetParent(go.transform, false);
            var tmp       = textGO.AddComponent<TextMeshProUGUI>();
            tmp.text      = text;
            tmp.fontSize  = fontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color     = Color.white;

            var trt       = textGO.GetComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = trt.offsetMax = Vector2.zero;

            return tmp;
        }

        // ── Audio source factory ─────────────────────────────────────────────────

        private static AudioSource MakeSource(Transform parent, string goName,
                                               bool loop, float spatialBlend,
                                               float minDist, float maxDist, float volume)
        {
            var go           = new GameObject(goName);
            go.transform.SetParent(parent, false);
            var src          = go.AddComponent<AudioSource>();
            src.loop         = loop;
            src.spatialBlend = spatialBlend;
            src.minDistance  = minDist;
            src.maxDistance  = maxDist;
            src.rolloffMode  = AudioRolloffMode.Linear;
            src.playOnAwake  = false;
            src.volume       = volume;
            return src;
        }
    }
}
