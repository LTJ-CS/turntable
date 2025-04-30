using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public static class GenerationSoundListToCfg
{
    
    private const string classPath = "/GameScript/Runtime/Util/SoundNameUtil.cs";
    private const string waring = "//Auto Generate Code--Warning:Don't Edit!!";
    private const string soundPath = "/GameRes/Runtime/Sound/";
    [MenuItem("Tools/声音/生成声音文件名字")]
    static void GenerateSoundName()
    {
        string path = Application.dataPath + soundPath;
        DirectoryInfo dinfo = new DirectoryInfo(path);
        if (!dinfo.Exists)
        {
            Debug.Log($"声音资源所在目录不存在:{path}");
            return;
        }

        DirectoryInfo[] childInfos = dinfo.GetDirectories("*", SearchOption.TopDirectoryOnly);
        List<string> names = new List<string>();
        List<string> soundDirs = new List<string>();
        for (int i = 0; i < childInfos.Length; i++)
        {
            var info = childInfos[i];
            soundDirs.Add(info.Name);
            if(info.Name.Equals("Mixer"))
                continue;
            FileInfo[] files = info.GetFiles("*.*",SearchOption.TopDirectoryOnly).Where(f => f.Extension == ".ogg" || f.Extension == ".wav" || f.Extension == ".mp3").ToArray();
            for (int j = 0; j < files.Length; j++)
            {
                string name = Path.GetFileNameWithoutExtension(files[j].Name);
                if (names.Contains(name))
                {
                    Debug.LogError($"声音文件有重名文件：{name}，无法生成");
                    return;
                }
                names.Add(name);
            }
        }

        if (names.Count > 0)
        {
            WriteInternal(names, soundDirs);
        }


    }
    
    static void WriteInternal(List<string> names, List<string> dirs)
    {
        string path = Application.dataPath + classPath;
        
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
                fsr = new FileStream(path,FileMode.Create,FileAccess.ReadWrite);
            }
            writer = new StreamWriter(fsr);
            writer.WriteLine(waring);
            writer.WriteLine();
            writer.WriteLine();

            writer.WriteLine("public static class SoundDirConst\n{");
            for (int i = 0; i < dirs.Count; i++)
            {
                string dir = dirs[i];
                writer.WriteLine($"    public const string {dir} = \"{dir}/\";");
            }
            writer.WriteLine("}\n");

            writer.WriteLine("public static class SoundNameUtil");
            writer.WriteLine("{");
            for (int i = 0; i < names.Count; i++)
            {
                string name = names[i];
                string paramName = name;
                if (paramName.Length > 0)
                {
                    paramName = char.ToUpper(paramName[0]) + paramName.Substring(1);
                }
                writer.WriteLine($"    public const string {paramName} = \"{name}\";");
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


    [MenuItem("Tools/修改收藏头像的名字")]
    static void RenameCollectName()
    {
        var objs = Selection.objects;
        for (int i = 0; i < objs.Length; i++)
        {
            var sprite = objs[i] as Texture2D;
            if(sprite == null)
                continue;
            int num = GetNumFromName(sprite.name);
            AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(sprite), num.ToString());
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    static int GetNumFromName(string strName)
    {
        string pattern = @"\d+";

        MatchCollection matches = Regex.Matches(strName, pattern);

        string result = "";
        foreach (Match match in matches)
        {
            result += match.Value;
        }

        int number = int.Parse(result)+10000;
        return number;
    }
}