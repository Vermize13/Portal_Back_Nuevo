namespace BusinessLogic.DTOs
{
    public class UpdatePasswordDto
    {
        public string CurrentPassword { get; set; } = default!;
        public string NewPassword { get; set; } = default!;
    }
}
