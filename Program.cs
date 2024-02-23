using MassTransit;
using Microsoft.AspNetCore.Http.HttpResults;
using ScheduledSendStateMachine;
using ScheduledSendStateMachine.Contracts;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.SetupMasstransit();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.MapPost("/test", async (IEventHubProducerProvider producerProvider) =>
    {
        var producer = await producerProvider.GetProducer("evh-appointments-created");
        await producer.Produce(new AppointmentCreated
        {
            AppointmentId = Guid.NewGuid(),
            Email = "someone@email.com",
            SendingDelayInSec = 5
        });
    })
    .WithName("Test Produce Event")
    .WithOpenApi();

app.Run();

