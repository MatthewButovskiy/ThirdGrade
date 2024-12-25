using Michsky.MUIP;
using TMPro;
using UnityEngine;
using System.Threading.Tasks;
using Npgsql;
using System;

public class OrderItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI orderIdText;
    [SerializeField] private TextMeshProUGUI orderDateText;
    [SerializeField] private TextMeshProUGUI customerAddressText;
    [SerializeField] private TextMeshProUGUI customerFullNameText;
    
    [SerializeField] private TextMeshProUGUI totalSumText; 
    [SerializeField] private GameObject loaderObject;
    [SerializeField] private GameObject totalPriceArea;
    [SerializeField] private ButtonManager calculateButton; 

    public int id;
    public DateTime orderDate;
    public string customerAddress;
    public string customerLastName;
    public string customerFirstName;
    public string customerPatronymic;

    private OrdersController parentController;
    private NotificationManager errorNotification;
    private DatabaseManager managerDBs;

    public void Init(int id, DateTime oDate, string address,
        string lastName, string firstName, string patronymic,
        OrdersController controller, NotificationManager errorNotif, DatabaseManager managerDB)
    {
        this.id = id;
        this.orderDate = oDate;
        this.customerAddress = address;
        this.customerLastName = lastName;
        this.customerFirstName = firstName;
        this.customerPatronymic = patronymic;
        parentController = controller;
        errorNotification = errorNotif;
        managerDBs = managerDB;
        orderIdText.text = id.ToString();
        orderDateText.text = oDate.ToString("yyyy-MM-dd");
        customerAddressText.text = address;

        string fullName = lastName + " " + firstName;
        if (!string.IsNullOrEmpty(patronymic))
            fullName += " " + patronymic;
        customerFullNameText.text = fullName;
        
        totalSumText.gameObject.SetActive(false);
        loaderObject.SetActive(false);
        calculateButton.gameObject.SetActive(true);
    }
    
    public void OnCalculateTotalClick()
    {
        totalSumText.gameObject.SetActive(false);
        loaderObject.SetActive(true);
        calculateButton.gameObject.SetActive(false);
        ComputeOrderTotalAsync();
    }

    private async void ComputeOrderTotalAsync()
    {
        decimal sum = 0;
        bool success = false;
        string errorMsg = "";
        
        string connStr = managerDBs.GetConnectionString();
        if (string.IsNullOrEmpty(connStr))
        {
            errorMsg = "Нет строки подключения.";
        }
        else
        {
            await Task.Run(() =>
            {
                try
                {
                    using (var c = new NpgsqlConnection(connStr))
                    {
                        c.Open();
                        using (var cmd = new NpgsqlCommand("SELECT techstore.get_order_total(@oid)", c))
                        {
                            cmd.Parameters.AddWithValue("oid", this.id);
                            object obj = cmd.ExecuteScalar();
                            if (obj != null && obj != DBNull.Value)
                                sum = (decimal)obj;
                        }
                    }
                    success = true;
                }
                catch (Exception ex)
                {
                    errorMsg = ex.Message;
                    success = false;
                }
            });
        }
        
        loaderObject.SetActive(false);

        if (!success)
        {
            ShowError("Ошибка при вычислении суммы заказа: " + errorMsg);
        }
        else
        {
            totalSumText.gameObject.SetActive(true);
            totalSumText.text = sum.ToString("F2");
        }
    }

    public void OnEditClick()
    {
        parentController.OpenAddOrderPopup(this, "Редактирование заказа");
    }
    public void OnDeleteClick()
    {
        parentController.ShowDeleteConfirmation(this);
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
