using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Michsky.MUIP; 
using Npgsql;

public class AddOrderPopup : MonoBehaviour
{
    [Header("Tab1: Order Info")]
    [SerializeField] private GameObject panelOrderInfo;
    [SerializeField] private TMP_InputField orderDateField;
    [SerializeField] private TMP_InputField addressField;
    [SerializeField] private TMP_InputField lastNameField;
    [SerializeField] private TMP_InputField firstNameField;
    [SerializeField] private TMP_InputField patronymicField;

    [Header("Tab2: Order Lines")]
    [SerializeField] private GameObject panelOrderLines;
    [SerializeField] private TMP_Dropdown productDropdown;
    [SerializeField] private TMP_InputField qtyField;
    [SerializeField] private Transform linesContent;
    [SerializeField] private GameObject linePrefab;

    [Header("General UI")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private NotificationManager errorNotification;

    private OrdersController parentController;
    private OrderItem editingOrder;
    private bool isEditMode;

    private List<OrderLineTemp> lines = new List<OrderLineTemp>();

    public NotificationManager GetErrorNotification()
    {
        return errorNotification;
    }

    public void Initialize(OrdersController controller, OrderItem orderToEdit, NotificationManager errorNotif, string popupTitle)
    {
        parentController = controller;
        //errorNotification = errorNotif;
        titleText.text = popupTitle;

        LoadProductsIntoDropdown();

        if (orderToEdit != null)
        {
            isEditMode = true;
            editingOrder = orderToEdit;

            orderDateField.text = editingOrder.orderDate.ToString("yyyy-MM-dd");
            addressField.text = editingOrder.customerAddress;
            lastNameField.text = editingOrder.customerLastName;
            firstNameField.text = editingOrder.customerFirstName;
            patronymicField.text = editingOrder.customerPatronymic;

            lines.Clear();
            LoadOrderLinesFromDB(editingOrder.id);
            RefreshLinesUI();
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

            lines.Clear();
            RefreshLinesUI();
        }

        ShowTab1();
    }

    private void LoadProductsIntoDropdown()
    {
        productDropdown.ClearOptions();
        var conn = DatabaseManager.Instance.GetConnection();
        using (var cmd = new NpgsqlCommand("SELECT id, name FROM techstore.products ORDER BY id", conn))
        using (var reader = cmd.ExecuteReader())
        {
            List<string> options = new List<string>();
            while (reader.Read())
            {
                int pid = reader.GetInt32(0);
                string pname = reader.GetString(1);
                options.Add(pid + " - " + pname);
            }
            productDropdown.AddOptions(options);
        }
        productDropdown.value = 0;
        productDropdown.RefreshShownValue();
    }

    private void LoadOrderLinesFromDB(int orderId)
    {
        var conn = DatabaseManager.Instance.GetConnection();
        using (var cmd = new NpgsqlCommand(@"
            SELECT op.product_id, p.name, op.quantity
            FROM techstore.order_products op
            JOIN techstore.products p ON p.id=op.product_id
            WHERE op.order_id=@oid", conn))
        {
            cmd.Parameters.AddWithValue("oid", orderId);
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    int pid = reader.GetInt32(0);
                    string pname = reader.GetString(1);
                    int qty = reader.GetInt32(2);
                    lines.Add(new OrderLineTemp{
                        productId=pid,
                        productName=pname,
                        quantity=qty
                    });
                }
            }
        }
    }

    public void OnNextTabClick()
    {
        // Validate Tab1
        if (!DateTime.TryParse(orderDateField.text.Trim(), out _))
        {
            ShowError("Некорректная дата");
            return;
        }
        if (string.IsNullOrEmpty(addressField.text.Trim()))
        {
            ShowError("Адрес пуст");
            return;
        }
        if (string.IsNullOrEmpty(lastNameField.text.Trim()) || string.IsNullOrEmpty(firstNameField.text.Trim()))
        {
            ShowError("Фамилия или имя пусты");
            return;
        }
        ShowTab2();
    }

    public void OnPrevTabClick()
    {
        ShowTab1();
    }

    void ShowTab1()
    {
        panelOrderInfo.SetActive(true);
        panelOrderLines.SetActive(false);
    }

    void ShowTab2()
    {
        panelOrderInfo.SetActive(false);
        panelOrderLines.SetActive(true);
    }

    public void OnAddLineClick()
    {
        int selIndex = productDropdown.value;
        if (selIndex < 0 || selIndex >= productDropdown.options.Count)
        {
            ShowError("Не выбран товар!");
            return;
        }
        string selectedText = productDropdown.options[selIndex].text;
        string[] parts = selectedText.Split('-');
        if (!int.TryParse(parts[0].Trim(), out int pid))
        {
            ShowError("Не удалось считать ID");
            return;
        }
        string pname = (parts.Length>1)? parts[1].Trim():"Unknown Product";

        if (!int.TryParse(qtyField.text.Trim(), out int q) || q<=0)
        {
            ShowError("Некорректное количество");
            return;
        }

        lines.Add(new OrderLineTemp{
            productId=pid, productName=pname, quantity=q
        });
        RefreshLinesUI();
    }

    public void RemoveLine(OrderLineTemp line)
    {
        lines.Remove(line);
        RefreshLinesUI();
    }

    public void OnLineQuantityChanged(OrderLineTemp line)
    {
        if (isEditMode && editingOrder != null)
        {
            var conn = DatabaseManager.Instance.GetConnection();
            try
            {
                using (var cmd = new NpgsqlCommand(@"
                    UPDATE techstore.order_products
                    SET quantity=@q
                    WHERE order_id=@oid AND product_id=@pid", conn))
                {
                    cmd.Parameters.AddWithValue("q", line.quantity);
                    cmd.Parameters.AddWithValue("oid", editingOrder.id);
                    cmd.Parameters.AddWithValue("pid", line.productId);

                    int rows = cmd.ExecuteNonQuery();
                    if (rows == 0)
                    {
                        ShowError($"Не удалось обновить, нет записи productId={line.productId} в БД.");
                    }
                }
            }
            catch (System.Exception ex)
            {
                ShowError("Ошибка при обновлении товара: " + ex.Message);
            }
        }
    }

    void RefreshLinesUI()
    {
        foreach (Transform t in linesContent)
            Destroy(t.gameObject);

        foreach (var line in lines)
        {
            GameObject go = Instantiate(linePrefab, linesContent);
            var cartLine = go.GetComponent<CartLine>();
            cartLine.Init(this, line);
        }
    }

    public void OnConfirmClick()
    {
        if (!DateTime.TryParse(orderDateField.text.Trim(), out DateTime oDate))
        {
            ShowError("Некорректная дата");
            return;
        }
        string addr = addressField.text.Trim();
        string lName = lastNameField.text.Trim();
        string fName = firstNameField.text.Trim();
        string pName = patronymicField.text.Trim();
        if (string.IsNullOrEmpty(addr) || string.IsNullOrEmpty(lName) || string.IsNullOrEmpty(fName))
        {
            ShowError("Адрес/Фамилия/Имя не должны быть пусты");
            return;
        }
        if (lines.Count==0)
        {
            ShowError("Не выбрано ни одного товара!");
            return;
        }

        var conn = DatabaseManager.Instance.GetConnection();
        int orderId=0;

        try
        {
            if (isEditMode && editingOrder!=null)
            {
                orderId = editingOrder.id;
              
                using (var cmd = new NpgsqlCommand(@"
                  UPDATE techstore.orders
                  SET order_date=@dt, customer_address=@adr,
                      customer_last_name=@ln, customer_first_name=@fn, customer_patronymic=@pn
                  WHERE id=@id", conn))
                {
                    cmd.Parameters.AddWithValue("dt", oDate);
                    cmd.Parameters.AddWithValue("adr", addr);
                    cmd.Parameters.AddWithValue("ln", lName);
                    cmd.Parameters.AddWithValue("fn", fName);
                    cmd.Parameters.AddWithValue("pn", string.IsNullOrEmpty(pName)? (object)DBNull.Value: pName);
                    cmd.Parameters.AddWithValue("id", orderId);
                    cmd.ExecuteNonQuery();
                }

            
                using (var cmd = new NpgsqlCommand(
                  "DELETE FROM techstore.order_products WHERE order_id=@oid", conn))
                {
                    cmd.Parameters.AddWithValue("oid", orderId);
                    cmd.ExecuteNonQuery();
                }
           
                foreach (var line in lines)
                {
                    using (var cmd = new NpgsqlCommand(@"
                      INSERT INTO techstore.order_products(product_id, order_id, quantity)
                      VALUES(@pid, @oid, @qty)", conn))
                    {
                        cmd.Parameters.AddWithValue("pid", line.productId);
                        cmd.Parameters.AddWithValue("oid", orderId);
                        cmd.Parameters.AddWithValue("qty", line.quantity);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            else
            {

                using (var cmd = new NpgsqlCommand(@"
                  INSERT INTO techstore.orders(order_date, customer_address, 
                    customer_last_name, customer_first_name, customer_patronymic)
                  VALUES(@dt,@adr,@ln,@fn,@pn) RETURNING id", conn))
                {
                    cmd.Parameters.AddWithValue("dt", oDate);
                    cmd.Parameters.AddWithValue("adr", addr);
                    cmd.Parameters.AddWithValue("ln", lName);
                    cmd.Parameters.AddWithValue("fn", fName);
                    cmd.Parameters.AddWithValue("pn", string.IsNullOrEmpty(pName)? (object)DBNull.Value: pName);
                    object res = cmd.ExecuteScalar();
                    orderId = Convert.ToInt32(res);
                }

                foreach (var line in lines)
                {
                    using (var cmd = new NpgsqlCommand(@"
                      INSERT INTO techstore.order_products(product_id, order_id, quantity)
                      VALUES(@pid, @oid, @qty)", conn))
                    {
                        cmd.Parameters.AddWithValue("pid", line.productId);
                        cmd.Parameters.AddWithValue("oid", orderId);
                        cmd.Parameters.AddWithValue("qty", line.quantity);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }
        catch(Exception ex)
        {
            ShowError("Ошибка при сохранении заказа: " + ex.Message);
            return;
        }

        // Успешно
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
