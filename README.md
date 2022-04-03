# AD-RestAPI
A standalone REST API built on .NET 6 for querying and managing Active Directory.

__* Work in Progress *__

Tore down and started to rebuild the API.
Right now, the API can receive simple queries with the LDAP filter in the JSON body.

I took extra time to make sure that the LDAP filters can be built on the fly and read to and from JSON natively.

Next step is to add bitwise operators into the mix.

* EXAMPLE

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
