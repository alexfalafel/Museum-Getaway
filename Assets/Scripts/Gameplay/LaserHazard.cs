using UnityEngine;

namespace MuseumGetaway
{
    public sealed class LaserHazard : MonoBehaviour
    {
        [SerializeField] private int damage = 1;

        private void OnTriggerEnter(Collider other)
        {
            Damage(other);
        }

        private void OnTriggerStay(Collider other)
        {
            Damage(other);
        }

        private void Damage(Collider other)
        {
            PlayerHealth health = other.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(damage);
            }
        }
    }
}
