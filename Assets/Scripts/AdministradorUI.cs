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
        private Color[] originalHeartColors;

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

        [Header("Video de Introducción")]
        [SerializeField] private UnityEngine.Video.VideoClip introVideoClip;

        private UnityEngine.Video.VideoPlayer videoPlayer;
        private RenderTexture videoTexture;
        private RawImage videoRawImage;
        [SerializeField] private Text skipText;
        private bool isPlayingVideo = false;
        public bool IsPlayingVideo => isPlayingVideo;
        private bool cameFromMap = false;
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
        [SerializeField] private GameObject instructionsPanel;
        [SerializeField] private GameObject mapPanel;
        [SerializeField] private GameObject orderDetailsPanel;
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
        [Header("Sprites de Detalle de Pedido")]
        [SerializeField] private Sprite orderDetailsSpriteLevel1;
        [SerializeField] private Sprite orderDetailsSpriteLevel2;

        [Header("UI de Tienda")]
        [SerializeField] private GameObject shopPanel;
        [SerializeField] private Text shopCoinsText;
        [SerializeField] private Button buyBackpackButton;
        [SerializeField] private Button buySuspensionButton;
        [SerializeField] private Button buyBicycleButton;
        [SerializeField] private Button buyExtraLivesButton;
        [SerializeField] private Button buyPowerUpButton;
        [SerializeField] private GameObject backpackMaxOverlay;
        [SerializeField] private GameObject suspensionMaxOverlay;
        [SerializeField] private GameObject bicycleMaxOverlay;
        [SerializeField] private GameObject extraLivesMaxOverlay;
        [SerializeField] private GameObject powerUpMaxOverlay;
        [SerializeField] private GameObject shopNameInputPanel;
        [SerializeField] private InputField shopNameInputField;
        [SerializeField] private Button shopNameConfirmButton;
        [SerializeField] private GameObject shopGridPanel;
        [SerializeField] private GameObject shopSuccessPanel;
        private Coroutine hideSuccessCoroutine;

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

        // Coordenadas originales de los botones en el GridBackground de 1480x1045
        private readonly float[] btnOriginalX = { -425f, -122f, 185f, 490f, -426f };
        private readonly float[] btnOriginalY = { -49f, -48f, -49f, -47f, -417f };

        private void Awake()
        {
            Instance = this;

            // 3. Ajustar el tamaño y posicionamiento de los overlays a nivel de código en tiempo de ejecución
            AjustarOverlaysTiendaRuntime();
        }

        private Transform FindDeepChild(Transform parent, string name)
        {
            if (parent == null) return null;
            if (parent.name == name) return parent;
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                Transform found = FindDeepChild(child, name);
                if (found != null) return found;
            }
            return null;
        }

        private void AjustarOverlaysTiendaRuntime()
        {
            Debug.Log("⚙️ [AUTOPREP] Ajustando posiciones y anclas de overlays en tiempo de ejecución...");
            
            // Buscar el GridBackground en toda la escena (incluso si está inactivo)
            Transform gridPanel = null;
            var rootObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (var root in rootObjects)
            {
                gridPanel = FindDeepChild(root.transform, "GridBackground");
                if (gridPanel != null) break;
            }
            
            if (gridPanel == null)
            {
                Debug.LogWarning("⚠️ [AUTOPREP] No se encontró el GridBackground en ningún objeto raíz de la escena.");
                return;
            }

            for (int i = 0; i < 5; i++)
            {
                Transform buttonTrans = FindDeepChild(gridPanel, "BuyButton_" + i);
                Transform overlayTrans = FindDeepChild(gridPanel, "MaxOverlay_" + i);

                if (buttonTrans == null || overlayTrans == null)
                {
                    Debug.LogWarning($"⚠️ [AUTOPREP] No se encontró BuyButton_{i} o MaxOverlay_{i} de forma recursiva.");
                    continue;
                }

                RectTransform buyBtnRect = buttonTrans.GetComponent<RectTransform>();
                RectTransform overlayRect = overlayTrans.GetComponent<RectTransform>();

                if (buyBtnRect != null && overlayRect != null)
                {
                    // Asignar referencias dinámicamente a los campos locales si estaban vacíos
                    Button btnComponent = buttonTrans.GetComponent<Button>();
                    GameObject overlayObj = overlayTrans.gameObject;

                    if (i == 0) { buyBackpackButton = btnComponent; backpackMaxOverlay = overlayObj; }
                    else if (i == 1) { buySuspensionButton = btnComponent; suspensionMaxOverlay = overlayObj; }
                    else if (i == 2) { buyBicycleButton = btnComponent; bicycleMaxOverlay = overlayObj; }
                    else if (i == 3) { buyExtraLivesButton = btnComponent; extraLivesMaxOverlay = overlayObj; }
                    else if (i == 4) { buyPowerUpButton = btnComponent; powerUpMaxOverlay = overlayObj; }

                    // Forzar anclas y pivotes al centro para evitar que desfases del editor rompan la posición
                    overlayRect.anchorMin = new Vector2(0.5f, 0.5f);
                    overlayRect.anchorMax = new Vector2(0.5f, 0.5f);
                    overlayRect.pivot = new Vector2(0.5f, 0.5f);

                    // Posicionamiento horizontal perfectamente centrado usando las coordenadas originales e inmutables.
                    // El offset es de -41f a la izquierda del botón de compra para quedar exactamente simétrico en la tarjeta.
                    overlayRect.anchoredPosition = new Vector2(btnOriginalX[i] - 41f, btnOriginalY[i]);
                    overlayRect.sizeDelta = new Vector2(255f, 45f); // Cubre precio y comprar de forma simétrica

                    // Ajustar color de fondo
                    Image overlayImg = overlayTrans.GetComponent<Image>();
                    if (overlayImg != null)
                    {
                        overlayImg.color = new Color(0.12f, 0.16f, 0.14f, 0.98f);
                    }

                    // Ajustar texto
                    Text txt = overlayTrans.GetComponentInChildren<Text>();
                    if (txt != null)
                    {
                        txt.supportRichText = true;
                        txt.text = "<color=#4DFF4D><b>✓</b></color> COMPRADO";
                        txt.fontSize = 17;
                        txt.fontStyle = FontStyle.Bold;
                        txt.color = Color.white;
                        txt.alignment = TextAnchor.MiddleCenter;
                    }
                    
                    Debug.Log($"✅ [AUTOPREP] Row {i} alineado y enlazado: Pos={overlayRect.anchoredPosition}");
                }
            }
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


            // Efecto de pulso en el primer corazón si es dorado (vida extra activa)
            if (AdministradorJuego.Instance != null && AdministradorJuego.Instance.CurrentLives > 3)
            {
                if (heartImages != null && heartImages.Length > 0 && heartImages[0] != null && heartImages[0].enabled)
                {
                    float pulse = 1f + Mathf.Sin(Time.time * 6f) * 0.12f;
                    heartImages[0].transform.localScale = new Vector3(pulse, pulse, 1f);
                }
            }
            else
            {
                if (heartImages != null && heartImages.Length > 0 && heartImages[0] != null)
                {
                    heartImages[0].transform.localScale = Vector3.one;
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

                    // Tinte e inyección de texto según la mejora de Power Up
                    bool isPowerUpUpgraded = (AdministradorMejoras.Instance != null && AdministradorMejoras.Instance.IsPowerUpEquipped());
                    boosterImage.color = isPowerUpUpgraded ? new Color(1f, 0.7f, 0.1f, 1f) : Color.white;

                    // Manejo del texto flotante Texto_Potenciador
                    Transform txtTransform = boosterImage.transform.Find("Texto_Potenciador");
                    Text boosterText = null;
                    if (txtTransform == null)
                    {
                        GameObject txtObj = new GameObject("Texto_Potenciador");
                        txtObj.transform.SetParent(boosterImage.transform, false);
                        boosterText = txtObj.AddComponent<Text>();

                        // Copiar fuente si está disponible
                        if (livesText != null)
                        {
                            boosterText.font = livesText.font;
                        }
                        else
                        {
                            boosterText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                        }

                        boosterText.fontSize = 12;
                        boosterText.fontStyle = FontStyle.Bold;
                        boosterText.alignment = TextAnchor.MiddleCenter;

                        // Efecto Sombra
                        Shadow sh = txtObj.AddComponent<Shadow>();
                        sh.effectColor = Color.black;
                        sh.effectDistance = new Vector2(1f, -1f);

                        // Posicionamiento
                        RectTransform rt = txtObj.GetComponent<RectTransform>();
                        rt.anchorMin = new Vector2(0.5f, 1f);
                        rt.anchorMax = new Vector2(0.5f, 1f);
                        rt.pivot = new Vector2(0.5f, 0f);
                        rt.anchoredPosition = new Vector2(0f, 6f);
                        rt.sizeDelta = new Vector2(200f, 25f);
                    }
                    else
                    {
                        boosterText = txtTransform.GetComponent<Text>();
                        if (!txtTransform.gameObject.activeSelf)
                        {
                            txtTransform.gameObject.SetActive(true);
                        }
                    }

                    if (boosterText != null)
                    {
                        boosterText.enabled = true;
                        if (isPowerUpUpgraded)
                        {
                            boosterText.text = "¡X1.5 DURACIÓN!";
                            float alpha = 0.6f + Mathf.PingPong(Time.time * 3f, 0.4f);
                            boosterText.color = new Color(1f, 0.85f, 0.1f, alpha);
                        }
                        else
                        {
                            boosterText.text = "TURBO";
                            boosterText.color = Color.cyan;
                        }
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
            if (mapPanel != null)
            {
                mapPanel.SetActive(false);
            }
            if (orderDetailsPanel != null)
            {
                orderDetailsPanel.SetActive(false);
            }
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
            Time.timeScale = 1f; // Set to 1f so VideoPlayer advances frames normally
            StartCoroutine(PlayVideoRoutine());
        }

        private IEnumerator PlayVideoRoutine()
        {
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
            if (introVideoClip != null)
            {
                videoPlayer.source = UnityEngine.Video.VideoSource.VideoClip;
                videoPlayer.clip = introVideoClip;
            }
            else
            {
                videoPlayer.source = UnityEngine.Video.VideoSource.Url;
                videoPlayer.url = System.IO.Path.Combine(Application.streamingAssetsPath, "videojuego_prueba_202606182214.mp4");
            }
            videoPlayer.renderMode = UnityEngine.Video.VideoRenderMode.RenderTexture;
            videoPlayer.targetTexture = videoTexture;
            videoPlayer.timeUpdateMode = UnityEngine.Video.VideoTimeUpdateMode.UnscaledGameTime;
            
            bool musicOn = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;

#if UNITY_WEBGL && !UNITY_EDITOR
            // Configurar audio vía AudioSource para evitar desbordamiento de búfer (Buffer Overflow) en WebGL
            AudioSource videoAudioSource = videoGo.AddComponent<AudioSource>();
            videoAudioSource.playOnAwake = false;
            videoAudioSource.loop = false;
            videoAudioSource.spatialBlend = 0f; // Asegurar 2D
            
            videoPlayer.audioOutputMode = UnityEngine.Video.VideoAudioOutputMode.AudioSource;
            videoPlayer.controlledAudioTrackCount = 1;
            videoPlayer.EnableAudioTrack(0, true);
            videoPlayer.SetTargetAudioSource(0, videoAudioSource);
            videoAudioSource.mute = !musicOn;
#else
            // En Windows/Editor, si se usa AudioSource a timescale 0, el reloj de audio se congela
            // y causa que el VideoPlayer se quede trabado en el primer frame. Usamos Direct.
            videoPlayer.audioOutputMode = UnityEngine.Video.VideoAudioOutputMode.Direct;
            videoPlayer.controlledAudioTrackCount = 1;
            videoPlayer.EnableAudioTrack(0, true);
            videoPlayer.SetDirectAudioMute(0, !musicOn);
#endif
            
            // Suscribirse al evento de finalización
            videoPlayer.loopPointReached += AlTerminarVideo;

            // Activar y traer al frente el texto de Skip persistente (o su panel contenedor)
            if (skipText != null)
            {
                GameObject skipObj = skipText.transform.parent != null ? skipText.transform.parent.gameObject : skipText.gameObject;
                skipObj.SetActive(true);
                skipObj.transform.SetAsLastSibling();
            }

            // Preparar el video de forma asíncrona
            videoPlayer.Prepare();

            // Esperar hasta que esté preparado (con un timeout de 4 segundos para evitar pantalla negra infinita si el navegador o CORS lo bloquea)
            float prepStartTime = Time.realtimeSinceStartup;
            while (!videoPlayer.isPrepared)
            {
                if (Time.realtimeSinceStartup - prepStartTime > 4f)
                {
                    Debug.LogWarning("⚠️ Tiempo de preparación del video agotado. Saltando intro...");
                    FinalizarIntroVideo();
                    yield break;
                }
                yield return null;
            }

            // Iniciar reproducción
            videoPlayer.Play();
            Debug.Log("🎬 Reproduciendo video de intro (preparado con éxito).");

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
            if (orderDetailsPanel != null)
            {
                orderDetailsPanel.SetActive(false);
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
                // Guardar los colores originales si no se han guardado
                if (originalHeartColors == null || originalHeartColors.Length != heartImages.Length)
                {
                    originalHeartColors = new Color[heartImages.Length];
                    for (int i = 0; i < heartImages.Length; i++)
                    {
                        if (heartImages[i] != null)
                        {
                            originalHeartColors[i] = heartImages[i].color;
                        }
                    }
                }

                for (int i = 0; i < heartImages.Length; i++)
                {
                    if (heartImages[i] != null)
                    {
                        heartImages[i].enabled = i < currentLives;

                        // Si tiene vida extra activa (>3 vidas), pintamos el primer corazón de dorado/amarillo
                        if (i == 0 && currentLives > 3)
                        {
                            heartImages[0].color = new Color(1f, 0.85f, 0.2f, 1f);
                        }
                        else
                        {
                            // Restablecemos el color original
                            if (originalHeartColors != null && i < originalHeartColors.Length)
                            {
                                heartImages[i].color = originalHeartColors[i];
                            }
                        }
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

        #region Tienda de Mejoras
        public void AbrirTienda()
        {
            PlayClickSound();
            if (shopPanel != null)
            {
                shopPanel.SetActive(true);
                
                if (shopNameInputPanel != null)
                {
                    shopNameInputPanel.SetActive(true);
                    if (shopNameInputField != null)
                    {
                        shopNameInputField.text = ""; // Siempre inicia vacío para completar
                    }
                }
                
                if (shopGridPanel != null)
                {
                    shopGridPanel.SetActive(false);
                }
                
                if (shopCoinsText != null)
                {
                    shopCoinsText.transform.parent.gameObject.SetActive(false);
                }
                
                ActualizarInterfazTienda();
                Debug.Log("🛒 Tienda de mejoras abierta.");
            }
        }

        public void ConfirmarNombreTienda()
        {
            PlayClickSound();
            string nombre = "";
            if (shopNameInputField != null)
            {
                nombre = shopNameInputField.text.Trim();
            }
            
            if (!string.IsNullOrEmpty(nombre))
            {
                PlayerPrefs.SetString("PlayerName", nombre);
                PlayerPrefs.Save();

                // Cargar los puntos del leaderboard correspondientes a este nombre
                LeaderboardData data = LoadLeaderboard();
                HighScoreEntry entry = data.entries.Find(e => e.name.Equals(nombre, System.StringComparison.OrdinalIgnoreCase));
                if (entry != null)
                {
                    if (AdministradorJuego.Instance != null)
                    {
                        AdministradorJuego.Instance.SetCoins(entry.score);
                    }
                }
                else
                {
                    // Si el nombre no existe, registrarlo con las monedas actuales
                    int currentCoins = (AdministradorJuego.Instance != null) ? AdministradorJuego.Instance.Coins : 0;
                    data.entries.Add(new HighScoreEntry { name = nombre, score = currentCoins });
                    SaveLeaderboard(data);
                }
            }
            
            if (shopNameInputPanel != null)
            {
                shopNameInputPanel.SetActive(false);
            }
            
            if (shopGridPanel != null)
            {
                shopGridPanel.SetActive(true);
            }
            
            if (shopCoinsText != null)
            {
                shopCoinsText.transform.parent.gameObject.SetActive(true);
            }
            
            // Recargar las mejoras del jugador actual
            if (AdministradorMejoras.Instance != null)
            {
                AdministradorMejoras.Instance.LoadUpgrades();
            }
            
            AjustarOverlaysTiendaRuntime();
            ActualizarInterfazTienda();
        }

        private void ActualizarPuntosLeaderboardConMonedas()
        {
            if (AdministradorJuego.Instance == null) return;
            string currentName = PlayerPrefs.GetString("PlayerName", "").Trim();
            if (string.IsNullOrEmpty(currentName)) return;

            LeaderboardData data = LoadLeaderboard();
            HighScoreEntry entry = data.entries.Find(e => e.name.Equals(currentName, System.StringComparison.OrdinalIgnoreCase));
            if (entry != null)
            {
                entry.score = AdministradorJuego.Instance.Coins;
                data.entries.Sort((a, b) => b.score.CompareTo(a.score));
                SaveLeaderboard(data);
            }
        }

        public void CerrarTienda()
        {
            PlayClickSound();
            if (shopPanel != null)
            {
                shopPanel.SetActive(false);
                Debug.Log("🛒 Tienda de mejoras cerrada.");
            }
        }

        public void ActualizarInterfazTienda()
        {
            if (AdministradorJuego.Instance == null || AdministradorMejoras.Instance == null) return;

            int coins = AdministradorJuego.Instance.Coins;
            if (shopCoinsText != null)
            {
                shopCoinsText.text = coins.ToString();
            }

            // 1. Mochila Pro (Cost: 100)
            int packLvl = AdministradorMejoras.Instance.GetBackpackLevel();
            bool packEq = AdministradorMejoras.Instance.IsBackpackEquipped();
            ActualizarFilaMejora(packLvl, packEq, 100, buyBackpackButton, backpackMaxOverlay, coins, btnOriginalX[0], btnOriginalY[0]);

            // 2. Casco protector (Cost: 300)
            int suspLvl = AdministradorMejoras.Instance.GetSuspensionLevel();
            bool suspEq = AdministradorMejoras.Instance.IsSuspensionEquipped();
            ActualizarFilaMejora(suspLvl, suspEq, 300, buySuspensionButton, suspensionMaxOverlay, coins, btnOriginalX[1], btnOriginalY[1]);

            // 3. Moto de reparto (Cost: 1000)
            int bikeLvl = AdministradorMejoras.Instance.GetBicycleLevel();
            bool bikeEq = AdministradorMejoras.Instance.IsBicycleEquipped();
            ActualizarFilaMejora(bikeLvl, bikeEq, 1000, buyBicycleButton, bicycleMaxOverlay, coins, btnOriginalX[2], btnOriginalY[2]);

            // 4. Vidas Extra (Cost: 1500)
            int livesLvl = AdministradorMejoras.Instance.GetExtraLivesLevel();
            bool livesEq = AdministradorMejoras.Instance.IsExtraLivesEquipped();
            ActualizarFilaMejora(livesLvl, livesEq, 1500, buyExtraLivesButton, extraLivesMaxOverlay, coins, btnOriginalX[3], btnOriginalY[3]);

            // 5. Power up (Cost: 2000)
            int powerLvl = AdministradorMejoras.Instance.GetPowerUpLevel();
            bool powerEq = AdministradorMejoras.Instance.IsPowerUpEquipped();
            ActualizarFilaMejora(powerLvl, powerEq, 2000, buyPowerUpButton, powerUpMaxOverlay, coins, btnOriginalX[4], btnOriginalY[4]);
        }

        private void ActualizarFilaMejora(int currentLevel, bool isEquipped, int cost, Button buyButton, GameObject maxOverlay, int coinCount, float origX, float origY)
        {
            bool isPurchased = (currentLevel > 0);

            if (maxOverlay != null)
            {
                maxOverlay.SetActive(isPurchased);
                
                if (isPurchased)
                {
                    Image overlayImg = maxOverlay.GetComponent<Image>();
                    if (overlayImg != null)
                    {
                        overlayImg.raycastTarget = false; // Permitir que los clics traspasen al botón invisible de abajo
                    }

                    Text txt = maxOverlay.GetComponentInChildren<Text>();
                    if (txt != null)
                    {
                        txt.raycastTarget = false; // Permitir que los clics traspasen al botón invisible de abajo
                    }
                    
                    if (isEquipped)
                    {
                        // Color gris pizarra/verdoso original (activo/equipado)
                        if (overlayImg != null) overlayImg.color = new Color(0.12f, 0.16f, 0.14f, 0.98f);
                        if (txt != null)
                        {
                            txt.supportRichText = true;
                            txt.text = "<color=#4DFF4D><b>✓</b></color> EQUIPADO";
                        }
                    }
                    else
                    {
                        // Color gris pizarra más claro y apagado (desactivado/sin equipar)
                        if (overlayImg != null) overlayImg.color = new Color(0.26f, 0.3f, 0.28f, 0.98f);
                        if (txt != null)
                        {
                            txt.supportRichText = true;
                            txt.text = "<color=#A0A0A0><b>+</b></color> EQUIPAR";
                        }
                    }
                }
            }

            if (buyButton != null)
            {
                // Mantenemos el botón activo para que sea interactivo después de comprar
                buyButton.gameObject.SetActive(true);
                RectTransform buyBtnRect = buyButton.GetComponent<RectTransform>();

                if (isPurchased)
                {
                    buyButton.interactable = true;
                    
                    // Colocar el botón invisible exactamente sobre la cápsula de overlay para que haga clic en toda su superficie
                    if (maxOverlay != null && buyBtnRect != null)
                    {
                        RectTransform overlayRect = maxOverlay.GetComponent<RectTransform>();
                        if (overlayRect != null)
                        {
                            buyBtnRect.anchorMin = overlayRect.anchorMin;
                            buyBtnRect.anchorMax = overlayRect.anchorMax;
                            buyBtnRect.pivot = overlayRect.pivot;
                            buyBtnRect.anchoredPosition = overlayRect.anchoredPosition;
                            buyBtnRect.sizeDelta = overlayRect.sizeDelta;
                        }
                    }
                }
                else
                {
                    buyButton.interactable = (coinCount >= cost);
                    if (buyBtnRect != null)
                    {
                        // Asegurar posición y tamaño de compra original e inalterado
                        buyBtnRect.anchorMin = new Vector2(0.5f, 0.5f);
                        buyBtnRect.anchorMax = new Vector2(0.5f, 0.5f);
                        buyBtnRect.pivot = new Vector2(0.5f, 0.5f);
                        buyBtnRect.anchoredPosition = new Vector2(origX, origY);
                        buyBtnRect.sizeDelta = new Vector2(124f, 41f);
                    }
                }
            }
        }

        public void ComprarMejoraBicicleta()
        {
            Debug.Log($"[TIENDA] ComprarMejoraBicicleta llamado. MejorasInst={AdministradorMejoras.Instance != null} JuegoInst={AdministradorJuego.Instance != null}");
            if (AdministradorMejoras.Instance == null) return;

            if (AdministradorMejoras.Instance.GetBicycleLevel() > 0)
            {
                Debug.Log("[TIENDA] Alternando estado de equipamiento para Bicicleta");
                AdministradorMejoras.Instance.ToggleBicycleEquipped();
                PlayClickSound();
                if (ControladorJugador.Instance != null)
                {
                    AdministradorMejoras.Instance.ApplyUpgradesToGameplay(ControladorJugador.Instance);
                }
                ActualizarInterfazTienda();
                return;
            }

            if (AdministradorJuego.Instance != null) Debug.Log($"[TIENDA] Monedas actuales: {AdministradorJuego.Instance.Coins}");
            if (AdministradorMejoras.Instance.BuyUpgradeBicycleSpeed())
            {
                Debug.Log("[TIENDA] Compra de bicicleta EXITOSA");
                PlayClickSound();
                ActualizarPuntosLeaderboardConMonedas();
                ActualizarInterfazTienda();
                UpdateCoinsUI(AdministradorJuego.Instance.Coins);
                if (ControladorJugador.Instance != null)
                {
                    AdministradorMejoras.Instance.ApplyUpgradesToGameplay(ControladorJugador.Instance);
                }
                MostrarMensajeExito();
            }
            else
            {
                Debug.Log("[TIENDA] Compra de bicicleta FALLIDA");
            }
        }

        public void ComprarMejoraSuspension()
        {
            Debug.Log($"[TIENDA] ComprarMejoraSuspension llamado. MejorasInst={AdministradorMejoras.Instance != null} JuegoInst={AdministradorJuego.Instance != null}");
            if (AdministradorMejoras.Instance == null) return;

            if (AdministradorMejoras.Instance.GetSuspensionLevel() > 0)
            {
                Debug.Log("[TIENDA] Alternando estado de equipamiento para Suspension");
                AdministradorMejoras.Instance.ToggleSuspensionEquipped();
                PlayClickSound();
                if (ControladorJugador.Instance != null)
                {
                    AdministradorMejoras.Instance.ApplyUpgradesToGameplay(ControladorJugador.Instance);
                }
                ActualizarInterfazTienda();
                return;
            }

            if (AdministradorJuego.Instance != null) Debug.Log($"[TIENDA] Monedas actuales: {AdministradorJuego.Instance.Coins}");
            if (AdministradorMejoras.Instance.BuyUpgradeSuspension())
            {
                Debug.Log("[TIENDA] Compra de suspension EXITOSA");
                PlayClickSound();
                ActualizarPuntosLeaderboardConMonedas();
                ActualizarInterfazTienda();
                UpdateCoinsUI(AdministradorJuego.Instance.Coins);
                if (ControladorJugador.Instance != null)
                {
                    AdministradorMejoras.Instance.ApplyUpgradesToGameplay(ControladorJugador.Instance);
                }
                MostrarMensajeExito();
            }
            else
            {
                Debug.Log("[TIENDA] Compra de suspension FALLIDA");
            }
        }

        public void ComprarMejoraMochila()
        {
            Debug.Log($"[TIENDA] ComprarMejoraMochila llamado. MejorasInst={AdministradorMejoras.Instance != null} JuegoInst={AdministradorJuego.Instance != null}");
            if (AdministradorMejoras.Instance == null) return;

            if (AdministradorMejoras.Instance.GetBackpackLevel() > 0)
            {
                Debug.Log("[TIENDA] Alternando estado de equipamiento para Mochila");
                AdministradorMejoras.Instance.ToggleBackpackEquipped();
                PlayClickSound();
                if (ControladorJugador.Instance != null)
                {
                    AdministradorMejoras.Instance.ApplyUpgradesToGameplay(ControladorJugador.Instance);
                }
                ActualizarInterfazTienda();
                return;
            }

            if (AdministradorJuego.Instance != null) Debug.Log($"[TIENDA] Monedas actuales: {AdministradorJuego.Instance.Coins}");
            if (AdministradorMejoras.Instance.BuyUpgradeBackpack())
            {
                Debug.Log("[TIENDA] Compra de mochila EXITOSA");
                PlayClickSound();
                ActualizarPuntosLeaderboardConMonedas();
                ActualizarInterfazTienda();
                UpdateCoinsUI(AdministradorJuego.Instance.Coins);
                if (ControladorJugador.Instance != null)
                {
                    AdministradorMejoras.Instance.ApplyUpgradesToGameplay(ControladorJugador.Instance);
                }
                MostrarMensajeExito();
            }
            else
            {
                Debug.Log("[TIENDA] Compra de mochila FALLIDA");
            }
        }

        public void ComprarMejoraVidasExtra()
        {
            Debug.Log($"[TIENDA] ComprarMejoraVidasExtra llamado. MejorasInst={AdministradorMejoras.Instance != null} JuegoInst={AdministradorJuego.Instance != null}");
            if (AdministradorMejoras.Instance == null) return;

            if (AdministradorMejoras.Instance.GetExtraLivesLevel() > 0)
            {
                Debug.Log("[TIENDA] Alternando estado de equipamiento para Vidas Extra");
                AdministradorMejoras.Instance.ToggleExtraLivesEquipped();
                PlayClickSound();
                if (ControladorJugador.Instance != null)
                {
                    AdministradorMejoras.Instance.ApplyUpgradesToGameplay(ControladorJugador.Instance);
                }
                ActualizarInterfazTienda();
                return;
            }

            if (AdministradorJuego.Instance != null) Debug.Log($"[TIENDA] Monedas actuales: {AdministradorJuego.Instance.Coins}");
            if (AdministradorMejoras.Instance.BuyUpgradeExtraLives())
            {
                Debug.Log("[TIENDA] Compra de vidas extra EXITOSA");
                PlayClickSound();
                ActualizarPuntosLeaderboardConMonedas();
                ActualizarInterfazTienda();
                UpdateCoinsUI(AdministradorJuego.Instance.Coins);
                if (ControladorJugador.Instance != null)
                {
                    AdministradorMejoras.Instance.ApplyUpgradesToGameplay(ControladorJugador.Instance);
                }
                MostrarMensajeExito();
            }
            else
            {
                Debug.Log("[TIENDA] Compra de vidas extra FALLIDA");
            }
        }

        public void ComprarMejoraPowerUp()
        {
            Debug.Log($"[TIENDA] ComprarMejoraPowerUp llamado. MejorasInst={AdministradorMejoras.Instance != null} JuegoInst={AdministradorJuego.Instance != null}");
            if (AdministradorMejoras.Instance == null) return;

            if (AdministradorMejoras.Instance.GetPowerUpLevel() > 0)
            {
                Debug.Log("[TIENDA] Alternando estado de equipamiento para Power Up");
                AdministradorMejoras.Instance.TogglePowerUpEquipped();
                PlayClickSound();
                if (ControladorJugador.Instance != null)
                {
                    AdministradorMejoras.Instance.ApplyUpgradesToGameplay(ControladorJugador.Instance);
                }
                ActualizarInterfazTienda();
                return;
            }

            if (AdministradorJuego.Instance != null) Debug.Log($"[TIENDA] Monedas actuales: {AdministradorJuego.Instance.Coins}");
            if (AdministradorMejoras.Instance.BuyUpgradePowerUp())
            {
                Debug.Log("[TIENDA] Compra de power up EXITOSA");
                PlayClickSound();
                ActualizarPuntosLeaderboardConMonedas();
                ActualizarInterfazTienda();
                UpdateCoinsUI(AdministradorJuego.Instance.Coins);
                if (ControladorJugador.Instance != null)
                {
                    AdministradorMejoras.Instance.ApplyUpgradesToGameplay(ControladorJugador.Instance);
                }
                MostrarMensajeExito();
            }
            else
            {
                Debug.Log("[TIENDA] Compra de power up FALLIDA");
            }
        }

        public void ComprarMejoraTiempoExtra()
        {
        }

        private void MostrarMensajeExito()
        {
            if (shopSuccessPanel == null) return;
            if (hideSuccessCoroutine != null)
            {
                StopCoroutine(hideSuccessCoroutine);
            }
            shopSuccessPanel.SetActive(true);
            hideSuccessCoroutine = StartCoroutine(OcultarMensajeExitoDespues(1.5f));
        }

        private IEnumerator OcultarMensajeExitoDespues(float segundos)
        {
            yield return new WaitForSecondsRealtime(segundos);
            if (shopSuccessPanel != null)
            {
                shopSuccessPanel.SetActive(false);
            }
            hideSuccessCoroutine = null;
        }
        #endregion

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

        public void AbrirInstrucciones()
        {
            PlayClickSound();
            if (instructionsPanel != null)
            {
                instructionsPanel.SetActive(true);
                Debug.Log("📖 Panel de instrucciones abierto.");
            }
        }

        public void CerrarInstrucciones()
        {
            PlayClickSound();
            if (instructionsPanel != null)
            {
                instructionsPanel.SetActive(false);
                Debug.Log("📖 Panel de instrucciones cerrado.");
            }
        }

        public void AbrirMapa()
        {
            PlayClickSound();
            if (mapPanel != null)
            {
                mapPanel.SetActive(true);
                Debug.Log("🗺️ Panel de mapa abierto.");
            }
        }

        public void CerrarMapa()
        {
            PlayClickSound();
            if (mapPanel != null)
            {
                mapPanel.SetActive(false);
                Debug.Log("🗺️ Panel de mapa cerrado.");
            }
        }

        /// <summary>
        /// Selecciona un nivel desde el mapa y abre el detalle del pedido.
        /// </summary>
        public void SeleccionarNivelMapa(int nivel)
        {
            PlayClickSound();
            if (AdministradorJuego.Instance != null)
            {
                AdministradorJuego.Instance.ConfigurarJornada(nivel);
            }
            AbrirDetallePedido();
        }

        public void AbrirDetallePedido()
        {
            PlayClickSound();
            if (mapPanel != null)
            {
                cameFromMap = mapPanel.activeSelf;
                mapPanel.SetActive(false);
            }
            else
            {
                cameFromMap = false;
            }

            if (orderDetailsPanel != null)
            {
                Transform contentTrans = orderDetailsPanel.transform.Find("Contenido");
                if (contentTrans != null)
                {
                    Image img = contentTrans.GetComponent<Image>();
                    if (img != null)
                    {
                        int currentDay = AdministradorJuego.Instance != null ? AdministradorJuego.Instance.CurrentDay : 1;
                        if (currentDay == 2 && orderDetailsSpriteLevel2 != null)
                        {
                            img.sprite = orderDetailsSpriteLevel2;
                        }
                        else if (orderDetailsSpriteLevel1 != null)
                        {
                            img.sprite = orderDetailsSpriteLevel1;
                        }
                    }
                }

                orderDetailsPanel.SetActive(true);
                Debug.Log("📦 Panel de detalle del pedido abierto. ¿Viene del mapa? " + cameFromMap);
            }
        }

        public void CerrarDetallePedido()
        {
            PlayClickSound();
            if (orderDetailsPanel != null)
            {
                orderDetailsPanel.SetActive(false);
                Debug.Log("📦 Panel de detalle del pedido cerrado.");
            }
            if (cameFromMap && mapPanel != null)
            {
                mapPanel.SetActive(true);
            }
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
            if (configBackgroundImage != null)
            {
                if (musicEnabled && soundEnabled)
                {
                    configBackgroundImage.sprite = imgConfigBoth;
                }
                else if (!musicEnabled && soundEnabled)
                {
                    configBackgroundImage.sprite = imgConfigNoMusic;
                }
                else if (musicEnabled && !soundEnabled)
                {
                    configBackgroundImage.sprite = imgConfigNoSound;
                }
                else // !musicEnabled && !soundEnabled
                {
                    configBackgroundImage.sprite = imgConfigNone;
                }
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
