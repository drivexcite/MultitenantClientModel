﻿using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ClientModel.DataAccess.Common;
using ClientModel.Dtos;
using ClientModel.Entities;
using ClientModel.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace ClientModel.DataAccess.Get.GetSubscriptions
{
    public class GetSubscriptionDelegate
    {
        private readonly ClientsDb _db;
        private readonly IMapper _mapper;

        public GetSubscriptionDelegate(ClientsDb db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public virtual async Task<(List<SubscriptionDto>, int)> GetSubscriptionsAsync(int accountId, int skip, int top)
        {
            try
            {
                var (subscriptions, total) = await GetSubscriptionsAndCountAsync(accountId, skip, top);
                return (await _mapper.ProjectTo<SubscriptionDto>(subscriptions).ToListAsync(), total);
            }
            catch (DbException e)
            {
                throw new PersistenceException($"An error occurred while reading a DataLink ({nameof(accountId)} = {accountId}, {nameof(skip)} = {skip}, {nameof(top)} = {top})", e);
            }
        }

        private async Task<(IQueryable<Subscription>, int)> GetSubscriptionsAndCountAsync(int accountId, int skip, int top)
        {
            var doesAccountExist = await (from a in _db.Accounts where a.AccountId == accountId select 1).AnyAsync();
            var accountSubscriptions = (from s in _db.Subscriptions where s.AccountId == accountId select s);

            if (!doesAccountExist)
                throw new AccountNotFoundException($"An account with AccountId {accountId} could not be found");

            return (accountSubscriptions.Skip(skip).Take(top), accountSubscriptions.Count());
        }

        public virtual async Task<SubscriptionDto> GetSubscriptionAsync(int accountId, int subscriptionId)
        {
            try
            {
                var subscription = await Utils.GetSubscriptionAsync(_db, accountId, subscriptionId);
                return _mapper.Map<SubscriptionDto>(subscription);
            }
            catch (DbException e)
            {
                throw new PersistenceException($"An error occurred while reading a DataLink ({nameof(accountId)} = {accountId}, {nameof(subscriptionId)} = {subscriptionId})", e);
            }
        }
    }
}
