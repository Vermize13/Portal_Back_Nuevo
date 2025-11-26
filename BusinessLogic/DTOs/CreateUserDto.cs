namespace BusinessLogic.DTOs
{
    public class CreateUserDto
    {
        public string Name { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string Username { get; set; } = default!;
        public string Password { get; set; } = default!;
        public Guid? RoleId { get; set; }
    }
}
