using System;
using Michsky.MUIP;
using Npgsql;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class AddProductPopup : MonoBehaviour
{
    [SerializeField] private TMP_InputField nameField;
    [SerializeField] private TMP_Text topName;
    [SerializeField] private TMP_InputField manufacturerNameField;
    [SerializeField] private TMP_InputField manufactureDateField;
    [SerializeField] private TMP_InputField priceField;
    [SerializeField] private TMP_InputField quantityField;
    [SerializeField] private TMP_InputField categoryIdField;
    [SerializeField] private TMP_InputField manufacturerIdField;
    [SerializeField] private NotificationManager errorNotification;

    private ProductsController parentController;
    private ProductItem editingProduct;
    private bool isEditMode = false;

    public void Initialize(ProductsController controller, ProductItem productToEdit, string topNameField)
    {
        parentController = controller;
        topName.text = topNameField;
        if (productToEdit != null)
        {
            isEditMode = true;
            editingProduct = productToEdit;
            nameField.text = editingProduct.productName;
            manufacturerNameField.text = editingProduct.manufacturerName;
            if (editingProduct.manufactureDate.Equals(String.Empty))
                manufactureDateField.text =
                    "2023-08-15"; 
            else
                manufactureDateField.text = editingProduct.manufactureDate;
            priceField.text = editingProduct.productPrice.ToString("F2");
            quantityField.text = editingProduct.productQuantity.ToString();
            categoryIdField.text = "1";
            manufacturerIdField.text = "1";
        }
        else
        {
            isEditMode = false;
            editingProduct = null;
            nameField.text = "";
            manufacturerNameField.text = "";
            manufactureDateField.text = "2023-08-15";
            priceField.text = "";
            quantityField.text = "";
            categoryIdField.text = "1";
            manufacturerIdField.text = "1";
        }
    }

    public void OnConfirmClick()
    {
        // Считываем
        string newName = nameField.text.Trim();
        string newManufName = manufacturerNameField.text.Trim();
        string dateStr = manufactureDateField.text.Trim();
        string priceStr = priceField.text.Trim();
        string qtyStr = quantityField.text.Trim();
        string catStr = categoryIdField.text.Trim();
        string manufIDStr = manufacturerIdField.text.Trim();

        // Валидируем
        if (string.IsNullOrEmpty(newName))
        {
            ShowError("Название товара не может быть пустым.");
            return;
        }
        if (string.IsNullOrEmpty(newManufName))
        {
            ShowError("Название производителя не может быть пустым.");
            return;
        }
        if (!System.DateTime.TryParse(dateStr, out var manDate))
        {
            ShowError("Некорректная дата изготовления. Используйте формат ГГГГ-ММ-ДД.");
            return;
        }
        if (!decimal.TryParse(priceStr, out decimal newPrice) || newPrice <= 0)
        {
            ShowError("Некорректная цена. Должна быть число больше 0");
            return;
        }
        if (!int.TryParse(qtyStr, out int newQty) || newQty <= 0)
        {
            ShowError("Некорректное количество. Целое число больше 0");
            return;
        }
        if (!int.TryParse(catStr, out int newCatID) || newCatID <= 0)
        {
            ShowError("Неверная категория. Введите ID категории.");
            return;
        }
        if (!int.TryParse(manufIDStr, out int newManufID) || newManufID <= 0)
        {
            ShowError("Неверный ID производителя.");
            return;
        }
        
        decimal totalAmt = newPrice * newQty;

        var conn = DatabaseManager.Instance.GetConnection();
        try
        {
            if (isEditMode)
            {
                // UPDATE
                using (var cmd = new NpgsqlCommand(@"UPDATE techstore.products 
                  SET name=@n, manufacturer_name=@mn, manufacture_date=@md, price=@p, quantity=@q, total_amount=@ta, category_id=@cat, manufacturer_id=@mid 
                  WHERE id=@id", conn))
                {
                    cmd.Parameters.AddWithValue("n", newName);
                    cmd.Parameters.AddWithValue("mn", newManufName);
                    cmd.Parameters.AddWithValue("md", manDate);
                    cmd.Parameters.AddWithValue("p", newPrice);
                    cmd.Parameters.AddWithValue("q", newQty);
                    cmd.Parameters.AddWithValue("ta", totalAmt);
                    cmd.Parameters.AddWithValue("cat", newCatID);
                    cmd.Parameters.AddWithValue("mid", newManufID);
                    cmd.Parameters.AddWithValue("id", editingProduct.id);
                    cmd.ExecuteNonQuery();
                }
            }
            else
            {
                // INSERT
                using (var cmd = new NpgsqlCommand(@"INSERT INTO techstore.products
                (name, manufacturer_name, manufacture_date, price, quantity, total_amount, category_id, manufacturer_id)
                VALUES(@n,@mn,@md,@p,@q,@ta,@cat,@mid)", conn))
                {
                    cmd.Parameters.AddWithValue("n", newName);
                    cmd.Parameters.AddWithValue("mn", newManufName);
                    cmd.Parameters.AddWithValue("md", manDate);
                    cmd.Parameters.AddWithValue("p", newPrice);
                    cmd.Parameters.AddWithValue("q", newQty);
                    cmd.Parameters.AddWithValue("ta", totalAmt);
                    cmd.Parameters.AddWithValue("cat", newCatID);
                    cmd.Parameters.AddWithValue("mid", newManufID);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        catch (System.Exception ex)
        {
            ShowError("Ошибка при сохранении: " + ex.Message);
            return;
        }

        parentController.RefreshList();
        Destroy(gameObject);
    }

    public void OnCancelClick()
    {
        Destroy(gameObject);
    }

    private void ShowError(string message)
    {
        errorNotification.title = "Ошибка";
        errorNotification.description = message;
        errorNotification.UpdateUI();
        errorNotification.Open();
    }
}
