using DotNetCore.CAP;
using Marten;
using MessageContracts.JobsApi;
using JobDomainEvents = MessageContracts.JobsApi;
using JobOfferingEvents = MessageContracts.JobListingsApi;
using WebEvents = MessageContracts.WebMessages;
using AclEvents = MessageContracts.HrAcl;
namespace Acl;



public class DataSubscribers : ICapSubscribe
{



    private readonly ICapPublisher _publisher;
    private readonly IDocumentSession _documentSession;
    private readonly ILogger<DataSubscribers> _logger;



    public DataSubscribers(IDocumentSession documentSession, ILogger<DataSubscribers> logger, ICapPublisher publisher)
    {
        _documentSession = documentSession;
        _logger = logger;
        _publisher = publisher;
    }



    [CapSubscribe("JobsApi.JobCreated")]
    public async Task SaveJobAsync(JobDomainEvents.JobCreated jobCreated)
    {
        _logger.LogInformation($"Got a JobCreated {jobCreated.Title}");
        _documentSession.Store(jobCreated);
        await _documentSession.SaveChangesAsync();
    }



    [CapSubscribe("JobListings.JobListingCreated")]
    public async Task SaveJobOfferAsync(JobOfferingEvents.JobListingCreated offer)
    {
        _logger.LogInformation($"Got a JobListingCreated {offer.JobName}");
        _documentSession.Store(offer);
        await _documentSession.SaveChangesAsync();
    }



    [CapSubscribe("WebMessages.ApplicantCreated")]
    public async Task SaveApplicantAsync(WebEvents.ApplicantCreated applicant)
    {
        _logger.LogInformation($"Got a ApplicantCreated {applicant.EmailAddress}");
        _documentSession.Store(applicant);
        await _documentSession.SaveChangesAsync();
    }



    [CapSubscribe("WebMessages.JobApplicationCreated")]
    public async Task TurnApplicationIntoHiringRequestAsync(WebEvents.JobApplicationCreated application)
    {
        // we need the applicant.
        var applicant = await _documentSession
         .Query<WebEvents.ApplicantCreated>()
         .Where(a => a.UserId == application.ApplicantId).SingleAsync();
                var offer = await _documentSession
         .Query<JobOfferingEvents.JobListingCreated>()
         .Where(o => o.Id == application.JobOfferingId).SingleAsync();



        var messageToPublish = new AclEvents.HiringRequestCreated
        {
            EmailAddress = applicant.EmailAddress,
            FirstName = applicant.FirstName,
            LastName = applicant.LastName,
            JobId = offer.JobId,
            OfferingId = application.JobOfferingId
        };

        await _publisher.PublishAsync(AclEvents.HiringRequestCreated.MessageId, messageToPublish);

    }
}