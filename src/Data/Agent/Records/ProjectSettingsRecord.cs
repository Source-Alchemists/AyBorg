using System.ComponentModel.DataAnnotations;

namespace AyBorg.Data.Agent
{
    public record ProjectSettingsRecord
    {
        [Key]
        public Guid DbId { get; set; }

        public bool IsForceResultCommunicationEnabled { get; set; } = false;
    }
}
