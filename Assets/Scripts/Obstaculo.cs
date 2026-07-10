using UnityEngine;
using System.Collections;

namespace DeliveryExpress
{
    public enum TipoObstaculo
    {
        BlackCar,
        GreenCar,
        Cone,
        Pothole, // Bache
        Pedestrian
    }

    /// <summary>
    /// Define el comportamiento de los obstáculos y vehículos en el asfalto.
    /// </summary>
    public class Obstaculo : MonoBehaviour
    {
        [Header("Configuración del Obstáculo")]
        [SerializeField] private TipoObstaculo type;
        public TipoObstaculo Type => type;
        [SerializeField] private float ownSpeed = 2f; // Velocidad de movimiento propio
        
        [Header("Cambio de Carril")]
        private bool isChangingLane = false;
        private float startX;
        private float targetX;
        private float laneChangeTimer = 0f;
        private float laneChangeDuration = 0.8f; // Tiempo de transición lateral
        private GameObject turnSignalObject;
        public bool IsChangingOrPlanning { get; private set; } = false;
        public float TargetX => targetX;
        public bool IsChangingLane => isChangingLane;

        private static float globalStreetScrollSpeed = 4f; // Velocidad del scroll de la calle
        public static float GlobalStreetScrollSpeed => globalStreetScrollSpeed;
        private float destroyYBound = -10f;       // Límite inferior para reciclar/destruir el objeto

        private Vector2 movementDirection = Vector2.down;

        private void Start()
        {
            switch (type)
            {
                case TipoObstaculo.BlackCar:
                    ownSpeed = 3.5f;
                    movementDirection = Vector2.up;
                    break;
                case TipoObstaculo.GreenCar:
                    ownSpeed = 5.0f;
                    movementDirection = Vector2.up;
                    break;
                case TipoObstaculo.Cone:
                    ownSpeed = 0f;
                    break;
                case TipoObstaculo.Pothole:
                    ownSpeed = 0f;
                    break;
                case TipoObstaculo.Pedestrian:
                    ownSpeed = 0.5f;
                    movementDirection = new Vector2(Random.value > 0.5f ? 1f : -1f, -1f).normalized;
                    break;
            }

            // Evaluar cambio de carril al aparecer (solo para autos)
            if (type == TipoObstaculo.BlackCar || type == TipoObstaculo.GreenCar)
            {
                EvaluateLaneChange();
            }
        }

