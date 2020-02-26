using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ClientModel.Dtos;
using ClientModel.Entities;
using ClientModel.Exceptions;
using Microsoft.EntityFrameworkCore;
using Z.EntityFramework.Plus;

namespace ClientModel.DataAccess.Get.GetDataLink
{
    public class GetDataLinkDelegate
    {
        private readonly ClientsDb _db;
        private readonly IMapper _mapper;

        public GetDataLinkDelegate(ClientsDb db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public virtual async Task<(List<DataLinkDto>, int)> GetDataLinksForAccountAsync(int accountId, int skip, int top)
        {
            try
            {
                var doesAccountExistFuture = (from a in _db.Accounts where a.AccountId == accountId select 1).DeferredAny().FutureValue();
                var subscriptionIdsFuture = (from s in _db.Subscriptions where s.AccountId == accountId select s.SubscriptionId).Future();

                if (!await doesAccountExistFuture.ValueAsync())
                {
                    throw new AccountNotFoundException($"An account with AccountId = {accountId} doesn't exist.");
                }

                var subscriptionIds = await subscriptionIdsFuture.ToListAsync();

                if (subscriptionIds.Count < 1)
                    return (new List<DataLinkDto>(), 0);

                var dataLinksQuery = (
                    from l in _db.DataLinks
                        .Include(l => l.From)
                        .Include(l => l.To)
                        .Include(l => l.Type)
                    where subscriptionIds.Contains(l.FromSubscriptionId)
                          || subscriptionIds.Contains(l.ToSubscriptionId)
                    select l
                );

                var dataLinksCountFuture = dataLinksQuery.DeferredCount().FutureValue();
                var dataLinksFuture = dataLinksQuery.Skip(skip).Take(top).Future();

                var count = await dataLinksCountFuture.ValueAsync();
                var dataLinks = (from l in await dataLinksFuture.ToListAsync() select _mapper.Map<DataLinkDto>(l)).ToList();

                return (dataLinks, count);
            }
            catch (DbException e)
            {
                throw new PersistenceException($"An error occurred while reading a DataLink ({nameof(accountId)} = {accountId}, {nameof(skip)} = {skip}, {nameof(top)} = {top})", e);
            }
        }

        public virtual async Task<(List<DataLinkDto>, int)> GetDataLinksForAccountAndSubscriptionAsync(int accountId, int subscriptionId, int skip, int top)
        {
            try
            {
                var doesAccountExistFuture = (from a in _db.Accounts where a.AccountId == accountId select 1).DeferredAny().FutureValue();
                var doesSubscriptionExistInAccountFuture = (from s in _db.Subscriptions where s.AccountId == accountId && s.SubscriptionId == subscriptionId select 1).DeferredAny().FutureValue();

                if (!await doesAccountExistFuture.ValueAsync())
                {
                    throw new AccountNotFoundException($"An account with AccountId = {accountId} doesn't exist.");
                }

                if (!await doesSubscriptionExistInAccountFuture.ValueAsync())
                {
                    throw new MalformedSubscriptionException($"A subscription with SubscriptionId = {subscriptionId} doesn't exist in Account with AccountId = {accountId}.");
                }

                var dataLinksQuery = (
                    from l in _db.DataLinks
                        .Include(l => l.From)
                        .Include(l => l.To)
                        .Include(l => l.Type)
                    where l.FromSubscriptionId == subscriptionId
                    select l
                );

                var dataLinksFutureCount = dataLinksQuery.DeferredCount().FutureValue();
                var dataLinksFuture = dataLinksQuery.Skip(skip).Take(top).Future();

                var count = await dataLinksFutureCount.ValueAsync();
                var dataLinks = (from l in await dataLinksFuture.ToListAsync() select _mapper.Map<DataLinkDto>(l)).ToList();

                return (dataLinks, count);
            }
            catch (DbException e)
            {
                throw new PersistenceException($"An error occurred while reading a DataLink ({nameof(accountId)} = {accountId}, {nameof(subscriptionId)} = {subscriptionId}, {nameof(skip)} = {skip}, {nameof(top)} = {top})", e);
            }
        }
    }
}
