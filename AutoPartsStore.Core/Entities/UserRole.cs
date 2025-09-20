namespace AutoPartsStore.Core.Entities
{
    public class UserRole 
    {
        public int Id { get; private set; }
        public string RoleName { get; private set; }
        public string? Description { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }

        // Relationship
        public List<UserRoleAssignment> Assignments { get; private set; } = new();

        public UserRole(string roleName, string? description = null)
        {
            RoleName = roleName;
            Description = description;
            CreatedAt = DateTime.UtcNow;
        }
    }
}
