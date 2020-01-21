// https://dbdiagram.io/

Enum ClientType {
  Client
  Partner
  Referral
}

Enum ClientArchetype {
  Basic
  Segregated
  Var 
  Hybrid
  Enterprise
}

Table Account as t {
  AccountId int [pk]
  Name varchar
  Type ClientType
  SalesforceId varchar
  Archetype ClientArchetype
}

Table PrimarySubscription as ps {
  PrimarySubscriptionId int [pk]
  AccountId int
  ParentSubscriptionId int
  OrganizationalUnit varchar
  ActivationDate date
}

Enum SubscriptionType {
  Production
  Staging
}

Table Subscription as s {
  SubscriptionId int [pk]
  PrimarySubscriptionId int
  ParentSubscriptionId int
  OrganizationalUnit varchar
  Type SubscriptionType
  ActivationDate date
}

Enum DataLinkType {
  Push
  Pull
}

Table DataLink as dl {
  SourceSubscriptionId int [pk]
  DestinationSubscriptionId int [pk]
  Type DataLinkType
  ValidFrom date
  Active bit
}

Table UserSet as us {
  UserSetId int [pk]
  PrimarySubscriptionId int
  Name varchar
  Description varchar
}

Ref:ps.AccountId > t.AccountId
Ref:dl.SourceSubscriptionId > s.SubscriptionId
Ref:dl.DestinationSubscriptionId > s.SubscriptionId
Ref:s.PrimarySubscriptionId > ps.PrimarySubscriptionId
Ref:s.ParentSubscriptionId > s.SubscriptionId
Ref:us.PrimarySubscriptionId > ps.PrimarySubscriptionId