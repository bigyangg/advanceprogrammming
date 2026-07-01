namespace CEMS.Models.Interfaces
{
    /// <summary>
    /// Anything that can send out notifications (e.g. Event announcing changes)
    /// implements this. Separated out so notification logic isn't tangled into
    /// the Event class itself.
    /// </summary>
    public interface INotifiable
    {
        void SendNotification(string message);
    }
}
