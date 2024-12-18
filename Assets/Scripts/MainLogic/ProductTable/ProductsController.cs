using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Npgsql;
using System.Collections.Generic;
using Michsky.MUIP;

public class ProductsController : MonoBehaviour
{
    [SerializeField] private Transform content; 
    [SerializeField] private GameObject productItemAdminPrefab;
    [SerializeField] private GameObject productItemReadOnlyPrefab;
    [SerializeField] private ButtonManager addProductButton;
    [SerializeField] private GameObject addProductPopupPrefab;
    [SerializeField] private NotificationManager errorNotification;

    private string role;
    private List<GameObject> currentItems = new List<GameObject>();

    void Start()
    {
        role = DatabaseManager.Instance.GetRole();
        bool canAdd = (role == "store_admin" || role == "store_manager");
        addProductButton.gameObject.SetActive(canAdd);

        LoadProducts();
    }

    void LoadProducts()
    {
        ClearProducts();

        var conn = DatabaseManager.Instance.GetConnection();
        using (var cmd = new NpgsqlCommand("SELECT id, name, price, quantity FROM techstore.products ORDER BY id", conn))
        using (var reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                int id = reader.GetInt32(0);
                string name = reader.GetString(1);
                decimal price = reader.GetDecimal(2);
                int quantity = reader.GetInt32(3);
                

                GameObject prefabToUse = (role == "store_admin" || role == "store_manager") ? productItemAdminPrefab : productItemReadOnlyPrefab;
                GameObject itemGO = Instantiate(prefabToUse, content);
                currentItems.Add(itemGO);

                var productItem = itemGO.GetComponent<ProductItem>();
                Debug.Log(id + " ");
                Debug.Log(name);
                productItem.Init(id, name, price, quantity, this, errorNotification);
            }
        }
    }

    void ClearProducts()
    {
        foreach (var go in currentItems)
        {
            Destroy(go);
        }
        currentItems.Clear();
    }

    public void OnAddProductClick()
    {
        OpenAddProductPopup(null);
    }

    public void OpenAddProductPopup(ProductItem productToEdit)
    {
        GameObject popupGO = Instantiate(addProductPopupPrefab, transform.parent);
        var popup = popupGO.GetComponent<AddProductPopup>();
        popup.Initialize(this, productToEdit, errorNotification);
    }

    public void RefreshList()
    {
        LoadProducts();
    }

    public void DeleteProduct(int id)
    {
        var conn = DatabaseManager.Instance.GetConnection();
        using (var cmd = new NpgsqlCommand("DELETE FROM techstore.products WHERE id=@id", conn))
        {
            cmd.Parameters.AddWithValue("id", id);
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

    private void ShowError(string message)
    {
        errorNotification.description = message;
        errorNotification.UpdateUI();
        errorNotification.Open();
    }
}
