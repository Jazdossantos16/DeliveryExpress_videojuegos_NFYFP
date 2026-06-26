using UnityEditor;
using UnityEngine;
using UnityEditor.Build.Reporting;

namespace DeliveryExpress
{
    public static class WebGLBuilder
    {
        [MenuItem("Build/Build WebGL")]
        public static void BuildWebGL()
        {
            Debug.Log("🚀 Iniciando compilación WebGL...");

            // Definimos la carpeta de destino dentro del proyecto
            string buildPath = "Builds/WebGL";

            // Escena principal del juego
            string[] scenes = new string[] { "Assets/Scenes/SampleScene.unity" };

            // Configuramos las opciones del Build
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = scenes;
            buildPlayerOptions.locationPathName = buildPath;
            buildPlayerOptions.target = BuildTarget.WebGL;
            buildPlayerOptions.options = BuildOptions.None;

            // Ejecutamos el Build
            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"✅ Compilación WebGL finalizada con éxito en: {buildPath}");
            }
            else
            {
                Debug.LogError("❌ La compilación de WebGL falló. Revisa la consola de Unity para ver los errores.");
            }
        }
    }
}
