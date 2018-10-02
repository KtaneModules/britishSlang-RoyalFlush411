using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using britishSlang;

public class britishSlangScript : MonoBehaviour
{
    public KMBombInfo Bomb;
    public KMAudio Audio;
    public Button[] buttons;
    public TextMesh displayedText;
    public TextMesh marker;
    public GameObject buttonsObject;

    //wordBanks
    public string[] words;
    public string[] definitions;

    //logic
    private List<int> selectedWords = new List<int>();
    private List<int> selectedDefinitions = new List<int>();
    int stage1Index = 0;
    int subsequentStageIndex = 0;
    private string[] correctAnswer = new string[6];

    //Logging
    static int moduleIdCounter = 1;
    int moduleId;
    int stage = 0;
    private bool moduleSolved = false;

    void Awake()
    {
        moduleId = moduleIdCounter++;
        foreach (Button button in buttons)
        {
            Button pressedButton = button;
            button.selectable.OnInteract += delegate () { OnButtonPress(pressedButton); return false; };
        }
    }

    void Start()
    {
        marker.text = "";
        stage1Selection();
    }

    void stage1Selection()
    {
        stage1Index = UnityEngine.Random.Range(0,42);
        displayedText.text = definitions[stage1Index];

        foreach(Button label in buttons)
        {
            int index = UnityEngine.Random.Range(0,42);
            while(selectedWords.Contains(index))
            {
                index = UnityEngine.Random.Range(0,42);
            }
            selectedWords.Add(index);
            label.text.text = words[index];
        }
        selectedWords.Clear();

        int emptyButtonIndex = UnityEngine.Random.Range(0,4);
        buttons[emptyButtonIndex].text.text = "";
        correctAnswer[stage] = words[stage1Index];
        Debug.LogFormat("[British Slang #{0}] Pip pip! The first definition is '{1}!'", moduleId, displayedText.text);
        Debug.LogFormat("[British Slang #{0}] Tally-ho! The correct word is the blank one!", moduleId, displayedText.text);
    }

    void subsequentStageSelection()
    {
        subsequentStageIndex = UnityEngine.Random.Range(0,42);
        while(selectedDefinitions.Contains(subsequentStageIndex) || subsequentStageIndex == stage1Index)
        {
            subsequentStageIndex = UnityEngine.Random.Range(0,42);
        }
        selectedDefinitions.Add(subsequentStageIndex);
        displayedText.text = definitions[subsequentStageIndex];

        foreach(Button label in buttons)
        {
            int index = UnityEngine.Random.Range(0,42);
            while(selectedWords.Contains(index))
            {
                index = UnityEngine.Random.Range(0,42);
            }
            selectedWords.Add(index);
            label.text.text = words[index];
        }
        selectedWords.Clear();
        if(buttons[0].text.text != correctAnswer[stage-1] && buttons[1].text.text != correctAnswer[stage-1] && buttons[2].text.text != correctAnswer[stage-1] && buttons[3].text.text != correctAnswer[stage-1])
        {
            int correctButtonIndex = UnityEngine.Random.Range(0,4);
            buttons[correctButtonIndex].text.text = correctAnswer[stage-1];
        }
        correctAnswer[stage] = words[subsequentStageIndex];
        Debug.LogFormat("[British Slang #{0}] Pip pip! The new definition is '{1}!'", moduleId, displayedText.text);
        Debug.LogFormat("[British Slang #{0}] Tally-ho! The correct word is '{1}!'", moduleId, correctAnswer[stage-1]);
    }

    public void OnButtonPress(Button button)
    {
        if(moduleSolved)
        {
            return;
        }
        button.selectable.AddInteractionPunch(0.5f);
        switch (stage)
        {
            case 0:
            if(button.text.text == "")
            {
                Debug.LogFormat("[British Slang #{0}] Spiffing! You pressed '{1}!' That's the one!", moduleId, button.text.text);
                stage++;
                Audio.PlaySoundAtTransform("correct", transform);
                marker.text = "|";
                subsequentStageSelection();
            }
            else
            {
                Debug.LogFormat("[British Slang #{0}] Strike! You pressed '{1}!' Dang and blast!", moduleId, button.text.text);
                GetComponent<KMBombModule>().HandleStrike();
                selectedDefinitions.Clear();
                stage = 0;
                marker.text = "";
                stage1Selection();
            }
            break;

            default:
            if(stage < 5)
                {
                if(button.text.text == correctAnswer[stage-1])
                {
                    Debug.LogFormat("[British Slang #{0}] Spiffing! You pressed '{1}!' That's the one!", moduleId, button.text.text);
                    stage++;
                    Audio.PlaySoundAtTransform("correct", transform);
                    marker.text += "|";
                    subsequentStageSelection();
                }
                else
                {
                    Debug.LogFormat("[British Slang #{0}] Strike! You pressed '{1}!' Dang and blast!", moduleId, button.text.text);
                    GetComponent<KMBombModule>().HandleStrike();
                    selectedDefinitions.Clear();
                    stage = 0;
                    marker.text = "";
                    stage1Selection();
                }
            }
            else
            {
                if(button.text.text == correctAnswer[stage-1])
                {
                    Debug.LogFormat("[British Slang #{0}] Spiffing! You pressed '{1}!' That's the one! Time for tea and scones! Module solved.", moduleId, button.text.text);
                    Audio.PlaySoundAtTransform("pass", transform);
                    moduleSolved = true;
                    marker.text += "|";
                    displayedText.text = "Jolly good show!";
                    buttonsObject.SetActive(false);
                    GetComponent<KMBombModule>().HandlePass();
                }
                else
                {
                    Debug.LogFormat("[British Slang #{0}] Strike! You pressed '{1}!' Dang and blast!", moduleId, button.text.text);
                    GetComponent<KMBombModule>().HandleStrike();
                    selectedDefinitions.Clear();
                    stage = 0;
                    marker.text = "";
                    stage1Selection();
                }
            }
            break;
        }
    }

    private string TwitchHelpMessage = @"Use '!{0} press 1' to press a button. Use '!{0} words' to get the words in the chat. The order of the buttons is top, right, bottom, and then left.";

    IEnumerator ProcessTwitchCommand(string command)
    {
        var parts = command.ToLowerInvariant().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length == 2 && parts[0] == "press" && parts[1].Length == 1 && "1234".Contains(parts[1]))
        {
            yield return null;
            OnButtonPress(buttons[Int32.Parse(parts[1]) - 1]);
        }
        else if (parts.Length == 1 && parts[0] == "words")
        {
            string b1 = buttons[0].text.text == "" ? "BLANK" : buttons[0].text.text;
            string b2 = buttons[1].text.text == "" ? "BLANK" : buttons[1].text.text;
            string b3 = buttons[2].text.text == "" ? "BLANK" : buttons[2].text.text;
            string b4 = buttons[3].text.text == "" ? "BLANK" : buttons[3].text.text;
            yield return "sendtochat The words are " + b1 + ", " + b2 + ", " + b3 + ", and " + b4;
            yield break;
        }
    }
}
