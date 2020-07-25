
using System.Collections.Concurrent;
using System.Threading.Tasks;

using System;
using System.Net;

using Amazon;
using Amazon.Runtime;
using Amazon.S3;

namespace s3iLib
{
    public class AmazonS3ClientMap
    {
        readonly ConcurrentDictionary<string, string> bucket2region = new ConcurrentDictionary<string, string>();
        readonly ConcurrentDictionary<string, AmazonS3Client> region2client = new ConcurrentDictionary<string, AmazonS3Client>();
        public AWSCredentials Credentials { get; protected set; }
        public AmazonS3Client Client { get; protected set; }
        public static RegionEndpoint DefaultRegion { get; set; } = RegionEndpoint.USEast1;
        public AmazonS3ClientMap(AWSCredentials credentials, AmazonS3Client client = null)
        {
            Credentials = credentials;
            Client = client ?? new AmazonS3Client(Credentials, DefaultRegion);
            region2client[Client.Config.RegionEndpoint.SystemName] = Client;
        }
        public async Task<AmazonS3Client> GetClientAsync(string bucketName)
        {
            if (null == Client)
            {
                Client = new AmazonS3Client(Credentials, DefaultRegion);
                region2client[Client.Config.RegionEndpoint.SystemName] = Client;
            }
            if (!bucket2region.TryGetValue(bucketName, out var bucketLocation))
            {
                try{
                    var bucketLocationResponse = await Client.GetBucketLocationAsync(bucketName).ConfigureAwait(false);
                    switch (bucketLocationResponse.HttpStatusCode)
                    {
                        case HttpStatusCode.OK:
                            bucketLocation = bucketLocationResponse.Location?.Value;
                            if (string.IsNullOrWhiteSpace(bucketLocation)) bucketLocation = Client.Config.RegionEndpoint.SystemName;
                            if (bucket2region.TryAdd(bucketName, bucketLocation)) { }
                            break;
                    }
                }
                catch (Exception x)
                {
                    Console.WriteLine($"? {nameof(AmazonS3ClientMap)}.{nameof(GetClientAsync)}({bucketName}[{DefaultRegion.SystemName}]): {x.GetType().Name}");
                    throw;
                }
            }
            return null == bucketLocation ? null : region2client.GetOrAdd(bucketLocation, (regionName) =>
            {
                return new AmazonS3Client(Credentials, new AmazonS3Config { RegionEndpoint = RegionEndpoint.GetBySystemName(regionName), SignatureVersion = "4" });
            });
        }
    }
}
