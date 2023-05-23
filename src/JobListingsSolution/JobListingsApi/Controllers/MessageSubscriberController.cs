using DotNetCore.CAP;
using Microsoft.AspNetCore.Mvc;
using DotNetCore.CAP;
using Microsoft.AspNetCore.Mvc;
using DomainEvents = MessageContracts.JobsApi;
using JobListingsApi.Models;
using Marten;

namespace JobListingsApi.Controllers;

public class MessageSubscriberController : ControllerBase
{

    private readonly ILogger<MessageSubscriberController> _logger;
    private readonly IDocumentSession _session;

    public MessageSubscriberController(ILogger<MessageSubscriberController> logger)
    {

        _logger = logger;
    }


    [HttpPost("cap-stuff")]
    [CapSubscribe("JobsApi.JobCreated")]
    public async Task<ActionResult> GetNewJob([FromBody] DomainEvents.JobCreated request)
    {
        _logger.LogInformation($"Got a job created request {request.Id}, {request.Title}");

        var job = new JobModel
        { 
            Id = request.Id,
            Title = request.Title
        };

        _session.Store(job);
        await _session.SaveChangesAsync();  
        
       // _logger.LogInformation($"Got a job created request {request.Id}, {request.Title}");
        return Ok();
    }
}



