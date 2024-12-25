using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Michsky.MUIP;
using Npgsql;

public class DiscountsController : MonoBehaviour
{
    [SerializeField] private Transform content;
    [SerializeField] private GameObject discountItemAdminPrefab;
    [SerializeField] private GameObject discountItemReadOnlyPrefab;
    [SerializeField] private NotificationManager errorNotification;
    [SerializeField] private GameObject addDiscountPopupPrefab;
    [SerializeField] private DeleteConfirmation deleteConfirmationPrefab; 
    [SerializeField] private ButtonManager addDiscountButton;

    private string role;
    private List<GameObject> currentItems = new List<GameObject>();

    public void StartWork()
    {
        role = DatabaseManager.Instance.GetRole();
        bool canAdd = (role == "store_admin" || role == "store_manager");
        addDiscountButton.gameObject.SetActive(canAdd);

        LoadDiscounts();
    }

    private void LoadDiscounts()
    {
        ClearDiscounts();
        var conn = DatabaseManager.Instance.GetConnection();
        if (conn == null)
        {
            ShowError("Нет соединения к БД.");
            return;
        }

        string sql = @"SELECT id, percent, reason, start_date, end_date 
                       FROM techstore.discounts
                       ORDER BY id";
        try
        {
            using (var cmd = new NpgsqlCommand(sql, conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    int id = reader.GetInt32(0);
                    decimal percent = reader.IsDBNull(1)? 0 : reader.GetDecimal(1);
                    string reason = reader.IsDBNull(2)? "" : reader.GetString(2);
                    System.DateTime? startD = reader.IsDBNull(3)? null : (System.DateTime?)reader.GetDateTime(3);
                    System.DateTime? endD = reader.IsDBNull(4)? null : (System.DateTime?)reader.GetDateTime(4);

                    // Выбираем префаб
                    GameObject prefabToUse = (role=="store_admin" || role=="store_manager") 
                        ? discountItemAdminPrefab 
                        : discountItemReadOnlyPrefab;

                    GameObject itemGO = Instantiate(prefabToUse, content);
                    currentItems.Add(itemGO);

                    var discountItem = itemGO.GetComponent<DiscountItem>();
                    discountItem.Init(id, percent, reason, startD, endD, this, errorNotification);
                }
            }
        }
        catch (System.Exception ex)
        {
            ShowError("Ошибка загрузки скидок: " + ex.Message);
        }
    }

    private void ClearDiscounts()
    {
        foreach (var go in currentItems)
        {
            Destroy(go);
        }
        currentItems.Clear();
    }

    public void OnAddDiscountClick()
    {
        OpenAddDiscountPopup(null, "Добавление скидки");
    }

    public void OpenAddDiscountPopup(DiscountItem editingItem, string title)
    {
        GameObject popupGO = Instantiate(addDiscountPopupPrefab, transform.parent);
        var popup = popupGO.GetComponent<AddDiscountPopup>();
        popup.Initialize(this, editingItem, errorNotification, title);
    }

    public void RefreshList()
    {
        LoadDiscounts();
    }

    public void ShowDeleteConfirmation(DiscountItem item)
    {
        var del = Instantiate(deleteConfirmationPrefab, transform.parent);
        del.Initialize(() => DeleteDiscount(item.id));
    }

    private void DeleteDiscount(int discountId)
    {
        var conn = DatabaseManager.Instance.GetConnection();
        string sql = @"DELETE FROM techstore.discounts WHERE id=@id";
        try
        {
            using (var cmd = new NpgsqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("id", discountId);
                cmd.ExecuteNonQuery();
            }
            RefreshList();
        }
        catch (System.Exception ex)
        {
            ShowError("Ошибка при удалении скидки: " + ex.Message);
        }
    }

    private void ShowError(string msg)
    {
        errorNotification.description = msg;
        errorNotification.UpdateUI();
        errorNotification.Open();
    }
}
