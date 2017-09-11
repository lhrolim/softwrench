namespace softwrench.sw4.user.classes.entities.security
{
    public interface IApplicationPermission
    {
        bool HasNoPermissions { get;  }
        bool AllowUpdate { get; set; }
        bool AllowCreation { get; set; }
    }
}