        private void EvaluateLaneChange()
        {
            // Si estamos en el Nivel 1, verificar el límite cuantitativo de cambios de carril
            if (AdministradorJuego.Instance != null && AdministradorJuego.Instance.CurrentDay == 1)
            {
                if (AdministradorJuego.Instance.LaneChangesThisDay >= 5)
                {
                    Debug.Log($"[BlinkerDiagnostico] {gameObject.name} - Abortado: Se alcanzó el límite de 5 cambios de carril para el Nivel 1.");
                    return;
                }
            }

            float progress = 0f;
            if (AdministradorJuego.Instance != null)
            {
                progress = AdministradorJuego.Instance.LevelProgress;
            }

            // Determinar rango de probabilidad según el nivel (currentDay)
            float minProbability = 0f;
            float maxProbability = 0.30f; // Ajustado de 20% a 30% max para Nivel 1

            if (AdministradorJuego.Instance != null && AdministradorJuego.Instance.CurrentDay >= 2)
            {
                minProbability = 0.20f; // 20% al inicio
                maxProbability = 0.30f; // 30% al final (ajustado de 40% a 30%)
            }

            float currentProbability = Mathf.Lerp(minProbability, maxProbability, progress);

            // Log de diagnóstico al evaluar
            Debug.Log($"[BlinkerDiagnostico] {gameObject.name} (X: {transform.position.x:F2}, Y: {transform.position.y:F2}) - Iniciando evaluación de cambio de carril. Nivel: {(AdministradorJuego.Instance != null ? AdministradorJuego.Instance.CurrentDay : 1)}, Progreso: {progress * 100:F1}%, Probabilidad: {currentProbability * 100:F1}%");

            if (Random.value < currentProbability)
            {
                Debug.Log($"[BlinkerDiagnostico] {gameObject.name} - Probabilidad superada ({currentProbability * 100:F1}%). Determinando carril destino...");

                GeneradorObstaculos generador = FindFirstObjectByType<GeneradorObstaculos>();
                if (generador == null)
                {
                    Debug.LogWarning($"[BlinkerDiagnostico] {gameObject.name} - Abortado: GeneradorObstaculos no encontrado en la escena.");
                    return;
                }

                float[] lanePositionsX = generador.LanePositionsX;
                if (lanePositionsX == null || lanePositionsX.Length == 0)
                {
                    Debug.LogWarning($"[BlinkerDiagnostico] {gameObject.name} - Abortado: LanePositionsX es nulo o vacío.");
                    return;
                }

                int currentLane = generador.GetLaneIndexFromX(transform.position.x);
                if (currentLane == -1)
                {
                    Debug.LogWarning($"[BlinkerDiagnostico] {gameObject.name} - Abortado: No se pudo determinar el carril actual para X: {transform.position.x:F2}.");
                    return;
                }

                // Elegir el carril destino
                int targetLane = -1;
                if (currentLane == 0)
                {
                    targetLane = 1; // Desde izquierdo al centro
                }
                else if (currentLane == 2)
                {
                    targetLane = 1; // Desde derecho al centro
                }
                else if (currentLane == 1)
                {
                    // Desde el centro hacia izquierda o derecha
                    targetLane = Random.value < 0.5f ? 0 : 2;
                }

                if (targetLane == -1) return;

                float checkTargetX = lanePositionsX[targetLane];
                Debug.Log($"[BlinkerDiagnostico] {gameObject.name} - Carril actual: {currentLane} (X: {transform.position.x:F2}), Carril planeado: {targetLane} (X: {checkTargetX:F2}). Buscando conflictos...");

                // Chequear que el carril destino esté despejado de otros obstáculos cerca de esta posición Y
                bool targetClear = true;
                Obstaculo[] activeObstacles = FindObjectsByType<Obstaculo>(FindObjectsSortMode.None);
                foreach (Obstaculo obs in activeObstacles)
                {
                    if (obs == null || obs == this) continue;

                    // 1. Chequeo de obstáculos físicos y autos en la misma posición X e Y (umbral ampliado a 6.0f)
                    if (Mathf.Abs(obs.transform.position.x - checkTargetX) < 0.5f)
                    {
                        if (Mathf.Abs(obs.transform.position.y - transform.position.y) < 6.0f)
                        {
                            targetClear = false;
                            Debug.Log($"[BlinkerDiagnostico] {gameObject.name} - Conflicto: Obstáculo '{obs.name}' encontrado en la ruta destino en X: {obs.transform.position.x:F2}, Y: {obs.transform.position.y:F2}.");
                            break;
                        }
                    }

                    // 2. Chequeo de otros autos que también estén planeando o cambiando hacia ese mismo carril (umbral ampliado a 6.0f)
                    if (obs.IsChangingOrPlanning && Mathf.Abs(obs.TargetX - checkTargetX) < 0.5f)
                    {
                        if (Mathf.Abs(obs.transform.position.y - transform.position.y) < 6.0f)
                        {
                            targetClear = false;
                            Debug.Log($"[BlinkerDiagnostico] {gameObject.name} - Conflicto: Auto '{obs.name}' ya está planificando/cambiando a la misma coordenada X: {obs.TargetX:F2} en Y: {obs.transform.position.y:F2}.");
                            break;
                        }
                    }
                }

                if (targetClear)
                {
                    Debug.Log($"[BlinkerDiagnostico] {gameObject.name} - Ruta despejada. Reservando carril destino X: {checkTargetX:F2} e iniciando señal de advertencia.");
                    IsChangingOrPlanning = true;
                    targetX = checkTargetX; // Reservar la coordenada de destino inmediatamente

                    // Registrar el cambio de carril en el administrador
                    if (AdministradorJuego.Instance != null)
                    {
                        AdministradorJuego.Instance.RegistrarCambioCarril();
                    }

                    StartCoroutine(LaneChangeRoutine(lanePositionsX[currentLane], checkTargetX));
                }
                else
                {
                    Debug.Log($"[BlinkerDiagnostico] {gameObject.name} - Cambio abortado debido a un obstáculo o auto en conflicto en la ruta de destino. El auto seguirá derecho.");
                }
            }
            else
            {
                Debug.Log($"[BlinkerDiagnostico] {gameObject.name} - Decisión negativa: la probabilidad no fue superada.");
            }
        }

