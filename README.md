# AD-RestAPI
A standalone REST API built on .NET 6 for querying and managing Active Directory.  Although, because of certain operations, the API can only be hosted on a Windows platform.

* [__Walkthroughs__](/Walkthroughs)

__* Work in Progress *__

See the [__WIKI__](https://github.com/Yevrag35/AD-RestAPI/wiki) for details about each operation.

## [Authentication](/Walkthroughs/Authentication)

By default, the project will use __Windows Authentication__, and it is highly recommended to [set up the application to use Kerberos/Negotiate](/Walkthroughs/Authentication/Negotiate-NTLM-Kerberos) to be as secure as possible.
I'm planning on baking in some options as well as creating walkthroughs for leveraging other authentication schemes (e.g. - AzureAD, custom JSON web tokens, etc.).

## Update 4/24/22

The following endpoints have been created:

| Endpoint       | Rest Method(s)   | For AD ObjectClass
| ---------------- | ---------------- | ----------------- |
| [/create/group](https://github.com/Yevrag35/AD-RestAPI/wiki/Group-Create)    | POST             | group
| [/create/ou](https://github.com/Yevrag35/AD-RestAPI/wiki/OU-Create)       | POST             | organizationalUnit
| [/create/user](https://github.com/Yevrag35/AD-RestAPI/wiki/User-Create)     | POST             | user
| [/delete](https://github.com/Yevrag35/AD-RestAPI/wiki/Endpoints#delete)          | DELETE           | \*ANY\*
| [/edit](https://github.com/Yevrag35/AD-RestAPI/wiki/Endpoints#edit)            | PUT              | \*ANY\*
| [/move](https://github.com/Yevrag35/AD-RestAPI/wiki/Endpoints#move)            | POST              | \*ANY\*
| [/password/change](https://github.com/Yevrag35/AD-RestAPI/wiki/Password-Change) | PUT              | user
| [/password/reset](https://github.com/Yevrag35/AD-RestAPI/wiki/Password-Reset)  | PUT              | user
| [/rename](https://github.com/Yevrag35/AD-RestAPI/wiki/Endpoints#rename)          | PUT              | \*ANY\*
| [/search](https://github.com/Yevrag35/AD-RestAPI/wiki/Generic-Search)          | GET, POST        | \*ANY\*
| [/search/computer](https://github.com/Yevrag35/AD-RestAPI/wiki/Computer-Query) | GET, POST        | computer
| [/search/group](https://github.com/Yevrag35/AD-RestAPI/wiki/Group-Query)    | GET, POST        | group
| [/search/user](https://github.com/Yevrag35/AD-RestAPI/wiki/User-Query)     | GET, POST        | user