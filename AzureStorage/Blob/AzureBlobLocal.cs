﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Common;

namespace AzureStorage.Blob
{
    public class AzureBlobLocal : IBlobStorage
    {
        private readonly string _host;

        public AzureBlobLocal(string host)
        {
            _host = host;
        }

        private string CompileRequestString(string container, string id)
        {
            return _host + "/b/" + container+"/"+id;

        }

        private async Task<MemoryStream> GetHttpReqest(string container, string id)
        {
            var oWebRequest =
               (HttpWebRequest)WebRequest.Create(CompileRequestString(container, id));

            oWebRequest.Method = "GET";
            oWebRequest.ContentType = "text/plain";


            var oWebResponse = (HttpWebResponse)(await oWebRequest.GetResponseAsync());


            if ((int) oWebResponse.StatusCode == 201)
                return null;

                var receiveStream = oWebResponse.GetResponseStream();

            if (receiveStream == null)
                throw new Exception("ReceiveStream == null");

            var ms = new MemoryStream();
            receiveStream.CopyTo(ms);
            return ms;
        }

        private async Task<MemoryStream> PostHttpReqest(string container, string id, byte[] data)
        {
            var oWebRequest =
               (HttpWebRequest)WebRequest.Create(CompileRequestString(container, id));

            oWebRequest.Method = "POST";
            oWebRequest.ContentType = "text/plain";

            var stream = oWebRequest.GetRequestStream();
            stream.Write(data, 0, data.Length);

            var oWebResponse = await oWebRequest.GetResponseAsync();
            var receiveStream = oWebResponse.GetResponseStream();


            if (receiveStream == null)
                throw new Exception("ReceiveStream == null");

            var ms = new MemoryStream();
            receiveStream.CopyTo(ms);
            return ms;
        }


        public void SaveBlob(string container, string key, Stream bloblStream)
        {
            SaveBlobAsync(container, key, bloblStream).Wait();
        }

        public async Task<string> SaveBlobAsync(string container, string key, Stream bloblStream, bool anonymousAccess = false)
        {
            await PostHttpReqest(container, key, bloblStream.ToBytes());
            return key;
        }

        public async Task<string> SaveBlobAsync(string container, string key, byte[] blob, bool anonymousAccess = false)
        {
            await PostHttpReqest(container, key, blob);
            return key;
        }


        public Task<bool> HasBlobAsync(string container, string key)
        {
            throw new NotImplementedException();
        }

        public Task<DateTime?> GetBlobsLastModifiedAsync(string container)
        {
            return Task.FromResult((DateTime?)DateTime.UtcNow);
        }

        public Stream this[string container, string key] => GetAsync(container, key).Result;

        public async Task<Stream> GetAsync(string blobContainer, string key)
        {
            return await GetHttpReqest(blobContainer, key);
        }

        public Task<string> GetAsTextAsync(string blobContainer, string key)
        {
            throw new NotImplementedException();
        }

        public string GetBlobUrl(string container, string key)
        {
            return string.Empty;
        }

        public string[] FindNamesByPrefix(string container, string prefix)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetListOfBlobs(string container)
        {
            throw new NotImplementedException();
        }

        public void DelBlob(string container, string key)
        {
            throw new NotImplementedException();
        }

        public Task DelBlobAsync(string blobContainer, string key)
        {
            throw new NotImplementedException();
        }
    }
}
