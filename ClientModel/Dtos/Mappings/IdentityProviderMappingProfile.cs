using AutoMapper;
using ClientModel.Entities;

namespace ClientModel.Dtos.Mappings
{
    public class IdentityProviderMappingProfile : Profile
    {
        public IdentityProviderMappingProfile()
        {
            CreateMap<IdentityProviderMapping, IdentityProviderDto>()
                .ForMember(viewModel => viewModel.Name, o => o.MapFrom(entity => entity.IdentityProvider.Name))
                .ForMember(viewModel => viewModel.IdentityProviderId, o => o.MapFrom(entity => entity.IdentityProviderId));
        }
    }
}
