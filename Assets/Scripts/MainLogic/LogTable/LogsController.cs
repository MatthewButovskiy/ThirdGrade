using UnityEngine;
using TMPro;
using Npgsql;
using System.Collections.Generic;
using Michsky.MUIP;

public class LogsController : MonoBehaviour
{
    [SerializeField] private Transform content; 
    [SerializeField] private GameObject logItemPrefab; 
    [SerializeField] private NotificationManager errorNotification; 

    private bool hasStarted = false;
    
    public void StartWork()
    {
        if (!hasStarted) 
        {
            hasStarted = true;
            LoadLogs();
        }
        else
        {
            LoadLogs();
        }
    }

    private void LoadLogs()
    {
        ClearLogs();
        var conn = DatabaseManager.Instance.GetConnection();
        if (conn == null)
        {
            ShowError("Нет соединения с БД");
            return;
        }
        
        string sql = @"
            SELECT id, user_role, operation_time, operation_type, table_name, record_id, description
            FROM techstore.action_log
            ORDER BY id DESC";

        try
        {
            using (var cmd = new NpgsqlCommand(sql, conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    int logId = reader.GetInt32(0);
                    string userRole = reader.IsDBNull(1) ? "" : reader.GetString(1);
                    System.DateTime opTime = reader.GetDateTime(2);
                    string opType = reader.GetString(3);
                    string tabName = reader.IsDBNull(4) ? "" : reader.GetString(4);
                    int recId = reader.IsDBNull(5) ? 0 : reader.GetInt32(5);
                    string desc = reader.IsDBNull(6) ? "" : reader.GetString(6);

                    // Спавним элемент UI
                    GameObject itemGO = Instantiate(logItemPrefab, content);
                    var logItem = itemGO.GetComponent<LogItem>();
                    logItem.SetData(logId, userRole, opTime, opType, tabName, recId, desc);
                }
            }
        }
        catch (System.Exception ex)
        {
            ShowError("Ошибка загрузки логов: " + ex.Message);
        }
    }

    private void ClearLogs()
    {
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }
    }

    private void ShowError(string msg)
    {
        if (errorNotification != null)
        {
            errorNotification.description = msg;
            errorNotification.UpdateUI();
            errorNotification.Open();
        }
        else
        {
            Debug.LogError(msg);
        }
    }
}
