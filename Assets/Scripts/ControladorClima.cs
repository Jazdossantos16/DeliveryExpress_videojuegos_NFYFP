using UnityEngine;

namespace DeliveryExpress
{
    /// <summary>
    /// Controlador de efectos climáticos procedurales para las distintas jornadas de trabajo.
    /// Genera sistemas de partículas de lluvia en tiempo de ejecución sin depender de assets externos.
    /// </summary>
    public class ControladorClima : MonoBehaviour
    {
        private ParticleSystem rainParticles;

        private void Start()
        {
            int currentDay = AdministradorJuego.Instance != null ? AdministradorJuego.Instance.CurrentDay : 1;
            if (currentDay == 2)
            {
                CrearSistemaLluvia();
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
            main.startLifetime = 1.5f;
            main.startSpeed = 16f;
            main.startSize = 0.12f;
            main.startColor = new Color(0.65f, 0.78f, 0.95f, 0.35f); // Celeste lluvia suave y translúcido
            main.gravityModifier = 1.2f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            // Emisión de partículas por segundo
            var emission = rainParticles.emission;
            emission.rateOverTime = 95f; 

            // Caja de emisión para cubrir todo el ancho de la calle
            var shape = rainParticles.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(16f, 1f, 1f);

            // Inclinamos el ángulo de caída simulando el empuje del viento lateral
            rainObj.transform.rotation = Quaternion.Euler(75f, 0f, 18f);

            // Configurar el renderizador en modo estirado (Stretch) para simular gotas veloces
            var pRenderer = rainObj.GetComponent<ParticleSystemRenderer>();
            if (pRenderer != null)
            {
                pRenderer.renderMode = ParticleSystemRenderMode.Stretch;
                pRenderer.lengthScale = 3.5f;
                pRenderer.velocityScale = 0.4f;
            }

            rainParticles.Play();
            Debug.Log("🌧️ Sistema de lluvia procedural creado para la Jornada 2.");
        }
    }
}
