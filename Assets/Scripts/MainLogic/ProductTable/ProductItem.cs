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
    [SerializeField] private TextMeshProUGUI IDcategory;
    [SerializeField] private ButtonManager editButton; 
    [SerializeField] private ButtonManager deleteButton;

    public int id;
    public string productName;
    public decimal productPrice;
    public int productQuantity;
    public int productCategoryID;
    public string manufacturerName;
    public string manufactureDate;
    private ProductsController parentController;
    private NotificationManager errorNotification;
    
    public void Init(int id, string name, string manufactorName, decimal price, int quantity, int categoryID, string date, ProductsController controller, NotificationManager errorNotif)
    {
        this.id = id;
        productName = name;
        productPrice = price;
        productQuantity = quantity;
        productCategoryID = categoryID;
        parentController = controller;
        errorNotification = errorNotif;
        manufactureDate = date;
        manufacturerName = manufactorName;
        IDcategory.text = productCategoryID.ToString();
        manufacturerNameHeader.text = manufacturerName;
        manufacturerDateHeader.text = manufactureDate;
        nameText.text = productName;
        priceText.text = productPrice.ToString("F2");
        quantityText.text = productQuantity.ToString();
    }
    
    public void OnEditClick()
    {
        parentController.OpenAddProductPopup(this, "Редактирование товара");
    }
    
    public void OnDeleteClick()
    {
        parentController.ShowDeleteConfirmation(this);
    }
}