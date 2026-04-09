using System;

namespace aspnet_get_started.Models
{
    public class BackgroundJobMessage
    {
        public Guid JobId { get; set; }

        public string Type { get; set; }

        public string Subject { get; set; }

        public string Message { get; set; }

        public string Priority { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        public string Source { get; set; }
    }
}
