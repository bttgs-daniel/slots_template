//v1_1 -Add SaveToTextFileAppPersist
/*
  28.05.2019 add json (save, load)
  22.08.2019 delete file from app persist folder
  20.05.2020 listwrapper
  27.05.2020 listwrapper [serializable]
  28.07.2020 - gethash
*/
using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;

namespace Mkey
{
    public class FileWorker
    {
        public static void SaveToTextFileAppPersist(string fName, string[] lines)
        {
            string path = Path.Combine(Application.persistentDataPath, fName);
            if (File.Exists(path)) File.Delete(path);
            using (StreamWriter sw = File.CreateText(path))
            {
                for (int i = 0; i < lines.Length; i++)
                {
                    sw.WriteLine(lines[i]);
                }
            }
        }

        public static void SaveToTextFile(string path, string[] lines)
        {
            if (File.Exists(path)) File.Delete(path);
            using (StreamWriter sw = File.CreateText(path))
            {
                for (int i = 0; i < lines.Length; i++)
                {
                    sw.WriteLine(lines[i]);
                }
            }
        }

        public static void AddToTextFile(string path, string[] lines)
        {
            if (!File.Exists(path)) return;
            using (StreamWriter sw = new StreamWriter(path, true))
            {
                for (int i = 0; i < lines.Length; i++)
                {
                    sw.WriteLine(lines[i]);
                }
            }
        }

        public static string[] ReadTextFile(string path)
        {
            if (!File.Exists(path)) return null;
            List<string> sList = new List<string>();
            using (StreamReader sr = File.OpenText(path))
            {
                string s = "";
                while ((s = sr.ReadLine()) != null)
                {
                    sList.Add(s);
                }
            }
            return sList.ToArray();
        }

        public static string ReadTextFileToSingleString(string path)
        {
            string s = "";
            if (!File.Exists(path)) return null;
            using (StreamReader sr = File.OpenText(path))
            {
                s = sr.ReadToEnd();
            }
            return s;
        }

        /// <summary>
        /// Save binary data to path file. Rewrite existing file.
        /// </summary>
        /// <returns>void.</returns>
        public static void SaveData(string path, object sObject,bool compress)
        {
            try
            {
                if (File.Exists(path)) { File.Delete(path); }
                Stream stream = File.Open(path, FileMode.OpenOrCreate);
                BinaryFormatter bformatter = new BinaryFormatter();
                bformatter.Serialize(stream, sObject);
                stream.Close();
                //Debug.Log("-------------save data --------------------------");
            }
            catch (Exception ex)
            {
                Debug.Log("-------------error save data --------------------------");
                Debug.Log(ex.Message);
            }
        }

        /// <summary>
        /// Save binary data to Application.persistentDataPath/fName file. Delete existing file.
        /// </summary>
        /// <returns>void.</returns>
        public static void SaveDataToAppPersist(string fName, object sObject, bool compress)
        {
            string pathToData = Path.Combine(Application.persistentDataPath, fName);
            SaveData(pathToData, sObject, compress);
        }

        /// <summary>
        /// Load T object data  from file if exist or return null
        /// </summary>
        /// <param name="fName"></param>
        /// <returns></returns>
        public static T LoadDataFromFile<T>(string path, bool deleteCorrupted, bool uncompress) where T : class
        {
            T f = null;
            if (File.Exists(path))
            {
                Stream stream = File.Open(path, FileMode.Open);
                try //if file corrupted or format is not correct
                {
                    BinaryFormatter bformatter = new BinaryFormatter();
                    f = (T)bformatter.Deserialize(stream);
                    stream.Close();
                    //  Debug.Log("-------------saved data loaded  --------------------------");
                }
                catch
                {
                    stream.Close();
                    if (deleteCorrupted) File.Delete(path);
                    Debug.Log("-------------error load data --------------------------");
                    Debug.Log("-------------delete old file --------------------------");
                }
            }
            else
            {
                Debug.Log("-------------file not exist --------------------------");
            }
            return f;
        }

        /// <summary>
        /// Load T object data  from Application.persistentDataPath/fName file
        /// </summary>
        /// <param name="fName"></param>
        /// <returns></returns>
        public static T LoadDataFromAppPersistFile<T>(string fName, bool deleteCorrupted, bool uncompress) where T : class
        {
            string pathToAppPerstst = Path.Combine(Application.persistentDataPath, fName);
            return LoadDataFromFile<T>(pathToAppPerstst, deleteCorrupted, uncompress);
        }

        /// <summary>
        /// save object in json format to Application.persistentDataPath/fName file. Delete existing file.
        /// </summary>
        /// <param name="fName"></param>
        /// <param name="sObject"></param>
        public static void SaveObjectJsonToAppPersist(string fName, object sObject)
        {
            string json = JsonUtility.ToJson(sObject);
            List<string> lines =new List<string> (json.GetLines());
            SaveToTextFileAppPersist(fName, lines.ToArray());
        }

        /// <summary>
        /// Load T object data  from file in json format if exist or return null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fName"></param>
        /// <returns></returns>
        public static T LoadObjectJsonFromAppPersist <T>(string fName) where T : class
        {
            string pathToAppPerstst = Path.Combine(Application.persistentDataPath, fName);
            string json = ReadTextFileToSingleString(pathToAppPerstst);
            return JsonUtility.FromJson<T>(json);
        }

        /// <summary>
        /// Delete file from apppersist folder
        /// </summary>
        /// <param name="fName"></param>
        public static void DeleteFilefromAppPersist(string fName)
        {
            string path = Path.Combine(Application.persistentDataPath, fName);
            if (File.Exists(path)) File.Delete(path);
        }

        /// <summary>
        /// return SHA256 hash
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static string GetChecksum(string file)
        {
            using (FileStream stream = File.OpenRead(file))
            {
                var sha = new SHA256Managed();
                byte[] checksum = sha.ComputeHash(stream);
                return BitConverter.ToString(checksum).Replace("-", String.Empty);
            }
        }

        /// <summary>
        /// return SHA256 hash
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static string GetChecksumBuffered(Stream stream)
        {
            using (var bufferedStream = new BufferedStream(stream, 1024 * 32))
            {
                var sha = new SHA256Managed();
                byte[] checksum = sha.ComputeHash(bufferedStream);
                return BitConverter.ToString(checksum).Replace("-", String.Empty);
            }
        }
    }
	 [Serializable]
    public class  ListWrapper <T> where T : class
    {
        public List<T> list;
        public ListWrapper(List<T> list)
        {
            this.list = list;
        }
    }

    [Serializable]
    public class ListWrapperStruct<T> where T : struct
    {
        public List<T> list;
        public ListWrapperStruct(List<T> list)
        {
            this.list = list;
        }
    }
}