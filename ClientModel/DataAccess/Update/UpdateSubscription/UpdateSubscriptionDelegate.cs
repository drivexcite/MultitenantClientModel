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

namespace ClientModel.DataAccess.Update.UpdateSubscription
{
    public class UpdateSubscriptionDelegate
    {
        private readonly ClientsDb _db;
        private readonly IMapper _mapper;

        public UpdateSubscriptionDelegate(ClientsDb db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public virtual async Task<SubscriptionDto> UpdateSubscriptionAsync(int accountId, int subscriptionId, UpdateSubscriptionDto subscriptionDto)
        {
            var validationErrors = new List<ValidationResult>();
            Utils.ValidateDto(subscriptionDto, validationErrors);

            try
            {
                var subscription = await Utils.GetSubscriptionAsync(_db, accountId, subscriptionId);

                subscription.Name = !string.IsNullOrEmpty(subscriptionDto.SubscriptionName)
                    ? subscriptionDto.SubscriptionName
                    : subscription.Name;

                subscription.Description = !string.IsNullOrEmpty(subscriptionDto.Description)
                    ? subscriptionDto.Description
                    : subscription.Description;

                subscription.Tags = subscriptionDto.Tags != null
                    ? JsonConvert.SerializeObject(subscriptionDto.Tags)
                    : subscription.Description;

                subscription.OrganizationalUnit = !string.IsNullOrEmpty(subscriptionDto.OrganizationalUnit)
                    ? subscriptionDto.OrganizationalUnit
                    : subscription.OrganizationalUnit;

                _db.Entry(subscription).State = EntityState.Modified;

                await _db.SaveChangesAsync();
                return _mapper.Map<SubscriptionDto>(subscription);
            }
            catch (DbException e)
            {
                throw new PersistenceException($"An error occurred while reading an IdentityProvider ({nameof(accountId)} = {accountId}, {nameof(subscriptionId)} = {subscriptionId}, {nameof(subscriptionDto)} = {JsonConvert.SerializeObject(subscriptionDto)})", e);
            }
        }
    }
}
