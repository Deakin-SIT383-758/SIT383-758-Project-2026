using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using TMPro.EditorUtilities;
using TMPro;
using System.Runtime.InteropServices.WindowsRuntime;

public class ChecklistManager : MonoBehaviour
{
    public static ChecklistManager Instance { get; set; }
    public class ChecklistItem
    {
        public ChecklistItem(string ins, string cName, float cValue, float mar)
        {
            instruction = ins;
            controlName = cName;
            controlValue = cValue;
            margin = mar;
            completed = false;
        }

        public bool isComplete(string cName, float cValue)
        {
            if (!(cName == controlName)) return false;
            completed = (Mathf.Abs(controlValue - cValue) <= margin) ? true : false;
            return completed;
        }

        public string instruction { get; } // Text to display to the user for the item
        public string controlName { get; } // Name of the relevant control
        private float controlValue { get; } // Required value to complete item
        private float margin { get; } // Allowable delta from control value to return completed
        public bool completed { get; set; } // Has this item been completed?
    }

    public String testControlName = "Engine Ignition";
    public float testControlValue = 0.0f;

    private List<ChecklistItem> checklistItems = new List<ChecklistItem>();
    // To create checklist items in inspector, type instruction,
    // controlName, controlValue and margin, separate by commas
    // One checklist item per string in the list
    public List<string> checklistInformation = new List<string>();
    private int currentItem = 0; // track current item in the list

    public TMP_Text textBox;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
        ConstructChecklist();
        UpdateTextDisplay();
    }

    // Update is called once per frame
    void Update()
    {

    }

    // Parse strings from checklistInformation to populate checklistItems
    void ConstructChecklist()
    {
        foreach (string s in checklistInformation)
        {
            string[] itemInfo = s.Split(",");
            if (itemInfo.Count() != 4) Debug.LogError("CHECKLIST ERROR: item has incorrect number of values");
            else
            {
                ChecklistItem i = new ChecklistItem(itemInfo[0], itemInfo[1], float.Parse(itemInfo[2]), float.Parse(itemInfo[3]));
                checklistItems.Add(i);
            }
        }
    }

    public void UpdateTextDisplay()
    {
        textBox.text = "";
        foreach (ChecklistItem i in checklistItems)
        {
            string itemString = (i.completed ? "☒" : "☐") + ": " + i.instruction + "\n";
            textBox.text += itemString;
        }
    }

    [ContextMenu("Test control update")]
    public void TestControlUpdate()
    {
        ControlUpdate(testControlName, testControlValue);
    }

    public void ControlUpdate(string cName, float cValue)
    {
        // Check against current item
        if (checklistItems[currentItem].isComplete(cName, cValue) && (currentItem != checklistItems.Count() - 1))
        {
            Debug.Log($"Checklist item {currentItem} completed");
            currentItem += 1; // go to next item
            UpdateTextDisplay();
        }
        else
        {
            // Check if previous items have been undone
            if (currentItem == 0) return;
            for (int i = 0; i <= 0; i++)
            {
                // If control name matches a previous step but value is incorrect
                if (cName == checklistItems[i].controlName && (!checklistItems[i].isComplete(cName, cValue)))
                {
                    Debug.Log("Previous step undone, updating checklist");
                    for (int j = i; j <= currentItem; j++)
                    {
                        checklistItems[j].completed = false; // uncheck subsequent steps
                    }
                }
            }
            UpdateTextDisplay();
        }
    }
}
