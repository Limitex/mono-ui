using TMPro;
using UdonSharp;
using UnityEngine;

namespace Limitex.MonoUI.Udon
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class QuizManager : UdonSharpBehaviour
    {
        [SerializeField] private TMP_Text questionText;
        [SerializeField] private TMP_Text answer0;
        [SerializeField] private TMP_Text answer1;
        [SerializeField] private TMP_Text answer2;
        [SerializeField] private TMP_Text answer3;
        [SerializeField] private Transform[] successfulObjects;
        [SerializeField] private Transform[] failureObjects;

        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip successClip;
        [SerializeField] private AudioClip failureClip;

        [SerializeField] private int requiredSuccesses = 3;
        [SerializeField] private string successMessage = "Success!";
        [SerializeField] private string failureMessage = "Failure!";

        [SerializeField] private string[] questions;
        [SerializeField] private string[] answers0;
        [SerializeField] private string[] answers1;
        [SerializeField] private string[] answers2;
        [SerializeField] private string[] answers3;
        [SerializeField] private int[] correctAnswers;

        [SerializeField] private int maxHistorySize = 5;

        public void OnButton0Pressed() => OnButtonPressed_handler(0);
        public void OnButton1Pressed() => OnButtonPressed_handler(1);
        public void OnButton2Pressed() => OnButtonPressed_handler(2);
        public void OnButton3Pressed() => OnButtonPressed_handler(3);

        private int[] questionHistory;
        private int historyCount = 0;
        private int currentQuestionIndex = -1;
        private int correctAnswerCount = 0;
        private bool isGameOver = false;

        private void OnButtonPressed_handler(int index)
        {
            if (isGameOver) return;

            if (correctAnswers[currentQuestionIndex] == index)
            {
                correctAnswerCount++;
                if (correctAnswerCount >= requiredSuccesses)
                {
                    ShowSuccess();
                }
                else
                {
                    NextQuestion();
                }
            }
            else
            {
                ShowFailure();
            }
        }

        private void ShowSuccess()
        {
            isGameOver = true;
            questionText.text = successMessage;
            ToggleObjects(successfulObjects);
            if (successClip != null && audioSource != null)
            {
                audioSource.PlayOneShot(successClip);
            }
        }

        private void ShowFailure()
        {
            isGameOver = true;
            questionText.text = failureMessage;
            ToggleObjects(failureObjects);
            if (failureClip != null && audioSource != null)
            {
                audioSource.PlayOneShot(failureClip);
            }
        }

        private void ToggleObjects(Transform[] objects)
        {
            if (objects == null) return;

            foreach (var obj in objects)
            {
                if (obj != null)
                {
                    obj.gameObject.SetActive(!obj.gameObject.activeSelf);
                }
            }
        }

        void Start()
        {
            if (!ValidateArrays())
            {
                Debug.LogError("[QuizManager] Array lengths do not match!");
                isGameOver = true;
                return;
            }

            InitializeQuiz();
            NextQuestion();
        }

        private void InitializeQuiz()
        {
            if (maxHistorySize > questions.Length)
            {
                maxHistorySize = questions.Length;
            }
            if (maxHistorySize < 1)
            {
                maxHistorySize = 1;
            }

            questionHistory = new int[maxHistorySize];
            historyCount = 0;
            correctAnswerCount = 0;
            isGameOver = false;
        }

        private bool ValidateArrays()
        {
            int length = questions.Length;
            return answers0.Length == length &&
                   answers1.Length == length &&
                   answers2.Length == length &&
                   answers3.Length == length &&
                   correctAnswers.Length == length;
        }

        void NextQuestion()
        {
            if (historyCount >= maxHistorySize)
            {
                historyCount = 0;
            }

            int newIndex;
            do
            {
                newIndex = Random.Range(0, questions.Length);
            } while (IsInHistory(newIndex));

            questionHistory[historyCount++] = newIndex;
            currentQuestionIndex = newIndex;
            DisplayQuestion(newIndex);
        }

        void DisplayQuestion(int index)
        {
            if (index < 0 || index >= questions.Length) return;

            questionText.text = questions[index];
            answer0.text = answers0[index];
            answer1.text = answers1[index];
            answer2.text = answers2[index];
            answer3.text = answers3[index];
        }

        private bool IsInHistory(int number)
        {
            for (int i = 0; i < historyCount; i++)
            {
                if (questionHistory[i] == number) return true;
            }
            return false;
        }
    }
}
