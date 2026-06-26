using UnityEngine;

namespace DeliveryExpress
{
    /// <summary>
    /// Potenciador de velocidad (energía/rayo) coleccionable que aparece en la calle.
    /// Al recogerlo, incrementa la velocidad del scroll y otorga invulnerabilidad al jugador durante unos segundos.
    /// Se mueve hacia abajo y soporta animación por sprites.
    /// </summary>
    public class PotenciadorEnergia : MonoBehaviour
    {
        [Header("Movimiento")]
        [SerializeField] private float scrollSpeed = 5f;

        [Header("Efectos del Potenciador")]
        [SerializeField] private float duration = 4.5f;       // Duración del potenciador
        [SerializeField] private float speedMultiplier = 1.6f; // Aumento de velocidad (1.6x)

        [Header("Animación por Sprites")]
        [SerializeField] private Sprite[] animationFrames;
        [SerializeField] private float frameRate = 0.1f;

        private SpriteRenderer spriteRenderer;
        private int currentFrameIndex;
        private float timer;

        private void Start()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            
            // Si el objeto no tiene BoxCollider2D, lo creamos y lo configuramos como Trigger
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
                speedMultiplier = 0.3f; // Reducir velocidad de aproximación al 30% al frenar
            }
            float finalSpeed = Obstaculo.GlobalStreetScrollSpeed * speedMultiplier;
            transform.position += Vector3.down * finalSpeed * Time.deltaTime;

            // Animación frame a frame
            if (animationFrames != null && animationFrames.Length > 0 && spriteRenderer != null)
            {
                timer += Time.deltaTime;
                if (timer >= frameRate)
                {
                    timer -= frameRate;
                    currentFrameIndex = (currentFrameIndex + 1) % animationFrames.Length;
                    spriteRenderer.sprite = animationFrames[currentFrameIndex];
                }
            }

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
                ControladorJugador player = other.GetComponent<ControladorJugador>();
                if (player != null)
                {
                    player.ActivarPotenciadorVelocidad(duration, speedMultiplier);
                }
                if (AdministradorAudio.Instance != null)
                {
                    AdministradorAudio.Instance.PlayPowerUpSound();
                }
                
                Destroy(gameObject);
            }
        }
    }
}
