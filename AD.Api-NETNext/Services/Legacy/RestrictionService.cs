using AD.Api.Ldap.Operations;
using AD.Api.Settings;

namespace AD.Api.Services
{
    public interface IRestrictionService
    {
        bool IsAllowed(OperationType operationType, string? objectClass);
    }

    public class RestrictionService : IRestrictionService
    {
        private RestrictionCategories Restrictions { get; }

        public RestrictionService(RestrictionCategories restrictions)
        {
            this.Restrictions = restrictions;
        }

        #region SERVICE METHODS
        public bool IsAllowed(OperationType operationType, string? objectClass)
        {
            if (string.IsNullOrWhiteSpace(objectClass))
            {
                return false;
            }

            return !this.Restrictions.TryGetRestriction(operationType, out Restriction? restriction)
                   || 
                   restriction.ObjectClasses.Contains(objectClass);
        }

        #endregion

        #region BACKEND METHODS


        #endregion
    }
}
