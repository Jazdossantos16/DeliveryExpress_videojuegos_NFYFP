using System.Collections;
using UnityEngine;

namespace DeliveryExpress
{
    /// <summary>
    /// Representa un cliente NPC en la vereda esperando su entrega.
    /// Gatilla la entrega automática al detectar la cercanía del jugador si quedan pedidos.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class NpcCliente : MonoBehaviour
    {
        [Header("Configuración del Cliente")]
        [SerializeField] private int coinsReward = 20;
        
        [Tooltip("Texto flotante a spawnear tras completar la entrega")]
        [SerializeField] private GameObject floatingTextPrefab;

        private bool hasDelivered = false;

        public bool HasDelivered => hasDelivered;

        private void Start()
        {
            // Registrarse en el indicador de navegación si hay uno activo
            IndicadorEntrega indicator = FindFirstObjectByType<IndicadorEntrega>();
            if (indicator != null)
            {
                indicator.RegisterNPC(this);
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (hasDelivered) return;

            // Detección del jugador
            if (collision.CompareTag("Player"))
            {
                // Solo entregar si el jugador tiene pedidos cargados encima
                if (AdministradorJuego.Instance != null && AdministradorJuego.Instance.ActiveOrders > 0)
                {
                    ExecuteDelivery(collision.gameObject);
                }
            }
        }

        private void ExecuteDelivery(GameObject playerObj)
        {
            hasDelivered = true;

            // Trigger de la animación de entrega del brazo en el repartidor
            ControladorJugador playerController = playerObj.GetComponent<ControladorJugador>();
            if (playerController != null)
            {
                playerController.TriggerDeliveryAnimation();
            }

            // Registrar entrega exitosa en el AdministradorJuego central
            if (AdministradorJuego.Instance != null)
            {
                AdministradorJuego.Instance.CompleteDelivery(coinsReward);
            }

            // Mostrar feedback visual
            SpawnDeliveryFeedback();

            // Pequeña animación del NPC celebrando
            StartCoroutine(NPCReactionSequence());
        }

        private void SpawnDeliveryFeedback()
        {
            if (floatingTextPrefab != null)
            {
                GameObject txt = Instantiate(floatingTextPrefab, transform.position + Vector3.up * 1.5f, Quaternion.identity);
                Destroy(txt, 1.2f);
            }
            else
            {
                // Fallback de logs
                Debug.Log($"¡Pedido entregado con éxito a {gameObject.name}! +{coinsReward} monedas.");
            }
        }

        private IEnumerator NPCReactionSequence()
        {
            Vector3 originalScale = transform.localScale;
            float elapsed = 0f;
            float duration = 0.5f;

            // Animación de saltito o rebote en el lugar (estilo cartoon 2D)
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float height = Mathf.Sin((elapsed / duration) * Mathf.PI) * 0.4f;
                transform.position = new Vector3(transform.position.x, transform.position.y + height, transform.position.z);
                yield return null;
            }

            // Cambiar sprite a un estado feliz/satisfecho o destruir tras salir de pantalla
        }
    }
}
