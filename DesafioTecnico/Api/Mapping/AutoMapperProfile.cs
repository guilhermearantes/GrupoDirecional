using AutoMapper;
using DesafioTecnico.Api.DTOs;
using DesafioTecnico.Domain.Entities;

namespace DesafioTecnico.Api.Mapping
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Cliente, ClienteReadDto>();
            CreateMap<ClienteCreateDto, Cliente>();
            CreateMap<ClienteUpdateDto, Cliente>();

            CreateMap<Apartamento, ApartamentoReadDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
            CreateMap<ApartamentoCreateDto, Apartamento>();
            CreateMap<ApartamentoUpdateDto, Apartamento>();

            CreateMap<Venda, VendaReadDto>();
            CreateMap<VendaCreateDto, Venda>();
            CreateMap<VendaUpdateDto, Venda>();

            CreateMap<Reserva, ReservaReadDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
            CreateMap<ReservaCreateDto, Reserva>();
        }
    }
}
