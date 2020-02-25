using System.Collections.Generic;
using ClientModel.Dtos;

namespace ClientModel.Test.DataAccess
{
    public static class DtoProvider
    {
        public static AccountDto CreateValidAccountDefinition()
        {
            return new AccountDto
            {
                AccountName = "Health Dialog",
                AccountTypeId = 1,
                ArchetypeId = 1,
                SalesforceAccountId = "SF00001",
                SalesforceAccountUrl = "https://healtwise.salesforce.com/accounts/SF00001",
                SalesforceAccountNumber = null,
                SalesforceAccountManager = "Katie Haller <khaller@healthwise.org>",
                ContractNumber = "HW.SF00001",
                Subscriptions = new List<SubscriptionDto>
                {
                    new SubscriptionDto {
                        SubscriptionName = "Health Dialog - Main",
                        Description = "Production subscription for Health Dialog",
                        Tags = new Dictionary<string, string> {
                            ["Managed"] = "true",
                            ["PHI"] = "true"
                        },
                        OrganizationalUnit = "Production",
                        SubscriptionTypeId = 1
                    },
                    new SubscriptionDto {
                        SubscriptionName = "Health Dialog - Staging",
                        Description = "Production subscription for Health Dialog",
                        Tags = new Dictionary<string, string> {
                            ["Managed"] = "true",
                            ["PHI"] = "false"
                        },
                        OrganizationalUnit = "Staging",
                        SubscriptionTypeId = 2
                    }
                },
                IdentityProviders = new List<IdentityProviderDto>()
                {
                    new IdentityProviderDto
                    {
                        Name = "Health Dialog IdP"
                    }
                }
            };
        }
    }
}