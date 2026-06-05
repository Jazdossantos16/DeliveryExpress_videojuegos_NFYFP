using UnityEngine;

namespace DeliveryExpress
{
    /// <summary>
    /// Script especial y premium que automatiza la creación completa de la escena en Unity a tiempo de ejecución.
    /// Si tienes una escena vacía, simplemente arrastra este script a cualquier objeto vacío, presiona PLAY,
    /// y el nivel se auto-construirá (Jugador, AdministradorJuego, Spawner y NPCs) para que puedas jugar inmediatamente.
    /// </summary>
    public class ConstructorEscena : MonoBehaviour
    {
        [Header("Assets Visuales")]
        [Tooltip("Arrastra aquí el sprite o spritesheet del repartidor desde Assets/Sprites")]
        [SerializeField] private Sprite playerSprite;

        [Tooltip("Arrastra aquí el spritesheet de los vehículos (autos)")]
        [SerializeField] private Sprite carsSprite;

        private void Start()
        {
            BuildLevel();
        }

        [ContextMenu("Auto-Construir Nivel")]
        public void BuildLevel()
        {
            Debug.Log("🏗️ Iniciando auto-construcción del nivel Delivery Express...");

            // 1. Configurar Cámara Principal
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                mainCam.transform.position = new Vector3(0, 0, -10);
                mainCam.orthographic = true;
                mainCam.orthographicSize = 6.0f; // Tamaño óptimo para ver el repartidor y veredas
                mainCam.backgroundColor = new Color(0.11f, 0.24f, 0.15f); // Color verde pasto
                mainCam.clearFlags = CameraClearFlags.SolidColor;
            }

            // 2. Crear Calle de Asfalto (Fondo)
            CreateRoadBackground();

            // 3. Crear AdministradorJuego Central
            if (AdministradorJuego.Instance == null)
            {
                GameObject gmObj = new GameObject("_GameManager");
                gmObj.AddComponent<AdministradorJuego>();
                gmObj.AddComponent<AdministradorMejoras>();
                Debug.Log("✅ AdministradorJuego y AdministradorMejoras instanciados correctamente.");
            }

            // 4. Crear Jugador (Repartidor)
            ControladorJugador existingPlayer = FindFirstObjectByType<ControladorJugador>();
            if (existingPlayer == null)
            {
                GameObject playerObj = new GameObject("Player");
                playerObj.tag = "Player";
                playerObj.transform.position = new Vector3(0, -3.5f, 0);

                // Agregar SpriteRenderer y cargar el sprite
                SpriteRenderer sr = playerObj.AddComponent<SpriteRenderer>();
                Shader spriteShader = Shader.Find("Sprites/Default");
                if (spriteShader != null) sr.sharedMaterial = new Material(spriteShader);
                if (playerSprite != null)
                {
                    sr.sprite = playerSprite;
                }
                else
                {
                    // Crear un rectángulo temporal de color naranja si el usuario no asignó el sprite en el inspector
                    Texture2D tex = new Texture2D(32, 32);
                    for (int y = 0; y < 32; y++)
                        for (int x = 0; x < 32; x++)
                            tex.SetPixel(x, y, new Color(0.9f, 0.35f, 0.1f));
                    tex.Apply();
                    sr.sprite = Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
                }
                sr.sortingOrder = 10; // Dibujar por encima de la calle

                // Agregar Animator
                playerObj.AddComponent<Animator>();

                // Configurar físicas
                Rigidbody2D rb = playerObj.AddComponent<Rigidbody2D>();
                rb.gravityScale = 0f;
                rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

                // Configurar colisionador BoxCollider2D
                BoxCollider2D col = playerObj.AddComponent<BoxCollider2D>();
                col.size = new Vector2(1f, 1.6f);
                col.isTrigger = false;

                // Agregar Controlador del jugador
                playerObj.AddComponent<ControladorJugador>();
                Debug.Log("✅ Objeto Player (Repartidor) auto-configurado.");
            }

            // 5. Crear Spawner de Obstáculos
            GeneradorObstaculos spawner = FindFirstObjectByType<GeneradorObstaculos>();
            if (spawner == null)
            {
                GameObject spawnerObj = new GameObject("GeneradorObstaculos");
                spawnerObj.transform.position = new Vector3(0, 8f, 0);
                spawner = spawnerObj.AddComponent<GeneradorObstaculos>();
                Debug.Log("✅ GeneradorObstaculos instanciado en carriles virtuales.");
            }

            // 6. Remover los NPCs de las veredas, la entrega es al final del recorrido
            // CreateNPCs();

            Debug.Log("🚀 ¡Nivel auto-construido con éxito! Presiona PLAY para jugar.");
        }

        private void CreateRoadBackground()
        {
            // Creamos un rectángulo de asfalto en el centro
            GameObject road = new GameObject("RoadBackground");
            road.transform.position = new Vector3(0, 0, 1);
            SpriteRenderer sr = road.AddComponent<SpriteRenderer>();
            Shader spriteShader = Shader.Find("Sprites/Default");
            if (spriteShader != null) sr.sharedMaterial = new Material(spriteShader);
            
            Texture2D tex = new Texture2D(128, 128);
            for (int y = 0; y < 128; y++)
            {
                for (int x = 0; x < 128; x++)
                {
                    // Asfalto gris oscuro
                    if (x > 20 && x < 108)
                    {
                        tex.SetPixel(x, y, new Color(0.18f, 0.17f, 0.23f));
                    }
                    // Cordón / Vereda lateral beige
                    else
                    {
                        tex.SetPixel(x, y, new Color(0.83f, 0.80f, 0.76f));
                    }
                }
            }
            tex.Apply();
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, 128, 128), new Vector2(0.5f, 0.5f), 16f);
            
            // Escalar para cubrir toda la cámara
            road.transform.localScale = new Vector3(2.5f, 10f, 1);
        }

        private void CreateNPCs()
        {
            // Cliente 1 (Vereda Izquierda)
            GameObject npc1 = new GameObject("Cliente_NPC_Izquierda");
            npc1.transform.position = new Vector3(-4.5f, 2.5f, 0);
            SpriteRenderer sr1 = npc1.AddComponent<SpriteRenderer>();
            SetNPCTempSprite(sr1, Color.blue);
            BoxCollider2D col1 = npc1.AddComponent<BoxCollider2D>();
            col1.isTrigger = true;
            npc1.AddComponent<NpcCliente>();

            // Cliente 2 (Vereda Derecha)
            GameObject npc2 = new GameObject("Cliente_NPC_Derecha");
            npc2.transform.position = new Vector3(4.5f, -1f, 0);
            SpriteRenderer sr2 = npc2.AddComponent<SpriteRenderer>();
            SetNPCTempSprite(sr2, Color.green);
            BoxCollider2D col2 = npc2.AddComponent<BoxCollider2D>();
            col2.isTrigger = true;
            npc2.AddComponent<NpcCliente>();
        }

        private void SetNPCTempSprite(SpriteRenderer sr, Color color)
        {
            Shader spriteShader = Shader.Find("Sprites/Default");
            if (spriteShader != null) sr.sharedMaterial = new Material(spriteShader);
            Texture2D tex = new Texture2D(32, 32);
            for (int y = 0; y < 32; y++)
                for (int x = 0; x < 32; x++)
                    tex.SetPixel(x, y, color);
            tex.Apply();
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
            sr.sortingOrder = 9;
        }
    }
}
