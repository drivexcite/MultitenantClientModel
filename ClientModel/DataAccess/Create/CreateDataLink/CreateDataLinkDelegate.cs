using System.Data.Common;
using System.Threading.Tasks;
using AutoMapper;
using ClientModel.Dtos;
using ClientModel.Entities;
using System.Linq;
using ClientModel.Exceptions;
using Newtonsoft.Json;
using Z.EntityFramework.Plus;

namespace ClientModel.DataAccess.Create.CreateDataLink
{
    public class CreateDataLinkDelegate
    {
        private readonly ClientsDb _db;
        private readonly IMapper _mapper;

        public CreateDataLinkDelegate(ClientsDb db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public virtual async Task<DataLinkDto> CreateDataLinkAsync(int accountId, DataLinkDto dataLinkDto)
        {
            try
            {
                var prefetch = await PrefetchAndValidate(accountId, dataLinkDto);

                var dataLink = new DataLink
                {
                    FromSubscriptionId = dataLinkDto.FromSubscriptionId,
                    From = prefetch.From,
                    ToSubscriptionId = dataLinkDto.ToSubscriptionId,
                    To = prefetch.To,
                    DataLinkTypeId = dataLinkDto.DataLinkTypeId,
                    Type = prefetch.Type
                };

                _db.DataLinks.Add(dataLink);
                await _db.SaveChangesAsync();

                return _mapper.Map<DataLinkDto>(dataLink);
            }
            catch (DbException e)
            {
                throw new PersistenceException($"An error occurred while creating the DataLink ({nameof(accountId)} = {accountId}, {nameof(dataLinkDto)} = {JsonConvert.SerializeObject(dataLinkDto)})", e);
            }
        }

        private async Task<PrefetchDataLinkResult> PrefetchAndValidate(int accountId, DataLinkDto dataLinkDto)
        {
            var doesAccountExistFuture = (from a in _db.Accounts where a.AccountId == accountId select 1).DeferredAny().FutureValue();
            var dataLinkTypeFuture = (from t in _db.DataLinkTypes where t.DataLinkTypeId == dataLinkDto.DataLinkTypeId select t).DeferredFirstOrDefault().FutureValue();
            var existingDataLink = (
                from l in _db.DataLinks
                where l.FromSubscriptionId == dataLinkDto.FromSubscriptionId
                      && l.ToSubscriptionId == dataLinkDto.ToSubscriptionId
                select 1
            ).DeferredAny().FutureValue();

            var subscriptionIdsFuture = (
                from s in _db.Subscriptions
                where s.AccountId == accountId
                    && (s.SubscriptionId == dataLinkDto.FromSubscriptionId || s.SubscriptionId == dataLinkDto.ToSubscriptionId)
                select s
            ).Future();

            if (!await doesAccountExistFuture.ValueAsync())
            {
                throw new AccountNotFoundException($"An account with AccountId = {accountId} doesn't exist.");
            }

            if (await existingDataLink.ValueAsync())
            {
                throw new MalformedDataLinkException($"An existing DataLink from subscription with SubscriptionId = {dataLinkDto.FromSubscriptionId} to subscription with SubscriptionId = {dataLinkDto.ToSubscriptionId} already exists.");
            }

            var dataLinkType = await dataLinkTypeFuture.ValueAsync();

            if (dataLinkType == null)
            {
                throw new DataLinkTypeNotFoundException($"A data link type with DataLinkTypeId = {dataLinkDto.DataLinkTypeId} could not be found.");
            }

            var subscriptions = await subscriptionIdsFuture.ToListAsync();
            var fromSubscription = subscriptions.FirstOrDefault(s => s.SubscriptionId == dataLinkDto.FromSubscriptionId);
            var toSubscription = subscriptions.FirstOrDefault(s => s.SubscriptionId == dataLinkDto.ToSubscriptionId);

            if (fromSubscription == null)
            {
                throw new MalformedSubscriptionException($"A subscription with SubscriptionId = {dataLinkDto.FromSubscriptionId} does not exists inside Account with AccountId {accountId}");
            }

            if (toSubscription == null)
            {
                throw new MalformedSubscriptionException($"A subscription with SubscriptionId = {dataLinkDto.ToSubscriptionId} does not exists inside Account with AccountId {accountId}");
            }

            return new PrefetchDataLinkResult { From = fromSubscription, To = toSubscription, Type = dataLinkType };
        }
    }

    internal class PrefetchDataLinkResult
    {
        public Subscription From { get; set; }
        public Subscription To { get; set; }
        public DataLinkType Type { get; set; }
    }
}
