using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DeliveryExpress
{
    /// <summary>
    /// Administrador central del ciclo de juego, tiempo, vidas, monedas y jornadas.
    /// Implementa el patrón Singleton.
    /// </summary>
    public class AdministradorJuego : MonoBehaviour
    {
        public static AdministradorJuego Instance { get; private set; }

        [Header("Configuración de Vidas y Tiempo")]
        [SerializeField] private int startingLives = 3; // Cantidad de vidas iniciales
        [SerializeField] private float baseLevelDuration = 60f; // Duración base del nivel en segundos

        [Header("Configuración de Jornadas")]
        [SerializeField] private int currentDay = 1;

        // Variables de juego en tiempo real
        private int currentLives;
        private float timeRemaining;
        private int coinsAccumulated = 0;
        private int activeOrders = 0;
        private int totalDeliveriesRequired = 0;
        private int currentDeliveriesCompleted = 0;

        private bool isGameRunning = false;
        private bool isGameOver = false;
        private bool isVictory = false;

        // Modificador de mejora de tiempo
        [HideInInspector] public float extraTimeUpgrade = 0f; // Tiempo adicional por mejoras

        // Eventos para actualizar la UI en Unity
        public event Action<int> OnLivesChanged;
        public event Action<float> OnTimeChanged;
        public event Action<int> OnCoinsChanged;
        public event Action<int, int> OnDeliveriesChanged;
        public event Action<int> OnOrdersWeightChanged;

        public int ActiveOrders => activeOrders;
        public int Coins => coinsAccumulated;
        public bool IsGameOver => isGameOver;
        public bool IsVictory => isVictory;
        public int CurrentDay => currentDay;
        public float TimeRemaining => timeRemaining;
        public bool IsFinishLineReached { get; set; } = false;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            StartNewDay();
        }

        /// <summary>
        /// Inicializa el estado para una nueva jornada laboral (nivel)
        /// </summary>
        public void StartNewDay()
        {
            isGameOver = false;
            isVictory = false;
            isGameRunning = true;

            // Restablecemos las vidas al iniciar el día
            currentLives = startingLives;
            
            baseLevelDuration = 60f;
            // Sumamos el tiempo de las mejoras adquiridas
            timeRemaining = baseLevelDuration + extraTimeUpgrade;

            // No hay entregas intermedias en este modo
            totalDeliveriesRequired = 0;
            currentDeliveriesCompleted = 0;

            // Inicializamos la cantidad de pedidos cargados
            activeOrders = totalDeliveriesRequired;

            OnLivesChanged?.Invoke(currentLives);
            OnTimeChanged?.Invoke(timeRemaining);
            OnCoinsChanged?.Invoke(coinsAccumulated);
            OnDeliveriesChanged?.Invoke(currentDeliveriesCompleted, totalDeliveriesRequired);
            OnOrdersWeightChanged?.Invoke(activeOrders);
        }

        private void Update()
        {
            if (!isGameRunning || isGameOver) return;

            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                OnTimeChanged?.Invoke(timeRemaining);
            }
            else
            {
                timeRemaining = 0;
                OnTimeChanged?.Invoke(timeRemaining);
                
                // Verificamos las entregas al finalizar el tiempo
                if (currentDeliveriesCompleted >= totalDeliveriesRequired)
                {
                    TriggerVictory();
                }
                else
                {
                    TriggerDefeat(true);
                }
            }
        }

        /// <summary>
        /// Resta una vida al jugador por colisión. Si llega a 0, activa la derrota.
        /// </summary>
        public void LoseLife()
        {
            if (isGameOver) return;

            currentLives--;
            OnLivesChanged?.Invoke(currentLives);

            if (currentLives <= 0)
            {
                TriggerDefeat(false);
            }
        }

        /// <summary>
        /// Restaura 1 vida al jugador por recoger una hamburguesa. Máximo: startingLives.
        /// </summary>
        public void GainLife()
        {
            if (isGameOver) return;

            if (currentLives < startingLives)
            {
                currentLives++;
                OnLivesChanged?.Invoke(currentLives);
                Debug.Log($"🍔 +1 vida recuperada. Vidas actuales: {currentLives}");
            }
            else
            {
                Debug.Log("🍔 Vida recogida pero ya tenés el máximo de vidas.");
            }
        }


        /// <summary>
        /// Colisión letal con un vehículo
        /// </summary>


        public void InstantGameOver()
        {
            if (isGameOver) return;
            currentLives = 0;
            OnLivesChanged?.Invoke(currentLives);
            TriggerDefeat(false);
        }

        /// <summary>
        /// Ejecuta una entrega de pedido exitosa a un NPC cliente
        /// </summary>
        public void CompleteDelivery(int coinsReward)
        {
            if (isGameOver || activeOrders <= 0) return;

            activeOrders--;
            currentDeliveriesCompleted++;

            coinsAccumulated += coinsReward;

            OnDeliveriesChanged?.Invoke(currentDeliveriesCompleted, totalDeliveriesRequired);
            OnCoinsChanged?.Invoke(coinsAccumulated);
            OnOrdersWeightChanged?.Invoke(activeOrders);

            // La victoria se evalúa por tiempo de supervivencia
        }

        /// <summary>
        /// Incrementamos las monedas acumuladas
        /// </summary>
        public void AddCoins(int amount)
        {
            coinsAccumulated += amount;
            OnCoinsChanged?.Invoke(coinsAccumulated);
        }

        /// <summary>
        /// Procesamos el pago en la tienda
        /// </summary>
        public bool SpendCoins(int amount)
        {
            if (coinsAccumulated >= amount)
            {
                coinsAccumulated -= amount;
                OnCoinsChanged?.Invoke(coinsAccumulated);
                return true;
            }
            return false;
        }

        private void TriggerVictory()
        {
            isGameRunning = false;
            isVictory = true;
            isGameOver = true;

            Debug.Log("¡Felicidades! Jornada completada con éxito.");
            
            // Iniciamos el descenso de la senda peatonal de meta
            // Buscamos la capa de fondo correspondiente a la calle
            CapaParallax[] capas = GameObject.FindObjectsByType<CapaParallax>(FindObjectsSortMode.None);
            CapaParallax cpCalle = null;
            foreach (CapaParallax capa in capas)
            {
                // Identificamos el componente por su nombre
                if (capa.gameObject.name.Contains("ScrollingBackground") || capa.gameObject.name.Contains("Calle"))
                {
                    cpCalle = capa;
                    break;
                }
            }
            if (cpCalle == null && capas.Length > 0) cpCalle = capas[0];
            
            if (cpCalle != null) cpCalle.ForceFinalStreet();
            
            // Los vehículos continúan su avance de forma natural
            
            // Transición diferida a la pantalla de mejoras
            StartCoroutine(TransitionToUpgradeShop());
        }

        private void TriggerDefeat(bool outOfTime)
        {
            isGameRunning = false;
            isGameOver = true;
            isVictory = false;

            if (outOfTime)
            {
                Debug.Log("¡Se acabó el tiempo! No lograste entregar todos los pedidos.");
            }
            else
            {
                Debug.Log("¡Te quedaste sin vidas debido a los choques urbanos!");
            }

            if (AdministradorUI.Instance != null)
            {
                AdministradorUI.Instance.ShowGameOver();
            }
        }

        private IEnumerator TransitionToUpgradeShop()
        {
            // Esperamos a que la senda peatonal de meta se detenga por completo
            yield return new WaitForSeconds(4.5f);
            
            // Avanzamos al siguiente día de trabajo
            currentDay++;
            
            // Cargamos la escena de la tienda
        }

        /// <summary>
        /// Reinicia el día actual (nivel) restableciendo monedas de la jornada actual
        /// </summary>
        public void RestartCurrentDay()
        {
            StartNewDay();
        }
    }
}
