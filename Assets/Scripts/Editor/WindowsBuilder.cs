using UnityEditor;
using UnityEngine;
using UnityEditor.Build.Reporting;

namespace DeliveryExpress
{
    public static class WindowsBuilder
    {
        [MenuItem("Build/Build Windows Standalone")]
        public static void BuildWindows()
        {
            Debug.Log("🚀 Iniciando compilación Windows Standalone (64-bit)...");

            // Definimos el ejecutable de destino
            string buildPath = "Builds/Windows/DeliveryExpress.exe";

            // Escena principal del juego
            string[] scenes = new string[] { "Assets/Scenes/SampleScene.unity" };

            // Configuramos las opciones del Build
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = scenes;
            buildPlayerOptions.locationPathName = buildPath;
            buildPlayerOptions.target = BuildTarget.StandaloneWindows64;
            buildPlayerOptions.options = BuildOptions.None;

            // Ejecutamos el Build
            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"✅ Compilación Windows finalizada con éxito en: {buildPath}");
            }
            else
            {
                Debug.LogError("❌ La compilación de Windows falló. Revisa la consola de Unity para ver los errores.");
            }
        }
    }
}
