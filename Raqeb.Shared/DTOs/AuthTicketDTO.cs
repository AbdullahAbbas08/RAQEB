namespace Raqeb.Shared.DTOs
{
    public class AuthTicketDTO
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string FullNameAr { get; set; }
        public string FullNameEn { get; set; }

        public int UserId { get; set; }
        public int? ProfileImageFileId { get; set; }

        public string UserImage { get; set; }
        public int? RoleId { get; set; }
        public int UserRoleId { get; set; }
        public string RoleNameAr { get; set; }
        public string RoleNameEn { get; set; }
        public string RoleNameFn { get; set; }

        public string DefaultCulture { get; set; }
        public string DefaultCalendar { get; set; }
        public virtual IEnumerable<string> Permissions { get; set; }
        //public virtual IEnumerable<UserRoleDTO> UserRoles { get; set; } 
        public int RolesCount { get; set; }
        //public string UserIdEncrypt { get { return Encrypt(UserId.ToString()); } }

    }

    public class EncriptedAuthTicketDTO
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string FullNameAr { get; set; }
        public string FullNameEn { get; set; }

        public int UserId { get; set; }
        public int? ProfileImageFileId { get; set; }

        public string UserImage { get; set; }
        public int? RoleId { get; set; }
        public int UserRoleId { get; set; }
        public string RoleNameAr { get; set; }
        public string RoleNameEn { get; set; }
        public string RoleNameFn { get; set; }

        public string DefaultCulture { get; set; }
        public string DefaultCalendar { get; set; }
        public virtual IEnumerable<string> Permissions { get; set; }
        //public virtual IEnumerable<UserRoleDTO> UserRoles { get; set; } 
        public int RolesCount { get; set; }
        //public string UserIdEncrypt { get { return Encrypt(UserId.ToString()); } }
    }

}
