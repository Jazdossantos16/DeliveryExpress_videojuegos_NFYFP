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
        [SerializeField] private int startingLives = 99; // 99 vidas para poder probar el escenario tranquilo
        [SerializeField] private float baseLevelDuration = 60f; // Duración base en segundos por jornada

        [Header("Configuración de Jornadas")]
        [SerializeField] private int currentDay = 1;
        [SerializeField] private int baseOrdersToDeliver = 2; // Jornada 1: 2 pedidos (porque hay 2 NPCs estáticos)

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
        [HideInInspector] public float extraTimeUpgrade = 0f; // Tiempo Extra (+segundos)

        // Eventos para actualizar la UI en Unity
        public event Action<int> OnLivesChanged;
        public event Action<float> OnTimeChanged;
        public event Action<int> OnCoinsChanged;
        public event Action<int, int> OnDeliveriesChanged; // (Completados, Requeridos)
        public event Action<int> OnOrdersWeightChanged; // (Pedidos cargados actualmente)

        public int ActiveOrders => activeOrders;
        public int Coins => coinsAccumulated;
        public bool IsGameOver => isGameOver;
        public bool IsVictory => isVictory;
        public int CurrentDay => currentDay;

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

            currentLives = startingLives;
            
            // Forzamos el tiempo a 60 segundos ignorando el Inspector
            baseLevelDuration = 60f;
            // Cargar tiempo base más la mejora permanente comprada en la tienda
            timeRemaining = baseLevelDuration + extraTimeUpgrade;

            // Forzar pedidos a 0 porque la entrega es al final del recorrido!
            baseOrdersToDeliver = 0;
            totalDeliveriesRequired = 0;
            currentDeliveriesCompleted = 0;

            // Al inicio del nivel, el repartidor sale cargado con todos sus pedidos asignados del restaurante
            activeOrders = totalDeliveriesRequired;

            // Emitir los estados iniciales
            OnLivesChanged?.Invoke(currentLives);
            OnTimeChanged?.Invoke(timeRemaining);
            OnCoinsChanged?.Invoke(coinsAccumulated);
            OnDeliveriesChanged?.Invoke(currentDeliveriesCompleted, totalDeliveriesRequired);
            OnOrdersWeightChanged?.Invoke(activeOrders);
        }

        private void Update()
        {
            if (!isGameRunning || isGameOver) return;

            // Manejo del temporizador (cuenta regresiva)
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                OnTimeChanged?.Invoke(timeRemaining);
            }
            else
            {
                timeRemaining = 0;
                OnTimeChanged?.Invoke(timeRemaining);
                
                // Si llegamos al final del tiempo, comprobamos si hicimos suficientes entregas
                if (currentDeliveriesCompleted >= totalDeliveriesRequired)
                {
                    TriggerVictory();
                }
                else
                {
                    TriggerDefeat(true); // Derrota por falta de tiempo / entregas
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
                TriggerDefeat(false); // Derrota por falta de vidas
            }
        }

        /// <summary>
        /// Ejecuta una entrega de pedido exitosa a un NPC cliente
        /// </summary>
        public void CompleteDelivery(int coinsReward)
        {
            if (isGameOver || activeOrders <= 0) return;

            activeOrders--;
            currentDeliveriesCompleted++;

            // Sumar monedas obtenidas de la entrega
            coinsAccumulated += coinsReward;

            OnDeliveriesChanged?.Invoke(currentDeliveriesCompleted, totalDeliveriesRequired);
            OnCoinsChanged?.Invoke(coinsAccumulated);
            OnOrdersWeightChanged?.Invoke(activeOrders);

            // Ya no disparamos la victoria acá, hay que sobrevivir hasta que se acabe el tiempo!
            // if (currentDeliveriesCompleted >= totalDeliveriesRequired)
            // {
            //     TriggerVictory();
            // }
        }

        /// <summary>
        /// Suma monedas al jugador (por ejemplo, al recolectar bonos de la calle)
        /// </summary>
        public void AddCoins(int amount)
        {
            coinsAccumulated += amount;
            OnCoinsChanged?.Invoke(coinsAccumulated);
        }

        /// <summary>
        /// Resta monedas al realizar compras en la tienda de mejoras
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
            
            // Forzar que la meta baje de inmediato en el fondo
            CapaParallax cp = GameObject.FindFirstObjectByType<CapaParallax>();
            if (cp != null) cp.ForceFinalStreet();
            
            // Destruir todos los autos que quedaron en la calle para que no pisen la meta
            GameObject[] cars = GameObject.FindGameObjectsWithTag("Car");
            foreach (GameObject car in cars)
            {
                Destroy(car);
            }
            
            // Abrir pantalla de Upgrades de forma diferida
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
            // Esperar 4.5 segundos para dar tiempo a que aparezca y baje la meta
            yield return new WaitForSeconds(4.5f);
            
            // Avanzar el día para la siguiente jornada
            currentDay++;
            
            // Cargar escena de la tienda de mejoras
            // SceneManager.LoadScene("UpgradeShopScene");
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
