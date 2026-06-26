using UnityEngine;

namespace DeliveryExpress
{
    /// <summary>
    /// Administrador central de audio para el juego (BGM y SFX).
    /// Controla la música de fondo en bucle y reproduce efectos de sonido de forma segura.
    /// </summary>
    public class AdministradorAudio : MonoBehaviour
    {
        public static AdministradorAudio Instance { get; private set; }

        [Header("Música de Fondo")]
        [SerializeField] private AudioClip backgroundMusic;

        [Header("Efectos de Sonido")]
        [SerializeField] private AudioClip coinSound;
        [SerializeField] private AudioClip collisionSound;
        [SerializeField] private AudioClip powerUpSound;
        [SerializeField] private AudioClip lifeSound;
        [SerializeField] private AudioClip defeatSound;
        [SerializeField] private AudioClip victorySound;
        [SerializeField] private AudioClip laneSwitchSound;
        [SerializeField] private AudioClip buttonClickSound;

        private AudioSource bgmSource;
        private AudioSource sfxSource;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                // Opcional para persistencia entre niveles si se agregan más escenas
                DontDestroyOnLoad(gameObject);
                InicializarComponentesAudio();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InicializarComponentesAudio()
        {
            // Buscar o crear los componentes AudioSource en el GameObject
            AudioSource[] sources = GetComponents<AudioSource>();
            
            if (sources.Length >= 2)
            {
                bgmSource = sources[0];
                sfxSource = sources[1];
            }
            else
            {
                // Limpiar si hay uno solo para crearlos de forma limpia
                foreach (var s in sources)
                {
                    Destroy(s);
                }
                bgmSource = gameObject.AddComponent<AudioSource>();
                sfxSource = gameObject.AddComponent<AudioSource>();
            }

            // Configurar AudioSource para la Música de Fondo (BGM)
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;
            bgmSource.volume = 0.5f; // Volumen moderado para la música de fondo
            bgmSource.spatialBlend = 0f; // Asegurar que sea 2D

            // Configurar AudioSource para Efectos de Sonido (SFX)
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
            sfxSource.volume = 0.8f; // Efectos ligeramente más fuertes
            sfxSource.spatialBlend = 0f; // Asegurar que sea 2D
        }

        private void Start()
        {
            // Cargar estado inicial de la música desde PlayerPrefs
            bool musicEnabled = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;
            SetMusicEnabled(musicEnabled);
        }

        /// <summary>
        /// Activa o desactiva la reproducción de la música de fondo.
        /// </summary>
        public void SetMusicEnabled(bool enabled)
        {
            if (bgmSource == null) return;

            if (enabled)
            {
                if (backgroundMusic != null)
                {
                    if (bgmSource.clip != backgroundMusic)
                    {
                        bgmSource.clip = backgroundMusic;
                    }
                    if (!bgmSource.isPlaying)
                    {
                        bgmSource.Play();
                    }
                }
                bgmSource.mute = false;
            }
            else
            {
                bgmSource.mute = true;
            }
        }

        /// <summary>
        /// Reproduce el efecto de sonido al recoger una moneda.
        /// </summary>
        public void PlayCoinSound()
        {
            PlaySFX(coinSound);
        }

        /// <summary>
        /// Reproduce el efecto de sonido al chocar contra un obstáculo.
        /// </summary>
        public void PlayCollisionSound()
        {
            PlaySFX(collisionSound);
        }

        /// <summary>
        /// Reproduce el efecto de sonido al recoger el potenciador de energía (rayo).
        /// </summary>
        public void PlayPowerUpSound()
        {
            PlaySFX(powerUpSound);
        }

        /// <summary>
        /// Reproduce el efecto de sonido al recoger una hamburguesa (vida).
        /// </summary>
        public void PlayLifeSound()
        {
            PlaySFX(lifeSound);
        }

        /// <summary>
        /// Reproduce el efecto de sonido de victoria al terminar el nivel.
        /// </summary>
        public void PlayVictorySound()
        {
            PlaySFX(victorySound);
        }

        /// <summary>
        /// Reproduce el efecto de sonido de derrota al perder todas las vidas.
        /// </summary>
        public void PlayDefeatSound()
        {
            PlaySFX(defeatSound);
        }

        /// <summary>
        /// Reproduce el efecto de sonido al cambiar de carril.
        /// </summary>
        public void PlayLaneSwitchSound()
        {
            PlaySFX(laneSwitchSound);
        }

        /// <summary>
        /// Reproduce el efecto de sonido al presionar un botón.
        /// </summary>
        public void PlayButtonClickSound()
        {
            PlaySFX(buttonClickSound);
        }

        private void PlaySFX(AudioClip clip)
        {
            if (sfxSource == null || clip == null) return;
            sfxSource.PlayOneShot(clip);
        }
    }
}
