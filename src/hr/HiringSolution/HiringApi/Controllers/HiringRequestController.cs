using DotNetCore.CAP;
using Microsoft.AspNetCore.Mvc;
using AclEvents = MessageContracts.HrAcl;
namespace HiringApi.Controllers;

public class HiringRequestsController : ControllerBase
{
    private readonly ICapPublisher _capPublisher;

    public HiringRequestsController(ICapPublisher capPublisher)
    {
        _capPublisher = capPublisher;
    }

    [HttpPost("/hiringrequests")]
    [CapSubscribe("HrAcl.HiringRequestCreated")]
    public async Task<ActionResult> ProcessHiringRequestAsync([FromBody] AclEvents.HiringRequestCreated request)
    {
        if (request.EmailAddress.ToLower().Contains("geico.com"))
        {
            // Publish the sad path (HiringRequestDenied)
            return BadRequest("Not allowed to hire from there, sorry. ");
        }
        else
        {
            // Publish the happy path (EmployeeCreated?)
            var eventToPublish = new AclEvents.EmployeeHired
            {
                EmailAddress = request.EmailAddress,
                FirstName = request.FirstName,
                LastName = request.LastName,
                JobId = request.JobId
            };

            var headers = new Dictionary<string, string?>()
             {
             { "offering-id", request.OfferingId.ToString() }
             };
            await _capPublisher.PublishAsync(AclEvents.EmployeeHired.MessageId, eventToPublish, headers);
            return Ok(request);
        }
    }
}