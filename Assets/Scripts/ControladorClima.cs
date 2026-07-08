using UnityEngine;

namespace DeliveryExpress
{
    /// <summary>
    /// Controlador de efectos climáticos procedurales para las distintas jornadas de trabajo.
    /// Genera sistemas de partículas de lluvia en tiempo de ejecución sin depender de assets externos.
    /// Soporta intervalos de lluvia y viento con ráfagas suaves que van y vienen.
    /// </summary>
    public class ControladorClima : MonoBehaviour
    {
        private ParticleSystem rainParticles;
        private float maxEmissionRate = 140f;

        /// <summary>
        /// Intensidad climática actual de la ráfaga (de 0 a 1).
        /// Afecta tanto la cantidad de lluvia visual como la fuerza física del viento en el jugador.
        /// </summary>
        public static float IntensidadClima { get; private set; } = 0f;

        private void Start()
        {
            IntensidadClima = 0f; // Resetear al iniciar la jornada
            
            int currentDay = AdministradorJuego.Instance != null ? AdministradorJuego.Instance.CurrentDay : 1;
            if (currentDay == 2)
            {
                CrearSistemaLluvia();
            }
        }

        private void Update()
        {
            if (rainParticles == null) return;

            // Oscilación suave con períodos de calma total.
            // La función Seno oscila entre -1 y 1. La escalamos y limitamos para tener:
            // - Un período de tormenta de viento y lluvia (cuando es > 0)
            // - Un período de calma absoluta (cuando es <= 0)
            float wave = Mathf.Sin(Time.time * 0.25f); // Ciclo completo de aprox 25 segundos
            IntensidadClima = Mathf.Clamp01(wave * 1.6f + 0.4f); 

            // Actualizar tasa de emisión de partículas dinámicamente
            var emission = rainParticles.emission;
            emission.rateOverTime = IntensidadClima * maxEmissionRate;
        }

        private void CrearSistemaLluvia()
        {
            GameObject rainObj = new GameObject("SistemaLluvia");
            rainObj.transform.SetParent(transform);
            
            // Posicionar la lluvia por encima del rango visible de la cámara ortográfica
            Camera cam = Camera.main;
            if (cam != null)
            {
                rainObj.transform.position = new Vector3(0f, cam.orthographicSize + 2.5f, 0f);
            }
            else
            {
                rainObj.transform.position = new Vector3(0f, 10f, 0f);
            }

            // Añadir el componente de sistema de partículas
            rainParticles = rainObj.AddComponent<ParticleSystem>();
            
            // --- CONFIGURACIÓN DEL SISTEMA DE PARTÍCULAS ---
            var main = rainParticles.main;
            main.duration = 10f;
            main.loop = true;
            main.startLifetime = 1.8f; // Mayor duración para asegurar que caigan por debajo del límite de pantalla sin cortarse
            main.startSpeed = 22f;
            main.startSize = 0.035f; // Gotas muy finas y estilizadas
            main.startColor = new Color(0.85f, 0.9f, 1f, 0.15f); // Muy translúcido para evitar barras opacas molestas
            main.gravityModifier = 1.3f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            // Emisión inicial (será controlada en el Update)
            var emission = rainParticles.emission;
            emission.rateOverTime = 0f; 

            // Caja de emisión para cubrir todo el ancho de la calle
            var shape = rainParticles.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(16f, 1f, 1f);

            // Inclinamos el ángulo de caída simulando el empuje del viento lateral
            rainObj.transform.rotation = Quaternion.Euler(75f, 0f, 18f);

            // Obtener el material de Sprite por defecto para evitar el shader rosa (magenta)
            Material defaultSpriteMat = null;
            GameObject tempObj = new GameObject("TempSprite");
            SpriteRenderer tempRenderer = tempObj.AddComponent<SpriteRenderer>();
            if (tempRenderer != null)
            {
                defaultSpriteMat = tempRenderer.sharedMaterial;
            }
            Destroy(tempObj);

            // Configurar el renderizador en modo estirado (Stretch) para simular gotas veloces
            var pRenderer = rainObj.GetComponent<ParticleSystemRenderer>();
            if (pRenderer != null)
            {
                pRenderer.renderMode = ParticleSystemRenderMode.Stretch;
                pRenderer.lengthScale = 2.0f; // Estiramiento menos exagerado
                pRenderer.velocityScale = 0.15f; // Ajuste suave de estirado por velocidad
                if (defaultSpriteMat != null)
                {
                    pRenderer.material = defaultSpriteMat;
                }
            }

            rainParticles.Play();
            Debug.Log("🌧️ Sistema de lluvia procedural creado para la Jornada 2 con ciclo dinámico.");
        }

        private void OnDestroy()
        {
            IntensidadClima = 0f; // Asegurar que el clima vuelva a cero al destruir el nivel
        }
    }
}
