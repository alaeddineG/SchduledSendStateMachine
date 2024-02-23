using MassTransit;
using ScheduledSendStateMachine.Contracts;

namespace ScheduledSendStateMachine;

public static class EventActivityBinderExtensions
{
    public static EventActivityBinder<DelayedSendState, T> PublishSendEvent<T>(this EventActivityBinder<DelayedSendState, T> binder)
        where T : class
    {
        return binder.Produce(x =>
            "evh-appointments-send-triggered", x => x.Init<AppointmentSendTriggeredEvent>(
            new AppointmentSendTriggeredEvent
            {
                AppointmentUid = x.Saga.CorrelationId,
                Email = x.Saga.Email
            }));
    }
}