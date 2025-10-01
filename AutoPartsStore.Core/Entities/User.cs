namespace AutoPartsStore.Core.Entities
{
    public class User
    {
        public int Id { get; private set; }
        public string Email { get; private set; }
        public string FullName { get; private set; }
        public string? PhoneNumber { get; private set; }
        public DateTime RegistrationDate { get; private set; }
        public DateTime? LastLoginDate { get; private set; }
        public bool IsActive { get; private set; }
        public bool IsDeleted { get; private set; }
        public DateTime? DeletedAt { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }
        public bool IsEmailVerified { get; set; } = false;
        public string? VerificationCode { get; set; }
        public DateTime? VerificationCodeExpiry { get; set; }

        // Relationships
        public List<UserRoleAssignment> RoleAssignments { get; private set; } = new();
        public List<Address> Addresses { get; private set; } = new();
        public ShoppingCart? ShoppingCart { get; private set; }
        public List<ProductReview> Reviews { get; private set; } = new();
        public List<Favorite> Favorites { get; private set; } = new();


        // Constructor
        public User(string email, string fullName, string phoneNumber)
        {
            FullName = fullName;
            Email = email;
            PhoneNumber = phoneNumber;
            RegistrationDate = DateTime.UtcNow;
            IsActive = false;
            IsDeleted = false;
            DeletedAt = null;
            CreatedAt = DateTime.UtcNow;
        }

        // Methods
        public void UpdateUsre(string email, string fullName, string phoneNumber)
        {
            FullName = fullName;
            Email = email;
            PhoneNumber = phoneNumber;
        }
        public void UpdateLastLogin() => LastLoginDate = DateTime.UtcNow;
        public void Deactivate() => IsActive = false;
        public void Activate() => IsActive = true;

        public void SoftDelete()
        {
            IsDeleted = true;
            DeletedAt = DateTime.UtcNow;
        }

        public void Restore()
        {
            IsDeleted = false;
            DeletedAt = null;
        }



    }
}
