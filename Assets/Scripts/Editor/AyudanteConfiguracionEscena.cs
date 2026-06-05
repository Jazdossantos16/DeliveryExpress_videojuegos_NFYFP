using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using UnityEditor.Animations;
using System.Linq;
using DeliveryExpress;

namespace DeliveryExpress.Editor
{
    [InitializeOnLoad]
    public static class AyudanteConfiguracionEscena
    {
        static AyudanteConfiguracionEscena()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

            // Auto-configurar la escena de forma segura en Edit Mode al compilar o iniciar
            EditorApplication.delayCall += () =>
            {
                AutoCheckAndFixScene();
            };
        }

        private static bool isCheckingScene = false;
        private static void AutoCheckAndFixScene()
        {
            if (EditorApplication.isPlaying || EditorApplication.isCompiling || isCheckingScene) return;

            // Verificar si hay corazones o hamburguesas huérfanos en la raíz de la escena o si la jerarquía es inválida
            bool needsFix = false;
            try
            {
                var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
                if (activeScene.isLoaded)
                {
                    GameObject[] rootObjects = activeScene.GetRootGameObjects();
                    foreach (GameObject go in rootObjects)
                    {
                        if (go != null && (go.name.StartsWith("Corazon_") || go.name.StartsWith("Hamburguesa_")))
                        {
                            needsFix = true;
                            break;
                        }
                    }
                }
            }
            catch (System.Exception)
            {
                // Evitar errores si la escena no está lista
            }

            if (!needsFix)
            {
                // Si aún existe el texto de vidas antiguo, reconstruir para eliminarlo
                if (GameObject.Find("Texto_Vidas") != null)
                {
                    needsFix = true;
                }
            }

            if (!needsFix)
            {
                GameObject livesContainer = GameObject.Find("Contenedor_Vidas");
                if (livesContainer == null)
                {
                    needsFix = true;
                }
                else
                {
                    if (livesContainer.transform.childCount < 3)
                    {
                        needsFix = true;
                    }
                    else
                    {
                        // Si hay objetos de Corazon_1 de la versión anterior, forzar reconstrucción
                        if (livesContainer.transform.Find("Corazon_1") != null)
                        {
                            needsFix = true;
                        }
                        else
                        {
                            // Verificar que tenga Hamburguesa_1
                            Transform firstLife = livesContainer.transform.Find("Hamburguesa_1");
                            if (firstLife == null)
                            {
                                needsFix = true;
                            }
                            else
                            {
                                Image img = firstLife.GetComponent<Image>();
                                if (img == null || img.sprite == null || !img.sprite.name.ToLower().Contains("hamburguesa"))
                                {
                                    needsFix = true;
                                }
                            }
                        }
                    }
                }
            }

            if (!needsFix)
            {
                GameObject goPanelObj = GameObject.Find("GameOverPanel");
                if (goPanelObj == null)
                {
                    needsFix = true;
                }
                else
                {
                    Image goImg = goPanelObj.GetComponent<Image>();
                    if (goImg == null)
                    {
                        needsFix = true;
                    }
                    else
                    {
                        // Si existe portada_perdiste.png, goImg.sprite no debería ser null
                        string losePath = "Assets/sprites/portada_perdiste.png";
                        if (System.IO.File.Exists(losePath) && goImg.sprite == null)
                        {
                            needsFix = true;
                        }
                    }

                    // Asegurar que tenga un Button
                    if (goPanelObj.GetComponent<Button>() == null)
                    {
                        needsFix = true;
                    }
                }
            }

            if (!needsFix)
            {
                GameObject canvasObj = GameObject.Find("_UI_Canvas");
                if (canvasObj != null)
                {
                    AdministradorUI uiManagerObj = canvasObj.GetComponent<AdministradorUI>();
                    if (uiManagerObj != null)
                    {
                        var loseSpriteField = typeof(AdministradorUI).GetField("loseSprite", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        if (loseSpriteField != null)
                        {
                            var spriteVal = loseSpriteField.GetValue(uiManagerObj) as Sprite;
                            string losePath = "Assets/sprites/portada_perdiste.png";
                            if (System.IO.File.Exists(losePath) && spriteVal == null)
                            {
                                needsFix = true;
                            }
                        }
                    }
                }
            }

            if (!needsFix)
            {
                GeneradorObstaculos spawnerObj = GameObject.FindFirstObjectByType<GeneradorObstaculos>();
                if (spawnerObj == null)
                {
                    needsFix = true;
                }
                else
                {
                    var carSpritesField = typeof(GeneradorObstaculos).GetField("carSprites", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (carSpritesField != null)
                    {
                        var spritesValue = carSpritesField.GetValue(spawnerObj) as Sprite[];
                        if (spritesValue == null || spritesValue.Length == 0)
                        {
                            needsFix = true;
                        }
                    }

                    var spawnerLanesField = typeof(GeneradorObstaculos).GetField("lanePositionsX", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (spawnerLanesField != null)
                    {
                        var lanesVal = spawnerLanesField.GetValue(spawnerObj) as float[];
                        if (lanesVal == null || lanesVal.Length < 3 || 
                            Mathf.Abs(lanesVal[0] - (-3.60f)) > 0.01f || 
                            Mathf.Abs(lanesVal[1] - (-0.12f)) > 0.01f || 
                            Mathf.Abs(lanesVal[2] - 3.51f) > 0.01f)
                        {
                            needsFix = true;
                        }
                    }
                }
            }

            if (!needsFix)
            {
                GameObject riderObj = GameObject.Find("imagen_repartidor_0 (1)");
                if (riderObj == null)
                {
                    GameObject[] allObjs = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
                    foreach (var go in allObjs)
                    {
                        if (go.name.ToLower().Contains("repartidor") || go.name.ToLower().Contains("player"))
                        {
                            riderObj = go;
                            break;
                        }
                    }
                }
                if (riderObj != null && (riderObj.transform.localScale.x > 0.4f || riderObj.transform.localScale.x < 0.35f))
                {
                    needsFix = true;
                }
            }

            if (!needsFix)
            {
                GameObject rider = GameObject.Find("imagen_repartidor_0 (1)");
                if (rider != null)
                {
                    ControladorJugador pc = rider.GetComponent<ControladorJugador>();
                    if (pc != null)
                    {
                        var pcLanesField = typeof(ControladorJugador).GetField("lanePositionsX", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        if (pcLanesField != null)
                        {
                            var lanesVal = pcLanesField.GetValue(pc) as float[];
                            if (lanesVal == null || lanesVal.Length < 3 || 
                                Mathf.Abs(lanesVal[0] - (-3.60f)) > 0.01f || 
                                Mathf.Abs(lanesVal[1] - (-0.12f)) > 0.01f || 
                                Mathf.Abs(lanesVal[2] - 3.51f) > 0.01f)
                            {
                                needsFix = true;
                            }
                        }
                    }
                }
            }

            if (needsFix)
            {
                isCheckingScene = true;
                try
                {
                    Debug.Log("🛠️ [Auto-Self-Heal] Se detectó un estado incorrecto o desordenado en los corazones. Corrigiendo y guardando escena automáticamente...");
                    SetupNewStreetAndSidewalkInternal(true);
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning("Error en auto-reconstrucción: " + ex.Message);
                }
                finally
                {
                    isCheckingScene = false;
                }
            }
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                // Cuando volvemos a Edit Mode, realizamos un chequeo de autocuración por si acaso
                AutoCheckAndFixScene();
            }
        }

        // Removido para no mostrar más el menú "Tools" en la barra de Unity
        // [MenuItem("Tools/Delivery Express/Rebuild Scene UI")]
        // public static void SetupNewStreetAndSidewalk()
        // {
        //     SetupNewStreetAndSidewalkInternal(false);
        // }

        public static void SetupNewStreetAndSidewalkInternal(bool silent)
        {
            RegisterRequiredTags();
            Debug.Log("🛣️ Configurando nueva calle y vereda desde calleyvereda.png...");

            // 1. Cargar la imagen completa sin cortar (soporta vereda_calle.png o calleyvereda.png)
            string spritePath = "Assets/sprites/vereda_calle.png";
            if (!System.IO.File.Exists(spritePath))
            {
                spritePath = "Assets/sprites/calleyvereda.png";
            }

            EnsureIsSprite(spritePath);
            AssetDatabase.Refresh();

            Sprite streetSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            if (streetSprite == null)
            {
                if (!silent)
                {
                    EditorUtility.DisplayDialog("Error", "No se pudo cargar 'vereda_calle.png' ni 'calleyvereda.png' en Assets/sprites/. Asegúrate de que exista.", "Aceptar");
                }
                else
                {
                    Debug.LogError("Error: No se pudo cargar 'vereda_calle.png' ni 'calleyvereda.png' en Assets/sprites/.");
                }
                return;
            }

            // 2. Limpiar todos los fondos anteriores y duplicados para evitar superposiciones
            CapaParallax[] oldLayers = GameObject.FindObjectsByType<CapaParallax>(FindObjectsSortMode.None);
            foreach (CapaParallax layer in oldLayers)
            {
                if (layer != null && layer.gameObject != null)
                {
                    UnityEngine.Object.DestroyImmediate(layer.gameObject);
                }
            }

            GameObject[] allObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            foreach (GameObject go in allObjects)
            {
                if (go != null && (go.name.Contains("ScrollingBackground") || 
                                   go.name.Contains("ParallaxBackground") || 
                                   go.name.Contains("Capa_Calle") || 
                                   go.name.Contains("Capa_Edificios") ||
                                   go.name.Contains("RoadBackground") ||
                                   go.name.Contains("Sprite_1") ||
                                   go.name.Contains("Sprite_2") ||
                                   go.name.StartsWith("Corazon_")))
                {
                    UnityEngine.Object.DestroyImmediate(go);
                }
            }

            // 3. Crear el objeto de fondo con el script de scroll infinito
            GameObject scrollingBackground = new GameObject("_ScrollingBackground");
            CapaParallax scrollScript = scrollingBackground.AddComponent<CapaParallax>();

            // Aplicamos un factor de escala mínimo de 1.05x para tapar los bordes negros
            // en los laterales y centrar la calle asimétrica perfectamente
            Vector3 finalScale = new Vector3(1.05f, 1.05f, 1f);
            // Desplazar X a 0.44f para centrar la calle debido a la asimetría del lienzo de vereda_calle.png
            scrollScript.Setup(streetSprite, 1.0f, -10, new Vector3(0.44f, 0f, 0f), finalScale);

            // 4. Buscar y configurar al repartidor en la escena
            GameObject riderObj = GameObject.Find("imagen_repartidor_0 (1)");
            if (riderObj == null)
            {
                GameObject[] allObjs = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
                foreach (var go in allObjs)
                {
                    if (go.name.ToLower().Contains("repartidor") || go.name.ToLower().Contains("player"))
                    {
                        riderObj = go;
                        break;
                    }
                }
            }

            if (riderObj != null)
            {
                riderObj.tag = "Player";
                
                // Asegurar que se dibuje por encima del fondo (Sorting Order 10)
                SpriteRenderer sr = riderObj.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.sortingOrder = 10;
                }

                // Posicionar al chico en la parte inferior central
                riderObj.transform.position = new Vector3(0, -3.5f, 0);

                // Escalar al repartidor para que sea más chico (más realista en relación a los autos)
                riderObj.transform.localScale = new Vector3(0.38f, 0.38f, 1f);

                // Asegurar controlador del jugador
                ControladorJugador pc = riderObj.GetComponent<ControladorJugador>();
                if (pc == null)
                {
                    pc = riderObj.AddComponent<ControladorJugador>();
                }

                // Configurar carriles para vereda_calle.png a PPU 181 con escala 1.05x y desplazamiento
                var laneField = typeof(ControladorJugador).GetField("lanePositionsX", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (laneField != null)
                {
                    laneField.SetValue(pc, new float[] { -3.60f, -0.12f, 3.51f });
                    Debug.Log("✅ Carriles de movimiento ajustados a: {-3.60, -0.12, 3.51}");
                }

                var limitField = typeof(ControladorJugador).GetField("screenLimitX", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (limitField != null)
                {
                    limitField.SetValue(pc, 4.5f); // Límite exacto ajustado para dar margen al carril derecho
                    Debug.Log("✅ Límite lateral de la calle ajustado a: 4.5");
                }
                EditorUtility.SetDirty(pc);

                // Asegurar físicas rígidas kinematic
                Rigidbody2D rb = riderObj.GetComponent<Rigidbody2D>();
                if (rb == null)
                {
                    rb = riderObj.AddComponent<Rigidbody2D>();
                }
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.gravityScale = 0f;
                rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

                // Asegurar colisionador del jugador
                BoxCollider2D col = riderObj.GetComponent<BoxCollider2D>();
                if (col == null)
                {
                    col = riderObj.AddComponent<BoxCollider2D>();
                    col.size = new Vector2(1f, 1.6f);
                    col.isTrigger = false;
                }

                // Configurar animaciones del jugador
                ConfigureAnimatorController(riderObj);
            }

            // 4.1 Configurar GeneradorObstaculos (Creación y sprites de autos)
            GeneradorObstaculos spawner = GameObject.FindFirstObjectByType<GeneradorObstaculos>();
            if (spawner == null)
            {
                GameObject spawnerObj = new GameObject("GeneradorObstaculos");
                spawnerObj.transform.position = new Vector3(0f, 8f, 0f);
                spawner = spawnerObj.AddComponent<GeneradorObstaculos>();
                Debug.Log("✅ GeneradorObstaculos creado automáticamente.");
            }

            if (spawner != null)
            {
                // Inyectar carriles {-2.9f, 0f, 2.9f}
                var spawnerLaneField = typeof(GeneradorObstaculos).GetField("lanePositionsX", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (spawnerLaneField != null)
                {
                    spawnerLaneField.SetValue(spawner, new float[] { -3.60f, -0.12f, 3.51f });
                    Debug.Log("✅ Carriles de GeneradorObstaculos ajustados a: {-3.60, -0.12, 3.51}");
                }
                EditorUtility.SetDirty(spawner);

                // Cargar y asignar sprites de autos desde imagen_auto.png
                string carSpritePath = "Assets/sprites/imagen_auto.png";
                Sprite[] carSprites = null;
                try
                {
                    if (System.IO.File.Exists(carSpritePath))
                    {
                        var subAssets = AssetDatabase.LoadAllAssetsAtPath(carSpritePath);
                        if (subAssets != null)
                        {
                            carSprites = subAssets.OfType<Sprite>().OrderBy(s => s.name).ToArray();
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning("Error al cargar sprites de autos: " + ex.Message);
                }

                var carSpritesField = typeof(GeneradorObstaculos).GetField("carSprites", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (carSpritesField != null && carSprites != null)
                {
                    carSpritesField.SetValue(spawner, carSprites);
                    EditorUtility.SetDirty(spawner);
                    Debug.Log($"✅ Asignados {carSprites.Length} sprites de autos al GeneradorObstaculos.");
                }
            }

            // 5. Mantener la cámara estrictamente a tamaño 6.0 (SI O SI para 1920x1080)
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                mainCam.transform.position = new Vector3(0, 0, -10);
                mainCam.orthographic = true;
                mainCam.orthographicSize = 6.0f; // SÍ O SÍ 6.0, tal como pediste
                mainCam.backgroundColor = Color.black;
                mainCam.clearFlags = CameraClearFlags.SolidColor;
            }

            // 6. Asegurar AdministradorJuego en la escena para controlar el sistema de vidas
            AdministradorJuego gameManager = GameObject.FindFirstObjectByType<AdministradorJuego>();
            if (gameManager == null)
            {
                GameObject managerObj = new GameObject("_GameManager");
                gameManager = managerObj.AddComponent<AdministradorJuego>();
                Debug.Log("✅ Se creó el objeto '_GameManager' con el script central.");
            }

            // 7. Crear el Canvas UI con el AdministradorUI y las 3 vidas visuales
            Canvas canvas = GameObject.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("_UI_Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
                
                // Asegurar EventSystem
                if (GameObject.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
                {
                    GameObject eventSystemObj = new GameObject("EventSystem");
                    eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
                    eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                }
            }

            // 7.1 Limpiar contenedor de vidas anterior si existía para evitar duplicaciones
            Transform oldContainer = canvas.transform.Find("Contenedor_Vidas");
            if (oldContainer != null)
            {
                UnityEngine.Object.DestroyImmediate(oldContainer.gameObject);
            }

            // Limpiar texto de vidas anterior si existía
            Transform oldText = canvas.transform.Find("Texto_Vidas");
            if (oldText != null)
            {
                UnityEngine.Object.DestroyImmediate(oldText.gameObject);
            }

            // Limpiar panel de GameOver anterior si existía
            Transform oldGameOver = canvas.transform.Find("GameOverPanel");
            if (oldGameOver != null)
            {
                UnityEngine.Object.DestroyImmediate(oldGameOver.gameObject);
            }

            // 7.2 Crear nuevo contenedor de corazones en la esquina superior izquierda (más grande)
            GameObject livesContainerObj = new GameObject("Contenedor_Vidas", typeof(RectTransform));
            RectTransform rect = livesContainerObj.GetComponent<RectTransform>();
            rect.SetParent(canvas.transform, false);
            rect.anchorMin = new Vector2(0f, 1f); // Esquina superior izquierda
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(35f, -35f); // Margen ajustado
            rect.sizeDelta = new Vector2(400f, 75f); // Más grande

            // Añadir layout horizontal
            HorizontalLayoutGroup layout = livesContainerObj.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 18f; // Spacing aumentado
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            // Cargar sprites de hamburguesas
            string hamburgerSpritePath = "Assets/sprites/hamburguesa_ui.png";
            Sprite[] hamburgerSprites = null;
            bool fileExists = System.IO.File.Exists(hamburgerSpritePath);
            Debug.Log($"[HAMBURGERS DEBUG] File exists at {hamburgerSpritePath}: {fileExists}");
            try
            {
                if (fileExists)
                {
                    var subAssets = AssetDatabase.LoadAllAssetsAtPath(hamburgerSpritePath);
                    if (subAssets != null)
                    {
                        Debug.Log($"[HAMBURGERS DEBUG] Total sub-assets found: {subAssets.Length}");
                        hamburgerSprites = subAssets.OfType<Sprite>().OrderBy(s => s.name).ToArray();
                        Debug.Log($"[HAMBURGERS DEBUG] Total sprites found: {hamburgerSprites.Length}");
                    }
                    else
                    {
                        Debug.LogWarning("[HAMBURGERS DEBUG] LoadAllAssetsAtPath returned null");
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError("[HAMBURGERS DEBUG] Error al cargar sprites de hamburguesas: " + ex.Message);
            }

            // Generar 3 hamburguesas como indicadores de vidas
            Image[] hearts = new Image[3];
            for (int i = 0; i < 3; i++)
            {
                GameObject heartObj = new GameObject($"Hamburguesa_{i + 1}", typeof(RectTransform));
                RectTransform hRect = heartObj.GetComponent<RectTransform>();
                hRect.SetParent(rect, false);
                // Ajustar el tamaño a 75x55 para mantener la relación de aspecto de la hamburguesa
                hRect.sizeDelta = new Vector2(75f, 55f); 

                Image img = heartObj.AddComponent<Image>();
                img.preserveAspect = true; // Evitar distorsión del sprite de la hamburguesa
                
                if (hamburgerSprites != null && hamburgerSprites.Length > i)
                {
                    img.sprite = hamburgerSprites[i];
                    img.color = Color.white;
                    Debug.Log($"[HAMBURGERS DEBUG] Assigned sprite {hamburgerSprites[i].name} to Hamburguesa_{i + 1}");
                }
                else if (hamburgerSprites != null && hamburgerSprites.Length > 0)
                {
                    img.sprite = hamburgerSprites[0];
                    img.color = Color.white;
                    Debug.Log($"[HAMBURGERS DEBUG] Fallback assigned sprite {hamburgerSprites[0].name} to Hamburguesa_{i + 1}");
                }
                else
                {
                    img.color = Color.yellow; // Fallback amarillo/hamburguesa
                    Debug.LogWarning($"[HAMBURGERS DEBUG] No sprites available, using yellow color fallback for Hamburguesa_{i + 1}");
                }

                hearts[i] = img;
            }

            // 7.4 Crear el panel de fin de partida (GameOverPanel)
            GameObject panelObj = new GameObject("GameOverPanel", typeof(RectTransform));
            RectTransform pRect = panelObj.GetComponent<RectTransform>();
            pRect.SetParent(canvas.transform, false);
            panelObj.SetActive(false); // Oculto al inicio

            pRect.anchorMin = Vector2.zero;
            pRect.anchorMax = Vector2.one;
            pRect.sizeDelta = Vector2.zero; // Cubrir toda la pantalla

            Image panelImage = panelObj.AddComponent<Image>();

            // Cargar sprite de portada_perdiste
            string loseSpritePath = "Assets/sprites/portada_perdiste.png";
            Sprite loseSprite = null;
            try
            {
                if (System.IO.File.Exists(loseSpritePath))
                {
                    var subAssets = AssetDatabase.LoadAllAssetsAtPath(loseSpritePath);
                    if (subAssets != null)
                    {
                        loseSprite = subAssets.OfType<Sprite>().FirstOrDefault();
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("Error al cargar sprite de derrota: " + ex.Message);
            }

            if (loseSprite != null)
            {
                panelImage.sprite = loseSprite;
                panelImage.color = Color.white;
                Debug.Log("✅ Pantalla de derrota asignada exitosamente.");
            }
            else
            {
                panelImage.color = new Color(0f, 0f, 0f, 0.85f); // Fallback fondo oscuro
                Debug.LogWarning("⚠️ No se encontró 'portada_perdiste.png' para el fondo de derrota, usando fondo oscuro.");
            }

            // Añadir botón para interactuar con el click del mouse
            panelObj.AddComponent<Button>();

            // Crear texto de Game Over (solo se muestra como fallback si no hay sprite)
            GameObject goTextObj = new GameObject("GameOverText", typeof(RectTransform));
            RectTransform goTRect = goTextObj.GetComponent<RectTransform>();
            goTRect.SetParent(pRect, false);
            
            Text goText = goTextObj.AddComponent<Text>();
            goText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            goText.fontSize = 44;
            goText.color = Color.red;
            goText.alignment = TextAnchor.MiddleCenter;

            if (loseSprite != null)
            {
                goText.text = ""; // El sprite ya contiene el arte de Game Over
            }
            else
            {
                goText.text = "¡JUEGO TERMINADO!\n\nPresiona 'R' para reiniciar";
            }

            goTRect.sizeDelta = new Vector2(600f, 300f);
            goTRect.anchorMin = new Vector2(0.5f, 0.5f);
            goTRect.anchorMax = new Vector2(0.5f, 0.5f);
            goTRect.pivot = new Vector2(0.5f, 0.5f);
            goTRect.anchoredPosition = Vector2.zero;

            // 7.5 Configurar el AdministradorUI
            AdministradorUI uiManager = canvas.GetComponent<AdministradorUI>();
            if (uiManager == null)
            {
                uiManager = canvas.gameObject.AddComponent<AdministradorUI>();
            }

            // Asignar corazones por reflexión
            var heartField = typeof(AdministradorUI).GetField("heartImages", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (heartField != null)
            {
                heartField.SetValue(uiManager, hearts);
                EditorUtility.SetDirty(uiManager); // Marcar AdministradorUI como dirty para que Unity guarde los cambios en la escena
                Debug.Log("[HEARTS DEBUG] Assigned heartImages array and marked AdministradorUI dirty.");
            }

            // Asignar texto por reflexión (null ya que eliminamos la frase de vidas)
            var textField = typeof(AdministradorUI).GetField("livesText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (textField != null)
            {
                textField.SetValue(uiManager, null);
                EditorUtility.SetDirty(uiManager);
            }

            // Asignar panel por reflexión
            var panelField = typeof(AdministradorUI).GetField("gameOverPanel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (panelField != null)
            {
                panelField.SetValue(uiManager, panelObj);
                EditorUtility.SetDirty(uiManager);
            }

            // Asignar sprite de derrota por reflexión
            var loseSpriteField = typeof(AdministradorUI).GetField("loseSprite", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (loseSpriteField != null && loseSprite != null)
            {
                loseSpriteField.SetValue(uiManager, loseSprite);
                EditorUtility.SetDirty(uiManager);
                Debug.Log("✅ Sprite de derrota inyectado en AdministradorUI.");
            }

            Debug.Log("✅ AdministradorUI configurado con corazones, HUD de texto y panel de GameOver.");

            // Asegurar que el Canvas y todos sus hijos estén en la capa "UI" (Capa 5)
            // para que Unity los renderice correctamente en la cámara
            int uiLayer = LayerMask.NameToLayer("UI");
            if (uiLayer >= 0)
            {
                SetLayerRecursively(canvas.gameObject, uiLayer);
            }

            // Forzar que el Canvas esté activo
            canvas.gameObject.SetActive(true);

            // 8. Marcar la escena como sucia y guardar de forma síncrona si no estamos jugando
            try
            {
                if (!EditorApplication.isPlaying)
                {
                    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                    EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("No se pudo guardar la escena en disco (modo juego o transición): " + ex.Message);
            }

            Debug.Log("✅ Loop de calleyvereda.png configurado sin recortes.");
            if (!silent)
            {
                EditorUtility.DisplayDialog("¡Configuración Exitosa!",
                    "Se ha cargado 'calleyvereda.png' en loop continuo sin separar la foto:\n\n" +
                    "1. Se configuró la imagen completa en escala 0.6x para adaptarse a tu cámara de tamaño 6.0.\n" +
                    "2. Se ajustaron los carriles a X: -2.9f, 0f, 2.9f (rodando cerca de los cordones y lejos de las líneas blancas).\n" +
                    "3. Se bloqueó al repartidor para que no pueda salirse de la calle (límite en X: 4.0).\n" +
                    "4. Se eliminaron costuras y se forzó el guardado síncrono.\n\n" +
                    "¡Dale a PLAY (▶️) para probar el loop continuo!", "¡Excelente, a jugar!");
            }
            else
            {
                Debug.Log("🛣️ Calle y vereda auto-configuradas silenciosamente para PLAY.");
            }
        }

        public static void CleanSceneKeepOnlyRider()
        {
            Debug.Log("🧹 Iniciando limpieza de la escena...");

            GameObject riderObj = GameObject.Find("imagen_repartidor_0 (1)");
            if (riderObj == null)
            {
                GameObject[] allObjs = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
                foreach (var go in allObjs)
                {
                    if (go.name.ToLower().Contains("repartidor") || go.name.ToLower().Contains("player"))
                    {
                        riderObj = go;
                        break;
                    }
                }
            }

            if (riderObj == null)
            {
                EditorUtility.DisplayDialog("Error", "No se encontró el objeto del repartidor en la escena.", "Aceptar");
                return;
            }

            riderObj.tag = "Player";
            ControladorJugador playerController = riderObj.GetComponent<ControladorJugador>();
            if (playerController == null)
            {
                playerController = riderObj.AddComponent<ControladorJugador>();
            }

            Rigidbody2D rb = riderObj.GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                rb = riderObj.AddComponent<Rigidbody2D>();
            }
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            BoxCollider2D col = riderObj.GetComponent<BoxCollider2D>();
            if (col == null)
            {
                col = riderObj.AddComponent<BoxCollider2D>();
                col.size = new Vector2(1f, 1.6f);
                col.isTrigger = false;
            }

            riderObj.transform.position = new Vector3(0, -3.5f, 0);

            GameObject[] allGameObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            int deletedCount = 0;
            foreach (GameObject go in allGameObjects)
            {
                if (go != riderObj && 
                    go.name != "Main Camera" && 
                    go.name != "Global Light 2D" && 
                    go.transform.parent == null)
                {
                    string lowerName = go.name.ToLower();
                    if (lowerName.Contains("directionallight") || lowerName.Contains("camera"))
                    {
                        continue;
                    }

                    UnityEngine.Object.DestroyImmediate(go);
                    deletedCount++;
                }
            }

            GameObject[] remainingObjs = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            foreach (GameObject go in remainingObjs)
            {
                if (go != riderObj && go.name != "Main Camera" && go.name != "Global Light 2D")
                {
                    string name = go.name;
                    if (name == "RoadBackground" || name == "_SceneBuilder" || name == "GeneradorObstaculos" || 
                        name.StartsWith("Cliente_NPC_") || name == "ControladorJugador" || name == "_GameManager" || name == "_ParallaxBackground" || name == "_ScrollingBackground")
                    {
                        UnityEngine.Object.DestroyImmediate(go);
                        deletedCount++;
                    }
                }
            }

            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                mainCam.transform.position = new Vector3(0, 0, -10);
                mainCam.orthographic = true;
                mainCam.orthographicSize = 6.0f;
                mainCam.backgroundColor = new Color(0.18f, 0.17f, 0.23f);
                mainCam.clearFlags = CameraClearFlags.SolidColor;
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());

            Debug.Log($"✅ Escena limpia. Se eliminaron {deletedCount} objetos duplicados del nivel.");
            EditorUtility.DisplayDialog("¡Escena Limpia!", 
                "Se ha limpiado la escena correctamente.", "Aceptar");
        }

        private static void EnsureIsSprite(string path)
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                bool changed = false;
                if (importer.textureType != TextureImporterType.Sprite)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    changed = true;
                }
                
                // Forzar Pixels Per Unit a 181 para que la zona activa de vereda_calle.png (3860px)
                // calce al milímetro en la cámara de tamaño 6.0 sin bordes negros y con veredas visibles.
                if (importer.spritePixelsPerUnit != 181f)
                {
                    importer.spritePixelsPerUnit = 181f;
                    changed = true;
                }

                if (changed)
                {
                    importer.SaveAndReimport();
                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                }
            }
            else
            {
                Debug.LogWarning("No se pudo obtener el importador para: " + path);
            }
        }

        private static void SetLayerRecursively(GameObject go, int layer)
        {
            if (go == null) return;
            go.layer = layer;
            foreach (Transform child in go.transform)
            {
                if (child != null && child.gameObject != null)
                {
                    SetLayerRecursively(child.gameObject, layer);
                }
            }
        }

        private static void ConfigureAnimatorController(GameObject riderObj)
        {
            if (riderObj == null) return;
            
            Animator animator = riderObj.GetComponent<Animator>();
            if (animator == null)
            {
                animator = riderObj.AddComponent<Animator>();
            }

            string spritePath = "Assets/sprites/imagen_repartidor.png";
            var assets = AssetDatabase.LoadAllAssetsAtPath(spritePath);
            System.Collections.Generic.List<Sprite> spritesList = new System.Collections.Generic.List<Sprite>();
            foreach (var asset in assets)
            {
                if (asset is Sprite sprite)
                {
                    spritesList.Add(sprite);
                }
            }
            
            if (spritesList.Count < 12)
            {
                Debug.LogWarning("No se encontraron suficientes sprites en imagen_repartidor.png para configurar las animaciones.");
                return;
            }

            spritesList.Sort((a, b) => {
                int aNum = GetSpriteNumber(a.name);
                int bNum = GetSpriteNumber(b.name);
                return aNum.CompareTo(bNum);
            });

            Sprite[] pedaleandoSprites = new Sprite[8];
            for (int i = 0; i < 8; i++) pedaleandoSprites[i] = spritesList[i];

            Sprite[] tambaleoSprites = new Sprite[4];
            for (int i = 0; i < 4; i++) tambaleoSprites[i] = spritesList[8 + i];

            Sprite[] choqueSprites = new Sprite[1];
            choqueSprites[0] = spritesList[8]; // fotograma estático de choque/caída

            AnimationClip pedaleandoClip = CreateOrReplaceClip("Assets/sprites/New Animation.anim", pedaleandoSprites, 12f, true);
            AnimationClip tambaleoClip = CreateOrReplaceClip("Assets/sprites/Tambaleo.anim", tambaleoSprites, 8f, true);
            AnimationClip choqueClip = CreateOrReplaceClip("Assets/sprites/Choque.anim", choqueSprites, 1f, false);

            string controllerPath = "Assets/sprites/imagen_repartidor_0 (1).controller";
            var controller = AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>(controllerPath);
            if (controller == null)
            {
                controller = UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            }

            // Asegurar parámetros
            bool hasStateParam = false;
            foreach (var parameter in controller.parameters)
            {
                if (parameter.name == "State") hasStateParam = true;
            }
            if (!hasStateParam) controller.AddParameter("State", AnimatorControllerParameterType.Int);

            var rootStateMachine = controller.layers[0].stateMachine;

            // Limpiar estados anteriores
            while (rootStateMachine.states.Length > 0)
            {
                rootStateMachine.RemoveState(rootStateMachine.states[0].state);
            }

            var stateIdle = rootStateMachine.AddState("Idle");
            stateIdle.motion = pedaleandoClip;

            var statePedaleando = rootStateMachine.AddState("Pedaleando");
            statePedaleando.motion = pedaleandoClip;

            var stateInestable = rootStateMachine.AddState("Inestable");
            stateInestable.motion = tambaleoClip;

            var stateChoque = rootStateMachine.AddState("Choque");
            stateChoque.motion = choqueClip;

            rootStateMachine.defaultState = statePedaleando;

            // Agregar transiciones
            var tToIdle = rootStateMachine.AddAnyStateTransition(stateIdle);
            tToIdle.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 0, "State");
            tToIdle.duration = 0.05f;
            tToIdle.canTransitionToSelf = false;

            var tToPedaleando = rootStateMachine.AddAnyStateTransition(statePedaleando);
            tToPedaleando.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 1, "State");
            tToPedaleando.duration = 0.05f;
            tToPedaleando.canTransitionToSelf = false;

            var tToInestable = rootStateMachine.AddAnyStateTransition(stateInestable);
            tToInestable.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 2, "State");
            tToInestable.duration = 0.05f;
            tToInestable.canTransitionToSelf = false;

            var tToChoque = rootStateMachine.AddAnyStateTransition(stateChoque);
            tToChoque.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 3, "State");
            tToChoque.duration = 0.05f;
            tToChoque.canTransitionToSelf = false;

            animator.runtimeAnimatorController = controller;
            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();

            Debug.Log("✅ Animator Controller y Clips de Animación reconstruidos con éxito.");
        }

        private static int GetSpriteNumber(string name)
        {
            string numberPart = name.Substring(name.LastIndexOf('_') + 1);
            int num;
            if (int.TryParse(numberPart, out num)) return num;
            return 0;
        }

        private static AnimationClip CreateOrReplaceClip(string path, Sprite[] sprites, float frameRate, bool loop)
        {
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip == null)
            {
                clip = new AnimationClip();
                AssetDatabase.CreateAsset(clip, path);
            }
            else
            {
                clip.ClearCurves();
            }

            clip.frameRate = frameRate;
            
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = loop;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            EditorCurveBinding binding = new EditorCurveBinding();
            binding.type = typeof(SpriteRenderer);
            binding.path = "";
            binding.propertyName = "m_Sprite";

            ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[sprites.Length];
            for (int i = 0; i < sprites.Length; i++)
            {
                keyframes[i] = new ObjectReferenceKeyframe();
                keyframes[i].time = i / frameRate;
                keyframes[i].value = sprites[i];
            }

            AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);
            EditorUtility.SetDirty(clip);
            return clip;
        }

        private static void RegisterRequiredTags()
        {
            try
            {
                var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
                if (assets != null && assets.Length > 0)
                {
                    SerializedObject tagManager = new SerializedObject(assets[0]);
                    SerializedProperty tagsProp = tagManager.FindProperty("tags");
                    if (tagsProp != null)
                    {
                        string[] requiredTags = new string[] { "Obstaculo", "Car" };
                        bool changed = false;
                        foreach (string requiredTag in requiredTags)
                        {
                            bool found = false;
                            for (int i = 0; i < tagsProp.arraySize; i++)
                            {
                                if (tagsProp.GetArrayElementAtIndex(i).stringValue == requiredTag)
                                {
                                    found = true;
                                    break;
                                }
                            }
                            if (!found)
                            {
                                tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
                                tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = requiredTag;
                                changed = true;
                                Debug.Log($"➕ Registrada etiqueta requerida: {requiredTag}");
                            }
                        }
                        if (changed)
                        {
                            tagManager.ApplyModifiedProperties();
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("No se pudieron registrar las etiquetas automáticamente: " + ex.Message);
            }
        }
    }
}
