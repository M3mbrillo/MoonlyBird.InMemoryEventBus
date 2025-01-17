using System.Runtime.CompilerServices;
using HappySnorlax;
using HappySnorlax.Snorlax;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MoonlyBird.InMemoryEventBus.Abstract;
using MoonlyBird.InMemoryEventBus.Abstract.Model;
using MoonlyBird.InMemoryEventBus.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInMemoryEvent<FeedEvent>()
    .AddHandler<BigSnorlax>()
    .AddHandler<ChadSnorlax>()
    .AddHandler<LazySnorlax>()
    .AddHandler<NormalSnorlax>()
    .AddHandler<AngrySnorlax>()
    ;

var app = builder.Build();

app.MapPost("/snorlax/feed", async ([FromServices] IProducer<FeedEvent> producer) =>
{
    var e = new Event<FeedEvent>(new FeedEvent());
    await producer.PublishAsync(e);
    return Results.Ok();
});

var consumer = app.Services.GetRequiredService<IConsumer<FeedEvent>>();
await consumer.Start();

app.Run();