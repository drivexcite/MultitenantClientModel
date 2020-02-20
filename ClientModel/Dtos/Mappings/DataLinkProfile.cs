using AutoMapper;
using ClientModel.Entities;

namespace ClientModel.Dtos.Mappings
{
    public class DataLinkProfile : Profile
    {
        public DataLinkProfile()
        {
            CreateMap<DataLink, DataLinkDto>()
                .ForMember(viewModel => viewModel.From, o => o.MapFrom(entity => entity.From.Name))
                .ForMember(viewModel => viewModel.To, o => o.MapFrom(entity => entity.To.Name))
                .ForMember(viewModel => viewModel.Type, o => o.MapFrom(entity => entity.Type.Name));
        }
    }
}
