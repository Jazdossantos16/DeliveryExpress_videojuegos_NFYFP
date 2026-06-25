using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DeliveryExpress
{
    /// <summary>
    /// Generador procedural de obstáculos y tráfico en base a carriles virtuales y a la curva de dificultad por jornadas.
    /// </summary>
    public class GeneradorObstaculos : MonoBehaviour
    {
        [Header("Prefabs de Obstáculos")]
        [SerializeField] private GameObject[] obstaclePrefabs; // Colección indexada por enum TipoObstaculo

        [Header("Prefabs de Obstáculos (Configurados en Unity)")]
        [SerializeField] private GameObject conoPrefab;
        [SerializeField] private GameObject bachePrefab;
        [SerializeField] private GameObject basuraPrefab;
        [SerializeField] private GameObject[] autoPrefabs;

        [Header("Power-Up de Vida")]
        [SerializeField] private GameObject hamburguesaPowerUpPrefab;
        [Tooltip("Cada cuántos segundos puede aparecer una hamburguesa (tiempo mínimo entre apariciones)")]
        [SerializeField] private float minTiempoEntreHamburguesas = 12f;
        [Tooltip("Tiempo máximo entre apariciones de hamburguesas")]
        [SerializeField] private float maxTiempoEntreHamburguesas = 25f;

        [Header("Sprites de Casas (Entorno Vereda)")]
        [SerializeField] private Sprite[] houseSprites;

        [Header("Monedas")]
        [SerializeField] private GameObject monedaPrefab;
        [SerializeField] private float minTiempoEntreMonedas = 4f;
        [SerializeField] private float maxTiempoEntreMonedas = 7f;
        [SerializeField] private int minMonedasPorFila = 3;
        [SerializeField] private int maxMonedasPorFila = 5;

        private float tiempoParaSiguienteMoneda = 0f;

        [Header("Potenciador de Velocidad (Rayo)")]
        [SerializeField] private GameObject potenciadorEnergiaPrefab;
        [SerializeField] private float minTiempoEntrePotenciadores = 15f;
        [SerializeField] private float maxTiempoEntrePotenciadores = 30f;

        private float tiempoParaSiguientePotenciador = 0f;

        [Header("Configuración de Carriles (Posiciones X)")]
        [SerializeField] private float[] lanePositionsX = new float[] { -4f, 0f, 4f }; // Izquierdo, Centro, Derecho
        [SerializeField] private float spawnYPosition = 12f; // Posición de entrada superior en pantalla

        [Header("Configuración de Frecuencias de Spawn")]
        [SerializeField] private float minSpawnDelay = 1.8f;
        [SerializeField] private float maxSpawnDelay = 3.5f;
        
        [Tooltip("Velocidad de avance/scroll del nivel")]
        [SerializeField] private float levelScrollSpeed = 5f;

        private bool canSpawn = true;
        private float tiempoParaSiguienteHamburguesa = 0f;

        private float baseLevelScrollSpeed = 5f;
        private float baseMinSpawnDelay = 1.8f;
        private float baseMaxSpawnDelay = 3.5f;

        private void Start()
        {
            fondosCacheados = FindObjectsByType<CapaParallax>(FindObjectsSortMode.None);

            // Destruir cualquier hamburguesa power-up pre-colocada en la jerarquía de la escena al iniciar
            GameObject[] prePlacedBurgers = GameObject.FindGameObjectsWithTag("PowerUp");
            foreach (GameObject burger in prePlacedBurgers)
            {
                if (burger != null && burger.scene.name != null)
                {
                    Destroy(burger);
                }
            }

            // Primer hamburguesa aparece entre 15 y 25 segundos desde el inicio
            tiempoParaSiguienteHamburguesa = Random.Range(minTiempoEntreHamburguesas, maxTiempoEntreHamburguesas);
            // Primer moneda aparece entre 3 y 6 segundos desde el inicio
            tiempoParaSiguienteMoneda = Random.Range(minTiempoEntreMonedas, maxTiempoEntreMonedas);
            // Primer potenciador de velocidad aparece entre 15 y 30 segundos desde el inicio
            tiempoParaSiguientePotenciador = Random.Range(minTiempoEntrePotenciadores, maxTiempoEntrePotenciadores);
            StartCoroutine(SpawnRoutine());
        }

        private GameObject lastLeftHouse;
        private float lastLeftHouseHeight;
        private GameObject lastRightHouse;
        private CapaParallax[] fondosCacheados;
        private float lastRightHouseHeight;
        private bool isPreSpawned = false;

        private int lastLeftHouseIndex = -1;
        private int lastRightHouseIndex = -1;

        private System.Collections.Generic.Queue<int> leftHouseQueue = new System.Collections.Generic.Queue<int>();
        private System.Collections.Generic.Queue<int> rightHouseQueue = new System.Collections.Generic.Queue<int>();

        private int GetNextHouseIndex(System.Collections.Generic.Queue<int> queue, ref int lastIndex)
        {
            if (houseSprites == null || houseSprites.Length <= 2) return 0;
            int max = houseSprites.Length / 2;

            if (queue.Count == 0)
            {
                System.Collections.Generic.List<int> bag = new System.Collections.Generic.List<int>();
                for (int i = 0; i < max; i++) bag.Add(i);
                
                // Mezclar la lista
                for (int i = 0; i < bag.Count; i++)
                {
                    int temp = bag[i];
                    int randomIndex = Random.Range(i, bag.Count);
                    bag[i] = bag[randomIndex];
                    bag[randomIndex] = temp;
                }

                // Evitar que la primera casa de la nueva bolsa sea igual a la última de la bolsa anterior
                if (bag[0] == lastIndex && bag.Count > 1)
                {
                    int temp = bag[0];
                    bag[0] = bag[1];
                    bag[1] = temp;
                }

                foreach (int val in bag) queue.Enqueue(val);
            }
            
            int result = queue.Dequeue();
            lastIndex = result;
            return result;
        }

                private void Update()
        {
            if (!canSpawn) return;

            // Incremento progresivo de velocidad durante la partida
            if (AdministradorJuego.Instance != null && !AdministradorJuego.Instance.IsGameOver)
            {
                float progress = AdministradorJuego.Instance.LevelProgress;

                // Aceleramos cuadráticamente la velocidad de scroll (hasta un +90% al final de la partida)
                float speedMultiplier = 1f + (progress * progress * 0.9f);
                levelScrollSpeed = baseLevelScrollSpeed * speedMultiplier;

                // Aplicamos el boost del potenciador de velocidad si está activo
                if (ControladorJugador.Instance != null)
                {
                    levelScrollSpeed *= ControladorJugador.Instance.SpeedBoostMultiplier;
                }

                // Reducimos los tiempos de spawn delay linealmente (hasta un 40% más rápido al final de la partida)
                float spawnDelayMultiplier = Mathf.Lerp(1f, 0.6f, progress);
                minSpawnDelay = Mathf.Max(0.4f, baseMinSpawnDelay * spawnDelayMultiplier);
                maxSpawnDelay = Mathf.Max(0.7f, baseMaxSpawnDelay * spawnDelayMultiplier);

                SyncBackgroundSpeeds();

                // Spawn de hamburguesa power-up (solo si al jugador le falta vida y el temporizador expira)
                if (hamburguesaPowerUpPrefab != null && !AdministradorJuego.Instance.IsGameOver)
                {
                    if (AdministradorJuego.Instance.CurrentLives >= AdministradorJuego.Instance.StartingLives)
                    {
                        // Si el jugador ya tiene todas las vidas, mantenemos el temporizador reiniciado
                        tiempoParaSiguienteHamburguesa = Random.Range(minTiempoEntreHamburguesas, maxTiempoEntreHamburguesas);
                    }
                    else
                    {
                        tiempoParaSiguienteHamburguesa -= Time.deltaTime;
                        if (tiempoParaSiguienteHamburguesa <= 0f)
                        {
                            SpawnHamburguesa();
                            tiempoParaSiguienteHamburguesa = Random.Range(minTiempoEntreHamburguesas, maxTiempoEntreHamburguesas);
                        }
                    }
                }

                // Spawn de monedas (temporizador independiente)
                if (monedaPrefab != null && !AdministradorJuego.Instance.IsGameOver)
                {
                    tiempoParaSiguienteMoneda -= Time.deltaTime;
                    if (tiempoParaSiguienteMoneda <= 0f)
                    {
                        SpawnMonedaRow();
                        tiempoParaSiguienteMoneda = Random.Range(minTiempoEntreMonedas, maxTiempoEntreMonedas);
                    }
                }

                // Spawn de potenciador de velocidad (temporizador independiente)
                if (potenciadorEnergiaPrefab != null && !AdministradorJuego.Instance.IsGameOver)
                {
                    tiempoParaSiguientePotenciador -= Time.deltaTime;
                    if (tiempoParaSiguientePotenciador <= 0f)
                    {
                        SpawnPotenciadorEnergia();
                        tiempoParaSiguientePotenciador = Random.Range(minTiempoEntrePotenciadores, maxTiempoEntrePotenciadores);
                    }
                }
            }

                        CapaParallax[] fondos = GameObject.FindObjectsByType<CapaParallax>(FindObjectsSortMode.None);
            CapaParallax bg = null;
            foreach (CapaParallax capa in fondos)
            {
                if (capa.gameObject.name.Contains("ScrollingBackground") || capa.gameObject.name.Contains("Calle"))
                {
                    bg = capa;
                    break;
                }
            }
            if (bg == null && fondos.Length > 0) bg = fondos[0];
            bool isCrossroad = (bg != null && bg.IsCrossroadOverlapping(spawnYPosition));

            if (!isPreSpawned)
            {
                AdjustDifficultyBasedOnDay();
                PreSpawnHouses();
                isPreSpawned = true;
            }

            if (isCrossroad)
            {
                // Dejamos un espacio libre en el carril para simular el cruce de calles
                lastLeftHouse = null;
                lastRightHouse = null;
            }
            else
            {
                // Evitamos generar casas si la posición de spawn ya superó la línea de meta (punto de fuga)
                bool stopSpawningHouses = false;
                if (fondosCacheados != null)
                {
                    foreach (CapaParallax fondo in fondosCacheados)
                    {
                        if (fondo != null)
                        {
                            float vpY = fondo.GetWorldVanishingPointY();
                            if (vpY != float.MaxValue && vpY <= spawnYPosition)
                            {
                                stopSpawningHouses = true;
                                break;
                            }
                        }
                    }
                }

                if (!stopSpawningHouses)
                {
                    if (lastLeftHouse == null || lastLeftHouse.transform.position.y <= spawnYPosition - lastLeftHouseHeight)
                    {
                        int index = GetNextHouseIndex(leftHouseQueue, ref lastLeftHouseIndex);
                        // Asignamos índices pares para las casas del lateral izquierdo
                        lastLeftHouse = SpawnHouse(-1f, spawnYPosition, index * 2);
                        if (lastLeftHouse != null)
                        {
                            lastLeftHouseHeight = lastLeftHouse.GetComponent<SpriteRenderer>().bounds.size.y;
                        }
                    }

                    if (lastRightHouse == null || lastRightHouse.transform.position.y <= spawnYPosition - lastRightHouseHeight)
                    {
                        int index = GetNextHouseIndex(rightHouseQueue, ref lastRightHouseIndex);
                        // Asignamos índices impares para las casas del lateral derecho
                        lastRightHouse = SpawnHouse(1f, spawnYPosition, index * 2 + 1);
                        if (lastRightHouse != null)
                        {
                            lastRightHouseHeight = lastRightHouse.GetComponent<SpriteRenderer>().bounds.size.y;
                        }
                    }
                }
            }
        }

        private void PreSpawnHouses()
        {
            float currentY = -6f; // Iniciamos la generación por debajo del límite inferior de la pantalla
            while (currentY <= spawnYPosition)
            {
                int leftIndex = GetNextHouseIndex(leftHouseQueue, ref lastLeftHouseIndex);
                GameObject leftH = SpawnHouse(-1f, currentY, leftIndex * 2);
                
                int rightIndex = GetNextHouseIndex(rightHouseQueue, ref lastRightHouseIndex);
                GameObject rightH = SpawnHouse(1f, currentY, rightIndex * 2 + 1);

                if (leftH != null && rightH != null)
                {
                    float h = leftH.GetComponent<SpriteRenderer>().bounds.size.y;
                    lastLeftHouseHeight = h;
                    lastRightHouseHeight = h;
                    lastLeftHouse = leftH;
                    lastRightHouse = rightH;
                    currentY += h; // Incrementamos la coordenada Y en base al alto de la casa para la siguiente iteración
                }
                else
                {
                    break;
                }
            }
        }

        private IEnumerator SpawnRoutine()
        {
            yield return new WaitForSeconds(0.5f);

            AdjustDifficultyBasedOnDay();

            float timeElapsed = 0f;

            while (canSpawn)
            {
                // Detenemos la generación de obstáculos si el juego terminó o si restan menos de 1.5 segundos (meta despejada)
                if (AdministradorJuego.Instance != null && 
                   (AdministradorJuego.Instance.IsGameOver || AdministradorJuego.Instance.TimeRemaining < 1.5f))
                {
                    yield return new WaitForSeconds(1.0f);
                    continue;
                }

                SpawnRandomObstacle();

                float currentMinDelay = minSpawnDelay;
                float currentMaxDelay = maxSpawnDelay;

                // Reducimos los tiempos de espera de forma moderada a medida que avanzan los días para aumentar la densidad del tráfico
                if (AdministradorJuego.Instance != null)
                {
                    int day = AdministradorJuego.Instance.CurrentDay;
                    float difficultyFactor = Mathf.Clamp(1f - ((day - 1) * 0.05f), 0.8f, 1f);
                    currentMinDelay *= difficultyFactor;
                    currentMaxDelay *= difficultyFactor;
                }

                // ACELERACIÓN PROGRESIVA: Partidas de 60 segundos.
                // Reducimos el tiempo de espera hasta un 0.65x al final de la jornada.
                float inGameTimeFactor = Mathf.Clamp(1f - (timeElapsed / 60f), 0.65f, 1f);
                currentMinDelay *= inGameTimeFactor;
                currentMaxDelay *= inGameTimeFactor;

                float waitTime = Random.Range(currentMinDelay, currentMaxDelay);
                
                // Esperar para el siguiente
                yield return new WaitForSeconds(waitTime);
                timeElapsed += waitTime;
            }
        }

        private void SpawnRandomObstacle()
        {
            if (autoPrefabs != null && autoPrefabs.Length > 0)
            {
                SpawnTrafficWave();
                return;
            }

            // Fallback a prefabs antiguos si existen
            if (obstaclePrefabs != null && obstaclePrefabs.Length > 0)
            {
                int randomLane = Random.Range(0, lanePositionsX.Length);
                float spawnX = lanePositionsX[randomLane];

                int randomPrefabIndex = Random.Range(0, obstaclePrefabs.Length);
                GameObject selectedPrefab = obstaclePrefabs[randomPrefabIndex];

                if (selectedPrefab != null)
                {
                    Vector3 spawnPosition = new Vector3(spawnX, spawnYPosition, 0f);
                    GameObject spawnedObj = Instantiate(selectedPrefab, spawnPosition, Quaternion.identity);
                    Obstaculo obstacleComponent = spawnedObj.GetComponent<Obstaculo>();
                    if (obstacleComponent != null)
                    {
                        obstacleComponent.SetScrollSpeed(levelScrollSpeed);
                    }
                }
            }
        }

        /// <summary>
        /// Obtiene un carril que esté libre de obstáculos y otros coleccionables cerca del área de spawn.
        /// Si todos están ocupados, elige el carril que tenga el obstáculo más lejano.
        /// </summary>
        private int GetSafeLaneForPowerUp()
        {
            if (lanePositionsX == null || lanePositionsX.Length == 0) return 0;

            List<int> safeLanes = new List<int>();
            for (int i = 0; i < lanePositionsX.Length; i++)
            {
                safeLanes.Add(i);
            }

            // Buscar todos los obstáculos activos
            Obstaculo[] activeObstacles = FindObjectsByType<Obstaculo>(FindObjectsSortMode.None);
            
            // Buscar coleccionables activos para evitar encimarlos
            Moneda[] activeCoins = FindObjectsByType<Moneda>(FindObjectsSortMode.None);
            PotenciadorEnergia[] activeBoosts = FindObjectsByType<PotenciadorEnergia>(FindObjectsSortMode.None);
            HamburguesaVida[] activeBurgers = FindObjectsByType<HamburguesaVida>(FindObjectsSortMode.None);

            // Verificar obstáculos
            foreach (Obstaculo obs in activeObstacles)
            {
                if (obs == null) continue;
                // Si el obstáculo está cerca de la zona de spawn (Y > 6f)
                if (obs.transform.position.y > 6.0f)
                {
                    int lane = GetLaneIndexFromX(obs.transform.position.x);
                    if (lane != -1 && safeLanes.Contains(lane))
                    {
                        safeLanes.Remove(lane);
                    }
                }
            }

            // Verificar coleccionables
            foreach (Moneda coin in activeCoins)
            {
                if (coin == null) continue;
                if (coin.transform.position.y > 6.0f)
                {
                    int lane = GetLaneIndexFromX(coin.transform.position.x);
                    if (lane != -1 && safeLanes.Contains(lane))
                    {
                        safeLanes.Remove(lane);
                    }
                }
            }

            foreach (PotenciadorEnergia boost in activeBoosts)
            {
                if (boost == null) continue;
                if (boost.transform.position.y > 6.0f)
                {
                    int lane = GetLaneIndexFromX(boost.transform.position.x);
                    if (lane != -1 && safeLanes.Contains(lane))
                    {
                        safeLanes.Remove(lane);
                    }
                }
            }

            foreach (HamburguesaVida burger in activeBurgers)
            {
                if (burger == null) continue;
                if (burger.transform.position.y > 6.0f)
                {
                    int lane = GetLaneIndexFromX(burger.transform.position.x);
                    if (lane != -1 && safeLanes.Contains(lane))
                    {
                        safeLanes.Remove(lane);
                    }
                }
            }

            // Si hay carriles seguros, elegir uno al azar
            if (safeLanes.Count > 0)
            {
                return safeLanes[Random.Range(0, safeLanes.Count)];
            }

            // Fallback: Si todos están ocupados cerca de la zona de spawn, buscar el carril cuyo objeto más alto esté lo más bajo posible (máximo Y en cada carril)
            int bestLane = Random.Range(0, lanePositionsX.Length);
            float minMaxY = float.MaxValue;

            for (int i = 0; i < lanePositionsX.Length; i++)
            {
                float maxYInLane = float.MinValue;
                
                // Verificar obstáculos en el carril i
                foreach (Obstaculo obs in activeObstacles)
                {
                    if (obs == null) continue;
                    if (GetLaneIndexFromX(obs.transform.position.x) == i)
                    {
                        if (obs.transform.position.y > maxYInLane) maxYInLane = obs.transform.position.y;
                    }
                }
                
                // Verificar coleccionables en el carril i
                foreach (Moneda coin in activeCoins)
                {
                    if (coin == null) continue;
                    if (GetLaneIndexFromX(coin.transform.position.x) == i)
                    {
                        if (coin.transform.position.y > maxYInLane) maxYInLane = coin.transform.position.y;
                    }
                }
                
                foreach (PotenciadorEnergia boost in activeBoosts)
                {
                    if (boost == null) continue;
                    if (GetLaneIndexFromX(boost.transform.position.x) == i)
                    {
                        if (boost.transform.position.y > maxYInLane) maxYInLane = boost.transform.position.y;
                    }
                }

                foreach (HamburguesaVida burger in activeBurgers)
                {
                    if (burger == null) continue;
                    if (GetLaneIndexFromX(burger.transform.position.x) == i)
                    {
                        if (burger.transform.position.y > maxYInLane) maxYInLane = burger.transform.position.y;
                    }
                }

                if (maxYInLane < minMaxY)
                {
                    minMaxY = maxYInLane;
                    bestLane = i;
                }
            }

            return bestLane;
        }

        /// <summary>
        /// Genera una hamburguesa coleccionable en un carril seguro libre de obstáculos.
        /// </summary>
        private void SpawnHamburguesa()
        {
            if (hamburguesaPowerUpPrefab == null || lanePositionsX == null || lanePositionsX.Length == 0) return;

            // Elegir carril seguro
            int randomLane = GetSafeLaneForPowerUp();
            float spawnX = lanePositionsX[randomLane];

            Vector3 spawnPos = new Vector3(spawnX, spawnYPosition, 0f);
            GameObject burgerObj = Instantiate(hamburguesaPowerUpPrefab, spawnPos, Quaternion.identity);

            HamburguesaVida burgerComponent = burgerObj.GetComponent<HamburguesaVida>();
            if (burgerComponent != null)
            {
                burgerComponent.SetScrollSpeed(levelScrollSpeed);
            }

            Debug.Log($"🍔 Hamburguesa power-up generada en carril {randomLane} (x={spawnX:F2})");
        }

        /// <summary>
        /// Genera una fila vertical de monedas en un carril seguro libre de obstáculos.
        /// </summary>
        private void SpawnMonedaRow()
        {
            if (monedaPrefab == null || lanePositionsX == null || lanePositionsX.Length == 0) return;

            // Elegir carril seguro
            int randomLane = GetSafeLaneForPowerUp();
            float spawnX = lanePositionsX[randomLane];

            // Cantidad de monedas en la fila
            int count = Random.Range(minMonedasPorFila, maxMonedasPorFila + 1);

            // Spawneamos las monedas una tras otra separadas verticalmente
            float spacingY = 1.4f;
            for (int i = 0; i < count; i++)
            {
                Vector3 spawnPos = new Vector3(spawnX, spawnYPosition + (i * spacingY), 0f);
                GameObject coinObj = Instantiate(monedaPrefab, spawnPos, Quaternion.identity);
                Moneda coinComponent = coinObj.GetComponent<Moneda>();
                if (coinComponent != null)
                {
                    coinComponent.SetScrollSpeed(levelScrollSpeed);
                }
            }

            Debug.Log($"🪙 Fila de {count} monedas generada en carril {randomLane} (x={spawnX:F2})");
        }

        /// <summary>
        /// Genera un potenciador de velocidad en un carril seguro libre de obstáculos.
        /// </summary>
        private void SpawnPotenciadorEnergia()
        {
            if (potenciadorEnergiaPrefab == null || lanePositionsX == null || lanePositionsX.Length == 0) return;

            // Elegir carril seguro
            int randomLane = GetSafeLaneForPowerUp();
            float spawnX = lanePositionsX[randomLane];

            Vector3 spawnPos = new Vector3(spawnX, spawnYPosition, 0f);
            GameObject powerUpObj = Instantiate(potenciadorEnergiaPrefab, spawnPos, Quaternion.identity);

            PotenciadorEnergia powerUpComponent = powerUpObj.GetComponent<PotenciadorEnergia>();
            if (powerUpComponent != null)
            {
                powerUpComponent.SetScrollSpeed(levelScrollSpeed);
            }

            Debug.Log($"⚡ Potenciador de velocidad generado en carril {randomLane} (x={spawnX:F2})");
        }

        private void SpawnTrafficWave()
        {
            if (lanePositionsX == null || lanePositionsX.Length == 0) return;

            List<int> blockedLanes = new List<int>();

            // Revisar conos en pantalla (bloquean el carril por mucho tiempo porque son lentos)
            GameObject[] activeCones = GameObject.FindGameObjectsWithTag("Obstaculo");
            bool hasConesOnScreen = activeCones.Length > 0;
            foreach (GameObject cone in activeCones)
            {
                if (cone.transform.position.y > -4f) // Bloquean hasta casi el final de la pantalla
                {
                    int lane = GetLaneIndexFromX(cone.transform.position.x);
                    if (lane != -1 && !blockedLanes.Contains(lane)) blockedLanes.Add(lane);
                }
            }

            // Revisar autos en pantalla (bloquean el carril menos tiempo porque son rápidos)
            GameObject[] activeCars = GameObject.FindGameObjectsWithTag("Car");
            foreach (GameObject car in activeCars)
            {
                if (car.transform.position.y > 0f) // Bloquean solo la mitad superior
                {
                    int lane = GetLaneIndexFromX(car.transform.position.x);
                    if (lane != -1 && !blockedLanes.Contains(lane)) blockedLanes.Add(lane);
                }
            }

            // Elegir carriles libres
            List<int> availableLaneIndices = new List<int>();
            for (int i = 0; i < lanePositionsX.Length; i++)
            {
                if (!blockedLanes.Contains(i)) availableLaneIndices.Add(i);
            }

            // Si todos los carriles están ocupados (o hay peligro de choque), cancelamos el spawn esta vez
            if (availableLaneIndices.Count == 0) return;

            // Decidir cuántos autos spawnear (85% de probabilidad de 1 auto, 15% de 2 autos)
            int spawnCount = hasConesOnScreen ? 1 : (Random.value < 0.15f ? 2 : 1);
            if (spawnCount > availableLaneIndices.Count) spawnCount = availableLaneIndices.Count;

            bool spawnedConeInThisWave = false;

            for (int k = 0; k < spawnCount; k++)
            {
                int listIndex = Random.Range(0, availableLaneIndices.Count);
                int selectedLaneIndex = availableLaneIndices[listIndex];
                availableLaneIndices.RemoveAt(listIndex);

                // Si ya hay un cono en pantalla o ya spawneamos uno en esta ola, forzar que sea un auto
                bool forceCar = hasConesOnScreen || spawnedConeInThisWave;
                bool isCone = SpawnObjectInLane(selectedLaneIndex, forceCar);
                
                if (isCone) spawnedConeInThisWave = true;
            }
        }

        private int GetLaneIndexFromX(float xPos)
        {
            if (lanePositionsX == null) return -1;
            for (int i = 0; i < lanePositionsX.Length; i++)
            {
                if (Mathf.Abs(lanePositionsX[i] - xPos) < 0.5f) return i;
            }
            return -1;
        }

        private bool SpawnObjectInLane(int laneIndex, bool forceCar)
        {
            float spawnX = lanePositionsX[laneIndex];

            // 30% de probabilidad de spawnear un obstáculo menor, solo si no estamos forzando un auto
            bool spawnMinorObstacle = !forceCar && (conoPrefab != null && Random.value < 0.3f);

            if (spawnMinorObstacle)
            {
                SpawnMinorObstacleInLane(spawnX);
                return true;
            }
            else if (autoPrefabs != null && autoPrefabs.Length > 0)
            {
                int randomPrefabIndex = Random.Range(0, autoPrefabs.Length);
                GameObject selectedPrefab = autoPrefabs[randomPrefabIndex];
                if (selectedPrefab != null)
                {
                    Obstaculo obsComp = selectedPrefab.GetComponent<Obstaculo>();
                    if (obsComp != null)
                    {
                        float carOwnSpeed = Obstaculo.GetOwnSpeedForType(obsComp.Type);
                        float carSpeed = levelScrollSpeed + carOwnSpeed;

                        // Si el auto va a generar un bloqueo completo de los 3 carriles, lo degradamos a un cono
                        if (WillVehicleCreateBlockade(laneIndex, carSpeed))
                        {
                            SpawnMinorObstacleInLane(spawnX);
                            return true; // Ahora es un cono
                        }
                    }

                    SpawnVehicleWithPrefab(spawnX, selectedPrefab);
                    return false;
                }
            }
            return false;
        }

        private void SpawnMinorObstacleInLane(float spawnX)
        {
            List<GameObject> availableMinors = new List<GameObject>();
            if (conoPrefab != null) availableMinors.Add(conoPrefab);
            if (basuraPrefab != null) availableMinors.Add(basuraPrefab);

            if (availableMinors.Count == 0) return;

            int randomIndex = Random.Range(0, availableMinors.Count);
            GameObject selectedPrefab = availableMinors[randomIndex];

            Vector3 spawnPosition = new Vector3(spawnX, spawnYPosition, 0f);
            GameObject spawnedObj = Instantiate(selectedPrefab, spawnPosition, Quaternion.identity);
            Obstaculo obstacleComponent = spawnedObj.GetComponent<Obstaculo>();
            if (obstacleComponent != null)
            {
                obstacleComponent.SetScrollSpeed(levelScrollSpeed);
            }
        }

        private void SpawnVehicleInLane(float spawnX)
        {
            if (autoPrefabs == null || autoPrefabs.Length == 0) return;
            int randomPrefabIndex = Random.Range(0, autoPrefabs.Length);
            GameObject selectedPrefab = autoPrefabs[randomPrefabIndex];
            if (selectedPrefab != null)
            {
                SpawnVehicleWithPrefab(spawnX, selectedPrefab);
            }
        }

        private void SpawnVehicleWithPrefab(float spawnX, GameObject prefab)
        {
            Vector3 spawnPosition = new Vector3(spawnX, spawnYPosition, 0f);
            GameObject spawnedObj = Instantiate(prefab, spawnPosition, Quaternion.identity);
            Obstaculo obstacleComponent = spawnedObj.GetComponent<Obstaculo>();
            if (obstacleComponent != null)
            {
                obstacleComponent.SetMovementDirection(Vector2.up);
                obstacleComponent.SetScrollSpeed(levelScrollSpeed);
            }
        }

        /// <summary>
        /// Predice si un vehículo a una velocidad específica causará que los tres carriles 
        /// queden bloqueados en un mismo punto de la pantalla por alineación.
        /// </summary>
        private bool WillVehicleCreateBlockade(int proposedLane, float vehicleSpeed)
        {
            List<Obstaculo> activeObstacles = new List<Obstaculo>();
            GameObject[] cones = GameObject.FindGameObjectsWithTag("Obstaculo");
            GameObject[] cars = GameObject.FindGameObjectsWithTag("Car");

            foreach (GameObject go in cones)
            {
                if (go == null) continue;
                Obstaculo obs = go.GetComponent<Obstaculo>();
                if (obs != null) activeObstacles.Add(obs);
            }
            foreach (GameObject go in cars)
            {
                if (go == null) continue;
                Obstaculo obs = go.GetComponent<Obstaculo>();
                if (obs != null) activeObstacles.Add(obs);
            }

            int laneA = -1;
            int laneB = -1;
            for (int i = 0; i < lanePositionsX.Length; i++)
            {
                if (i == proposedLane) continue;
                if (laneA == -1) laneA = i;
                else laneB = i;
            }

            if (laneA == -1 || laneB == -1) return false;

            List<Obstaculo> obstaclesInLaneA = new List<Obstaculo>();
            List<Obstaculo> obstaclesInLaneB = new List<Obstaculo>();

            foreach (Obstaculo obs in activeObstacles)
            {
                int lane = GetLaneIndexFromX(obs.transform.position.x);
                if (lane == laneA) obstaclesInLaneA.Add(obs);
                else if (lane == laneB) obstaclesInLaneB.Add(obs);
            }

            foreach (Obstaculo obsA in obstaclesInLaneA)
            {
                float vA = obsA.GetSpeedWithoutMultiplier();
                if (Mathf.Approximately(vehicleSpeed, vA) || vehicleSpeed < vA) continue;

                float t = (spawnYPosition - obsA.transform.position.y) / (vehicleSpeed - vA);
                if (t <= 0f) continue;

                float yAlign = spawnYPosition - vehicleSpeed * t;
                if (yAlign <= -10f) continue;

                foreach (Obstaculo obsB in obstaclesInLaneB)
                {
                    float vB = obsB.GetSpeedWithoutMultiplier();
                    float yBAtT = obsB.transform.position.y - vB * t;

                    // Si están a menos de 2.5 unidades de distancia vertical, el carril queda tapado completamente
                    if (Mathf.Abs(yBAtT - yAlign) < 2.5f)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void AdjustDifficultyBasedOnDay()
        {
            if (AdministradorJuego.Instance == null) return;

            int day = AdministradorJuego.Instance.CurrentDay;

            // Estructura de Jornadas del GDD:
            // Jornada 1: Tutorial, pocos obstáculos, velocidad lenta.
            // Jornadas 2-4: Mayor densidad.
            // Jornadas finales: Máxima presión, más tráfico, mayor velocidad del scroll.
            if (day == 1)
            {
                baseLevelScrollSpeed = 5.0f;
                baseMinSpawnDelay = 1.8f;
                baseMaxSpawnDelay = 2.8f;
            }
            else if (day >= 2 && day <= 4)
            {
                baseLevelScrollSpeed = 6.5f;
                baseMinSpawnDelay = 1.4f;
                baseMaxSpawnDelay = 2.2f;
            }
            else // Jornadas finales (Day >= 5)
            {
                baseLevelScrollSpeed = 8.0f;
                baseMinSpawnDelay = 1.1f;
                baseMaxSpawnDelay = 1.7f;
            }

            levelScrollSpeed = baseLevelScrollSpeed;
            minSpawnDelay = baseMinSpawnDelay;
            maxSpawnDelay = baseMaxSpawnDelay;

            SyncBackgroundSpeeds();
        }

                        private void SyncBackgroundSpeeds()
        {
            if (fondosCacheados != null)
            {
                foreach (CapaParallax fondo in fondosCacheados)
                {
                    fondo.SetBaseSpeed(levelScrollSpeed);
                }
            }
            
            // Actualizar la variable estática de Obstaculo para que todas las casas y autos ya spawneados 
            // aceleren en perfecta sincronía con el fondo.
            Obstaculo.SetGlobalScrollSpeed(levelScrollSpeed);
        }

        public void StopSpawning()
        {
            canSpawn = false;
        }



        private GameObject SpawnHouse(float spawnX, float spawnY, int spriteIndex)
        {
            if (houseSprites == null || houseSprites.Length == 0) return null;

            GameObject houseObj = new GameObject("Entorno_Casa");
            houseObj.transform.position = new Vector3(spawnX, spawnY, 0f);
            houseObj.transform.rotation = Quaternion.identity;
            
            // Escala de las casas (los sprites originales miden 600x600 aprox)
            houseObj.transform.localScale = new Vector3(0.65f, 0.65f, 1f);

            SpriteRenderer sr = houseObj.AddComponent<SpriteRenderer>();

            sr.sprite = houseSprites[spriteIndex];
            sr.sortingOrder = 2; // Debajo de los obstáculos (8) y el jugador (10), pero encima de la calle (0)

            // Ajuste fino: Alineamos TODAS las casas por su FACHADA (borde interno hacia la calle)
            // Empujamos la fachada lo más atrás posible (hacia el borde de la pantalla) 
            // garantizando que la casa más ancha no se corte.
            if (sr.sprite != null)
            {
                float scaledWidth = sr.sprite.bounds.size.x * 0.65f;
                
                float screenEdgeX = 8.9f;
                if (Camera.main != null)
                {
                    screenEdgeX = Camera.main.orthographicSize * Camera.main.aspect;
                }
                
                // El ancho máximo de tus dibujos es de aprox 6.8 unidades
                float maxHouseWidth = 6.8f * 0.65f; // ~4.42f
                
                // Fijamos la línea de la vereda (fachada) para que la casa más ancha toque justo el borde negro
                float facadeX = screenEdgeX - maxHouseWidth;
                
                // Si está a la izquierda, la fachada es el borde derecho de la casa.
                // Si está a la derecha, la fachada es el borde izquierdo de la casa.
                float finalX = (spawnX < 0) ? (-facadeX - (scaledWidth / 2f)) : (facadeX + (scaledWidth / 2f));
                
                houseObj.transform.position = new Vector3(finalX, spawnY, 0f);
            }

            // Usamos la lógica de movimiento de Obstaculo para que acompañe el parallax de la calle
            Obstaculo obstacle = houseObj.AddComponent<Obstaculo>();
            var typeField = typeof(Obstaculo).GetField("type", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (typeField != null)
            {
                typeField.SetValue(obstacle, TipoObstaculo.Cone); // Cone no tiene velocidad propia, se mueve con la calle
            }
            obstacle.SetScrollSpeed(levelScrollSpeed);

            return houseObj;
        }
    }
}
