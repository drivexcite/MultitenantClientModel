using AutoMapper;
using ClientModel.Entities;

namespace ClientModel.Dtos.Mappings
{
    public class IdentityProviderProfile : Profile
    {        
        public IdentityProviderProfile()
        {
            CreateMap<IdentityProvider, IdentityProviderDto>();
        }
    }
}
