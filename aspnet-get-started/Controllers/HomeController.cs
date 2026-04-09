using System;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Azure.Storage.Queues;

namespace aspnet_get_started.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
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
        public async Task<ActionResult> SendMessageToQueue(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                TempData["QueueError"] = "Zprava nesmi byt prazdna.";
                return RedirectToAction("Index");
            }

            try
            {
                await SendMessageToQueueAsync(message);
                TempData["QueueSuccess"] = "Zprava byla odeslana do fronty.";
            }
            catch (Exception ex)
            {
                TempData["QueueError"] = ex.Message;
            }

            return RedirectToAction("Index");
        }

        private static async Task SendMessageToQueueAsync(string message)
        {
            var connectionStringSettings = ConfigurationManager.ConnectionStrings["AzureStorageConnectionString"];
            string connectionString = connectionStringSettings?.ConnectionString;
            string queueName = ConfigurationManager.AppSettings["AzureQueueName"] ?? "jobs";

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ConfigurationErrorsException("V konfiguraci chybi AzureStorageConnectionString.");
            }

            var queueClient = new QueueClient(connectionString, queueName);

            await queueClient.CreateIfNotExistsAsync();

            string encodedMessage = Convert.ToBase64String(Encoding.UTF8.GetBytes(message));

            await queueClient.SendMessageAsync(encodedMessage);
        }
    }
}
