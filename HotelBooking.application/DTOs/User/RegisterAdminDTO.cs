public class RegisterAdminDTO
{
    public string Username { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string Password { get; set; } = null!;

    private int RoleId = RoleTypeConstDTO.Admin;
    public int GetRoleId()
    {
        return RoleId;
    }
}