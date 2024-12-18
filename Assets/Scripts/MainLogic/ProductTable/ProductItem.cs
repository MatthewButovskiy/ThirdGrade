using Michsky.MUIP;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ProductItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private TextMeshProUGUI quantityText;
    [SerializeField] private ButtonManager editButton;   // Есть только в AdminPrefab
    [SerializeField] private ButtonManager deleteButton; // Есть только в AdminPrefab

    public int id;
    public string productName;
    public decimal productPrice;
    public int productQuantity;

    private ProductsController parentController;
    private NotificationManager errorNotification;

    public void Init(int id, string name, decimal price, int quantity, ProductsController controller, NotificationManager errorNotif)
    {
        this.id = id;
        productName = name;
        productPrice = price;
        productQuantity = quantity;
        parentController = controller;
        errorNotification = errorNotif;

        nameText.text = name;
        priceText.text = price.ToString("F2");
        quantityText.text = quantity.ToString();
    }

    public void OnEditClick()
    {
        parentController.OpenAddProductPopup(this);
    }

    public void OnDeleteClick()
    {
        // Удаляем товар
        parentController.DeleteProduct(id);
    }
}