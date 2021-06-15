using System;
using System.Text.Json.Serialization;

namespace API.DTOs
{
    public class MessageDTO
    {
        public int Id { get; set; }
        public int SenderId { get; set; }
        public string SenderUserName { get; set; }
        public string SenderPhotoUrl { get; set; }
        public int RecipientId { get; set; }
        public string RecipientUserName { get; set; }
        public string RecipientPhotoUrl { get; set; }
        public string Content { get; set; }
        public DateTime? MessageRead { get; set; }
        public DateTime MessageSent { get; set; }

        [JsonIgnore]
        public bool SenderDeleted { get; set; }

        [JsonIgnore]
        public bool RecipientDeleted { get; set; }
    }
}
