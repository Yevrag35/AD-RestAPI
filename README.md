# AD-RestAPI
A standalone REST API built on .NET 6 for querying and managing Active Directory.

* [__Walkthroughs__](/Walkthroughs)
* [__WIKI__](https://github.com/Yevrag35/AD-RestAPI/wiki)

__* Work in Progress *__

## [Authentication](/Walkthroughs/Authentication)

By default, the project will use __Windows Authentication__, but I'm baking in some options as well as creating walkthroughs for leveraging other authentication schemes (e.g. - AzureAD, custom JSON web tokens, etc.).

## Update 4/21/22

The following endpoints have been created:

| Endpoint       | Rest Method(s)   | For AD ObjectClass
| ---------------- | ---------------- | ----------------- |
| /create/group    | POST             | group
| /create/ou       | POST             | organizationalUnit
| /create/user     | POST             | user
| /delete          | DELETE           | \*ANY\*
| /edit            | PUT              | \*ANY\*
| /move            | PUT              | \*ANY\*
| /rename          | PUT              | \*ANY\*
| /search          | GET, POST        | \*ANY\*
| /search/computer | GET, POST        | computer
| /search/group    | GET, POST        | group
| /search/user     | GET, POST        | user