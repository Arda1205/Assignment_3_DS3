using UnityEngine;
using UnityEngine.UI;

// Controls crosshair visual feedback when aiming at interactable objects
public class CrosshairUI : MonoBehaviour
{
    // This script will be accessed by player interaction to change its color
    public Image crosshairImage;
    public Color normalColor = Color.white;
    public Color interactColor = Color.red;

    public void SetInteract(bool canInteract)
    {
        crosshairImage.color = canInteract ? interactColor : normalColor;
    }
}
