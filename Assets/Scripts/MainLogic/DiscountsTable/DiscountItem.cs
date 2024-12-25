using UnityEngine;
using TMPro;
using Michsky.MUIP;
using System;

public class DiscountItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI idText;
    [SerializeField] private TextMeshProUGUI percentText;
    [SerializeField] private TextMeshProUGUI reasonText;
    [SerializeField] private TextMeshProUGUI dateRangeText; 
    [SerializeField] private ButtonManager editButton;
    [SerializeField] private ButtonManager deleteButton;

    public int id;
    public decimal percent;
    public string reason;
    public DateTime? startDate;
    public DateTime? endDate;

    private DiscountsController parentController;
    private NotificationManager errorNotification;

    public void Init(int id, decimal percent, string reason, DateTime? sD, DateTime? eD,
        DiscountsController ctrl, NotificationManager errNotif)
    {
        this.id = id;
        this.percent = percent;
        this.reason = reason;
        this.startDate = sD;
        this.endDate = eD;
        parentController = ctrl;
        errorNotification = errNotif;

        idText.text = id.ToString();
        percentText.text = percent.ToString("F2") + "%";
        reasonText.text = reason;
        string sStr = startDate.HasValue ? startDate.Value.ToString("yyyy-MM-dd") : "-";
        string eStr = endDate.HasValue ? endDate.Value.ToString("yyyy-MM-dd") : "-";
        dateRangeText.text = sStr + " / " + eStr;
    }

    public void OnEditClick()
    {
        parentController.OpenAddDiscountPopup(this, "Редактирование скидки");
    }

    public void OnDeleteClick()
    {
        parentController.ShowDeleteConfirmation(this);
    }
}