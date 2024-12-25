using Michsky.MUIP;
using TMPro;
using UnityEngine;

public class OrderItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI orderIdText;
    [SerializeField] private TextMeshProUGUI orderDateText;
    [SerializeField] private TextMeshProUGUI customerAddressText;
    [SerializeField] private TextMeshProUGUI customerFullNameText; 
    [SerializeField] private ButtonManager editButton;   // Будет только в AdminPrefab
    [SerializeField] private ButtonManager deleteButton; // Будет только в AdminPrefab

    public int id;
    public System.DateTime orderDate;
    public string customerAddress;
    public string customerLastName;
    public string customerFirstName;
    public string customerPatronymic;

    private OrdersController parentController;
    private NotificationManager errorNotification;

    public void Init(int id, System.DateTime oDate, string address, 
        string lastName, string firstName, string patronymic,
        OrdersController controller, NotificationManager errorNotif)
    {
        this.id = id;
        this.orderDate = oDate;
        this.customerAddress = address;
        this.customerLastName = lastName;
        this.customerFirstName = firstName;
        this.customerPatronymic = patronymic;
        parentController = controller;
        errorNotification = errorNotif;

        orderIdText.text = id.ToString();
        orderDateText.text = oDate.ToString("yyyy-MM-dd");
        customerAddressText.text = address;

        string fullName = lastName + " " + firstName;
        if (!string.IsNullOrEmpty(patronymic))
            fullName += " " + patronymic;

        customerFullNameText.text = fullName;
    }

    public void OnEditClick()
    {
        parentController.OpenAddOrderPopup(this, "Редактирование заказа");
    }

    public void OnDeleteClick()
    {
        parentController.ShowDeleteConfirmation(this);
    }
}