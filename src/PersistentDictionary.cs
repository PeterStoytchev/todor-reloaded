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
    public class PersistentDictionary<KeyType, ValueType>
    {
        private Dictionary<KeyType, ValueType> pairs;

        private string filePath;

        private bool isUptodate;


        public PersistentDictionary(string path)
        {
            filePath = path;

            LoadDictionary();
        }

        ~PersistentDictionary()
        {
            if (!isUptodate)
            {
                StoreDictionary();
            }

            pairs.Clear();
        }

        public ValueType Get(KeyType key)
        {
            ValueType val;

            if (pairs.TryGetValue(key, out val))
            {
                return val;
            }
            else
            {
                throw new Exception($"Value for {key} not found in the dictionary!");
            }
        }


        public void Add(KeyType key, ValueType value)
        {
            if (pairs.ContainsKey(key))
            {
                Update(key, value);
            }
            else
            {
                pairs.Add(key, value);
            }

            isUptodate = false;
        }

        public void Update(KeyType key, ValueType value)
        {
            if (pairs.Remove(key))
            {
                pairs.Add(key, value);

                isUptodate = false;
            }
            else
            {
                throw new Exception($"Value for {key} not found in the dictionary!");
            }
        }

        public void Remove(KeyType key)
        {
            if (!pairs.Remove(key))
            {
                throw new Exception($"Value for {key} not found in the dictionary!");
            }

            isUptodate = false;
        }

        public bool Contains(KeyType key)
        {
            return pairs.ContainsKey(key);
        }

        public KeyValuePair<KeyType, ValueType>[] GetPairs()
        {
            return pairs.ToArray();
        }

        public void StoreDictionary()
        {
            FileStream fileStream = new FileStream(filePath, FileMode.OpenOrCreate);
            BinaryFormatter bf = new BinaryFormatter();
            
            fileStream.Seek(0, SeekOrigin.Begin);

            bf.Serialize(fileStream, pairs);

            isUptodate = true;

            fileStream.Close();

        }


        private void LoadDictionary()
        {
            FileStream fileStream = new FileStream(filePath, FileMode.OpenOrCreate);

            fileStream.Seek(0, SeekOrigin.Begin);

            BinaryFormatter bf = new BinaryFormatter();

            //load the dictionaty from file or create a new one
            if (fileStream.Length != 0)
            {
                pairs = bf.Deserialize(fileStream) as Dictionary<KeyType, ValueType>;
            }
            else
            {
                pairs = new Dictionary<KeyType, ValueType>();
            }

            fileStream.Close();

        }

    }
}
