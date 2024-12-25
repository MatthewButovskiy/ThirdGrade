using UnityEngine;
using TMPro;
using System;

public class LogItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI idText;
    [SerializeField] private TextMeshProUGUI roleText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI opTypeText;
    [SerializeField] private TextMeshProUGUI tableNameText;
    //[SerializeField] private TextMeshProUGUI recordIdText;
    [SerializeField] private TextMeshProUGUI descText;

    public void SetData(
        int logId,
        string userRole,
        DateTime opTime,
        string opType,
        string tabName,
        int recId,
        string description)
    {
        idText.text = logId.ToString();
        roleText.text = userRole;
        timeText.text = opTime.ToString("yyyy-MM-dd HH:mm:ss");
        opTypeText.text = opType;
        tableNameText.text = tabName;
        //recordIdText.text = recId.ToString();
        descText.text = description;
    }
}