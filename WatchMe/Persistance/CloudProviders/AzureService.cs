using Azure.Storage.Blobs;
using WatchMe.Helpers;

namespace WatchMe.Persistance.CloudProviders
{
    public class AzureService : ICloudProviderService
    {
        private const string AZURESTORAGECONTAINERCONNECTIONSTRINGKEY = "azure_storagecontainer_connectionstring";
        public async Task UploadContentToCloud(Stream fileStream, string contentName)
        {
            //Right now, we only have Azure configured.

            string storageContainerConnectionString = await SecureStorage.Default.GetAsync(AZURESTORAGECONTAINERCONNECTIONSTRINGKEY);

            if (storageContainerConnectionString == null)
            {
                throw new Exception($"unable to gather {AZURESTORAGECONTAINERCONNECTIONSTRINGKEY} from secure storage");
            }
            try
            {
                var blobServiceClient = new BlobServiceClient(storageContainerConnectionString);

                string containerName = "watchmefileuploadcontainer";

                var containerClient = blobServiceClient.GetBlobContainerClient(containerName);


                BlobClient blobClient = containerClient.GetBlobClient(contentName);
                var result = await blobClient.UploadAsync(fileStream, true);
            }
            catch (Exception ex)
            {
                ToastHelper.CreateToast("Issue uploading to SC");
            }
        }

        public async Task<string> GetAzureConnectionString() =>
            await SecureStorage.Default.GetAsync(AZURESTORAGECONTAINERCONNECTIONSTRINGKEY);

        public async Task SetAzureConnectionString(string connstr) =>
            await SecureStorage.Default.SetAsync(AZURESTORAGECONTAINERCONNECTIONSTRINGKEY, connstr);
    }
}
