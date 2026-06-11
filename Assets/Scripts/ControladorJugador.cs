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
        [SerializeField] private float baseLateralSpeed = 8f; // Respaldo analógico
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
        [Tooltip("Fuerza con la que el movimiento de carril inclina la bicicleta")]
        [SerializeField] private float tiltSensitivity = 30f;
        [Tooltip("Tasa de auto-estabilización de la bici para volver al centro vertical")]
        [SerializeField] private float selfRightingSpeed = 35f;
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
            // Auto-recuperar o añadir el Rigidbody2D de manera dinámica por seguridad
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
                rb2d.bodyType = RigidbodyType2D.Kinematic; // Establecer como Kinematic para evitar caídas físicas
                rb2d.gravityScale = 0f; // Asegurar que no caiga por gravedad física en un juego 2D top-down
                rb2d.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            }
            
            targetX = transform.position.x;

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

            // Encontrar el carril inicial más cercano a la posición de inicio
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
            Debug.Log("ControladorJugador iniciado. Carril inicial: " + currentLaneIndex + " (X: " + lanePositionsX[currentLaneIndex] + ")");
        }

        private void Update()
        {
            // Bloquear el movimiento solo si el jugador perdió. 
            // Si ganó, permitimos que se siga moviendo durante los 4.5 segundos de la animación final.
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
                // Evitar crasheo si el sistema de inputs no está inicializado o hay conflicto de configuración
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

            // Capturar entrada del jugador para saltar entre carriles (A/D o flechas izquierda/derecha)
            if (leftPressed)
            {
                Debug.Log("Tecla Izquierda presionada. Carril actual: " + currentLaneIndex);
                if (currentLaneIndex > 0)
                {
                    currentLaneIndex--;
                    Debug.Log("Cambiando a carril izquierdo: " + currentLaneIndex + " (X objetivo: " + lanePositionsX[currentLaneIndex] + ")");
                }
            }
            if (rightPressed)
            {
                Debug.Log("Tecla Derecha presionada. Carril actual: " + currentLaneIndex);
                if (currentLaneIndex < lanePositionsX.Length - 1)
                {
                    currentLaneIndex++;
                    Debug.Log("Cambiando a carril derecho: " + currentLaneIndex + " (X objetivo: " + lanePositionsX[currentLaneIndex] + ")");
                }
            }

            // Calcular penalización de velocidad lateral debido al peso cargado
            int currentOrders = AdministradorJuego.Instance != null ? AdministradorJuego.Instance.ActiveOrders : 0;
            
            // La mejora de mochila reduce la penalización de peso (ej: factor de 0.6f reduce la penalización en un 40%)
            float activeSpeedPenalty = weightSpeedPenalty * backpackUpgradeFactor;
            float speedMultiplier = Mathf.Max(0.3f, 1f - (currentOrders * activeSpeedPenalty));
            
            // Velocidad lateral final combinada con la mejora de la bicicleta y transición de carril
            float currentLateralSpeed = laneTransitionSpeed * speedUpgradeFactor * speedMultiplier;

            // Conseguir la posición X objetivo del carril actual
            float targetLaneX = lanePositionsX[currentLaneIndex];

            // Mover horizontalmente de manera suave hacia el carril seleccionado
            float prevX = targetX;
            targetX = Mathf.MoveTowards(targetX, targetLaneX, currentLateralSpeed * Time.deltaTime);
            if (Mathf.Abs(targetX - prevX) > 0.001f)
            {
                Debug.Log("Desplazando personaje. X actual: " + targetX + " | X destino: " + targetLaneX + " | Velocidad: " + currentLateralSpeed);
            }

            // --- CÁLCULO DE INCLINACIÓN Y EQUILIBRIO ---
            // 1. Inclinación visual basada en la velocidad lateral de transición y el wobble (tambaleo)
            float rawLateralSpeed = Time.deltaTime > 0 ? (targetX - prevX) / Time.deltaTime : 0f;
            float targetTilt = (rawLateralSpeed / laneTransitionSpeed) * maxTiltAngle;
            float wobbleTiltEffect = wobbleOffset * 15f; // Convertir el offset lateral a grados visuales

            // Suavizar la inclinación de la bicicleta para que sea fluida
            currentTiltAngle = Mathf.Lerp(currentTiltAngle, targetTilt + wobbleTiltEffect, 8f * Time.deltaTime);
            currentTiltAngle = Mathf.Clamp(currentTiltAngle, -maxTiltAngle - 5f, maxTiltAngle + 5f);

            // 2. Drenaje y recuperación de la variable de equilibrio
            // Usamos la velocidad lateral de transición de carril (sin incluir el wobble)
            float laneTransitionVelocity = Time.deltaTime > 0 ? Mathf.Abs(targetX - prevX) / Time.deltaTime : 0f;

            // Se considera inestable si se desplaza rápido o si está muy inclinado
            bool isTilted = Mathf.Abs(currentTiltAngle) > 20f;
            bool isMovingFast = laneTransitionVelocity > stableThreshold;

            if (isMovingFast || isTilted)
            {
                // Drenaje por giro lateral
                float turningDrain = balanceDrainFromTurning * (laneTransitionVelocity / laneTransitionSpeed);
                // Drenaje por velocidad de avance
                float speedDrain = balanceDrainFromSpeedFactor * speedUpgradeFactor;
                // Drenaje continuo proporcional a la inclinación de la bicicleta (gravedad simulada)
                float tiltDrain = (Mathf.Abs(currentTiltAngle) / maxTiltAngle) * 40f; 
                
                // Multiplicador por peso de pedidos cargados
                float weightMultiplier = 1f + (currentOrders * balanceDrainFromWeightFactor * 0.15f);

                float totalDrain = (turningDrain + speedDrain + tiltDrain) * weightMultiplier;
                currentBalance = Mathf.Max(0f, currentBalance - totalDrain * Time.deltaTime);
            }
            else
            {
                // Solo recupera el equilibrio si está completamente derecho y no se desplaza
                currentBalance = Mathf.Min(maxBalance, currentBalance + balanceRecoveryRate * Time.deltaTime);
            }

            // 3. Si pierde por completo el equilibrio (cero), se cae hacia el lado de la inclinación
            if (currentBalance <= 0f && !isInvulnerable)
            {
                // Orientar la animación de caída (flipX) según el lado al que se inclinó la bici
                if (spriteRenderer != null)
                {
                    spriteRenderer.flipX = (currentTiltAngle < 0f);
                }

                // Mantener el personaje derecho en lugar de acostarlo en el piso
                transform.rotation = Quaternion.identity;
                
                TakeDamage();
            }
            else if (!isInvulnerable)
            {
                // Aplicar inclinación visual al Sprite en el eje Z durante el juego
                transform.rotation = Quaternion.Euler(0f, 0f, -currentTiltAngle);
            }

            // Calcular el valor de entrada horizontal de forma dinámica para las animaciones de giro
            if (Mathf.Abs(targetX - targetLaneX) > 0.01f)
            {
                currentHorizontalInput = Mathf.Sign(targetLaneX - targetX);
            }
            else
            {
                currentHorizontalInput = 0f;
            }

            // Calcular el efecto de tambaleo (Wobble) en base al peso y al nivel de equilibrio actual
            // El factor de inestabilidad va de 0.15 (equilibrio perfecto) hasta 1.5 (equilibrio en 0) para evitar un zigzag excesivo en recta
            float balanceLoss = 1f - (currentBalance / maxBalance);
            float balanceInstabilityFactor = 0.15f + (1.35f * balanceLoss);
            
            // El tambaleo se produce si tiene pedidos o si el equilibrio ha bajado de 90%
            if (currentOrders > 0 || currentBalance < maxBalance * 0.9f)
            {
                // La mejora de suspensión reduce la amplitud del tambaleo
                float effectiveOrders = Mathf.Max(0.5f, currentOrders);
                float activeWobbleAmplitude = baseWobbleAmplitude * suspensionUpgradeFactor * effectiveOrders * balanceInstabilityFactor;
                
                // Variación sinusoidal dinámica para simular pérdida de estabilidad
                wobbleOffset = Mathf.Sin(Time.time * wobbleFrequency) * activeWobbleAmplitude;
            }
            else
            {
                wobbleOffset = 0f;
            }

            // Aplicar límites de la calle (veredas)
            float finalX = targetX + wobbleOffset;
            
            if (Mathf.Abs(finalX) >= screenLimitX)
            {
                finalX = Mathf.Sign(finalX) * screenLimitX;
                targetX = finalX - wobbleOffset; // Bloquear el target para evitar acumulación fuera de límites

                // Si chocar contra la vereda hace daño (opción del GDD)
                if (curbDamage && !isInvulnerable)
                {
                    TakeDamage();
                }
            }

            // Aplicar posición al Rigidbody2D y al Transform para garantizar el movimiento lateral en cualquier modo físico
            rb2d.position = new Vector2(finalX, rb2d.position.y);
            transform.position = new Vector3(finalX, transform.position.y, transform.position.z);

            // Actualizar animaciones
            UpdateAnimatorStates(currentOrders);
        }

        /// <summary>
        /// Determina y actualiza los estados de la animación en el Animator de Unity
        /// </summary>
        private void UpdateAnimatorStates(int currentOrders)
        {
            // Validar que el Animator tenga asignado un controlador para evitar spamear la consola de errores
            if (animator == null || animator.runtimeAnimatorController == null) return;

            animator.SetFloat(SpeedXHash, currentHorizontalInput);

            if (isInvulnerable && animator.GetInteger(StateHash) == 3)
            {
                // Mantener estado de choque durante el golpe inicial
                return;
            }

            if (currentOrders >= 4 || currentBalance < maxBalance * 0.5f)
            {
                animator.SetInteger(StateHash, 3); // Estado "Inestable" (Usa Choque.anim/frames 8-11 para simular el tambaleo)
            }
            else if (Mathf.Abs(currentHorizontalInput) > 0.1f)
            {
                animator.SetInteger(StateHash, 1); // Animación de "Pedaleo" (Movimiento activo)
            }
            else
            {
                animator.SetInteger(StateHash, 0); // Animación "Idle" / Avance calmo
            }

            // Reducir la velocidad visual de la animación para simular el freno
            animator.speed = IsBraking ? 0.5f : 1f;
        }

        /// <summary>
        /// Ejecuta la animación de entrega del pedido cuando se pasa cerca de un NPC correcto
        /// </summary>
        public void TriggerDeliveryAnimation()
        {
            StartCoroutine(DeliverySequence());
        }

        private IEnumerator DeliverySequence()
        {
            animator.SetInteger(StateHash, 4); // 4: Animación de "Entrega" (repartidor estirando el brazo)
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

            // Los autos son letales y atraviesan la invulnerabilidad
            if (isCar)
            {
                TakeDamage(true); // Muerte instantánea
                return;
            }

            // Si es un obstáculo menor (cono), la invulnerabilidad te protege
            if (isInvulnerable) return;

            if (collision.CompareTag("Obstaculo") || obs != null)
            {
                TakeDamage(false); // Solo perder 1 vida
            }
        }

        private void TakeDamage(bool instantKill = false)
        {
            currentBalance = maxBalance; // Restablecer equilibrio al chocar
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
            
            // Usar tiempo no escalado para que la animación de choque se reproduzca aun en pausa (Time.timeScale = 0)
            if (animator != null)
            {
                animator.updateMode = AnimatorUpdateMode.UnscaledTime;
            }
            
            animator.SetInteger(StateHash, 3); // 3: Animación de "Choque" / Pérdida de control

            // Pequeño retroceso visual o pausa
            float crashTime = 0.5f;
            float elapsed = 0f;
            Vector2 originalPos = rb2d.position;
            
            while (elapsed < crashTime)
            {
                elapsed += Time.unscaledDeltaTime;
                // Efecto sutil de temblor en pantalla o en el personaje
                rb2d.position = originalPos + new Vector2(Random.Range(-0.1f, 0.1f), 0);
                yield return null;
            }

            // Restablecer inclinación, rotación y volteo (flipX) tras la caída
            transform.rotation = Quaternion.identity;
            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = false;
            }
            currentTiltAngle = 0f;
            currentBalance = maxBalance;

            // Efecto de parpadeo de invulnerabilidad usando tiempo real (unscaled)
            float invulnElapsed = 0f;
            while (invulnElapsed < invulnerabilityDuration)
            {
                spriteRenderer.enabled = !spriteRenderer.enabled;
                yield return new WaitForSecondsRealtime(flashInterval);
                invulnElapsed += flashInterval;
            }

            spriteRenderer.enabled = true;
            isInvulnerable = false;

            // Restablecer el modo de actualización del animator
            if (animator != null)
            {
                animator.updateMode = AnimatorUpdateMode.Normal;
            }
        }
    }
}
