using UnityEngine;
using TMPro;
using Michsky.MUIP;
using Npgsql;
using System;

public class AddDiscountPopup : MonoBehaviour
{
    [SerializeField] private TMP_InputField percentField;
    [SerializeField] private TMP_InputField reasonField;
    [SerializeField] private TMP_InputField startDateField; 
    [SerializeField] private TMP_InputField endDateField;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private NotificationManager errorNotification;

    private DiscountsController parentController;
    private DiscountItem editingItem;
    private bool isEditMode = false;

    public void Initialize(DiscountsController controller, DiscountItem item, NotificationManager notif, string title)
    {
        parentController = controller;
        editingItem = item;
        //errorNotification = notif;
        titleText.text = title;

        if (item != null)
        {
            isEditMode = true;
            percentField.text = item.percent.ToString("F2");
            reasonField.text = item.reason;
            startDateField.text = item.startDate.HasValue ? item.startDate.Value.ToString("yyyy-MM-dd") : "";
            endDateField.text = item.endDate.HasValue ? item.endDate.Value.ToString("yyyy-MM-dd") : "";
        }
        else
        {
            isEditMode = false;
            percentField.text = "";
            reasonField.text = "";
            startDateField.text = "";
            endDateField.text = "";
        }
    }

    public void OnConfirmClick()
    {
        string percStr = percentField.text.Trim();
        string reason = reasonField.text.Trim();
        string sDateStr = startDateField.text.Trim();
        string eDateStr = endDateField.text.Trim();

        if (!decimal.TryParse(percStr, out decimal perc) || perc < 1 || perc > 100)
        {
            ShowError("Некорректный процент скидки (1..100).");
            return;
        }

        DateTime? sDt = null;
        DateTime? eDt = null;
        if (!string.IsNullOrEmpty(sDateStr))
        {
            if (!DateTime.TryParse(sDateStr, out var parsedS))
            {
                ShowError("Некорректная дата начала.");
                return;
            }
            sDt = parsedS;
        }
        if (!string.IsNullOrEmpty(eDateStr))
        {
            if (!DateTime.TryParse(eDateStr, out var parsedE))
            {
                ShowError("Некорректная дата окончания.");
                return;
            }
            eDt = parsedE;
        }

        var conn = DatabaseManager.Instance.GetConnection();
        if (conn == null)
        {
            ShowError("Нет соединения к БД.");
            return;
        }

        try
        {
            if (isEditMode && editingItem != null)
            {
                // UPDATE
                using (var cmd = new NpgsqlCommand(@"
                UPDATE techstore.discounts
                SET percent=@p, reason=@r, start_date=@sd, end_date=@ed
                WHERE id=@id", conn))
                {
                    cmd.Parameters.AddWithValue("p", perc);
                    cmd.Parameters.AddWithValue("r", reason);
                    cmd.Parameters.AddWithValue("sd", sDt.HasValue ? (object)sDt.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("ed", eDt.HasValue ? (object)eDt.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("id", editingItem.id);
                    cmd.ExecuteNonQuery();
                }
            }
            else
            {
                // INSERT
                using (var cmd = new NpgsqlCommand(@"
                INSERT INTO techstore.discounts (percent, reason, start_date, end_date)
                VALUES(@p, @r, @sd, @ed)", conn))
                {
                    cmd.Parameters.AddWithValue("p", perc);
                    cmd.Parameters.AddWithValue("r", reason);
                    cmd.Parameters.AddWithValue("sd", sDt.HasValue ? (object)sDt.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("ed", eDt.HasValue ? (object)eDt.Value : DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        catch(Exception ex)
        {
            ShowError("Ошибка при сохранении скидки: " + ex.Message);
            return;
        }

        parentController.RefreshList();
        Destroy(gameObject);
    }

    public void OnCancelClick()
    {
        Destroy(gameObject);
    }

    private void ShowError(string msg)
    {
        errorNotification.title = "Ошибка";
        errorNotification.description = msg;
        errorNotification.UpdateUI();
        errorNotification.Open();
    }
}
