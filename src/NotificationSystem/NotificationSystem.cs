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

        public NotificationSystem(string dataPath)
        {
            if (File.Exists(dataPath))
            {
                string jsonSource = File.ReadAllText(dataPath);
                m_NotificationGroups = JsonSerializer.Deserialize<NotificationSystem>(jsonSource).m_NotificationGroups;
            }
        }
        public async Task ProcessEvents()
        {
            foreach (NotificationGroup group in m_NotificationGroups)
            {
                foreach (Notification notification in group)
                {

                }
            }
        }

        ~NotificationSystem()
        {

        }
    }
}
