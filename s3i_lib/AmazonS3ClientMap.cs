using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;

using Amazon;
using Amazon.Runtime;
using Amazon.S3;

namespace s3i_lib
{
    public class AmazonS3ClientMap
    {
        protected ConcurrentDictionary<string, string> bucket2region = new ConcurrentDictionary<string, string>();
        protected ConcurrentDictionary<string, AmazonS3Client> region2client = new ConcurrentDictionary<string, AmazonS3Client>();    // region -> client
        public AWSCredentials Credentials { get; protected set; }
        public AmazonS3Client Client { get; protected set; }
        public AmazonS3ClientMap(AWSCredentials credentials, AmazonS3Client client = null)
        {
            Credentials = credentials;
            Client = client ?? new AmazonS3Client(Credentials, RegionEndpoint.USEast1);
            region2client[Client.Config.RegionEndpoint.SystemName] = Client;
        }
        public async Task<AmazonS3Client> GetClientAsync(string bucketName)
        {
            string regionName;
            if (!bucket2region.TryGetValue(bucketName, out regionName))
            {
                var bucketLocationResponse = await Client.GetBucketLocationAsync(bucketName);
                switch (bucketLocationResponse.HttpStatusCode)
                {
                    case HttpStatusCode.OK:
                        regionName = bucketLocationResponse.Location?.Value;
                        if (string.IsNullOrWhiteSpace(regionName)) regionName = RegionEndpoint.USEast1.SystemName;
                        if (bucket2region.TryAdd(bucketName, regionName)) { }
                        break;
                }
            }
            return region2client.GetOrAdd(regionName, (_regionName) =>
            {
                return new AmazonS3Client(Credentials, new AmazonS3Config { RegionEndpoint = RegionEndpoint.GetBySystemName(_regionName), SignatureVersion = "4" });
            });
        }
    }
}
