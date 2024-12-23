using System.Collections.Generic;
using Michsky.MUIP;
using UnityEngine;
using TMPro;
using Npgsql;

public class CategoriesController : MonoBehaviour
{
    [SerializeField] private Transform content;
    [SerializeField] private GameObject categoryItemPrefab;
    [SerializeField] private NotificationManager errorNotification;
    [SerializeField] private GameObject deleteConfirmationPanelPrefab; // Панель подтверждения удаления
   
    [SerializeField] private GameObject addCategoryPopupPrefab;
    private List<GameObject> currentItems = new List<GameObject>();

    public void StartDoinWork()
    {
        LoadCategories();
    }

    void LoadCategories()
    {
        ClearCategories();

        var conn = DatabaseManager.Instance.GetConnection();
        using (var cmd = new NpgsqlCommand("SELECT id, name, description FROM techstore.categories ORDER BY id", conn))
        using (var reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                int id = reader.GetInt32(0);
                string name = reader.GetString(1);
                string description = reader.IsDBNull(2) ? "" : reader.GetString(2);

                GameObject itemGO = Instantiate(categoryItemPrefab, content);
                currentItems.Add(itemGO);

                var categoryItem = itemGO.GetComponent<CategoryItem>();
                categoryItem.Init(id, name, description, this);
            }
        }
    }

    void ClearCategories()
    {
        foreach (var go in currentItems)
        {
            Destroy(go);
        }
        currentItems.Clear();
    }

    public void OpenAddCategoryPopup(CategoryItem categoryToEdit, string popupTitle)
    {
        GameObject popupGO = Instantiate(addCategoryPopupPrefab, transform.parent);
        var popup = popupGO.GetComponent<AddCategoryPopup>();

        popup.Initialize(this, categoryToEdit, errorNotification, popupTitle);
    }

    public void OnAddCategoryClick()
    {
        OpenAddCategoryPopup(null, "Добавление категории");
    }

    public void RefreshList()
    {
        LoadCategories();
    }

    public void ShowDeleteConfirmation(CategoryItem category)
    {
        GameObject deletePanelGO = Instantiate(deleteConfirmationPanelPrefab, transform.parent);
        var deletePanel = deletePanelGO.GetComponent<DeleteConfirmation>();
        
        deletePanel.Initialize(() => DeleteCategory(category.id));
    }


    public void DeleteCategory(int id)
    {
        var conn = DatabaseManager.Instance.GetConnection();
        using (var cmd = new NpgsqlCommand("DELETE FROM techstore.categories WHERE id=@id", conn))
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
