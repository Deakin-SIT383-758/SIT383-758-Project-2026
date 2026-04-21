using System;
using Unity.Mathematics;
using Unity.InferenceEngine;
using UnityEngine;
using UnityEngine.UI;

public class HandDetection : MonoBehaviour
{
    public HandPreview handPreview;
    public ImagePreview imagePreview;
    public Texture2D imageTexture;
    public ModelAsset handDetector;
    public ModelAsset handLandmarker;
    public TextAsset anchorsCSV;
    
    public GameObject palmButtonPrefab; 
    public Canvas worldCanvas; 
    
    public float scoreThreshold = 0.5f;
    public float pinchThreshold = 0.3f;
    public float palmOpennessThreshold = 0.15f; // Threshold for detecting open palm
    public float palmFacingThreshold = 0.7f; // How much the palm needs to face camera (dot product)

    const int k_NumAnchors = 2016;
    float[,] m_Anchors;

    const int k_NumKeypoints = 21;
    const int detectorInputSize = 192;
    const int landmarkerInputSize = 224;

    Worker m_HandDetectorWorker;
    Worker m_HandLandmarkerWorker;
    Tensor<float> m_DetectorInput;
    Tensor<float> m_LandmarkerInput;
    Awaitable m_DetectAwaitable;

    float m_TextureWidth;
    float m_TextureHeight;
    
    // Palm detection state
    private GameObject m_PalmButton;
    private RectTransform m_ButtonRectTransform;
    private bool m_IsPalmOpen = false;
    private float m_PalmStabilityTimer = 0f;
    private const float k_PalmStabilityRequired = 0.1f; // Seconds of stable detection before showing button
    
    private readonly int[] palmKeypoints = { 0, 1, 5, 9, 13, 17 }; 
    
