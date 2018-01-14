# New Mailman Features

### 2.1.23
- [ ] A list's nonmember_rejection_notice attribute will now be the default
  rejection reason for a held non-member post in addition to it's prior
  role as the reson for an automatically rejected non-member post.
  (LP: #1572330)

### 2.1.21
- [x] There is a new dmarc_none_moderation_action list setting and a
    DEFAULT_DMARC_NONE_MODERATION_ACTION mm_cfg.py setting to optionally
    apply Munge From or Wrap Message actions to posts From: domains that
    publish DMARC p=none.  The intent is to eliminate failure reports to
    the domain owner for messages that would be munged or wrapped if the
    domain published a stronger DMARC policy.  See the descriptions in
    Defaults.py, the web UI and the bug report for more.  (LP: #1539384)

- [x] Thanks to Jim Popovitch there is now a feature to automatically turn
  on moderation for a malicious list member who attempts to flood a list
  with spam.  See the details for the Privacy options ... -> Sender
  filters -> member_verbosity_threshold and member_verbosity_interval
  settings in the web admin UI and the documentation in Defaults.py for
  the DEFAULT_MEMBER_VERBOSITY_* and VERBOSE_CLEAN_LIMIT settings for
  information.

### 2.1.20
- [x] There is a new Address Change sub-section in the web admin Membership
  Management section to allow a list admin to change a list member's
  address in one step rather than adding the new address, copying settings
  and deleting the old address.  (LP: #266809)

### 2.1.19
- [x] There is a new list attribute 'subscribe_auto_approval' which is a list
  of email addresses and regular expressions matching email addresses
  whose subscriptions are exempt from admin approval.  (LP: #266609)

- [x] Added real name display to the web roster.  (LP: #266754)      *I don't see this on my server*

- [x] There is a new list attribute dmarc_wrapped_message_text and a
  DEFAULT_DMARC_WRAPPED_MESSAGE_TEXT setting to set the default for new
  lists.  This text is added to a message which is wrapped because of
  dmarc_moderation_action in a separate text/plain part that precedes the
  message/rfc822 part containing the original message.  It can be used to
  provide an explanation of why the message was wrapped or similar info.

- [x] There is a new list attribute equivalent_domains and a
  DEFAULT_EQUIVALENT_DOMAINS setting to set the default for new lists which
  in turn defaults to the empty string.  This provides a way to specify one
  or more groups of domains, e.g., mac.com, me.com, icloud.com, which are
  considered equivalent for validating list membership for posting and
  moderation purposes.

- [x] There is a new list attribute in the Bounce processing section.
  bounce_notify_owner_on_bounce_increment if set to Yes will cause
  Mailman to notify the list owner on every bounce that increments a
  list member's score but doesn't result in a probe or disable.  There
  is a new configuration setting setting
  DEFAULT_BOUNCE_NOTIFY_OWNER_ON_BOUNCE_INCREMENT to set the default
  for new lists.  This in turn defaults to No.  (LP: #1382150)    

### 2.1.18
- [x] The from_is_list feature introduced in 2.1.16 is now unconditionally
  available to list owners.  There is also, a new Privacy options ->
  Sender filters -> **dmarc_moderation_action** feature which applies to list
  messages where the From: address is in a domain which publishes a DMARC
  policy of reject or possibly quarantine.  This is a list setting with
  values of Accept, Wrap Message, Munge From, Reject or Discard. There is
  a new DEFAULT_DMARC_MODERATION_ACTION configuration setting to set the
  default for this, and the list admin UI is not able to set an action
  which is 'less' than the default.  The prior ALLOW_FROM_IS_LIST setting
  has been removed and is effectively always Yes. There is a new
  **dmarc_quarantine_moderation_action** list setting with default set by a
  new DEFAULT_DMARC_QUARANTINE_MODERATION_ACTION configuration setting
  which in turn defaults to Yes.  The list setting can be set to No to
  exclude domains with DMARC policy of quarantine from
  dmarc_moderation_action.