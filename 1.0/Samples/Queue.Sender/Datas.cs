using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;

namespace Queue.Sender
{
    public static class Datas
    {
        public static IEnumerable<Message> CreateMessages() => OnCreateMessages();

        private static IEnumerable<Message> OnCreateMessages()
        {
            var datas = Datas.CreateDatas();
            var messages =  new List<Message>();
            for (int i = 0; i < datas.Length; i++)
            {
                string sessionId = Guid.NewGuid().ToString();
                var message = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(datas[i])))
                {
                    SessionId = sessionId,
                    ContentType = "application/json",
                    Label = "RecipeStep",
                    MessageId = i.ToString(),
                    TimeToLive = TimeSpan.FromMinutes(2)
                };
                messages.Add(message);
            }
            return messages;
        }

        public static dynamic CreateDatas() => OnCreateDatas();

        private static dynamic OnCreateDatas()
        {
            dynamic datas = new[]
            {
                new {step = 1, title = "Shop"},
                new {step = 2, title = "Unpack"},
                new {step = 3, title = "Prepare"},
                new {step = 4, title = "Cook"},
                new {step = 5, title = "Eat"},
            };
            return datas;
        }
    }
}
