using UnityEngine;

namespace DeliveryExpress
{
    /// <summary>
    /// Se encarga de desplazar una capa de fondo hacia abajo y reposicionarla
    /// de forma infinita y continua para simular movimiento y efecto Parallax.
    /// </summary>
    public class CapaParallax : MonoBehaviour
    {
        [Header("Configuración de Movimiento")]
        [Tooltip("Multiplicador de velocidad de scroll (ej: 1.0 para la calle, 0.8 para edificios para dar profundidad)")]
        [SerializeField] private float speedMultiplier = 1f;
        [SerializeField] private float baseScrollSpeed = 4f;

        private GameObject obj1;
        private GameObject obj2;
        private SpriteRenderer sprite1;
        private SpriteRenderer sprite2;
        private float spriteHeight;

        public void Setup(Sprite sprite, float multiplier, int sortingOrder, Vector3 localPos, Vector3 scale)
        {
            speedMultiplier = multiplier;
            transform.localPosition = localPos;

            // Limpiar hijos anteriores por seguridad
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }

            // Crear el primer sprite
            obj1 = new GameObject("Sprite_1");
            obj1.transform.SetParent(transform);
            obj1.transform.localPosition = Vector3.zero;
            obj1.transform.localScale = scale;
            sprite1 = obj1.AddComponent<SpriteRenderer>();
            sprite1.sprite = sprite;
            sprite1.sortingOrder = sortingOrder;

            // Medir la altura real del sprite escalado en unidades de Unity
            // Restamos un pequeño factor de solapamiento (0.05f) para evitar costuras y líneas transparentes (tearing)
            spriteHeight = (sprite.bounds.size.y * scale.y) - 0.05f;

            // Crear el segundo sprite justo arriba del primero para hacer el tiling seamless
            obj2 = new GameObject("Sprite_2");
            obj2.transform.SetParent(transform);
            obj2.transform.localPosition = new Vector3(0, spriteHeight, 0);
            obj2.transform.localScale = scale;
            sprite2 = obj2.AddComponent<SpriteRenderer>();
            sprite2.sprite = sprite;
            sprite2.sortingOrder = sortingOrder;
        }

        private void Start()
        {
            InitializeReferences();
        }

        private void InitializeReferences()
        {
            if (sprite1 == null || sprite2 == null || spriteHeight <= 0)
            {
                Transform t1 = transform.Find("Sprite_1");
                Transform t2 = transform.Find("Sprite_2");

                if (t1 != null)
                {
                    obj1 = t1.gameObject;
                    sprite1 = obj1.GetComponent<SpriteRenderer>();
                }
                if (t2 != null)
                {
                    obj2 = t2.gameObject;
                    sprite2 = obj2.GetComponent<SpriteRenderer>();
                }

                if (sprite1 != null && sprite1.sprite != null)
                {
                    spriteHeight = (sprite1.sprite.bounds.size.y * obj1.transform.localScale.y) - 0.05f;
                }
            }
        }

        private void Update()
        {
            // Si el juego ha terminado por derrota, congelamos el movimiento del fondo (no en victoria)
            if (AdministradorJuego.Instance != null && AdministradorJuego.Instance.IsGameOver && !AdministradorJuego.Instance.IsVictory)
            {
                return;
            }

            // Garantizar que las referencias estén restauradas en tiempo de ejecución
            InitializeReferences();

            // Calcular velocidad de desplazamiento final
            float currentSpeed = baseScrollSpeed * speedMultiplier;
            float movement = currentSpeed * Time.deltaTime;

            // Mover ambos sprites hacia abajo
            if (sprite1 != null && sprite2 != null)
            {
                sprite1.transform.localPosition -= new Vector3(0, movement, 0);
                sprite2.transform.localPosition -= new Vector3(0, movement, 0);

                // Si el primer sprite salió de la pantalla por abajo, lo reposicionamos arriba del segundo
                if (sprite1.transform.localPosition.y <= -spriteHeight)
                {
                    sprite1.transform.localPosition = sprite2.transform.localPosition + new Vector3(0, spriteHeight, 0);
                }

                // Si el segundo sprite salió de la pantalla por abajo, lo reposicionamos arriba del primero
                if (sprite2.transform.localPosition.y <= -spriteHeight)
                {
                    sprite2.transform.localPosition = sprite1.transform.localPosition + new Vector3(0, spriteHeight, 0);
                }
            }
        }

        /// <summary>
        /// Permite sincronizar la velocidad con la del scroll del nivel actual
        /// </summary>
        public void SetBaseSpeed(float newSpeed)
        {
            baseScrollSpeed = newSpeed;
        }
    }
}
