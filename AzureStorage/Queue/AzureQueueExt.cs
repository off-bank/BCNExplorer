﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;

namespace AzureStorage.Queue
{
    public class AzureQueueExt : IQueueExt
    {
        private readonly CloudQueue _queue;

        private readonly Dictionary<string, Type> _types = new Dictionary<string, Type>(); 

        public AzureQueueExt(string conectionString, string queueName)
        {
            queueName = queueName.ToLower();
            var storageAccount = CloudStorageAccount.Parse(conectionString);
            var queueClient = storageAccount.CreateCloudQueueClient();

            _queue = queueClient.GetQueueReference(queueName);
            _queue.CreateIfNotExists();
        }

        private string SerializeObject(object itm)
        {
            var myType = itm.GetType();
            return (from tp in _types where tp.Value == myType select tp.Key + ":" + Newtonsoft.Json.JsonConvert.SerializeObject(itm)).FirstOrDefault();
        }

        private object DeserializeObject(string itm)
        {
            try
            {
                var i = itm.IndexOf(':');

                var typeStr = itm.Substring(0, i);

                if (!_types.ContainsKey(typeStr))
                    return null;

                var data = itm.Substring(i + 1, itm.Count() - i - 1);

                return Newtonsoft.Json.JsonConvert.DeserializeObject(data, _types[typeStr]);
            }
            catch
            {
                return null;
            }
        }

        public async Task<QueueData> GetMessageAsync()
        {
            var msg = await _queue.GetMessageAsync();

            if (msg == null)
                return null;

            return new QueueData
            {
                Token = msg,
                Data = DeserializeObject(msg.AsString)
            };


        }

        public Task FinishMessageAsync(QueueData token)
        {
            var cloudQueueMessage = token.Token as CloudQueueMessage;
            if (cloudQueueMessage == null)
                return Task.FromResult(0);
             
            return _queue.DeleteMessageAsync(cloudQueueMessage);
        }

        public Task PutRawMessageAsync(string msg)
        {
            return _queue.AddMessageAsync(new CloudQueueMessage(msg));
        }

        public void PutMessage(object itm)
        {
            var msg = SerializeObject(itm);
            if (itm == null)
                return;
            _queue.AddMessage(new CloudQueueMessage(msg));
        }

        public async Task<string> PutMessageAsync(object itm)
        {
            var msg = SerializeObject(itm);
            if (msg == null)
                return string.Empty;

            await _queue.AddMessageAsync(new CloudQueueMessage(msg));
            return msg;
        }

        public object GetMessage()
        {
            var msg = _queue.GetMessage();

            if (msg == null)
                return null;

            _queue.DeleteMessage(msg);
            return DeserializeObject(msg.AsString);
        }



        public object[] GetMessages(int maxCount)
        {
            var messages = _queue.GetMessages(maxCount);

            var cloudQueueMessages = messages as CloudQueueMessage[] ?? messages.ToArray();
            foreach (var cloudQueueMessage in cloudQueueMessages)
                _queue.DeleteMessage(cloudQueueMessage);
          
            return cloudQueueMessages
                .Select(message => DeserializeObject(message.AsString))
                .Where(itm => itm != null).ToArray();
        }

        public async Task<object[]> GetMessagesAsync(int maxCount)
        {
            var messages = await _queue.GetMessagesAsync(maxCount);

            var cloudQueueMessages = messages as CloudQueueMessage[] ?? messages.ToArray();
            foreach (var cloudQueueMessage in cloudQueueMessages)
                await _queue.DeleteMessageAsync(cloudQueueMessage);

            return cloudQueueMessages
                .Select(message => DeserializeObject(message.AsString))
                .Where(itm => itm != null).ToArray();
        }

        public void Clear()
        {
            _queue.Clear();
        }

        public void RegisterTypes(params QueueType[] types)
        {
            foreach (var type in types)
                _types.Add(type.Id, type.Type);
        }

    }
}
