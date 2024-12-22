using UnityEngine;

public class DeleteConfirmation : MonoBehaviour
{
    private ProductsController parentController;
    private ProductItem productToDelete;

    public void Initialize(ProductsController controller)
    {
        parentController = controller;
    }

    public void ShowForItem(ProductItem item)
    {
        productToDelete = item;
        gameObject.SetActive(true);
    }

    public void OnConfirmClick()
    {
        parentController.DeleteProduct(productToDelete.id);
        parentController.ShowSuccess("Товар успешно удалён!");
        gameObject.SetActive(false);
    }

    public void OnCancelClick()
    {
        gameObject.SetActive(false);
    }
}