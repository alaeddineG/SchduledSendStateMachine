namespace ScheduledSendStateMachine.Contracts;

public class AppointmentCreated
{
    public Guid AppointmentId { get; set; }
    public int SendingDelayInSec { get; set; }
    public string Email { get; set; }
}