using Hangfire;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies;
using Hangfire.Mongo.Migration.Strategies.Backup;
using MassTransit;
using ScheduledSendStateMachine.Contracts;

namespace ScheduledSendStateMachine;

public static class ServiceCollectionExtensions
{
    private const string MongoDbName = "my-service-db";
    private const string MongoConnectionString = "mongodb://127.0.0.1";
    private const string SagaCollectionName = "delayed-send-saga";

    public static IServiceCollection SetupMasstransit(this IServiceCollection services)
    {
        services.AddHangfire(h =>
        {
            h.UseRecommendedSerializerSettings();
            h.UseMongoStorage(MongoConnectionString, MongoDbName, new MongoStorageOptions
            {
                CheckQueuedJobsStrategy = CheckQueuedJobsStrategy.TailNotificationsCollection,
                MigrationOptions = new MongoMigrationOptions
                {
                    MigrationStrategy = new MigrateMongoMigrationStrategy(),
                    BackupStrategy = new CollectionMongoBackupStrategy()
                },
                Prefix = "hangfire",
                CheckConnection = true
            });
        });
        
        services.AddMassTransit(configurator =>
        {
            configurator.AddPublishMessageScheduler();
            configurator.AddHangfireConsumers();
   
            configurator.UsingInMemory((context, factoryConfigurator) =>
            {
                factoryConfigurator.UsePublishMessageScheduler();
                factoryConfigurator.ConfigureEndpoints(context);
            } );

            configurator.AddRider(rider =>
            {
                
                rider.AddSagaStateMachine<DelayedSendStateMachine, DelayedSendState>()
                    .MongoDbRepository(r =>
                    {
                        r.Connection = MongoConnectionString;
                        r.DatabaseName = MongoDbName;
                        r.CollectionName = SagaCollectionName;
                    });
                
                rider.UsingEventHub((context, factoryConfigurator) =>
                {
                    factoryConfigurator.Host("FIXME");
                    factoryConfigurator.Storage("FIXME");
                    
                    factoryConfigurator.ReceiveEndpoint("evh-appointments-created", "my-service-group",
                        endpointConfigurator =>
                        {
                            endpointConfigurator.ConfigureSaga<DelayedSendState>(context);
                        });
                });
            });
        });
        
        return services;
    }
}