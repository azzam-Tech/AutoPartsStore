namespace AutoPartsStore.Core.Entities
{
    public class UserRoleAssignment 
    {
        public int Id { get; private set; }
        public int UserId { get; private set; }
        public int RoleId { get; private set; }

        // Navigation
        public User User { get; private set; }
        public UserRole Role { get; private set; }

        public UserRoleAssignment(int userId, int roleId)
        {
            UserId = userId;
            RoleId = roleId;
        }

        // Prevent duplicate assignment
        public static UserRoleAssignment Create(int userId, int roleId)
        {
            if (userId <= 0) throw new ArgumentException("User ID is required");
            if (roleId <= 0) throw new ArgumentException("Role ID is required");
            return new UserRoleAssignment(userId, roleId);
        }
    }
}
