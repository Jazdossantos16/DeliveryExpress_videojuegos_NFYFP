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

        [Header("UI de Monedas")]
        [SerializeField] private Text coinsText;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            Time.timeScale = 1f;

            if (AdministradorJuego.Instance != null)
            {
                // Si la escena se recargó por derrota, se reinicia el día acá para mantener el scroll congelado durante la carga.
                if (AdministradorJuego.Instance.IsGameOver)
                {
                    AdministradorJuego.Instance.RestartCurrentDay();
                }

                AdministradorJuego.Instance.OnLivesChanged += UpdateLivesUI;
                AdministradorJuego.Instance.OnCoinsChanged += UpdateCoinsUI;
                
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

                if (coinsText == null)
                {
                    Transform t = transform.Find("Texto_Monedas");
                    if (t != null)
                    {
                        coinsText = t.GetComponent<Text>();
                        t.gameObject.layer = this.gameObject.layer;
                    }
                    else
                    {
                        // Crear automáticamente el objeto de texto para monedas en la parte superior derecha
                        GameObject coinsObj = new GameObject("Texto_Monedas", typeof(RectTransform));
                        coinsObj.layer = this.gameObject.layer;
                        coinsObj.transform.SetParent(this.transform, false);
                        
                        coinsText = coinsObj.AddComponent<Text>();
                        
                        // Copiar fuente de livesText si está disponible, o buscar cualquier texto (incluyendo inactivos)
                        Text anyText = GetComponentInChildren<Text>(true);
                        if (anyText != null)
                        {
                            coinsText.font = anyText.font;
                        }
                        else
                        {
                            coinsText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                            if (coinsText.font == null) coinsText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                        }
                        
                        coinsText.fontSize = 24;
                        coinsText.color = new Color(1f, 0.84f, 0f); // Dorado
                        coinsText.alignment = TextAnchor.MiddleRight;
                        
                        RectTransform rect = coinsObj.GetComponent<RectTransform>();
                        rect.anchorMin = new Vector2(1f, 1f);
                        rect.anchorMax = new Vector2(1f, 1f);
                        rect.pivot = new Vector2(1f, 1f);
                        rect.anchoredPosition = new Vector2(-35f, -35f); // 35px de margen
                        rect.sizeDelta = new Vector2(200f, 50f);
                        
                        Shadow shadow = coinsObj.AddComponent<Shadow>();
                        shadow.effectColor = Color.black;
                        shadow.effectDistance = new Vector2(1f, -1f);
                    }
                }

                if (coinsText != null)
                {
                    coinsText.gameObject.layer = this.gameObject.layer;
                }

                UpdateLivesUI(3);
                UpdateCoinsUI(AdministradorJuego.Instance.Coins);
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
                AdministradorJuego.Instance.OnCoinsChanged -= UpdateCoinsUI;
            }
        }

        private void Update()
        {
            // Detecta el reinicio (tecla R o click) si terminó la partida
            if (AdministradorJuego.Instance != null && AdministradorJuego.Instance.IsGameOver)
            {
                if (Input.GetKeyDown(KeyCode.R) || Input.GetMouseButtonDown(0))
                {
                    RestartGame();
                }
            }
        }

        public void RestartGame()
        {
            // Carga la escena sin reiniciar el AdministradorJuego para que el fondo siga congelado en la carga
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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

        public void UpdateCoinsUI(int coins)
        {
            if (coinsText != null)
            {
                coinsText.text = "Monedas: " + coins;
            }
        }
    }
}
