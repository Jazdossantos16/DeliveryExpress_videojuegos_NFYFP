using UnityEngine;

namespace DeliveryExpress
{
    public enum TipoObstaculo
    {
        BlackCar,
        GreenCar,
        Cone,
        Pothole, // Bache
        Pedestrian
    }

    /// <summary>
    /// Define el comportamiento de los obstáculos y vehículos en el asfalto.
    /// </summary>
    public class Obstaculo : MonoBehaviour
    {
        [Header("Configuración del Obstáculo")]
        [SerializeField] private TipoObstaculo type;
        public TipoObstaculo Type => type;
        [SerializeField] private float ownSpeed = 2f; // Velocidad de movimiento propio
        


        private static float globalStreetScrollSpeed = 4f; // Velocidad del scroll de la calle
        private float destroyYBound = -10f;       // Límite inferior para reciclar/destruir el objeto

        private Vector2 movementDirection = Vector2.down;

        private void Start()
        {
            switch (type)
            {
                case TipoObstaculo.BlackCar:
                    ownSpeed = 3.5f;
                    movementDirection = Vector2.up;
                    break;
                case TipoObstaculo.GreenCar:
                    ownSpeed = 5.0f;
                    movementDirection = Vector2.up;
                    break;
                case TipoObstaculo.Cone:
                    ownSpeed = 0f;
                    break;
                case TipoObstaculo.Pothole:
                    ownSpeed = 0f;
                    break;
                case TipoObstaculo.Pedestrian:
                    ownSpeed = 0.5f;
                    movementDirection = new Vector2(Random.value > 0.5f ? 1f : -1f, -1f).normalized;
                    break;
            }
        }

        private void Update()
        {
            // Los autos avanzan y salen de la pantalla aunque termine la partida, salvo al cruzar la meta
            if (AdministradorJuego.Instance != null && AdministradorJuego.Instance.IsFinishLineReached)
            {
                return;
            }

            float speedMultiplier = 1f;
            ControladorJugador player = GameObject.FindFirstObjectByType<ControladorJugador>();
            if (player != null && player.IsBraking)
            {
                speedMultiplier = 0.3f; // Reduce la velocidad de acercamiento al 30%
            }

            // La velocidad final hacia abajo combina el scroll y el movimiento propio del obstáculo
            float finalDownwardSpeed = (globalStreetScrollSpeed + (movementDirection.y * ownSpeed)) * speedMultiplier;
            
            if (type == TipoObstaculo.BlackCar || type == TipoObstaculo.GreenCar)
            {
                transform.Translate(new Vector3(0f, -finalDownwardSpeed * Time.deltaTime, 0f), Space.World);
            }
            else
            {
                float finalHorizontalSpeed = movementDirection.x * ownSpeed * speedMultiplier;
                transform.Translate(new Vector3(finalHorizontalSpeed * Time.deltaTime, -finalDownwardSpeed * Time.deltaTime, 0f), Space.World);
            }

            if (transform.position.y <= destroyYBound)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Configura la velocidad de scroll global del asfalto.
        /// </summary>
        public static void SetGlobalScrollSpeed(float speed)
        {
            globalStreetScrollSpeed = speed;
        }

        /// <summary>
        /// Configura la velocidad de scroll del asfalto.
        /// </summary>
        public void SetScrollSpeed(float speed)
        {
            SetGlobalScrollSpeed(speed);
        }

        /// <summary>
        /// Obtiene la velocidad propia base asociada a cada tipo de obstáculo.
        /// </summary>
        public static float GetOwnSpeedForType(TipoObstaculo obstacleType)
        {
            switch (obstacleType)
            {
                case TipoObstaculo.BlackCar:
                    return 3.5f;
                case TipoObstaculo.GreenCar:
                    return 5.0f;
                case TipoObstaculo.Pedestrian:
                    return 0.5f;
                default:
                    return 0f;
            }
        }

        /// <summary>
        /// Obtiene la velocidad de avance/descenso en el eje Y sin considerar el freno del jugador.
        /// </summary>
        public float GetSpeedWithoutMultiplier()
        {
            float oSpeed = GetOwnSpeedForType(type);
            float dirY = (type == TipoObstaculo.BlackCar || type == TipoObstaculo.GreenCar) ? 1f : (type == TipoObstaculo.Pedestrian ? -0.707f : 0f);
            return globalStreetScrollSpeed + (dirY * oSpeed);
        }

        /// <summary>
        /// Configura la dirección de movimiento del obstáculo.
        /// </summary>
        public void SetMovementDirection(Vector2 direction)
        {
            movementDirection = direction;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                // El bache genera inestabilidad temporal en lugar de restar vidas directamente
                if (type == TipoObstaculo.Pothole)
                {
                    ControladorJugador player = other.GetComponent<ControladorJugador>();
                    if (player != null)
                    {
                        player.TriggerDeliveryAnimation();
                    }
                }
                
                // Se destruye el obstáculo tras chocar, salvo que sea un bache
                if (type != TipoObstaculo.Pothole)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}
