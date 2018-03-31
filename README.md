![Icon](https://raw.githubusercontent.com/asherber/MailmanSharp/master/MailmanSharp-64.png)

# MailmanSharp [![NuGet](https://img.shields.io/nuget/v/MailmanSharp.svg)](https://nuget.org/packages/MailmanSharp) ![TestedWithMailman](https://img.shields.io/badge/tested%20with%20mailman-2.1.23-brightgreen.svg) ![ShouldWorkWith](https://img.shields.io/badge/should%20work%20with%20mailman-2.1.26-yellow.svg)

A C# library for configuration and manipulation of Mailman mailing lists by controlling the admin web interface.

I wrote this library to solve the problem of trying to configure a new Mailman list to be just like another list I have. I got tired of going back and forth between list admin pages  comparing settings; I wanted a way to save the configuration from one list and use it as a template to configure other lists. 

Since Mailman 2 does not expose an official REST API (except for certain operations on the membership list), this assembly relies on scraping the HTML from the admin pages. It has been tested with Mailman 2.1.23 and includes all pages and settings except for the Language Options page.

For a list of changes in v3 of this library, please see https://github.com/asherber/MailmanSharp/wiki/Changes-in-v3



## Features
* All Mailman settings are available as object properties
* Bulk subscribe/unsubscribe
* Change settings for individual members, or groups of members
* Save or load list configuration using JSON

## Sample Code
```csharp
// Create list object
var myList = new MailmanList("http://foo.org/mailman/admin/mylist", "password");

// Work with list settings
await myList.ReadAsync();  // read all current settings
myList.General.MaxMessageSize = 500;
myList.Privacy.SubscribePolicy = SubscribePolicyOption.Confirm;
await myList.WriteAsync();

// Add members
var newMembers = new List<string>() { "bob@example.com", "jane@example.com" };
await myList.Membership.SubscribeAsync(newMembers);
List<string> currentMembers = myList.Membership.EmailList;  

// Change a member's settings
Member bob = await myList.Membership.SearchMembersAsync("bob@example.com").Single();
bob.NoMail = true;
await myList.Membership.SaveMemberAsync(bob);

// Work with configuration as JSON
string listConfig = myList.CurrentConfig;
// Maybe change some things in the JSON, or save to file, then ...
myOtherList.LoadConfig(listConfig);
await myOtherList.WriteAsync();
```

## NuGet
Available at https://www.nuget.org/packages/MailmanSharp/
