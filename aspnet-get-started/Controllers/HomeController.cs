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
            var connectionStringSettings = ConfigurationManager.ConnectionStrings["AzureStorageConnectionString"];
            string connectionString = connectionStringSettings?.ConnectionString;
            string queueName = ConfigurationManager.AppSettings["AzureQueueName"] ?? "jobs";

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ConfigurationErrorsException("V konfiguraci chybi AzureStorageConnectionString.");
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
    }
}
