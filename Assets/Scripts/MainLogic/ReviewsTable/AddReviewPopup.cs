using UnityEngine;
using TMPro;
using Michsky.MUIP;
using Npgsql;
using System;

public class AddReviewPopup : MonoBehaviour
{
    [SerializeField] private TMP_InputField ratingField;
    [SerializeField] private TMP_InputField commentField;
    [SerializeField] private TMP_InputField reviewDateField;
    [SerializeField] private TMP_InputField productIdField;
    [SerializeField] private TMP_InputField lastNameField;
    [SerializeField] private TMP_InputField firstNameField;
    [SerializeField] private TMP_InputField patronymicField;

    [SerializeField] private TMP_Text titleText;
    [SerializeField] private NotificationManager errorNotification;

    private ReviewsController parentController;
    private ReviewItem editingItem;
    private bool isEditMode = false;

    public void Initialize(ReviewsController ctrl, ReviewItem item, NotificationManager notif, string title)
    {
        parentController = ctrl;
        editingItem = item;
        //errorNotification = notif;
        titleText.text = title;

        if (item != null)
        {
            isEditMode = true;
            ratingField.text = item.rating.ToString();
            commentField.text = item.reviewComment;
            reviewDateField.text = item.reviewDate.ToString("yyyy-MM-dd");
            productIdField.text = item.productId.ToString();
            lastNameField.text = item.lastName;
            firstNameField.text = item.firstName;
            patronymicField.text = item.patronymic;
        }
        else
        {
            isEditMode = false;
            ratingField.text = "";
            commentField.text = "";
            reviewDateField.text = DateTime.Now.ToString("yyyy-MM-dd");
            productIdField.text = "";
            lastNameField.text = "";
            firstNameField.text = "";
            patronymicField.text = "";
        }
    }

    public void OnConfirmClick()
    {
        if (!int.TryParse(ratingField.text.Trim(), out int rating) || rating <=0 || rating>5)
        {
            ShowError("Некорректная оценка. Диапазон 1..5");
            return;
        }
        string comment = commentField.text.Trim();
        if (!DateTime.TryParse(reviewDateField.text.Trim(), out DateTime rDate))
        {
            ShowError("Некорректная дата отзыва (yyyy-MM-dd).");
            return;
        }
        if (!int.TryParse(productIdField.text.Trim(), out int prodId) || prodId <= 0)
        {
            ShowError("Некорректный product_id.");
            return;
        }
        string lastN = lastNameField.text.Trim();
        string firstN = firstNameField.text.Trim();
        string patN = patronymicField.text.Trim();
        if (string.IsNullOrEmpty(lastN) || string.IsNullOrEmpty(firstN))
        {
            ShowError("Фамилия/Имя не могут быть пусты.");
            return;
        }

        var conn = DatabaseManager.Instance.GetConnection();
        if (conn == null)
        {
            ShowError("Нет соединения к БД.");
            return;
        }

        try
        {
            if (isEditMode && editingItem!=null)
            {
                using (var cmd = new NpgsqlCommand(@"
                  UPDATE techstore.reviews
                  SET rating=@rt, comment=@cm, review_date=@rd, product_id=@pid,
                      customer_last_name=@ln, customer_first_name=@fn, customer_patronymic=@pn
                  WHERE id=@id", conn))
                {
                    cmd.Parameters.AddWithValue("rt", rating);
                    cmd.Parameters.AddWithValue("cm", comment);
                    cmd.Parameters.AddWithValue("rd", rDate);
                    cmd.Parameters.AddWithValue("pid", prodId);
                    cmd.Parameters.AddWithValue("ln", lastN);
                    cmd.Parameters.AddWithValue("fn", firstN);
                    if (string.IsNullOrEmpty(patN))
                        cmd.Parameters.AddWithValue("pn", DBNull.Value);
                    else
                        cmd.Parameters.AddWithValue("pn", patN);

                    cmd.Parameters.AddWithValue("id", editingItem.id);
                    cmd.ExecuteNonQuery();
                }
            }
            else
            {
                // INSERT
                using (var cmd = new NpgsqlCommand(@"
                  INSERT INTO techstore.reviews(
                     rating, comment, review_date, product_id,
                     customer_last_name, customer_first_name, customer_patronymic
                  )
                  VALUES(@rt, @cm, @rd, @pid, @ln, @fn, @pn)", conn))
                {
                    cmd.Parameters.AddWithValue("rt", rating);
                    cmd.Parameters.AddWithValue("cm", comment);
                    cmd.Parameters.AddWithValue("rd", rDate);
                    cmd.Parameters.AddWithValue("pid", prodId);
                    cmd.Parameters.AddWithValue("ln", lastN);
                    cmd.Parameters.AddWithValue("fn", firstN);
                    if (string.IsNullOrEmpty(patN))
                        cmd.Parameters.AddWithValue("pn", DBNull.Value);
                    else
                        cmd.Parameters.AddWithValue("pn", patN);

                    cmd.ExecuteNonQuery();
                }
            }
        }
        catch(System.Exception ex)
        {
            ShowError("Ошибка при сохранении отзыва: " + ex.Message);
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
        errorNotification.title = "Ошибка";
        errorNotification.description = msg;
        errorNotification.UpdateUI();
        errorNotification.Open();
    }
}
