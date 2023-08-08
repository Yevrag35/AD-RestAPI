using System.Globalization;

namespace AD.Api.Exceptions
{
    public class NoNamingContextException : Exception
    {
        public string Domain { get; }

        public NoNamingContextException(string domain)
            : base(string.Format(CultureInfo.CurrentCulture, Messages.SearchDomain_Err_NoNamingContext, domain))
        {
            this.Domain = domain;
        }
    }
}
