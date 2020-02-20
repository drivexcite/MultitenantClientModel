using AutoMapper;
using ClientModel.Dtos;
using ClientModel.Entities;
using ClientModel.Exceptions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

namespace ClientModel.DataAccess.Create.CreateIdentityProvider
{
    public class CreateIdentityProviderDelegate
    {
        private readonly ClientsDb _db;
        private readonly IMapper _mapper;

        public CreateIdentityProviderDelegate(ClientsDb db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<IdentityProviderDto> CreateIdentityProvider(int accountId, IdentityProviderDto identityProvider)
        {
            var validationErrors = new List<ValidationResult>();
            Validator.TryValidateObject(identityProvider, new ValidationContext(identityProvider, null, null), validationErrors, true);

            if (validationErrors.Count > 0)
            {
                throw new AggregateException(validationErrors.Select((e) => new ValidationException(e.ErrorMessage)));
            }

            var account = (from a in _db.Accounts where a.AccountId == accountId select a).FirstOrDefault();
            if (account == null)
            {
                throw new AccountNotFoundException($"An account with AccountId {accountId} could not be found");
            }

            if (account.IdentityProviders.Select(p => string.Equals(p.Name, identityProvider.Name, StringComparison.OrdinalIgnoreCase)).Any())
            {
                throw new MalformedAccountException($"An identity provider with the same name [{identityProvider.Name}] already exists.");
            }

            var newIdentityProvider = new IdentityProvider
            {
                Name = identityProvider.Name,
                Account = account,
                AccountId = accountId
            };

            account.IdentityProviders.Add(newIdentityProvider);
            await _db.SaveChangesAsync();

            return _mapper.Map<IdentityProviderDto>(newIdentityProvider);
        }
    }
}
