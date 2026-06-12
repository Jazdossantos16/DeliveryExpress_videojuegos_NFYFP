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

        [Header("Secuencia de Escenario")]
        [SerializeField] private Sprite normalStreetSprite;
        [SerializeField] private Sprite crossroadSprite;
        [SerializeField] private Sprite finalStreetSprite;
        
        [Tooltip("Cuántas porciones de calle normal antes de un cruce")]
        [SerializeField] private int chunksUntilCrossroad = 3;

        private GameObject obj1;
        private GameObject obj2;
        private SpriteRenderer sprite1;
        private SpriteRenderer sprite2;
        private float spriteHeight;

        private int currentChunkCount = 0;
        private bool finalStreetSpawned = false;
        private bool stopScrolling = false;

        public void SetupSequence(Sprite normal, Sprite crossroad, Sprite finalSpr)
        {
            normalStreetSprite = normal;
            crossroadSprite = crossroad;
            finalStreetSprite = finalSpr;
        }

        public void Setup(Sprite sprite, float multiplier, int sortingOrder, Vector3 localPos, Vector3 scale)
        {
            // Mantenemos retrocompatibilidad si normalStreetSprite no fue asignado
            if (normalStreetSprite == null) normalStreetSprite = sprite;
            
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
            sprite1.sprite = normalStreetSprite;
            sprite1.sortingOrder = sortingOrder;

            // Medir la altura real del sprite escalado en unidades de Unity
            spriteHeight = (normalStreetSprite.bounds.size.y * scale.y) - 0.05f;

            // Crear el segundo sprite justo arriba del primero
            obj2 = new GameObject("Sprite_2");
            obj2.transform.SetParent(transform);
            obj2.transform.localPosition = new Vector3(0, spriteHeight, 0);
            obj2.transform.localScale = scale;
            sprite2 = obj2.AddComponent<SpriteRenderer>();
            sprite2.sprite = normalStreetSprite;
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

        private Sprite GetNextSprite()
        {
            // Si el juego está en victoria y aún no spawneamos la meta
            if (AdministradorJuego.Instance != null && AdministradorJuego.Instance.IsVictory)
            {
                if (!finalStreetSpawned && finalStreetSprite != null)
                {
                    finalStreetSpawned = true;
                    return finalStreetSprite;
                }
                else if (finalStreetSpawned)
                {
                    // Después de la línea de llegada, podemos devolver calle normal (o la misma)
                    // aunque el juego se frenará cuando la llegada toque el centro
                    return normalStreetSprite;
                }
            }

            // Si hay cruce configurado, intercala cada X calles
            if (crossroadSprite != null)
            {
                currentChunkCount++;
                if (currentChunkCount >= chunksUntilCrossroad)
                {
                    currentChunkCount = 0;
                    return crossroadSprite;
                }
            }

            return normalStreetSprite;
        }

        private void Update()
        {
            if (stopScrolling) return;

            // Si el juego ha terminado por derrota, congelamos el movimiento del fondo
            if (AdministradorJuego.Instance != null && AdministradorJuego.Instance.IsGameOver && !AdministradorJuego.Instance.IsVictory)
            {
                return;
            }

            // Garantizar referencias
            InitializeReferences();

            // Calcular movimiento
            float currentSpeed = baseScrollSpeed * speedMultiplier;
            ControladorJugador player = GameObject.FindFirstObjectByType<ControladorJugador>();
            if (player != null && player.IsBraking)
            {
                currentSpeed *= 0.3f; // Reducir velocidad al 30% si está frenando
            }
            float movement = currentSpeed * Time.deltaTime;

            if (sprite1 != null && sprite2 != null)
            {
                sprite1.transform.localPosition -= new Vector3(0, movement, 0);
                sprite2.transform.localPosition -= new Vector3(0, movement, 0);
                
                if (finishLineObj != null)
                {
                    finishLineObj.transform.localPosition -= new Vector3(0, movement, 0);

                    // Frenar todo cuando la meta llega a la posición del jugador (Y = 0)
                    if (finishLineObj.transform.localPosition.y <= 0f)
                    {
                        finishLineObj.transform.localPosition = new Vector3(0, 0f, 0);
                        stopScrolling = true; // Frenar el asfalto

                        if (AdministradorJuego.Instance != null)
                        {
                            AdministradorJuego.Instance.IsFinishLineReached = true; // Frenar casas y autos
                        }
                    }
                }

                // Reposicionamiento y cambio de sprite
                if (sprite1.transform.localPosition.y <= -spriteHeight)
                {
                    sprite1.transform.localPosition = sprite2.transform.localPosition + new Vector3(0, spriteHeight, 0);
                    sprite1.sprite = GetNextSprite();
                }

                if (sprite2.transform.localPosition.y <= -spriteHeight)
                {
                    sprite2.transform.localPosition = sprite1.transform.localPosition + new Vector3(0, spriteHeight, 0);
                    sprite2.sprite = GetNextSprite();
                }
            }
        }

        private GameObject finishLineObj;

        public void ForceFinalStreet()
        {
            if (finalStreetSprite != null && !finalStreetSpawned)
            {
                finalStreetSpawned = true;
                
                // En lugar de reemplazar el sprite de la calle (lo que deja un hueco negro si la meta es más chica),
                // instanciamos la línea de meta como un objeto superpuesto a la calle que baja con ella.
                
                finishLineObj = new GameObject("LineaDeMeta");
                finishLineObj.transform.SetParent(transform);
                
                // La ponemos bien arriba, por encima del borde superior de la cámara (Y=6) para que baje fluidamente
                // La coordenada exacta dependerá de qué tan rápido llegue al centro. Y=12 es un buen lugar seguro.
                finishLineObj.transform.position = new Vector3(0, 10f, 0);
                if (obj1 != null) {
                    finishLineObj.transform.localScale = obj1.transform.localScale; // misma escala que la calle
                }
                
                SpriteRenderer sr = finishLineObj.AddComponent<SpriteRenderer>();
                sr.sprite = finalStreetSprite;
                sr.sortingOrder = 1; // Por encima del asfalto (-10), pero debajo de las casas (2) y del jugador
            }
        }

        public void SetBaseSpeed(float newSpeed)
        {
            baseScrollSpeed = newSpeed;
        }

        public bool IsCrossroadOverlapping(float yPosition)
        {
            float halfHeight = spriteHeight / 2f;
            
            if (sprite1 != null && sprite1.sprite == crossroadSprite)
            {
                if (yPosition >= sprite1.transform.localPosition.y - halfHeight && yPosition <= sprite1.transform.localPosition.y + halfHeight) 
                    return true;
            }
            if (sprite2 != null && sprite2.sprite == crossroadSprite)
            {
                if (yPosition >= sprite2.transform.localPosition.y - halfHeight && yPosition <= sprite2.transform.localPosition.y + halfHeight) 
                    return true;
            }
            return false;
        }
    }
}
