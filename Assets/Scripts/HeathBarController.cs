using UnityEngine;
using UnityEngine.UI;

public class HealthBarController : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Image fillImage;

    [SerializeField] private Color fullHealthColor = Color.red;
    [SerializeField] private Color emptyHealthColor = Color.white;

    private void Start()
    {
        if (healthSlider == null)
            healthSlider = GetComponent<Slider>();

        if (fillImage == null && healthSlider.fillRect != null)
            fillImage = healthSlider.fillRect.GetComponent<Image>();

        UpdateColor();

        if (healthSlider.handleRect != null)
        {
            healthSlider.handleRect.gameObject.SetActive(false);
        }
    }

    public void SetMaxHealth(int maxHealth)
    {
        healthSlider.maxValue = maxHealth;
        healthSlider.value = maxHealth;
        UpdateColor();
    }

    public void SetHealth(int currentHealth)
    {
        healthSlider.value = currentHealth;
        UpdateColor();
    }

    private void UpdateColor()
    {
        if (fillImage == null)
            return;

        fillImage.color = fullHealthColor;
    }
}
