using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using OAS.HandTracking;
using Unity.XR.CoreUtils;

namespace OAS.HandTracking.Editor
{
    public static class TabletopSceneSetup
    {
        private const float TableHeight   = 0.76f;
        private const float TableThick    = 0.05f;
        private const float TableTopY     = TableHeight + TableThick * 0.5f;
        private const float HotspotSize   = 0.08f;
        private const float HotspotY      = TableHeight + TableThick + HotspotSize * 0.5f;

        [MenuItem("OAS/1 - Setup Tabletop Scene")]
        public static void Setup()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");

            EnsureDirectionalLight();
            CreateFloor();

            SetupCameraRig(
                out var rightHand, out var rightControllerAnchor, out var rightSkeleton,
                out var leftHand,  out var leftControllerAnchor,  out var leftSkeleton);

            var table    = CreateTable();
            var hotspots = CreateHotspots(table.transform);

            var (pointer, rightLine, leftLine) = CreateHandPointer();
            AssignPointerRefs(pointer, rightLine, leftLine,
                rightHand, rightControllerAnchor, rightSkeleton,
                leftHand,  leftControllerAnchor,  leftSkeleton);

            var fingerTouch = CreateFingerTouch(rightHand, rightSkeleton, leftHand, leftSkeleton);

            var (questionTMP, scoreTMP, completionPanel) = CreateUI();
            CreateTrainingManager(pointer, fingerTouch, questionTMP, scoreTMP, completionPanel);

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        }

        private static void EnsureDirectionalLight()
        {
            if (Object.FindFirstObjectByType<Light>() != null) return;
            var go = new GameObject("Directional Light");
            go.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            var light = go.AddComponent<Light>();
            light.type      = LightType.Directional;
            light.intensity = 1f;
        }

        private static void CreateFloor()
        {
            if (GameObject.Find("Floor") != null) return;
            var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Floor";
            floor.transform.position   = Vector3.zero;
            floor.transform.localScale = new Vector3(2f, 1f, 2f);
        }

        private static void SetupCameraRig(
            out OVRHand  rightHand,  out Transform rightControllerAnchor, out OVRSkeleton rightSkeleton,
            out OVRHand  leftHand,   out Transform leftControllerAnchor,  out OVRSkeleton leftSkeleton)
        {
            rightHand = leftHand = null;
            rightControllerAnchor = leftControllerAnchor = null;
            rightSkeleton = leftSkeleton = null;

            var xrOrigin = Object.FindFirstObjectByType<XROrigin>();
            if (xrOrigin != null)
            {
                Debug.Log("[OAS Setup] Removing XR Origin — OVRCameraRig is used for Meta Quest hand tracking.");
                Object.DestroyImmediate(xrOrigin.gameObject);
            }

            var existing = Object.FindFirstObjectByType<OVRCameraRig>();
            GameObject rig = existing != null ? existing.gameObject : null;

            if (rig == null)
            {
                var guids = AssetDatabase.FindAssets("OVRCameraRig t:Prefab");
                foreach (var guid in guids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    if (System.IO.Path.GetFileNameWithoutExtension(path) == "OVRCameraRig")
                    {
                        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                        rig = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                        rig.name = "OVRCameraRig";
                        break;
                    }
                }
            }

            if (rig == null)
            {
                Debug.LogWarning("[OAS Setup] OVRCameraRig prefab not found.");
                return;
            }

            rig.transform.position = Vector3.zero;
            AddHandPrefabs(rig);
            ConfigureOVRManager(rig);

            var allHands = rig.GetComponentsInChildren<OVRHand>(true);

            rightHand     = allHands.FirstOrDefault(h => h.GetHand() == OVRPlugin.Hand.HandRight);
            leftHand      = allHands.FirstOrDefault(h => h.GetHand() == OVRPlugin.Hand.HandLeft);
            rightSkeleton = rightHand != null ? rightHand.GetComponent<OVRSkeleton>() : null;
            leftSkeleton  = leftHand  != null ? leftHand.GetComponent<OVRSkeleton>()  : null;

            rightControllerAnchor =
                rig.transform.Find("TrackingSpace/RightHandAnchor/RightControllerAnchor") ??
                FindDescendantByName(rig.transform, "RightControllerAnchor");

            leftControllerAnchor =
                rig.transform.Find("TrackingSpace/LeftHandAnchor/LeftControllerAnchor") ??
                FindDescendantByName(rig.transform, "LeftControllerAnchor");
        }

