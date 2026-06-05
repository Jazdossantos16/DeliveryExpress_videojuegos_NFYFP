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
            StartCoroutine(SpawnRoutine());
        }

        private IEnumerator SpawnRoutine()
        {
            // Esperar que inicie el juego
            yield return new WaitForSeconds(1.5f);

            // Ajustar dificultad según el día de la jornada actual
            AdjustDifficultyBasedOnDay();

            while (canSpawn)
            {
                if (AdministradorJuego.Instance != null && AdministradorJuego.Instance.IsGameOver)
                {
                    yield return new WaitForSeconds(1.0f);
                    continue;
                }

                // Esperar tiempo aleatorio antes del siguiente spawn
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

                yield return new WaitForSeconds(Random.Range(currentMinDelay, currentMaxDelay));

                // Spawnear obstáculo
                SpawnRandomObstacle();
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

            // Determinar si spawnear 1 o 2 autos (50% de probabilidad para cada uno)
            int spawnCount = Random.Range(1, 3); // 1 o 2
            
            if (lanePositionsX.Length <= spawnCount)
            {
                spawnCount = lanePositionsX.Length - 1;
            }
            if (spawnCount <= 0) spawnCount = 1;

            // Elegir carriles sin repetir
            List<int> availableLaneIndices = new List<int>();
            for (int i = 0; i < lanePositionsX.Length; i++)
            {
                availableLaneIndices.Add(i);
            }

            for (int k = 0; k < spawnCount; k++)
            {
                int listIndex = Random.Range(0, availableLaneIndices.Count);
                int selectedLaneIndex = availableLaneIndices[listIndex];
                availableLaneIndices.RemoveAt(listIndex);

                SpawnCarInLane(selectedLaneIndex);
            }
        }

        private void SpawnCarInLane(int laneIndex)
        {
            if (carSprites == null || carSprites.Length == 0) return;

            float spawnX = lanePositionsX[laneIndex];
            
            // Crear el GameObject del auto
            GameObject carObj = new GameObject("Obstaculo_Auto");
            carObj.tag = "Car"; // Es detectado por el ControladorJugador para el daño
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
            col.size = new Vector2(1.2f, 1.4f);

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
                levelScrollSpeed = 4.0f;
                minSpawnDelay = 2.5f;
                maxSpawnDelay = 4.0f;
            }
            else if (day >= 2 && day <= 4)
            {
                levelScrollSpeed = 5.5f;
                minSpawnDelay = 1.8f;
                maxSpawnDelay = 3.0f;
            }
            else // Jornadas finales (Day >= 5)
            {
                levelScrollSpeed = 7.0f;
                minSpawnDelay = 1.0f;
                maxSpawnDelay = 2.0f;
            }
        }

        public void StopSpawning()
        {
            canSpawn = false;
        }
    }
}
