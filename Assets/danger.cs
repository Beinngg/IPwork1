using UnityEngine;

public class Danger1 : MonoBehaviour
{
    [SerializeField] private float damage = 25f; // Amount of damage to deal

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PlayerHealth>(out var player))
        {
            // Damage is negative health change
            player.ModifyHealth(-damage);
        }
    }
}
