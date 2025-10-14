namespace BusinessLogic.DTOs
{
    public class AssignUserToProjectDto
    {
        public Guid UserId { get; set; }
        public Guid ProjectId { get; set; }
        public Guid RoleId { get; set; }
    }
}
