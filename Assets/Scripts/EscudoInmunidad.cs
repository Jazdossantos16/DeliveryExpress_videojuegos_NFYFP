using UnityEngine;

namespace DeliveryExpress
{
    /// <summary>
    /// Escudo de inmunidad coleccionable que aparece en la calle.
    /// Al recogerlo, otorga al jugador inmunidad contra colisiones con autos y obstáculos durante 10 segundos.
    /// </summary>
    public class EscudoInmunidad : MonoBehaviour
    {
        [Header("Movimiento")]
        [SerializeField] private float scrollSpeed = 5f;
        [SerializeField] private float duration = 10f; // Duración de la inmunidad

        private void Start()
        {
            // Asegurarnos de tener BoxCollider2D configurado como Trigger
            BoxCollider2D col = GetComponent<BoxCollider2D>();
            if (col == null)
            {
                col = gameObject.AddComponent<BoxCollider2D>();
            }
            col.isTrigger = true;
        }

        private void Update()
        {
            // Desplazamiento hacia abajo (sincronizado con la calle y respetando el freno del jugador)
            float speedMultiplier = 1f;
            ControladorJugador player = ControladorJugador.Instance;
            if (player != null && player.IsBraking)
            {
                speedMultiplier = 0.3f; // Reducir velocidad al frenar
            }
            float finalSpeed = Obstaculo.GlobalStreetScrollSpeed * speedMultiplier;
            transform.position += Vector3.down * finalSpeed * Time.deltaTime;

            // Destruir si sale de pantalla por abajo
            if (transform.position.y < -10f)
            {
                Destroy(gameObject);
            }
        }

        public void SetScrollSpeed(float speed)
        {
            scrollSpeed = speed;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                ControladorJugador player = other.GetComponent<ControladorJugador>();
                if (player != null)
                {
                    player.ActivarEscudoInmunidad(duration);
                }
                if (AdministradorAudio.Instance != null)
                {
                    // Reproducir el sonido de escudo
                    AdministradorAudio.Instance.PlayShieldSound();
                }
                
                Destroy(gameObject);
            }
        }
    }
}
