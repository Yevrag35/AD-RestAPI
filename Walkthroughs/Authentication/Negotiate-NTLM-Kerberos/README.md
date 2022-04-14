# Using AD-RestAPI with Negotiate Authentication

__Table of Contents__

* [Overview](#windows-auth-overview)
    * [Negotiate](#negotiate)
    * [NTLM](#ntlm)
* [Setting up and Enforcing Kerberos Authentication](#setting-up-and-enforcing-kerberos-authentication)
    * [Creating a New Service Account](#creating-a-new-service-account)
        * [Creating a Managed Service Account](#creating-a-managed-service-account)
    * [Configuring the Application Pool](#configuring-the-application-pool)
    * [Setting the Service Princpal Name(s)](#setting-the-service-princpal-names)
        * [ADUC Method](#aduc-method)
        * [PowerShell Method](#powershell-method)
    * [Constrained Kerberos Delegation](#recommended---constrained-kerberos-delegation)
    * [Final Step](#final-step)

---

## Windows Auth Overview

Windows Authentication is the primary functionality normally used in conjunction with IIS applications.

![Negotiate Protocol](https://docs.microsoft.com/en-us/openspecs/windows_protocols/ms-authsod/ms-authsod_files/image025.png)

While IIS supports different authentication schemes, making sure the client and server agree upon a specific one is where Negotiate comes in.

In an IIS site, by default when using __Windows Authentication__, the Providers are set to: "Negotiate" first, followed by NTLM.

![Windows Auth Default Providers](https://images.yevrag35.com/walkthrus/auth/iis_windows_auth_providers.png)

## Negotiate

Negotiate authentication automatically selects between the Kerberos protocol and NTLM authentication, depending on availability. The Kerberos protocol is used if it is available; otherwise, NTLM is tried. Kerberos authentication significantly improves upon NTLM. Kerberos authentication is both faster than NTLM and allows the use of mutual authentication and delegation of credentials to remote machines.

## NTLM

NT LAN Manager (NTLM) authentication is a challenge-response scheme that is a securer variation of Digest authentication. NTLM uses Windows credentials to transform the challenge data instead of the unencoded user name and password. NTLM authentication requires multiple exchanges between the client and server. The server and any intervening proxies must support persistent connections to successfully complete the authentication.

* _More Information - [Understanding HTTP Authentication](https://docs.microsoft.com/en-us/dotnet/framework/wcf/feature-details/understanding-http-authentication)_

# Setting up and Enforcing Kerberos Authentication

Setting the API to leverage Kerberos is the most secure option for Windows Authentication.  Passing through a Kerberos ticket for authentication allows the service to naturally respect and enforces the permissions the requester has in the AD environment.  No secondary roles/groups are required to restrict access to the API's functionality.  The only special permission granted to the API service is the allowance to delegate Kerberos requests to a Domain Controller on behalf of the requester.  Other than that, the service runs as a standard, non-privileged identity in the environment.

## Creating a New Service Account

Because the API will need access to Active Directory, the IIS site will have to have to run as a user that has at least "authenticate" access into the domains that the API will query.  As a security best-practice, a __new, dedicated__ account should be created that will run the IIS Application Pool.

This account can either be a traditional user account (i.e. - with a static password), or you can leverage the much more secure "Managed Service Account" option (can also be Group Managed Service Accounts too).

### Creating a Managed Service Account

If the Active Directory Forest you want the Service Account to be in has never created one before, you'll need to run the setup procedures first.  These can be found here: [Creating the KDS Root Key](https://docs.microsoft.com/en-us/windows-server/security/group-managed-service-accounts/create-the-key-distribution-services-kds-root-key)

Here's an example set of commands that creates a Managed Service Account, and allows our IIS server to retrieve the password from Active Directory.

```powershell
Import-Module ActiveDirectory

# Parameters
$svcAcctParams = @{
    Name                                       = "ApiTester"
    DisplayName                                = "ApiTester"
    SamAccountName                             = "ApiTester$"   
                                               # ^ I think the SamAccountName would get created automatically with it but notice the '$' at the end
    DNSHostName                                = "localhost"        # You can make this whatever you want, really.
    KerberosEncryptionType                     = "AES128", "AES256" # The most secure options
    ManagedPasswordIntervalInDays              = 7                  # The default is 30 days - FYI: Once this is set, you can't change it
    PrincipalsAllowedToRetrieveManagedPassword = $(
        Get-ADComputer "<Name of the IIS server running AD-RestAPI>"
    )
}

New-ADServiceAccount @svcAcctParams
```

Even though the Application Pool runs as this account, the authentication scheme will force the API caller to provide either:

1. NTLM credentials
    * This option includes using explicit credentials (e.g. - Username/Password combo) as the API cannot directly challenge the requester with an Windows Authentication prompt.
1. A Kerberos Ticket
    * The most secure and best-performing option, but takes a bit of set up to get working.

---

## Configuring the Application Pool

With the account created, now it's possible to have the IIS Application Pool that's configured for the API to run as that account.  Open up the IIS Manager console, head to the __Application Pools__ node on the left-side navigation, and select the AppPool that is associated with the API site.  Right-click the AppPool, and select __"Advance Settings..."__.

* _\*NOTE\* - If the server hosts other sites, you'll want to consider making a new Application Pool for the API site specifically._

![IIS AppPool Custom Account](https://images.yevrag35.com/walkthrus/auth/iis_app_pool_custom_acct.png)

Find the __Identity__ field under _"Process Model"_, highlight the field, and click the little box with the three ellipses.  In the "Application Pool Identity" box that pops up, switch to the __Custom account:__ radio button, and click _"Set..."_.  Enter the username and password of the service account you created previously.

* _I typically use the traditional NTAccount format of "\<Domain\>\\<SamAccountName\>"_

If you created the service account as a Managed Service Account, you'll notice, after typing the '$', the password field greys out.  This is because you don't need it; as long as the computer that's running IIS is allowed to retrieve the password from AD, that's all that's needed.

---

## Setting the Service Princpal Name(s)

In order for the API to use Kerberos authentication, the service account must have the SPN of the site using the HTTP protocol registered in Active Directory.  The SPN's to be added are determined by the URL you intend to make the API accessible by.  You can add multiple URL's as long as their unique in the AD environment.

A SPN's format is: <code>\<PROTOCOL\>/\<Host\>[:\<PortNumber\>]</code> with the "PortNumber" being optional.  The _"Protocol"_ value for us will always be __HTTP__ regardless whether or not we're using HTTP over SSL.  The _"Host"_ value must be the EXACT hostname url (i.e. - minus the 'http/https' prefix) that the API will be accessed through.

In this example, my site's URL is accessible through IIS bindings using 'localhost' as the hostname and on port 41812.  So my SPN for the service account will need to be:

<code>HTTP/localhost:41812</code>

Now, let's register it to the service account.  There's multiple ways to do this, but this guide will demonstrate the PowerShell and ADUC (Active Directory Users and Computers MMC snap-in) methods.

### __ADUC Method__

__FIRST!!!__ (because I always forget lol), make sure you have __"Advanced Features"__ enabled.

![ADUC Advanced Features](https://images.yevrag35.com/walkthrus/auth/aduc_advanced_features.png)


Navigate in the snap-in to the service account created that's now running the IIS application pool, open the user's property window, and go to the __"Attribute Editor"__ tab.

* _If you don't see this tab, you don't have 'Advanced Features' enabled... >\_>_

Scroll down to the __servicePrincipalName__ attribute and double-click it.  In the Multi-valued String Editor window that pops up, enter the desired SPN's to register.

![Adding SPNs to Service Account](https://images.yevrag35.com/walkthrus/auth/delegate_kerberos_spn.png)

### __PowerShell Method__

```powershell
Import-Module ActiveDirectory

$samAccountName = 'ApiTester$'  # The SAM of the service account.
$filter = "SamAccountName -eq '$samAccountName'"

$spnsToAdd = @(
    'HTTP/localhost:41812'  # Multiple SPNs can be added to this array.
)

Get-ADObject -Filter $filter | Set-ADObject -Add @{ 
    servicePrincipalName = $spnsToAdd
}
```

## RECOMMENDED - Constrained Kerberos Delegation

The best-practice around Kerberos delegation is to make sure the account doing the 'delegating' is only able to do so for very specific Windows services on very specific domain computers.  Because the API will only be running on a single IIS server this is a very easy set up for us to do.

If the service account created was a Managed Service Account, you may have noticed that the ADUC snap-in lacks a lot of tabs.  And more importantly, for this example, it lacks the __"Delegation"__ tab that would normally otherwise show up if you used a standard user account.  But, as always, PowerShell comes to the rescue.

* _\*NOTE\*: these examples all work for both MSA's and standard user accounts._

```powershell
Import-Module ActiveDirectory

# Parameters
$samAccountName = 'ApiTester$'  # The SAM of the service account.

$iisServerName = "ADRestApiServer.yevrag35.com"  # The FQDN of the IIS server running the API.

# We'll construct the rest of the variables automatically
$iisShortName = $($iisServerName -split '\.' | Select-Object -First 1)
$principalsAllowed = @(
    "w3svc/$iisShortName"   # w3svc is the service name of IIS
    "w3svc/$iisServerName"
)
$filter = "SamAccountName -eq '$samAccountName'"

# And apply
Get-ADObject -Filter $filter | Set-ADObject -Add @{
    "msDS-AllowedToDelegateTo" = $principalsAllowed
}
```

## Final Step

The last step is to change the [__"launchSettings.json"__](../../../AD.Api-NET6/Properties/launchSettings.json) for the API project to use 'windowsAuthentication' and not anonymous under the _"iisSettings"_ object.