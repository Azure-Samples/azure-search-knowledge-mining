// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Auth;
using System.Threading.Tasks;

namespace CognitiveSearch.UI.Controllers
{
    public class StorageController : Controller
    {
        private IConfiguration _configuration { get; set; }
        private DocumentSearchClient _docSearch { get; set; }

        public StorageController(IConfiguration configuration)
        {
            _configuration = configuration;
            _docSearch = new DocumentSearchClient(configuration);
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload()
        {
            if (Request.Form.Files.Any())
            {
                var container = GetStorageContainer();

                foreach (var formFile in Request.Form.Files)
                {
                    if (formFile.Length > 0)
                    {
                        var cloudBlockBlob = container.GetBlockBlobReference(formFile.FileName);
                        await cloudBlockBlob.UploadFromStreamAsync(formFile.OpenReadStream());
                    }
                }
            }

            await _docSearch.RunIndexer();

            return new JsonResult("ok");
        }

        private CloudBlobContainer GetStorageContainer()
        {
            string accountName = _configuration.GetSection("StorageAccountName")?.Value;
            string accountKey = _configuration.GetSection("StorageAccountKey")?.Value;
            var containerAddress = _configuration.GetSection("StorageContainerAddress")?.Value.ToLower();

            var container = new CloudBlobContainer(new Uri(containerAddress), new StorageCredentials(accountName, accountKey));
            return container;
        }
    }
}
