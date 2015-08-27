# MailmanSharp
A C# library for configuration and manipulation of Mailman mailing lists by controlling the admin web interface.

I wrote this library to solve the problem of trying to configure a new Mailman list to be just like another list I have. I got tired of going back and forth between list admin pages comparing settings; I wanted a way to save the configuration from one list and use it as a template to configure other lists.

Since Mailman 2 does not expose an official REST API (except for certain operations on the membership list), this assembly relies on scraping the HTML from the admin pages. It has been tested with Mailman 2.1.17 and includes all pages and settings except for the Language Options page.

## Features
* All Mailman settings are available as object properties
* Bulk subscribe/unsubscribe
* Change settings for individual members, or groups of members
* Save or load list configuration from XML

## Sample Code
```csharp
// Create list object
var myList = new MailmanList("http://foo.org/mailman/admin/mylist", "password");

// Work with list settings
myList.Read();  // read all current settings
myList.General.MaxMessageSize = 500;
myList.Privacy.SubscribePolicy = SubscribePolicyOption.Confirm;
myList.Write();

// Add members
var newMembers = new List<string>() { "bob@example.com", "jane@example.com" };
myList.Membership.Subscribe(newMembers);
List<string> currentMembers = myList.Membership.EmailList;  

// Change a member's settings
Member bob = myList.Membership.GetMembers("bob@example.com").Single();
bob.NoMail = true;
myList.Membership.SaveMembers(bob);

// Work with configuration as XML
string listConfig = myList.CurrentConfig;
// Maybe change some things in the XML, or save to file, then ...
myOtherList.LoadConfig(listConfig);
myOtherList.Write();
```

## NuGet
Available at https://www.nuget.org/packages/MailmanSharp/