    public async void Start()
    {
        m_Anchors = BlazeUtils.LoadAnchors(anchorsCSV.text, k_NumAnchors);

        var handDetectorModel = ModelLoader.Load(handDetector);

        // post process the model to filter scores + argmax select the best hand
        var graph = new FunctionalGraph();
        var input = graph.AddInput(handDetectorModel, 0);
        var outputs = Functional.Forward(handDetectorModel, input);
        var boxes = outputs[0]; // (1, 2016, 18)
        var scores = outputs[1]; // (1, 2016, 1)
        var idx_scores_boxes = BlazeUtils.ArgMaxFiltering(boxes, scores);
        handDetectorModel = graph.Compile(idx_scores_boxes.Item1, idx_scores_boxes.Item2, idx_scores_boxes.Item3);

        m_HandDetectorWorker = new Worker(handDetectorModel, BackendType.GPUCompute);

        var handLandmarkerModel = ModelLoader.Load(handLandmarker);
        m_HandLandmarkerWorker = new Worker(handLandmarkerModel, BackendType.GPUCompute);

        m_DetectorInput = new Tensor<float>(new TensorShape(1, detectorInputSize, detectorInputSize, 3));
        m_LandmarkerInput = new Tensor<float>(new TensorShape(1, landmarkerInputSize, landmarkerInputSize, 3));
        
        // Initialize UI button
        InitializePalmButton();

        WebCamTexture wc = new WebCamTexture();
        wc.Play();

        while (true)
        {
            try
            {
                m_DetectAwaitable = Detect(wc);
                await m_DetectAwaitable;
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        m_HandDetectorWorker.Dispose();
        m_HandLandmarkerWorker.Dispose();
        m_DetectorInput.Dispose();
        m_LandmarkerInput.Dispose();
    }
    
    void InitializePalmButton()
    {
        if (palmButtonPrefab != null && worldCanvas != null)
        {
            m_PalmButton = Instantiate(palmButtonPrefab, worldCanvas.transform);
            m_ButtonRectTransform = m_PalmButton.GetComponent<RectTransform>();
            
            Button button = m_PalmButton.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(OnPalmButtonClicked);
            }
            
            m_PalmButton.SetActive(false);
        }
        else
        {
            Debug.LogError("Palm button prefab or world canvas not assigned!");
        }
    }
    
    void OnPalmButtonClicked()
    {
        Debug.Log("Palm Button Clicked!");
        // Func Coming Soon
    }
    
    Vector3 ImageToWorld(Vector2 position)
    {
        return (position - 0.5f * new Vector2(m_TextureWidth, m_TextureHeight)) / m_TextureHeight;
    }
    
    // Calculate palm normal to determine if palm is facing camera 
    Vector3 CalculatePalmNormal(Vector3[] joints)
    {
        // Use wrist, index finger base, and pinky base to calculate palm plane
        Vector3 wrist = joints[0];
        Vector3 indexBase = joints[5];
        Vector3 pinkyBase = joints[17];
        Vector3 middleBase = joints[9];
        
        // Detect if it's left or right hand
        // For right hand, thumb is on the left side of the palm when facing palm
        // For left hand, thumb is on the right side
        Vector3 thumbDir = joints[2] - wrist; // Direction to thumb base
        Vector3 pinkyDir = pinkyBase - wrist; // Direction to pinky base
        Vector3 indexDir = indexBase - wrist; // Direction to index base
        
        // Cross product to determine handedness
        Vector3 handPlane = Vector3.Cross(indexDir, pinkyDir);
        float handednessSign = Vector3.Dot(handPlane, thumbDir);
        
        // Calculate normal based on hand orientation
        Vector3 v1 = indexBase - wrist;
        Vector3 v2 = pinkyBase - wrist;
        
        // Cross product gives normal to palm plane
        Vector3 normal;
        if (handednessSign > 0)
        {
            // Right hand - reverse cross product order
            normal = Vector3.Cross(v2, v1).normalized;
        }
        else
        {
            // Left hand - standard cross product
            normal = Vector3.Cross(v1, v2).normalized;
        }
        
        return normal;
    }
    
    // Check if palm is open (fingers extended)
    bool IsPalmOpen(Vector3[] joints)
    {
        // Check distances from fingertip to palm center for each finger
        Vector3 palmCenter = (joints[0] + joints[9]) * 0.5f; // Wrist and middle finger base
        
        // Check each finger's extension
        float indexExtension = Vector3.Distance(joints[8], palmCenter) / Vector3.Distance(joints[5], palmCenter);
        float middleExtension = Vector3.Distance(joints[12], palmCenter) / Vector3.Distance(joints[9], palmCenter);
        float ringExtension = Vector3.Distance(joints[16], palmCenter) / Vector3.Distance(joints[13], palmCenter);
        float pinkyExtension = Vector3.Distance(joints[20], palmCenter) / Vector3.Distance(joints[17], palmCenter);
        float thumbExtension = Vector3.Distance(joints[4], palmCenter) / Vector3.Distance(joints[2], palmCenter);
        
        // Fingers should be extended (ratio > threshold)
        float avgExtension = (indexExtension + middleExtension + ringExtension + pinkyExtension + thumbExtension) / 5f;
        
        return avgExtension > (1f + palmOpennessThreshold);
    }
    
    // Check if palm is facing the camera
    bool IsPalmFacingCamera(Vector3 palmNormal)
    {
        // Camera forward is (0, 0, 1) in world space
        // Palm normal should point roughly towards camera
        float dotProduct = Vector3.Dot(palmNormal, Vector3.forward);
        return dotProduct > palmFacingThreshold;
    }

    async Awaitable Detect(Texture texture)
    {
        m_TextureWidth = texture.width;
        m_TextureHeight = texture.height;
        imagePreview.SetTexture(texture);

        var size = Mathf.Max(texture.width, texture.height);

        // The affine transformation matrix to go from tensor coordinates to image coordinates
        var scale = size / (float)detectorInputSize;
        var M = BlazeUtils.mul(BlazeUtils.TranslationMatrix(0.5f * (new Vector2(texture.width, texture.height) + new Vector2(-size, size))), BlazeUtils.ScaleMatrix(new Vector2(scale, -scale)));
        BlazeUtils.SampleImageAffine(texture, m_DetectorInput, M);

        m_HandDetectorWorker.Schedule(m_DetectorInput);

        var outputIdxAwaitable = (m_HandDetectorWorker.PeekOutput(0) as Tensor<int>).ReadbackAndCloneAsync();
        var outputScoreAwaitable = (m_HandDetectorWorker.PeekOutput(1) as Tensor<float>).ReadbackAndCloneAsync();
        var outputBoxAwaitable = (m_HandDetectorWorker.PeekOutput(2) as Tensor<float>).ReadbackAndCloneAsync();

        using var outputIdx = await outputIdxAwaitable;
        using var outputScore = await outputScoreAwaitable;
        using var outputBox = await outputBoxAwaitable;

        var scorePassesThreshold = outputScore[0] >= scoreThreshold;
        handPreview.SetActive(scorePassesThreshold);

        if (!scorePassesThreshold)
        {
            HidePalmButton();
            return;
        }

        var idx = outputIdx[0];

        var anchorPosition = detectorInputSize * new float2(m_Anchors[idx, 0], m_Anchors[idx, 1]);

        var boxCentre_TensorSpace = anchorPosition + new float2(outputBox[0, 0, 0], outputBox[0, 0, 1]);
        var boxSize_TensorSpace = math.max(outputBox[0, 0, 2], outputBox[0, 0, 3]);

        var kp0_TensorSpace = anchorPosition + new float2(outputBox[0, 0, 4 + 2 * 0 + 0], outputBox[0, 0, 4 + 2 * 0 + 1]);
        var kp2_TensorSpace = anchorPosition + new float2(outputBox[0, 0, 4 + 2 * 2 + 0], outputBox[0, 0, 4 + 2 * 2 + 1]);
        var delta_TensorSpace = kp2_TensorSpace - kp0_TensorSpace;
        var up_TensorSpace = delta_TensorSpace / math.length(delta_TensorSpace);
        var theta = math.atan2(delta_TensorSpace.y, delta_TensorSpace.x);
        var rotation = 0.5f * Mathf.PI - theta;
        boxCentre_TensorSpace += 0.5f * boxSize_TensorSpace * up_TensorSpace;
        boxSize_TensorSpace *= 2.6f;

        var origin2 = new float2(0.5f * landmarkerInputSize, 0.5f * landmarkerInputSize);
        var scale2 = boxSize_TensorSpace / landmarkerInputSize;
        var M2 = BlazeUtils.mul(M, BlazeUtils.mul(BlazeUtils.mul(BlazeUtils.mul(BlazeUtils.TranslationMatrix(boxCentre_TensorSpace), BlazeUtils.ScaleMatrix(new float2(scale2, -scale2))), BlazeUtils.RotationMatrix(rotation)), BlazeUtils.TranslationMatrix(-origin2)));
        BlazeUtils.SampleImageAffine(texture, m_LandmarkerInput, M2);

        m_HandLandmarkerWorker.Schedule(m_LandmarkerInput);

        var landmarksAwaitable = (m_HandLandmarkerWorker.PeekOutput("Identity") as Tensor<float>).ReadbackAndCloneAsync();
        using var landmarks = await landmarksAwaitable;

        Vector3[] jointPositions = new Vector3[k_NumKeypoints];

        for (var i = 0; i < k_NumKeypoints; i++)
        {
            var position_ImageSpace = BlazeUtils.mul(M2, new float2(landmarks[3 * i + 0], landmarks[3 * i + 1]));

            Vector3 position_WorldSpace = ImageToWorld(position_ImageSpace) + new Vector3(0, 0, landmarks[3 * i + 2] / m_TextureHeight);
            handPreview.SetKeypoint(i, true, position_WorldSpace);

            jointPositions[i] = position_WorldSpace;
        }

        // Check palm orientation and if its open or not
        Vector3 palmNormal = CalculatePalmNormal(jointPositions);
        bool palmFacing = IsPalmFacingCamera(palmNormal);
        bool palmOpen = IsPalmOpen(jointPositions);
        
        // Handle button visibility based on palm state
        if (palmFacing && palmOpen)
        {
            if (!m_IsPalmOpen)
            {
                m_PalmStabilityTimer += Time.deltaTime;
                if (m_PalmStabilityTimer >= k_PalmStabilityRequired)
                {
                    ShowPalmButton(jointPositions[4]); // Position at thumb tip
                    m_IsPalmOpen = true;
                }
            }
            else
            {
                UpdatePalmButtonPosition(jointPositions[4]); // Update thumb position
            }
        }
        else
        {
            HidePalmButton();
            m_IsPalmOpen = false;
            m_PalmStabilityTimer = 0f;
        }
        
        // Pinch Gesture
        Vector3 wrist = jointPositions[0];
        Vector3 thumpTip = jointPositions[4];
        Vector3 indexTip = jointPositions[8];

        float normalisedFTDistance = (indexTip-thumpTip).magnitude / (thumpTip - wrist).magnitude;
        
        if(normalisedFTDistance < pinchThreshold)
        {
            Debug.Log("Pinch Gesture Detected");
        }
        
        if (palmFacing && palmOpen)
        {
            Debug.Log($"Open Palm Facing Camera - Stability: {m_PalmStabilityTimer:F2}s");
        }
    }
    
    void ShowPalmButton(Vector3 worldPosition)
    {
        if (m_PalmButton == null) return;
        
        m_PalmButton.SetActive(true);
        UpdatePalmButtonPosition(worldPosition);
    }
    
    void UpdatePalmButtonPosition(Vector3 worldPosition)
    {
        if (m_PalmButton == null || !m_PalmButton.activeSelf) return;
        
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPosition);
        
        // Update button position
        if (m_ButtonRectTransform != null)
        {
            m_ButtonRectTransform.position = screenPos;
        }
    }
    
    void HidePalmButton()
    {
        if (m_PalmButton != null)
        {
            m_PalmButton.SetActive(false);
        }
        m_IsPalmOpen = false;
        m_PalmStabilityTimer = 0f;
    }

    void OnDestroy()
    {
        m_DetectAwaitable?.Cancel();
        
        if (m_PalmButton != null)
        {
            Destroy(m_PalmButton);
        }
    }
}