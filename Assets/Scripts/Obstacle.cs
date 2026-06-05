using UnityEngine;

namespace DeliveryExpress
{
    public enum ObstacleType
    {
        BlackCar,
        GreenCar,
        Cone,
        Pothole, // Bache
        Pedestrian
    }

    /// <summary>
    /// Define el comportamiento de los obstáculos y vehículos en la calle.
    /// Se mueven verticalmente hacia abajo para simular que el repartidor avanza hacia arriba.
    /// </summary>
    public class Obstacle : MonoBehaviour
    {
        [Header("Configuración del Obstáculo")]
        [SerializeField] private ObstacleType type;
        [SerializeField] private float ownSpeed = 2f; // Velocidad propia del obstáculo (los autos se mueven más rápido)
        
        [Tooltip("Daño infligido al jugador al colisionar")]
        [SerializeField] private int damage = 1;

        private float globalStreetScrollSpeed = 4f; // Velocidad del scroll de la calle
        private float destroyYBound = -10f;       // Límite inferior para reciclar/destruir el objeto

        private Vector2 movementDirection = Vector2.down;

        private void Start()
        {
            // Ajustar comportamientos específicos de velocidad e IA según el tipo
            switch (type)
            {
                case ObstacleType.BlackCar:
                    ownSpeed = 3.5f; // Los autos bajan rápido
                    movementDirection = Vector2.up;
                    break;
                case ObstacleType.GreenCar:
                    ownSpeed = 5.0f; // Autos verdes son deportivos/rápidos
                    movementDirection = Vector2.up;
                    break;
                case ObstacleType.Cone:
                    ownSpeed = 0f; // Estático respecto a la calle
                    break;
                case ObstacleType.Pothole:
                    ownSpeed = 0f; // Estático
                    break;
                case ObstacleType.Pedestrian:
                    ownSpeed = 0.5f; // Cruza lateralmente
                    movementDirection = new Vector2(Random.value > 0.5f ? 1f : -1f, -1f).normalized; // Movimiento diagonal
                    break;
            }
        }

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            {
                return;
            }

            // El movimiento relativo del obstáculo es la combinación del avance del scroll de la calle más su propia velocidad
            float finalDownwardSpeed = globalStreetScrollSpeed + (movementDirection.y * ownSpeed);
            
            // Aplicar traslación
            if (type == ObstacleType.BlackCar || type == ObstacleType.GreenCar)
            {
                // Los autos se desplazan estrictamente en línea recta vertical sin movimiento lateral
                transform.Translate(new Vector3(0f, -finalDownwardSpeed * Time.deltaTime, 0f), Space.World);
            }
            else
            {
                transform.Translate(new Vector3(movementDirection.x * ownSpeed * Time.deltaTime, -finalDownwardSpeed * Time.deltaTime, 0f), Space.World);
            }

            // Destruir el obstáculo si sale de la pantalla por la parte inferior
            if (transform.position.y <= destroyYBound)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Permite al Spawner configurar dinámicamente la velocidad del scroll del nivel
        /// </summary>
        public void SetScrollSpeed(float speed)
        {
            globalStreetScrollSpeed = speed;
        }

        /// <summary>
        /// Configura la dirección del movimiento propio del obstáculo
        /// </summary>
        public void SetMovementDirection(Vector2 direction)
        {
            movementDirection = direction;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                // Si es un bache (pothole), genera inestabilidad temporal en vez de restar vidas directas obligatorias
                if (type == ObstacleType.Pothole)
                {
                    // Triggers extreme wobble
                    PlayerController player = other.GetComponent<PlayerController>();
                    if (player != null)
                    {
                        // Provoca un sacudón en los controles
                        player.TriggerDeliveryAnimation(); // Animación temporal de desbalanceo
                    }
                }
                
                // Desaparecer o desactivar el obstáculo tras chocar (excepto los baches que están pintados en el suelo)
                if (type != ObstacleType.Pothole)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}
