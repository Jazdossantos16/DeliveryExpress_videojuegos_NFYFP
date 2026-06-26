using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace DeliveryExpress.Editor
{
    public static class ReconstruirUI
    {
        [MenuItem("Tools/Rebuild UI")]
        public static void EjecutarReconstruccionMenu()
        {
            EjecutarReconstruccion();
        }

        public static void EjecutarReconstruccion()
        {
            string[] escenas = new string[] {
                "Assets/Scenes/SampleScene.unity",
                "Assets/videojuego.unity"
            };

            foreach (var path in escenas)
            {
                if (System.IO.File.Exists(path))
                {
                    Debug.Log($"🎬 Abriendo escena: {path}");
                    var escena = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                    
                    // Ejecutar la reconstrucción completa
                    AyudanteConfiguracionEscena.SetupNewStreetAndSidewalkInternal(true);
                    
                    // Marcar escena como modificada y guardarla
                    EditorSceneManager.MarkSceneDirty(escena);
                    EditorSceneManager.SaveScene(escena);
                    Debug.Log($"✅ Escena {path} reconstruida y guardada.");
                }
            }
        }
    }
}
