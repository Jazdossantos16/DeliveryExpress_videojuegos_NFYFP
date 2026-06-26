using UnityEngine;

namespace DeliveryExpress
{
    /// <summary>
    /// Moneda coleccionable que aparece en la calle y suma 1 moneda al contador.
    /// Se mueve hacia abajo como el tráfico y desaparece al ser recogida o salir de pantalla.
    /// Soporta animación por sprites.
    /// </summary>
    public class Moneda : MonoBehaviour
    {
        [Header("Movimiento")]
        [SerializeField] private float scrollSpeed = 5f;

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
                if (AdministradorJuego.Instance != null)
                {
                    AdministradorJuego.Instance.AddCoins(1);
                }
                if (AdministradorAudio.Instance != null)
                {
                    AdministradorAudio.Instance.PlayCoinSound();
                }
                Destroy(gameObject);
            }
        }
    }
}
