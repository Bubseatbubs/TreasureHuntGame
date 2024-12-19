using UnityEngine;
using TMPro;

public class ConnectToHostUI : MonoBehaviour
{
    [SerializeField]
    private GameObject connectMenu;

    [SerializeField]
    private TMP_InputField IPAddressInput;

    [SerializeField]
    private TMP_InputField PortInput;

    [SerializeField]
    private TMP_InputField UsernameInput;

    [SerializeField]
    private GameObject networkButtons;

    [SerializeField]
    private GameObject waitForHostPanel;

    [SerializeField]
    private GameObject tutorialPanel;

    [SerializeField]
    private TMP_Dropdown availableGamesDropdown;

    [SerializeField]
    private TMP_Text connectTypeButtonText;

    [SerializeField]
    private GameObject refreshButton;

    bool connectByLAN = true;


    public void ToggleMenu()
    {
        if (connectMenu != null)
        {
            if (connectMenu.activeSelf)
            {
                connectMenu.SetActive(false);
            }
            else
            {
                connectMenu.SetActive(true);
                if (connectByLAN)
                {
                    NetworkController.instance.SearchForGames();
                }
            }
        }
    }

    public void ConnectToHost()
    {
        int port;
        string IPAddress = "";
        if (connectByLAN)
        {
            port = 6077;
            IPAddress = AvailableGamesDropdown.instance.GetLobbyIP();
            if (IPAddress == "")
            {
                Debug.Log("Please select a lobby!");
                return;
            }
        }
        else
        {
            IPAddress = convertFieldToString(IPAddressInput);
            if (int.TryParse(convertFieldToString(PortInput), out port))
            {
                Debug.Log($"Parsed value: {port}");
            }
            else
            {
                Debug.Log("The input could not be parsed as an integer.");
                return;
            }
        }


        string username = convertFieldToString(UsernameInput);
        if (!IsValidUsername(username))
        {
            Debug.Log("Invalid username typed in.");
            return;
        }

        NetworkController networkController = NetworkController.instance;
        bool success = networkController.ConnectToGame(IPAddress, port, username);
        if (!success) return;

        // Set up buttons if connecting was successful
        networkButtons.SetActive(false);
        waitForHostPanel.SetActive(true);
    }

    public void SwapConnectionType()
    {
        if (connectByLAN)
        {
            IPAddressInput.gameObject.SetActive(true);
            PortInput.gameObject.SetActive(true);
            availableGamesDropdown.gameObject.SetActive(false);
            refreshButton.SetActive(false);
            connectTypeButtonText.text = "Direct Connect";
        }
        else
        {
            IPAddressInput.gameObject.SetActive(false);
            PortInput.gameObject.SetActive(false);
            availableGamesDropdown.gameObject.SetActive(true);
            refreshButton.SetActive(true);
            connectTypeButtonText.text = "Connect Nearby";
        }

        connectByLAN = !connectByLAN;
    }

    private string convertFieldToString(TMP_InputField field)
    {
        string text = field.text;
        return text;
    }

    private static bool IsValidUsername(string s)
    {
        if (s.Length > 12 || s.Length <= 0)
        {
            return false;
        }

        foreach (var c in s)
        {
            if (!char.IsLetterOrDigit(c))
            {
                return false;
            }
        }

        return true;
    }
}
