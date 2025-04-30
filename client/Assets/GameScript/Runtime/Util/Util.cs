// namespace GameScript.Runtime.Util
// {
//     using UnityEngine;
//     using UnityEditor;
//     using System.Text;
//     using System.Collections.Generic;
//     using System.Text.RegularExpressions;
//
//     public static class Util
//     {
//         [MenuItem("Tools/Extract Ending Numbers")]
//         static void ExtractNumbersFromSelection()
//         {
//             StringBuilder sb = new StringBuilder();
//             Object[] selectedObjects = Selection.objects;
//
//             if (selectedObjects == null || selectedObjects.Length == 0)
//             {
//                 return;
//             }
//
//             sb.AppendLine($"Processing {selectedObjects.Length} selected assets:");
//             sb.AppendLine("----------------------------------------");
//
//
//             foreach (Object obj in selectedObjects)
//             {
//                 string assetName = obj.name;
//                 string numbers = ExtractEndingNumbers(assetName);
//                 AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(obj), numbers);
//             }
//
//             AssetDatabase.Refresh();
//             static string ExtractEndingNumbers(string input)
//             {
//                 // 使用正则表达式匹配末尾的数字
//                 Match match = Regex.Match(input, @"(\d+)(?!.*\d)");
//                 return match.Success ? match.Value : "";
//             }
//         }
//         
//         [MenuItem("Tools/Extract Hair Numbers")]
//         static void ExtractNumbersFromSelectionHair()
//         {
//             StringBuilder sb = new StringBuilder();
//             Object[] selectedObjects = Selection.objects;
//
//             if (selectedObjects == null || selectedObjects.Length == 0)
//             {
//                 return;
//             }
//
//             sb.AppendLine($"Processing {selectedObjects.Length} selected assets:");
//             sb.AppendLine("----------------------------------------");
//
//
//             foreach (Object obj in selectedObjects)
//             {
//                 string assetName = obj.name;
//                 string numbers = ExtractEndingNumbers(assetName);
//                 AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(obj), numbers);
//             }
//
//             AssetDatabase.Refresh();
//             static string ExtractEndingNumbers(string input)
//             {
//                 // 使用正则表达式匹配末尾的数字
//                 Match match = Regex.Match(input, @"(-\d+-\d+)_?");
//                 string a=  Regex.Replace(match.Value, "_", "");
//                 //去除首字符
//                return a.Substring(2,a.Length-2);
//             }
//         }
//     }
// }