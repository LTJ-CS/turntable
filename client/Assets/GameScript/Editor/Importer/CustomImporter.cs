using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UIFramework;
using Unity.VectorGraphics;
using Unity.VectorGraphics.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameScript.Editor.Importer
{
    /// <summary>
    /// 自动处理项目本身的资源导入
    /// </summary>
    public class CustomImporter : AssetPostprocessor
    {
        private List<string> NormalSpriteImportPath = new List<string>()
                                                      {
                                                          "Assets/GameRes/Runtime/Textures/Icon",
                                                          "Assets/GameRes/Runtime/Textures/LevelDesignerAvatar",
                                                          "Assets/GameRes/Runtime/Textures/Level/Box",

                                                      };
        private void OnPreprocessTexture()
        {
            // 自动设置纹理的导入: SpriteImportMode.Single, TextureWrapMode.Clamp, SpriteMeshType.FullRect, mipmapEnabled = false
            if (
                assetPath.StartsWith(UIFrameworkSettings.UIProjectResPath) ||
                assetPath.StartsWith("Assets/GameRes/Runtime/Textures/Level/Screw")
                                     || NormalSpriteImportPath.Exists(x=>assetImporter.assetPath.Contains(x))
            )
            {
                // 自动设置 UI 目录下的纹理导入选项
                var importer = (TextureImporter)assetImporter;
                importer.maxTextureSize = 1024;
                importer.isReadable = false;
                importer.mipmapEnabled = false;
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.wrapMode = TextureWrapMode.Clamp;

                var importerSettings = new TextureImporterSettings();
                importer.ReadTextureSettings(importerSettings);
                importerSettings.spriteGenerateFallbackPhysicsShape = false;
                importerSettings.mipmapEnabled = false;
                importerSettings.spriteMeshType = SpriteMeshType.FullRect;
                importer.SetTextureSettings(importerSettings);
            }
            else if (NormalSpriteImportPath.Exists(x=>assetImporter.assetPath.Contains(x)))
            {
                var importer = (TextureImporter)assetImporter;
                importer.maxTextureSize = 1024;
                importer.isReadable = false;
                importer.mipmapEnabled = false;
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.wrapMode = TextureWrapMode.Clamp;

                var importerSettings = new TextureImporterSettings();
                importer.ReadTextureSettings(importerSettings);
                importerSettings.spriteGenerateFallbackPhysicsShape = false;
                importerSettings.mipmapEnabled = false;
                importerSettings.spriteMeshType = SpriteMeshType.FullRect;
                importer.SetTextureSettings(importerSettings);
            }
        }

        /// <summary>
        /// 获取皮肤的不透明的区域, 这里假设皮肤所在的区域一定有透明度
        /// </summary>
        /// <param name="texture">指定要检测的皮肤纹理</param>
        /// <returns></returns>
        private static Vector4 GetSkinTrimmedBorders(Texture2D texture)
        {
            int width = texture.width;
            int height = texture.height;

            Color[] pixels = texture.GetPixels();

            int left = width;
            int right = 0;
            int top = height;
            int bottom = 0;

            // 遍历像素，找到非透明像素的边界
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color pixel = pixels[x + y * width];

                    if (pixel.a != 0) // 非透明像素
                    {
                        if (x < left) left = x;
                        if (x > right) right = x;
                        if (y < top) top = y;
                        if (y > bottom) bottom = y;
                    }
                }
            }

            // 如果所有像素都是透明的，返回一个空的 Border (0, 0, 0, 0)
            if (left > right || top > bottom)
            {
                return new Vector4(0, 0, 0, 0);
            }
            
            // 多留一点边界, 免得由于采样会导致有些颜色没有被正确显示出来
            const int pad = 2;  // 留一点边界
            bottom = Math.Min(height - 1, bottom + pad);
            right = Math.Min(width - 1, right + pad);
            left = Math.Max(0, left - pad);
            top = Math.Max(0, top - pad);
            
            // 返回代表Sprite Border的Vector4
            return new Vector4(left, right, top, bottom);
        }

        private static Color32 Premultiply(Color32 color)
        {
            float a = color.a / 255f;
            return new Color32((byte)(color.r * a), (byte)(color.g * a), (byte)(color.b * a), color.a);
        }

        /// <summary>
        /// 在这里填充不需要进行名称合规检测的文件
        /// </summary>
        private static readonly List<string> IgnoreSvgNameList = new List<string>()
                                                                 {
                                                                     "CircleTable",
                                                                     "TableShadow"
                                                                 };

        #region 配置导入

        private static List<string> cfgPathList = new List<string>();

        static void GenerateCfgName(string cfgName)
        {
            if (cfgPathList.Contains(cfgName))
                return;
            string path = Application.dataPath + PathManager.CfgPathRoot.Replace("Assets", "");
            DirectoryInfo dinfo = new DirectoryInfo(path);
            if (!dinfo.Exists)
            {
                Debug.Log($"配置资源所在目录不存在:{path}");
                return;
            }


            cfgPathList.Clear();
            FileInfo[] files = dinfo.GetFiles("*.*", SearchOption.TopDirectoryOnly).Where(f => f.Extension == ".json").ToArray();
            for (int j = 0; j < files.Length; j++)
            {
                string name = Path.GetFileNameWithoutExtension(files[j].Name);
                if (cfgPathList.Contains(name))
                {
                    Debug.LogError($"cfg文件有重名文件：{name}，无法生成");
                    return;
                }

                cfgPathList.Add(name);
            }

            if (cfgPathList.Count > 0)
            {
                WriteInternal(cfgPathList);
            }
        }

        static void WriteInternal(List<string> names)
        {
            string path = Application.dataPath + "/GameScript/Runtime/Util/CfgNameUtil.cs";

            bool isExist = File.Exists(path);
            FileStream fsr = null;
            StreamWriter writer = null;

            try
            {
                if (isExist)
                {
                    fsr = new FileStream(path, FileMode.Truncate, FileAccess.ReadWrite);
                }
                else
                {
                    fsr = new FileStream(path, FileMode.Create, FileAccess.ReadWrite);
                }

                writer = new StreamWriter(fsr);
                writer.WriteLine("//Auto Generate Code--Warning:Don't Edit!!");
                writer.WriteLine();
                writer.WriteLine();
                writer.WriteLine("public static class CfgNameUtil");
                writer.WriteLine("{");
                for (int i = 0; i < names.Count; i++)
                {
                    string name = names[i];
                    string paramName = name;
                    if (paramName.Length > 0)
                    {
                        paramName = char.ToUpper(paramName[0]) + paramName.Substring(1);
                    }

                    writer.WriteLine($"\t public const string {paramName} = \"{name}\";");
                }

                writer.WriteLine("}");
                writer.Flush();
            }
            catch (Exception)
            {
                // ignored
            }
            finally
            {
                if (writer != null)
                {
                    writer.Close();
                    writer.Dispose();
                }

                if (fsr != null)
                {
                    fsr.Close();
                    fsr.Dispose();
                }
            }
        }

        #endregion
    }
}