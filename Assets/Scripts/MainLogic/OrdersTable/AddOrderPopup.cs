using System;
using UnityEngine;
using TMPro;
using Michsky.MUIP;
using Npgsql;

public class AddOrderPopup : MonoBehaviour
{
    [SerializeField] private TMP_InputField orderDateField;       // yyyy-MM-dd
    [SerializeField] private TMP_InputField addressField;
    [SerializeField] private TMP_InputField lastNameField;
    [SerializeField] private TMP_InputField firstNameField;
    [SerializeField] private TMP_InputField patronymicField;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private NotificationManager errorNotification;

    private OrdersController parentController;
    private OrderItem editingOrder;
    private bool isEditMode = false;

    public void Initialize(OrdersController controller, OrderItem orderToEdit, NotificationManager errorNotif, string popupTitle)
    {
        parentController = controller;
        errorNotification = errorNotif;
            // titleText.text = popupTitle;

        if (orderToEdit != null)
        {
            isEditMode = true;
            editingOrder = orderToEdit;

            orderDateField.text = editingOrder.orderDate.ToString("yyyy-MM-dd");
            addressField.text = editingOrder.customerAddress;
            lastNameField.text = editingOrder.customerLastName;
            firstNameField.text = editingOrder.customerFirstName;
            patronymicField.text = editingOrder.customerPatronymic;
        }
        else
        {
            isEditMode = false;
            editingOrder = null;
            orderDateField.text = DateTime.Now.ToString("yyyy-MM-dd");
            addressField.text = "";
            lastNameField.text = "";
            firstNameField.text = "";
            patronymicField.text = "";
        }
    }

    public void OnConfirmClick()
    {
        string dateStr = orderDateField.text.Trim();
        string address = addressField.text.Trim();
        string lastName = lastNameField.text.Trim();
        string firstName = firstNameField.text.Trim();
        string patronymic = patronymicField.text.Trim();

        if (!DateTime.TryParse(dateStr, out DateTime orderDate))
        {
            ShowError("Некорректная дата. Используйте формат ГГГГ-ММ-ДД");
            return;
        }
        if (string.IsNullOrEmpty(address))
        {
            ShowError("Адрес не может быть пустым.");
            return;
        }
        if (string.IsNullOrEmpty(lastName))
        {
            ShowError("Фамилия не может быть пустой.");
            return;
        }
        if (string.IsNullOrEmpty(firstName))
        {
            ShowError("Имя не может быть пустым.");
            return;
        }
        // patronymicField может быть пустым (NULL)

        var conn = DatabaseManager.Instance.GetConnection();
        try
        {
            if (isEditMode)
            {
                using (var cmd = new NpgsqlCommand(
                  @"UPDATE techstore.orders
                    SET order_date=@dt, customer_address=@adr,
                        customer_last_name=@ln, customer_first_name=@fn, customer_patronymic=@pn
                    WHERE id=@id", conn))
                {
                    cmd.Parameters.AddWithValue("dt", orderDate);
                    cmd.Parameters.AddWithValue("adr", address);
                    cmd.Parameters.AddWithValue("ln", lastName);
                    cmd.Parameters.AddWithValue("fn", firstName);
                    if (!string.IsNullOrEmpty(patronymic))
                        cmd.Parameters.AddWithValue("pn", patronymic);
                    else
                        cmd.Parameters.AddWithValue("pn", DBNull.Value);

                    cmd.Parameters.AddWithValue("id", editingOrder.id);
                    cmd.ExecuteNonQuery();
                }
            }
            else
            {
                using (var cmd = new NpgsqlCommand(
                  @"INSERT INTO techstore.orders(order_date, customer_address, customer_last_name, customer_first_name, customer_patronymic)
                    VALUES(@dt, @adr, @ln, @fn, @pn)", conn))
                {
                    cmd.Parameters.AddWithValue("dt", orderDate);
                    cmd.Parameters.AddWithValue("adr", address);
                    cmd.Parameters.AddWithValue("ln", lastName);
                    cmd.Parameters.AddWithValue("fn", firstName);
                    if (!string.IsNullOrEmpty(patronymic))
                        cmd.Parameters.AddWithValue("pn", patronymic);
                    else
                        cmd.Parameters.AddWithValue("pn", DBNull.Value);

                    cmd.ExecuteNonQuery();
                }
            }
        }
        catch (Exception ex)
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

    private void ShowError(string msg)
    {
        errorNotification.description = msg;
        errorNotification.UpdateUI();
        errorNotification.Open();
    }
}
