using System;
using System.Configuration;
using System.Threading.Tasks;
using System.Web.Mvc;
using Azure.Storage.Queues;
using Newtonsoft.Json;
using aspnet_get_started.Models;

namespace aspnet_get_started.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View(new QueueRequestViewModel());
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendMessageToQueue(QueueRequestViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            try
            {
                BackgroundJobMessage job = await SendMessageToQueueAsync(model);

                ViewBag.QueueSuccess = $"Pozadavek byl zarazen do fronty. Job ID: {job.JobId}";

                return View("Index", new QueueRequestViewModel());
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View("Index", model);
            }
        }

        private static async Task<BackgroundJobMessage> SendMessageToQueueAsync(QueueRequestViewModel model)
        {
            string connectionString = GetAzureStorageConnectionString();
            string queueName = ConfigurationManager.AppSettings["AzureQueueName"] ?? "jobs";

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ConfigurationErrorsException("V konfiguraci chybi AzureStorageConnectionString.");
            }

            if (!IsValidAzureStorageConnectionString(connectionString))
            {
                throw new ConfigurationErrorsException("AzureStorageConnectionString nema platny format. Ocekava se Azure Storage connection string ve tvaru DefaultEndpointsProtocol=...;AccountName=...;AccountKey=...;EndpointSuffix=...");
            }

            var queueClient = new QueueClient(
                connectionString,
                queueName,
                new QueueClientOptions
                {
                    MessageEncoding = QueueMessageEncoding.None
                });

            await queueClient.CreateIfNotExistsAsync();

            var job = new BackgroundJobMessage
            {
                JobId = Guid.NewGuid(),
                Type = "processContactRequest",
                Subject = model.Subject,
                Message = model.Message,
                Priority = model.Priority,
                CreatedAtUtc = DateTime.UtcNow,
                Source = "web-app"
            };

            string payload = JsonConvert.SerializeObject(job);
            await queueClient.SendMessageAsync(payload);

            return job;
        }

        private static string GetAzureStorageConnectionString()
        {
            string[] candidates =
            {
                Environment.GetEnvironmentVariable("AzureStorageConnectionString"),
                Environment.GetEnvironmentVariable("APPSETTING_AzureStorageConnectionString"),
                Environment.GetEnvironmentVariable("CUSTOMCONNSTR_AzureStorageConnectionString"),
                Environment.GetEnvironmentVariable("AzureWebJobsStorage"),
                Environment.GetEnvironmentVariable("APPSETTING_AzureWebJobsStorage"),
                Environment.GetEnvironmentVariable("CUSTOMCONNSTR_AzureWebJobsStorage"),
                ConfigurationManager.AppSettings["AzureStorageConnectionString"],
                ConfigurationManager.AppSettings["AzureWebJobsStorage"],
                ConfigurationManager.ConnectionStrings["AzureWebJobsStorage"]?.ConnectionString,
                ConfigurationManager.ConnectionStrings["AzureStorageConnectionString"]?.ConnectionString
            };

            foreach (string candidate in candidates)
            {
                if (!string.IsNullOrWhiteSpace(candidate))
                {
                    return candidate.Trim();
                }
            }

            return null;
        }

        private static bool IsValidAzureStorageConnectionString(string connectionString)
        {
            if (string.Equals(connectionString, "UseDevelopmentStorage=true", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            string[] settings = connectionString.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            if (settings.Length == 0)
            {
                return false;
            }

            foreach (string setting in settings)
            {
                if (setting.IndexOf('=') <= 0)
                {
                    return false;
                }
            }

            return connectionString.IndexOf("AccountName=", StringComparison.OrdinalIgnoreCase) >= 0
                && (connectionString.IndexOf("AccountKey=", StringComparison.OrdinalIgnoreCase) >= 0
                    || connectionString.IndexOf("SharedAccessSignature=", StringComparison.OrdinalIgnoreCase) >= 0);
        }
    }
}
