using UnityEngine;

public class AutoDestroyer : MonoBehaviour
{
    public float delay;

    void Start()
    {
        Destroy(gameObject, delay);
    }

}
