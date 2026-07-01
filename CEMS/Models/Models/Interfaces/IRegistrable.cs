namespace CEMS.Models.Interfaces
{
    /// <summary>
    /// Anything that can register/cancel for an event implements this.
    /// Keeps the registration contract decoupled from the Participant implementation.
    /// </summary>
    public interface IRegistrable
    {
        void Register(Event e);
        void CancelRegistration(Event e);
    }
}
