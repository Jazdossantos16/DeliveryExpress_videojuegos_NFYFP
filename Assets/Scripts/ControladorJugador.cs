using System.Collections; // Force recompilation
using UnityEngine;

namespace DeliveryExpress
{
    /// <summary>
    /// Controla el movimiento lateral, físicas de peso (inestabilidad/tambaleo) y colisiones del repartidor.
    /// Diseñado para una vista cenital picada en 2D (el eje Y representa el avance hacia adelante).
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Animator))]
    public class ControladorJugador : MonoBehaviour
    {
        [Header("Movimiento Lateral (Sistema de Carriles)")]
        [SerializeField] private float[] lanePositionsX = new float[] { -4f, 0f, 4f }; // Izquierdo, Centro, Derecho
        [SerializeField] private int currentLaneIndex = 1; // 0: Izquierdo, 1: Centro, 2: Derecho
        [SerializeField] private float laneTransitionSpeed = 15f; // Velocidad para deslizarse entre carriles
        [SerializeField] private float screenLimitX = 6f; // Límite de la calle

        [Header("Mecánica de Peso e Inestabilidad")]
        [Tooltip("Penalización base de velocidad de giro por cada pedido cargado")]
        [SerializeField] private float weightSpeedPenalty = 0.12f; 
        
        [Tooltip("Amplitud máxima del tambaleo por cada pedido cargado")]
        [SerializeField] private float baseWobbleAmplitude = 0.25f;
        
        [Tooltip("Frecuencia (rapidez) de la oscilación por tambaleo")]
        [SerializeField] private float wobbleFrequency = 3.5f;

        [Header("Sistema de Vuelcos / Límites")]
        [Tooltip("¿Chocar contra la vereda hace perder estabilidad o vida?")]
        [SerializeField] private bool curbDamage = false;

        [Header("Invulnerabilidad tras Choques")]
        [SerializeField] private float invulnerabilityDuration = 1.5f;
        [SerializeField] private float flashInterval = 0.15f;

        [Header("Mecánica de Equilibrio (Balance e Inclinación Z)")]
        [Tooltip("Valor máximo de equilibrio (100 = perfecto, 0 = caída)")]
        [SerializeField] private float maxBalance = 100f;
        [SerializeField] private float currentBalance = 100f;
        [Tooltip("Ángulo de inclinación actual de la bicicleta en grados Z")]
        [SerializeField] private float currentTiltAngle = 0f;
        [Tooltip("Ángulo máximo de inclinación antes de perder el equilibrio y caer")]
        [SerializeField] private float maxTiltAngle = 35f;
        [Tooltip("Umbral de velocidad lateral por debajo de la cual se considera estable")]
        [SerializeField] private float stableThreshold = 0.5f;

        [Header("Configuración de Desgaste de Equilibrio")]
        [Tooltip("Pérdida de equilibrio base por segundo al realizar giros/desplazamiento lateral")]
        [SerializeField] private float balanceDrainFromTurning = 25f;
        [Tooltip("Pérdida de equilibrio constante adicional según la velocidad a la que se circula")]
        [SerializeField] private float balanceDrainFromSpeedFactor = 3f;
        [Tooltip("Multiplicador de pérdida de equilibrio según la cantidad de pedidos cargados (mochila pesada)")]
        [SerializeField] private float balanceDrainFromWeightFactor = 1.5f;
        [Tooltip("Tasa de recuperación del equilibrio por segundo al viajar de forma estable")]
        [SerializeField] private float balanceRecoveryRate = 15f;

        public float CurrentBalance => currentBalance;
        public float MaxBalance => maxBalance;
        public float CurrentTiltAngle => currentTiltAngle;
        public bool IsBraking { get; private set; }

        // Estado del potenciador de velocidad (energía/rayo)
        private bool isSpeedBoostActive = false;
        private float speedBoostDurationRemaining = 0f;
        private float speedBoostDurationMax = 1f;
        private float speedBoostMultiplier = 1.5f;

        public bool IsSpeedBoostActive => isSpeedBoostActive;
        public float SpeedBoostMultiplier => isSpeedBoostActive ? speedBoostMultiplier : 1f;
        public float SpeedBoostDurationMax => speedBoostDurationMax;
        public float SpeedBoostDurationRemaining => speedBoostDurationRemaining;

        public static ControladorJugador Instance { get; private set; }

        // Variables de estado interno de mejoras (permanentemente actualizadas por el AdministradorMejoras)
        [HideInInspector] public float speedUpgradeFactor = 1f;       // Mejor Bicicleta
        [HideInInspector] public float suspensionUpgradeFactor = 1f;  // Mejor Suspensión (reduce wobble)
        [HideInInspector] public float backpackUpgradeFactor = 1f;    // Mochila Liviana (reduce penalización por peso)

        // Referencias a componentes
        private Rigidbody2D rb2d;
        private SpriteRenderer spriteRenderer;
        private Animator animator;

        // Estado del Gameplay
        private float currentHorizontalInput;
        private bool isInvulnerable = false;
        private float wobbleOffset = 0f;
        private float targetX = 0f;

        // Hashes de Animator para optimizar rendimiento
        private static readonly int StateHash = Animator.StringToHash("State"); // 0: Idle, 1: Pedaleo, 2: Inestable, 3: Choque, 4: Entrega
        private static readonly int SpeedXHash = Animator.StringToHash("SpeedX");

        private void Start()
        {
            Instance = this;

            // Recuperamos el componente Rigidbody2D o lo creamos dinámicamente si no existe
            rb2d = GetComponent<Rigidbody2D>();
            if (rb2d == null)
            {
                rb2d = gameObject.AddComponent<Rigidbody2D>();
            }

            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }

            animator = GetComponent<Animator>();

            if (rb2d != null)
            {
                rb2d.gravityScale = 0f; // Desactivamos la gravedad en el Rigidbody para evitar desplazamientos involuntarios en 2D
                rb2d.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            }
            
            targetX = transform.position.x;

            // Evitamos que un arreglo vacío en el Inspector genere excepciones en ejecución
            if (lanePositionsX == null || lanePositionsX.Length == 0)
            {
                lanePositionsX = new float[] { -4f, 0f, 4f };
            }

            // Validamos que los límites laterales no bloqueen el movimiento del jugador
            if (screenLimitX < 5f)
            {
                screenLimitX = 6f;
            }

            // Asignamos una velocidad por defecto si el valor del Inspector no es válido
            if (laneTransitionSpeed < 1f)
            {
                laneTransitionSpeed = 15f;
            }

            // Evitar que un array vacío desde el Inspector de Unity cause bloqueos o excepciones
            if (lanePositionsX == null || lanePositionsX.Length == 0)
            {
                lanePositionsX = new float[] { -4f, 0f, 4f };
            }

            // Evitar que límites incorrectos del Inspector bloqueen el movimiento lateral
            if (screenLimitX < 5f)
            {
                screenLimitX = 6f;
            }

            // Evitar velocidades nulas o corruptas desde el Inspector
            if (laneTransitionSpeed < 1f)
            {
                laneTransitionSpeed = 15f;
            }

            // Evitar que factores ocultos serializados en cero bloqueen la velocidad
            if (speedUpgradeFactor < 0.1f) speedUpgradeFactor = 1f;
            if (backpackUpgradeFactor < 0.1f) backpackUpgradeFactor = 1f;
            if (suspensionUpgradeFactor < 0.1f) suspensionUpgradeFactor = 1f;

            // Buscamos el carril inicial que se encuentra más cercano a la posición del jugador
            float minDistance = float.MaxValue;
            for (int i = 0; i < lanePositionsX.Length; i++)
            {
                float dist = Mathf.Abs(targetX - lanePositionsX[i]);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    currentLaneIndex = i;
                }
            }
        }

        private void Update()
        {
            // Decrementar duración del potenciador de velocidad si está activo
            if (isSpeedBoostActive)
            {
                speedBoostDurationRemaining -= Time.deltaTime;
                if (speedBoostDurationRemaining <= 0f)
                {
                    DesactivarPotenciadorVelocidad();
                }
            }

            // Si la partida terminó en derrota, bloqueamos el movimiento lateral.
            // Si es victoria, permitimos movimiento durante el transcurso de la secuencia final.
            if (AdministradorJuego.Instance != null && AdministradorJuego.Instance.IsGameOver && !AdministradorJuego.Instance.IsVictory)
            {
                rb2d.linearVelocity = Vector2.zero;
                return;
            }
            bool leftPressed = false;
            bool rightPressed = false;

            #if UNITY_INPUT_SYSTEM || ENABLE_INPUT_SYSTEM
            try
            {
                if (UnityEngine.InputSystem.Keyboard.current != null)
                {
                    leftPressed = UnityEngine.InputSystem.Keyboard.current.aKey.wasPressedThisFrame || UnityEngine.InputSystem.Keyboard.current.leftArrowKey.wasPressedThisFrame;
                    rightPressed = UnityEngine.InputSystem.Keyboard.current.dKey.wasPressedThisFrame || UnityEngine.InputSystem.Keyboard.current.rightArrowKey.wasPressedThisFrame;
                    IsBraking = UnityEngine.InputSystem.Keyboard.current.spaceKey.isPressed;
                }
                else
                {
                    leftPressed = Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow);
                    rightPressed = Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow);
                    IsBraking = Input.GetKey(KeyCode.Space);
                }
            }
            catch (System.Exception)
            {
                // Controlamos excepciones si el sistema de entrada no está inicializado
            }
            #else
            try
            {
                leftPressed = Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow);
                rightPressed = Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow);
                IsBraking = Input.GetKey(KeyCode.Space);
            }
            catch (System.Exception) {}
            #endif

            if (leftPressed)
            {
                if (currentLaneIndex > 0)
                {
                    currentLaneIndex--;
                    if (AdministradorAudio.Instance != null)
                    {
                        AdministradorAudio.Instance.PlayLaneSwitchSound();
                    }
                }
            }
            if (rightPressed)
            {
                if (currentLaneIndex < lanePositionsX.Length - 1)
                {
                    currentLaneIndex++;
                    if (AdministradorAudio.Instance != null)
                    {
                        AdministradorAudio.Instance.PlayLaneSwitchSound();
                    }
                }
            }

            // Calculamos la penalización en la velocidad de giro debido al peso de los pedidos
            int currentOrders = AdministradorJuego.Instance != null ? AdministradorJuego.Instance.ActiveOrders : 0;
            
            // La mejora de la mochila aligera el peso de la mochila, reduciendo la penalización
            float activeSpeedPenalty = weightSpeedPenalty * backpackUpgradeFactor;
            float speedMultiplier = Mathf.Max(0.3f, 1f - (currentOrders * activeSpeedPenalty));
            
            // Escalamos sutilmente la velocidad lateral según la velocidad actual del scroll para mantener el control responsivo
            float currentScrollSpeed = Obstaculo.GlobalStreetScrollSpeed;
            float baseReferenceSpeed = 5.0f;
            float speedScale = 1f;
            if (baseReferenceSpeed > 0f && currentScrollSpeed > baseReferenceSpeed)
            {
                // Aumenta hasta un 40% la respuesta lateral a máxima velocidad
                speedScale = Mathf.Lerp(1f, 1.4f, (currentScrollSpeed - baseReferenceSpeed) / baseReferenceSpeed);
            }
            
            float currentLateralSpeed = laneTransitionSpeed * speedUpgradeFactor * speedMultiplier * speedScale;

            float targetLaneX = lanePositionsX[currentLaneIndex];

            float prevX = targetX;
            targetX = Mathf.MoveTowards(targetX, targetLaneX, currentLateralSpeed * Time.deltaTime);

            // --- CÁLCULO DE INCLINACIÓN Y EQUILIBRIO ---
            // 1. Inclinación visual basada en la velocidad de transición lateral y el efecto de tambaleo
            float rawLateralSpeed = Time.deltaTime > 0 ? (targetX - prevX) / Time.deltaTime : 0f;
            float targetTilt = (rawLateralSpeed / laneTransitionSpeed) * maxTiltAngle;
            float wobbleTiltEffect = wobbleOffset * 15f; // Convertir el offset lateral a grados visuales

            currentTiltAngle = Mathf.Lerp(currentTiltAngle, targetTilt + wobbleTiltEffect, 8f * Time.deltaTime);
            currentTiltAngle = Mathf.Clamp(currentTiltAngle, -maxTiltAngle - 5f, maxTiltAngle + 5f);

            // 2. Desgaste y recuperación del nivel de equilibrio del jugador
            // Evaluamos el equilibrio utilizando la velocidad lateral pura (sin incluir tambaleo)
            float laneTransitionVelocity = Time.deltaTime > 0 ? Mathf.Abs(targetX - prevX) / Time.deltaTime : 0f;

            // Aumentamos la inestabilidad si el giro es muy pronunciado o si la inclinación es alta
            bool isTilted = Mathf.Abs(currentTiltAngle) > 20f;
            bool isMovingFast = laneTransitionVelocity > stableThreshold;

            if (isMovingFast || isTilted)
            {
                // Desgaste por giro lateral
                float turningDrain = balanceDrainFromTurning * (laneTransitionVelocity / laneTransitionSpeed);
                // Desgaste por velocidad de avance
                float speedDrain = balanceDrainFromSpeedFactor * speedUpgradeFactor;
                // Desgaste continuo proporcional a la inclinación de la bicicleta (gravedad simulada)
                float tiltDrain = (Mathf.Abs(currentTiltAngle) / maxTiltAngle) * 40f; 
                
                // Multiplicador por peso de pedidos cargados
                float weightMultiplier = 1f + (currentOrders * balanceDrainFromWeightFactor * 0.15f);

                float totalDrain = (turningDrain + speedDrain + tiltDrain) * weightMultiplier;
                currentBalance = Mathf.Max(0f, currentBalance - totalDrain * Time.deltaTime);
            }
            else
            {
                // Recuperamos el equilibrio si el jugador avanza de forma estable sin realizar giros
                // Escalamos la tasa de recuperación de forma proporcional a la velocidad del scroll para compensar el menor tiempo de reacción
                float speedRatio = Mathf.Max(1f, currentScrollSpeed / 5.0f);
                float dynamicRecovery = balanceRecoveryRate * speedRatio;
                currentBalance = Mathf.Min(maxBalance, currentBalance + dynamicRecovery * Time.deltaTime);
            }

            if (AdministradorUI.Instance != null)
            {
                AdministradorUI.Instance.UpdateBalanceUI(currentBalance, maxBalance);
            }

            // 3. Si el equilibrio llega a cero, se activa la caída del jugador hacia el lado de inclinación
            if (currentBalance <= 0f && !isInvulnerable)
            {
                // Invertimos el sprite según la dirección de inclinación para la animación de caída
                if (spriteRenderer != null)
                {
                    spriteRenderer.flipX = (currentTiltAngle < 0f);
                }

                // Mantenemos la rotación en cero para iniciar la secuencia de caída correctamente
                transform.rotation = Quaternion.identity;
                
                TakeDamage();
            }
            else if (!isInvulnerable)
            {
                transform.rotation = Quaternion.Euler(0f, 0f, -currentTiltAngle);
            }

            if (Mathf.Abs(targetX - targetLaneX) > 0.01f)
            {
                currentHorizontalInput = Mathf.Sign(targetLaneX - targetX);
            }
            else
            {
                currentHorizontalInput = 0f;
            }

            // Calculamos el efecto de tambaleo (wobble) basado en el peso y nivel de equilibrio actual
            // El factor de inestabilidad varía según la pérdida de equilibrio para evitar un zigzag exagerado en recta
            float balanceLoss = 1f - (currentBalance / maxBalance);
            float balanceInstabilityFactor = 0.15f + (1.35f * balanceLoss);
            
            // El tambaleo se activa si se transportan pedidos o si el equilibrio disminuye del 90%
            if (currentOrders > 0 || currentBalance < maxBalance * 0.9f)
            {
                // La mejora de suspensión reduce la amplitud del efecto de tambaleo
                float effectiveOrders = Mathf.Max(0.5f, currentOrders);
                float activeWobbleAmplitude = baseWobbleAmplitude * suspensionUpgradeFactor * effectiveOrders * balanceInstabilityFactor;
                
                wobbleOffset = Mathf.Sin(Time.time * wobbleFrequency) * activeWobbleAmplitude;
            }
            else
            {
                wobbleOffset = 0f;
            }

            float finalX = targetX + wobbleOffset;
            
            if (Mathf.Abs(finalX) >= screenLimitX)
            {
                finalX = Mathf.Sign(finalX) * screenLimitX;
                targetX = finalX - wobbleOffset; // Limitamos la variable objetivo para evitar el desplazamiento fuera de la calle

                // Si colisionar contra el cordón de la vereda inflige daño (parámetro configurable)
                if (curbDamage && !isInvulnerable)
                {
                    TakeDamage();
                }
            }

            rb2d.position = new Vector2(finalX, rb2d.position.y);
            transform.position = new Vector3(finalX, transform.position.y, transform.position.z);

            UpdateAnimatorStates(currentOrders);
        }

        /// <summary>
        /// Determina y actualiza los estados de la animación en el Animator de Unity
        /// </summary>
        private void UpdateAnimatorStates(int currentOrders)
        {
            // Verificamos el AnimatorController antes de enviar parámetros para evitar advertencias
            if (animator == null || animator.runtimeAnimatorController == null) return;

            animator.SetFloat(SpeedXHash, currentHorizontalInput);

            if (isInvulnerable && animator.GetInteger(StateHash) == 3)
            {
                // Mantenemos el estado de colisión durante la secuencia de caída inicial
                return;
            }

            if (currentOrders >= 4 || currentBalance < maxBalance * 0.5f)
            {
                animator.SetInteger(StateHash, 3); // Estado "Inestable" (utilizamos los fotogramas de tambaleo)
            }
            else if (Mathf.Abs(currentHorizontalInput) > 0.1f)
            {
                animator.SetInteger(StateHash, 1);
            }
            else
            {
                animator.SetInteger(StateHash, 0);
            }

            // Escalar la velocidad del animator con la velocidad global de scroll
            float currentScrollSpeedAnim = Obstaculo.GlobalStreetScrollSpeed;
            float baseReferenceSpeedAnim = 5.0f; // Velocidad base de referencia (día 1)
            float speedRatioAnim = baseReferenceSpeedAnim > 0f ? (currentScrollSpeedAnim / baseReferenceSpeedAnim) : 1f;

            animator.speed = (IsBraking ? 0.5f : 1f) * speedRatioAnim;
        }

        /// Ejecuta la secuencia de animación de entrega cuando pasa cerca del cliente
        public void TriggerDeliveryAnimation()
        {
            StartCoroutine(DeliverySequence());
        }

        private IEnumerator DeliverySequence()
        {
            animator.SetInteger(StateHash, 4);
            yield return new WaitForSeconds(0.6f);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (AdministradorJuego.Instance != null && AdministradorJuego.Instance.IsGameOver) return;

            Obstaculo obs = collision.GetComponent<Obstaculo>();
            string objName = collision.gameObject.name.ToLower();
            bool isCar = collision.CompareTag("Car") 
                        || objName.Contains("auto") 
                        || objName.Contains("car") 
                        || (obs != null && (obs.Type == TipoObstaculo.BlackCar || obs.Type == TipoObstaculo.GreenCar));

            // Si el jugador está invulnerable o tiene el potenciador de velocidad activo, absorbe el impacto de cualquier colisión
            if (isInvulnerable || isSpeedBoostActive) return;

            if (collision.CompareTag("Obstaculo") || obs != null || isCar)
            {
                // Si es un bache (pothole), no resta vidas (solo genera desequilibrio/animación manejada por el obstáculo)
                if (obs != null && obs.Type == TipoObstaculo.Pothole)
                {
                    Debug.Log($"🕳️ [BACHES] Entró en bache: {collision.gameObject.name}. Genera desequilibrio temporal sin restar vidas.");
                    return;
                }

                if (isCar)
                {
                    Debug.Log($"💥 [COLISIÓN VEHÍCULO] Choque con vehículo: {collision.gameObject.name}. Muerte instantánea.");
                    TakeDamage(true);
                }
                else
                {
                    Debug.Log($"⚠️ [COLISIÓN MENOR] Choque con obstáculo: {collision.gameObject.name}. Resta 1 vida. Vidas restantes: {AdministradorJuego.Instance.CurrentLives - 1}");
                    TakeDamage(false);
                }
            }
        }

        private void TakeDamage(bool instantKill = false)
        {
            currentBalance = maxBalance;
            if (AdministradorAudio.Instance != null)
            {
                AdministradorAudio.Instance.PlayCollisionSound();
            }
            if (AdministradorJuego.Instance != null)
            {
                if (instantKill)
                {
                    AdministradorJuego.Instance.InstantGameOver();
                }
                else
                {
                    AdministradorJuego.Instance.LoseLife();
                }
            }
            
            StartCoroutine(InvulnerabilitySequence());
        }

        private IEnumerator InvulnerabilitySequence()
        {
            isInvulnerable = true;
            
            // Hacemos uso de tiempo no escalado para que la caída se anime correctamente en pausa
            if (animator != null)
            {
                animator.updateMode = AnimatorUpdateMode.UnscaledTime;
            }
            
            animator.SetInteger(StateHash, 3); // 3: Animación de "Choque" / Pérdida de control

            // Pequeño retroceso y retardo visual de impacto
            float crashTime = 0.5f;
            float elapsed = 0f;
            Vector2 originalPos = rb2d.position;
            
            while (elapsed < crashTime)
            {
                elapsed += Time.unscaledDeltaTime;
                // Aplicamos una oscilación de baja amplitud para representar el impacto
                rb2d.position = originalPos + new Vector2(Random.Range(-0.1f, 0.1f), 0);
                yield return null;
            }

            transform.rotation = Quaternion.identity;
            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = false;
            }
            currentTiltAngle = 0f;
            currentBalance = maxBalance;

            // Secuencia visual de parpadeo utilizando tiempo real
            float invulnElapsed = 0f;
            while (invulnElapsed < invulnerabilityDuration)
            {
                spriteRenderer.enabled = !spriteRenderer.enabled;
                yield return new WaitForSecondsRealtime(flashInterval);
                invulnElapsed += flashInterval;
            }

            spriteRenderer.enabled = true;
            isInvulnerable = false;

            if (animator != null)
            {
                animator.updateMode = AnimatorUpdateMode.Normal;
            }
        }

        public void ActivarPotenciadorVelocidad(float duracion, float multiplicador)
        {
            isSpeedBoostActive = true;
            speedBoostDurationMax = duracion;
            speedBoostDurationRemaining = duracion;
            speedBoostMultiplier = multiplicador;

            if (spriteRenderer != null)
            {
                spriteRenderer.color = new Color(0.3f, 0.8f, 1f, 1f);
            }

            Debug.Log($"⚡ Potenciador de velocidad activado por {duracion} segundos con multiplicador {multiplicador}x!");
        }

        private void DesactivarPotenciadorVelocidad()
        {
            isSpeedBoostActive = false;
            speedBoostDurationRemaining = 0f;

            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.white;
            }

            Debug.Log("⚡ Potenciador de velocidad terminado.");
        }
    }
}
