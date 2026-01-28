using UnityEngine;

public class CharacterSlotHighlight : MonoBehaviour
{
    [SerializeField] private GameObject selectedShader;

    public void Select()
    {
        if (selectedShader != null)
            selectedShader.SetActive(true);
    }

    public void Deselect()
    {
        if (selectedShader != null)
            selectedShader.SetActive(false);
    }
}
