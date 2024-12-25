using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Michsky.MUIP;
using Npgsql;

public class ReviewsController : MonoBehaviour
{
    [SerializeField] private Transform content;
    [SerializeField] private GameObject reviewItemAdminPrefab;
    [SerializeField] private GameObject reviewItemReadOnlyPrefab;
    [SerializeField] private ButtonManager addReviewButton;
    [SerializeField] private NotificationManager errorNotification;
    [SerializeField] private GameObject addReviewPopupPrefab; 
    [SerializeField] private DeleteConfirmation deleteConfirmationPrefab;

    private string role;
    private List<GameObject> currentItems = new List<GameObject>();
    
    public void StartWork()
    {
        role = DatabaseManager.Instance.GetRole();
        
        bool canAdd = (role == "store_admin" || role == "store_manager" || role == "store_moderator");
        addReviewButton.gameObject.SetActive(canAdd);

        LoadReviews();
    }

    private void LoadReviews()
    {
        ClearReviews();
        var conn = DatabaseManager.Instance.GetConnection();
        if (conn == null)
        {
            ShowError("Нет соединения к БД.");
            return;
        }
        
        string sql = @"
            SELECT id, rating, comment, review_date, product_id,
                   customer_last_name, customer_first_name, customer_patronymic
            FROM techstore.reviews
            ORDER BY id";

        try
        {
            using (var cmd = new NpgsqlCommand(sql, conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    int id = reader.GetInt32(0);
                    int rating = reader.GetInt32(1);
                    string comment = reader.IsDBNull(2)? "" : reader.GetString(2);
                    System.DateTime rDate = reader.GetDateTime(3);
                    int productId = reader.GetInt32(4);
                    string lastName = reader.GetString(5);
                    string firstName = reader.GetString(6);
                    string patronymic = reader.IsDBNull(7)? "" : reader.GetString(7);
                    
                    bool canEdit = (role=="store_admin" || role=="store_manager" || role=="store_moderator");
                    GameObject prefabToUse = canEdit ? reviewItemAdminPrefab : reviewItemReadOnlyPrefab;

                    GameObject itemGO = Instantiate(prefabToUse, content);
                    currentItems.Add(itemGO);

                    var reviewItem = itemGO.GetComponent<ReviewItem>();
                    reviewItem.Init(id, rating, comment, rDate, productId,
                                    lastName, firstName, patronymic, this, errorNotification);
                }
            }
        }
        catch (System.Exception ex)
        {
            ShowError("Ошибка загрузки отзывов: " + ex.Message);
        }
    }

    private void ClearReviews()
    {
        foreach (var go in currentItems)
        {
            Destroy(go);
        }
        currentItems.Clear();
    }
    
    public void OnAddReviewClick()
    {
        OpenAddReviewPopup(null, "Добавление отзыва");
    }

    public void OpenAddReviewPopup(ReviewItem editingItem, string title)
    {
        GameObject popupGO = Instantiate(addReviewPopupPrefab, transform.parent);
        var popup = popupGO.GetComponent<AddReviewPopup>();
        popup.Initialize(this, editingItem, errorNotification, title);
    }

    public void RefreshList()
    {
        LoadReviews();
    }

    public void ShowDeleteConfirmation(ReviewItem item)
    {
        var del = Instantiate(deleteConfirmationPrefab, transform.parent);
        del.Initialize(() => DeleteReview(item.id));
    }

    private void DeleteReview(int reviewId)
    {
        var conn = DatabaseManager.Instance.GetConnection();
        if (conn == null)
        {
            ShowError("Нет соединения к БД.");
            return;
        }
        string sql = @"DELETE FROM techstore.reviews WHERE id=@id";
        try
        {
            using (var cmd = new NpgsqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("id", reviewId);
                cmd.ExecuteNonQuery();
            }
            RefreshList();
        }
        catch (System.Exception ex)
        {
            ShowError("Ошибка при удалении отзыва: " + ex.Message);
        }
    }

    private void ShowError(string msg)
    {
        errorNotification.description = msg;
        errorNotification.UpdateUI();
        errorNotification.Open();
    }
}
