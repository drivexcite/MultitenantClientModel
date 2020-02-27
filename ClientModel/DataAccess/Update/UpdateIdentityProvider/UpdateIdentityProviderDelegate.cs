using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.Threading.Tasks;
using AutoMapper;
using ClientModel.DataAccess.Common;
using ClientModel.Dtos;
using ClientModel.Dtos.Update;
using ClientModel.Entities;
using ClientModel.Exceptions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace ClientModel.DataAccess.Update.UpdateIdentityProvider
{
    public class UpdateIdentityProviderDelegate
    {
        private readonly ClientsDb _db;
        private readonly IMapper _mapper;

        public UpdateIdentityProviderDelegate(ClientsDb db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public virtual async Task<IdentityProviderDto> UpdateIdentityProvider(int accountId, int identityProviderId, UpdateIdentityProviderDto identityProviderDto)
        {
            var validationErrors = new List<ValidationResult>();
            Utils.ValidateDto(identityProviderDto, validationErrors);

            try
            {
                var identityProvider = await Utils.GetIdentityProviderAsync(_db, accountId, identityProviderId);

                if (!string.Equals(identityProvider.Name, identityProviderDto.Name))
                {
                    identityProvider.Name = identityProviderDto.Name;
                    _db.Entry(identityProvider).State = EntityState.Modified;
                }

                await _db.SaveChangesAsync();
                return _mapper.Map<IdentityProviderDto>(identityProvider);
            }
            catch (DbException e)
            {
                throw new PersistenceException($"An error occurred while reading an IdentityProvider ({nameof(accountId)} = {accountId},{nameof(identityProviderId)} = {identityProviderId}, {nameof(identityProviderDto)} = {JsonConvert.SerializeObject(identityProviderDto)})", e);
            }
        }
    }
}
