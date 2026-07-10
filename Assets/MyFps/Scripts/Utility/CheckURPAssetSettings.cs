using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using System.IO;

public class CheckURPAssetSettings
{
    public static void Execute()
    {
        string logPath = @"C:\Users\MBC\.gemini\antigravity\brain\591af453-1332-4aa3-a836-74a1e08fd21d\scratch\urp_asset_settings.txt";
        
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        
        sb.AppendLine($"GraphicsSettings.defaultRenderPipeline: {(GraphicsSettings.defaultRenderPipeline != null ? GraphicsSettings.defaultRenderPipeline.name : "null")}");
        sb.AppendLine($"GraphicsSettings.renderPipelineAsset: {(GraphicsSettings.defaultRenderPipeline != null ? GraphicsSettings.defaultRenderPipeline.name : "null")}");
        
        // Quality settings
        int qualityLevel = QualitySettings.GetQualityLevel();
        sb.AppendLine($"Current Quality Level: {qualityLevel} ({QualitySettings.names[qualityLevel]})");
        //sb.AppendLine($"QualitySettings.renderPipelineAsset: {(QualitySettings.renderPipelineAsset != null ? QualitySettings.renderPipelineAsset.name : "null")}");

        // Let's also check if there are any other URP assets in the project
        string[] guids = AssetDatabase.FindAssets("t:UniversalRenderPipelineAsset");
        sb.AppendLine($"\nURP Assets found on disk: {guids.Length}");
        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            sb.AppendLine($"- Path: {path}");
        }

        File.WriteAllText(logPath, sb.ToString());
    }
}
