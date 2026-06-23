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

            // Configura la escena automáticamente en Edit Mode al compilar o iniciar
            EditorApplication.delayCall += () =>
            {
                AutoCheckAndFixScene();
            };
        }

        private static bool isCheckingScene = false;
        private static void AutoCheckAndFixScene()
        {
            if (EditorApplication.isPlaying || EditorApplication.isCompiling || isCheckingScene) return;

            // Verifica si hay hamburguesas huérfanas en la raíz de la escena o si la jerarquía es inválida
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
                // Evita excepciones si la escena no está lista
            }

            if (!needsFix)
            {
                // Reconstruye la escena si todavía existe el texto de vidas anterior
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
                        // Si quedan corazones de la versión anterior, fuerza la reconstrucción
                        if (livesContainer.transform.Find("Corazon_1") != null)
                        {
                            needsFix = true;
                        }
                        else
                        {
                            Transform firstLife = livesContainer.transform.Find("Hamburguesa_1");
                            if (firstLife == null)
                            {
                                needsFix = true;
                            }
                            else
                            {
                                // Si no es una instancia de prefab, forzar reconstrucción
                                if (!PrefabUtility.IsPartOfPrefabInstance(firstLife.gameObject))
                                {
                                    needsFix = true;
                                }
                                // Es instancia de prefab válida - no reconstruir
                                // Nota: img.sprite puede ser null en Edit mode en componentes stripped,
                                // el sprite se hereda del prefab en runtime correctamente.
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
                    
                    if (goPanelObj.transform.Find("BotonIntentoNuevo") == null ||
                        goPanelObj.transform.Find("BotonMenu") == null)
                    {
                        needsFix = true;
                    }
                }
            }

            if (!needsFix)
            {
                GameObject startPanelObj = GameObject.Find("StartPanel");
                if (startPanelObj == null)
                {
                    needsFix = true;
                }
                else
                {
                    Image startImg = startPanelObj.GetComponent<Image>();
                    if (startImg == null)
                    {
                        needsFix = true;
                    }
                    else if (startPanelObj.transform.Find("BotonJugar") == null ||
                             startPanelObj.transform.Find("BotonMapa") == null ||
                             startPanelObj.transform.Find("BotonConfiguracion") == null)
                    {
                        needsFix = true;
                    }
                }
            }

            if (!needsFix)
            {
                GameObject victoryPanelObj = GameObject.Find("VictoryPanel");
                if (victoryPanelObj == null)
                {
                    needsFix = true;
                }
                else
                {
                    Image vicImg = victoryPanelObj.GetComponent<Image>();
                    if (vicImg == null)
                    {
                        needsFix = true;
                    }
                    else if (victoryPanelObj.transform.Find("BotonSiguiente") == null ||
                             victoryPanelObj.transform.Find("BotonMenu") == null)
                    {
                        needsFix = true;
                    }
                }
            }

            if (!needsFix)
            {
                GameObject canvasObj = GameObject.Find("_Lienzo_UI") ?? GameObject.Find("_UI_Canvas");
                if (canvasObj != null)
                {
                    Transform tMonedas = canvasObj.transform.Find("Texto_Monedas");
                    if (tMonedas == null)
                    {
                        needsFix = true;
                    }
                }
                else
                {
                    needsFix = true;
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

                    // Verificar que los prefabs requeridos estén asignados en el spawner
                    var monField = typeof(GeneradorObstaculos).GetField("monedaPrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var hamField = typeof(GeneradorObstaculos).GetField("hamburguesaPowerUpPrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var potField = typeof(GeneradorObstaculos).GetField("potenciadorEnergiaPrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (monField != null && monField.GetValue(spawnerObj) == null) needsFix = true;
                    if (hamField != null && hamField.GetValue(spawnerObj) == null) needsFix = true;
                    if (potField != null && potField.GetValue(spawnerObj) == null) needsFix = true;
                }
            }

            if (!needsFix)
            {
                GameObject riderObj = FindRiderGameObject();
                if (riderObj != null && (riderObj.transform.localScale.x > 0.4f || riderObj.transform.localScale.x < 0.35f))
                {
                    needsFix = true;
                }
            }

            if (!needsFix)
            {
                GameObject rider = FindRiderGameObject();
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

            if (!needsFix)
            {
                CapaParallax cp = GameObject.FindFirstObjectByType<CapaParallax>();
                if (cp == null)
                {
                    needsFix = true;
                }
                else
                {
                    var crossField = typeof(CapaParallax).GetField("crossroadSprite", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (crossField != null && crossField.GetValue(cp) == null)
                    {
                        needsFix = true;
                    }
                }
            }

            // Renombra los objetos clave al español si aún tienen nombres heredados en inglés
            if (!needsFix)
            {
                // Sólo dispara fix si el canvas principal todavía tiene el nombre de inglés
                if (GameObject.Find("_UI_Canvas") != null || 
                    GameObject.Find("_GameManager") != null)
                {
                    needsFix = true;
                }
            }

            // Actualiza los Pixels Per Unit a 181 en las imágenes nuevas
            if (!needsFix)
            {
                TextureImporter importer = AssetImporter.GetAtPath("Assets/sprites/Calle_cruce.png") as TextureImporter;
                if (importer != null && importer.spritePixelsPerUnit != 181f)
                {
                    needsFix = true;
                }
            }

            if (needsFix)
            {
                isCheckingScene = true;
                try
                {
                    // Log de diagnóstico para identificar el trigger
                    bool dbgCanvas   = GameObject.Find("_UI_Canvas") != null;
                    bool dbgGM       = GameObject.Find("_GameManager") != null;
                    bool dbgSprites  = false;
                    var sp = GameObject.FindFirstObjectByType<GeneradorObstaculos>();
                    if (sp != null)
                    {
                        var f = typeof(GeneradorObstaculos).GetField("carSprites", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        var v = f?.GetValue(sp) as Sprite[];
                        dbgSprites = (v == null || v.Length == 0);
                    }
                    bool dbgLanes = false;
                    if (sp != null)
                    {
                        var lf = typeof(GeneradorObstaculos).GetField("lanePositionsX", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        var lv = lf?.GetValue(sp) as float[];
                        dbgLanes = (lv == null || lv.Length < 3);
                    }
                    bool dbgPrefab = !PrefabUtility.IsPartOfPrefabInstance(
                        GameObject.Find("Contenedor_Vidas")?.transform.Find("Hamburguesa_1")?.gameObject ?? new GameObject());
                    Debug.Log($"[DIAGNÓSTICO AUTO-HEAL] canvas={dbgCanvas} gm={dbgGM} carSprites={dbgSprites} lanes={dbgLanes} prefab={dbgPrefab}");
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
                // Al volver a Edit Mode, realiza un chequeo de autocuración
                AutoCheckAndFixScene();
            }
        }

        [MenuItem("Tools/Delivery Express/Rebuild Scene UI")]
        public static void SetupNewStreetAndSidewalk()
        {
            SetupNewStreetAndSidewalkInternal(true);
        }

        public static void SetupNewStreetAndSidewalkInternal(bool silent)
        {
            RegisterRequiredTags();
            Debug.Log("🛣️ Configurando nueva calle y vereda desde calleyvereda.png...");

            string spritePath = "Assets/sprites/calle_vereda.png";
            if (!System.IO.File.Exists(spritePath))
            {
                spritePath = "Assets/sprites/vereda_calle.png";
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

            // Limpia los fondos anteriores y duplicados para evitar superposiciones
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
                                   go.name.Contains("_FondoCalle") ||
                                   go.name.StartsWith("Corazon_")))
                {
                    UnityEngine.Object.DestroyImmediate(go);
                }
            }

            GameObject scrollingBackground = new GameObject("_FondoCalle");
            CapaParallax scrollScript = scrollingBackground.AddComponent<CapaParallax>();

            EnsureIsSprite("Assets/sprites/Calle_cruce.png");
            EnsureIsSprite("Assets/sprites/calle_final.png");
            Sprite crossroadSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/sprites/Calle_cruce.png");
            Sprite finalStreetSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/sprites/calle_final.png");
            
            scrollScript.SetupSequence(streetSprite, crossroadSprite, finalStreetSprite);

            // Aplica escala de 1.05x para evitar bordes negros
            Vector3 finalScale = new Vector3(1.05f, 1.05f, 1f);
            scrollScript.Setup(streetSprite, 1.0f, -10, new Vector3(0f, 0f, 0f), finalScale);

            // Cargar sprite de la casa final
            string finalHousePath = "Assets/sprites/casa_final.png";
            Sprite finalHouseSprite = null;
            try
            {
                if (System.IO.File.Exists(finalHousePath))
                {
                    var subAssets = AssetDatabase.LoadAllAssetsAtPath(finalHousePath);
                    if (subAssets != null)
                    {
                        finalHouseSprite = System.Linq.Enumerable.FirstOrDefault(System.Linq.Enumerable.OfType<Sprite>(subAssets), s => s.name == "casa_final_0");
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("Error al cargar sprite de casa final: " + ex.Message);
            }

            var finalHouseField = typeof(CapaParallax).GetField("finalHouseSprite", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (finalHouseField != null && finalHouseSprite != null)
            {
                finalHouseField.SetValue(scrollScript, finalHouseSprite);
                EditorUtility.SetDirty(scrollScript);
                Debug.Log("✅ Sprite de casa final asignado a CapaParallax.");
            }

            GameObject riderObj = FindRiderGameObject();

            if (riderObj != null)
            {
                riderObj.tag = "Player";
                
                SpriteRenderer sr = riderObj.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.sortingOrder = 10;
                }

                // Posiciona al repartidor en la parte inferior
                riderObj.transform.position = new Vector3(0, -3.5f, 0);

                // Escala al repartidor para que sea más chico
                riderObj.transform.localScale = new Vector3(0.38f, 0.38f, 1f);

                ControladorJugador pc = riderObj.GetComponent<ControladorJugador>();
                if (pc == null)
                {
                    pc = riderObj.AddComponent<ControladorJugador>();
                }

                // Configura los carriles de la calle
                var laneField = typeof(ControladorJugador).GetField("lanePositionsX", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (laneField != null)
                {
                    laneField.SetValue(pc, new float[] { -3.60f, -0.12f, 3.51f });
                    Debug.Log("✅ Carriles de movimiento ajustados a: {-3.60, -0.12, 3.51}");
                }

                var limitField = typeof(ControladorJugador).GetField("screenLimitX", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (limitField != null)
                {
                    limitField.SetValue(pc, 4.5f);
                    Debug.Log("✅ Límite lateral de la calle ajustado a: 4.5");
                }
                EditorUtility.SetDirty(pc);

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

                ConfigureAnimatorController(riderObj);
            }

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
                var spawnerLaneField = typeof(GeneradorObstaculos).GetField("lanePositionsX", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (spawnerLaneField != null)
                {
                    spawnerLaneField.SetValue(spawner, new float[] { -3.60f, -0.12f, 3.51f });
                    Debug.Log("✅ Carriles de GeneradorObstaculos ajustados a: {-3.60, -0.12, 3.51}");
                }
                EditorUtility.SetDirty(spawner);

                // Carga los sprites de casas desde imagenes_ casas.png (se siguen spawneando dinámicamente)
                string houseSpritePath = "Assets/sprites/imagenes_ casas.png";
                Sprite[] houseSprites = null;
                try
                {
                    if (System.IO.File.Exists(houseSpritePath))
                    {
                        var subAssets = AssetDatabase.LoadAllAssetsAtPath(houseSpritePath);
                        if (subAssets != null)
                        {
                            houseSprites = subAssets.OfType<Sprite>().OrderBy(s => GetSpriteNumber(s.name)).ToArray();
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning("Error al cargar sprites de casas: " + ex.Message);
                }

                var houseSpritesField = typeof(GeneradorObstaculos).GetField("houseSprites", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (houseSpritesField != null && houseSprites != null)
                {
                    houseSpritesField.SetValue(spawner, houseSprites);
                    Debug.Log($"✅ Asignados {houseSprites.Length} sprites de casas al GeneradorObstaculos.");
                }

                // Asegura la creación y configuración de los prefabs de obstáculos para que el usuario pueda configurarlos desde Unity
                EnsurePrefabsExist(spawner);
                EditorUtility.SetDirty(spawner);
            }

            // Configura la cámara con tamaño ortográfico de 6.0
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                mainCam.gameObject.name = "Main Camera";
                mainCam.transform.position = new Vector3(0, 0, -10);
                mainCam.orthographic = true;
                mainCam.orthographicSize = 6.0f;
                mainCam.backgroundColor = Color.black;
                mainCam.clearFlags = CameraClearFlags.SolidColor;
            }

            // Renombra la luz a inglés
            GameObject globalLight = GameObject.Find("Luz Global 2D");
            if (globalLight != null)
            {
                globalLight.name = "Global Light 2D";
            }

            // Renombra objetos al español si todavía tienen nombres en inglés heredados
            GameObject oldGameManager = GameObject.Find("_GameManager");
            if (oldGameManager != null) oldGameManager.name = "_AdministradorJuego";

            GameObject oldCanvas = GameObject.Find("_UI_Canvas");
            if (oldCanvas != null) oldCanvas.name = "_Lienzo_UI";

            GameObject oldEventSystem = GameObject.Find("EventSystem");
            if (oldEventSystem != null) oldEventSystem.name = "SistemaDeEventos";

            AdministradorJuego gameManager = GameObject.FindFirstObjectByType<AdministradorJuego>();
            if (gameManager == null)
            {
                GameObject managerObj = new GameObject("_AdministradorJuego");
                gameManager = managerObj.AddComponent<AdministradorJuego>();
                Debug.Log("✅ Se creó el objeto '_AdministradorJuego' con el script central.");
            }

            Canvas canvas = GameObject.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("_Lienzo_UI");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
                
                if (GameObject.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
                {
                    GameObject eventSystemObj = new GameObject("SistemaDeEventos");
                    eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
                    eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                }
            }

            // 7.05 Barra de Equilibrio
            Transform oldBalance = canvas.transform.Find("Barra_Equilibrio");
            if (oldBalance != null)
            {
                UnityEngine.Object.DestroyImmediate(oldBalance.gameObject);
            }

            GameObject sliderObj = UnityEngine.UI.DefaultControls.CreateSlider(new UnityEngine.UI.DefaultControls.Resources());
            sliderObj.name = "Barra_Equilibrio";
            sliderObj.transform.SetParent(canvas.transform, false);
            
            UnityEngine.UI.Slider slider = sliderObj.GetComponent<UnityEngine.UI.Slider>();
            slider.interactable = false;
            slider.value = 1f;
            
            RectTransform sliderRect = slider.GetComponent<RectTransform>();
            // Ancla la barra en el centro inferior, ideal para monitorearla con visión periférica
            sliderRect.anchorMin = new Vector2(0.5f, 0f);
            sliderRect.anchorMax = new Vector2(0.5f, 0f);
            sliderRect.pivot = new Vector2(0.5f, 0f);
            sliderRect.anchoredPosition = new Vector2(0f, 35f);
            sliderRect.sizeDelta = new Vector2(220f, 16f);

            UnityEngine.UI.Image fillImage = slider.fillRect.GetComponent<UnityEngine.UI.Image>();
            fillImage.color = Color.green;

            // Oculta la perilla del slider
            Transform handleSlideArea = sliderObj.transform.Find("Handle Slide Area");
            if (handleSlideArea != null) handleSlideArea.gameObject.SetActive(false);

            AdministradorUI tempUiManager = canvas.gameObject.GetComponent<AdministradorUI>();
            if (tempUiManager == null) tempUiManager = canvas.gameObject.AddComponent<AdministradorUI>();

            // Asigna el slider por reflexión
            var sliderField = typeof(AdministradorUI).GetField("balanceSlider", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (sliderField != null) sliderField.SetValue(tempUiManager, slider);

            var fillImageField = typeof(AdministradorUI).GetField("balanceFillImage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (fillImageField != null) fillImageField.SetValue(tempUiManager, fillImage);

            Transform oldContainer = canvas.transform.Find("Contenedor_Vidas");
            if (oldContainer != null)
            {
                UnityEngine.Object.DestroyImmediate(oldContainer.gameObject);
            }

            Transform oldText = canvas.transform.Find("Texto_Vidas");
            if (oldText != null)
            {
                UnityEngine.Object.DestroyImmediate(oldText.gameObject);
            }

            Transform oldGameOver = canvas.transform.Find("GameOverPanel");
            if (oldGameOver != null)
            {
                UnityEngine.Object.DestroyImmediate(oldGameOver.gameObject);
            }

            Transform oldCoinsText = canvas.transform.Find("Texto_Monedas");
            if (oldCoinsText != null)
            {
                UnityEngine.Object.DestroyImmediate(oldCoinsText.gameObject);
            }

            GameObject livesContainerObj = new GameObject("Contenedor_Vidas", typeof(RectTransform));
            RectTransform rect = livesContainerObj.GetComponent<RectTransform>();
            rect.SetParent(canvas.transform, false);
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(35f, -35f);
            rect.sizeDelta = new Vector2(450f, 100f);

            HorizontalLayoutGroup layout = livesContainerObj.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 18f;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            // Carga los sprites de hamburguesas
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

            // Crea las hamburguesas de vidas cargándolas como instancias del prefab Hamburguesa_Vida.prefab
            string hamburgerPrefabPath = "Assets/Prefabs/Hamburguesa_Vida.prefab";
            GameObject hamburgerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(hamburgerPrefabPath);

            Image[] hearts = new Image[3];
            for (int i = 0; i < 3; i++)
            {
                GameObject heartObj = null;
                if (hamburgerPrefab != null)
                {
                    heartObj = PrefabUtility.InstantiatePrefab(hamburgerPrefab) as GameObject;
                    heartObj.name = $"Hamburguesa_{i + 1}";
                }
                else
                {
                    heartObj = new GameObject($"Hamburguesa_{i + 1}", typeof(RectTransform));
                }

                RectTransform hRect = heartObj.GetComponent<RectTransform>();
                hRect.SetParent(rect, false);
                hRect.sizeDelta = new Vector2(115f, 85f);

                Image img = heartObj.GetComponent<Image>();
                if (img == null)
                {
                    img = heartObj.AddComponent<Image>();
                    img.preserveAspect = true;
                }

                if (hamburgerPrefab == null)
                {
                    img.preserveAspect = true;
                    
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
                        img.color = Color.yellow;
                        Debug.LogWarning($"[HAMBURGERS DEBUG] No sprites available, using yellow color fallback for Hamburguesa_{i + 1}");
                    }
                }
                else
                {
                    // Si usamos el prefab, sólo asignamos el sprite de la variante/rebanada correspondiente
                    // si el sprite actual es null o es el default (sprite 0)
                    if (img.sprite == null || (hamburgerSprites != null && hamburgerSprites.Length > 0 && img.sprite == hamburgerSprites[0]))
                    {
                        if (hamburgerSprites != null && hamburgerSprites.Length > i)
                        {
                            img.sprite = hamburgerSprites[i];
                            Debug.Log($"[HAMBURGERS DEBUG] Assigned sprite variant {hamburgerSprites[i].name} from prefab to Hamburguesa_{i + 1}");
                        }
                    }
                }

                hearts[i] = img;
            }

            GameObject panelObj = new GameObject("GameOverPanel", typeof(RectTransform));
            RectTransform pRect = panelObj.GetComponent<RectTransform>();
            pRect.SetParent(canvas.transform, false);
            panelObj.SetActive(false);

            pRect.anchorMin = Vector2.zero;
            pRect.anchorMax = Vector2.one;
            pRect.sizeDelta = Vector2.zero;

            Image panelImage = panelObj.AddComponent<Image>();

            // Carga el sprite de derrota
            string loseSpritePath = "Assets/sprites/imagen_perdiste.jpg";
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
                panelImage.color = new Color(0f, 0f, 0f, 0.85f);
                Debug.LogWarning("⚠️ No se encontró 'imagen_perdiste.jpg' para el fondo de derrota, usando fondo oscuro.");
            }

            // panelObj.AddComponent<Button>(); // Removido para que solo los botones sean cliqueables

            // Texto de fallback si no hay sprite
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
                goText.text = "";
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

            // 7.45 Crear el contador de monedas Texto_Monedas
            GameObject coinsTextObj = new GameObject("Texto_Monedas", typeof(RectTransform));
            RectTransform coinsRect = coinsTextObj.GetComponent<RectTransform>();
            coinsRect.SetParent(canvas.transform, false);

            Text coinsText = coinsTextObj.AddComponent<Text>();
            coinsText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (coinsText.font == null) coinsText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            coinsText.fontSize = 24;
            coinsText.color = new Color(1f, 0.84f, 0f); // Dorado
            coinsText.alignment = TextAnchor.MiddleRight;
            coinsText.text = "Monedas: 0";

            coinsRect.anchorMin = new Vector2(1f, 1f);
            coinsRect.anchorMax = new Vector2(1f, 1f);
            coinsRect.pivot = new Vector2(1f, 1f);
            coinsRect.anchoredPosition = new Vector2(-35f, -35f); // 35px de margen
            coinsRect.sizeDelta = new Vector2(200f, 50f);

            Shadow coinsShadow = coinsTextObj.AddComponent<Shadow>();
            coinsShadow.effectColor = Color.black;
            coinsShadow.effectDistance = new Vector2(1f, -1f);

            // 7.5 Configurar el AdministradorUI
            AdministradorUI uiManager = canvas.GetComponent<AdministradorUI>();
            if (uiManager == null)
            {
                uiManager = canvas.gameObject.AddComponent<AdministradorUI>();
            }

            // Crear los botones interactivos de Reintento y Menú
            // Botón "Intentar de nuevo" (Abajo a la Derecha)
            GameObject btnIntentoObj = new GameObject("BotonIntentoNuevo", typeof(RectTransform));
            RectTransform btnIntentoRect = btnIntentoObj.GetComponent<RectTransform>();
            btnIntentoRect.SetParent(pRect, false);
            btnIntentoRect.anchorMin = new Vector2(1f, 0f); // Esquina inferior derecha
            btnIntentoRect.anchorMax = new Vector2(1f, 0f);
            btnIntentoRect.pivot = new Vector2(1f, 0f);     // Pivot en esquina inferior derecha
            btnIntentoRect.anchoredPosition = new Vector2(-100f, 80f); // Separado del borde
            btnIntentoRect.sizeDelta = new Vector2(300f, 105f); // Tamaño proporcional (390x136 nativo)

            Image btnIntentoImg = btnIntentoObj.AddComponent<Image>();
            Sprite spIntento = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/sprites/boton_intenuevo.png");
            if (spIntento != null)
            {
                btnIntentoImg.sprite = spIntento;
                btnIntentoImg.color = Color.white;
            }
            else
            {
                btnIntentoImg.color = Color.gray;
            }

            Button btnIntento = btnIntentoObj.AddComponent<Button>();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(btnIntento.onClick, uiManager.RestartGame);

            // Botón "Menú" (Abajo a la Izquierda)
            GameObject btnMenuObj = new GameObject("BotonMenu", typeof(RectTransform));
            RectTransform btnMenuRect = btnMenuObj.GetComponent<RectTransform>();
            btnMenuRect.SetParent(pRect, false);
            btnMenuRect.anchorMin = new Vector2(0f, 0f); // Esquina inferior izquierda
            btnMenuRect.anchorMax = new Vector2(0f, 0f);
            btnMenuRect.pivot = new Vector2(0f, 0f);     // Pivot en esquina inferior izquierda
            btnMenuRect.anchoredPosition = new Vector2(100f, 80f); // Separado del borde
            btnMenuRect.sizeDelta = new Vector2(300f, 105f); // Tamaño idéntico proporcional

            Image btnMenuImg = btnMenuObj.AddComponent<Image>();
            Sprite spMenu = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/sprites/boton_menu.png");
            if (spMenu != null)
            {
                btnMenuImg.sprite = spMenu;
                btnMenuImg.color = Color.white;
            }
            else
            {
                btnMenuImg.color = Color.gray;
            }

            Button btnMenu = btnMenuObj.AddComponent<Button>();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(btnMenu.onClick, uiManager.CargarMenu);

            var coinsTextField = typeof(AdministradorUI).GetField("coinsText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (coinsTextField != null)
            {
                coinsTextField.SetValue(uiManager, coinsText);
                EditorUtility.SetDirty(uiManager);
                Debug.Log("✅ coinsText inyectado en AdministradorUI.");
            }
            // Asigna los corazones por reflexión
            var heartField = typeof(AdministradorUI).GetField("heartImages", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (heartField != null)
            {
                heartField.SetValue(uiManager, hearts);
                EditorUtility.SetDirty(uiManager);
                Debug.Log("[HEARTS DEBUG] Assigned heartImages array and marked AdministradorUI dirty.");
            }

            var textField = typeof(AdministradorUI).GetField("livesText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (textField != null)
            {
                textField.SetValue(uiManager, null);
                EditorUtility.SetDirty(uiManager);
            }

            var panelField = typeof(AdministradorUI).GetField("gameOverPanel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (panelField != null)
            {
                panelField.SetValue(uiManager, panelObj);
                EditorUtility.SetDirty(uiManager);
            }

            var loseSpriteField = typeof(AdministradorUI).GetField("loseSprite", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (loseSpriteField != null && loseSprite != null)
            {
                loseSpriteField.SetValue(uiManager, loseSprite);
                EditorUtility.SetDirty(uiManager);
                Debug.Log("✅ Sprite de derrota inyectado en AdministradorUI.");
            }

            Debug.Log("✅ AdministradorUI configurado con corazones, HUD de texto y panel de GameOver.");

            // 7.6 Crear o buscar StartPanel
            Transform oldStartPanel = canvas.transform.Find("StartPanel");
            if (oldStartPanel != null)
            {
                UnityEngine.Object.DestroyImmediate(oldStartPanel.gameObject);
            }

            GameObject startPanelObj = new GameObject("StartPanel", typeof(RectTransform));
            RectTransform startPanelRect = startPanelObj.GetComponent<RectTransform>();
            startPanelRect.SetParent(canvas.transform, false);
            
            // Set StartPanel to cover the whole screen
            startPanelRect.anchorMin = Vector2.zero;
            startPanelRect.anchorMax = Vector2.one;
            startPanelRect.sizeDelta = Vector2.zero;

            Image startPanelImage = startPanelObj.AddComponent<Image>();

            // Carga el sprite de inicio
            string startSpritePath = "Assets/sprites/imagen_inicio.jpg";
            EnsureIsSprite(startSpritePath);
            Sprite startSprite = AssetDatabase.LoadAssetAtPath<Sprite>(startSpritePath);
            if (startSprite != null)
            {
                startPanelImage.sprite = startSprite;
                startPanelImage.color = Color.white;
                Debug.Log("✅ Pantalla de inicio asignada exitosamente.");
            }
            else
            {
                startPanelImage.color = new Color(0.1f, 0.1f, 0.1f, 1f);
                Debug.LogWarning("⚠️ No se encontró 'imagen_inicio.jpg', usando fondo oscuro.");
            }

            // Asegurarse de que los botones estén importados como Sprites
            EnsureIsSprite("Assets/sprites/boton_jugar.png");
            EnsureIsSprite("Assets/sprites/boton_mapa.png");
            EnsureIsSprite("Assets/sprites/boton_configuracion.png");

            Sprite spriteJugar = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/sprites/boton_jugar.png");
            Sprite spriteMapa = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/sprites/boton_mapa.png");
            Sprite spriteConfiguracion = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/sprites/boton_configuracion.png");

            // Crear Botón "BotonJugar" (Verde, Y: -50)
            GameObject btnJugarObj = new GameObject("BotonJugar", typeof(RectTransform));
            RectTransform btnJugarRect = btnJugarObj.GetComponent<RectTransform>();
            btnJugarRect.SetParent(startPanelRect, false);
            btnJugarRect.anchorMin = new Vector2(0.5f, 0.5f);
            btnJugarRect.anchorMax = new Vector2(0.5f, 0.5f);
            btnJugarRect.pivot = new Vector2(0.5f, 0.5f);
            btnJugarRect.anchoredPosition = new Vector2(0f, -50f);
            btnJugarRect.sizeDelta = new Vector2(300f, 102f);

            Image btnJugarImg = btnJugarObj.AddComponent<Image>();
            if (spriteJugar != null)
            {
                btnJugarImg.sprite = spriteJugar;
                btnJugarImg.color = Color.white;
            }
            else
            {
                btnJugarImg.color = Color.green;
            }

            Button btnJugar = btnJugarObj.AddComponent<Button>();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(btnJugar.onClick, uiManager.IniciarJuego);

            // Crear Botón "BotonMapa" (Amarillo, Y: -160)
            GameObject btnMapaObj = new GameObject("BotonMapa", typeof(RectTransform));
            RectTransform btnMapaRect = btnMapaObj.GetComponent<RectTransform>();
            btnMapaRect.SetParent(startPanelRect, false);
            btnMapaRect.anchorMin = new Vector2(0.5f, 0.5f);
            btnMapaRect.anchorMax = new Vector2(0.5f, 0.5f);
            btnMapaRect.pivot = new Vector2(0.5f, 0.5f);
            btnMapaRect.anchoredPosition = new Vector2(0f, -160f);
            btnMapaRect.sizeDelta = new Vector2(300f, 102f);

            Image btnMapaImg = btnMapaObj.AddComponent<Image>();
            if (spriteMapa != null)
            {
                btnMapaImg.sprite = spriteMapa;
                btnMapaImg.color = Color.white;
            }
            else
            {
                btnMapaImg.color = Color.yellow;
            }

            Button btnMapa = btnMapaObj.AddComponent<Button>();

            // Crear Botón "BotonConfiguracion" (Azul, Y: -270)
            GameObject btnConfigObj = new GameObject("BotonConfiguracion", typeof(RectTransform));
            RectTransform btnConfigRect = btnConfigObj.GetComponent<RectTransform>();
            btnConfigRect.SetParent(startPanelRect, false);
            btnConfigRect.anchorMin = new Vector2(0.5f, 0.5f);
            btnConfigRect.anchorMax = new Vector2(0.5f, 0.5f);
            btnConfigRect.pivot = new Vector2(0.5f, 0.5f);
            btnConfigRect.anchoredPosition = new Vector2(0f, -270f);
            btnConfigRect.sizeDelta = new Vector2(300f, 98f);

            Image btnConfigImg = btnConfigObj.AddComponent<Image>();
            if (spriteConfiguracion != null)
            {
                btnConfigImg.sprite = spriteConfiguracion;
                btnConfigImg.color = Color.white;
            }
            else
            {
                btnConfigImg.color = Color.blue;
            }

            Button btnConfig = btnConfigObj.AddComponent<Button>();

            // Asignar el startPanel en el AdministradorUI por reflexión
            var startPanelField = typeof(AdministradorUI).GetField("startPanel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (startPanelField != null)
            {
                startPanelField.SetValue(uiManager, startPanelObj);
                EditorUtility.SetDirty(uiManager);
                Debug.Log("✅ startPanel inyectado en AdministradorUI.");
            }

            // 7.7 Crear o buscar VictoryPanel
            Transform oldVictoryPanel = canvas.transform.Find("VictoryPanel");
            if (oldVictoryPanel != null)
            {
                UnityEngine.Object.DestroyImmediate(oldVictoryPanel.gameObject);
            }

            GameObject victoryPanelObj = new GameObject("VictoryPanel", typeof(RectTransform));
            RectTransform victoryPanelRect = victoryPanelObj.GetComponent<RectTransform>();
            victoryPanelRect.SetParent(canvas.transform, false);
            victoryPanelObj.SetActive(false); // Empieza desactivado

            victoryPanelRect.anchorMin = Vector2.zero;
            victoryPanelRect.anchorMax = Vector2.one;
            victoryPanelRect.sizeDelta = Vector2.zero;

            Image victoryPanelImage = victoryPanelObj.AddComponent<Image>();

            // Carga el sprite de victoria
            string victorySpritePath = "Assets/sprites/imagen_ganaste.jpg";
            EnsureIsSprite(victorySpritePath);
            Sprite victorySprite = AssetDatabase.LoadAssetAtPath<Sprite>(victorySpritePath);
            if (victorySprite != null)
            {
                victoryPanelImage.sprite = victorySprite;
                victoryPanelImage.color = Color.white;
                Debug.Log("✅ Pantalla de victoria asignada exitosamente.");
            }
            else
            {
                victoryPanelImage.color = new Color(0f, 0.5f, 0f, 0.85f);
                Debug.LogWarning("⚠️ No se encontró 'imagen_ganaste.jpg', usando fondo verde.");
            }

            // Crear Botón "Siguiente" (Verde, Jugar, abajo a la derecha)
            GameObject btnSiguienteObj = new GameObject("BotonSiguiente", typeof(RectTransform));
            RectTransform btnSiguienteRect = btnSiguienteObj.GetComponent<RectTransform>();
            btnSiguienteRect.SetParent(victoryPanelRect, false);
            btnSiguienteRect.anchorMin = new Vector2(1f, 0f); // Esquina inferior derecha
            btnSiguienteRect.anchorMax = new Vector2(1f, 0f);
            btnSiguienteRect.pivot = new Vector2(1f, 0f);     // Pivot en esquina inferior derecha
            btnSiguienteRect.anchoredPosition = new Vector2(-100f, 80f); // Separado del borde
            btnSiguienteRect.sizeDelta = new Vector2(300f, 102f); // Tamaño proporcional

            Image btnSiguienteImg = btnSiguienteObj.AddComponent<Image>();
            Sprite spSiguiente = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/sprites/boton_jugar.png");
            if (spSiguiente != null)
            {
                btnSiguienteImg.sprite = spSiguiente;
                btnSiguienteImg.color = Color.white;
            }
            else
            {
                btnSiguienteImg.color = Color.green;
            }

            Button btnSiguiente = btnSiguienteObj.AddComponent<Button>();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(btnSiguiente.onClick, uiManager.AvanzarSiguienteDia);

            // Botón "Menú" (Abajo a la Izquierda, blue Menu button)
            GameObject btnVicMenuObj = new GameObject("BotonMenu", typeof(RectTransform));
            RectTransform btnVicMenuRect = btnVicMenuObj.GetComponent<RectTransform>();
            btnVicMenuRect.SetParent(victoryPanelRect, false);
            btnVicMenuRect.anchorMin = new Vector2(0f, 0f); // Esquina inferior izquierda
            btnVicMenuRect.anchorMax = new Vector2(0f, 0f);
            btnVicMenuRect.pivot = new Vector2(0f, 0f);     // Pivot en esquina inferior izquierda
            btnVicMenuRect.anchoredPosition = new Vector2(100f, 80f); // Separado del borde
            btnVicMenuRect.sizeDelta = new Vector2(300f, 105f); // Tamaño proporcional

            Image btnVicMenuImg = btnVicMenuObj.AddComponent<Image>();
            Sprite spVicMenu = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/sprites/boton_menu.png");
            if (spVicMenu != null)
            {
                btnVicMenuImg.sprite = spVicMenu;
                btnVicMenuImg.color = Color.white;
            }
            else
            {
                btnVicMenuImg.color = Color.blue;
            }

            Button btnVicMenu = btnVicMenuObj.AddComponent<Button>();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(btnVicMenu.onClick, uiManager.CargarMenu);

            // Inyectar referencias en AdministradorUI por reflexión
            var victoryPanelField = typeof(AdministradorUI).GetField("victoryPanel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (victoryPanelField != null)
            {
                victoryPanelField.SetValue(uiManager, victoryPanelObj);
                EditorUtility.SetDirty(uiManager);
            }

            var victorySpriteField = typeof(AdministradorUI).GetField("victorySprite", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (victorySpriteField != null && victorySprite != null)
            {
                victorySpriteField.SetValue(uiManager, victorySprite);
                EditorUtility.SetDirty(uiManager);
            }

            // Asigna la capa UI de forma recursiva
            int uiLayer = LayerMask.NameToLayer("UI");
            if (uiLayer >= 0)
            {
                SetLayerRecursively(canvas.gameObject, uiLayer);
            }

            canvas.gameObject.SetActive(true);


            // Marca la escena sucia y guarda en disco
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

            GameObject riderObj = FindRiderGameObject();

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
                    go.name != "Camara Principal" && 
                    go.name != "Global Light 2D" && 
                    go.name != "Luz Global 2D" && 
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
                if (go != riderObj && 
                    go.name != "Main Camera" && 
                    go.name != "Camara Principal" && 
                    go.name != "Global Light 2D" && 
                    go.name != "Luz Global 2D")
                {
                    string name = go.name;
                    if (name == "RoadBackground" || name == "FondoCalle" || name == "_SceneBuilder" || name == "GeneradorObstaculos" || 
                        name.StartsWith("Cliente_NPC_") || name == "ControladorJugador" || 
                        name == "_GameManager" || name == "_AdministradorJuego" || 
                        name == "_ParallaxBackground" || name == "_ScrollingBackground" || name == "_FondoCalle")
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
                
                // Configura Pixels Per Unit en 181 para calzar en la cámara de tamaño 6.0
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

        private static void EnsureSpritesheetSliced(string path, int columns, int rows)
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                bool needsReimport = false;
                if (importer.textureType != TextureImporterType.Sprite || importer.spriteImportMode != SpriteImportMode.Multiple)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Multiple;
                    needsReimport = true;
                }

                // Check if already sliced
                var currentSheet = importer.spritesheet;
                if (currentSheet == null || currentSheet.Length != columns * rows)
                {
                    needsReimport = true;
                }

                if (needsReimport)
                {
                    Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                    if (tex != null)
                    {
                        int width = tex.width;
                        int height = tex.height;
                        int frameWidth = width / columns;
                        int frameHeight = height / rows;
                        
                        SpriteMetaData[] metas = new SpriteMetaData[columns * rows];
                        int idx = 0;
                        for (int r = rows - 1; r >= 0; r--) // Unity grid starts from bottom-left
                        {
                            for (int c = 0; c < columns; c++)
                            {
                                metas[idx] = new SpriteMetaData();
                                metas[idx].rect = new Rect(c * frameWidth, r * frameHeight, frameWidth, frameHeight);
                                metas[idx].name = $"{tex.name}_{idx}";
                                metas[idx].alignment = 0; // Center
                                idx++;
                            }
                        }
                        importer.spritesheet = metas;
                        importer.SaveAndReimport();
                        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                    }
                }
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
            choqueSprites[0] = spritesList[8]; // fotograma de choque

            AnimationClip pedaleandoClip = CreateOrReplaceClip("Assets/sprites/Pedaleando.anim", pedaleandoSprites, 12f, true);
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

        private static void EnsurePrefabsExist(GeneradorObstaculos spawner)
        {
            AssetDatabase.Refresh();

            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            {
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            }

            var conoPrefabField = typeof(GeneradorObstaculos).GetField("conoPrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var bachePrefabField = typeof(GeneradorObstaculos).GetField("bachePrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var basuraPrefabField = typeof(GeneradorObstaculos).GetField("basuraPrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var autoPrefabsField = typeof(GeneradorObstaculos).GetField("autoPrefabs", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            Sprite[] minorSprites = LoadSpritesFromPath("Assets/sprites/imagen_obstaculos.png");

            // 1. Crear Prefab de Cono si no existe
            string conoPrefabPath = "Assets/Prefabs/Obstaculo_Cono.prefab";
            GameObject conoPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(conoPrefabPath);
            if (conoPrefab == null && minorSprites != null && minorSprites.Length > 0)
            {
                GameObject tempObj = new GameObject("Obstaculo_Cono");
                tempObj.tag = "Obstaculo";
                tempObj.transform.localScale = new Vector3(1.05f, 1.05f, 1f);

                GameObject visualObj = new GameObject("Visual");
                visualObj.transform.SetParent(tempObj.transform, false);
                SpriteRenderer sr = visualObj.AddComponent<SpriteRenderer>();
                sr.sprite = minorSprites[0];
                Vector3 centerOffset = sr.sprite.bounds.center;
                visualObj.transform.localPosition = new Vector3(-centerOffset.x, -centerOffset.y, 0f);
                sr.sortingOrder = 8;

                BoxCollider2D col = tempObj.AddComponent<BoxCollider2D>();
                col.isTrigger = true;
                col.size = new Vector2(0.9f, 0.9f);

                Obstaculo obstacle = tempObj.AddComponent<Obstaculo>();
                var typeField = typeof(Obstaculo).GetField("type", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (typeField != null)
                {
                    typeField.SetValue(obstacle, TipoObstaculo.Cone);
                }

                conoPrefab = PrefabUtility.SaveAsPrefabAsset(tempObj, conoPrefabPath);
                UnityEngine.Object.DestroyImmediate(tempObj);
                Debug.Log("✅ Prefab de Cono creado con éxito en Assets/Prefabs.");
            }

            if (conoPrefabField != null && conoPrefab != null)
            {
                conoPrefabField.SetValue(spawner, conoPrefab);
            }

            // 1b. Crear Prefab de Bache si no existe
            string bachePrefabPath = "Assets/Prefabs/Obstaculo_Bache.prefab";
            GameObject bachePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(bachePrefabPath);
            if (bachePrefab == null && minorSprites != null && minorSprites.Length > 1)
            {
                GameObject tempObj = new GameObject("Obstaculo_Bache");
                tempObj.tag = "Obstaculo";
                tempObj.transform.localScale = new Vector3(1.15f, 1.15f, 1f);

                GameObject visualObj = new GameObject("Visual");
                visualObj.transform.SetParent(tempObj.transform, false);
                SpriteRenderer sr = visualObj.AddComponent<SpriteRenderer>();
                sr.sprite = minorSprites[1];
                Vector3 centerOffset = sr.sprite.bounds.center;
                visualObj.transform.localPosition = new Vector3(-centerOffset.x, -centerOffset.y, 0f);
                sr.sortingOrder = 7; // Debajo de los obstáculos y autos

                BoxCollider2D col = tempObj.AddComponent<BoxCollider2D>();
                col.isTrigger = true;
                col.size = new Vector2(sr.sprite.bounds.size.x * 0.8f, sr.sprite.bounds.size.y * 0.8f);

                Obstaculo obstacle = tempObj.AddComponent<Obstaculo>();
                var typeField = typeof(Obstaculo).GetField("type", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (typeField != null)
                {
                    typeField.SetValue(obstacle, TipoObstaculo.Pothole);
                }

                bachePrefab = PrefabUtility.SaveAsPrefabAsset(tempObj, bachePrefabPath);
                UnityEngine.Object.DestroyImmediate(tempObj);
                Debug.Log("✅ Prefab de Bache creado con éxito en Assets/Prefabs.");
            }

            if (bachePrefabField != null && bachePrefab != null)
            {
                bachePrefabField.SetValue(spawner, bachePrefab);
            }

            // 1c. Crear Prefab de Basura si no existe
            string basuraPrefabPath = "Assets/Prefabs/Obstaculo_Basura.prefab";
            GameObject basuraPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(basuraPrefabPath);
            if (basuraPrefab == null && minorSprites != null && minorSprites.Length > 2)
            {
                GameObject tempObj = new GameObject("Obstaculo_Basura");
                tempObj.tag = "Obstaculo";
                tempObj.transform.localScale = new Vector3(1.05f, 1.05f, 1f);

                GameObject visualObj = new GameObject("Visual");
                visualObj.transform.SetParent(tempObj.transform, false);
                SpriteRenderer sr = visualObj.AddComponent<SpriteRenderer>();
                sr.sprite = minorSprites[2];
                Vector3 centerOffset = sr.sprite.bounds.center;
                visualObj.transform.localPosition = new Vector3(-centerOffset.x, -centerOffset.y, 0f);
                sr.sortingOrder = 8;

                BoxCollider2D col = tempObj.AddComponent<BoxCollider2D>();
                col.isTrigger = true;
                col.size = new Vector2(0.9f, 0.9f);

                Obstaculo obstacle = tempObj.AddComponent<Obstaculo>();
                var typeField = typeof(Obstaculo).GetField("type", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (typeField != null)
                {
                    typeField.SetValue(obstacle, TipoObstaculo.Cone); // Se comporta como cono (fijo, daño, se destruye)
                }

                basuraPrefab = PrefabUtility.SaveAsPrefabAsset(tempObj, basuraPrefabPath);
                UnityEngine.Object.DestroyImmediate(tempObj);
                Debug.Log("✅ Prefab de Basura creado con éxito en Assets/Prefabs.");
            }

            if (basuraPrefabField != null && basuraPrefab != null)
            {
                basuraPrefabField.SetValue(spawner, basuraPrefab);
            }

            // 2. Crear Prefabs de Autos si no existen
            Sprite[] carSprites = LoadSpritesFromPath("Assets/sprites/imagenes_autos.png");
            if (carSprites != null && carSprites.Length > 0)
            {
                GameObject[] autoPrefabs = new GameObject[carSprites.Length];
                for (int i = 0; i < carSprites.Length; i++)
                {
                    string autoPrefabPath = $"Assets/Prefabs/Obstaculo_Auto_{i}.prefab";
                    GameObject autoPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(autoPrefabPath);
                    if (autoPrefab == null)
                    {
                        GameObject tempObj = new GameObject($"Obstaculo_Auto_{i}");
                        tempObj.tag = "Car";
                        tempObj.transform.localScale = new Vector3(1.65f, 1.65f, 1f);

                        GameObject visualObj = new GameObject("Visual");
                        visualObj.transform.SetParent(tempObj.transform, false);
                        SpriteRenderer sr = visualObj.AddComponent<SpriteRenderer>();
                        sr.sprite = carSprites[i];
                        sr.sortingOrder = 8;

                        Vector3 centerOffset = sr.sprite.bounds.center;
                        visualObj.transform.localPosition = new Vector3(-centerOffset.x, -centerOffset.y, 0f);

                        BoxCollider2D col = tempObj.AddComponent<BoxCollider2D>();
                        col.isTrigger = true;
                        col.size = new Vector2(sr.sprite.bounds.size.x * 0.35f, sr.sprite.bounds.size.y * 0.6f);
                        col.offset = new Vector2(-sr.sprite.bounds.center.x, -sr.sprite.bounds.center.y);

                        Obstaculo obstacle = tempObj.AddComponent<Obstaculo>();
                        var typeField = typeof(Obstaculo).GetField("type", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        if (typeField != null)
                        {
                            TipoObstaculo carType = (i % 2 == 0) ? TipoObstaculo.BlackCar : TipoObstaculo.GreenCar;
                            typeField.SetValue(obstacle, carType);
                        }

                        autoPrefab = PrefabUtility.SaveAsPrefabAsset(tempObj, autoPrefabPath);
                        UnityEngine.Object.DestroyImmediate(tempObj);
                        Debug.Log($"✅ Prefab de Auto {i} creado con éxito en Assets/Prefabs.");
                    }
                    autoPrefabs[i] = autoPrefab;
                }

                if (autoPrefabsField != null)
                {
                    autoPrefabsField.SetValue(spawner, autoPrefabs);
                }
            }

            // 3. Crear Prefab de Hamburguesa_Vida si no existe
            string hamburguesaPrefabPath = "Assets/Prefabs/Hamburguesa_Vida.prefab";
            GameObject hamburguesaPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(hamburguesaPrefabPath);
            if (hamburguesaPrefab == null)
            {
                GameObject tempObj = new GameObject("Hamburguesa_Vida", typeof(RectTransform));
                RectTransform rTrans = tempObj.GetComponent<RectTransform>();
                rTrans.sizeDelta = new Vector2(115f, 85f);

                Image img = tempObj.AddComponent<Image>();
                img.preserveAspect = true;

                Sprite[] hamburgerSprites = LoadSpritesFromPath("Assets/sprites/hamburguesa_ui.png");
                if (hamburgerSprites != null && hamburgerSprites.Length > 0)
                {
                    img.sprite = hamburgerSprites[0];
                    img.color = Color.white;
                }
                else
                {
                    img.color = Color.yellow;
                }

                hamburguesaPrefab = PrefabUtility.SaveAsPrefabAsset(tempObj, hamburguesaPrefabPath);
                UnityEngine.Object.DestroyImmediate(tempObj);
                Debug.Log("✅ Prefab de Hamburguesa_Vida creado con éxito en Assets/Prefabs.");
            }

            // 4. Crear Prefab de Hamburguesa_PowerUp (coleccionable en la calle) si no existe
            string powerUpPrefabPath = "Assets/Prefabs/Hamburguesa_PowerUp.prefab";
            GameObject powerUpPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(powerUpPrefabPath);
            if (powerUpPrefab == null)
            {
                // Objeto raíz con tag PowerUp
                GameObject tempObj = new GameObject("Hamburguesa_PowerUp");
                tempObj.tag = "PowerUp"; // registrado abajo

                // Colisionador trigger
                CircleCollider2D col = tempObj.AddComponent<CircleCollider2D>();
                col.isTrigger = true;
                col.radius = 0.45f;

                // Hijo visual con SpriteRenderer (world-space, no UI)
                GameObject visualObj = new GameObject("Visual");
                visualObj.transform.SetParent(tempObj.transform, false);
                SpriteRenderer sr = visualObj.AddComponent<SpriteRenderer>();
                sr.sortingLayerName = "Default";
                sr.sortingOrder = 10;

                // Usar el primer sprite de hamburguesa disponible
                Sprite[] hamburgerSprites = LoadSpritesFromPath("Assets/sprites/hamburguesa_ui.png");
                if (hamburgerSprites != null && hamburgerSprites.Length > 0)
                {
                    sr.sprite = hamburgerSprites[0];
                }

                // Escala para que se vea bien en la calle
                tempObj.transform.localScale = new Vector3(0.8f, 0.8f, 1f);

                // Script de movimiento y colección
                tempObj.AddComponent<HamburguesaVida>();

                powerUpPrefab = PrefabUtility.SaveAsPrefabAsset(tempObj, powerUpPrefabPath);
                UnityEngine.Object.DestroyImmediate(tempObj);
                Debug.Log("✅ Prefab de Hamburguesa_PowerUp creado con éxito en Assets/Prefabs.");
            }

            // Asignar el power-up prefab al spawner
            var hamburguesaPowerUpField = typeof(GeneradorObstaculos).GetField("hamburguesaPowerUpPrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (hamburguesaPowerUpField != null && powerUpPrefab != null)
            {
                hamburguesaPowerUpField.SetValue(spawner, powerUpPrefab);
            }

            // 5. Crear Prefab de Moneda si no existe o tiene escala incorrecta
            string monedaPrefabPath = "Assets/Prefabs/Moneda.prefab";
            GameObject monedaPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(monedaPrefabPath);
            if (monedaPrefab == null || Mathf.Abs(monedaPrefab.transform.localScale.x - 0.8f) > 0.01f)
            {
                if (monedaPrefab != null)
                {
                    AssetDatabase.DeleteAsset(monedaPrefabPath);
                }
                GameObject tempObj = new GameObject("Moneda");
                tempObj.transform.localScale = new Vector3(0.8f, 0.8f, 1f);

                CircleCollider2D col = tempObj.AddComponent<CircleCollider2D>();
                col.isTrigger = true;
                col.radius = 0.45f;

                SpriteRenderer sr = tempObj.AddComponent<SpriteRenderer>();
                sr.sortingOrder = 9;

                // Cargar sprite estático de moneda.png
                EnsureIsSprite("Assets/sprites/moneda.png");
                Sprite singleCoinSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/sprites/moneda.png");
                sr.sprite = singleCoinSprite;

                Moneda coinScript = tempObj.AddComponent<Moneda>();
                
                // Intentar cargar fotogramas de la hoja de animación (8 columnas, 1 fila)
                EnsureSpritesheetSliced("Assets/sprites/moneda_spritesheet.png", 8, 1);
                Sprite[] animationFrames = LoadSpritesFromPath("Assets/sprites/moneda_spritesheet.png");
                if (animationFrames != null && animationFrames.Length > 1)
                {
                    var animFramesField = typeof(Moneda).GetField("animationFrames", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (animFramesField != null)
                    {
                        animFramesField.SetValue(coinScript, animationFrames);
                    }
                }

                monedaPrefab = PrefabUtility.SaveAsPrefabAsset(tempObj, monedaPrefabPath);
                UnityEngine.Object.DestroyImmediate(tempObj);
                Debug.Log("✅ Prefab de Moneda creado con éxito en Assets/Prefabs.");
            }

            // Asignar el prefab de moneda al spawner
            var monedaPrefabField = typeof(GeneradorObstaculos).GetField("monedaPrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (monedaPrefabField != null && monedaPrefab != null)
            {
                monedaPrefabField.SetValue(spawner, monedaPrefab);
            }

            // 6. Crear Prefab de Potenciador_Energia si no existe o tiene escala incorrecta
            string potenciadorPrefabPath = "Assets/Prefabs/Potenciador_Energia.prefab";
            GameObject potenciadorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(potenciadorPrefabPath);
            if (potenciadorPrefab == null || Mathf.Abs(potenciadorPrefab.transform.localScale.x - 0.8f) > 0.01f)
            {
                if (potenciadorPrefab != null)
                {
                    AssetDatabase.DeleteAsset(potenciadorPrefabPath);
                }
                GameObject tempObj = new GameObject("Potenciador_Energia");
                tempObj.tag = "PowerUp";
                tempObj.transform.localScale = new Vector3(0.8f, 0.8f, 1f);

                CircleCollider2D col = tempObj.AddComponent<CircleCollider2D>();
                col.isTrigger = true;
                col.radius = 0.45f;

                SpriteRenderer sr = tempObj.AddComponent<SpriteRenderer>();
                sr.sortingOrder = 9;

                // Cargar sprite estático de potenciador_energia.png
                EnsureIsSprite("Assets/sprites/potenciador_energia.png");
                Sprite singlePowerUpSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/sprites/potenciador_energia.png");
                sr.sprite = singlePowerUpSprite;

                PotenciadorEnergia powerScript = tempObj.AddComponent<PotenciadorEnergia>();
                
                // Intentar cargar fotogramas de la hoja de animación (8 columnas, 1 fila)
                EnsureSpritesheetSliced("Assets/sprites/potenciador_energia_spritesheet.png", 8, 1);
                Sprite[] animationFrames = LoadSpritesFromPath("Assets/sprites/potenciador_energia_spritesheet.png");
                if (animationFrames != null && animationFrames.Length > 1)
                {
                    var animFramesField = typeof(PotenciadorEnergia).GetField("animationFrames", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (animFramesField != null)
                    {
                        animFramesField.SetValue(powerScript, animationFrames);
                    }
                }

                potenciadorPrefab = PrefabUtility.SaveAsPrefabAsset(tempObj, potenciadorPrefabPath);
                UnityEngine.Object.DestroyImmediate(tempObj);
                Debug.Log("✅ Prefab de Potenciador_Energia creado con éxito en Assets/Prefabs.");
            }

            // Asignar el prefab de potenciador al spawner
            var potenciadorPrefabField = typeof(GeneradorObstaculos).GetField("potenciadorEnergiaPrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (potenciadorPrefabField != null && potenciadorPrefab != null)
            {
                potenciadorPrefabField.SetValue(spawner, potenciadorPrefab);
            }

            EditorUtility.SetDirty(spawner);
        }

        private static Sprite[] LoadSpritesFromPath(string path)
        {
            if (System.IO.File.Exists(path))
            {
                var subAssets = AssetDatabase.LoadAllAssetsAtPath(path);
                if (subAssets != null)
                {
                    return subAssets.OfType<Sprite>().OrderBy(s => GetSpriteNumber(s.name)).ToArray();
                }
            }
            return null;
        }

        private static GameObject FindRiderGameObject()
        {
            ControladorJugador pc = GameObject.FindAnyObjectByType<ControladorJugador>();
            if (pc != null)
            {
                return pc.gameObject;
            }

            GameObject riderObj = GameObject.Find("imagen_repartidor_0 (1)");
            if (riderObj == null)
            {
                riderObj = GameObject.Find("Jugador");
            }
            if (riderObj == null)
            {
                GameObject[] allObjs = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
                foreach (var go in allObjs)
                {
                    string nameLower = go.name.ToLower();
                    if (nameLower.Contains("repartidor") || nameLower.Contains("player") || nameLower.Contains("jugador"))
                    {
                        riderObj = go;
                        break;
                    }
                }
            }
            return riderObj;
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
                        string[] requiredTags = new string[] { "Obstaculo", "Car", "PowerUp" };
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
