using UnityEngine;
using Unity.Cinemachine;

public class Hazard : MonoBehaviour
{
    Vector3 rotation;

    [SerializeField]
    private ParticleSystem breakingEffect;
    private CinemachineImpulseSource cinemachineImpulseSource;
    private Player player;

    private void Start()
    {
        cinemachineImpulseSource = GetComponent<CinemachineImpulseSource>();
        player = Object.FindFirstObjectByType<Player>();

        var xRotation = Random.Range(90f, 180f);
        rotation = new Vector3(-xRotation, 0);
    }

    private void Update()
    {
        transform.Rotate(rotation * Time.deltaTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Hazard"))
        {
            Destroy(gameObject);
            Instantiate(breakingEffect, transform.position, Quaternion.identity);

            if (player != null)
            {
                var distance = Vector3.Distance(transform.position, player.transform.position);
                var force = 1f / distance;

                cinemachineImpulseSource.GenerateImpulse(force);
            }
        }

    }
}
