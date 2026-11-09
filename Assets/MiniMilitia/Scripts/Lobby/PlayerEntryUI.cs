using UnityEngine;
using TMPro;
public class PlayerEntryUI : MonoBehaviour
{
    [Header("Player UI")]
    [SerializeField] private TMP_Text playerNameText;
    //[SerializeField] private TMP_Text roleText;
    [SerializeField] private TMP_Text readyText;
        

    public void SetupUI(string name, bool isReady)
    {
        playerNameText.text = name;
        readyText.text = isReady ? "Ready" : "Not Ready";
    }
}
