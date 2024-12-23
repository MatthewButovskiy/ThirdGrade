using TMPro;
using UnityEngine;

public class CategoryItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI categoryText;

    public int id;
    public string categoryName;
    public string categoryDescription;

    private CategoriesController parentController;

    public void Init(int id, string name, string description, CategoriesController controller)
    {
        this.id = id;
        categoryName = name;
        categoryDescription = description;
        parentController = controller;

        nameText.text = categoryName;
        descriptionText.text = categoryDescription;
        categoryText.text = id.ToString();
    }

    public void OnEditClick()
    {
        parentController.OpenAddCategoryPopup(this, "Редактирование категории");
    }

    public void OnDeleteClick()
    {
        parentController.ShowDeleteConfirmation(this);
    }
}
