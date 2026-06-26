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
                             startPanelObj.transform.Find("BotonConfiguracion") == null ||
                             startPanelObj.transform.Find("Boton_instrucciones") == null)
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
                GameObject configPanelObj = GameObject.Find("ConfigPanel");
                if (configPanelObj == null)
                {
                    needsFix = true;
                }
                else
                {
                    Transform iconUser = configPanelObj.transform.Find("IconoUsuario");
                    Transform iconMusic = configPanelObj.transform.Find("IconoMusica");
                    Transform iconSound = configPanelObj.transform.Find("IconoSonido");
                    Transform maskUser = configPanelObj.transform.Find("Mascara_Texto_Usuario");
                    Transform maskMusic = configPanelObj.transform.Find("Mascara_Texto_Musica");
                    Transform maskSound = configPanelObj.transform.Find("Mascara_Texto_Sonido");

                    if (iconUser == null || iconMusic == null || iconSound == null || maskUser == null || maskMusic != null || maskSound != null)
                    {
                        needsFix = true;
                    }
                    else
                    {
                        RectTransform iconUserRect = iconUser.GetComponent<RectTransform>();
                        RectTransform iconSoundRect = iconSound.GetComponent<RectTransform>();
                        RectTransform maskUserRect = maskUser.GetComponent<RectTransform>();
                        
                        if (iconUserRect != null && (Mathf.Abs(iconUserRect.anchoredPosition.x - (-251.5f)) > 1.0f || Mathf.Abs(iconUserRect.anchoredPosition.y - 110.0f) > 1.0f))
                        {
                            needsFix = true;
                        }
                        else if (iconSoundRect != null && Mathf.Abs(iconSoundRect.anchoredPosition.y - (-200.0f)) > 1.0f)
                        {
                            needsFix = true;
                        }
                        else if (maskUserRect != null && (Mathf.Abs(maskUserRect.anchoredPosition.y - 58.0f) > 1.0f || Mathf.Abs(maskUserRect.sizeDelta.x - 100f) > 1.0f))
                        {
                            needsFix = true;
                        }
                    }
                }
            }

            if (!needsFix)
            {
                GameObject canvasObj = GameObject.Find("_Lienzo_UI") ?? GameObject.Find("_UI_Canvas");
                if (canvasObj != null)
                {
                    Transform tMonedas = canvasObj.transform.Find("Texto_Monedas");
                    Transform tSkipPanel = canvasObj.transform.Find("IntroVideo_SkipPanel");
                    if (tMonedas == null || tSkipPanel == null)
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
                GameObject canvasObj = GameObject.Find("_Lienzo_UI") ?? GameObject.Find("_UI_Canvas");
                if (canvasObj != null)
                {
                    Transform tInstrucciones = canvasObj.transform.Find("PanelInstrucciones");
                    if (tInstrucciones == null)
                    {
                        needsFix = true;
                    }
                    else
                    {
                        if (tInstrucciones.transform.Find("Contenido/BotonCerrar") == null)
                        {
                            needsFix = true;
                        }
                    }
                }
            }

            if (!needsFix)
            {
                GameObject canvasObj = GameObject.Find("_Lienzo_UI") ?? GameObject.Find("_UI_Canvas");
                if (canvasObj != null)
                {
                    Transform tBalance = canvasObj.transform.Find("Barra_Equilibrio");
                    if (tBalance == null || tBalance.GetComponent<UnityEngine.UI.Image>() == null)
                    {
                        needsFix = true;
                    }
                    else
                    {
                        RectTransform rt = tBalance.GetComponent<RectTransform>();
                        if (rt != null && (Mathf.Abs(rt.anchoredPosition.y - 40f) > 0.1f || 
                                           Mathf.Abs(rt.sizeDelta.x - 270f) > 0.1f || 
                                           Mathf.Abs(rt.sizeDelta.y - 69f) > 0.1f))
                        {
                            needsFix = true;
                        }
                    }

                    if (!needsFix)
                    {
                        Transform tBooster = canvasObj.transform.Find("Barra_Potenciador");
                        if (tBooster == null || tBooster.GetComponent<UnityEngine.UI.Image>() == null)
                        {
                            needsFix = true;
                        }
                        else
                        {
                            RectTransform rt = tBooster.GetComponent<RectTransform>();
                            if (rt != null && (Mathf.Abs(rt.anchoredPosition.y - 40f) > 0.1f || 
                                               Mathf.Abs(rt.anchoredPosition.x - 170f) > 0.1f ||
                                               Mathf.Abs(rt.sizeDelta.x - 270f) > 0.1f || 
                                               Mathf.Abs(rt.sizeDelta.y - 69f) > 0.1f))
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
                if (riderObj == null || (riderObj.transform.localScale.x > 0.8f || riderObj.transform.localScale.x < 0.7f))
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

            if (riderObj == null)
            {
                riderObj = new GameObject("Jugador");
                SpriteRenderer sr = riderObj.AddComponent<SpriteRenderer>();
                
                // Cargar sprite inicial de sprite_repartidor.png
                string playerSpriteSheetPath = "Assets/sprites/sprite_repartidor.png";
                var assets = AssetDatabase.LoadAllAssetsAtPath(playerSpriteSheetPath);
                foreach (var asset in assets)
                {
                    if (asset is Sprite sprite && (sprite.name == "sprite_repartidor_0" || sprite.name.EndsWith("_0")))
                    {
                        sr.sprite = sprite;
                        break;
                    }
                }
                Debug.Log("✅ Creando GameObject 'Jugador' desde cero con SpriteRenderer.");
            }

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

                // Escala al repartidor para compensar la resolución del nuevo sprite
                riderObj.transform.localScale = new Vector3(0.75f, 0.75f, 1f);

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

            GameObject balanceObj = new GameObject("Barra_Equilibrio");
            balanceObj.transform.SetParent(canvas.transform, false);
            
            UnityEngine.UI.Image balanceImage = balanceObj.AddComponent<UnityEngine.UI.Image>();
            
            RectTransform balanceRect = balanceObj.GetComponent<RectTransform>();
            // Ancla la barra en el centro inferior, ideal para monitorearla con visión periférica
            balanceRect.anchorMin = new Vector2(0.5f, 0f);
            balanceRect.anchorMax = new Vector2(0.5f, 0f);
            balanceRect.pivot = new Vector2(0.5f, 0f);
            // Tamaño de la barra escalado a 270x69 (50% del original de 540x138)
            balanceRect.sizeDelta = new Vector2(270f, 69f);
            balanceRect.anchoredPosition = new Vector2(0f, 40f);

            // Cargar los sprites múltiples
            string balanceSpritePath = "Assets/sprites/barra_equilibrio.png";
            Sprite[] balanceSprites = AssetDatabase.LoadAllAssetsAtPath(balanceSpritePath)
                .OfType<Sprite>()
                .OrderBy(s => s.name)
                .ToArray();

            if (balanceSprites != null && balanceSprites.Length > 0)
            {
                // Asignar el sprite barra_equilibrio_0 (index 0) que corresponde a lleno
                balanceImage.sprite = balanceSprites[0];
            }
            balanceImage.preserveAspect = true;



            // 7.06 Barra de Potenciador
            Transform oldBooster = canvas.transform.Find("Barra_Potenciador");
            if (oldBooster != null)
            {
                UnityEngine.Object.DestroyImmediate(oldBooster.gameObject);
            }

            UnityEngine.UI.DefaultControls.Resources uiResources = new UnityEngine.UI.DefaultControls.Resources();
            GameObject boosterObj = new GameObject("Barra_Potenciador");
            boosterObj.transform.SetParent(canvas.transform, false);
            
            UnityEngine.UI.Image boosterImage = boosterObj.AddComponent<UnityEngine.UI.Image>();
            
            RectTransform boosterRect = boosterObj.GetComponent<RectTransform>();
            boosterRect.anchorMin = new Vector2(0f, 0f);
            boosterRect.anchorMax = new Vector2(0f, 0f);
            boosterRect.pivot = new Vector2(0.5f, 0f); // Mismo pivot de Y y de alineación que la barra de equilibrio
            // Tamaño de la barra de potenciador igual que la de equilibrio: 270x69
            boosterRect.sizeDelta = new Vector2(270f, 69f);
            boosterRect.anchoredPosition = new Vector2(170f, 40f); // 170 - (270/2) = 35 de margen izquierdo, alineación Y=40 idéntica

            // Cargar los sprites múltiples de la barra de potenciador
            string boosterSpritePath = "Assets/sprites/barra_potenciador.png";
            Sprite[] boosterSprites = AssetDatabase.LoadAllAssetsAtPath(boosterSpritePath)
                .OfType<Sprite>()
                .OrderBy(s => s.name)
                .ToArray();

            if (boosterSprites != null && boosterSprites.Length > 0)
            {
                boosterImage.sprite = boosterSprites[0];
            }
            boosterImage.preserveAspect = true;

            // Por defecto oculto en Edit Mode
            boosterObj.SetActive(false);

            AdministradorUI tempUiManager = canvas.gameObject.GetComponent<AdministradorUI>();
            if (tempUiManager == null) tempUiManager = canvas.gameObject.AddComponent<AdministradorUI>();

            // Asigna los nuevos campos por reflexión
            var balanceImageField = typeof(AdministradorUI).GetField("balanceImage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (balanceImageField != null) balanceImageField.SetValue(tempUiManager, balanceImage);

            var balanceSpritesField = typeof(AdministradorUI).GetField("balanceSprites", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (balanceSpritesField != null) balanceSpritesField.SetValue(tempUiManager, balanceSprites);

            var boosterImageField = typeof(AdministradorUI).GetField("boosterImage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (boosterImageField != null) boosterImageField.SetValue(tempUiManager, boosterImage);

            var boosterSpritesField = typeof(AdministradorUI).GetField("boosterSprites", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (boosterSpritesField != null) boosterSpritesField.SetValue(tempUiManager, boosterSprites);

            Transform oldMarco = canvas.transform.Find("Marco_HUD");
            if (oldMarco != null)
            {
                UnityEngine.Object.DestroyImmediate(oldMarco.gameObject);
            }

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

            Transform oldPausePlay = canvas.transform.Find("Boton_PausaPlay");
            if (oldPausePlay != null)
            {
                UnityEngine.Object.DestroyImmediate(oldPausePlay.gameObject);
            }

            Transform oldWinBtn = canvas.transform.Find("Boton_Ganar");
            if (oldWinBtn != null)
            {
                UnityEngine.Object.DestroyImmediate(oldWinBtn.gameObject);
            }

            // 1. Crear el Panel Marco/Borde para Vidas
            GameObject livesPanelObj = new GameObject("Marco_HUD", typeof(RectTransform));
            RectTransform panelRect = livesPanelObj.GetComponent<RectTransform>();
            panelRect.SetParent(canvas.transform, false);
            panelRect.anchorMin = new Vector2(0f, 1f);
            panelRect.anchorMax = new Vector2(0f, 1f);
            panelRect.pivot = new Vector2(0f, 1f);
            panelRect.anchoredPosition = new Vector2(35f, -35f);
            panelRect.sizeDelta = new Vector2(350f, 100f);

            // Obtener sprite de fondo redondeado estándar de Unity
            Sprite roundedBoxSprite = uiResources.background;

            // Agregar fondo oscuro redondeado del recuadro
            Image hudPanelImage = livesPanelObj.AddComponent<Image>();
            hudPanelImage.sprite = roundedBoxSprite;
            hudPanelImage.type = Image.Type.Sliced;
            hudPanelImage.color = new Color(0.08f, 0.08f, 0.08f, 0.85f); // Recuadro elegante y oscuro

            // Agregar sombra al recuadro para darle profundidad
            Shadow panelShadow = livesPanelObj.AddComponent<Shadow>();
            panelShadow.effectColor = new Color(0f, 0f, 0f, 0.5f);
            panelShadow.effectDistance = new Vector2(5f, -5f);

            // 2. Crear el contenedor interno de hamburguesas (centrado en el recuadro)
            GameObject livesContainerObj = new GameObject("Contenedor_Vidas", typeof(RectTransform));
            RectTransform rect = livesContainerObj.GetComponent<RectTransform>();
            rect.SetParent(panelRect, false);
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, 0f);
            rect.sizeDelta = new Vector2(-30f, -20f); // 15px de margen izquierdo/derecho, 10px arriba/abajo

            HorizontalLayoutGroup layout = livesContainerObj.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 15f;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            layout.childAlignment = TextAnchor.MiddleCenter;

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
                hRect.sizeDelta = new Vector2(90f, 66f);

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

            // 7.45 Crear el recuadro de monedas Marco_Monedas
            Transform oldMarcoMonedas = canvas.transform.Find("Marco_Monedas");
            if (oldMarcoMonedas != null)
            {
                UnityEngine.Object.DestroyImmediate(oldMarcoMonedas.gameObject);
            }

            GameObject coinsPanelObj = new GameObject("Marco_Monedas", typeof(RectTransform));
            RectTransform coinsPanelRect = coinsPanelObj.GetComponent<RectTransform>();
            coinsPanelRect.SetParent(canvas.transform, false);
            coinsPanelRect.anchorMin = new Vector2(0f, 1f);
            coinsPanelRect.anchorMax = new Vector2(0f, 1f);
            coinsPanelRect.pivot = new Vector2(0f, 1f);
            // Posicionado al lado de Marco_HUD (x: 35 + 350 + 15 = 400)
            coinsPanelRect.anchoredPosition = new Vector2(400f, -35f);
            coinsPanelRect.sizeDelta = new Vector2(180f, 100f);

            // Fondo redondeado estándar igual al de vidas
            Image coinsPanelImage = coinsPanelObj.AddComponent<Image>();
            coinsPanelImage.sprite = roundedBoxSprite;
            coinsPanelImage.type = Image.Type.Sliced;
            coinsPanelImage.color = new Color(0.08f, 0.08f, 0.08f, 0.85f);

            // Sombra
            Shadow coinsPanelShadow = coinsPanelObj.AddComponent<Shadow>();
            coinsPanelShadow.effectColor = new Color(0f, 0f, 0f, 0.5f);
            coinsPanelShadow.effectDistance = new Vector2(5f, -5f);

            // Agregar imagen/icono de moneda
            GameObject coinIconObj = new GameObject("Imagen_Icono_Moneda", typeof(RectTransform));
            coinIconObj.transform.SetParent(coinsPanelObj.transform, false);
            Image coinIconImage = coinIconObj.AddComponent<Image>();
            coinIconImage.preserveAspect = true;

            Sprite coinSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/sprites/moneda.png");
            if (coinSprite != null)
            {
                coinIconImage.sprite = coinSprite;
                coinIconImage.color = Color.white;
            }
            else
            {
                coinIconImage.color = Color.yellow; // Fallback
            }

            RectTransform coinIconRect = coinIconObj.GetComponent<RectTransform>();
            coinIconRect.anchorMin = new Vector2(0f, 0.5f);
            coinIconRect.anchorMax = new Vector2(0f, 0.5f);
            coinIconRect.pivot = new Vector2(0f, 0.5f);
            coinIconRect.anchoredPosition = new Vector2(15f, 0f);
            coinIconRect.sizeDelta = new Vector2(50f, 50f);

            // Crear el texto de las monedas Texto_Monedas
            GameObject coinsTextObj = new GameObject("Texto_Monedas", typeof(RectTransform));
            coinsTextObj.transform.SetParent(coinsPanelObj.transform, false);

            Text coinsText = coinsTextObj.AddComponent<Text>();
            
            Text anyText = canvas.GetComponentInChildren<Text>(true);
            if (anyText != null) coinsText.font = anyText.font;
            else coinsText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (coinsText.font == null) coinsText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            coinsText.fontSize = 28;
            coinsText.fontStyle = FontStyle.Bold;
            coinsText.color = new Color(1f, 0.84f, 0f); // Dorado
            coinsText.alignment = TextAnchor.MiddleLeft;
            coinsText.text = "0";

            RectTransform coinsRect = coinsTextObj.GetComponent<RectTransform>();
            coinsRect.anchorMin = new Vector2(0f, 0.5f);
            coinsRect.anchorMax = new Vector2(1f, 0.5f);
            coinsRect.pivot = new Vector2(0f, 0.5f);
            coinsRect.anchoredPosition = new Vector2(75f, 0f); // Ubicado a la derecha de la moneda (15 + 50 + 10 = 75)
            coinsRect.sizeDelta = new Vector2(-90f, 60f); // Ocupa el resto del espacio

            Shadow coinsShadow = coinsTextObj.AddComponent<Shadow>();
            coinsShadow.effectColor = Color.black;
            coinsShadow.effectDistance = new Vector2(1.5f, -1.5f);

            // 7.47 Crear el botón de pausa y play Boton_PausaPlay
            GameObject pauseBtnObj = new GameObject("Boton_PausaPlay", typeof(RectTransform));
            RectTransform pauseBtnRect = pauseBtnObj.GetComponent<RectTransform>();
            pauseBtnRect.SetParent(canvas.transform, false);
            pauseBtnRect.anchorMin = new Vector2(1f, 1f); // Esquina superior derecha
            pauseBtnRect.anchorMax = new Vector2(1f, 1f);
            pauseBtnRect.pivot = new Vector2(1f, 1f);
            pauseBtnRect.anchoredPosition = new Vector2(-35f, -35f); // 35px del borde
            pauseBtnRect.sizeDelta = new Vector2(65f, 65f); // Tamaño del botón

            Image pauseBtnImage = pauseBtnObj.AddComponent<Image>();
            Sprite spPausa = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/sprites/boton_pausa.png");
            Sprite spPlay = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/sprites/boton_play.png");
            if (spPausa != null)
            {
                pauseBtnImage.sprite = spPausa;
                pauseBtnImage.color = Color.white;
            }
            else
            {
                pauseBtnImage.color = Color.blue; // Fallback
            }

            Button pauseBtn = pauseBtnObj.AddComponent<Button>();

            // 7.5 Configurar el AdministradorUI
            AdministradorUI uiManager = canvas.GetComponent<AdministradorUI>();
            if (uiManager == null)
            {
                uiManager = canvas.gameObject.AddComponent<AdministradorUI>();
            }

            // Asignar los métodos a ejecutar al hacer click
            UnityEditor.Events.UnityEventTools.AddPersistentListener(pauseBtn.onClick, uiManager.AlternarPausa);

            // 7.48 Crear el botón de ganar Boton_Ganar
            GameObject winBtnObj = new GameObject("Boton_Ganar", typeof(RectTransform));
            RectTransform winBtnRect = winBtnObj.GetComponent<RectTransform>();
            winBtnRect.SetParent(canvas.transform, false);
            winBtnRect.anchorMin = new Vector2(1f, 0f); // Esquina inferior derecha
            winBtnRect.anchorMax = new Vector2(1f, 0f);
            winBtnRect.pivot = new Vector2(1f, 0f);
            winBtnRect.anchoredPosition = new Vector2(-50f, 50f);
            winBtnRect.sizeDelta = new Vector2(200f, 65f);

            Image winBtnImage = winBtnObj.AddComponent<Image>();
            EnsureIsSprite("Assets/sprites/boton_ganar.png");
            Sprite spGanar = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/sprites/boton_ganar.png");
            if (spGanar != null)
            {
                winBtnImage.sprite = spGanar;
                winBtnImage.color = Color.white;
            }
            else
            {
                winBtnImage.color = Color.yellow; // Fallback
            }

            Button winBtn = winBtnObj.AddComponent<Button>();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(winBtn.onClick, uiManager.ShowVictory);

            // Inyectar los sprites y el botón mediante reflexión en el AdministradorUI
            var pauseSpriteField = typeof(AdministradorUI).GetField("pauseSprite", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (pauseSpriteField != null && spPausa != null)
            {
                pauseSpriteField.SetValue(uiManager, spPausa);
            }
            var playSpriteField = typeof(AdministradorUI).GetField("playSprite", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (playSpriteField != null && spPlay != null)
            {
                playSpriteField.SetValue(uiManager, spPlay);
            }
            var pausePlayImageField = typeof(AdministradorUI).GetField("pausePlayButtonImage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (pausePlayImageField != null)
            {
                pausePlayImageField.SetValue(uiManager, pauseBtnImage);
            }
            EditorUtility.SetDirty(uiManager);

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

            // --- CONSTRUCCIÓN DEL LEADERBOARD EN GAMEOVER ---
            Font customFont = AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/RobotoCondensed-Bold.ttf");
            if (customFont == null)
            {
                Text foundText = canvas.GetComponentInChildren<Text>(true);
                customFont = foundText != null ? foundText.font : null;
                if (customFont == null)
                {
                    customFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                }
            }

            // Inyectar customFont en AdministradorUI
            var customFontField = typeof(AdministradorUI).GetField("customFont", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (customFontField != null)
            {
                customFontField.SetValue(uiManager, customFont);
            }

            // Contenedor de la tabla
            GameObject goLeaderboardContainer = new GameObject("Contenedor_TablaPosiciones", typeof(RectTransform));
            RectTransform goLeadRect = goLeaderboardContainer.GetComponent<RectTransform>();
            goLeadRect.SetParent(pRect, false);
            goLeadRect.anchorMin = new Vector2(0.5f, 0.5f);
            goLeadRect.anchorMax = new Vector2(0.5f, 0.5f);
            goLeadRect.pivot = new Vector2(0.5f, 0.5f);
            goLeadRect.anchoredPosition = new Vector2(0f, -90f); // Shipped down further to avoid overlapping background text
            goLeadRect.sizeDelta = new Vector2(500f, 360f); // Increased size from 420x320

            Image goLeadBg = goLeaderboardContainer.AddComponent<Image>();
            goLeadBg.sprite = roundedBoxSprite;
            goLeadBg.type = Image.Type.Sliced;
            goLeadBg.color = new Color(0.08f, 0.08f, 0.08f, 0.85f); // Fondo elegante y oscuro

            Shadow goLeadShadow = goLeaderboardContainer.AddComponent<Shadow>();
            goLeadShadow.effectColor = new Color(0f, 0f, 0f, 0.5f);
            goLeadShadow.effectDistance = new Vector2(5f, -5f);

            // Título
            GameObject goLeadTitleObj = new GameObject("GameOverLeaderboardTitle", typeof(RectTransform));
            RectTransform goLeadTitleRect = goLeadTitleObj.GetComponent<RectTransform>();
            goLeadTitleRect.SetParent(goLeadRect, false);
            goLeadTitleRect.anchorMin = new Vector2(0f, 1f);
            goLeadTitleRect.anchorMax = new Vector2(1f, 1f);
            goLeadTitleRect.pivot = new Vector2(0.5f, 1f);
            goLeadTitleRect.anchoredPosition = new Vector2(0f, -20f); // Adjusted from -15f
            goLeadTitleRect.sizeDelta = new Vector2(0f, 35f); // Adjusted from 30f

            Text goLeadTitleText = goLeadTitleObj.AddComponent<Text>();
            goLeadTitleText.font = customFont;
            goLeadTitleText.fontSize = 26; // Increased from 22
            goLeadTitleText.color = new Color(1f, 0.84f, 0f); // Dorado
            goLeadTitleText.alignment = TextAnchor.MiddleCenter;
            goLeadTitleText.text = "TABLA DE POSICIONES";

            // Texto de posiciones
            GameObject goLeadTextObj = new GameObject("Texto_Tabla", typeof(RectTransform));
            RectTransform goLeadTextRect = goLeadTextObj.GetComponent<RectTransform>();
            goLeadTextRect.SetParent(goLeadRect, false);
            goLeadTextRect.anchorMin = new Vector2(0f, 1f);
            goLeadTextRect.anchorMax = new Vector2(1f, 1f);
            goLeadTextRect.pivot = new Vector2(0.5f, 1f);
            goLeadTextRect.anchoredPosition = new Vector2(0f, -65f); // Adjusted from -50f
            goLeadTextRect.sizeDelta = new Vector2(-50f, 180f); // Adjusted from -40x140

            Text goLeadText = goLeadTextObj.AddComponent<Text>();
            goLeadText.font = customFont;
            goLeadText.fontSize = 22; // Increased from 18
            goLeadText.color = Color.white;
            goLeadText.alignment = TextAnchor.UpperCenter;
            goLeadText.text = "Cargando posiciones...";

            // InputField para el nombre
            GameObject goLeadInputObj = new GameObject("InputField_Nombre", typeof(RectTransform));
            RectTransform goLeadInputRect = goLeadInputObj.GetComponent<RectTransform>();
            goLeadInputRect.SetParent(goLeadRect, false);
            goLeadInputRect.anchorMin = new Vector2(0f, 0f);
            goLeadInputRect.anchorMax = new Vector2(0f, 0f);
            goLeadInputRect.pivot = new Vector2(0f, 0f);
            goLeadInputRect.anchoredPosition = new Vector2(25f, 25f); // Adjusted from 20x20
            goLeadInputRect.sizeDelta = new Vector2(300f, 50f); // Adjusted from 250x45

            Image goLeadInputBg = goLeadInputObj.AddComponent<Image>();
            goLeadInputBg.sprite = roundedBoxSprite;
            goLeadInputBg.type = Image.Type.Sliced;
            goLeadInputBg.color = new Color(0.15f, 0.15f, 0.15f, 1f);

            InputField goLeadInput = goLeadInputObj.AddComponent<InputField>();

            // Texto del input
            GameObject goLeadInputTextObj = new GameObject("Text", typeof(RectTransform));
            RectTransform goLeadInputTextRect = goLeadInputTextObj.GetComponent<RectTransform>();
            goLeadInputTextRect.SetParent(goLeadInputRect, false);
            goLeadInputTextRect.anchorMin = Vector2.zero;
            goLeadInputTextRect.anchorMax = Vector2.one;
            goLeadInputTextRect.offsetMin = new Vector2(10f, 5f);
            goLeadInputTextRect.offsetMax = new Vector2(-10f, -5f);

            Text goLeadInputTextComp = goLeadInputTextObj.AddComponent<Text>();
            goLeadInputTextComp.font = customFont;
            goLeadInputTextComp.fontSize = 20; // Increased from 18
            goLeadInputTextComp.color = Color.white;
            goLeadInputTextComp.alignment = TextAnchor.MiddleLeft;
            goLeadInputTextComp.supportRichText = false;

            // Placeholder del input
            GameObject goLeadInputPlaceholderObj = new GameObject("Placeholder", typeof(RectTransform));
            RectTransform goLeadInputPlaceholderRect = goLeadInputPlaceholderObj.GetComponent<RectTransform>();
            goLeadInputPlaceholderRect.SetParent(goLeadInputRect, false);
            goLeadInputPlaceholderRect.anchorMin = Vector2.zero;
            goLeadInputPlaceholderRect.anchorMax = Vector2.one;
            goLeadInputPlaceholderRect.offsetMin = new Vector2(10f, 5f);
            goLeadInputPlaceholderRect.offsetMax = new Vector2(-10f, -5f);

            Text goLeadInputPlaceholderComp = goLeadInputPlaceholderObj.AddComponent<Text>();
            goLeadInputPlaceholderComp.font = customFont;
            goLeadInputPlaceholderComp.fontSize = 20; // Increased from 18
            goLeadInputPlaceholderComp.fontStyle = FontStyle.Italic;
            goLeadInputPlaceholderComp.color = new Color(0.6f, 0.6f, 0.6f, 0.5f);
            goLeadInputPlaceholderComp.alignment = TextAnchor.MiddleLeft;
            goLeadInputPlaceholderComp.text = "Ingresa tu nombre...";

            goLeadInput.textComponent = goLeadInputTextComp;
            goLeadInput.placeholder = goLeadInputPlaceholderComp;
            goLeadInput.targetGraphic = goLeadInputBg;

            // Botón Guardar
            GameObject goLeadSaveBtnObj = new GameObject("Boton_Guardar", typeof(RectTransform));
            RectTransform goLeadSaveBtnRect = goLeadSaveBtnObj.GetComponent<RectTransform>();
            goLeadSaveBtnRect.SetParent(goLeadRect, false);
            goLeadSaveBtnRect.anchorMin = new Vector2(0f, 0f);
            goLeadSaveBtnRect.anchorMax = new Vector2(0f, 0f);
            goLeadSaveBtnRect.pivot = new Vector2(0f, 0f);
            goLeadSaveBtnRect.anchoredPosition = new Vector2(340f, 25f); // Adjusted from 285x20
            goLeadSaveBtnRect.sizeDelta = new Vector2(135f, 50f); // Adjusted from 115x45

            Image goLeadSaveBtnBg = goLeadSaveBtnObj.AddComponent<Image>();
            goLeadSaveBtnBg.sprite = roundedBoxSprite;
            goLeadSaveBtnBg.type = Image.Type.Sliced;
            goLeadSaveBtnBg.color = new Color(1f, 0.84f, 0f); // Dorado/Amarillo

            Button goLeadSaveBtn = goLeadSaveBtnObj.AddComponent<Button>();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(goLeadSaveBtn.onClick, uiManager.SaveGameOverScore);

            GameObject goLeadSaveBtnTextObj = new GameObject("Texto", typeof(RectTransform));
            RectTransform goLeadSaveBtnTextRect = goLeadSaveBtnTextObj.GetComponent<RectTransform>();
            goLeadSaveBtnTextRect.SetParent(goLeadSaveBtnRect, false);
            goLeadSaveBtnTextRect.anchorMin = Vector2.zero;
            goLeadSaveBtnTextRect.anchorMax = Vector2.one;
            goLeadSaveBtnTextRect.offsetMin = Vector2.zero;
            goLeadSaveBtnTextRect.offsetMax = Vector2.zero;

            Text goLeadSaveBtnText = goLeadSaveBtnTextObj.AddComponent<Text>();
            goLeadSaveBtnText.font = customFont;
            goLeadSaveBtnText.fontSize = 20; // Increased from 16
            goLeadSaveBtnText.fontStyle = FontStyle.Bold;
            goLeadSaveBtnText.color = new Color(0.08f, 0.08f, 0.08f, 1f); // Contraste oscuro
            goLeadSaveBtnText.alignment = TextAnchor.MiddleCenter;
            goLeadSaveBtnText.text = "GUARDAR";

            // Inyectar referencias en AdministradorUI por reflexión
            var gameOverNameInputFieldField = typeof(AdministradorUI).GetField("gameOverNameInputField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (gameOverNameInputFieldField != null) gameOverNameInputFieldField.SetValue(uiManager, goLeadInput);

            var gameOverSaveButtonField = typeof(AdministradorUI).GetField("gameOverSaveButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (gameOverSaveButtonField != null) gameOverSaveButtonField.SetValue(uiManager, goLeadSaveBtn);

            var gameOverLeaderboardTextField = typeof(AdministradorUI).GetField("gameOverLeaderboardText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (gameOverLeaderboardTextField != null) gameOverLeaderboardTextField.SetValue(uiManager, goLeadText);

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
            startPanelRect.pivot = new Vector2(0.5f, 0.5f);
            startPanelRect.anchoredPosition = Vector2.zero;
            startPanelRect.localScale = Vector3.one;
            startPanelRect.localRotation = Quaternion.identity;

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
            EnsureIsSprite("Assets/sprites/boton_instrucciones.png");
            EnsureIsSprite("Assets/sprites/boton_ganar.png");

            Sprite spriteJugar = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/sprites/boton_jugar.png");
            Sprite spriteMapa = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/sprites/boton_mapa.png");
            Sprite spriteConfiguracion = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/sprites/boton_configuracion.png");
            Sprite spriteInstrucciones = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/sprites/boton_instrucciones.png");

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
            UnityEditor.Events.UnityEventTools.AddPersistentListener(btnJugar.onClick, uiManager.AbrirDetallePedido);

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
            UnityEditor.Events.UnityEventTools.AddPersistentListener(btnMapa.onClick, uiManager.AbrirMapa);

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
            UnityEditor.Events.UnityEventTools.AddPersistentListener(btnConfig.onClick, uiManager.AbrirConfiguracion);

            // Crear Botón "Boton_instrucciones" (Rojo, esquina superior derecha)
            GameObject btnInstruccionesObj = new GameObject("Boton_instrucciones", typeof(RectTransform));
            RectTransform btnInstruccionesRect = btnInstruccionesObj.GetComponent<RectTransform>();
            btnInstruccionesRect.SetParent(startPanelRect, false);
            btnInstruccionesRect.anchorMin = new Vector2(1f, 1f); // Esquina superior derecha
            btnInstruccionesRect.anchorMax = new Vector2(1f, 1f);
            btnInstruccionesRect.pivot = new Vector2(1f, 1f);
            btnInstruccionesRect.anchoredPosition = new Vector2(-60f, -60f); // Desplazamiento desde el borde
            btnInstruccionesRect.sizeDelta = new Vector2(90f, 90f); // Proporcional en píxeles

            Image btnInstruccionesImg = btnInstruccionesObj.AddComponent<Image>();
            if (spriteInstrucciones != null)
            {
                btnInstruccionesImg.sprite = spriteInstrucciones;
                btnInstruccionesImg.color = Color.white;
            }
            else
            {
                btnInstruccionesImg.color = Color.red;
            }

            Button btnInstrucciones = btnInstruccionesObj.AddComponent<Button>();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(btnInstrucciones.onClick, uiManager.AbrirInstrucciones);

            // Asignar el startPanel en el AdministradorUI por reflexión
            var startPanelField = typeof(AdministradorUI).GetField("startPanel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (startPanelField != null)
            {
                startPanelField.SetValue(uiManager, startPanelObj);
                EditorUtility.SetDirty(uiManager);
                Debug.Log("✅ startPanel inyectado en AdministradorUI.");
            }

            // 7.6.1 Crear o buscar ConfigPanel
            Transform oldConfigPanel = canvas.transform.Find("ConfigPanel");
            if (oldConfigPanel != null)
            {
                UnityEngine.Object.DestroyImmediate(oldConfigPanel.gameObject);
            }

            GameObject configPanelObj = new GameObject("ConfigPanel", typeof(RectTransform));
            RectTransform configPanelRect = configPanelObj.GetComponent<RectTransform>();
            configPanelRect.SetParent(canvas.transform, false);
            configPanelObj.SetActive(false); // Empieza desactivado

            // Set ConfigPanel to cover the whole screen with an average -5.5px X shift to align backgrounds perfectly
            configPanelRect.anchorMin = Vector2.zero;
            configPanelRect.anchorMax = Vector2.one;
            configPanelRect.offsetMin = new Vector2(-5.5f, 0f);
            configPanelRect.offsetMax = new Vector2(-5.5f, 0f);
            configPanelRect.pivot = new Vector2(0.5f, 0.5f);
            configPanelRect.localScale = Vector3.one;
            configPanelRect.localRotation = Quaternion.identity;

            Image configPanelImage = configPanelObj.AddComponent<Image>();

            // Cargar los 4 sprites para configuración
            string pathBoth = "Assets/sprites/imagen_config.jpg";
            string pathNoMusic = "Assets/sprites/imagen_nomusica.jpg";
            string pathNoSound = "Assets/sprites/imagen_nosonido.jpg";
            string pathNone = "Assets/sprites/imagen_noambas.jpg";

            EnsureIsSprite(pathBoth);
            EnsureIsSprite(pathNoMusic);
            EnsureIsSprite(pathNoSound);
            EnsureIsSprite(pathNone);

            Sprite spriteBoth = LoadSprite(pathBoth);
            Sprite spriteNoMusic = LoadSprite(pathNoMusic);
            Sprite spriteNoSound = LoadSprite(pathNoSound);
            Sprite spriteNone = LoadSprite(pathNone);

            if (spriteBoth != null)
            {
                configPanelImage.sprite = spriteBoth;
                configPanelImage.color = Color.white;
            }
            else
            {
                configPanelImage.color = new Color(0.2f, 0.2f, 0.2f, 0.95f);
            }

            // Cargar los sprites de los iconos de configuración
            EnsureIsSprite("Assets/sprites/imagen_usuario.png");
            EnsureIsSprite("Assets/sprites/boton_musica.png");
            EnsureIsSprite("Assets/sprites/boton_nomusica.png");
            EnsureIsSprite("Assets/sprites/boton_sonido.png");
            EnsureIsSprite("Assets/sprites/boton_nosonido.png");

            Sprite spriteUser = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/sprites/imagen_usuario.png");
            Sprite spriteMusicOn = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/sprites/boton_musica.png");
            Sprite spriteMusicOff = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/sprites/boton_nomusica.png");
            Sprite spriteSoundOn = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/sprites/boton_sonido.png");
            Sprite spriteSoundOff = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/sprites/boton_nosonido.png");

            // Cargar fuente estándar para los textos de la interfaz
            Font standardFont = null;
            Text existingText = canvas.GetComponentInChildren<Text>(true);
            if (existingText != null)
            {
                standardFont = existingText.font;
            }
            if (standardFont == null)
            {
                standardFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }
            if (standardFont == null)
            {
                standardFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }

            // Crear GameObject para el Icono de Usuario (X: -251.5, Y: 110.0, size 84x88)
            GameObject iconUserObj = new GameObject("IconoUsuario", typeof(RectTransform));
            RectTransform iconUserRect = iconUserObj.GetComponent<RectTransform>();
            iconUserRect.SetParent(configPanelRect, false);
            iconUserRect.anchorMin = new Vector2(0.5f, 0.5f);
            iconUserRect.anchorMax = new Vector2(0.5f, 0.5f);
            iconUserRect.pivot = new Vector2(0.5f, 0.5f);
            iconUserRect.anchoredPosition = new Vector2(-251.5f, 110.0f);
            iconUserRect.sizeDelta = new Vector2(84f, 88f);
            Image iconUserImg = iconUserObj.AddComponent<Image>();
            if (spriteUser != null)
            {
                iconUserImg.sprite = spriteUser;
                iconUserImg.color = Color.white;
            }

            // Crear GameObject para el Icono de Música (X: -251.5, Y: -40.0, size 84x88)
            GameObject iconMusicObj = new GameObject("IconoMusica", typeof(RectTransform));
            RectTransform iconMusicRect = iconMusicObj.GetComponent<RectTransform>();
            iconMusicRect.SetParent(configPanelRect, false);
            iconMusicRect.anchorMin = new Vector2(0.5f, 0.5f);
            iconMusicRect.anchorMax = new Vector2(0.5f, 0.5f);
            iconMusicRect.pivot = new Vector2(0.5f, 0.5f);
            iconMusicRect.anchoredPosition = new Vector2(-251.5f, -40.0f);
            iconMusicRect.sizeDelta = new Vector2(84f, 88f);
            Image iconMusicImg = iconMusicObj.AddComponent<Image>();
            bool startMusicOn = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;
            if (startMusicOn && spriteMusicOn != null)
            {
                iconMusicImg.sprite = spriteMusicOn;
            }
            else if (!startMusicOn && spriteMusicOff != null)
            {
                iconMusicImg.sprite = spriteMusicOff;
            }
            iconMusicImg.color = Color.white;
            Button btnIconoMusica = iconMusicObj.AddComponent<Button>();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(btnIconoMusica.onClick, uiManager.ToggleMusica);

            // Crear GameObject para el Icono de Sonido (X: -251.5, Y: -200.0, size 84x88)
            GameObject iconSoundObj = new GameObject("IconoSonido", typeof(RectTransform));
            RectTransform iconSoundRect = iconSoundObj.GetComponent<RectTransform>();
            iconSoundRect.SetParent(configPanelRect, false);
            iconSoundRect.anchorMin = new Vector2(0.5f, 0.5f);
            iconSoundRect.anchorMax = new Vector2(0.5f, 0.5f);
            iconSoundRect.pivot = new Vector2(0.5f, 0.5f);
            iconSoundRect.anchoredPosition = new Vector2(-251.5f, -200.0f);
            iconSoundRect.sizeDelta = new Vector2(84f, 88f);
            Image iconSoundImg = iconSoundObj.AddComponent<Image>();
            bool startSoundOn = PlayerPrefs.GetInt("SoundEnabled", 1) == 1;
            if (startSoundOn && spriteSoundOn != null)
            {
                iconSoundImg.sprite = spriteSoundOn;
            }
            else if (!startSoundOn && spriteSoundOff != null)
            {
                iconSoundImg.sprite = spriteSoundOff;
            }
            iconSoundImg.color = Color.white;
            Button btnIconoSonido = iconSoundObj.AddComponent<Button>();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(btnIconoSonido.onClick, uiManager.ToggleSonido);

            // Botón de Cerrar (Círculo/Cuadrado rojo arriba a la derecha de la ventana blanca)
            // X aproximada: 333, Y aproximada: 259
            GameObject btnCerrarObj = new GameObject("BotonCerrar", typeof(RectTransform));
            RectTransform btnCerrarRect = btnCerrarObj.GetComponent<RectTransform>();
            btnCerrarRect.SetParent(configPanelRect, false);
            btnCerrarRect.anchorMin = new Vector2(0.5f, 0.5f);
            btnCerrarRect.anchorMax = new Vector2(0.5f, 0.5f);
            btnCerrarRect.pivot = new Vector2(0.5f, 0.5f);
            btnCerrarRect.anchoredPosition = new Vector2(333f, 259f);
            btnCerrarRect.sizeDelta = new Vector2(85f, 85f); // Tamaño unificado a 85x85

            Image btnCerrarImg = btnCerrarObj.AddComponent<Image>();
            EnsureIsSprite("Assets/sprites/boton_cerrar.png");
            Sprite spriteCerrar = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/sprites/boton_cerrar.png");
            if (spriteCerrar != null)
            {
                btnCerrarImg.sprite = spriteCerrar;
                btnCerrarImg.color = Color.white;
            }
            else
            {
                btnCerrarImg.color = Color.red;
            }

            Button btnCerrar = btnCerrarObj.AddComponent<Button>();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(btnCerrar.onClick, uiManager.CerrarConfiguracion);

            // Botón de Música (Clic invisible sobre la fila de música, X: 0, Y: -40.0, 500x90)
            GameObject btnMusicaObj = new GameObject("BotonMusica", typeof(RectTransform));
            RectTransform btnMusicaRect = btnMusicaObj.GetComponent<RectTransform>();
            btnMusicaRect.SetParent(configPanelRect, false);
            btnMusicaRect.anchorMin = new Vector2(0.5f, 0.5f);
            btnMusicaRect.anchorMax = new Vector2(0.5f, 0.5f);
            btnMusicaRect.pivot = new Vector2(0.5f, 0.5f);
            btnMusicaRect.anchoredPosition = new Vector2(0f, -40.0f);
            btnMusicaRect.sizeDelta = new Vector2(500f, 90f);

            Image btnMusicaImg = btnMusicaObj.AddComponent<Image>();
            btnMusicaImg.color = Color.clear; // Clic zone transparente
            Button btnMusica = btnMusicaObj.AddComponent<Button>();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(btnMusica.onClick, uiManager.ToggleMusica);

            // Botón de Sonido (Clic invisible sobre la fila de sonido, X: 0, Y: -200.0, 500x90)
            GameObject btnSonidoObj = new GameObject("BotonSonido", typeof(RectTransform));
            RectTransform btnSonidoRect = btnSonidoObj.GetComponent<RectTransform>();
            btnSonidoRect.SetParent(configPanelRect, false);
            btnSonidoRect.anchorMin = new Vector2(0.5f, 0.5f);
            btnSonidoRect.anchorMax = new Vector2(0.5f, 0.5f);
            btnSonidoRect.pivot = new Vector2(0.5f, 0.5f);
            btnSonidoRect.anchoredPosition = new Vector2(0f, -200.0f);
            btnSonidoRect.sizeDelta = new Vector2(500f, 90f);

            Image btnSonidoImg = btnSonidoObj.AddComponent<Image>();
            btnSonidoImg.color = Color.clear; // Clic zone transparente
            Button btnSonido = btnSonidoObj.AddComponent<Button>();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(btnSonido.onClick, uiManager.ToggleSonido);

            // Máscara blanca para tapar el "User123" pre-renderizado del fondo
            GameObject maskObj = new GameObject("Mascara_Texto_Usuario", typeof(RectTransform));
            RectTransform maskRect = maskObj.GetComponent<RectTransform>();
            maskRect.SetParent(configPanelRect, false);
            maskRect.anchorMin = new Vector2(0.5f, 0.5f);
            maskRect.anchorMax = new Vector2(0.5f, 0.5f);
            maskRect.pivot = new Vector2(0.5f, 0.5f);
            maskRect.anchoredPosition = new Vector2(-145f, 58.0f);
            maskRect.sizeDelta = new Vector2(100f, 35f);
            Image maskImg = maskObj.AddComponent<Image>();
            maskImg.color = Color.white;

            // InputField para nombre de usuario (X: -94, Y: 58.0, tamaño 200x40)
            GameObject inputFieldObj = new GameObject("InputField_Usuario", typeof(RectTransform));
            RectTransform inputFieldRect = inputFieldObj.GetComponent<RectTransform>();
            inputFieldRect.SetParent(configPanelRect, false);
            inputFieldRect.anchorMin = new Vector2(0.5f, 0.5f);
            inputFieldRect.anchorMax = new Vector2(0.5f, 0.5f);
            inputFieldRect.pivot = new Vector2(0.5f, 0.5f);
            inputFieldRect.anchoredPosition = new Vector2(-94f, 58.0f);
            inputFieldRect.sizeDelta = new Vector2(200f, 40f);

            Image inputFieldBg = inputFieldObj.AddComponent<Image>();
            inputFieldBg.color = Color.clear; // Fondo transparente
            InputField inputField = inputFieldObj.AddComponent<InputField>();

            // Texto de InputField
            GameObject textObj = new GameObject("Text", typeof(RectTransform));
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.SetParent(inputFieldRect, false);
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            Text textComp = textObj.AddComponent<Text>();
            textComp.font = standardFont;
            textComp.fontSize = 24;
            textComp.color = new Color(0.15f, 0.15f, 0.15f, 1f); // Gris oscuro
            textComp.alignment = TextAnchor.MiddleLeft;

            inputField.textComponent = textComp;
            inputField.targetGraphic = inputFieldBg;

            // Las máscaras de texto se eliminaron porque el fondo cambia de textura dinámicamente y expone los textos Si/No correspondientes.

            // Inyectar referencias en AdministradorUI
            var configPanelField = typeof(AdministradorUI).GetField("configPanel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (configPanelField != null) configPanelField.SetValue(uiManager, configPanelObj);

            var configBgField = typeof(AdministradorUI).GetField("configBackgroundImage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (configBgField != null) configBgField.SetValue(uiManager, configPanelImage);

            var imgConfigBothField = typeof(AdministradorUI).GetField("imgConfigBoth", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (imgConfigBothField != null) imgConfigBothField.SetValue(uiManager, spriteBoth);

            var imgConfigNoMusicField = typeof(AdministradorUI).GetField("imgConfigNoMusic", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (imgConfigNoMusicField != null) imgConfigNoMusicField.SetValue(uiManager, spriteNoMusic);

            var imgConfigNoSoundField = typeof(AdministradorUI).GetField("imgConfigNoSound", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (imgConfigNoSoundField != null) imgConfigNoSoundField.SetValue(uiManager, spriteNoSound);

            var imgConfigNoneField = typeof(AdministradorUI).GetField("imgConfigNone", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (imgConfigNoneField != null) imgConfigNoneField.SetValue(uiManager, spriteNone);

            var inputFieldField = typeof(AdministradorUI).GetField("usernameInputField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (inputFieldField != null) inputFieldField.SetValue(uiManager, inputField);

            // Inyectar referencias de iconos e imágenes en AdministradorUI
            var musicIconImageField = typeof(AdministradorUI).GetField("musicIconImage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (musicIconImageField != null) musicIconImageField.SetValue(uiManager, iconMusicImg);

            var soundIconImageField = typeof(AdministradorUI).GetField("soundIconImage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (soundIconImageField != null) soundIconImageField.SetValue(uiManager, iconSoundImg);

            var iconMusicOnField = typeof(AdministradorUI).GetField("iconMusicOn", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (iconMusicOnField != null) iconMusicOnField.SetValue(uiManager, spriteMusicOn);

            var iconMusicOffField = typeof(AdministradorUI).GetField("iconMusicOff", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (iconMusicOffField != null) iconMusicOffField.SetValue(uiManager, spriteMusicOff);

            var iconSoundOnField = typeof(AdministradorUI).GetField("iconSoundOn", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (iconSoundOnField != null) iconSoundOnField.SetValue(uiManager, spriteSoundOn);

            var iconSoundOffField = typeof(AdministradorUI).GetField("iconSoundOff", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (iconSoundOffField != null) iconSoundOffField.SetValue(uiManager, spriteSoundOff);

            var musicStateTextField = typeof(AdministradorUI).GetField("musicStateText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (musicStateTextField != null) musicStateTextField.SetValue(uiManager, null);

            var soundStateTextField = typeof(AdministradorUI).GetField("soundStateText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (soundStateTextField != null) soundStateTextField.SetValue(uiManager, null);

            EditorUtility.SetDirty(uiManager);
            Debug.Log("✅ ConfigPanel creado e inyectado en AdministradorUI.");

            // 7.6.2 Crear o buscar PanelInstrucciones (InstructionsPanel)
            Transform oldInstructionsPanel = canvas.transform.Find("PanelInstrucciones");
            if (oldInstructionsPanel != null)
            {
                UnityEngine.Object.DestroyImmediate(oldInstructionsPanel.gameObject);
            }

            GameObject instructionsPanelObj = new GameObject("PanelInstrucciones", typeof(RectTransform));
            RectTransform instructionsPanelRect = instructionsPanelObj.GetComponent<RectTransform>();
            instructionsPanelRect.SetParent(canvas.transform, false);
            instructionsPanelObj.SetActive(false); // Empieza oculto

            // Set to cover the whole screen with a slight 10px overflow to prevent rounding gap lines at the edges
            instructionsPanelRect.anchorMin = Vector2.zero;
            instructionsPanelRect.anchorMax = Vector2.one;
            instructionsPanelRect.offsetMin = new Vector2(-10f, -10f);
            instructionsPanelRect.offsetMax = new Vector2(10f, 10f);
            instructionsPanelRect.pivot = new Vector2(0.5f, 0.5f);
            instructionsPanelRect.localScale = Vector3.one;
            instructionsPanelRect.localRotation = Quaternion.identity;

            // Fondo semitransparente oscuro detrás
            Image instructionsPanelBg = instructionsPanelObj.AddComponent<Image>();
            instructionsPanelBg.color = new Color(0f, 0f, 0f, 0.6f); // 60% opaco

            // Crear el panel de contenido (Popup)
            GameObject popupObj = new GameObject("Contenido", typeof(RectTransform));
            RectTransform popupRect = popupObj.GetComponent<RectTransform>();
            popupRect.SetParent(instructionsPanelRect, false);
            
            // Centrado en pantalla con AspectRatioFitter para escalar exactamente con la imagen de fondo 16:9
            popupRect.anchorMin = new Vector2(0.5f, 0.5f);
            popupRect.anchorMax = new Vector2(0.5f, 0.5f);
            popupRect.pivot = new Vector2(0.5f, 0.5f);
            popupRect.sizeDelta = new Vector2(1920f, 1080f);
            popupRect.anchoredPosition = Vector2.zero;
            popupRect.localScale = Vector3.one; // Mantener la escala original para evitar desfasamiento del botón y distorsión de la UI

            UnityEngine.UI.AspectRatioFitter aspectFitter = popupObj.AddComponent<UnityEngine.UI.AspectRatioFitter>();
            aspectFitter.aspectMode = UnityEngine.UI.AspectRatioFitter.AspectMode.FitInParent;
            aspectFitter.aspectRatio = 1920f / 1080f;

            Image popupImg = popupObj.AddComponent<Image>();
            
            EnsureIsSprite("Assets/sprites/imagen_instrucciones.png");
            Sprite spriteInstruccionesContent = null;
            var subAssetsInst = AssetDatabase.LoadAllAssetsAtPath("Assets/sprites/imagen_instrucciones.png");
            if (subAssetsInst != null)
            {
                spriteInstruccionesContent = subAssetsInst.OfType<Sprite>().FirstOrDefault(s => s.name == "imagen_instrucciones_0" || s.name.EndsWith("_0"));
            }
            if (spriteInstruccionesContent == null)
            {
                spriteInstruccionesContent = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/sprites/imagen_instrucciones.png");
            }

            if (spriteInstruccionesContent != null)
            {
                popupImg.sprite = spriteInstruccionesContent;
                popupImg.color = Color.white;
                popupImg.preserveAspect = true; // Para que mantenga la proporción 16:9 en cualquier resolución
            }

            // Crear Botón "BotonCerrar" (Rojo con X, arriba a la derecha de la tarjeta)
            // En base a un canvas de referencia 1920x1080, el botón se posiciona en X: 535, Y: 335 relativo al centro de la pantalla
            GameObject btnCerrarInstObj = new GameObject("BotonCerrar", typeof(RectTransform));
            RectTransform btnCerrarInstRect = btnCerrarInstObj.GetComponent<RectTransform>();
            btnCerrarInstRect.SetParent(popupRect, false);
            btnCerrarInstRect.anchorMin = new Vector2(0.7786f, 0.8102f); // Ancla proporcional X en el canvas base
            btnCerrarInstRect.anchorMax = new Vector2(0.7786f, 0.8102f); // Ancla proporcional Y en el canvas base
            btnCerrarInstRect.pivot = new Vector2(0.5f, 0.5f);
            btnCerrarInstRect.anchoredPosition = Vector2.zero; // Posición exacta en el ancla proporcional
            btnCerrarInstRect.sizeDelta = new Vector2(85f, 85f); // Tamaño cuadrado

            EnsureIsSprite("Assets/sprites/boton_cerrar.png");
            Sprite spriteCerrarInst = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/sprites/boton_cerrar.png");
            Image btnCerrarInstImg = btnCerrarInstObj.AddComponent<Image>();
            if (spriteCerrarInst != null)
            {
                btnCerrarInstImg.sprite = spriteCerrarInst;
                btnCerrarInstImg.color = Color.white;
            }
            else
            {
                btnCerrarInstImg.color = Color.red;
            }

            Button btnCerrarInst = btnCerrarInstObj.AddComponent<Button>();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(btnCerrarInst.onClick, uiManager.CerrarInstrucciones);

            // Inyectar el panel de instrucciones en el AdministradorUI por reflexión
            var instructionsPanelField = typeof(AdministradorUI).GetField("instructionsPanel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (instructionsPanelField != null)
            {
                instructionsPanelField.SetValue(uiManager, instructionsPanelObj);
                EditorUtility.SetDirty(uiManager);
                Debug.Log("✅ instructionsPanel inyectado en AdministradorUI.");
            }

            // 7.6.3 Crear o buscar PanelMapa
            Transform oldMapPanel = canvas.transform.Find("PanelMapa");
            if (oldMapPanel != null)
            {
                UnityEngine.Object.DestroyImmediate(oldMapPanel.gameObject);
            }

            GameObject mapPanelObj = new GameObject("PanelMapa", typeof(RectTransform));
            RectTransform mapPanelRect = mapPanelObj.GetComponent<RectTransform>();
            mapPanelRect.SetParent(canvas.transform, false);
            mapPanelObj.SetActive(false); // Empieza oculto

            // Set to cover the whole screen with a slight 10px overflow to prevent rounding gap lines at the edges
            mapPanelRect.anchorMin = Vector2.zero;
            mapPanelRect.anchorMax = Vector2.one;
            mapPanelRect.offsetMin = new Vector2(-10f, -10f);
            mapPanelRect.offsetMax = new Vector2(10f, 10f);
            mapPanelRect.pivot = new Vector2(0.5f, 0.5f);
            mapPanelRect.localScale = Vector3.one;
            mapPanelRect.localRotation = Quaternion.identity;

            // Fondo semitransparente oscuro detrás
            Image mapPanelBg = mapPanelObj.AddComponent<Image>();
            mapPanelBg.color = new Color(0f, 0f, 0f, 0.6f); // 60% opaco

            // Crear el panel de contenido (Popup)
            GameObject mapPopupObj = new GameObject("Contenido", typeof(RectTransform));
            RectTransform mapPopupRect = mapPopupObj.GetComponent<RectTransform>();
            mapPopupRect.SetParent(mapPanelRect, false);
            
            // Centrado en pantalla con AspectRatioFitter para escalar exactamente con la imagen de fondo 16:9
            mapPopupRect.anchorMin = new Vector2(0.5f, 0.5f);
            mapPopupRect.anchorMax = new Vector2(0.5f, 0.5f);
            mapPopupRect.pivot = new Vector2(0.5f, 0.5f);
            mapPopupRect.sizeDelta = new Vector2(1920f, 1080f);
            mapPopupRect.anchoredPosition = Vector2.zero;
            mapPopupRect.localScale = Vector3.one;

            UnityEngine.UI.AspectRatioFitter mapAspectFitter = mapPopupObj.AddComponent<UnityEngine.UI.AspectRatioFitter>();
            mapAspectFitter.aspectMode = UnityEngine.UI.AspectRatioFitter.AspectMode.FitInParent;
            mapAspectFitter.aspectRatio = 1920f / 1080f;

            Image mapPopupImg = mapPopupObj.AddComponent<Image>();
            
            EnsureIsSprite("Assets/sprites/pantalla_mapa.png");
            Sprite spriteMapContent = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/sprites/pantalla_mapa.png");
            if (spriteMapContent != null)
            {
                mapPopupImg.sprite = spriteMapContent;
                mapPopupImg.color = Color.white;
                mapPopupImg.preserveAspect = true;
            }

            // Crear Botón "BotonCerrar" (Rojo con X, arriba a la derecha de la tarjeta)
            GameObject btnCerrarMapObj = new GameObject("BotonCerrar", typeof(RectTransform));
            RectTransform btnCerrarMapRect = btnCerrarMapObj.GetComponent<RectTransform>();
            btnCerrarMapRect.SetParent(mapPopupRect, false);
            btnCerrarMapRect.anchorMin = new Vector2(0.7994f, 0.8630f);
            btnCerrarMapRect.anchorMax = new Vector2(0.7994f, 0.8630f);
            btnCerrarMapRect.pivot = new Vector2(0.5f, 0.5f);
            btnCerrarMapRect.anchoredPosition = Vector2.zero;
            btnCerrarMapRect.sizeDelta = new Vector2(85f, 85f);

            EnsureIsSprite("Assets/sprites/boton_cerrar.png");
            Sprite spriteCerrarMap = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/sprites/boton_cerrar.png");
            Image btnCerrarMapImg = btnCerrarMapObj.AddComponent<Image>();
            if (spriteCerrarMap != null)
            {
                btnCerrarMapImg.sprite = spriteCerrarMap;
                btnCerrarMapImg.color = Color.white;
            }
            else
            {
                btnCerrarMapImg.color = Color.red;
            }

            Button btnCerrarMap = btnCerrarMapObj.AddComponent<Button>();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(btnCerrarMap.onClick, uiManager.CerrarMapa);

            // Crear los 3 botones de pedidos
            // Pedido 1: Izquierda (Inactivo)
            GameObject btnPedido1Obj = new GameObject("Pedido_Izquierda", typeof(RectTransform));
            RectTransform btnPedido1Rect = btnPedido1Obj.GetComponent<RectTransform>();
            btnPedido1Rect.SetParent(mapPopupRect, false);
            btnPedido1Rect.anchorMin = new Vector2(0.5f, 0.5f);
            btnPedido1Rect.anchorMax = new Vector2(0.5f, 0.5f);
            btnPedido1Rect.pivot = new Vector2(0.5f, 0.5f);
            btnPedido1Rect.anchoredPosition = new Vector2(-370f, -30f);
            btnPedido1Rect.sizeDelta = new Vector2(90f, 90f);

            EnsureIsSprite("Assets/sprites/boton_nopedido.png");
            Sprite spriteNoPedido = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/sprites/boton_nopedido.png");
            Image btnPedido1Img = btnPedido1Obj.AddComponent<Image>();
            if (spriteNoPedido != null)
            {
                btnPedido1Img.sprite = spriteNoPedido;
                btnPedido1Img.color = Color.white;
            }
            else
            {
                btnPedido1Img.color = Color.gray;
            }
            btnPedido1Obj.AddComponent<Button>(); // Sin acción, solo visual / inactivo

            // Pedido 2: Centro (Activo)
            GameObject btnPedido2Obj = new GameObject("Pedido_Centro", typeof(RectTransform));
            RectTransform btnPedido2Rect = btnPedido2Obj.GetComponent<RectTransform>();
            btnPedido2Rect.SetParent(mapPopupRect, false);
            btnPedido2Rect.anchorMin = new Vector2(0.5f, 0.5f);
            btnPedido2Rect.anchorMax = new Vector2(0.5f, 0.5f);
            btnPedido2Rect.pivot = new Vector2(0.5f, 0.5f);
            btnPedido2Rect.anchoredPosition = new Vector2(-60f, -60f);
            btnPedido2Rect.sizeDelta = new Vector2(90f, 90f);

            EnsureIsSprite("Assets/sprites/boton_pedido.png");
            Sprite spritePedido = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/sprites/boton_pedido.png");
            Image btnPedido2Img = btnPedido2Obj.AddComponent<Image>();
            if (spritePedido != null)
            {
                btnPedido2Img.sprite = spritePedido;
                btnPedido2Img.color = Color.white;
            }
            else
            {
                btnPedido2Img.color = Color.yellow;
            }
            Button btnPedido2 = btnPedido2Obj.AddComponent<Button>();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(btnPedido2.onClick, uiManager.AbrirDetallePedido);

            // Pedido 3: Derecha (Inactivo)
            GameObject btnPedido3Obj = new GameObject("Pedido_Derecha", typeof(RectTransform));
            RectTransform btnPedido3Rect = btnPedido3Obj.GetComponent<RectTransform>();
            btnPedido3Rect.SetParent(mapPopupRect, false);
            btnPedido3Rect.anchorMin = new Vector2(0.5f, 0.5f);
            btnPedido3Rect.anchorMax = new Vector2(0.5f, 0.5f);
            btnPedido3Rect.pivot = new Vector2(0.5f, 0.5f);
            btnPedido3Rect.anchoredPosition = new Vector2(190f, -60f);
            btnPedido3Rect.sizeDelta = new Vector2(90f, 90f);

            Image btnPedido3Img = btnPedido3Obj.AddComponent<Image>();
            if (spriteNoPedido != null)
            {
                btnPedido3Img.sprite = spriteNoPedido;
                btnPedido3Img.color = Color.white;
            }
            else
            {
                btnPedido3Img.color = Color.gray;
            }
            btnPedido3Obj.AddComponent<Button>(); // Sin acción

            // Inyectar el panel del mapa en el AdministradorUI por reflexión
            var mapPanelField = typeof(AdministradorUI).GetField("mapPanel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (mapPanelField != null)
            {
                mapPanelField.SetValue(uiManager, mapPanelObj);
                EditorUtility.SetDirty(uiManager);
                Debug.Log("✅ mapPanel inyectado en AdministradorUI.");
            }

            // 7.6.4 Crear o buscar PanelDetallePedido
            Transform oldDetailsPanel = canvas.transform.Find("PanelDetallePedido");
            if (oldDetailsPanel != null)
            {
                UnityEngine.Object.DestroyImmediate(oldDetailsPanel.gameObject);
            }

            GameObject detailsPanelObj = new GameObject("PanelDetallePedido", typeof(RectTransform));
            RectTransform detailsPanelRect = detailsPanelObj.GetComponent<RectTransform>();
            detailsPanelRect.SetParent(canvas.transform, false);
            detailsPanelObj.SetActive(false); // Empieza oculto

            // Set to cover the whole screen with a slight 10px overflow to prevent rounding gap lines at the edges
            detailsPanelRect.anchorMin = Vector2.zero;
            detailsPanelRect.anchorMax = Vector2.one;
            detailsPanelRect.offsetMin = new Vector2(-10f, -10f);
            detailsPanelRect.offsetMax = new Vector2(10f, 10f);
            detailsPanelRect.pivot = new Vector2(0.5f, 0.5f);
            detailsPanelRect.localScale = Vector3.one;
            detailsPanelRect.localRotation = Quaternion.identity;

            // Fondo semitransparente oscuro detrás
            Image detailsPanelBg = detailsPanelObj.AddComponent<Image>();
            detailsPanelBg.color = new Color(0f, 0f, 0f, 0.6f); // 60% opaco

            // Crear el panel de contenido (Popup)
            GameObject detailsPopupObj = new GameObject("Contenido", typeof(RectTransform));
            RectTransform detailsPopupRect = detailsPopupObj.GetComponent<RectTransform>();
            detailsPopupRect.SetParent(detailsPanelRect, false);
            
            // Centrado en pantalla con AspectRatioFitter para escalar exactamente con la imagen de fondo 16:9
            detailsPopupRect.anchorMin = new Vector2(0.5f, 0.5f);
            detailsPopupRect.anchorMax = new Vector2(0.5f, 0.5f);
            detailsPopupRect.pivot = new Vector2(0.5f, 0.5f);
            detailsPopupRect.sizeDelta = new Vector2(1920f, 1080f);
            detailsPopupRect.anchoredPosition = Vector2.zero;
            detailsPopupRect.localScale = Vector3.one;

            UnityEngine.UI.AspectRatioFitter detailsAspectFitter = detailsPopupObj.AddComponent<UnityEngine.UI.AspectRatioFitter>();
            detailsAspectFitter.aspectMode = UnityEngine.UI.AspectRatioFitter.AspectMode.FitInParent;
            detailsAspectFitter.aspectRatio = 1920f / 1080f;

            Image detailsPopupImg = detailsPopupObj.AddComponent<Image>();
            
            EnsureIsSprite("Assets/sprites/imagen_pedido.png");
            Sprite spriteDetailsContent = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/sprites/imagen_pedido.png");
            if (spriteDetailsContent != null)
            {
                detailsPopupImg.sprite = spriteDetailsContent;
                detailsPopupImg.color = Color.white;
                detailsPopupImg.preserveAspect = true;
            }

            // Crear Botón "BotonCerrar" (Rojo con X, arriba a la derecha de la tarjeta roja interna)
            GameObject btnCerrarDetailsObj = new GameObject("BotonCerrar", typeof(RectTransform));
            RectTransform btnCerrarDetailsRect = btnCerrarDetailsObj.GetComponent<RectTransform>();
            btnCerrarDetailsRect.SetParent(detailsPopupRect, false);
            btnCerrarDetailsRect.anchorMin = new Vector2(0.7494f, 0.8019f);
            btnCerrarDetailsRect.anchorMax = new Vector2(0.7494f, 0.8019f);
            btnCerrarDetailsRect.pivot = new Vector2(0.5f, 0.5f);
            btnCerrarDetailsRect.anchoredPosition = Vector2.zero;
            btnCerrarDetailsRect.sizeDelta = new Vector2(85f, 85f);

            EnsureIsSprite("Assets/sprites/boton_cerrar.png");
            Sprite spriteCerrarDetails = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/sprites/boton_cerrar.png");
            Image btnCerrarDetailsImg = btnCerrarDetailsObj.AddComponent<Image>();
            if (spriteCerrarDetails != null)
            {
                btnCerrarDetailsImg.sprite = spriteCerrarDetails;
                btnCerrarDetailsImg.color = Color.white;
            }
            else
            {
                btnCerrarDetailsImg.color = Color.red;
            }

            Button btnCerrarDetails = btnCerrarDetailsObj.AddComponent<Button>();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(btnCerrarDetails.onClick, uiManager.CerrarDetallePedido);

            // Crear Botón "BotonComenzar" (Verde, abajo al centro)
            GameObject btnComenzarObj = new GameObject("BotonComenzar", typeof(RectTransform));
            RectTransform btnComenzarRect = btnComenzarObj.GetComponent<RectTransform>();
            btnComenzarRect.SetParent(detailsPopupRect, false);
            btnComenzarRect.anchorMin = new Vector2(0.5f, 0.5f);
            btnComenzarRect.anchorMax = new Vector2(0.5f, 0.5f);
            btnComenzarRect.pivot = new Vector2(0.5f, 0.5f);
            btnComenzarRect.anchoredPosition = new Vector2(0f, -240f);
            btnComenzarRect.sizeDelta = new Vector2(390f, 136f);

            EnsureIsSprite("Assets/sprites/boton_comenzar.png");
            Sprite spriteComenzar = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/sprites/boton_comenzar.png");
            Image btnComenzarImg = btnComenzarObj.AddComponent<Image>();
            if (spriteComenzar != null)
            {
                btnComenzarImg.sprite = spriteComenzar;
                btnComenzarImg.color = Color.white;
            }
            else
            {
                btnComenzarImg.color = Color.green;
            }

            Button btnComenzar = btnComenzarObj.AddComponent<Button>();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(btnComenzar.onClick, uiManager.IniciarJuego);

            // Inyectar el panel del detalle de pedido en el AdministradorUI por reflexión
            var detailsPanelField = typeof(AdministradorUI).GetField("orderDetailsPanel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (detailsPanelField != null)
            {
                detailsPanelField.SetValue(uiManager, detailsPanelObj);
                EditorUtility.SetDirty(uiManager);
                Debug.Log("✅ orderDetailsPanel inyectado en AdministradorUI.");
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

            // --- CONSTRUCCIÓN DEL LEADERBOARD EN VICTORYPANEL ---
            GameObject vicLeaderboardContainer = new GameObject("Contenedor_TablaPosiciones", typeof(RectTransform));
            RectTransform vicLeadRect = vicLeaderboardContainer.GetComponent<RectTransform>();
            vicLeadRect.SetParent(victoryPanelRect, false);
            vicLeadRect.anchorMin = new Vector2(0.5f, 0.5f);
            vicLeadRect.anchorMax = new Vector2(0.5f, 0.5f);
            vicLeadRect.pivot = new Vector2(0.5f, 0.5f);
            vicLeadRect.anchoredPosition = new Vector2(0f, -90f); // Shipped down further to avoid overlapping background text
            vicLeadRect.sizeDelta = new Vector2(500f, 360f); // Increased size from 420x320

            Image vicLeadBg = vicLeaderboardContainer.AddComponent<Image>();
            vicLeadBg.sprite = roundedBoxSprite;
            vicLeadBg.type = Image.Type.Sliced;
            vicLeadBg.color = new Color(0.08f, 0.08f, 0.08f, 0.85f); // Fondo elegante y oscuro

            Shadow vicLeadShadow = vicLeaderboardContainer.AddComponent<Shadow>();
            vicLeadShadow.effectColor = new Color(0f, 0f, 0f, 0.5f);
            vicLeadShadow.effectDistance = new Vector2(5f, -5f);

            // Título
            GameObject vicLeadTitleObj = new GameObject("VictoryLeaderboardTitle", typeof(RectTransform));
            RectTransform vicLeadTitleRect = vicLeadTitleObj.GetComponent<RectTransform>();
            vicLeadTitleRect.SetParent(vicLeadRect, false);
            vicLeadTitleRect.anchorMin = new Vector2(0f, 1f);
            vicLeadTitleRect.anchorMax = new Vector2(1f, 1f);
            vicLeadTitleRect.pivot = new Vector2(0.5f, 1f);
            vicLeadTitleRect.anchoredPosition = new Vector2(0f, -20f); // Adjusted from -15f
            vicLeadTitleRect.sizeDelta = new Vector2(0f, 35f); // Adjusted from 30f

            Text vicLeadTitleText = vicLeadTitleObj.AddComponent<Text>();
            vicLeadTitleText.font = customFont;
            vicLeadTitleText.fontSize = 26; // Increased from 22
            vicLeadTitleText.color = new Color(1f, 0.84f, 0f); // Dorado
            vicLeadTitleText.alignment = TextAnchor.MiddleCenter;
            vicLeadTitleText.text = "TABLA DE POSICIONES";

            // Texto de posiciones
            GameObject vicLeadTextObj = new GameObject("Texto_Tabla", typeof(RectTransform));
            RectTransform vicLeadTextRect = vicLeadTextObj.GetComponent<RectTransform>();
            vicLeadTextRect.SetParent(vicLeadRect, false);
            vicLeadTextRect.anchorMin = new Vector2(0f, 1f);
            vicLeadTextRect.anchorMax = new Vector2(1f, 1f);
            vicLeadTextRect.pivot = new Vector2(0.5f, 1f);
            vicLeadTextRect.anchoredPosition = new Vector2(0f, -65f); // Adjusted from -50f
            vicLeadTextRect.sizeDelta = new Vector2(-50f, 180f); // Adjusted from -40x140

            Text vicLeadText = vicLeadTextObj.AddComponent<Text>();
            vicLeadText.font = customFont;
            vicLeadText.fontSize = 22; // Increased from 18
            vicLeadText.color = Color.white;
            vicLeadText.alignment = TextAnchor.UpperCenter;
            vicLeadText.text = "Cargando posiciones...";

            // InputField para el nombre
            GameObject vicLeadInputObj = new GameObject("InputField_Nombre", typeof(RectTransform));
            RectTransform vicLeadInputRect = vicLeadInputObj.GetComponent<RectTransform>();
            vicLeadInputRect.SetParent(vicLeadRect, false);
            vicLeadInputRect.anchorMin = new Vector2(0f, 0f);
            vicLeadInputRect.anchorMax = new Vector2(0f, 0f);
            vicLeadInputRect.pivot = new Vector2(0f, 0f);
            vicLeadInputRect.anchoredPosition = new Vector2(25f, 25f); // Adjusted from 20x20
            vicLeadInputRect.sizeDelta = new Vector2(300f, 50f); // Adjusted from 250x45

            Image vicLeadInputBg = vicLeadInputObj.AddComponent<Image>();
            vicLeadInputBg.sprite = roundedBoxSprite;
            vicLeadInputBg.type = Image.Type.Sliced;
            vicLeadInputBg.color = new Color(0.15f, 0.15f, 0.15f, 1f);

            InputField vicLeadInput = vicLeadInputObj.AddComponent<InputField>();

            // Texto del input
            GameObject vicLeadInputTextObj = new GameObject("Text", typeof(RectTransform));
            RectTransform vicLeadInputTextRect = vicLeadInputTextObj.GetComponent<RectTransform>();
            vicLeadInputTextRect.SetParent(vicLeadInputRect, false);
            vicLeadInputTextRect.anchorMin = Vector2.zero;
            vicLeadInputTextRect.anchorMax = Vector2.one;
            vicLeadInputTextRect.offsetMin = new Vector2(10f, 5f);
            vicLeadInputTextRect.offsetMax = new Vector2(-10f, -5f);

            Text vicLeadInputTextComp = vicLeadInputTextObj.AddComponent<Text>();
            vicLeadInputTextComp.font = customFont;
            vicLeadInputTextComp.fontSize = 20; // Increased from 18
            vicLeadInputTextComp.color = Color.white;
            vicLeadInputTextComp.alignment = TextAnchor.MiddleLeft;
            vicLeadInputTextComp.supportRichText = false;

            // Placeholder del input
            GameObject vicLeadInputPlaceholderObj = new GameObject("Placeholder", typeof(RectTransform));
            RectTransform vicLeadInputPlaceholderRect = vicLeadInputPlaceholderObj.GetComponent<RectTransform>();
            vicLeadInputPlaceholderRect.SetParent(vicLeadInputRect, false);
            vicLeadInputPlaceholderRect.anchorMin = Vector2.zero;
            vicLeadInputPlaceholderRect.anchorMax = Vector2.one;
            vicLeadInputPlaceholderRect.offsetMin = new Vector2(10f, 5f);
            vicLeadInputPlaceholderRect.offsetMax = new Vector2(-10f, -5f);

            Text vicLeadInputPlaceholderComp = vicLeadInputPlaceholderObj.AddComponent<Text>();
            vicLeadInputPlaceholderComp.font = customFont;
            vicLeadInputPlaceholderComp.fontSize = 20; // Increased from 18
            vicLeadInputPlaceholderComp.fontStyle = FontStyle.Italic;
            vicLeadInputPlaceholderComp.color = new Color(0.6f, 0.6f, 0.6f, 0.5f);
            vicLeadInputPlaceholderComp.alignment = TextAnchor.MiddleLeft;
            vicLeadInputPlaceholderComp.text = "Ingresa tu nombre...";

            vicLeadInput.textComponent = vicLeadInputTextComp;
            vicLeadInput.placeholder = vicLeadInputPlaceholderComp;
            vicLeadInput.targetGraphic = vicLeadInputBg;

            // Botón Guardar
            GameObject vicLeadSaveBtnObj = new GameObject("Boton_Guardar", typeof(RectTransform));
            RectTransform vicLeadSaveBtnRect = vicLeadSaveBtnObj.GetComponent<RectTransform>();
            vicLeadSaveBtnRect.SetParent(vicLeadRect, false);
            vicLeadSaveBtnRect.anchorMin = new Vector2(0f, 0f);
            vicLeadSaveBtnRect.anchorMax = new Vector2(0f, 0f);
            vicLeadSaveBtnRect.pivot = new Vector2(0f, 0f);
            vicLeadSaveBtnRect.anchoredPosition = new Vector2(340f, 25f); // Adjusted from 285x20
            vicLeadSaveBtnRect.sizeDelta = new Vector2(135f, 50f); // Adjusted from 115x45

            Image vicLeadSaveBtnBg = vicLeadSaveBtnObj.AddComponent<Image>();
            vicLeadSaveBtnBg.sprite = roundedBoxSprite;
            vicLeadSaveBtnBg.type = Image.Type.Sliced;
            vicLeadSaveBtnBg.color = new Color(1f, 0.84f, 0f); // Dorado/Amarillo

            Button vicLeadSaveBtn = vicLeadSaveBtnObj.AddComponent<Button>();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(vicLeadSaveBtn.onClick, uiManager.SaveVictoryScore);

            GameObject vicLeadSaveBtnTextObj = new GameObject("Texto", typeof(RectTransform));
            RectTransform vicLeadSaveBtnTextRect = vicLeadSaveBtnTextObj.GetComponent<RectTransform>();
            vicLeadSaveBtnTextRect.SetParent(vicLeadSaveBtnRect, false);
            vicLeadSaveBtnTextRect.anchorMin = Vector2.zero;
            vicLeadSaveBtnTextRect.anchorMax = Vector2.one;
            vicLeadSaveBtnTextRect.offsetMin = Vector2.zero;
            vicLeadSaveBtnTextRect.offsetMax = Vector2.zero;

            Text vicLeadSaveBtnText = vicLeadSaveBtnTextObj.AddComponent<Text>();
            vicLeadSaveBtnText.font = customFont;
            vicLeadSaveBtnText.fontSize = 20; // Increased from 16
            vicLeadSaveBtnText.fontStyle = FontStyle.Bold;
            vicLeadSaveBtnText.color = new Color(0.08f, 0.08f, 0.08f, 1f); // Contraste oscuro
            vicLeadSaveBtnText.alignment = TextAnchor.MiddleCenter;
            vicLeadSaveBtnText.text = "GUARDAR";

            // Inyectar referencias en AdministradorUI por reflexión
            var victoryNameInputFieldField = typeof(AdministradorUI).GetField("victoryNameInputField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (victoryNameInputFieldField != null) victoryNameInputFieldField.SetValue(uiManager, vicLeadInput);

            var victorySaveButtonField = typeof(AdministradorUI).GetField("victorySaveButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (victorySaveButtonField != null) victorySaveButtonField.SetValue(uiManager, vicLeadSaveBtn);

            var victoryLeaderboardTextField = typeof(AdministradorUI).GetField("victoryLeaderboardText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (victoryLeaderboardTextField != null) victoryLeaderboardTextField.SetValue(uiManager, vicLeadText);

            // 7.8 Crear o buscar el panel y texto de Skip del video persistentes
            Transform oldSkipText = canvas.transform.Find("IntroVideo_SkipText");
            if (oldSkipText != null)
            {
                UnityEngine.Object.DestroyImmediate(oldSkipText.gameObject);
            }
            Transform oldSkipPanel = canvas.transform.Find("IntroVideo_SkipPanel");
            if (oldSkipPanel != null)
            {
                UnityEngine.Object.DestroyImmediate(oldSkipPanel.gameObject);
            }

            // Creamos el Panel de fondo (el marco)
            GameObject skipPanelObj = new GameObject("IntroVideo_SkipPanel", typeof(RectTransform));
            skipPanelObj.transform.SetParent(canvas.transform, false);

            Image skipPanelImage = skipPanelObj.AddComponent<Image>();
            // Color de fondo: Negro semitransparente (65% opaco) para excelente contraste
            skipPanelImage.color = new Color(0.12f, 0.12f, 0.12f, 0.65f);

            // Cargar el sprite por defecto de Unity UI con bordes suaves redondeados
            Sprite defaultBg = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            if (defaultBg != null)
            {
                skipPanelImage.sprite = defaultBg;
                skipPanelImage.type = Image.Type.Sliced;
            }

            // Posicionamiento del panel en la esquina inferior derecha
            RectTransform skipPanelRect = skipPanelObj.GetComponent<RectTransform>();
            skipPanelRect.anchorMin = new Vector2(1f, 0f);
            skipPanelRect.anchorMax = new Vector2(1f, 0f);
            skipPanelRect.pivot = new Vector2(1f, 0f);
            skipPanelRect.anchoredPosition = new Vector2(-40f, 40f);
            skipPanelRect.sizeDelta = new Vector2(340f, 60f); // Tamaño del marco llamativo

            // Creamos el Texto como hijo del panel
            GameObject skipTextObj = new GameObject("Texto_Skip", typeof(RectTransform));
            skipTextObj.transform.SetParent(skipPanelObj.transform, false);

            Text skipTextComp = skipTextObj.AddComponent<Text>();
            skipTextComp.font = standardFont;
            skipTextComp.text = "Presiona E para omitir";
            skipTextComp.fontSize = 22; // Tamaño de fuente ideal para el marco
            skipTextComp.alignment = TextAnchor.MiddleCenter;
            skipTextComp.color = Color.white; // Blanco para resalte en fondo oscuro
            skipTextComp.horizontalOverflow = HorizontalWrapMode.Overflow;
            skipTextComp.verticalOverflow = VerticalWrapMode.Overflow;

            // Sombra oscura para las letras
            Shadow skipShadow = skipTextObj.AddComponent<Shadow>();
            skipShadow.effectColor = new Color(0f, 0f, 0f, 0.8f);
            skipShadow.effectDistance = new Vector2(1.5f, -1.5f);

            // Ajustar el RectTransform del texto para llenar el panel con margen (padding)
            RectTransform skipTextRect = skipTextObj.GetComponent<RectTransform>();
            skipTextRect.anchorMin = Vector2.zero;
            skipTextRect.anchorMax = Vector2.one;
            skipTextRect.pivot = new Vector2(0.5f, 0.5f);
            skipTextRect.offsetMin = new Vector2(15f, 5f);
            skipTextRect.offsetMax = new Vector2(-15f, -5f);

            // Inactivo por defecto (se activa al reproducir el video)
            skipPanelObj.SetActive(false);

            // Inyectar referencia por reflexión en AdministradorUI
            var skipTextField = typeof(AdministradorUI).GetField("skipText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (skipTextField != null)
            {
                skipTextField.SetValue(uiManager, skipTextComp);
                EditorUtility.SetDirty(uiManager);
                Debug.Log("✅ skipText persistente inyectado en AdministradorUI.");
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

            string spritePath = "Assets/sprites/sprite_repartidor.png";
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
                Debug.LogWarning("No se encontraron suficientes sprites en sprite_repartidor.png para configurar las animaciones.");
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

            string controllerPath = "Assets/sprites/sprite_repartidor.controller";
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
                rTrans.sizeDelta = new Vector2(90f, 66f);

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

            GameObject riderObj = GameObject.Find("sprite_repartidor_0 (1)");
            if (riderObj == null)
            {
                riderObj = GameObject.Find("sprite_repartidor_0");
            }
            if (riderObj == null)
            {
                riderObj = GameObject.Find("imagen_repartidor_0 (1)");
            }
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

        private static Sprite LoadSprite(string path)
        {
            Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (s != null) return s;

            var subAssets = AssetDatabase.LoadAllAssetsAtPath(path);
            if (subAssets != null)
            {
                foreach (var asset in subAssets)
                {
                    if (asset is Sprite sprite)
                    {
                        return sprite;
                    }
                }
            }
            return null;
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
