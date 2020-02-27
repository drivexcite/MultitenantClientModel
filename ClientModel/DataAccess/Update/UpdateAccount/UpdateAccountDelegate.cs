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

namespace ClientModel.DataAccess.Update.UpdateAccount
{
    public class UpdateAccountDelegate
    {
        private readonly ClientsDb _db;
        private readonly IMapper _mapper;

        public UpdateAccountDelegate(ClientsDb db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public virtual async Task<AccountDto> UpdateAccountAsync(int accountId, UpdateAccountDto accountDto)
        {
            var account = await Utils.GetAccountAsync(_db, accountId);

            try
            {
                var errors = new List<ValidationResult>();

                Utils.ValidateDto(accountDto, errors);
                Utils.ThrowAggregateExceptionOnValidationErrors(errors);

                account.Name = !string.IsNullOrEmpty(accountDto.AccountName) 
                    ? accountDto.AccountName 
                    : account.Name;

                account.AccountTypeId = accountDto.AccountTypeId != default 
                    ? accountDto.AccountTypeId 
                    : account.AccountTypeId;

                account.ArchetypeId = accountDto.ArchetypeId != default
                    ? accountDto.ArchetypeId 
                    : account.ArchetypeId;

                account.SalesforceAccountId = !string.IsNullOrEmpty(accountDto.SalesforceAccountId)
                    ? accountDto.SalesforceAccountId
                    : account.SalesforceAccountId;

                account.SalesforceAccountManager = !string.IsNullOrEmpty(accountDto.SalesforceAccountManager)
                    ? accountDto.SalesforceAccountManager
                    : account.SalesforceAccountManager;

                account.SalesforceAccountNumber = !string.IsNullOrEmpty(accountDto.SalesforceAccountNumber)
                    ? accountDto.SalesforceAccountNumber
                    : account.SalesforceAccountNumber;

                account.SalesforceAccountUrl = !string.IsNullOrEmpty(accountDto.SalesforceAccountUrl)
                    ? accountDto.SalesforceAccountUrl
                    : account.SalesforceAccountUrl;

                account.ContractNumber = !string.IsNullOrEmpty(accountDto.ContractNumber)
                    ? accountDto.ContractNumber
                    : account.ContractNumber;

                _db.Entry(account).State = EntityState.Modified;
                await _db.SaveChangesAsync();

                return _mapper.Map<AccountDto>(account);
            }
            catch (DbException e)
            {
                throw new PersistenceException($"An error occurred while updating Account ({nameof(accountId)}={accountId}, {nameof(accountDto)}={JsonConvert.SerializeObject(accountDto)})", e);
            }
        }
    }
}
