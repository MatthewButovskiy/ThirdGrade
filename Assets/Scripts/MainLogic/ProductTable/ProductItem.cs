using Michsky.MUIP;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ProductItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private TextMeshProUGUI quantityText;
    [SerializeField] private TextMeshProUGUI manufacturerNameHeader;
    [SerializeField] private TextMeshProUGUI manufacturerDateHeader;
    [SerializeField] private ButtonManager editButton;   // Только у AdminPrefab
    [SerializeField] private ButtonManager deleteButton; // Только у AdminPrefab

    public int id;
    public string productName;
    public decimal productPrice;
    public int productQuantity;
    public string manufacturerName;
    public string manufactureDate;
    private ProductsController parentController;
    private NotificationManager errorNotification;

    // Инициализация данных
    public void Init(int id, string name, string manufactorName, decimal price, int quantity, string date, ProductsController controller, NotificationManager errorNotif)
    {
        this.id = id;
        productName = name;
        productPrice = price;
        productQuantity = quantity;
        parentController = controller;
        errorNotification = errorNotif;
        manufactureDate = date;
        manufacturerName = manufactorName;
        manufacturerNameHeader.text = manufacturerName;
        manufacturerDateHeader.text = manufactureDate;
        nameText.text = productName;
        priceText.text = productPrice.ToString("F2");
        quantityText.text = productQuantity.ToString();
    }

    // Нажатие "Редактировать"
    public void OnEditClick()
    {
        parentController.OpenAddProductPopup(this, "Редактирование товара");
    }

    // Нажатие "Удалить"
    public void OnDeleteClick()
    {
        parentController.ShowDeleteConfirmation(this);
    }
}