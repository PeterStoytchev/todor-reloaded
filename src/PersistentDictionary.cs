using DSharpPlus;
using DSharpPlus.CommandsNext;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace todor_reloaded
{
    public class PersistentDictionary
    { 
        private List<string[]> keys;
        private List<Song> values;

        private string filePath;

        private bool isUptodate;

        public PersistentDictionary(string path)
        {
            filePath = path;
            string dirPath = path.Substring(0, path.LastIndexOf("/"));

            if (Directory.Exists(dirPath) == false)
            {
                Directory.CreateDirectory(dirPath);
                FileStream fs = File.Create(path);
                fs.Close();
            }

            LoadDictionary();
        }

        ~PersistentDictionary()
        {
            if (!isUptodate)
            {
                StoreDictionary();
            }

            values.Clear();
            keys.Clear();
        }

        public Song Get(string[] key)
        {
            for (int j = 0; j < keys.Count; j++)
            {
                if (Compare(keys[j], key)) { return values[j]; }
            }

            return null;
        }

        public int GetIndex(string[] key)
        {
            for (int j = 0; j < keys.Count; j++)
            {
                if (Compare(keys[j], key)) { return j; }
            }

            return -1;
        }

        public void Add(string[] key, Song value)
        {
            if (keys.Contains(key))
            {
                Update(key, value);
            }
            else
            {
                keys.Add(key);
                values.Add(value);
            }

            isUptodate = false;
        }

        public void Update(string[] key, Song value)
        {
            int valueIndex = GetIndex(key);

            if (valueIndex != -1)
            {
                values[valueIndex] = value;

                isUptodate = false;
            }
            else
            {
                throw new Exception($"Value for {key} not found in the dictionary!");
            }
        }

        public void Remove(string[] key)
        {
            int index = GetIndex(key);
            if (index != -1)
            {
                keys.RemoveAt(index);
                values.RemoveAt(index);

                isUptodate = false;
            }
            else
            {
                throw new Exception($"Value for {key} not found in the dictionary!");
            }
        }
        
        public Dictionary<string[], Song> GetPairs()
        {
            Dictionary<string[], Song> pairs = new Dictionary<string[], Song>(values.Count);

            for (int i = 0; i < values.Count; i++)
            {
                pairs.Add(keys[i], values[i]);
            }

            return pairs;
        }

        public void StoreDictionary()
        {
            FileStream fileStream = new FileStream(filePath, FileMode.OpenOrCreate);
            BinaryFormatter bf = new BinaryFormatter();
            
            fileStream.Seek(0, SeekOrigin.Begin);

            Dictionary<string[], Song> pairs = new Dictionary<string[], Song>(values.Count);

            for (int i = 0; i < values.Count; i++)
            {
                pairs.Add(keys[i], values[i]);
            }

            bf.Serialize(fileStream, pairs);

            isUptodate = true;

            fileStream.Close();

            pairs.Clear();
        }


        private void LoadDictionary()
        {
            FileStream fileStream = new FileStream(filePath, FileMode.OpenOrCreate);

            fileStream.Seek(0, SeekOrigin.Begin);

            BinaryFormatter bf = new BinaryFormatter();

            Dictionary<string[], Song> pairs = new Dictionary<string[], Song>();

            //load the dictionaty from file or create a new one
            if (fileStream.Length != 0)
            {
                pairs = bf.Deserialize(fileStream) as Dictionary<string[], Song>;
            }
            else
            {
                pairs = new Dictionary<string[], Song>();
            }

            keys = new List<string[]>(pairs.Count);
            values = new List<Song>(pairs.Count);

            int index = 0;
            foreach (KeyValuePair<string[], Song> pair in pairs)
            {
                keys.Add(pair.Key);
                values.Add(pair.Value);
                index++;
            }

            fileStream.Close();

            pairs.Clear();
        }

        private bool Compare(string[] arr1, string[] arr2)
        {
            if (arr1.Length != arr2.Length) { return false; }

            int count = 0;
            for (int i = 0; i < arr2.Length; i++)
            {
                if (arr1[i] == arr2[i]) { count++; }
            }

            if (count == arr2.Length) { return true; }

            return false;
        }

    }
}
