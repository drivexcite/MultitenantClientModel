﻿using AutoMapper;
using ClientApi.Entities;

namespace ClientApi.ViewModels.Mappings
{
    public class AccountProfile : Profile
    {
        public AccountProfile()
        {
            CreateMap<Account, AccountViewModel>()
                .ForMember(viewModel => viewModel.AccountName, o => o.MapFrom(entity => entity.Name))
                .ForMember(viewModel => viewModel.AccountTypeId, o => o.MapFrom(entity => entity.AccountTypeId))
                .ForMember(viewModel => viewModel.AccountType, o => o.MapFrom(entity => entity.AccountType.Name))
                .ForMember(viewModel => viewModel.ArchetypeId, o => o.MapFrom(entity => entity.AccountTypeId))
                .ForMember(viewModel => viewModel.Achetype, o => o.MapFrom(entity => entity.Archetype.Name))
                .ForMember(viewModel => viewModel.ArchetypeId, o => o.MapFrom(entity => entity.AccountTypeId))
                .ForMember(viewModel => viewModel.SubscriptionCount, o => o.MapFrom(entity => entity.Subscriptions.Count));
        }
    }
}
