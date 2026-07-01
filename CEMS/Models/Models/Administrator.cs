namespace CEMS.Models
{
    /// <summary>
    /// Manages events, venues, activities and participants.
    /// Overload example on CreateEvent shows method overloading (OOP requirement).
    /// </summary>
    public class Administrator : Person
    {
        public List<string> Permissions { get; set; } = new();

        public Event CreateEvent(string name, DateTime date, string description, int capacity)
        {
            return new Event
            {
                Name = name,
                EventDate = date,
                Description = description,
                Capacity = capacity
            };
        }

        // Overload: create an event directly from an existing template/event.
        public Event CreateEvent(Event template)
        {
            if (template is null) throw new ArgumentNullException(nameof(template));
            return CreateEvent(template.Name, template.EventDate, template.Description, template.Capacity);
        }

        public void ManageVenue(Venue v)
        {
            if (v is null) throw new ArgumentNullException(nameof(v));
            // Actual persistence handled by the service/repository layer —
            // this method represents the admin action/business rule entry point.
        }

        public override string GetRole() => "Administrator";
    }
}
