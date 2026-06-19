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

        [Header("Configuración de Video Intro")]
        [SerializeField] private UnityEngine.Video.VideoClip introVideoClip;

        private UnityEngine.Video.VideoPlayer videoPlayer;
        private RenderTexture videoTexture;
        private RawImage videoRawImage;
        private Text skipText;
        private bool isPlayingVideo = false;

        [Header("UI de Monedas")]
        [SerializeField] private Text coinsText;

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
            if (isPlayingVideo)
            {
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
                {
                    FinalizarIntroVideo();
                    return;
                }
            }

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
            if (introVideoClip != null)
            {
                PlayIntroVideo();
            }
            else
            {
                ComenzarPartidaReal();
            }
        }

        private void PlayIntroVideo()
        {
            isPlayingVideo = true;
            Time.timeScale = 0f; // Asegurar que el juego esté pausado

            // Ocultamos la pantalla de inicio
            if (startPanel != null)
            {
                startPanel.SetActive(false);
            }

            // Creamos un objeto UI para mostrar el video
            GameObject videoGo = new GameObject("IntroVideo_RawImage");
            videoGo.transform.SetParent(transform, false); // transform es el Canvas
            videoGo.transform.SetAsLastSibling(); // Poner al frente

            videoRawImage = videoGo.AddComponent<RawImage>();
            videoRawImage.color = Color.white;
            
            // Configurar RectTransform a pantalla completa
            RectTransform rect = videoRawImage.rectTransform;
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            // Crear RenderTexture dinámico
            videoTexture = new RenderTexture(1920, 1080, 16, RenderTextureFormat.ARGB32);
            videoTexture.Create();
            videoRawImage.texture = videoTexture;

            // Agregar VideoPlayer
            videoPlayer = videoGo.AddComponent<UnityEngine.Video.VideoPlayer>();
            videoPlayer.playOnAwake = false;
            videoPlayer.source = UnityEngine.Video.VideoSource.Url;
            videoPlayer.url = System.IO.Path.Combine(Application.streamingAssetsPath, "videojuego_prueba_202606182214.mp4");
            videoPlayer.renderMode = UnityEngine.Video.VideoRenderMode.RenderTexture;
            videoPlayer.targetTexture = videoTexture;
            
            // Configurar audio
            videoPlayer.audioOutputMode = UnityEngine.Video.VideoAudioOutputMode.Direct;
            
            // Suscribirse al evento de finalización
            videoPlayer.loopPointReached += AlTerminarVideo;

            // Agregar texto de Skip
            GameObject skipTextGo = new GameObject("IntroVideo_SkipText");
            skipTextGo.transform.SetParent(videoGo.transform, false);
            skipText = skipTextGo.AddComponent<Text>();
            
            // Cargar una fuente estándar
            Font standardFont = null;
            Text existingText = GetComponentInChildren<Text>(true);
            if (existingText != null)
            {
                standardFont = existingText.font;
            }
            if (standardFont == null)
            {
                standardFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }
            if (standardFont == null)
            {
                standardFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }
            skipText.font = standardFont;
            
            skipText.text = "Presiona ESPACIO para omitir";
            skipText.fontSize = 24;
            skipText.alignment = TextAnchor.LowerRight;
            skipText.color = new Color(1f, 1f, 1f, 0.7f);
            
            // Configurar RectTransform para el texto en la esquina inferior derecha
            RectTransform skipRect = skipText.rectTransform;
            skipRect.anchorMin = new Vector2(0.5f, 0f);
            skipRect.anchorMax = new Vector2(1f, 0.2f);
            skipRect.pivot = new Vector2(1f, 0f);
            skipRect.anchoredPosition = new Vector2(-40f, 40f);
            skipRect.sizeDelta = new Vector2(400f, 50f);

            // Iniciar reproducción
            videoPlayer.Play();
            Debug.Log("🎬 Reproduciendo video de intro: " + introVideoClip.name);
        }

        private void AlTerminarVideo(UnityEngine.Video.VideoPlayer vp)
        {
            FinalizarIntroVideo();
        }

        private void FinalizarIntroVideo()
        {
            if (!isPlayingVideo) return;
            isPlayingVideo = false;

            if (videoPlayer != null)
            {
                videoPlayer.loopPointReached -= AlTerminarVideo;
            }

            // Destruir elementos del video
            if (videoRawImage != null)
            {
                Destroy(videoRawImage.gameObject);
            }

            if (videoTexture != null)
            {
                videoTexture.Release();
                Destroy(videoTexture);
            }

            Debug.Log("🎬 Video de intro finalizado o salteado.");
            ComenzarPartidaReal();
        }

        private void ComenzarPartidaReal()
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

        public void UpdateCoinsUI(int coins)
        {
            if (coinsText != null)
            {
                coinsText.text = "Monedas: " + coins;
            }
        }
    }
}
