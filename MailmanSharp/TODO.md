# ToDo

- [x] `InvokeSectionMethod()`
- [x] Section initializers for properties
- [x] Parallel
- [x] Custom exceptions
- [x] Move all the helpers in `SectionBase` to utility classes
- [x] Get rid of `SafeSelectNodes()`
- [x] Test with properties that don't exist in Mailman
- [x] Make `MailmanVersion` more usable
- [ ] Introduce some kind of internal 'tested with' property?
- [ ] ~~Don't let RestSharp leak through~~
- [x] Parameters as tuples
- [ ] RosterRequest should check status code
- [x] Implement `IEquatable` on `Member`
- [x] Differentiate `textarea` strings and lists
- [x] Make all properties nullable
- [x] Switch to JSON
- [x] `ReadAsync` needs to start with a blank object
- [ ] Ignore changes to `RealName`
- [ ] Handle validation errors? See below
- [x] `CurrentConfig` needs to include ignored props
- [x] `Privacy.EquivalentDomains` should be a list of lists
- [x] Add test for `General.NewMemberOptions`
- [x] Allow additional text for subscription notice





### Error handling

- `General.Moderator` has to be email
- `General.ReplyToAddress` has to be email
- `General.ReplyGoesToList` can't be `ExplicitAddress` without a `ReplyToAddress`
- Passwords have to match
- `Nondigest.RegularIncludeLists` have to be email
- `Nondigest.RegularExcludeLists` have to be email
- `Privacy.SubscribeAutoApproval` has to be email
- `Privacy.BanList` has to be email
- `Privacy.AcceptTheseNonMembers` has to be email
- `Privacy.HoldTheseNonmembers` has to be email
- `Privacy.RejectTheseNonmembers` has to be email
- `Privacy.DiscardTheseNonmembers` has to be email
- `Privacy.AcceptableAliases` has to be email
- `MailNews` Can't enable gatewaying unless `NntpHost` and `LinkedNewsgroup` are both filled in


Errors are in an `h3` right under `Body`. See `Membership.ChangeMemberAddressAsync()`