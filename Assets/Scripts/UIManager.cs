using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace DeliveryExpress
{
    /// <summary>
    /// Gestiona la interfaz de usuario en tiempo real (HUD de vidas, pantalla de Game Over).
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("UI de Vidas")]
        [SerializeField] private Image[] heartImages;
        [SerializeField] private Text livesText;

        [Header("Pantalla de Fin de Juego")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private Sprite loseSprite;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            // Asegurar que el tiempo corra normalmente al iniciar la escena
            Time.timeScale = 1f;

            if (GameManager.Instance != null)
            {
                // Si la escena fue recargada por derrota, reiniciamos el día aquí.
                // Esto garantiza que GameManager.Instance.IsGameOver continúe siendo true
                // durante la carga de la escena, manteniendo el piso congelado.
                if (GameManager.Instance.IsGameOver)
                {
                    GameManager.Instance.RestartCurrentDay();
                }

                GameManager.Instance.OnLivesChanged += UpdateLivesUI;
                
                // Buscar componentes dinámicamente si no se asignaron en el inspector
                if (heartImages == null || heartImages.Length == 0)
                {
                    FindHeartImages();
                }

                if (livesText == null)
                {
                    Transform t = transform.Find("Texto_Vidas");
                    if (t != null) livesText = t.GetComponent<Text>();
                }

                if (gameOverPanel == null)
                {
                    Transform t = transform.Find("GameOverPanel");
                    if (t != null) gameOverPanel = t.gameObject;
                }

                // Sincronizar estado inicial
                UpdateLivesUI(3);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnLivesChanged -= UpdateLivesUI;
            }
        }

        private void Update()
        {
            // Detectar reinicio si la partida ha terminado (tecla R o click del mouse)
            if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            {
                if (Input.GetKeyDown(KeyCode.R) || Input.GetMouseButtonDown(0))
                {
                    RestartGame();
                }
            }
        }

        public void RestartGame()
        {
            // Solo cargamos la escena sin reiniciar GameManager todavía.
            // De esta forma, GameManager.Instance.IsGameOver sigue siendo true y
            // Time.timeScale sigue siendo 0f durante todo el proceso de carga de la escena,
            // garantizando que el piso se quede perfectamente congelado en todo momento.
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void ShowGameOver()
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);

                // Configurar el fondo a nivel de código para asegurar que no falle la renderización en tiempo de ejecución
                Image img = gameOverPanel.GetComponent<Image>();
                if (img != null && loseSprite != null)
                {
                    img.sprite = loseSprite;
                    img.color = Color.white;
                }

                // Apagar el texto de fallback si tenemos la imagen cargada
                Transform txtTransform = gameOverPanel.transform.Find("GameOverText");
                if (txtTransform != null && loseSprite != null)
                {
                    Text txt = txtTransform.GetComponent<Text>();
                    if (txt != null)
                    {
                        txt.text = ""; // Limpiar texto para que no tape la imagen
                    }
                }
            }
            Time.timeScale = 0f; // Congelar físicas y movimiento
        }

        public void FindHeartImages()
        {
            Image[] allImages = GetComponentsInChildren<Image>(true);
            System.Collections.Generic.List<Image> hearts = new System.Collections.Generic.List<Image>();
            
            foreach (Image img in allImages)
            {
                if (img != null && (img.gameObject.name.ToLower().Contains("corazon") || 
                                    img.gameObject.name.ToLower().Contains("hamburguesa") || 
                                    img.gameObject.name.ToLower().Contains("vida")))
                {
                    hearts.Add(img);
                }
            }
            
            if (hearts.Count > 0)
            {
                // Ordenar alfabéticamente para asegurar que desaparezcan en orden
                hearts.Sort((a, b) => string.Compare(a.gameObject.name, b.gameObject.name, System.StringComparison.Ordinal));
                heartImages = hearts.ToArray();
            }
        }

        /// <summary>
        /// Actualiza la UI de vidas y muestra la pantalla de derrota si llega a 0.
        /// </summary>
        public void UpdateLivesUI(int currentLives)
        {
            // 1. Actualizar contador en formato texto
            if (livesText != null)
            {
                livesText.text = "Vidas: " + Mathf.Max(0, currentLives);
            }

            // 2. Actualizar corazones visuales
            if (heartImages != null && heartImages.Length > 0)
            {
                for (int i = 0; i < heartImages.Length; i++)
                {
                    if (heartImages[i] != null)
                    {
                        heartImages[i].enabled = i < currentLives;
                    }
                }
            }

            // 3. Si se agotan las vidas, congelar partida y activar panel
            if (currentLives <= 0)
            {
                ShowGameOver();
            }
            else
            {
                if (gameOverPanel != null)
                {
                    gameOverPanel.SetActive(false);
                }
            }
        }
    }
}
