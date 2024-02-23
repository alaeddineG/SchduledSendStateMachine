namespace ScheduledSendStateMachine.Contracts;

public class SendDelayedEmailMessage
{
    public Guid AppointmentUid { get; set; }
    public string Email { get; set; }
}