using UnityEngine;

namespace DeliveryExpress
{
    /// <summary>
    /// Controlador de efectos climáticos procedurales para las distintas jornadas de trabajo.
    /// Genera sistemas de partículas de lluvia en tiempo de ejecución sin depender de assets externos.
    /// Utiliza espacio local para que la lluvia siga a la cámara y nunca se corte en los bordes.
    /// </summary>
    public class ControladorClima : MonoBehaviour
    {
        private ParticleSystem rainParticles;
        private float steadyEmissionRate = 120f;

        /// <summary>
        /// Intensidad climática constante del nivel (1.0 cuando está activo).
        /// Afecta tanto la cantidad de lluvia visual como la fuerza física del viento en el jugador.
        /// </summary>
        public static float IntensidadClima { get; private set; } = 0f;

        private void Start()
        {
            int currentDay = AdministradorJuego.Instance != null ? AdministradorJuego.Instance.CurrentDay : 1;
            if (currentDay == 2)
            {
                IntensidadClima = 1f; // Clima constante para el Nivel 2
                CrearSistemaLluvia();
            }
            else
            {
                IntensidadClima = 0f;
            }
        }

        private void CrearSistemaLluvia()
        {
            GameObject rainObj = new GameObject("SistemaLluvia");
            rainObj.transform.SetParent(transform);
            
            // Posicionar la lluvia por encima del rango visible de la cámara ortográfica
            Camera cam = Camera.main;
            if (cam != null)
            {
                rainObj.transform.position = new Vector3(0f, cam.orthographicSize + 2f, 0f);
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
            main.startLifetime = 1.2f; // Tiempo de vida suficiente para cruzar la pantalla en 2D
            main.startSpeed = 0f; // CRÍTICO: 0f para evitar que salgan disparadas en el eje Z (profundidad 3D) de la caja
            main.startSize = 0.035f; // Gotas finas y estéticas
            main.startColor = new Color(0.85f, 0.9f, 1f, 0.15f); // Translúcido
            main.gravityModifier = 0f; // Sin gravedad para evitar curvaturas
            main.simulationSpace = ParticleSystemSimulationSpace.Local;

            // --- CONFIGURAR VELOCIDAD EN 2D STRICTO (X, Y) ---
            var velocityModule = rainParticles.velocityOverLifetime;
            velocityModule.enabled = true;
            velocityModule.space = ParticleSystemSimulationSpace.Local;
            // Para caer de forma totalmente vertical (arriba a abajo) a 22 units/sec
            velocityModule.x = new ParticleSystem.MinMaxCurve(0f);
            velocityModule.y = new ParticleSystem.MinMaxCurve(-22f);
            velocityModule.z = new ParticleSystem.MinMaxCurve(0f); // Estrictamente 0 en profundidad 2D

            // Emisión constante
            var emission = rainParticles.emission;
            emission.rateOverTime = steadyEmissionRate; 

            // Caja de emisión para cubrir todo el ancho de la calle
            var shape = rainParticles.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(16f, 1f, 1f);

            // Mantenemos la rotación en cero para asegurar que la caja emita horizontalmente en el plano 2D
            rainObj.transform.rotation = Quaternion.identity;

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
                pRenderer.lengthScale = 2.0f; // Estiramiento natural
                pRenderer.velocityScale = 0.15f; // Estirado suave por velocidad
                if (defaultSpriteMat != null)
                {
                    pRenderer.material = defaultSpriteMat;
                }
            }

            rainParticles.Play();
            Debug.Log("🌧️ Sistema de lluvia procedural constante (Local) creado para la Jornada 2.");
        }

        private void OnDestroy()
        {
            IntensidadClima = 0f; // Limpiar estado al destruir la escena
        }
    }
}
