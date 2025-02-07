using System.Collections.Generic;
using UnityEngine;
using Npgsql;
using TMPro;
using Michsky.MUIP;

public class OrdersController : MonoBehaviour
{
    [SerializeField] private Transform content; 
    [SerializeField] private GameObject orderItemAdminPrefab;   
    [SerializeField] private GameObject orderItemReadOnlyPrefab;
    [SerializeField] private ButtonManager addOrderButton;      
    [SerializeField] private GameObject addOrderPopupPrefab;    
    [SerializeField] private NotificationManager errorNotification;
    [SerializeField] private DeleteConfirmation deleteConfirmationPanelPrefab;
    [SerializeField] private DatabaseManager dbManager;

    private string role;
    private List<GameObject> currentItems = new List<GameObject>();

    public void StartWork()
    {
        role = DatabaseManager.Instance.GetRole();
        bool canAdd = (role == "store_admin" || role == "store_manager" || role == "store_seller");
        addOrderButton.gameObject.SetActive(canAdd);

        LoadOrders();
    }

    private void LoadOrders()
    {
        ClearOrders();
        var conn = DatabaseManager.Instance.GetConnection();

        using (var cmd = new NpgsqlCommand(
            @"SELECT id, order_date, customer_address, 
                     customer_last_name, customer_first_name, customer_patronymic
              FROM techstore.orders
              ORDER BY id", conn))
        using (var reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                int id = reader.GetInt32(0);
                System.DateTime oDate = reader.GetDateTime(1);
                string address = reader.GetString(2);
                string lastName = reader.GetString(3);
                string firstName = reader.GetString(4);
                string patronymic = reader.IsDBNull(5) ? "" : reader.GetString(5);

                GameObject prefabToUse;
                if (role == "store_admin" || role == "store_manager" || role == "store_seller")
                    prefabToUse = orderItemAdminPrefab;
                else
                    prefabToUse = orderItemReadOnlyPrefab;

                GameObject itemGO = Instantiate(prefabToUse, content);
                currentItems.Add(itemGO);

                var orderItem = itemGO.GetComponent<OrderItem>();
                orderItem.Init(id, oDate, address, lastName, firstName, patronymic, this, errorNotification, dbManager);
            }
        }
    }

    private void ClearOrders()
    {
        foreach (var go in currentItems)
        {
            Destroy(go);
        }
        currentItems.Clear();
    }

    public void OnAddOrderClick()
    {
        OpenAddOrderPopup(null, "Добавление заказа");
    }

    public void OpenAddOrderPopup(OrderItem orderToEdit, string popupTitle)
    {
        var popupGO = Instantiate(addOrderPopupPrefab, transform.parent);
        var popup = popupGO.GetComponent<AddOrderPopup>();
        popup.Initialize(this, orderToEdit, errorNotification, popupTitle);
    }

    public void RefreshList()
    {
        LoadOrders();
    }

    public void ShowDeleteConfirmation(OrderItem orderItem)
    {
        var delConfirm = Instantiate(deleteConfirmationPanelPrefab, transform.parent);
        delConfirm.Initialize(() => DeleteOrder(orderItem.id));
    }

    public void DeleteOrder(int orderId)
    {
        var conn = DatabaseManager.Instance.GetConnection();
        using (var cmd = new NpgsqlCommand("DELETE FROM techstore.orders WHERE id=@id", conn))
        {
            cmd.Parameters.AddWithValue("id", orderId);
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (System.Exception ex)
            {
                ShowError("Ошибка при удалении: " + ex.Message);
                return;
            }
        }
        RefreshList();
    }

    private void ShowError(string msg)
    {
        errorNotification.description = msg;
        errorNotification.UpdateUI();
        errorNotification.Open();
    }
}
