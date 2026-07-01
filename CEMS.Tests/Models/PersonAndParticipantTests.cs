using CEMS.Models;

namespace CEMS.Tests.Models;

public class PersonAndParticipantTests
{
    [Theory]
    [InlineData("student@uni.edu")]
    [InlineData("a.b@college.org")]
    public void Participant_EmailValidation_AllowsValidEmails(string email)
    {
        var participant = new Participant { Name = "Test User", Email = email, Phone = "12345" };
        Assert.Equal(email, participant.Email);
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid-email")]
    [InlineData("missing-at.com")]
    public void Participant_EmailValidation_RejectsInvalidEmails(string email)
    {
        Assert.Throws<ArgumentException>(() => new Participant
        {
            Name = "Test User",
            Email = email,
            Phone = "12345"
        });
    }

    [Fact]
    public void Participant_RegisterAndCancel_UpdatesRegistrationState()
    {
        var participant = new Participant { PersonId = 1, Name = "User", Email = "user@uni.edu", Phone = "12345" };
        var evt = new Event { EventId = 10, Name = "Event", EventDate = DateTime.UtcNow.AddDays(1), Description = "d", Capacity = 10 };

        participant.Register(evt);
        var reg = participant.ViewRegistrations().Single();
        Assert.Equal(RegistrationStatus.Pending, reg.Status);

        participant.CancelRegistration(evt);
        Assert.Equal(RegistrationStatus.Cancelled, reg.Status);
    }

    [Fact]
    public void Participant_RegisterTwiceForSameEvent_Throws()
    {
        var participant = new Participant { PersonId = 1, Name = "User", Email = "user@uni.edu", Phone = "12345" };
        var evt = new Event { EventId = 10, Name = "Event", EventDate = DateTime.UtcNow.AddDays(1), Description = "d", Capacity = 10 };

        participant.Register(evt);
        Assert.Throws<InvalidOperationException>(() => participant.Register(evt));
    }
}
