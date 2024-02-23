using MassTransit;
using ScheduledSendStateMachine.Contracts;

namespace ScheduledSendStateMachine;

public class DelayedSendStateMachine : MassTransitStateMachine<DelayedSendState>
{
    public State Created { get; private set; } = null!;
    public State AwaitingUpdate { get; private set; } = null!;
    public State Sent { get; private set; } = null!;
    
    public Event<AppointmentCreated> AppointmentCreated { get; set; }
    public Schedule<DelayedSendState, SendDelayedEmailMessage> SendDelayedEmailMessageSchedule { get; }
    public Event<SendDelayedEmailMessage> SendDelayedEmailMessage { get; }

    static DelayedSendStateMachine()
    {
        GlobalTopology.Send.UseCorrelationId<SendDelayedEmailMessage>(x => x.AppointmentUid);
    }
    
    public DelayedSendStateMachine()
    {
        InstanceState(m => m.CurrentState);
        Event(() => AppointmentCreated, configurator =>
        {
            configurator.CorrelateById(context => context.Message.AppointmentId);
            configurator.InsertOnInitial = true;
        } );

        // Event(() => CommunicationSendNotification, c =>
        // {
        //     c.CorrelateById(context => context.Message.AppointmentUid);
        // });
        
        Schedule(() => SendDelayedEmailMessageSchedule, x => x.DeliverDelayedEmailMessageTimeoutTokenId,
            x =>
            {
                // x.Delay = TimeSpan.FromSeconds(5);
                // x.Received = r => r.CorrelateById(context => context.Message.AppointmentUid);
                x.DelayProvider = context => TimeSpan.FromSeconds(context.Saga.SendingDelayInSec);
            });

        Initially(
            // Behavior Starts
            When(AppointmentCreated)
                .Then(context =>
                {
                    context.Saga.CorrelationId = context.Message.AppointmentId;
                    context.Saga.SendingDelayInSec = context.Message.SendingDelayInSec;
                    context.Saga.Email = context.Message.Email;
                })
                .TransitionTo(Created)
        );
        
        WhenEnter(Created, s => 
            s.IfElse(context => context.Saga.SendingDelayInSec > 0 , 
                e =>
                    e.Schedule(SendDelayedEmailMessageSchedule, c =>c.Init<SendDelayedEmailMessage>(
                        new
                        {
                            AppointmentUid = c.Saga.CorrelationId
                        }))
                        .TransitionTo(AwaitingUpdate)
            , context => 
                    context
                        .Send(c => new SendDelayedEmailMessage()
                        {
                             Email = c.Saga.Email,
                            AppointmentUid = c.Saga.CorrelationId
                        })
                        .TransitionTo(Sent) )
                );
   
        When(SendDelayedEmailMessage)
            .PublishSendEvent()
            .TransitionTo(Sent);
    }
}

public class AppointmentSendTriggeredEvent
{
    public Guid AppointmentUid { get; set; }
    public string Email { get; set; }
}