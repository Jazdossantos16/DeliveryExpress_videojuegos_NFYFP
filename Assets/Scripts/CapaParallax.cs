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
        
        [Header("Casa Final")]
        [SerializeField] private Sprite finalHouseSprite;
        
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
            // Mantenemos la compatibilidad por si no se asignó el sprite de calle normal
            if (normalStreetSprite == null) normalStreetSprite = sprite;
            
            speedMultiplier = multiplier;
            transform.localPosition = localPos;

            // Limpiamos los objetos hijos anteriores para evitar duplicados
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }

            obj1 = new GameObject("Sprite_1");
            obj1.transform.SetParent(transform);
            obj1.transform.localPosition = Vector3.zero;
            obj1.transform.localScale = scale;
            sprite1 = obj1.AddComponent<SpriteRenderer>();
            sprite1.sprite = normalStreetSprite;
            sprite1.sortingOrder = sortingOrder;

            // Medimos el alto real del sprite escalado en coordenadas del mundo
            spriteHeight = (normalStreetSprite.bounds.size.y * scale.y) - 0.05f;

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
            // Si la jornada terminó en victoria y aún no se generó la meta final
            if (AdministradorJuego.Instance != null && AdministradorJuego.Instance.IsVictory)
            {
                if (!finalStreetSpawned && finalStreetSprite != null)
                {
                    finalStreetSpawned = true;
                    return finalStreetSprite;
                }
                else if (finalStreetSpawned)
                {
                    return null; // Evitamos generar más tramos de calle por encima de la línea de meta
                }
            }

            // Si hay un cruce configurado, lo intercalamos cada cierta cantidad de calles
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
            if (AdministradorUI.Instance != null && AdministradorUI.Instance.IsPlayingVideo) return;
            if (stopScrolling) return;

            // Si se alcanzó la línea de meta, frenamos el desplazamiento de todas las capas
            if (AdministradorJuego.Instance != null && AdministradorJuego.Instance.IsFinishLineReached)
            {
                return;
            }

            // Si el juego termina en derrota, detenemos el fondo inmediatamente
            if (AdministradorJuego.Instance != null && AdministradorJuego.Instance.IsGameOver && !AdministradorJuego.Instance.IsVictory)
            {
                return;
            }

            InitializeReferences();

            float currentSpeed = baseScrollSpeed * speedMultiplier;
            ControladorJugador player = GameObject.FindFirstObjectByType<ControladorJugador>();
            if (player != null && player.IsBraking)
            {
                currentSpeed *= 0.3f; // Reducimos la velocidad de scroll al 30% si el jugador está frenando
            }
            float movement = currentSpeed * Time.deltaTime;

                        if (sprite1 != null && sprite2 != null)
            {
                sprite1.transform.localPosition -= new Vector3(0, movement, 0);
                sprite2.transform.localPosition -= new Vector3(0, movement, 0);
                
                if (finishLineObj != null)
                {
                    finishLineObj.transform.localPosition -= new Vector3(0, movement, 0);

                    // El límite donde termina la calle y arranca la vereda superior está a aproximadamente 1900/3375 del alto de la meta.
                    // Con el pivot al centro, esto equivale a un desplazamiento local de -0.063f * metaHeight.
                    float metaHeight = finishLineObj.GetComponent<SpriteRenderer>().sprite.bounds.size.y * finishLineObj.transform.localScale.y;
                    float localVanishingPointY = -0.063f * metaHeight;
                    float worldVanishingPointY = finishLineObj.transform.localPosition.y + localVanishingPointY;
                    
                    // Hacemos que la senda peatonal se detenga en Y=1.5f para que el jugador (ubicado en Y=-3.5f)
                    // quede parado justo sobre el asfalto y no sobre la vereda.
                    float targetVanishingPointY = 1.5f;
                    
                    if (worldVanishingPointY <= targetVanishingPointY)
                    {
                        float correction = targetVanishingPointY - worldVanishingPointY;
                        finishLineObj.transform.localPosition += new Vector3(0, correction, 0);
                        sprite1.transform.localPosition += new Vector3(0, correction, 0);
                        sprite2.transform.localPosition += new Vector3(0, correction, 0);
                        
                        stopScrolling = true;
                        if (AdministradorJuego.Instance != null)
                        {
                            AdministradorJuego.Instance.IsFinishLineReached = true;
                        }
                    }
                }

                // Reposicionamos los sprites de forma alternada para lograr el bucle infinito
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
                
                // Instanciamos la meta de forma superpuesta para evitar costuras o huecos visuales
                // Esto garantiza que no quede ningún hueco de fondo.
                finishLineObj = new GameObject("LineaDeMeta");
                finishLineObj.transform.SetParent(transform);
                
                Transform highestSprite = sprite1.transform;
                if (sprite2.transform.localPosition.y > sprite1.transform.localPosition.y)
                {
                    highestSprite = sprite2.transform;
                }
                
                SpriteRenderer sr = finishLineObj.AddComponent<SpriteRenderer>();
                sr.sprite = finalStreetSprite;
                sr.sortingOrder = 1;
                
                if (obj1 != null) {
                    finishLineObj.transform.localScale = obj1.transform.localScale;
                }
                
                // Calculamos la posición exacta para acoplar la meta sin costuras visuales
                float highestTopEdgeY = highestSprite.localPosition.y + (spriteHeight / 2f);
                float metaHeightLocal = finalStreetSprite.bounds.size.y * finishLineObj.transform.localScale.y; 
                float metaCenterY = highestTopEdgeY + (metaHeightLocal / 2f);
                
                finishLineObj.transform.localPosition = new Vector3(0, metaCenterY, 0);

                // Instanciamos la casa final sobre la vereda
                if (finalHouseSprite != null)
                {
                    GameObject houseObj = new GameObject("CasaFinal_Edificio");
                    houseObj.transform.SetParent(finishLineObj.transform, false);
                    houseObj.transform.localScale = new Vector3(0.35f, 0.35f, 1f);
                    houseObj.transform.localPosition = new Vector3(0f, 2.3f, 0f);

                    SpriteRenderer houseSr = houseObj.AddComponent<SpriteRenderer>();
                    houseSr.sprite = finalHouseSprite;
                    houseSr.sortingOrder = 2; // Arriba de la calle (1), abajo del repartidor (10)
                    Debug.Log("🏠 Casa final creada y posicionada en la vereda de la calle final.");
                }
            }
        }

        public void SetBaseSpeed(float newSpeed)
        {
            baseScrollSpeed = newSpeed;
        }

        public float GetWorldVanishingPointY()
        {
            if (finishLineObj == null) return float.MaxValue;
            float metaHeight = finishLineObj.GetComponent<SpriteRenderer>().sprite.bounds.size.y * finishLineObj.transform.localScale.y;
            return finishLineObj.transform.position.y - 0.063f * metaHeight;
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
