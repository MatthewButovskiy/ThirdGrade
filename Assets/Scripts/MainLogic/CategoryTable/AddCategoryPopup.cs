using Michsky.MUIP;
using TMPro;
using UnityEngine;
using Npgsql;

public class AddCategoryPopup : MonoBehaviour
{
    [SerializeField] private TMP_InputField nameField;
    [SerializeField] private TMP_InputField descriptionField;
    [SerializeField] private NotificationManager errorNotification;

    private CategoriesController parentController;
    private CategoryItem editingCategory;
    private bool isEditMode = false;

    public void Initialize(CategoriesController controller, CategoryItem categoryToEdit, NotificationManager errorNotif, string popupTitle)
    {
        parentController = controller;
        errorNotification = errorNotif;
        if (categoryToEdit != null)
        {
            isEditMode = true;
            editingCategory = categoryToEdit;
            nameField.text = editingCategory.categoryName;
            descriptionField.text = editingCategory.categoryDescription;
        }
        else
        {
            isEditMode = false;
            editingCategory = null;
            nameField.text = "";
            descriptionField.text = "";
        }
    }

    public void OnConfirmClick()
    {
        string newName = nameField.text.Trim();
        string newDescription = descriptionField.text.Trim();

        if (string.IsNullOrEmpty(newName))
        {
            ShowError("Название категории не может быть пустым.");
            return;
        }

        var conn = DatabaseManager.Instance.GetConnection();
        try
        {
            if (isEditMode)
            {
                using (var cmd = new NpgsqlCommand(@"UPDATE techstore.categories 
                  SET name=@n, description=@d WHERE id=@id", conn))
                {
                    cmd.Parameters.AddWithValue("n", newName);
                    cmd.Parameters.AddWithValue("d", newDescription);
                    cmd.Parameters.AddWithValue("id", editingCategory.id);
                    cmd.ExecuteNonQuery();
                }
            }
            else
            {
                using (var cmd = new NpgsqlCommand(@"INSERT INTO techstore.categories (name, description) VALUES (@n, @d)", conn))
                {
                    cmd.Parameters.AddWithValue("n", newName);
                    cmd.Parameters.AddWithValue("d", newDescription);
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

    private void ShowError(string message)
    {
        errorNotification.description = message;
        errorNotification.UpdateUI();
        errorNotification.Open();
    }
}
