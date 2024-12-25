using UnityEngine;
using TMPro;
using Michsky.MUIP;
using System;

public class ReviewItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI idText;
    [SerializeField] private TextMeshProUGUI ratingText;
    [SerializeField] private TextMeshProUGUI commentText;
    [SerializeField] private TextMeshProUGUI reviewDateText;
    [SerializeField] private TextMeshProUGUI productIdText;
    [SerializeField] private TextMeshProUGUI customerNameText;

    [SerializeField] private ButtonManager editButton;
    [SerializeField] private ButtonManager deleteButton;

    public int id;
    public int rating;
    public string reviewComment;
    public DateTime reviewDate;
    public int productId;
    public string lastName;
    public string firstName;
    public string patronymic;

    private ReviewsController parentController;
    private NotificationManager errorNotification;

    public void Init(int id, int rating, string comment, DateTime rDate, int pId,
                     string lastN, string firstN, string patromic,
                     ReviewsController ctrl, NotificationManager errNotif)
    {
        this.id = id;
        this.rating = rating;
        this.reviewComment = comment;
        this.reviewDate = rDate;
        this.productId = pId;
        this.lastName = lastN;
        this.firstName = firstN;
        this.patronymic = patromic;
        parentController = ctrl;
        errorNotification = errNotif;

        idText.text = id.ToString();
        ratingText.text = rating.ToString();
        commentText.text = comment;
        reviewDateText.text = rDate.ToString("yyyy-MM-dd");
        productIdText.text = pId.ToString();

        string fullName = lastN + " " + firstN;
        if (!string.IsNullOrEmpty(patromic))
            fullName += " " + patromic;
        customerNameText.text = fullName;
    }

    public void OnEditClick()
    {
        parentController.OpenAddReviewPopup(this, "Редактирование отзыва");
    }

    public void OnDeleteClick()
    {
        parentController.ShowDeleteConfirmation(this);
    }
}
