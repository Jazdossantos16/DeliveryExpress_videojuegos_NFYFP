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
        public static float IntensidadClima { get; set; } = 0f;

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
            
            // CRÍTICO: Detener la reproducción automática para poder modificar la duración sin errores de consola
            rainParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            
            // --- CONFIGURACIÓN DEL SISTEMA DE PARTÍCULAS ---
            var main = rainParticles.main;
            main.duration = 10f;
            main.loop = true;
            // Aleatorizar tiempo de vida para que no desaparezcan todas a la misma altura
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.9f, 1.4f); 
            main.startSpeed = 0f; // CRÍTICO: 0f para evitar que salgan disparadas en el eje Z (profundidad 3D) de la caja
            // Aleatorizar tamaño de gota para dar sensación de profundidad natural (primer plano vs fondo)
            main.startSize = new ParticleSystem.MinMaxCurve(0.018f, 0.035f); 
            main.startColor = new Color(0.85f, 0.9f, 1f, 0.15f); // Translúcido
            main.gravityModifier = 0f; // Sin gravedad para evitar curvaturas
            main.simulationSpace = ParticleSystemSimulationSpace.Local;

            // --- CONFIGURAR VELOCIDAD EN 2D STRICTO (X, Y) ---
            var velocityModule = rainParticles.velocityOverLifetime;
            velocityModule.enabled = true;
            velocityModule.space = ParticleSystemSimulationSpace.Local;
            
            // CRÍTICO: Todas las curvas de velocidad deben estar en el mismo modo (Dos Constantes).
            // Si Y usa (-16, -26), X y Z también deben usar (0, 0) en lugar de una constante simple.
            velocityModule.x = new ParticleSystem.MinMaxCurve(0f, 0f);
            velocityModule.y = new ParticleSystem.MinMaxCurve(-16f, -26f); 
            velocityModule.z = new ParticleSystem.MinMaxCurve(0f, 0f); // Estrictamente 0 en profundidad 2D

            // Emisión constante
            var emission = rainParticles.emission;
            emission.rateOverTime = steadyEmissionRate; 

            // Caja de emisión extremadamente ancha para cubrir toda la pantalla globalmente (calle, veredas y edificios)
            var shape = rainParticles.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(32f, 1f, 1f);

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
                pRenderer.lengthScale = 1.2f; // Gotas más cortas y sutiles
                pRenderer.velocityScale = 0.04f; // Estirado suave y natural por velocidad
                
                // CRÍTICO: Asignar Sorting Layer y Order alto para renderizar en el frente (Foreground)
                // Evita que la lluvia quede tapada por los edificios, obstáculos o la casa final (meta)
                pRenderer.sortingLayerName = "Default";
                pRenderer.sortingOrder = 25; 
                
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
