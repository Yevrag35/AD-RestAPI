# AD-RestAPI
A standalone REST API built on .NET 6 for querying and managing Active Directory.

* [__Walkthroughs__](/Walkthroughs)
* [__WIKI__](https://github.com/Yevrag35/AD-RestAPI/wiki)

__* Work in Progress *__

Tore down and started to rebuild the API.
Right now, the API can receive simple queries with the LDAP filter in the JSON body.

I took extra time to make sure that the LDAP filters and editing operations can be built on the fly and read to and from JSON natively.

## [Authentication](/Walkthroughs/Authentication)

By default, the project will use __Windows Authentication__, but I'm baking in some options as well as creating walkthroughs for leveraging other authentication schemes (e.g. - AzureAD, custom JSON web tokens, etc.).

## Update 4/13/22

The query and edit controllers are done.  Now working on Create.

* EXAMPLE -

_See more examples [in the wiki](https://github.com/Yevrag35/AD-RestAPI/wiki/LDAP-Filters---JSON-Body-syntax)_

Sample JSON body
```JSON
{
  "and": {
    "objectClass": "user",
    "or": [
      { "name": "John Doe" },
      { "name": "Jane Doe" },
      {
        "and": {
          "userPrincipalName": null,
          "mail": "john@doe.com"
        }
      }
    ]
  }
}
```

Sample JSON query using PowerShell
```PowerShell
$body = @{
  and = @{
    objectClass = "user"
    or = @(
      @{ name = "John Doe" },
      @{ name = "Jane Doe" },
      @{ 
        and = @{
          userPrincipalName = $null
          mail = "john@doe.com"
        }
      }
    )
  }
}

Invoke-RestMethod -Uri 'https://localhost/search' -Body $($body | ConvertTo-Json -Depth 4)
```

The resulting LDAP filter is serialized as:

<code>(&(objectClass=user)(|(name=John Doe)(name=Jane Doe)(&(!(userPrincipalName=*))(mail=john@doe.com))))</code>
