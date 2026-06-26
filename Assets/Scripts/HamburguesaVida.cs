using UnityEngine;

namespace DeliveryExpress
{
    /// <summary>
    /// Hamburguesa coleccionable que aparece en la calle y restaura 1 vida al jugador.
    /// Se mueve hacia abajo como el tráfico y desaparece al ser recogida o salir de pantalla.
    /// </summary>
    public class HamburguesaVida : MonoBehaviour
    {
        [Header("Movimiento")]
        [SerializeField] private float scrollSpeed = 5f;

        [Header("Efectos")]
        [SerializeField] private float bobAmplitude = 0.12f;   // amplitud del efecto de flotación
        [SerializeField] private float bobFrequency = 2.5f;    // frecuencia del efecto de flotación
        [SerializeField] private float rotationSpeed = 60f;    // grados por segundo de rotación

        private float baseY;
        private float timeOffset;

        private void Start()
        {
            baseY = transform.position.y;
            timeOffset = Random.Range(0f, Mathf.PI * 2f);
        }

        private void Update()
        {
            // Desplazamiento hacia abajo (sincronizado con la calle y respetando el freno del jugador)
            float speedMultiplier = 1f;
            ControladorJugador player = ControladorJugador.Instance;
            if (player != null && player.IsBraking)
            {
                speedMultiplier = 0.3f; // Reducir velocidad de aproximación al 30% al frenar
            }
            float finalSpeed = Obstaculo.GlobalStreetScrollSpeed * speedMultiplier;
            transform.position += Vector3.down * finalSpeed * Time.deltaTime;

            // Efecto flotante suave
            float bobOffset = Mathf.Sin((Time.time + timeOffset) * bobFrequency) * bobAmplitude;
            transform.position = new Vector3(
                transform.position.x,
                transform.position.y,
                transform.position.z
            );

            // Rotación visual
            transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);

            // Destruir si sale de pantalla por abajo
            if (transform.position.y < -10f)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Permite al generador actualizar la velocidad de scroll para mantenerla sincronizada.
        /// </summary>
        public void SetScrollSpeed(float speed)
        {
            scrollSpeed = speed;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                if (AdministradorJuego.Instance != null)
                {
                    AdministradorJuego.Instance.GainLife();
                }
                if (AdministradorAudio.Instance != null)
                {
                    AdministradorAudio.Instance.PlayLifeSound();
                }
                Destroy(gameObject);
            }
        }
    }
}
