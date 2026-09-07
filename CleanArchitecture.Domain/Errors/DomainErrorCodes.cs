namespace CleanArchitecture.Domain.Errors;

public static class DomainErrorCodes
{
    public static class User
    {
        // User Validation
        public const string EmptyId = "USER_EMPTY_ID";
        public const string EmptyFirstName = "USER_EMPTY_FIRST_NAME";
        public const string EmptyLastName = "USER_EMPTY_LAST_NAME";
        public const string EmailExceedsMaxLength = "USER_EMAIL_EXCEEDS_MAX_LENGTH";
        public const string FirstNameExceedsMaxLength = "USER_FIRST_NAME_EXCEEDS_MAX_LENGTH";
        public const string LastNameExceedsMaxLength = "USER_LAST_NAME_EXCEEDS_MAX_LENGTH";
        public const string InvalidEmail = "USER_INVALID_EMAIL";
        public const string InvalidRole = "USER_INVALID_ROLE";

        // User Password Validation
        public const string EmptyPassword = "USER_PASSWORD_MAY_NOT_BE_EMPTY";
        public const string ShortPassword = "USER_PASSWORD_MAY_NOT_BE_SHORTER_THAN_6_CHARACTERS";
        public const string LongPassword = "USER_PASSWORD_MAY_NOT_BE_LONGER_THAN_50_CHARACTERS";
        public const string UppercaseLetterPassword = "USER_PASSWORD_MUST_CONTAIN_A_UPPERCASE_LETTER";
        public const string LowercaseLetterPassword = "USER_PASSWORD_MUST_CONTAIN_A_LOWERCASE_LETTER";
        public const string NumberPassword = "USER_PASSWORD_MUST_CONTAIN_A_NUMBER";
        public const string SpecialCharPassword = "USER_PASSWORD_MUST_CONTAIN_A_SPECIAL_CHARACTER";

        // General
        public const string AlreadyExists = "USER_ALREADY_EXISTS";
        public const string PasswordIncorrect = "USER_PASSWORD_INCORRECT";
    }

    public static class Tenant
    {
        // Tenant Validation
        public const string EmptyId = "TENANT_EMPTY_ID";
        public const string EmptyName = "TENANT_EMPTY_NAME";
        public const string NameExceedsMaxLength = "TENANT_NAME_EXCEEDS_MAX_LENGTH";

        // General
        public const string AlreadyExists = "TENANT_ALREADY_EXISTS";
    }

    public static class Client
    {
        public const string EmptyId = "CLIENT_EMPTY_ID";
        public const string EmptyNombres = "CLIENT_EMPTY_NOMBRES";
        public const string EmptyApellidos = "CLIENT_EMPTY_APELLIDOS";
        public const string EmptyDocumentoIdentidad = "CLIENT_EMPTY_DOCUMENTO_IDENTIDAD";
        public const string EmptyEmail = "CLIENT_EMPTY_EMAIL";
        public const string EmptyEstado = "CLIENT_EMPTY_ESTADO";
        public const string InvalidEmail = "CLIENT_INVALID_EMAIL";
        public const string NombresExceedsMaxLength = "CLIENT_NOMBRES_EXCEEDS_MAX_LENGTH";
        public const string ApellidosExceedsMaxLength = "CLIENT_APELLIDOS_EXCEEDS_MAX_LENGTH";
        public const string DocumentoIdentidadExceedsMaxLength = "CLIENT_DOCUMENTO_IDENTIDAD_EXCEEDS_MAX_LENGTH";
        public const string EmailExceedsMaxLength = "CLIENT_EMAIL_EXCEEDS_MAX_LENGTH";
        public const string TelefonoExceedsMaxLength = "CLIENT_TELEFONO_EXCEEDS_MAX_LENGTH";
        public const string EstadoExceedsMaxLength = "CLIENT_ESTADO_EXCEEDS_MAX_LENGTH";
        public const string AlreadyExists = "CLIENT_ALREADY_EXISTS";
    }
}