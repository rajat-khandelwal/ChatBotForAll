namespace ChatBotForAll.ApiService.Entities
{
    public class Tenant : DefaultColumns
    {
        public Guid TenantId { get; set; }

        public string TenantName { get; set; }

        // unique slug for tenant, used for url and other unique identification
        public string Slug { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
