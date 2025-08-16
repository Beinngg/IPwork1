using UnityEngine;

public class Danger : MonoBehaviour
{
    [SerializeField] private float damage = 1f; // Amount of damage to deal

    private void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent<PlayerHealth>(out var player))
        {
            // Damage is negative health change
            player.ModifyHealth(-damage);
        }
    }
}
