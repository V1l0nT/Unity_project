using UnityEngine;
using Unity.Cinemachine;

public class Player : MonoBehaviour
{
    [SerializeField]
    private float forceMultiplier = 10f;
    [SerializeField]
    private float maximumVelocity = 6f;
    [SerializeField]
    private ParticleSystem deathParticles;

    [SerializeField]
    private int maxHealth = 3;
    private int currentHealth;

    [SerializeField]
    private HealthBarController healthBar;

    [SerializeField]
    private ParticleSystem healingParticlesPrefab;

    private Rigidbody rb;
    private CinemachineImpulseSource cinemachineImpulseSource;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        cinemachineImpulseSource = GetComponent<CinemachineImpulseSource>();
    }

    private void OnEnable()
    {
        transform.position = new Vector3(0, 1.68f, 0);
        transform.rotation = Quaternion.identity;
        rb.linearVelocity = Vector3.zero;

        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    void Update()
    {
        if (GameManager.Instance == null)
        {
            return;
        }

        float horizontalInput = 0;

        if (Input.GetMouseButton(0))
        {
            var center = Screen.width / 2;
            var mousePosition = Input.mousePosition;
            if (mousePosition.x > center)
            {
                horizontalInput = 1;
            }
            else if (mousePosition.x < center)
            {
                horizontalInput = -1;
            }
        }
        else
        {
            horizontalInput = Input.GetAxis("Horizontal");
        }

        if (rb.linearVelocity.magnitude <= maximumVelocity)
        {
            rb.AddForce(new Vector3(horizontalInput * forceMultiplier * Time.deltaTime, 0, 0));
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Hazard"))
        {
            TakeDamage(1);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("FallDown"))
        {
            TakeDamage(currentHealth);
        }
    }

    private void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;

        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(HitEffect());
        }
    }

    private void Die()
    {
        GameManager.Instance.GameOver();

        Instantiate(deathParticles, transform.position, Quaternion.identity);
        cinemachineImpulseSource.GenerateImpulse();

        gameObject.SetActive(false);
    }

    private System.Collections.IEnumerator HitEffect()
    {
        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
        {
            Color originalColor = rend.material.color;
            rend.material.color = Color.red;
            yield return new WaitForSeconds(0.2f);
            rend.material.color = originalColor;
        }
    }

    private void UpdateHealthUI()
    {
        if (healthBar != null)
        {
            healthBar.SetMaxHealth(maxHealth);
            healthBar.SetHealth(currentHealth);
        }
    }

    public void Heal()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();

        if (healingParticlesPrefab != null)
        {
            ParticleSystem healEffect = Instantiate(healingParticlesPrefab, transform.position, Quaternion.identity);
            healEffect.Play();
            Destroy(healEffect.gameObject, healEffect.main.duration + healEffect.main.startLifetime.constantMax);
        }
    }
}
