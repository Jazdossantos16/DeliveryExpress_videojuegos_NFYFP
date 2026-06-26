using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

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
        [SerializeField] private Image balanceImage;
        [SerializeField] private Sprite[] balanceSprites;

        [Header("UI de Potenciador")]
        [SerializeField] private Image boosterImage;
        [SerializeField] private Sprite[] boosterSprites;

        [Header("Pantalla de Fin de Juego")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private Sprite loseSprite;

        [Header("Pantalla de Inicio")]
        [SerializeField] private GameObject startPanel;
        private static bool skipStartPanel = false;

        [Header("Pantalla de Victoria")]
        [SerializeField] private GameObject victoryPanel;
        [SerializeField] private Sprite victorySprite;

        private UnityEngine.Video.VideoPlayer videoPlayer;
        private RenderTexture videoTexture;
        private RawImage videoRawImage;
        [SerializeField] private Text skipText;
        private bool isPlayingVideo = false;
        private RawImage fadeOverlay;
        private bool isTransitioning = false;

        [Header("UI de Monedas")]
        [SerializeField] private Text coinsText;

        [Header("Sprites de Pausa y Play")]
        [SerializeField] private Sprite pauseSprite;
        [SerializeField] private Sprite playSprite;
        private Image pausePlayButtonImage;

        [Header("UI de Configuración")]
        [SerializeField] private GameObject configPanel;
        [SerializeField] private Image configBackgroundImage;
        [SerializeField] private Sprite imgConfigBoth;
        [SerializeField] private Sprite imgConfigNoMusic;
        [SerializeField] private Sprite imgConfigNoSound;
        [SerializeField] private Sprite imgConfigNone;
        [SerializeField] private InputField usernameInputField;

        [Header("Leaderboard UI")]
        [SerializeField] private Font customFont;
        [SerializeField] private InputField gameOverNameInputField;
        [SerializeField] private Button gameOverSaveButton;
        [SerializeField] private Text gameOverLeaderboardText;
        [SerializeField] private InputField victoryNameInputField;
        [SerializeField] private Button victorySaveButton;
        [SerializeField] private Text victoryLeaderboardText;

        [Header("Iconos de Configuración")]
        [SerializeField] private Image musicIconImage;
        [SerializeField] private Image soundIconImage;
        [SerializeField] private Sprite iconMusicOn;
        [SerializeField] private Sprite iconMusicOff;
        [SerializeField] private Sprite iconSoundOn;
        [SerializeField] private Sprite iconSoundOff;
        [SerializeField] private Text musicStateText;
        [SerializeField] private Text soundStateText;

        private bool musicEnabled = true;
        private bool soundEnabled = true;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            // Cargar y aplicar configuraciones de audio al iniciar
            soundEnabled = PlayerPrefs.GetInt("SoundEnabled", 1) == 1;
            musicEnabled = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;
            AudioListener.volume = soundEnabled ? 1f : 0f;
            if (AdministradorAudio.Instance != null)
            {
                AdministradorAudio.Instance.SetMusicEnabled(musicEnabled);
            }

            if (skipStartPanel)
            {
                skipStartPanel = false;
                if (startPanel != null)
                {
                    startPanel.SetActive(false);
                }
                Time.timeScale = 1f;
                SetHUDActive(true);
            }
            else
            {
                if (startPanel != null && startPanel.activeSelf)
                {
                    Time.timeScale = 0f; // Pausa el juego mientras esté la pantalla de inicio activa
                    SetHUDActive(false);
                }
                else
                {
                    Time.timeScale = 1f;
                    SetHUDActive(true);
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

                Transform pauseBtnTrans = transform.Find("Boton_PausaPlay");
                if (pauseBtnTrans != null)
                {
                    pausePlayButtonImage = pauseBtnTrans.GetComponent<Image>();
                    if (pausePlayButtonImage != null && pauseSprite != null)
                    {
                        pausePlayButtonImage.sprite = pauseSprite;
                    }
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

                UpdateLivesUI(AdministradorJuego.Instance.CurrentLives);
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
            if (isPlayingVideo && !isTransitioning)
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    isTransitioning = true;
                    StartCoroutine(FadeScreen(0f, 1f, 0.5f, () => {
                        isTransitioning = false;
                        FinalizarIntroVideo();
                    }));
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

            // Tecla P o Esc para pausar/reanudar
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
            {
                if (startPanel == null || !startPanel.activeSelf)
                {
                    AlternarPausa();
                }
            }

            // Actualizar barra de potenciador de velocidad (energía)
            ControladorJugador player = ControladorJugador.Instance;
            if (player != null && player.IsSpeedBoostActive)
            {
                if (boosterImage != null && boosterSprites != null && boosterSprites.Length >= 7)
                {
                    if (!boosterImage.gameObject.activeSelf)
                    {
                        boosterImage.gameObject.SetActive(true);
                    }

                    float fillPercentage = Mathf.Clamp01(player.SpeedBoostDurationRemaining / player.SpeedBoostDurationMax);
                    int spriteIndex = 1; // vacío por defecto
                    if (fillPercentage > 0.85f) spriteIndex = 0;
                    else if (fillPercentage > 0.68f) spriteIndex = 2;
                    else if (fillPercentage > 0.51f) spriteIndex = 3;
                    else if (fillPercentage > 0.34f) spriteIndex = 4;
                    else if (fillPercentage > 0.17f) spriteIndex = 5;
                    else if (fillPercentage > 0.0f) spriteIndex = 6;
                    else spriteIndex = 1;

                    if (spriteIndex < boosterSprites.Length && boosterSprites[spriteIndex] != null)
                    {
                        boosterImage.sprite = boosterSprites[spriteIndex];
                    }
                }
            }
            else
            {
                if (boosterImage != null && boosterImage.gameObject.activeSelf)
                {
                    boosterImage.gameObject.SetActive(false);
                }
            }
        }

        public void RestartGame()
        {
            PlayClickSound();
            if (AdministradorJuego.Instance != null)
            {
                AdministradorJuego.Instance.ResetCoins();
            }
            skipStartPanel = true;
            Time.timeScale = 1f; // Asegura restablecer la escala de tiempo
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void CargarMenu()
        {
            PlayClickSound();
            if (AdministradorJuego.Instance != null)
            {
                AdministradorJuego.Instance.ResetCoins();
            }
            skipStartPanel = false;
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void IniciarJuego()
        {
            PlayClickSound();
#if UNITY_WEBGL && !UNITY_EDITOR
            StartCoroutine(FadeScreen(0f, 1f, 0.5f, () => PlayIntroVideo()));
#else
            string videoPath = System.IO.Path.Combine(Application.streamingAssetsPath, "videojuego_prueba_202606182214.mp4");
            if (System.IO.File.Exists(videoPath))
            {
                StartCoroutine(FadeScreen(0f, 1f, 0.5f, () => PlayIntroVideo()));
            }
            else
            {
                Debug.LogWarning("⚠️ No se encontró el video de intro en StreamingAssets: " + videoPath);
                ComenzarPartidaReal();
            }
#endif
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
            bool musicOn = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;
            videoPlayer.SetDirectAudioMute(0, !musicOn);
            
            // Suscribirse al evento de finalización
            videoPlayer.loopPointReached += AlTerminarVideo;

            // Activar y traer al frente el texto de Skip persistente (o su panel contenedor)
            if (skipText != null)
            {
                GameObject skipObj = skipText.transform.parent != null ? skipText.transform.parent.gameObject : skipText.gameObject;
                skipObj.SetActive(true);
                skipObj.transform.SetAsLastSibling();
            }

            // Iniciar reproducción
            videoPlayer.Play();
            Debug.Log("🎬 Reproduciendo video de intro.");

            // Hacemos fade-out del overlay negro para revelar el video
            StartCoroutine(FadeScreen(1f, 0f, 0.8f));
        }

        private void AlTerminarVideo(UnityEngine.Video.VideoPlayer vp)
        {
            if (isTransitioning) return;
            isTransitioning = true;
            StartCoroutine(FadeScreen(0f, 1f, 0.5f, () => {
                isTransitioning = false;
                FinalizarIntroVideo();
            }));
        }

        private void FinalizarIntroVideo()
        {
            if (!isPlayingVideo) return;
            isPlayingVideo = false;

            if (videoPlayer != null)
            {
                videoPlayer.loopPointReached -= AlTerminarVideo;
            }

            // Desactivar el texto de Skip persistente (o su panel contenedor)
            if (skipText != null)
            {
                GameObject skipObj = skipText.transform.parent != null ? skipText.transform.parent.gameObject : skipText.gameObject;
                skipObj.SetActive(false);
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

            // Hacemos fade-out del overlay negro para revelar el juego real
            StartCoroutine(FadeScreen(1f, 0f, 0.8f));
        }

        private void ComenzarPartidaReal()
        {
            Time.timeScale = 1f; // Reanuda el juego
            if (startPanel != null)
            {
                startPanel.SetActive(false); // Oculta la pantalla de inicio
            }
            if (pausePlayButtonImage != null && pauseSprite != null)
            {
                pausePlayButtonImage.sprite = pauseSprite;
            }
            SetHUDActive(true);
            Debug.Log("✅ Juego Iniciado.");
        }

        private IEnumerator FadeScreen(float startAlpha, float endAlpha, float duration, System.Action onComplete = null)
        {
            if (fadeOverlay == null)
            {
                GameObject fadeGo = new GameObject("IntroVideo_FadeOverlay");
                fadeGo.transform.SetParent(transform, false);
                fadeGo.transform.SetAsLastSibling();
                
                fadeOverlay = fadeGo.AddComponent<RawImage>();
                fadeOverlay.color = Color.clear;
                
                RectTransform rect = fadeOverlay.rectTransform;
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
            }
            
            fadeOverlay.gameObject.SetActive(true);
            fadeOverlay.transform.SetAsLastSibling();
            
            float elapsed = 0f;
            Color c = Color.black;
            
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
                c.a = alpha;
                fadeOverlay.color = c;
                yield return null;
            }
            
            c.a = endAlpha;
            fadeOverlay.color = c;
            
            if (endAlpha <= 0f)
            {
                fadeOverlay.gameObject.SetActive(false);
            }
            
            onComplete?.Invoke();
        }

        public void ShowGameOver()
        {
            SetHUDActive(false);
            if (AdministradorAudio.Instance != null)
            {
                AdministradorAudio.Instance.PlayDefeatSound();
            }
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

                // Inicializar y limpiar el Leaderboard para GameOver
                if (gameOverNameInputField != null)
                {
                    gameOverNameInputField.text = "";
                    gameOverNameInputField.interactable = true;
                }
                if (gameOverSaveButton != null)
                {
                    gameOverSaveButton.interactable = true;
                }
                ActualizarLeaderboardTexto(gameOverLeaderboardText);
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
            float fillPercentage = Mathf.Clamp01(current / max);

            // Actualizar la imagen con el sprite correspondiente según el nivel de equilibrio
            if (balanceImage != null && balanceSprites != null && balanceSprites.Length >= 7)
            {
                // Mapeo:
                // barra_equilibrio_0 (index 0) -> 6 celdas (equilibrio > 85%)
                // barra_equilibrio_2 (index 2) -> 5 celdas (equilibrio entre 68% y 85%)
                // barra_equilibrio_3 (index 3) -> 4 celdas (equilibrio entre 51% y 68%)
                // barra_equilibrio_4 (index 4) -> 3 celdas (equilibrio entre 34% y 51%)
                // barra_equilibrio_5 (index 5) -> 2 celdas (equilibrio entre 17% y 34%)
                // barra_equilibrio_6 (index 6) -> 1 celda (equilibrio entre 0% y 17%)
                // barra_equilibrio_1 (index 1) -> vacío (equilibrio = 0%)
                
                int spriteIndex = 1; // vacío por defecto
                if (fillPercentage > 0.85f) spriteIndex = 0;
                else if (fillPercentage > 0.68f) spriteIndex = 2;
                else if (fillPercentage > 0.51f) spriteIndex = 3;
                else if (fillPercentage > 0.34f) spriteIndex = 4;
                else if (fillPercentage > 0.17f) spriteIndex = 5;
                else if (fillPercentage > 0.0f) spriteIndex = 6;
                else spriteIndex = 1;

                if (spriteIndex < balanceSprites.Length && balanceSprites[spriteIndex] != null)
                {
                    balanceImage.sprite = balanceSprites[spriteIndex];
                }
            }

            // Retrocompatibilidad con el Slider convencional
            if (balanceSlider != null)
            {
                balanceSlider.value = fillPercentage;
                if (balanceFillImage != null)
                {
                    balanceFillImage.color = Color.Lerp(Color.red, Color.green, fillPercentage);
                }
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
            SetHUDActive(false);
            if (AdministradorAudio.Instance != null)
            {
                AdministradorAudio.Instance.PlayVictorySound();
            }
            if (victoryPanel != null)
            {
                victoryPanel.SetActive(true);

                Image img = victoryPanel.GetComponent<Image>();
                if (img != null && victorySprite != null)
                {
                    img.sprite = victorySprite;
                    img.color = Color.white;
                }

                // Inicializar y limpiar el Leaderboard para Victoria
                if (victoryNameInputField != null)
                {
                    victoryNameInputField.text = "";
                    victoryNameInputField.interactable = true;
                }
                if (victorySaveButton != null)
                {
                    victorySaveButton.interactable = true;
                }
                ActualizarLeaderboardTexto(victoryLeaderboardText);
            }
            Time.timeScale = 0f;
        }

        private void SetHUDActive(bool active)
        {
            Transform hudLives = transform.Find("Marco_HUD");
            if (hudLives != null) hudLives.gameObject.SetActive(active);

            Transform hudCoins = transform.Find("Marco_Monedas");
            if (hudCoins != null) hudCoins.gameObject.SetActive(active);

            Transform hudBalance = transform.Find("Barra_Equilibrio");
            if (hudBalance != null) hudBalance.gameObject.SetActive(active);

            Transform hudPause = transform.Find("Boton_PausaPlay");
            if (hudPause != null) hudPause.gameObject.SetActive(active);

            Transform hudWin = transform.Find("Boton_Ganar");
            if (hudWin != null) hudWin.gameObject.SetActive(active);

            Transform hudBooster = transform.Find("Barra_Potenciador");
            if (hudBooster != null)
            {
                hudBooster.gameObject.SetActive(active && (ControladorJugador.Instance != null && ControladorJugador.Instance.IsSpeedBoostActive));
            }

            if (coinsText != null && coinsText.transform.parent == transform) 
                coinsText.gameObject.SetActive(active);
        }

        public void AlternarPausa()
        {
            // No permitir pausar si el juego ya terminó o está en la pantalla de inicio o reproduciendo video
            if (AdministradorJuego.Instance != null && (AdministradorJuego.Instance.IsGameOver || isPlayingVideo))
                return;

            if (Time.timeScale > 0f)
            {
                // Pausar
                Time.timeScale = 0f;
                if (pausePlayButtonImage != null && playSprite != null)
                {
                    pausePlayButtonImage.sprite = playSprite;
                }
                Debug.Log("⏸️ Juego Pausado.");
            }
            else
            {
                // Reanudar
                Time.timeScale = 1f;
                if (pausePlayButtonImage != null && pauseSprite != null)
                {
                    pausePlayButtonImage.sprite = pauseSprite;
                }
                Debug.Log("▶️ Juego Reanudado.");
            }
        }

        public void AvanzarSiguienteDia()
        {
            PlayClickSound();
            skipStartPanel = true;
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void UpdateCoinsUI(int coins)
        {
            if (coinsText != null)
            {
                coinsText.text = coins.ToString();
            }
        }

        public void AbrirConfiguracion()
        {
            PlayClickSound();
            if (configPanel != null)
            {
                configPanel.SetActive(true);
                
                string savedUser = PlayerPrefs.GetString("Username", "User123");
                musicEnabled = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;
                soundEnabled = PlayerPrefs.GetInt("SoundEnabled", 1) == 1;

                if (usernameInputField != null)
                {
                    usernameInputField.text = savedUser;
                }

                ActualizarPanelConfiguracion();
                Debug.Log("⚙️ Panel de configuración abierto.");
            }
        }

        public void CerrarConfiguracion()
        {
            PlayClickSound();
            if (usernameInputField != null)
            {
                PlayerPrefs.SetString("Username", usernameInputField.text);
            }
            PlayerPrefs.Save();

            if (configPanel != null)
            {
                configPanel.SetActive(false);
            }
            Debug.Log("⚙️ Panel de configuración cerrado.");
        }

        public void ToggleMusica()
        {
            PlayClickSound();
            musicEnabled = !musicEnabled;
            PlayerPrefs.SetInt("MusicEnabled", musicEnabled ? 1 : 0);
            PlayerPrefs.Save();
            if (AdministradorAudio.Instance != null)
            {
                AdministradorAudio.Instance.SetMusicEnabled(musicEnabled);
            }
            ActualizarPanelConfiguracion();
            Debug.Log("🎵 Música toggled: " + musicEnabled);
        }

        public void ToggleSonido()
        {
            PlayClickSound();
            soundEnabled = !soundEnabled;
            PlayerPrefs.SetInt("SoundEnabled", soundEnabled ? 1 : 0);
            PlayerPrefs.Save();
            AudioListener.volume = soundEnabled ? 1f : 0f;
            ActualizarPanelConfiguracion();
            Debug.Log("🔊 Sonido toggled: " + soundEnabled);
        }

        public void PlayClickSound()
        {
            if (AdministradorAudio.Instance != null)
            {
                AdministradorAudio.Instance.PlayButtonClickSound();
            }
        }

        public void OnUsernameChanged(string newName)
        {
            PlayerPrefs.SetString("Username", newName);
            PlayerPrefs.Save();
        }

        private void ActualizarPanelConfiguracion()
        {
            if (configBackgroundImage != null && imgConfigBoth != null)
            {
                configBackgroundImage.sprite = imgConfigBoth;
            }

            // Actualizar iconos dinámicos
            if (musicIconImage != null)
            {
                musicIconImage.sprite = musicEnabled ? iconMusicOn : iconMusicOff;
            }
            if (soundIconImage != null)
            {
                soundIconImage.sprite = soundEnabled ? iconSoundOn : iconSoundOff;
            }

            // Los textos de estado se ocultan; solo el ícono indica si está activo o no
            if (musicStateText != null)
            {
                musicStateText.text = "";
                musicStateText.gameObject.SetActive(false);
            }
            if (soundStateText != null)
            {
                soundStateText.text = "";
                soundStateText.gameObject.SetActive(false);
            }
        }

        // LÓGICA Y ESTRUCTURAS DEL LEADERBOARD
        private const string LeaderboardPrefsKey = "DeliveryExpress_Leaderboard";

        [System.Serializable]
        public class HighScoreEntry
        {
            public string name;
            public int score;
        }

        [System.Serializable]
        public class LeaderboardData
        {
            public System.Collections.Generic.List<HighScoreEntry> entries = new System.Collections.Generic.List<HighScoreEntry>();
        }

        private LeaderboardData LoadLeaderboard()
        {
            string json = PlayerPrefs.GetString(LeaderboardPrefsKey, "");
            if (string.IsNullOrEmpty(json))
            {
                return new LeaderboardData();
            }
            try
            {
                return JsonUtility.FromJson<LeaderboardData>(json);
            }
            catch
            {
                return new LeaderboardData();
            }
        }

        private void SaveLeaderboard(LeaderboardData data)
        {
            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(LeaderboardPrefsKey, json);
            PlayerPrefs.Save();
        }

        public void SaveGameOverScore()
        {
            PlayClickSound();
            if (gameOverNameInputField != null && AdministradorJuego.Instance != null)
            {
                string name = gameOverNameInputField.text;
                int coins = AdministradorJuego.Instance.Coins;
                GuardarYActualizarUI(name, coins, gameOverNameInputField, gameOverSaveButton, gameOverLeaderboardText);
            }
        }

        public void SaveVictoryScore()
        {
            PlayClickSound();
            if (victoryNameInputField != null && AdministradorJuego.Instance != null)
            {
                string name = victoryNameInputField.text;
                int coins = AdministradorJuego.Instance.Coins;
                GuardarYActualizarUI(name, coins, victoryNameInputField, victorySaveButton, victoryLeaderboardText);
            }
        }

        private void GuardarYActualizarUI(string name, int coins, InputField inputField, Button saveBtn, Text leaderboardTxt)
        {
            if (string.IsNullOrEmpty(name)) return;
            name = name.Trim();
            if (string.IsNullOrEmpty(name)) return;

            LeaderboardData data = LoadLeaderboard();

            // Buscar si ya existe el nombre para sumarle las monedas ("ir sumando")
            HighScoreEntry entry = data.entries.Find(e => e.name.Equals(name, System.StringComparison.OrdinalIgnoreCase));
            if (entry != null)
            {
                entry.score += coins;
            }
            else
            {
                data.entries.Add(new HighScoreEntry { name = name, score = coins });
            }

            // Ordenar de mayor a menor
            data.entries.Sort((a, b) => b.score.CompareTo(a.score));

            // Limitar a top 10
            if (data.entries.Count > 10)
            {
                data.entries.RemoveRange(10, data.entries.Count - 10);
            }

            SaveLeaderboard(data);

            // Desactivar el input y el botón para evitar doble guardado
            if (inputField != null) inputField.interactable = false;
            if (saveBtn != null) saveBtn.interactable = false;

            // Actualizar la lista en pantalla
            ActualizarLeaderboardTexto(leaderboardTxt);
        }

        private void ActualizarLeaderboardTexto(Text textComponent)
        {
            if (textComponent == null) return;

            LeaderboardData data = LoadLeaderboard();
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            if (data.entries.Count == 0)
            {
                sb.AppendLine("Aún no hay puntuaciones");
            }
            else
            {
                int limit = Mathf.Min(data.entries.Count, 5);
                for (int i = 0; i < limit; i++)
                {
                    sb.AppendLine($"{i + 1}. {data.entries[i].name.ToUpper()} - {data.entries[i].score} pts");
                }
            }

            textComponent.text = sb.ToString();
        }
    }
}
