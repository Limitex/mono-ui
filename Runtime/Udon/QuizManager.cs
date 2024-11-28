using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class QuizManager : UdonSharpBehaviour
{
    [SerializeField] private TMP_Text questionText;
    [SerializeField] private TMP_Text answer0;
    [SerializeField] private TMP_Text answer1;
    [SerializeField] private TMP_Text answer2;
    [SerializeField] private TMP_Text answer3;
    [SerializeField] private Transform[] successfulObjects;

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

    private int[] history;
    private int historyIndex = 0;
    private int historyCount = 0;
    private int currentQuestionIndex = -1;
    private bool isExit = false;

    private void OnButtonPressed_handler(int index)
    {
        if (isExit) return;

        if (correctAnswers[currentQuestionIndex] == index)
        {
            requiredSuccesses--;
            if (requiredSuccesses <= 0)
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
        isExit = true;
        questionText.text = successMessage;
        foreach (var obj in successfulObjects)
        {
            if (obj != null)
            {
                obj.gameObject.SetActive(!obj.gameObject.activeSelf);
            }
        }
    }

    private void ShowFailure()
    {
        isExit = true;
        questionText.text = failureMessage;
    }

    void Start()
    {
        if (!ValidateArrays())
        {
            Debug.LogError("[QuizManager] Array lengths do not match!");
            isExit = true;
            return;
        }

        if (maxHistorySize > questions.Length)
        {
            maxHistorySize = questions.Length;
        }
        if (maxHistorySize < 1)
        {
            maxHistorySize = 1;
        }

        history = new int[maxHistorySize];
        NextQuestion();
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
        int index = GetRandom(0, questions.Length - 1);
        SetQuestion(index);
        currentQuestionIndex = index;
    }

    void SetQuestion(int index)
    {
        if (index < 0 || index >= questions.Length) return;

        questionText.text = questions[index];
        answer0.text = answers0[index];
        answer1.text = answers1[index];
        answer2.text = answers2[index];
        answer3.text = answers3[index];
    }

    int GetRandom(int min, int max)
    {
        if (max <= min) return min;
        if (max - min + 1 <= maxHistorySize)
        {
            return GetRandomWithFullHistory(min, max);
        }

        int number;
        do
        {
            number = Random.Range(min, max + 1);
        } while (IsInHistory(number));

        AddToHistory(number);
        return number;
    }

    private int GetRandomWithFullHistory(int min, int max)
    {
        for (int i = min; i <= max; i++)
        {
            if (!IsInHistory(i))
            {
                AddToHistory(i);
                return i;
            }
        }

        historyCount = 0;
        historyIndex = 0;
        int number = Random.Range(min, max + 1);
        AddToHistory(number);
        return number;
    }

    private bool IsInHistory(int number)
    {
        for (int i = 0; i < historyCount; i++)
        {
            if (history[i] == number) return true;
        }
        return false;
    }

    private void AddToHistory(int number)
    {
        history[historyIndex] = number;
        historyIndex = (historyIndex + 1) % maxHistorySize;
        if (historyCount < maxHistorySize)
        {
            historyCount++;
        }
    }
}