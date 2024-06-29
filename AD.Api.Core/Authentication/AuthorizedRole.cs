namespace AD.Api.Core.Authentication
{
    [Flags]
    public enum AuthorizedRole
    {
        None = 0x0,
        Reader = 1,

        UserCreator = 2,
        UserEditor = 4,
        UserDeleter = 8,

        GroupCreator = 16,
        GroupEditor = 32,
        GroupDeleter = 64,

        ComputerCreator = 128,
        ComputerEditor = 256,
        ComputerDeleter = 512,

        PasswordChanger = 1024,
        PasswordResetter = 2048,

        UserAdmin = Reader | UserCreator | UserEditor | UserDeleter | PasswordChanger,
        GroupAdmin = Reader | GroupCreator | GroupEditor | GroupDeleter,
        ComputerAdmin = Reader | ComputerCreator | ComputerEditor | ComputerDeleter,

        SuperAdmin = 0x8000000 | UserAdmin | GroupAdmin | ComputerAdmin | PasswordResetter,
    }
}

