using System.Collections;
using TMPro;
using UnityEngine;

namespace OAS.HandTracking
{
    public class TabletopTrainingManager : MonoBehaviour
    {
        [SerializeField] private TabletopHandPointer pointer;
        [SerializeField] private TabletopFingerTouch fingerTouch;
        [SerializeField] private TMP_Text questionText;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private GameObject completionPanel;
        [SerializeField] private CabinAudioManager audioManager;
        [SerializeField] private float delayBetweenQuestions = 2f;

        private static readonly HotspotType[] Questions =
        {
            HotspotType.EmergencyExit,
            HotspotType.FireHydrant,
            HotspotType.LifeVest
        };

        private int currentQuestionIndex;
        private int score;
        private bool awaitingInput;

        private void OnEnable()
        {
            if (pointer     != null) pointer.OnHotspotSelected     += HandleSelection;
            if (fingerTouch != null) fingerTouch.OnHotspotSelected += HandleSelection;
        }

        private void OnDisable()
        {
            if (pointer     != null) pointer.OnHotspotSelected     -= HandleSelection;
            if (fingerTouch != null) fingerTouch.OnHotspotSelected -= HandleSelection;
        }

        private void Start()
        {
            if (completionPanel != null) completionPanel.SetActive(false);

            audioManager?.PlayAllPassengers();

            ShowCurrentQuestion();
            awaitingInput = true;
        }

        private void HandleSelection(TabletopHotspot hotspot)
        {
            if (!awaitingInput) return;
            awaitingInput = false;

            if (Questions[currentQuestionIndex] == HotspotType.EmergencyExit)
                audioManager?.StopAlarm();

            bool correct = hotspot.Type == Questions[currentQuestionIndex];
            if (correct) score++;
            hotspot.OnSelected(correct);

            StartCoroutine(AdvanceToNextQuestion());
        }

        private IEnumerator AdvanceToNextQuestion()
        {
            yield return new WaitForSeconds(delayBetweenQuestions);
            currentQuestionIndex++;

            if (currentQuestionIndex >= Questions.Length)
            {
                ShowCompletion();
            }
            else
            {
                ShowCurrentQuestion();
                awaitingInput = true;
            }
        }

        private void ShowCurrentQuestion()
        {
            if (questionText != null)
                questionText.text = GetQuestionPrompt(Questions[currentQuestionIndex]);
            RefreshScore();

            if (Questions[currentQuestionIndex] == HotspotType.EmergencyExit)
                audioManager?.TriggerAlarm();
        }

        private void ShowCompletion()
        {
            audioManager?.StopAll();
            if (questionText != null)
                questionText.text = "Training Complete!";
            RefreshScore();
            if (completionPanel != null) completionPanel.SetActive(true);
        }

        private void RefreshScore()
        {
            if (scoreText != null)
                scoreText.text = $"Score: {score} / {Questions.Length}";
        }

        private static string GetQuestionPrompt(HotspotType type) => type switch
        {
            HotspotType.EmergencyExit => "Follow the alarm — point to the Emergency Exit",
            HotspotType.FireHydrant   => "Point to the Fire Hydrant",
            HotspotType.LifeVest      => "Point to the Life Vest storage",
            _                         => string.Empty
        };
    }
}
