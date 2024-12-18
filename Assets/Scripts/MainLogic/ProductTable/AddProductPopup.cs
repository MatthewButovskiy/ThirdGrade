using System;
using Michsky.MUIP;
using UnityEngine;
using TMPro;
using Npgsql;

public class AddProductPopup : MonoBehaviour
{
    [SerializeField] private TMP_InputField nameField;
    [SerializeField] private TMP_InputField priceField;
    [SerializeField] private TMP_InputField quantityField;
    [SerializeField] private TMP_InputField descriptionField;
    [SerializeField] private NotificationManager errorNotification;

    private ProductsController parentController;
    private ProductItem editingProduct;
    private bool isEditMode = false;

    public void Initialize(ProductsController controller, ProductItem productToEdit, NotificationManager errorNotif)
    {
        parentController = controller;
        errorNotification = errorNotif;

        if (productToEdit != null)
        {
            isEditMode = true;
            editingProduct = productToEdit;

            nameField.text = editingProduct.productName;
            priceField.text = editingProduct.productPrice.ToString("F2");
            quantityField.text = editingProduct.productQuantity.ToString();
        }
        else
        {
            isEditMode = false;
            editingProduct = null;
            // поля пустые для добавления
            nameField.text = "";
            priceField.text = "";
            quantityField.text = "";
            descriptionField.text = "";
        }
    }

    public void OnConfirmClick()
    {
        string newName = nameField.text.Trim();
        string priceStr = priceField.text.Trim();
        string quantityStr = quantityField.text.Trim();
        string desc = descriptionField.text.Trim();

        if (string.IsNullOrEmpty(newName))
        {
            ShowError("Название товара не может быть пустым.");
            return;
        }

        if (!decimal.TryParse(priceStr, out decimal newPrice) || newPrice < 0)
        {
            ShowError("Некорректная цена. Введите число больше или равно 0.");
            return;
        }

        if (!int.TryParse(quantityStr, out int newQuantity) || newQuantity < 0)
        {
            ShowError("Некорректное количество. Введите целое число больше или равно 0.");
            return;
        }

        var conn = DatabaseManager.Instance.GetConnection();

        try
        {
            if (isEditMode)
            {
                using (var cmd = new NpgsqlCommand("UPDATE techstore.products SET name=@n, price=@p, quantity=@q, description=@d WHERE id=@id", conn))
                {
                    cmd.Parameters.AddWithValue("n", newName);
                    cmd.Parameters.AddWithValue("p", newPrice);
                    cmd.Parameters.AddWithValue("q", newQuantity);
                    cmd.Parameters.AddWithValue("d", string.IsNullOrEmpty(desc) ? (object)DBNull.Value : desc);
                    cmd.Parameters.AddWithValue("id", editingProduct.id);
                    cmd.ExecuteNonQuery();
                }
            }
            else
            {
                using (var cmd = new NpgsqlCommand("INSERT INTO techstore.products(name, price, quantity, description) VALUES(@n,@p,@q,@d)", conn))
                {
                    cmd.Parameters.AddWithValue("n", newName);
                    cmd.Parameters.AddWithValue("p", newPrice);
                    cmd.Parameters.AddWithValue("q", newQuantity);
                    cmd.Parameters.AddWithValue("d", string.IsNullOrEmpty(desc) ? (object)DBNull.Value : desc);
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
        errorNotification.description = message;
        errorNotification.UpdateUI();
        errorNotification.Open();
    }
}