        private IEnumerator LaneChangeRoutine(float currentX, float newX)
        {
            // Duración aleatoria de aviso entre 0.5 y 0.8 segundos
            float warningDuration = Random.Range(0.5f, 0.8f);
            Debug.Log($"[BlinkerDiagnostico] {gameObject.name} - Corrutina de advertencia iniciada. Duración de señal: {warningDuration:F2}s. Desde X: {currentX:F2} hacia X: {newX:F2}");

            // Reproducir efecto de sonido de la luz de giro del auto
            if (AdministradorAudio.Instance != null)
            {
                AdministradorAudio.Instance.PlayCarTurnSignalSound();
            }

            // Crear el objeto indicador de luz de giro (blinker)
            turnSignalObject = new GameObject("LuzDeGiro");
            turnSignalObject.transform.SetParent(transform);

            SpriteRenderer parentSR = GetComponent<SpriteRenderer>();

            // Calcular el offset basado en las dimensiones reales de la imagen del auto (rear-left or rear-right corner)
            float sideOffset = 0f;
            float rearOffset = 0f;
            if (parentSR != null && parentSR.sprite != null)
            {
                // Obtenemos la mitad del ancho y alto del sprite en unidades locales
                float halfWidth = parentSR.sprite.rect.width / (2f * parentSR.sprite.pixelsPerUnit);
                float halfHeight = parentSR.sprite.rect.height / (2f * parentSR.sprite.pixelsPerUnit);
                
                // Ubicamos la señal a ~85% del ancho del auto (izquierda o derecha)
                sideOffset = (newX > currentX) ? halfWidth * 0.85f : -halfWidth * 0.85f;
                // Ubicamos la señal en la parte trasera del auto (~85% de la altura hacia atrás)
                rearOffset = -halfHeight * 0.85f;
            }
            else
            {
                // Fallback si no hay SpriteRenderer
                sideOffset = (newX > currentX) ? 0.6f : -0.6f;
                rearOffset = -0.6f;
            }

            turnSignalObject.transform.localPosition = new Vector3(sideOffset, rearOffset, -0.1f);

            // Configurar la escala absoluta para que no se afecte por el encogimiento del parent
            Vector3 parentScale = transform.localScale;
            float targetWorldScale = 0.25f; // Diámetro deseado en unidades del mundo
            turnSignalObject.transform.localScale = new Vector3(
                parentScale.x != 0 ? targetWorldScale / Mathf.Abs(parentScale.x) : targetWorldScale,
                parentScale.y != 0 ? targetWorldScale / Mathf.Abs(parentScale.y) : targetWorldScale,
                1f
            );

            SpriteRenderer srSignal = turnSignalObject.AddComponent<SpriteRenderer>();
            srSignal.sprite = CreateCircleSprite();

            // Sincronizar el sorting layer y orden para que siempre renderice encima del auto
            if (parentSR != null)
            {
                srSignal.sortingLayerID = parentSR.sortingLayerID;
                srSignal.sortingLayerName = parentSR.sortingLayerName;
                srSignal.sortingOrder = parentSR.sortingOrder + 1;
            }
            else
            {
                srSignal.sortingOrder = 11;
            }

            Debug.Log($"[BlinkerDiagnostico] {gameObject.name} - Blinker creado: '{turnSignalObject.name}'. Posición local: {turnSignalObject.transform.localPosition}. Escala local: {turnSignalObject.transform.localScale}. Layer: {srSignal.sortingLayerName} ({srSignal.sortingLayerID}), Orden: {srSignal.sortingOrder}");

            float elapsed = 0f;
            float blinkInterval = 0.15f;
            float lastBlinkTime = 0f;
            bool signalOn = true;

            while (elapsed < warningDuration)
            {
                if (AdministradorUI.Instance != null && AdministradorUI.Instance.IsPlayingVideo)
                {
                    yield return null;
                    continue;
                }
                if (AdministradorJuego.Instance != null && AdministradorJuego.Instance.IsFinishLineReached)
                {
                    yield return null;
                    continue;
                }

                if (elapsed - lastBlinkTime >= blinkInterval)
                {
                    signalOn = !signalOn;
                    srSignal.enabled = signalOn;
                    lastBlinkTime = elapsed;
                    Debug.Log($"[BlinkerDiagnostico] {gameObject.name} - Blinker parpadeo: {(signalOn ? "ENCENDIDO" : "APAGADO")}");
                }

                yield return null;
                elapsed += Time.deltaTime;
            }

            Destroy(turnSignalObject);
            Debug.Log($"[BlinkerDiagnostico] {gameObject.name} - Finalizó tiempo de advertencia. Destruyendo LuzDeGiro e iniciando movimiento lateral en física.");

            // Iniciar movimiento lateral
            startX = currentX;
            targetX = newX;
            laneChangeTimer = 0f;
            isChangingLane = true;
        }

