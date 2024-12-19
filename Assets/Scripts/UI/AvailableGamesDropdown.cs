using System.Collections;
using System.Collections.Generic;
using System.Net;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AvailableGamesDropdown : MonoBehaviour
{
    public static AvailableGamesDropdown instance;
    private Dictionary<string, IPAddress> optionValues = new Dictionary<string, IPAddress>();
    public TMP_Dropdown dropdown;

    /*
    Singleton Pattern: Make sure there's only one Chat Window
    */
    void Awake()
    {
        if (instance)
        {
            // Remove if instance exists
            Destroy(gameObject);
            return;
        }

        // No instance yet, set this to it
        instance = this;
    }

    public void AddItem(string entry, IPAddress ipAddress)
    {
        if (optionValues.ContainsKey(entry))
        {
            return;
        }

        if (dropdown.options.Count == 0)
        {
            dropdown.options.Add(new TMP_Dropdown.OptionData("Select a lobby:"));
        }
        
        optionValues.Add(entry, ipAddress);
        dropdown.options.Add(new TMP_Dropdown.OptionData(entry));
        dropdown.value = dropdown.options.Count - 1;
        dropdown.RefreshShownValue();
    }

    public string GetLobbyIP()
    {
        string entry = dropdown.options[dropdown.value].text;
        Debug.Log(entry);
        if (!optionValues.ContainsKey(entry))
        {
            return "";
        }

        string address = optionValues[entry].ToString();
        return address;
    }

    public void ClearItems()
    {
        dropdown.ClearOptions();
        optionValues.Clear();
    }
}
