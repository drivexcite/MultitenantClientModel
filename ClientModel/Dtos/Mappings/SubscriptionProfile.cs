using AutoMapper;
using ClientModel.Entities;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace ClientApi.Dtos.Mappings
{
    public class SubscriptionProfile : Profile
    {
        private static Dictionary<string, string> MaterializeTags(string tagsField)
        {
            try
            {
                if (JToken.Parse(tagsField) is JObject jobject)
                {
                    return jobject.ToObject<Dictionary<string, string>>();
                }

                return new Dictionary<string, string>();
            }
            catch { return null; }
        }

        public SubscriptionProfile()
        {
            CreateMap<Subscription, SubscriptionDto>()
                .ForMember(viewModel => viewModel.SubscriptionName, o => o.MapFrom(entity => entity.Name))
                .ForMember(viewModel => viewModel.Tags, o => o.MapFrom(entity => MaterializeTags(entity.Tags)))
                .ForMember(viewModel => viewModel.SubscriptionType, o => o.MapFrom(entity => entity.SubscriptionType.Name));
        }
    }
}
