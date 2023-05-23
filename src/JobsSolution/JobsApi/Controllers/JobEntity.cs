namespace JobsApi.Controllers
{
    public class JobEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Slug { get; set; } = "";

        public string Title { get; set; } = "";

        public string Description { get; set; } = "";

        public bool IsRetired { get; set; } = false;
    }
}
