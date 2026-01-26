using AutoMapper;
using Facturacion.API.Models.Domain;
using Facturacion.API.Models.Dto.Cliente.Catalogos;
using Facturacion.API.Models.Dto.Cliente.Cliente;
using Facturacion.API.Models.Dto.Cliente.Perfil;

namespace Facturacion.API
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            //catalogos
            CreateMap<CRegimenFiscal, ClienteGetRegimenFiscal>();
            CreateMap<CMetodoPago, GetMetodoPagoDto>();
            CreateMap<CFormaPago, GetFormaPagoDto>();
            CreateMap<CMonedum, GetMonedaDto>();
            CreateMap<CExportacion, GetExportacionDto>();
            CreateMap<CUsoCfdi, GetUsoCfdiDto>();

            //organizacion
            CreateMap<RazonSocial, GetRazonSocialResponse>()
                 .ForMember(dest => dest.razonSocial, opt => opt.MapFrom(src => src.RazonSocial1));

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
