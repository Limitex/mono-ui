using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

#if UNITY_EDITOR
[CustomEditor(typeof(QuizManager))]
public class QuizManagerEditor : Editor
{
    private SerializedProperty questionText;
    private SerializedProperty answer0;
    private SerializedProperty answer1;
    private SerializedProperty answer2;
    private SerializedProperty answer3;
    private SerializedProperty successfulObjects;
    private SerializedProperty failureObjects;
    private SerializedProperty questions;
    private SerializedProperty answers0;
    private SerializedProperty answers1;
    private SerializedProperty answers2;
    private SerializedProperty answers3;
    private SerializedProperty correctAnswers;
    private SerializedProperty requiredSuccesses;
    private SerializedProperty successMessage;
    private SerializedProperty failureMessage;
    private SerializedProperty maxHistorySize;

    private bool[] foldouts;
    private Vector2 scrollPosition;
    private bool showQuizList = true;
    private bool showSettings = true;
    private bool showObjects = true;
    private readonly string[] answerOptions = new string[] { "Option 1", "Option 2", "Option 3", "Option 4" };
    private readonly Color correctAnswerColor = new Color(0.7f, 1f, 0.7f);

    private void OnEnable()
    {
        questionText = serializedObject.FindProperty("questionText");
        answer0 = serializedObject.FindProperty("answer0");
        answer1 = serializedObject.FindProperty("answer1");
        answer2 = serializedObject.FindProperty("answer2");
        answer3 = serializedObject.FindProperty("answer3");
        successfulObjects = serializedObject.FindProperty("successfulObjects");
        failureObjects = serializedObject.FindProperty("failureObjects");
        questions = serializedObject.FindProperty("questions");
        answers0 = serializedObject.FindProperty("answers0");
        answers1 = serializedObject.FindProperty("answers1");
        answers2 = serializedObject.FindProperty("answers2");
        answers3 = serializedObject.FindProperty("answers3");
        correctAnswers = serializedObject.FindProperty("correctAnswers");
        requiredSuccesses = serializedObject.FindProperty("requiredSuccesses");
        successMessage = serializedObject.FindProperty("successMessage");
        failureMessage = serializedObject.FindProperty("failureMessage");
        maxHistorySize = serializedObject.FindProperty("maxHistorySize");

        if (questions != null)
        {
            foldouts = new bool[questions.arraySize];
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Quiz Manager Settings", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        // UI References
        EditorGUILayout.LabelField("UI References", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(questionText);
        EditorGUILayout.PropertyField(answer0);
        EditorGUILayout.PropertyField(answer1);
        EditorGUILayout.PropertyField(answer2);
        EditorGUILayout.PropertyField(answer3);
        EditorGUI.indentLevel--;

        EditorGUILayout.Space(10);

        // Toggle Objects
        showObjects = EditorGUILayout.Foldout(showObjects, "Toggle Objects", true);
        if (showObjects)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(successfulObjects, new GUIContent("Success Objects"), true);
            EditorGUILayout.PropertyField(failureObjects, new GUIContent("Failure Objects"), true);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(10);

        // Game Settings
        showSettings = EditorGUILayout.Foldout(showSettings, "Game Settings", true);
        if (showSettings)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(requiredSuccesses, new GUIContent("Required Correct Answers"));
            EditorGUILayout.PropertyField(successMessage, new GUIContent("Success Message"));
            EditorGUILayout.PropertyField(failureMessage, new GUIContent("Failure Message"));
            EditorGUILayout.PropertyField(maxHistorySize, new GUIContent("Max History Size"));
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(10);

        showQuizList = EditorGUILayout.Foldout(showQuizList, "Quiz List", true);
        if (showQuizList)
        {
            EditorGUI.indentLevel++;
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            EditorGUI.BeginChangeCheck();
            int newSize = EditorGUILayout.IntField("Number of Quizzes", questions.arraySize);
            if (EditorGUI.EndChangeCheck())
            {
                questions.arraySize = newSize;
                answers0.arraySize = newSize;
                answers1.arraySize = newSize;
                answers2.arraySize = newSize;
                answers3.arraySize = newSize;
                correctAnswers.arraySize = newSize;
                foldouts = new bool[newSize];
            }

            for (int i = 0; i < questions.arraySize; i++)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.BeginVertical(GUI.skin.box);

                string questionPreview = questions.GetArrayElementAtIndex(i).stringValue;
                if (string.IsNullOrEmpty(questionPreview))
                {
                    questionPreview = "<No question>";
                }
                else if (questionPreview.Length > 50)
                {
                    questionPreview = questionPreview.Substring(0, 47) + "...";
                }

                string foldoutLabel = $"Quiz {i + 1} - {questionPreview}";
                foldouts[i] = EditorGUILayout.Foldout(foldouts[i], foldoutLabel, true);

                if (foldouts[i])
                {
                    EditorGUI.indentLevel++;

                    EditorGUILayout.PropertyField(questions.GetArrayElementAtIndex(i), new GUIContent("Question"));

                    EditorGUILayout.LabelField("Options", EditorStyles.boldLabel);
                    EditorGUI.indentLevel++;

                    int currentCorrectAnswer = correctAnswers.GetArrayElementAtIndex(i).intValue;

                    DrawOptionField(answers0.GetArrayElementAtIndex(i), "Option 1", currentCorrectAnswer == 0);
                    DrawOptionField(answers1.GetArrayElementAtIndex(i), "Option 2", currentCorrectAnswer == 1);
                    DrawOptionField(answers2.GetArrayElementAtIndex(i), "Option 3", currentCorrectAnswer == 2);
                    DrawOptionField(answers3.GetArrayElementAtIndex(i), "Option 4", currentCorrectAnswer == 3);

                    EditorGUI.indentLevel--;

                    EditorGUILayout.Space(5);
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField("Correct Answer", GUILayout.Width(120));
                        SerializedProperty correctAnswer = correctAnswers.GetArrayElementAtIndex(i);
                        int newAnswer = EditorGUILayout.Popup(correctAnswer.intValue, answerOptions);
                        if (correctAnswer.intValue != newAnswer)
                        {
                            correctAnswer.intValue = newAnswer;
                        }
                    }

                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndScrollView();
            EditorGUI.indentLevel--;
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawOptionField(SerializedProperty property, string label, bool isCorrect)
    {
        EditorGUILayout.BeginHorizontal();

        Color originalColor = GUI.backgroundColor;

        if (isCorrect)
        {
            GUI.backgroundColor = correctAnswerColor;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        }

        EditorGUILayout.PropertyField(property, new GUIContent(label));

        if (isCorrect)
        {
            EditorGUILayout.EndVertical();
            GUI.backgroundColor = originalColor;
        }

        EditorGUILayout.EndHorizontal();
    }
}
#endif