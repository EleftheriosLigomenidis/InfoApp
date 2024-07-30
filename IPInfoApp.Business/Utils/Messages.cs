using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPInfoApp.Business.Utils
{
    public  class Messages
    {
        public static string CacheKeyNull() => "Cache key cannot be null or empty.";
        public static string BatchProcessed(string batchNumber) => $"Batch {batchNumber} processed successfully.";
        public static string UpdateIpInformationStarted() => "UpdateIpInformation has started mass updating ip addresses.";
        public static string UpdateIpInformationFailed() => "UpdateIpInformation has encountered an error.";
        public static string WebServiceBulkOperation() => "Performing bulk call to ip2c WebSerice";
        public static string WebServiceBulkOperationFailed() => "An error occured during bulk call to ip2c Web Service for multiple addresses";

        public static string ApiKeyNotProvided() => "Api key is not provided";
        public static string Unauthorised() => "Unauthorised request";
        public static string FetchInformation(string ip) => $"Could not fetch country information for ip {ip}";
        public static string FetchCollection(string entityName,string source) =>
              $"Fetching collection of {entityName}s from  {source}";
        public static string FetchEntity(string entityName, string identifier, string id,string source) =>
        $"Fetching {entityName} from {source} with {identifier} {id}.";

        public static string FetchCollectionFailed(string entityName) =>
    $"Fetching collection of {entityName}s failed.";

        public static string CreatingEntity(string entityName) =>
     $"Creating {entityName}.";

        public static string CreateEntityFailed(string entityName) =>
        $"Creating entity {entityName} failed.";
        public static string FetchEntityFailed(string entityName,string source,string id) => $"Fetching {entityName} from {source} with identifier {id}.";
    }
}