        private Sprite CreateCircleSprite()
        {
            int size = 16;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float center = size / 2f;
            float radius = size / 2f - 0.5f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Mathf.Sqrt((x - center) * (x - center) + (y - center) * (y - center));
                    if (dist <= radius)
                    {
                        float alpha = Mathf.Clamp01(radius - dist + 0.5f);
                        texture.SetPixel(x, y, new Color(1.0f, 0.65f, 0f, alpha)); // Naranja/Amarillo
                    }
                    else
                    {
                        // Pintamos píxeles transparentes con el mismo color base para evitar el sangrado oscuro de la interpolación bilineal
                        texture.SetPixel(x, y, new Color(1.0f, 0.65f, 0f, 0f));
                    }
                }
            }
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16f);
        }

        private void Update()
        {
            if (AdministradorUI.Instance != null && AdministradorUI.Instance.IsPlayingVideo) return;
            // Los autos avanzan y salen de la pantalla aunque termine la partida, salvo al cruzar la meta
            if (AdministradorJuego.Instance != null && AdministradorJuego.Instance.IsFinishLineReached)
            {
                return;
            }

            float speedMultiplier = 1f;
            ControladorJugador player = GameObject.FindFirstObjectByType<ControladorJugador>();
            if (player != null && player.IsBraking)
            {
                speedMultiplier = 0.3f; // Reduce la velocidad de acercamiento al 30%
            }

            // La velocidad final hacia abajo combina el scroll y el movimiento propio del obstáculo
            float finalDownwardSpeed = (globalStreetScrollSpeed + (movementDirection.y * ownSpeed)) * speedMultiplier;
            
            Vector3 pos = transform.position;
            pos.y -= finalDownwardSpeed * Time.deltaTime;

            if (isChangingLane)
            {
                // El movimiento lateral acompaña la velocidad/frenado del juego
                laneChangeTimer += Time.deltaTime * speedMultiplier;
                float t = Mathf.Clamp01(laneChangeTimer / laneChangeDuration);
                pos.x = Mathf.Lerp(startX, targetX, t);
                if (t >= 1f)
                {
                    isChangingLane = false;
                    IsChangingOrPlanning = false; // El cambio ha finalizado por completo
                    Debug.Log($"[BlinkerDiagnostico] {gameObject.name} - Completó cambio de carril en X: {pos.x:F2}");
                }
            }
            else if (type == TipoObstaculo.Pedestrian)
            {
                pos.x += movementDirection.x * ownSpeed * speedMultiplier * Time.deltaTime;
            }

            transform.position = pos;

            if (transform.position.y <= destroyYBound)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Configura la velocidad de scroll global del asfalto.
        /// </summary>
        public static void SetGlobalScrollSpeed(float speed)
        {
            globalStreetScrollSpeed = speed;
        }

        /// <summary>
        /// Configura la velocidad de scroll del asfalto.
        /// </summary>
        public void SetScrollSpeed(float speed)
        {
            SetGlobalScrollSpeed(speed);
        }

        /// <summary>
        /// Obtiene la velocidad propia base asociada a cada tipo de obstáculo.
        /// </summary>
        public static float GetOwnSpeedForType(TipoObstaculo obstacleType)
        {
            switch (obstacleType)
            {
                case TipoObstaculo.BlackCar:
                    return 3.5f;
                case TipoObstaculo.GreenCar:
                    return 5.0f;
                case TipoObstaculo.Pedestrian:
                    return 0.5f;
                default:
                    return 0f;
            }
        }

        /// <summary>
        /// Obtiene la velocidad de avance/descenso en el eje Y sin considerar el freno del jugador.
        /// </summary>
        public float GetSpeedWithoutMultiplier()
        {
            float oSpeed = GetOwnSpeedForType(type);
            float dirY = (type == TipoObstaculo.BlackCar || type == TipoObstaculo.GreenCar) ? 1f : (type == TipoObstaculo.Pedestrian ? -0.707f : 0f);
            return globalStreetScrollSpeed + (dirY * oSpeed);
        }

        /// <summary>
        /// Configura la dirección de movimiento del obstáculo.
        /// </summary>
        public void SetMovementDirection(Vector2 direction)
        {
            movementDirection = direction;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                ControladorJugador player = other.GetComponent<ControladorJugador>();
                if (player != null && player.IsJumping)
                {
                    // Si el jugador está saltando, se ignora la colisión del obstáculo
                    return;
                }

                // El bache genera inestabilidad temporal en lugar de restar vidas directamente
                if (type == TipoObstaculo.Pothole)
                {
                    if (player != null)
                    {
                        player.TriggerDeliveryAnimation();
                    }
                }
                
                // Se destruye el obstáculo tras chocar, salvo que sea un bache
                if (type != TipoObstaculo.Pothole)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}
