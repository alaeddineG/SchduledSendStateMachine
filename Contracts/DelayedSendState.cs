using MassTransit;

namespace ScheduledSendStateMachine.Contracts;

public class DelayedSendState:  SagaStateMachineInstance, ISagaVersion
{
    public Guid CorrelationId { get; set; }
    public Guid? DeliverDelayedEmailMessageTimeoutTokenId { get; set; }
    public int Version { get; set; }
    public string CurrentState { get; set; }
    public int SendingDelayInSec { get; set; }
    public string Email { get; set; }
}