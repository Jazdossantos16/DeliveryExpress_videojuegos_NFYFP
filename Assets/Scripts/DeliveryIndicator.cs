using System.Collections.Generic;
using UnityEngine;

namespace DeliveryExpress
{
    /// <summary>
    /// Maneja el indicador visual de navegación (flecha estilo arcade) para apuntar constantemente
    /// al cliente activo (NPC) más cercano que aún no haya recibido su pedido.
    /// </summary>
    public class DeliveryIndicator : MonoBehaviour
    {
        [Header("Configuración Visual")]
        [SerializeField] private GameObject arrowVisual; // El sprite de la flecha
        [SerializeField] private float offsetDistance = 1.8f; // Distancia desde el jugador
        
        private Transform playerTransform;
        private List<DeliveryNPC> registeredNPCs = new List<DeliveryNPC>();

        public void RegisterNPC(DeliveryNPC npc)
        {
            if (!registeredNPCs.Contains(npc))
            {
                registeredNPCs.Add(npc);
            }
        }

        private void Start()
        {
            // Buscar la referencia del jugador
            PlayerController player = FindFirstObjectByType<PlayerController>();
            if (player != null)
            {
                playerTransform = player.transform;
            }

            if (arrowVisual != null)
            {
                arrowVisual.SetActive(false);
            }
        }

        private void Update()
        {
            if (playerTransform == null || arrowVisual == null) return;

            // Encontrar el cliente activo más cercano
            DeliveryNPC targetNPC = GetClosestActiveNPC();

            if (targetNPC != null)
            {
                arrowVisual.SetActive(true);

                // Calcular dirección del jugador hacia el NPC
                Vector3 direction = (targetNPC.transform.position - playerTransform.position).normalized;

                // Posicionar la flecha del jugador
                transform.position = playerTransform.position + direction * offsetDistance;

                // Rotar la flecha para que apunte hacia el NPC
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.AngleAxis(angle - 90f, Vector3.forward); // Desfase de -90 grados
            }
            else
            {
                // Apagar flecha si no hay clientes válidos esperando
                arrowVisual.SetActive(false);
            }
        }

        private DeliveryNPC GetClosestActiveNPC()
        {
            DeliveryNPC closest = null;
            float minDistance = float.MaxValue;

            for (int i = registeredNPCs.Count - 1; i >= 0; i--)
            {
                // Limpiar referencias destruidas
                if (registeredNPCs[i] == null)
                {
                    registeredNPCs.RemoveAt(i);
                    continue;
                }

                // Evaluar solo clientes no entregados
                if (!registeredNPCs[i].HasDelivered)
                {
                    float dist = Vector3.Distance(playerTransform.position, registeredNPCs[i].transform.position);
                    if (dist < minDistance)
                    {
                        minDistance = dist;
                        closest = registeredNPCs[i];
                    }
                }
            }

            return closest;
        }
    }
}
