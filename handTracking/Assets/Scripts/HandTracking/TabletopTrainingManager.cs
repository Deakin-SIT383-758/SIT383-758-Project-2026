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

        private int  _questionIndex;
        private int  _score;
        private bool _awaitingInput;

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

            // Passenger chatter plays throughout the session
            audioManager?.PlayAllPassengers();

            ShowCurrentQuestion();
            _awaitingInput = true;
        }

        private void HandleSelection(TabletopHotspot hotspot)
        {
            if (!_awaitingInput) return;
            _awaitingInput = false;

            // Stop alarm as soon as the user selects something (correct or not)
            if (Questions[_questionIndex] == HotspotType.EmergencyExit)
                audioManager?.StopAlarm();

            bool correct = hotspot.Type == Questions[_questionIndex];
            if (correct) _score++;
            hotspot.OnSelected(correct);

            StartCoroutine(Advance());
        }

        private IEnumerator Advance()
        {
            yield return new WaitForSeconds(delayBetweenQuestions);
            _questionIndex++;

            if (_questionIndex >= Questions.Length)
            {
                ShowCompletion();
            }
            else
            {
                ShowCurrentQuestion();
                _awaitingInput = true;
            }
        }

        private void ShowCurrentQuestion()
        {
            if (questionText != null)
                questionText.text = QuestionPrompt(Questions[_questionIndex]);
            RefreshScore();

            // Trigger alarm when asking the user to find the emergency exit
            if (Questions[_questionIndex] == HotspotType.EmergencyExit)
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
                scoreText.text = $"Score: {_score} / {Questions.Length}";
        }

        private static string QuestionPrompt(HotspotType type) => type switch
        {
            HotspotType.EmergencyExit => "Follow the alarm — point to the Emergency Exit",
            HotspotType.FireHydrant   => "Point to the Fire Hydrant",
            HotspotType.LifeVest      => "Point to the Life Vest storage",
            _                         => string.Empty
        };
    }
}
