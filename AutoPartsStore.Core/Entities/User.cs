using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AutoPartsStore.Core.Entities
{
    public class User
    {
        public int Id { get; private set; }
        public string Username { get; private set; }
        public string PasswordHash { get; private set; }
        public string Email { get; private set; }
        public string FullName { get; private set; }
        public string PhoneNumber { get; private set; }
        public DateTime RegistrationDate { get; private set; }
        public DateTime? LastLoginDate { get; private set; }
        public DateTime? LastLocationUpdate { get; private set; }
        public bool IsActive { get; private set; }
        public bool IsDeleted { get; private set; }
        public DateTime? DeletedAt { get; private set; }

        // Relationships
        public List<UserRoleAssignment> RoleAssignments { get; private set; } = new();
        public List<Address> Addresses { get; private set; } = new();
        public ShoppingCart? ShoppingCart { get; private set; }
        public List<ProductReview> Reviews { get; private set; } = new();

        // Constructor
        public User(string username, string passwordHash, string email, string fullName, string phoneNumber)
        {
            Username = username;
            PasswordHash = passwordHash;
            Email = email;
            FullName = fullName;
            PhoneNumber = phoneNumber;
            RegistrationDate = DateTime.UtcNow;
            IsActive = true;
            IsDeleted = false;
            DeletedAt = null;
        }

        // Methods
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
