using System;

[Obsolete("Sử dụng MessageResponse.UserManagement.Register thay vào")]
public static class MessageRegister
{
    public const string REGISTER_SUCCESS = MessageResponse.UserManagement.Register.SUCCESS;
    public const string REGISTER_FAIL = MessageResponse.UserManagement.Register.FAIL;
    public const string USERNAME_EXIST = MessageResponse.UserManagement.Register.USERNAME_EXIST;
    public const string EMAIL_EXIST = MessageResponse.UserManagement.Register.EMAIL_EXIST;
    public const string INVALID_EMAIL = MessageResponse.UserManagement.Register.INVALID_EMAIL;
    public const string SHORT_PASSWORD = MessageResponse.UserManagement.Register.SHORT_PASSWORD;
    public const string EMPTY_PASSWORD = MessageResponse.UserManagement.Register.EMPTY_PASSWORD;
    public const string UPPERCASE_LETTER_PASSWORD = MessageResponse.UserManagement.Register.UPPERCASE_LETTER_PASSWORD;
    public const string NUMBER_PASSWORD = MessageResponse.UserManagement.Register.NUMBER_PASSWORD;
    public const string LOWERCASE_LETTER_PASSWORD = MessageResponse.UserManagement.Register.LOWERCASE_LETTER_PASSWORD;
    public const string SPECIAL_CHARACTER_PASSWORD = MessageResponse.UserManagement.Register.SPECIAL_CHARACTER_PASSWORD;
}
