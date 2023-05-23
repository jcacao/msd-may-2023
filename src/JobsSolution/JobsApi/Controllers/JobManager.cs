﻿using DotNetCore.CAP;
using JasperFx.Core;
using Marten;
using MessageContracts.JobsApi;
using SlugGenerators;
using System.Reflection;
using System.Runtime.InteropServices;
using DomainEvents = MessageContracts.JobsApi;

namespace JobsApi.Controllers;

public class JobManager
{

 private readonly SlugGenerator _slugGenerator;
    private readonly IDocumentStore _documentStore;
    private readonly ICapPublisher _publisher;
    public JobManager(SlugGenerator slugGenerator, IDocumentStore documentStore, ICapPublisher publisher)

    {
        _slugGenerator = slugGenerator;
        _documentStore = documentStore;
        _publisher = publisher;
    }
    public async Task<JobItemModel> CreateJobAsync(JobCreateItem request)
    {
        //var response = new JobItemModel
        var jobToSave = new JobEntity
        {

            //Id = await _slugGenerator.GenerateSlugForAsync(request.Title),
            Title = request.Title,
            Slug = await _slugGenerator.GenerateSlugForAsync(request.Title),
            Description = request.Description,
        };
        //return response;

        using var session = _documentStore.LightweightSession();
        session.Insert(jobToSave);

        await session.SaveChangesAsync();

        // Map it to the JobItemModel that we return to the client.
        var response = new JobItemModel
        {
            Id = jobToSave.Slug,
            Title = jobToSave.Title,
            Description = jobToSave.Description,
        };
        // Write that event out  *somewhere*.
        var domainEvent = new DomainEvents.JobCreated
        {
            Id = response.Id,
            Title = response.Title,
            Description = response.Description,
        };
        await _publisher.PublishAsync(DomainEvents.JobCreated.MessageId, domainEvent);
        return response;
    }

    public async Task<CollectionResponse<JobItemModel>> GetAllCurrentJobsAsync()
    {
        // Here we will have to map from JobEntity -> JobItemModel
        using var session = _documentStore.LightweightSession();
        var jobs = await session.Query<JobEntity>()
        .Where(j => j.IsRetired == false)
        .Select(job => new JobItemModel
        {
            Title = job.Title,
            Id = job.Slug,
            Description = job.Description,
        }).ToListAsync();

        return new CollectionResponse<JobItemModel> { Data = jobs.ToList() };
    }

    public async Task<JobItemModel?> GetJobBySlugAsync(string slug)
    {
        using var session = _documentStore.LightweightSession();
        var job = await session.Query<JobEntity>()
        .Where(j => j.IsRetired == false && j.Slug == slug)
        .Select(job => new JobItemModel
        {
            Title = job.Title,
            Id = job.Slug,
            Description = job.Description,
        }).SingleOrDefaultAsync();
        return job;
    }

    public async Task<bool> CheckForJobAsync(string slug)
    {
        using var session = _documentStore.LightweightSession();
        return await session.Query<JobEntity>()
        .Where(j => j.IsRetired == false && j.Slug == slug)
        .AnyAsync();
    }
}