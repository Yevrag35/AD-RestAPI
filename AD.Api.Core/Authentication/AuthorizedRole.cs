namespace AD.Api.Core.Authentication
{
    /// <summary>
    /// Enumeration values for denoting authorized roles for the application.
    /// </summary>
    [Flags]
    public enum AuthorizedRole
    {
        /// <summary>
        /// Is authenticated but has no defined role.
        /// </summary>
        None = 0x0,
        /// <summary>
        /// Can view and read directory data.
        /// </summary>
        Reader = 1,

        /// <summary>
        /// Can create new users.
        /// </summary>
        UserCreator = 2,
        /// <summary>
        /// Can modify attributes on existing users.
        /// </summary>
        UserEditor = 4,
        /// <summary>
        /// Can delete users from the directory.
        /// </summary>
        UserDeleter = 8,

        /// <summary>
        /// Can create new groups.
        /// </summary>
        GroupCreator = 0x10,
        /// <summary>
        /// Can modify attributes on existing groups.
        /// </summary>
        GroupEditor = 0x20,
        /// <summary>
        /// Can delete groups from the directory.
        /// </summary>
        GroupDeleter = 0x40,

        /// <summary>
        /// Can create new computer objects.
        /// </summary>
        ComputerCreator = 0x80,
        /// <summary>
        /// Can modify attributes on existing computer objects.
        /// </summary>
        ComputerEditor = 0x100,
        /// <summary>
        /// Can delete computer objects from the directory.
        /// </summary>
        ComputerDeleter = 0x200,

        /// <summary>
        /// Can change the password of a user objects. This requires the
        /// requester to provide the current password of the user.
        /// </summary>
        PasswordChanger = 0x400,
        /// <summary>
        /// Can reset the password of a user object. This does *not* require
        /// the requester to provide the current password of the user.
        /// </summary>
        PasswordResetter = 0x800,

        GroupAdmin = Reader | GroupCreator | GroupEditor | GroupDeleter,
        ComputerAdmin = Reader | ComputerCreator | ComputerEditor | ComputerDeleter,
        UserAdmin = Reader | UserCreator | UserEditor | UserDeleter | PasswordChanger | PasswordResetter,

        /// <summary>
        /// Can perform all action in the directory. Equivalent to
        /// Reader | UserAdmin | GroupAdmin | ComputerAdmin - (i.e. - Domain Admin)
        /// </summary>
        SuperAdmin = 134217725 | UserAdmin | GroupAdmin | ComputerAdmin
    }
}
