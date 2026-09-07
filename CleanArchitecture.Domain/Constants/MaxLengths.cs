namespace CleanArchitecture.Domain.Constants;

public static class MaxLengths
{
    public static class User
    {
        public const int Email = 320;
        public const int FirstName = 100;
        public const int LastName = 100;
        public const int Password = 128;
    }

    public static class Tenant
    {
        public const int Name = 255;
    }

    public static class Client
    {
        public const int Nombres = 100;
        public const int Apellidos = 100;
        public const int DocumentoIdentidad = 50;
        public const int Email = 320;
        public const int Telefono = 30;
        public const int Estado = 30;
    }
}