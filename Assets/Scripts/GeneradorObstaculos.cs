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

        [Header("Sprites de Autos (Imagen Auto)")]
        [SerializeField] private Sprite[] carSprites;

        [Header("Sprites de Obstáculos Menores (Ej: Cono)")]
        [SerializeField] private Sprite[] minorObstacleSprites;

        [Header("Sprites de Casas (Entorno Vereda)")]
        [SerializeField] private Sprite[] houseSprites;

        [Header("Configuración de Carriles (Posiciones X)")]
        [SerializeField] private float[] lanePositionsX = new float[] { -4f, 0f, 4f }; // Izquierdo, Centro, Derecho
        [SerializeField] private float spawnYPosition = 12f; // Posición de entrada superior en pantalla

        [Header("Configuración de Frecuencias de Spawn")]
        [SerializeField] private float minSpawnDelay = 1.8f;
        [SerializeField] private float maxSpawnDelay = 3.5f;
        
        [Tooltip("Velocidad de avance/scroll del nivel")]
        [SerializeField] private float levelScrollSpeed = 5f;

        private bool canSpawn = true;

        private void Start()
        {
            // Autocargar los nuevos sprites de obstáculos (ahora son múltiples en una sola imagen)
            if (minorObstacleSprites == null || minorObstacleSprites.Length == 0)
            {
                Sprite[] loadedSprites = Resources.LoadAll<Sprite>("imagen_obstaculos");
                if (loadedSprites != null && loadedSprites.Length > 0)
                {
                    minorObstacleSprites = loadedSprites; // Cargamos todos los obstáculos cortados
                }
            }

            // Autocargar las imágenes de las casas para la vereda
            if (houseSprites == null || houseSprites.Length == 0)
            {
                Sprite[] loadedHouses = Resources.LoadAll<Sprite>("imagenes_ casas");
                if (loadedHouses != null && loadedHouses.Length > 0)
                {
                    houseSprites = loadedHouses;
                }
            }

            StartCoroutine(SpawnRoutine());
        }

        private GameObject lastLeftHouse;
        private float lastLeftHouseHeight;
        private GameObject lastRightHouse;
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
            // Quitamos la restricción de IsGameOver para que las casas sigan rellenando el fondo
            // mientras la calle se mueve durante la animación de la línea de llegada.

            CapaParallax bg = GameObject.FindFirstObjectByType<CapaParallax>();
            bool isCrossroad = (bg != null && bg.IsCrossroadOverlapping(spawnYPosition));

            if (!isPreSpawned)
            {
                AdjustDifficultyBasedOnDay(); // Ajustar la velocidad primero para que coincida con el fondo
                PreSpawnHouses();
                isPreSpawned = true;
            }

            if (isCrossroad)
            {
                // Dejar hueco para la calle transversal
                lastLeftHouse = null;
                lastRightHouse = null;
            }
            else
            {
                // Casa izquierda pegada al borde
                if (lastLeftHouse == null || lastLeftHouse.transform.position.y <= spawnYPosition - lastLeftHouseHeight)
                {
                    int index = GetNextHouseIndex(leftHouseQueue, ref lastLeftHouseIndex);
                    // Usamos index * 2 (pares) para la izquierda
                    lastLeftHouse = SpawnHouse(-1f, spawnYPosition, index * 2);
                    if (lastLeftHouse != null)
                    {
                        lastLeftHouseHeight = lastLeftHouse.GetComponent<SpriteRenderer>().bounds.size.y;
                    }
                }

                // Casa derecha pegada al borde
                if (lastRightHouse == null || lastRightHouse.transform.position.y <= spawnYPosition - lastRightHouseHeight)
                {
                    int index = GetNextHouseIndex(rightHouseQueue, ref lastRightHouseIndex);
                    // Usamos index * 2 + 1 (impares) para la derecha
                    lastRightHouse = SpawnHouse(1f, spawnYPosition, index * 2 + 1);
                    if (lastRightHouse != null)
                    {
                        lastRightHouseHeight = lastRightHouse.GetComponent<SpriteRenderer>().bounds.size.y;
                    }
                }
            }
        }

        private void PreSpawnHouses()
        {
            float currentY = -6f; // Empezar por debajo del borde inferior de la cámara
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
                    currentY += h; // Subir el equivalente a una casa para la próxima
                }
                else
                {
                    break;
                }
            }
        }

        private IEnumerator SpawnRoutine()
        {
            // Esperar un instante pequeñísimo que inicie el juego
            yield return new WaitForSeconds(0.5f);

            // Ajustar dificultad según el día de la jornada actual
            AdjustDifficultyBasedOnDay();

            float timeElapsed = 0f;

            while (canSpawn)
            {
                // Verificación: si el juego terminó o estamos en los últimos 3.5 segundos (acercándonos a la meta)
                // dejamos de generar obstáculos para que la línea de llegada esté despejada.
                if (AdministradorJuego.Instance != null && 
                   (AdministradorJuego.Instance.IsGameOver || AdministradorJuego.Instance.TimeRemaining < 3.5f))
                {
                    yield return new WaitForSeconds(1.0f);
                    continue;
                }

                // Spawnear obstáculo INMEDIATAMENTE al inicio del ciclo
                SpawnRandomObstacle();

                // Calcular cuánto tiempo esperar ANTES de spawnear el SIGUIENTE obstáculo
                float currentMinDelay = minSpawnDelay;
                float currentMaxDelay = maxSpawnDelay;

                // Reducir tiempos de espera a medida que avanzan los días para mayor densidad
                if (AdministradorJuego.Instance != null)
                {
                    int day = AdministradorJuego.Instance.CurrentDay;
                    float difficultyFactor = Mathf.Clamp(1f - ((day - 1) * 0.12f), 0.45f, 1f);
                    currentMinDelay *= difficultyFactor;
                    currentMaxDelay *= difficultyFactor;
                }

                // ACELERACIÓN PROGRESIVA: Partidas de 60 segundos.
                // Reducimos el tiempo de espera pero con un límite más conservador (0.65x)
                // para que no salgan "autos pegados" uno detrás del otro.
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
            // Si tenemos sprites de autos cargados, usamos la nueva lógica de tráfico dinámico
            if (carSprites != null && carSprites.Length > 0)
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

            // Decidir cuántos autos spawnear
            int spawnCount = hasConesOnScreen ? 1 : Random.Range(1, 3);
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
            bool spawnMinorObstacle = !forceCar && (minorObstacleSprites != null && minorObstacleSprites.Length > 0 && Random.value < 0.3f);

            if (spawnMinorObstacle)
            {
                SpawnMinorObstacleInLane(spawnX);
                return true;
            }
            else if (carSprites != null && carSprites.Length > 0)
            {
                SpawnVehicleInLane(spawnX);
                return false;
            }
            return false;
        }



        private void SpawnMinorObstacleInLane(float spawnX)
        {
            GameObject obsObj = new GameObject("Obstaculo_Cono");
            obsObj.tag = "Obstaculo"; // Tag "Obstaculo" quita 1 sola vida en ControladorJugador
            obsObj.transform.position = new Vector3(spawnX, spawnYPosition, 0f);
            obsObj.transform.rotation = Quaternion.identity;
            
            // Ajustamos el tamaño a 1.25f para lograr el punto medio perfecto
            obsObj.transform.localScale = new Vector3(1.25f, 1.25f, 1f); 

            GameObject visualObj = new GameObject("Visual");
            visualObj.transform.SetParent(obsObj.transform, false);

            SpriteRenderer sr = visualObj.AddComponent<SpriteRenderer>();
            sr.sprite = minorObstacleSprites[Random.Range(0, minorObstacleSprites.Length)];
            sr.sortingOrder = 8;

            if (sr.sprite != null)
            {
                Vector3 centerOffset = sr.sprite.bounds.center;
                visualObj.transform.localPosition = new Vector3(-centerOffset.x, -centerOffset.y, 0f);
            }

            BoxCollider2D col = obsObj.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(0.9f, 0.9f); // Collider más pequeño para el cono

            Obstaculo obstacle = obsObj.AddComponent<Obstaculo>();
            
            var typeField = typeof(Obstaculo).GetField("type", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (typeField != null)
            {
                typeField.SetValue(obstacle, TipoObstaculo.Cone); // Tipo cono (ownSpeed = 0)
            }

            // Los conos no tienen velocidad propia, solo se mueven con la calle
            obstacle.SetScrollSpeed(levelScrollSpeed);
        }

        private void SpawnVehicleInLane(float spawnX)
        {
            if (carSprites == null || carSprites.Length == 0) return;

            // Crear el GameObject del auto
            GameObject carObj = new GameObject("Obstaculo_Auto");
            carObj.tag = "Car"; // Es detectado por el ControladorJugador para muerte instantánea
            carObj.transform.position = new Vector3(spawnX, spawnYPosition, 0f);
            
            // Sin rotación (los sprites originales en imagen_auto.png ya están orientados hacia abajo)
            carObj.transform.rotation = Quaternion.identity;
            
            // Aumentar la escala de los autos a 1.95f para que sean grandes y realistas, y al estar centrados quepan justo dentro de la línea blanca
            carObj.transform.localScale = new Vector3(1.95f, 1.95f, 1f);

            // Crear el objeto visual hijo para poder centrar el sprite usando su bounds center (corrige pivots incorrectos)
            GameObject visualObj = new GameObject("Visual");
            visualObj.transform.SetParent(carObj.transform, false);

            // Añadir SpriteRenderer al visual
            SpriteRenderer sr = visualObj.AddComponent<SpriteRenderer>();
            int randomSpriteIndex = Random.Range(0, carSprites.Length);
            sr.sprite = carSprites[randomSpriteIndex];
            sr.sortingOrder = 8; // Por encima de la calle, debajo del repartidor (sortingOrder 10)

            // Centrar el sprite usando su centro geométrico local
            if (sr.sprite != null)
            {
                Vector3 centerOffset = sr.sprite.bounds.center;
                visualObj.transform.localPosition = new Vector3(-centerOffset.x, -centerOffset.y, 0f);
            }

            // Añadir BoxCollider2D como Trigger al padre
            BoxCollider2D col = carObj.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            if (sr.sprite != null)
            {
                // Ajustar el tamaño del collider para que cubra casi todo el sprite
                col.size = new Vector2(sr.sprite.bounds.size.x * 0.85f, sr.sprite.bounds.size.y * 0.9f);
                // Alinear el centro del collider al centro desplazado del sprite visual
                col.offset = new Vector2(-sr.sprite.bounds.center.x, -sr.sprite.bounds.center.y);
            }
            else
            {
                col.size = new Vector2(1.2f, 1.4f);
            }

            // Añadir componente Obstaculo al padre
            Obstaculo obstacle = carObj.AddComponent<Obstaculo>();
            
            // Configurar el tipo de auto y velocidad en Obstaculo.cs por reflexión
            var typeField = typeof(Obstaculo).GetField("type", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (typeField != null)
            {
                // Alternar tipo de auto (BlackCar o GreenCar) según el sprite seleccionado
                TipoObstaculo carType = (randomSpriteIndex % 2 == 0) ? TipoObstaculo.BlackCar : TipoObstaculo.GreenCar;
                typeField.SetValue(obstacle, carType);
            }

            // Los autos van en sentido contrario a el chico (Vector2.up en la ecuación final de Obstaculo bajan a scrollSpeed + ownSpeed)
            obstacle.SetMovementDirection(Vector2.up);
            obstacle.SetScrollSpeed(levelScrollSpeed);
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
                levelScrollSpeed = 5.0f;
                minSpawnDelay = 1.3f;
                maxSpawnDelay = 2.2f;
            }
            else if (day >= 2 && day <= 4)
            {
                levelScrollSpeed = 6.5f;
                minSpawnDelay = 1.0f;
                maxSpawnDelay = 1.8f;
            }
            else // Jornadas finales (Day >= 5)
            {
                levelScrollSpeed = 8.0f;
                minSpawnDelay = 0.8f;
                maxSpawnDelay = 1.4f;
            }

            SyncBackgroundSpeeds();
        }

        private void SyncBackgroundSpeeds()
        {
            CapaParallax[] fondos = FindObjectsOfType<CapaParallax>();
            foreach (CapaParallax fondo in fondos)
            {
                fondo.SetBaseSpeed(levelScrollSpeed);
            }
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