        private static GameObject CreateTable()
        {
            if (GameObject.Find("Table") is GameObject t) return t;

            var table = GameObject.CreatePrimitive(PrimitiveType.Cube);
            table.name = "Table";
            // Centred at (0, TableTopY - half, 1m forward)
            table.transform.position   = new Vector3(0f, TableTopY - TableThick * 0.5f, 0.7f);
            table.transform.localScale = new Vector3(1f, TableThick, 0.6f);
            return table;
        }

        private static TabletopHotspot[] CreateHotspots(Transform tableTransform)
        {
            // (name, type, local offset from table centre on its top surface)
            var defs = new (string name, HotspotType type, Vector3 worldPos)[]
            {
                ("Exit_A",      HotspotType.EmergencyExit, new Vector3(-0.28f, HotspotY, 0.55f)),
                ("Exit_B",      HotspotType.EmergencyExit, new Vector3( 0.28f, HotspotY, 0.55f)),
                ("FireHydrant", HotspotType.FireHydrant,   new Vector3(-0.15f, HotspotY, 0.82f)),
                ("LifeVest",    HotspotType.LifeVest,      new Vector3( 0.15f, HotspotY, 0.82f)),
            };

            // Default tint colours so types are distinguishable before interaction
            var baseColors = new[]
            {
                new Color(0.2f, 0.4f, 0.9f),  // Exit A  – blue
                new Color(0.2f, 0.4f, 0.9f),  // Exit B  – blue
                new Color(0.9f, 0.5f, 0.1f),  // Hydrant – orange
                new Color(0.6f, 0.2f, 0.8f),  // LifeVest– purple
            };

            var results = new TabletopHotspot[defs.Length];
            for (int i = 0; i < defs.Length; i++)
            {
                // Reuse if already placed
                var existing = GameObject.Find(defs[i].name);
                if (existing != null)
                {
                    results[i] = existing.GetComponent<TabletopHotspot>();
                    continue;
                }

                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = defs[i].name;
                go.transform.SetParent(tableTransform);
                go.transform.position   = defs[i].worldPos;
                go.transform.localScale = Vector3.one * HotspotSize;

                // Tint
                var rend = go.GetComponent<Renderer>();
                if (rend != null)
                {
                    var mat = new Material(rend.sharedMaterial) { color = baseColors[i] };
                    rend.sharedMaterial = mat;
                }

                var hotspot = go.AddComponent<TabletopHotspot>();

                // Set hotspotType via SerializedObject so it survives
                var so = new SerializedObject(hotspot);
                so.FindProperty("hotspotType").enumValueIndex = (int)defs[i].type;
                so.ApplyModifiedPropertiesWithoutUndo();

                results[i] = hotspot;
            }
            return results;
        }

        private static (TabletopHandPointer pointer, LineRenderer rightLine, LineRenderer leftLine)
            CreateHandPointer()
        {
            var existing = Object.FindFirstObjectByType<TabletopHandPointer>();
            if (existing != null)
            {
                var eRight = existing.transform.Find("RightRay")?.GetComponent<LineRenderer>();
                var eLeft  = existing.transform.Find("LeftRay")?.GetComponent<LineRenderer>();
                return (existing, eRight, eLeft);
            }

            var go      = new GameObject("HandPointer");
            var pointer = go.AddComponent<TabletopHandPointer>();

            var rightLine = MakeRayChild(go, "RightRay", new Color(0.9f, 0.9f, 1f, 0.8f));
            var leftLine  = MakeRayChild(go, "LeftRay",  new Color(1f, 0.9f, 0.7f, 0.8f));

            return (pointer, rightLine, leftLine);
        }

        private static LineRenderer MakeRayChild(GameObject parent, string childName, Color color)
        {
            var child = new GameObject(childName);
            child.transform.SetParent(parent.transform, false);
            var lr = child.AddComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.startWidth    = 0.005f;
            lr.endWidth      = 0.002f;
            lr.useWorldSpace = true;
            lr.material      = new Material(Shader.Find("Universal Render Pipeline/Unlit")) { color = color };
            return lr;
        }

