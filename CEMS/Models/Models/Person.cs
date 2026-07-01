using System.ComponentModel.DataAnnotations;

namespace CEMS.Models
{
    /// <summary>
    /// Abstract base class for anyone in the system (Participant, Administrator).
    /// Demonstrates abstraction + encapsulation: shared identity fields live here,
    /// role-specific behaviour lives in the subclasses.
    /// </summary>
    public abstract class Person
    {
        public int PersonId { get; set; }

        protected string _name = string.Empty;
        [Required]
        [StringLength(120)]
        public string Name
        {
            get => _name;
            set => _name = !string.IsNullOrWhiteSpace(value)
                ? value
                : throw new ArgumentException("Name cannot be empty.");
        }

        protected string _email = string.Empty;
        [Required]
        [StringLength(200)]
        [EmailAddress]
        public string Email
        {
            get => _email;
            set => _email = IsValidEmail(value)
                ? value
                : throw new ArgumentException("Email is not in a valid format.");
        }

        [Phone]
        [StringLength(30)]
        public string Phone { get; set; } = string.Empty;

        public string GetContactInfo() => $"{Name} <{Email}> ({Phone})";

        /// <summary>
        /// Abstract method — every concrete Person type must say what role it plays.
        /// This is the polymorphism hook: calling GetRole() on a Person reference
        /// resolves to the actual runtime type (Participant/Administrator).
        /// </summary>
        public abstract string GetRole();

        private static bool IsValidEmail(string email) =>
            !string.IsNullOrWhiteSpace(email) && email.Contains('@') && email.Contains('.');
    }
}
