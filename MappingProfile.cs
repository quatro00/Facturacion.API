using AutoMapper;
using Facturacion.API.Models.Domain;

namespace Facturacion.API
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            ////organizacion
            //CreateMap<Organizacion, GetOrganizacionDto>();

            //CreateMap<CreateOrganizacionDto, Organizacion>()
            //    .ForMember(x => x.Id, opt => opt.Ignore())
            //    .ForMember(x => x.UsuarioCreacion, opt => opt.Ignore())
            //    .ForMember(x => x.FechaCreacion, opt => opt.Ignore());

            //CreateMap<UpdateOrganizacionDto, Organizacion>()
            //    .ForMember(dest => dest.Id, opt => opt.Ignore())
            //    .ForMember(dest => dest.UsuarioCreacion, opt => opt.Ignore())
            //    .ForMember(dest => dest.FechaCreacion, opt => opt.Ignore());


            ////sistema
            //CreateMap<Sistema, GetSistemaDto>()
            //    .ForMember(
            //        dest => dest.roles,
            //        opt => opt.MapFrom(src =>
            //            src.SistemaRols.Select(sr => sr.Rol)
            //        )
            //    );

            //CreateMap<CreateSistemaDto, Sistema>()
            //    .ForMember(x => x.Id, opt => opt.Ignore())
            //    .ForMember(x => x.UsuarioCreacion, opt => opt.Ignore())
            //    .ForMember(x => x.FechaCreacion, opt => opt.Ignore());


            //CreateMap<UpdateSistemaDto, Sistema>()
            //    .ForMember(dest => dest.Id, opt => opt.Ignore())
            //    .ForMember(dest => dest.UsuarioCreacion, opt => opt.Ignore())
            //    .ForMember(dest => dest.Activo, opt => opt.Ignore())
            //    .ForMember(dest => dest.FechaCreacion, opt => opt.Ignore());
        }
    }
}
