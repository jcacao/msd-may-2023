using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageContracts.JobsApi;

public record JobCreated
{
    public readonly string MessageId = "JobsApi.JobCreated";   
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";   
}

public record JobRetired
{
    public static readonly string MessageId = "JobsApi.JobRetired";
    public string Id { get; set; } = "";
}
