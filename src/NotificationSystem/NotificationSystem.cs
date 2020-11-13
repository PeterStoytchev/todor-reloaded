using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

using System.Threading.Tasks;

namespace todor_reloaded
{
    class NotificationSystem
    {
        private List<NotificationGroup> m_NotificationGroups { get; set; }
        private string dataPath;

        public NotificationSystem(string dataPath)
        {
            this.dataPath = dataPath;
            if (File.Exists(dataPath))
            {
                string jsonSource = File.ReadAllText(dataPath);
                m_NotificationGroups = JsonSerializer.Deserialize<NotificationSystem>(jsonSource).m_NotificationGroups;
            }
        }
        public async Task ProcessEvents()
        {
            while (true)
            {
                //could run the loop bellow in parralel wit tasks but given the scale at which the bot runs, it will not be necessary, at least for now
                foreach (NotificationGroup group in m_NotificationGroups)
                {
                    group.ProcessNotificaitons(global.bot);
                }

                await Task.Delay(1000); //wait for a second, before processing again
            }
        }

        ~NotificationSystem()
        {
            string output = JsonSerializer.Serialize(this);

            File.WriteAllText(dataPath, output);
        }
    }
}