        private static void AssignPointerRefs(
            TabletopHandPointer pointer, LineRenderer rightLine, LineRenderer leftLine,
            OVRHand rightHand, Transform rightControllerAnchor, OVRSkeleton rightSkeleton,
            OVRHand leftHand,  Transform leftControllerAnchor,  OVRSkeleton leftSkeleton)
        {
            var so = new SerializedObject(pointer);
            so.FindProperty("rightHand").objectReferenceValue             = rightHand;
            so.FindProperty("rightSkeleton").objectReferenceValue         = rightSkeleton;
            so.FindProperty("rightControllerAnchor").objectReferenceValue = rightControllerAnchor;
            so.FindProperty("rightRayLine").objectReferenceValue          = rightLine;
            so.FindProperty("leftHand").objectReferenceValue              = leftHand;
            so.FindProperty("leftSkeleton").objectReferenceValue          = leftSkeleton;
            so.FindProperty("leftControllerAnchor").objectReferenceValue  = leftControllerAnchor;
            so.FindProperty("leftRayLine").objectReferenceValue           = leftLine;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static (TMP_Text question, TMP_Text score, GameObject completionPanel) CreateUI()
        {
            // Reuse if already in scene
            var existingCanvas = GameObject.Find("TrainingUI");
            if (existingCanvas != null)
            {
                var q = existingCanvas.transform.Find("QuestionText")?.GetComponent<TMP_Text>();
                var s = existingCanvas.transform.Find("ScoreText")?.GetComponent<TMP_Text>();
                var c = existingCanvas.transform.Find("CompletionPanel")?.gameObject;
                return (q, s, c);
            }

            // World-space canvas positioned in front of the trainee at eye level
            var canvasGO = new GameObject("TrainingUI");
            var canvas   = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();

            // 800×200 units, scale 0.002 → 1.6m × 0.4m in world space
            var canvasRect  = canvasGO.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(800f, 220f);
            canvasGO.transform.position   = new Vector3(0f, 1.55f, 1.2f);
            canvasGO.transform.rotation   = Quaternion.identity;
            canvasGO.transform.localScale = Vector3.one * 0.002f;

            // Background panel
            var bgGO   = new GameObject("Background");
            bgGO.transform.SetParent(canvasGO.transform, false);
            var bgImg  = bgGO.AddComponent<Image>();
            bgImg.color = new Color(0f, 0f, 0f, 0.65f);
            var bgRect = bgGO.GetComponent<RectTransform>();
            bgRect.anchorMin  = Vector2.zero;
            bgRect.anchorMax  = Vector2.one;
            bgRect.offsetMin  = Vector2.zero;
            bgRect.offsetMax  = Vector2.zero;

            // Question text
            var qGO  = new GameObject("QuestionText");
            qGO.transform.SetParent(canvasGO.transform, false);
            var qTMP = qGO.AddComponent<TextMeshProUGUI>();
            qTMP.text      = "Loading...";
            qTMP.fontSize  = 40f;
            qTMP.alignment = TextAlignmentOptions.Center;
            qTMP.color     = Color.white;
            var qRect = qGO.GetComponent<RectTransform>();
            qRect.sizeDelta         = new Vector2(780f, 130f);
            qRect.anchoredPosition  = new Vector2(0f, 40f);

            // Score text
            var sGO  = new GameObject("ScoreText");
            sGO.transform.SetParent(canvasGO.transform, false);
            var sTMP = sGO.AddComponent<TextMeshProUGUI>();
            sTMP.text      = "Score: 0 / 3";
            sTMP.fontSize  = 28f;
            sTMP.alignment = TextAlignmentOptions.Center;
            sTMP.color     = new Color(0.8f, 0.95f, 0.8f);
            var sRect = sGO.GetComponent<RectTransform>();
            sRect.sizeDelta        = new Vector2(780f, 60f);
            sRect.anchoredPosition = new Vector2(0f, -75f);

            // Completion panel (hidden until training finishes)
            var cpGO  = new GameObject("CompletionPanel");
            cpGO.transform.SetParent(canvasGO.transform, false);
            var cpImg = cpGO.AddComponent<Image>();
            cpImg.color = new Color(0f, 0.45f, 0f, 0.9f);
            var cpRect = cpGO.GetComponent<RectTransform>();
            cpRect.anchorMin  = Vector2.zero;
            cpRect.anchorMax  = Vector2.one;
            cpRect.offsetMin  = Vector2.zero;
            cpRect.offsetMax  = Vector2.zero;

            var ctGO  = new GameObject("CompletionText");
            ctGO.transform.SetParent(cpGO.transform, false);
            var ctTMP = ctGO.AddComponent<TextMeshProUGUI>();
            ctTMP.text      = "Training Complete!";
            ctTMP.fontSize  = 48f;
            ctTMP.alignment = TextAlignmentOptions.Center;
            ctTMP.color     = Color.white;
            var ctRect = ctGO.GetComponent<RectTransform>();
            ctRect.anchorMin = Vector2.zero;
            ctRect.anchorMax = Vector2.one;
            ctRect.offsetMin = Vector2.zero;
            ctRect.offsetMax = Vector2.zero;

            cpGO.SetActive(false);

            return (qTMP, sTMP, cpGO);
        }

        private static TabletopFingerTouch CreateFingerTouch(
            OVRHand rightHand, OVRSkeleton rightSkeleton,
            OVRHand leftHand,  OVRSkeleton leftSkeleton)
        {
            var touch = Object.FindFirstObjectByType<TabletopFingerTouch>()
                     ?? new GameObject("FingerTouch").AddComponent<TabletopFingerTouch>();

            var so = new SerializedObject(touch);
            so.FindProperty("rightHand").objectReferenceValue     = rightHand;
            so.FindProperty("rightSkeleton").objectReferenceValue = rightSkeleton;
            so.FindProperty("leftHand").objectReferenceValue      = leftHand;
            so.FindProperty("leftSkeleton").objectReferenceValue  = leftSkeleton;
            so.ApplyModifiedPropertiesWithoutUndo();
            return touch;
        }

        private static void CreateTrainingManager(TabletopHandPointer pointer,
                                                   TabletopFingerTouch fingerTouch,
                                                   TMP_Text questionTMP,
                                                   TMP_Text scoreTMP,
                                                   GameObject completionPanel)
        {
            var manager = Object.FindFirstObjectByType<TabletopTrainingManager>()
                       ?? new GameObject("TrainingManager").AddComponent<TabletopTrainingManager>();

            var so = new SerializedObject(manager);
            so.FindProperty("pointer").objectReferenceValue         = pointer;
            so.FindProperty("fingerTouch").objectReferenceValue     = fingerTouch;
            so.FindProperty("questionText").objectReferenceValue    = questionTMP;
            so.FindProperty("scoreText").objectReferenceValue       = scoreTMP;
            so.FindProperty("completionPanel").objectReferenceValue = completionPanel;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ─────────────────────────────────────────────────────────────────────
        // Utilities / Hand prefab helpers
        // ─────────────────────────────────────────────────────────────────────

        private static void AddHandPrefabs(GameObject rig)
        {
            var guids = AssetDatabase.FindAssets("OVRHandPrefab t:Prefab");
            if (guids.Length == 0)
            {
                Debug.LogWarning("[OAS Setup] OVRHandPrefab prefab not found in project.");
                return;
            }
            var prefabPath = AssetDatabase.GUIDToAssetPath(guids[0]);
            var handPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            var leftAnchor  = FindDescendantByName(rig.transform, "LeftHandAnchor");
            var rightAnchor = FindDescendantByName(rig.transform, "RightHandAnchor");

            SetupHandOnAnchor(handPrefab, leftAnchor,  isRight: false);
            SetupHandOnAnchor(handPrefab, rightAnchor, isRight: true);
        }

        private static void SetupHandOnAnchor(GameObject handPrefab, Transform anchor, bool isRight)
        {
            if (anchor == null) return;
            if (anchor.GetComponentInChildren<OVRHand>() != null) return; // already set up

            var go = (GameObject)PrefabUtility.InstantiatePrefab(handPrefab, anchor);
            go.name = isRight ? "OVRRightHandVisual" : "OVRLeftHandVisual";
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;

            int handInt = isRight ? 1 : 0;

            var ovrHand = go.GetComponent<OVRHand>();
            if (ovrHand != null)
            {
                var so = new SerializedObject(ovrHand);
                so.FindProperty("HandType").intValue = handInt;
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            var ovrSkel = go.GetComponent<OVRSkeleton>();
            if (ovrSkel != null)
            {
                var so = new SerializedObject(ovrSkel);
                so.FindProperty("_skeletonType").intValue = handInt;
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            var ovrMesh = go.GetComponent<OVRMesh>();
            if (ovrMesh != null)
            {
                var so = new SerializedObject(ovrMesh);
                so.FindProperty("_meshType").intValue = handInt;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static void ConfigureOVRManager(GameObject rig)
        {
            var mgr = rig.GetComponent<OVRManager>();
            if (mgr == null) return;

            var so = new SerializedObject(mgr);

            // Allow controller and hands to work simultaneously
            var simProp = so.FindProperty("SimultaneousHandsAndControllersEnabled");
            if (simProp != null) simProp.boolValue = true;

            // Controller can drive hand poses when hands are not tracked (Natural = index 1)
            var ctrlProp = so.FindProperty("controllerDrivenHandPosesType");
            if (ctrlProp != null) ctrlProp.enumValueIndex = 1;

            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Transform FindDescendantByName(Transform root, string name)
        {
            foreach (Transform child in root)
            {
                if (child.name == name) return child;
                var found = FindDescendantByName(child, name);
                if (found != null) return found;
            }
            return null;
        }
    }
}

