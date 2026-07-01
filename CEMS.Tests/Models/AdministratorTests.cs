using CEMS.Models;

namespace CEMS.Tests.Models;

public class AdministratorTests
{
    [Fact]
    public void CreateEvent_WithParameters_CreatesExpectedEvent()
    {
        var admin = new Administrator { Name = "Admin", Email = "admin@uni.edu", Phone = "111" };
        var date = new DateTime(2026, 12, 1, 10, 0, 0, DateTimeKind.Utc);

        var evt = admin.CreateEvent("Community Meet", date, "Description", 75);

        Assert.Equal("Community Meet", evt.Name);
        Assert.Equal(date, evt.EventDate);
        Assert.Equal("Description", evt.Description);
        Assert.Equal(75, evt.Capacity);
    }

    [Fact]
    public void CreateEvent_WithTemplate_UsesOverloadedMethod()
    {
        var admin = new Administrator { Name = "Admin", Email = "admin@uni.edu", Phone = "111" };
        var template = new Event
        {
            Name = "Template Event",
            EventDate = new DateTime(2026, 11, 5, 9, 0, 0, DateTimeKind.Utc),
            Description = "Template Desc",
            Capacity = 40
        };

        var evt = admin.CreateEvent(template);

        Assert.NotSame(template, evt);
        Assert.Equal(template.Name, evt.Name);
        Assert.Equal(template.EventDate, evt.EventDate);
        Assert.Equal(template.Description, evt.Description);
        Assert.Equal(template.Capacity, evt.Capacity);
    }
}
