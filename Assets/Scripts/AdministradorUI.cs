using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace DeliveryExpress
{
    /// <summary>
    /// Administra la interfaz de usuario (HUD de vidas, barra de equilibrio y pantalla de Game Over).
    /// </summary>
    public class AdministradorUI : MonoBehaviour
    {
        public static AdministradorUI Instance { get; private set; }

                [Header("UI de Vidas")]
        [SerializeField] private Image[] heartImages;
        [SerializeField] private Text livesText;

        [Header("UI de Equilibrio")]
        [SerializeField] private Slider balanceSlider;
        [SerializeField] private Image balanceFillImage;

        [Header("Pantalla de Fin de Juego")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private Sprite loseSprite;

        [Header("Pantalla de Inicio")]
        [SerializeField] private GameObject startPanel;
        private static bool skipStartPanel = false;

        [Header("Pantalla de Victoria")]
        [SerializeField] private GameObject victoryPanel;
        [SerializeField] private Sprite victorySprite;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            if (skipStartPanel)
            {
                skipStartPanel = false;
                if (startPanel != null)
                {
                    startPanel.SetActive(false);
                }
                Time.timeScale = 1f;
            }
            else
            {
                if (startPanel != null && startPanel.activeSelf)
                {
                    Time.timeScale = 0f; // Pausa el juego mientras esté la pantalla de inicio activa
                }
                else
                {
                    Time.timeScale = 1f;
                }
            }

            if (AdministradorJuego.Instance != null)
            {
                // Si la escena se recargó por derrota, se reinicia el día acá para mantener el scroll congelado durante la carga.
                if (AdministradorJuego.Instance.IsGameOver)
                {
                    AdministradorJuego.Instance.RestartCurrentDay();
                }

                AdministradorJuego.Instance.OnLivesChanged += UpdateLivesUI;
                
                // Busca componentes si no están asignados en el Inspector
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

                UpdateLivesUI(3);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
            if (AdministradorJuego.Instance != null)
            {
                AdministradorJuego.Instance.OnLivesChanged -= UpdateLivesUI;
            }
        }

        private void Update()
        {
            // Detecta el reinicio mediante la tecla R
            if (AdministradorJuego.Instance != null && AdministradorJuego.Instance.IsGameOver)
            {
                if (Input.GetKeyDown(KeyCode.R))
                {
                    RestartGame();
                }
            }
        }

        public void RestartGame()
        {
            skipStartPanel = true;
            Time.timeScale = 1f; // Asegura restablecer la escala de tiempo
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void CargarMenu()
        {
            skipStartPanel = false;
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void IniciarJuego()
        {
            Time.timeScale = 1f; // Reanuda el juego
            if (startPanel != null)
            {
                startPanel.SetActive(false); // Oculta la pantalla de inicio
            }
            Debug.Log("✅ Juego Iniciado.");
        }

        public void ShowGameOver()
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);

                // Asigna la imagen de fondo por código
                Image img = gameOverPanel.GetComponent<Image>();
                if (img != null && loseSprite != null)
                {
                    img.sprite = loseSprite;
                    img.color = Color.white;
                }

                // Oculta el texto si la imagen de fondo ya cargó
                Transform txtTransform = gameOverPanel.transform.Find("GameOverText");
                if (txtTransform != null && loseSprite != null)
                {
                    Text txt = txtTransform.GetComponent<Text>();
                    if (txt != null)
                    {
                        txt.text = "";
                    }
                }
            }
            Time.timeScale = 0f;
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
                // Ordena alfabéticamente para apagarlos en orden
                hearts.Sort((a, b) => string.Compare(a.gameObject.name, b.gameObject.name, System.StringComparison.Ordinal));
                heartImages = hearts.ToArray();
            }
        }

        /// <summary>
        /// Actualiza la barra de equilibrio y cambia su color según el porcentaje.
        /// </summary>
        public void UpdateBalanceUI(float current, float max)
        {
            if (balanceSlider != null && balanceFillImage != null)
            {
                float fillPercentage = Mathf.Clamp01(current / max);
                balanceSlider.value = fillPercentage;
                
                balanceFillImage.color = Color.Lerp(Color.red, Color.green, fillPercentage);
            }
        }

        /// <summary>
        /// Actualiza la UI de vidas y muestra la pantalla de derrota si llega a 0.
        /// </summary>
        public void UpdateLivesUI(int currentLives)
        {
            if (livesText != null)
            {
                livesText.text = "Vidas: " + Mathf.Max(0, currentLives);
            }

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

            // Si no quedan vidas, muestra el Game Over
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

        public void ShowVictory()
        {
            if (victoryPanel != null)
            {
                victoryPanel.SetActive(true);

                Image img = victoryPanel.GetComponent<Image>();
                if (img != null && victorySprite != null)
                {
                    img.sprite = victorySprite;
                    img.color = Color.white;
                }
            }
            Time.timeScale = 0f;
        }

        public void AvanzarSiguienteDia()
        {
            skipStartPanel = true;
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
