using JobListingsApi.Models;
using Marten;
using Microsoft.AspNetCore.Mvc;

using JobListingsApi.Models;
using Marten;
using Microsoft.AspNetCore.Mvc;
using DotNetCore.CAP;
using DomainEvents = MessageContracts.JobListingsApi;

namespace JobListingsApi.Controllers;

public class JobListingsController : ControllerBase
{


    private readonly ICapPublisher _publisher;
    private readonly IDocumentSession _documentSession;
    public JobListingsController(IDocumentSession documentSession)
    {

        _documentSession = documentSession;
    }

    [HttpPost("/job-listings/{slug}/openings")]
    public async Task<ActionResult> AddJobListing([FromRoute] string slug, [FromBody] JobListingCreateModel request)
    {

        var savedJob = await _documentSession.Query<JobModel>().Where(job => job.Id == slug).SingleOrDefaultAsync();

        if (savedJob != null)
        {
            var jobToAdd = new JobListingEntity
            {

                JobId = slug,

                JobName = savedJob.Title,

                OpeningStartDate = request.OpeningStartDate,

                SalaryRange = request.SalaryRange,
            };
            _documentSession.Insert(jobToAdd);
            await _documentSession.SaveChangesAsync();

            //publish the event that there is a new job listing.
            var domainEvent = new DomainEvents.JobListingCreated
            {
                Id = jobToAdd.Id.ToString(),
                JobId = jobToAdd.JobId,
                JobName = jobToAdd.JobName,
                OpeningStartDate = jobToAdd.OpeningStartDate,
                SalaryRange = new DomainEvents.Salaryrange
                {
                    Min = jobToAdd.SalaryRange.Min.Value,
                    Max = jobToAdd.SalaryRange.Max.Value
                }
            };
            await _publisher.PublishAsync(DomainEvents.JobListingCreated.MessageId, domainEvent);                       
            return StatusCode(201, jobToAdd);
        }
        else
        {
            return NotFound();
        }
    }
}



