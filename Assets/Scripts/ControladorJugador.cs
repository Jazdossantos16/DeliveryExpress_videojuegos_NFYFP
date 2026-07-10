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

        [Header("Mecánica de Salto (Jump)")]
        [SerializeField] private float jumpDuration = 0.8f;
        [SerializeField] private float jumpScaleMultiplier = 1.3f;

        private bool isJumping = false;
        private Vector3 originalScale;

        public bool IsJumping => isJumping;

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

        // Estado del escudo de inmunidad
        private bool isShieldActive = false;
        private float shieldDurationRemaining = 0f;
        private float shieldDurationMax = 1f;

        public bool IsShieldActive => isShieldActive;
        public float ShieldDurationMax => shieldDurationMax;
        public float ShieldDurationRemaining => shieldDurationRemaining;

        // Estado del multiplicador de monedas X2
        private bool isDoubleCoinsActive = false;
        private float doubleCoinsDurationRemaining = 0f;
        private float doubleCoinsDurationMax = 1f;

        public bool IsDoubleCoinsActive => isDoubleCoinsActive;
        public float DoubleCoinsDurationMax => doubleCoinsDurationMax;
        public float DoubleCoinsDurationRemaining => doubleCoinsDurationRemaining;

        public static ControladorJugador Instance { get; private set; }

        // Variables de estado interno de mejoras (permanentemente actualizadas por el AdministradorMejoras)
        [HideInInspector] public float speedUpgradeFactor = 1f;       // Mejor Bicicleta
        [HideInInspector] public float suspensionUpgradeFactor = 1f;  // Mejor Suspensión (reduce wobble)
        [HideInInspector] public float backpackUpgradeFactor = 1f;    // Mochila Liviana (reduce penalización por peso)
        [HideInInspector] public float powerUpDurationFactor = 1f;    // Factor de duración de power-ups

        // Referencias a componentes
        private Rigidbody2D rb2d;
        private SpriteRenderer spriteRenderer;
        private Animator animator;

        [Header("Sprites de Mejoras")]
        public Sprite[] spritesMochilaPro;
        public Sprite[] spritesCasco;
        public Sprite[] spritesMoto;
        public Sprite[] spritesCascoMoto;
        public Sprite[] spritesMochilaMoto;
        public Sprite[] spritesMochilaYCasco;
        public Sprite[] spritesMochilaCascoMoto;
        
        private System.Collections.Generic.Dictionary<string, Sprite> mochilaSpritesDict;
        private System.Collections.Generic.Dictionary<string, Sprite> cascoSpritesDict;
        private System.Collections.Generic.Dictionary<string, Sprite> motoSpritesDict;
        private System.Collections.Generic.Dictionary<string, Sprite> cascoMotoSpritesDict;
        private System.Collections.Generic.Dictionary<string, Sprite> mochilaMotoSpritesDict;
        private System.Collections.Generic.Dictionary<string, Sprite> mochilaYCascoSpritesDict;
        private System.Collections.Generic.Dictionary<string, Sprite> mochilaCascoMotoSpritesDict;

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

            // Inicializar el diccionario de sprites de mochila pro
            if (spritesMochilaPro != null && spritesMochilaPro.Length > 0)
            {
                mochilaSpritesDict = new System.Collections.Generic.Dictionary<string, Sprite>();
                foreach (var spr in spritesMochilaPro)
                {
                    if (spr != null)
                    {
                        int lastUnderscore = spr.name.LastIndexOf('_');
                        if (lastUnderscore >= 0)
                        {
                            string suffix = spr.name.Substring(lastUnderscore);
                            if (!mochilaSpritesDict.ContainsKey(suffix))
                            {
                                mochilaSpritesDict[suffix] = spr;
                            }
                        }
                    }
                }
            }

            // Inicializar el diccionario de sprites de casco
            if (spritesCasco != null && spritesCasco.Length > 0)
            {
                cascoSpritesDict = new System.Collections.Generic.Dictionary<string, Sprite>();
                foreach (var spr in spritesCasco)
                {
                    if (spr != null)
                    {
                        int lastUnderscore = spr.name.LastIndexOf('_');
                        if (lastUnderscore >= 0)
                        {
                            string suffix = spr.name.Substring(lastUnderscore);
                            if (!cascoSpritesDict.ContainsKey(suffix))
                            {
                                cascoSpritesDict[suffix] = spr;
                            }
                        }
                    }
                }
            }

            // Inicializar el diccionario de sprites combinados (mochila y casco)
            if (spritesMochilaYCasco != null && spritesMochilaYCasco.Length > 0)
            {
                mochilaYCascoSpritesDict = new System.Collections.Generic.Dictionary<string, Sprite>();
                foreach (var spr in spritesMochilaYCasco)
                {
                    if (spr != null)
                    {
                        int lastUnderscore = spr.name.LastIndexOf('_');
                        if (lastUnderscore >= 0)
                        {
                            string suffix = spr.name.Substring(lastUnderscore);
                            if (!mochilaYCascoSpritesDict.ContainsKey(suffix))
                            {
                                mochilaYCascoSpritesDict[suffix] = spr;
                            }
                        }
                    }
                }
            }

            // Inicializar el diccionario de sprites de moto
            if (spritesMoto != null && spritesMoto.Length > 0)
            {
                motoSpritesDict = new System.Collections.Generic.Dictionary<string, Sprite>();
                foreach (var spr in spritesMoto)
                {
                    if (spr != null)
                    {
                        int lastUnderscore = spr.name.LastIndexOf('_');
                        if (lastUnderscore >= 0)
                        {
                            string suffix = spr.name.Substring(lastUnderscore);
                            if (!motoSpritesDict.ContainsKey(suffix))
                            {
                                motoSpritesDict[suffix] = spr;
                            }
                        }
                    }
                }
            }

            // Inicializar el diccionario de sprites de casco + moto
            if (spritesCascoMoto != null && spritesCascoMoto.Length > 0)
            {
                cascoMotoSpritesDict = new System.Collections.Generic.Dictionary<string, Sprite>();
                foreach (var spr in spritesCascoMoto)
                {
                    if (spr != null)
                    {
                        int lastUnderscore = spr.name.LastIndexOf('_');
                        if (lastUnderscore >= 0)
                        {
                            string suffix = spr.name.Substring(lastUnderscore);
                            if (!cascoMotoSpritesDict.ContainsKey(suffix))
                            {
                                cascoMotoSpritesDict[suffix] = spr;
                            }
                        }
                    }
                }
            }

            // Inicializar el diccionario de sprites de mochila + moto
            if (spritesMochilaMoto != null && spritesMochilaMoto.Length > 0)
            {
                mochilaMotoSpritesDict = new System.Collections.Generic.Dictionary<string, Sprite>();
                foreach (var spr in spritesMochilaMoto)
                {
                    if (spr != null)
                    {
                        int lastUnderscore = spr.name.LastIndexOf('_');
                        if (lastUnderscore >= 0)
                        {
                            string suffix = spr.name.Substring(lastUnderscore);
                            if (!mochilaMotoSpritesDict.ContainsKey(suffix))
                            {
                                mochilaMotoSpritesDict[suffix] = spr;
                            }
                        }
                    }
                }
            }

            // Inicializar el diccionario de sprites de mochila + casco + moto
            if (spritesMochilaCascoMoto != null && spritesMochilaCascoMoto.Length > 0)
            {
                mochilaCascoMotoSpritesDict = new System.Collections.Generic.Dictionary<string, Sprite>();
                foreach (var spr in spritesMochilaCascoMoto)
                {
                    if (spr != null)
                    {
                        int lastUnderscore = spr.name.LastIndexOf('_');
                        if (lastUnderscore >= 0)
                        {
                            string suffix = spr.name.Substring(lastUnderscore);
                            if (!mochilaCascoMotoSpritesDict.ContainsKey(suffix))
                            {
                                mochilaCascoMotoSpritesDict[suffix] = spr;
                            }
                        }
                    }
                }
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
            if (powerUpDurationFactor < 0.1f) powerUpDurationFactor = 1f;

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

            originalScale = transform.localScale;

            // Aplicar las mejoras guardadas al inicializar el jugador en la escena
            if (AdministradorMejoras.Instance != null)
            {
                AdministradorMejoras.Instance.ApplyUpgradesToGameplay(this);
            }
        }

        private void Update()
        {
            if (AdministradorUI.Instance != null && AdministradorUI.Instance.IsPlayingVideo)
            {
                if (rb2d != null) rb2d.linearVelocity = Vector2.zero;
                return;
            }

            // Decrementar duración del potenciador de velocidad si está activo
            if (isSpeedBoostActive)
            {
                speedBoostDurationRemaining -= Time.deltaTime;
                if (speedBoostDurationRemaining <= 0f)
                {
                    DesactivarPotenciadorVelocidad();
                }
            }

            // Decrementar duración del escudo de inmunidad si está activo
            if (isShieldActive)
            {
                shieldDurationRemaining -= Time.deltaTime;
                if (shieldDurationRemaining <= 0f)
                {
                    DesactivarEscudoInmunidad();
                }
            }

            // Decrementar duración del multiplicador de monedas si está activo
            if (isDoubleCoinsActive)
            {
                doubleCoinsDurationRemaining -= Time.deltaTime;
                if (doubleCoinsDurationRemaining <= 0f)
                {
                    DesactivarDoubleCoins();
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

            bool jumpPressed = false;
            #if UNITY_INPUT_SYSTEM || ENABLE_INPUT_SYSTEM
            try
            {
                if (UnityEngine.InputSystem.Keyboard.current != null)
                {
                    leftPressed = UnityEngine.InputSystem.Keyboard.current.aKey.wasPressedThisFrame || UnityEngine.InputSystem.Keyboard.current.leftArrowKey.wasPressedThisFrame;
                    rightPressed = UnityEngine.InputSystem.Keyboard.current.dKey.wasPressedThisFrame || UnityEngine.InputSystem.Keyboard.current.rightArrowKey.wasPressedThisFrame;
                    IsBraking = UnityEngine.InputSystem.Keyboard.current.spaceKey.isPressed;
                    jumpPressed = UnityEngine.InputSystem.Keyboard.current.wKey.wasPressedThisFrame || UnityEngine.InputSystem.Keyboard.current.upArrowKey.wasPressedThisFrame;
                }
                else
                {
                    leftPressed = Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow);
                    rightPressed = Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow);
                    IsBraking = Input.GetKey(KeyCode.Space);
                    jumpPressed = Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow);
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
                jumpPressed = Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow);
            }
            catch (System.Exception) {}
            #endif

            if (jumpPressed && !isJumping && !isInvulnerable)
            {
                StartCoroutine(JumpRoutine());
            }

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
            // 1. Inclinación visual basada en la velocidad de transición lateral, el efecto de tambaleo y el viento de costado
            float rawLateralSpeed = Time.deltaTime > 0 ? (targetX - prevX) / Time.deltaTime : 0f;
            float targetTilt = (rawLateralSpeed / laneTransitionSpeed) * maxTiltAngle;
            float wobbleTiltEffect = wobbleOffset * 15f; // Convertir el offset lateral a grados visuales
            
            float windTiltEffect = 0f;
            if (AdministradorJuego.Instance != null && AdministradorJuego.Instance.CurrentDay == 2)
            {
                // Viento cruzado oscilante de lado a lado (+/- 8 grados)
                windTiltEffect = Mathf.Sin(Time.time * 0.8f) * 8f * ControladorClima.IntensidadClima;
            }

            currentTiltAngle = Mathf.Lerp(currentTiltAngle, targetTilt + wobbleTiltEffect + windTiltEffect, 8f * Time.deltaTime);
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

            float windWobble = 0f;
            if (AdministradorJuego.Instance != null && AdministradorJuego.Instance.CurrentDay == 2)
            {
                // Empuje lateral oscilante simétrico (+/- 0.12f)
                windWobble = Mathf.Sin(Time.time * 0.8f) * 0.12f * ControladorClima.IntensidadClima;
            }

            float finalX = targetX + wobbleOffset + windWobble;
            
            if (Mathf.Abs(finalX) >= screenLimitX)
            {
                finalX = Mathf.Sign(finalX) * screenLimitX;
                targetX = finalX - wobbleOffset - windWobble; // Limitamos la variable objetivo para evitar el desplazamiento fuera de la calle

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

            bool isMotoActive = AdministradorMejoras.Instance != null && 
                               AdministradorMejoras.Instance.GetBicycleLevel() > 0 && 
                               AdministradorMejoras.Instance.IsBicycleEquipped();

            if (currentOrders >= 4 || currentBalance < maxBalance * 0.5f)
            {
                animator.SetInteger(StateHash, 3); // Estado "Inestable" (utilizamos los fotogramas de tambaleo)
            }
            else if (Mathf.Abs(currentHorizontalInput) > 0.1f)
            {
                animator.SetInteger(StateHash, isMotoActive ? 0 : 1);
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

            // Si el jugador está saltando, esquiva todos los obstáculos que no sean autos (los salta por arriba)
            if (isJumping && !isCar)
            {
                Debug.Log($"🦘 [SALTO] Saltó con éxito sobre: {collision.gameObject.name}");
                return;
            }

            // Si el jugador está invulnerable, tiene el potenciador de velocidad o el escudo activo, absorbe el impacto de cualquier colisión
            if (isInvulnerable || isSpeedBoostActive || isShieldActive) return;

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
                    // Si el auto está cambiando de carril lateralmente, aplicar tolerancia de distancia
                    if (obs != null && obs.IsChangingLane)
                    {
                        float horizontalDist = Mathf.Abs(transform.position.x - collision.transform.position.x);
                        if (horizontalDist > 1.2f)
                        {
                            Debug.Log($"🛡️ [COLISIÓN TOLERADA] Evitado choque con auto {collision.gameObject.name} por distancia lateral de {horizontalDist:F2} unidades (umbral: 1.2).");
                            return;
                        }
                    }

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

        private void ActualizarColorJugador()
        {
            if (spriteRenderer == null) return;

            if (isShieldActive)
            {
                spriteRenderer.color = new Color(0.5f, 0.9f, 1f, 1f); // Celeste/Cyan para Escudo
            }
            else if (isDoubleCoinsActive)
            {
                spriteRenderer.color = new Color(0.5f, 1f, 0.5f, 1f); // Verde claro para Monedas X2
            }
            else if (isSpeedBoostActive)
            {
                bool isUpgraded = (AdministradorMejoras.Instance != null && AdministradorMejoras.Instance.IsPowerUpEquipped());
                spriteRenderer.color = isUpgraded ? new Color(1f, 0.85f, 0.2f, 1f) : new Color(0.3f, 0.8f, 1f, 1f);
            }
            else
            {
                spriteRenderer.color = Color.white;
            }
        }

        public void ActivarPotenciadorVelocidad(float duracion, float multiplicador)
        {
            isSpeedBoostActive = true;
            float finalDuracion = duracion * powerUpDurationFactor;
            speedBoostDurationMax = finalDuracion;
            speedBoostDurationRemaining = finalDuracion;
            speedBoostMultiplier = multiplicador;

            ActualizarColorJugador();
            Debug.Log($"⚡ Potenciador de velocidad activado por {duracion} segundos con multiplicador {multiplicador}x!");
        }

        private void DesactivarPotenciadorVelocidad()
        {
            isSpeedBoostActive = false;
            speedBoostDurationRemaining = 0f;

            ActualizarColorJugador();
            Debug.Log("⚡ Potenciador de velocidad terminado.");
        }

        public void ActivarEscudoInmunidad(float duracion)
        {
            isShieldActive = true;
            float finalDuracion = duracion * powerUpDurationFactor;
            shieldDurationMax = finalDuracion;
            shieldDurationRemaining = finalDuracion;

            ActualizarColorJugador();
            Debug.Log($"🛡️ Escudo de inmunidad activado por {duracion} segundos!");
        }

        private void DesactivarEscudoInmunidad()
        {
            isShieldActive = false;
            shieldDurationRemaining = 0f;

            ActualizarColorJugador();
            Debug.Log("🛡️ Escudo de inmunidad terminado.");
        }

        public void ActivarDoubleCoins(float duracion)
        {
            isDoubleCoinsActive = true;
            float finalDuracion = duracion * powerUpDurationFactor;
            doubleCoinsDurationMax = finalDuracion;
            doubleCoinsDurationRemaining = finalDuracion;

            ActualizarColorJugador();
            Debug.Log($"💰 Multiplicador de Monedas X2 activado por {duracion} segundos!");
        }

        private void DesactivarDoubleCoins()
        {
            isDoubleCoinsActive = false;
            doubleCoinsDurationRemaining = 0f;

            ActualizarColorJugador();
            Debug.Log("💰 Multiplicador de Monedas X2 terminado.");
        }

        private void LateUpdate()
        {
            if (AdministradorMejoras.Instance == null || spriteRenderer == null || spriteRenderer.sprite == null) return;

            bool isBackpackActive = AdministradorMejoras.Instance.GetBackpackLevel() > 0 && AdministradorMejoras.Instance.IsBackpackEquipped();
            bool isHelmetActive = AdministradorMejoras.Instance.GetSuspensionLevel() > 0 && AdministradorMejoras.Instance.IsSuspensionEquipped();
            bool isMotoActive = AdministradorMejoras.Instance.GetBicycleLevel() > 0 && AdministradorMejoras.Instance.IsBicycleEquipped();

            if (!isBackpackActive && !isHelmetActive && !isMotoActive) return;

            string spriteName = spriteRenderer.sprite.name;
            int lastUnderscore = spriteName.LastIndexOf('_');
            if (lastUnderscore >= 0)
            {
                string suffix = spriteName.Substring(lastUnderscore);
                
                // Si la moto está activa, no debe pedalear (mapear fotogramas 0 a 7 al fotograma estático 6, donde tiene ambos pies arriba y encogidos)
                if (isMotoActive)
                {
                    if (suffix == "_0" || suffix == "_1" || suffix == "_2" || suffix == "_3" ||
                        suffix == "_4" || suffix == "_5" || suffix == "_6" || suffix == "_7")
                    {
                        suffix = "_6";
                    }
                }

                Sprite newSprite = null;

                // 1. Triple combinación: Mochila + Casco + Moto
                if (isBackpackActive && isHelmetActive && isMotoActive)
                {
                    if (mochilaCascoMotoSpritesDict != null && mochilaCascoMotoSpritesDict.TryGetValue(suffix, out Sprite triple))
                    {
                        newSprite = triple;
                    }
                    else if (cascoMotoSpritesDict != null && cascoMotoSpritesDict.TryGetValue(suffix, out Sprite doubleHelm))
                    {
                        newSprite = doubleHelm;
                    }
                    else if (motoSpritesDict != null && motoSpritesDict.TryGetValue(suffix, out Sprite singleMoto))
                    {
                        newSprite = singleMoto;
                    }
                }
                // 2. Doble combinación: Casco + Moto
                else if (isHelmetActive && isMotoActive)
                {
                    if (cascoMotoSpritesDict != null && cascoMotoSpritesDict.TryGetValue(suffix, out Sprite doubleHelm))
                    {
                        newSprite = doubleHelm;
                    }
                    else if (motoSpritesDict != null && motoSpritesDict.TryGetValue(suffix, out Sprite singleMoto))
                    {
                        newSprite = singleMoto;
                    }
                }
                // 3. Doble combinación: Mochila + Moto
                else if (isBackpackActive && isMotoActive)
                {
                    if (mochilaMotoSpritesDict != null && mochilaMotoSpritesDict.TryGetValue(suffix, out Sprite doublePack))
                    {
                        newSprite = doublePack;
                    }
                    else if (motoSpritesDict != null && motoSpritesDict.TryGetValue(suffix, out Sprite singleMoto))
                    {
                        newSprite = singleMoto;
                    }
                }
                // 4. Doble combinación: Mochila + Casco
                else if (isBackpackActive && isHelmetActive)
                {
                    if (mochilaYCascoSpritesDict != null && mochilaYCascoSpritesDict.TryGetValue(suffix, out Sprite doublePackHelm))
                    {
                        newSprite = doublePackHelm;
                    }
                    else if (cascoSpritesDict != null && cascoSpritesDict.TryGetValue(suffix, out Sprite singleHelm))
                    {
                        newSprite = singleHelm;
                    }
                }
                // 5. Individual: Moto
                else if (isMotoActive)
                {
                    if (motoSpritesDict != null && motoSpritesDict.TryGetValue(suffix, out Sprite singleMoto))
                    {
                        newSprite = singleMoto;
                    }
                }
                // 6. Individual: Casco
                else if (isHelmetActive)
                {
                    if (cascoSpritesDict != null && cascoSpritesDict.TryGetValue(suffix, out Sprite singleHelm))
                    {
                        newSprite = singleHelm;
                    }
                }
                // 7. Individual: Mochila
                else if (isBackpackActive)
                {
                    if (mochilaSpritesDict != null && mochilaSpritesDict.TryGetValue(suffix, out Sprite singlePack))
                    {
                        newSprite = singlePack;
                    }
                }

                if (newSprite != null)
                {
                    spriteRenderer.sprite = newSprite;
                }
            }
        }

        private IEnumerator JumpRoutine()
        {
            isJumping = true;
            float elapsed = 0f;
            Vector3 startScale = originalScale;
            Vector3 targetScale = originalScale * jumpScaleMultiplier;

            while (elapsed < jumpDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / jumpDuration;
                
                // Curva parabólica: el factor de altura sube de 0 a 1 y vuelve a 0
                float heightFactor = Mathf.Sin(progress * Mathf.PI);
                
                // Efecto de escala para representar altura visual
                transform.localScale = Vector3.Lerp(startScale, targetScale, heightFactor);
                
                yield return null;
            }

            transform.localScale = originalScale;
            isJumping = false;
        }
    }
}
