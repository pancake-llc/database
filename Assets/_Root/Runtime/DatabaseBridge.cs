using System;
using System.Collections.Generic;
using System.IO;
using LiteDB;
using MEC;
using UnityEngine;
using UnityEngine.Networking;

namespace Snorlax.Database
{
    public static class DatabaseBridge
    {
        /// <summary>
        /// take database in streaming asset folder
        /// </summary>
        /// <param name="relativeFilePath">part file name in StreamingAssets Folder (including the file extension) ex: Data/Enemy/training_enemy.db</param>
        /// <returns></returns>
        public static string TakeDatabaseStreamingAsset(string relativeFilePath)
        {
            string path = string.Format("{0}/{1}", Application.persistentDataPath, relativeFilePath);
            bool isExists = File.Exists(path);

            switch (Application.platform)
            {
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.LinuxEditor:
                {
                    return string.Format("{0}/StreamingAssets/{1}", Application.dataPath, relativeFilePath);
                }
                case RuntimePlatform.IPhonePlayer:
                {
                    if (isExists) return path;

                    string iosPath = string.Format("/{0}Raw/{1}", Application.dataPath, relativeFilePath);
                    File.Copy(iosPath, path);
                    break;
                }
                case RuntimePlatform.Android:
                {
                    if (isExists) return path;

                    string androidPath = string.Format("jar:file://{0}!/assets/{1}", Application.dataPath, relativeFilePath);
                    FetchDataFile(androidPath, path).RunCoroutine();
                    break;
                }
                default:
                    throw new NotSupportedException();
            }

#if UNITY_2020_2_OR_NEWER
            static IEnumerator<float> FetchDataFile(string pathLocalVar, string destinationPath)
#else
            IEnumerator<float> FetchDataFile(string pathLocalVar, string destinationPath)
#endif
            {
                using var webRequest = UnityWebRequest.Get(pathLocalVar);
                yield return Timing.WaitUntilDone(webRequest.SendWebRequest());

                if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError($"[StreamingAssets] {webRequest.error}");
                }
                else
                {
                    while (!webRequest.isDone)
                    {
                        yield return Timing.WaitForOneFrame;
                    }

                    File.WriteAllBytes(destinationPath, webRequest.downloadHandler.data);
                }
            }

            return path;
        }
        
        
        /// <summary>
        /// Create if it not exist otherwise open it
        /// </summary>
        /// <param name="name">name of database or relative path in StreamingAssets folder not include file extension ex: Data/Item/wood-sword</param>
        /// <param name="mapper"></param>
        public static LiteDatabase Open(string name, BsonMapper mapper = null)
        {
            string path = TakeDatabaseStreamingAsset($"{name}.db");
            
#if UNITY_EDITOR
            string[] pathPart = path.Split(Path.DirectorySeparatorChar);
            string directory = path.Replace(pathPart[pathPart.Length - 1], "");
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                UnityEditor.AssetDatabase.SaveAssets();
                UnityEditor.AssetDatabase.Refresh();
            }
#endif
            
            return InternalCreateDatabase(path, mapper);
        }

        
        /// <summary>
        /// Create or open database with absolute path
        /// </summary>
        /// <param name="absolutePath">absolute path of file</param>
        /// <param name="mapper"></param>
        /// <returns></returns>
        private static LiteDatabase InternalCreateDatabase(string absolutePath, BsonMapper mapper = null)
        {
            return new LiteDatabase(absolutePath, mapper);
        }
    }
}