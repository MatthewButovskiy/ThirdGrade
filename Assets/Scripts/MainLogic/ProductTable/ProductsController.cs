using System;
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
    [SerializeField] private DeleteConfirmation  deleteConfirmationPanel;

    private string role;
    private List<GameObject> currentItems = new List<GameObject>();

    public void StartDoinWork()
    {
        role = DatabaseManager.Instance.GetRole();
        
        bool canAdd = (role == "store_admin" || role == "store_manager");
        addProductButton.gameObject.SetActive(canAdd);
        LoadProducts();
    }
    public void ShowDeleteConfirmation(ProductItem product)
    {
        DeleteConfirmation deletePanelGO = Instantiate(deleteConfirmationPanel, transform.parent);
        var deletePanel = deletePanelGO.GetComponent<DeleteConfirmation>();
        
        deletePanel.Initialize(() => DeleteProduct(product.id));
    }


    public void ShowSuccess(string msg)
    {
        errorNotification.description = msg;
        errorNotification.UpdateUI();
        errorNotification.Open();
    }
    
    void LoadProducts()
    {
        ClearProducts();

        var conn = DatabaseManager.Instance.GetConnection();
        using (var cmd = new NpgsqlCommand("SELECT id, name, price, quantity, manufacturer_name,  manufacture_date, category_id FROM techstore.products ORDER BY id", conn))
        using (var reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                int id = reader.GetInt32(0);
                string name = reader.GetString(1);
                decimal price = reader.GetDecimal(2);
                int quantity = reader.GetInt32(3);
                string manufactorName = reader.GetString(4);
                DateTime manufactorDate = reader.GetDateTime(5);
                int categoryID = reader.GetInt32(6);
                
                GameObject prefabToUse;
                if (role == "store_admin" || role == "store_manager")
                {
                    prefabToUse = productItemAdminPrefab;
                }

                else
                {
                    prefabToUse = productItemReadOnlyPrefab;
                }

                GameObject itemGO = Instantiate(prefabToUse, content);
                currentItems.Add(itemGO);

                var productItem = itemGO.GetComponent<ProductItem>();
                productItem.Init(id, name, manufactorName, price, quantity,categoryID, manufactorDate.ToString("yyyy-MM-dd"),this, errorNotification);
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
        OpenAddProductPopup(null, "Добавление товара");
    }
    
    public void OpenAddProductPopup(ProductItem productToEdit, string textForTopLable)
    {
        GameObject popupGO = Instantiate(addProductPopupPrefab, transform.parent);
        var popup = popupGO.GetComponent<AddProductPopup>();
        popup.Initialize(this, productToEdit, textForTopLable);
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
