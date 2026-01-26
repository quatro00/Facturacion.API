using AutoMapper;
using Facturacion.API.Data;
using Facturacion.API.Models.Domain;
using Facturacion.API.Models.Dto.Cliente.Catalogos;
using Facturacion.API.Models.Dto.Cliente.Cliente;
using Facturacion.API.Services.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Facturacion.API.Services.Implementation
{
    public class ClienteService : IClienteService
    {
        private readonly FacturacionContext _context;
        private readonly IMapper _mapper;

        public ClienteService(
            IMapper mapper,
            FacturacionContext context
            )
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<Guid> ActualizarCliente(ActualizarClienteDto request, string userId, Guid clienteId)
        {

            //PENDIENTE
            //agregar validacion de no permitir cambiar rfc ni razon social si ya fuern emitidas facturas
            var cliente = await this._context.Clientes.Where(x => x.Id == clienteId).FirstOrDefaultAsync();
            var clienteConfiguracion = await this._context.ClienteConfiguracions.Where(x => x.ClienteId == clienteId).FirstOrDefaultAsync();


            cliente.Rfc = request.Rfc;
            cliente.RazonSocial = request.RazonSocial;
            cliente.RegimenFiscalId = request.RegimenFiscal;
            cliente.Email = request.Email;
            cliente.Telefono = request.Telefono;
            cliente.Pais = request.Pais;
            cliente.CodigoPostal = request.CodigoPostal;
            cliente.Estado = request.Estado;
            cliente.Municipio = request.Municipio;
            cliente.Colonia = request.Colonia;
            cliente.Calle = request.Calle;
            cliente.NumeroExterior = request.NoExterior;
            cliente.NumeroInterior = request.NoInterior;
            
            if(clienteConfiguracion != null)
            {
                clienteConfiguracion.MetodoPago = request.MetodoPago;
                clienteConfiguracion.FormaPago = request.FormaPago;
                clienteConfiguracion.Moneda = request.Moneda;
                clienteConfiguracion.Exportacion = request.Exportacion;
                clienteConfiguracion.UsoCfdiDefault = request.UsoCfdi;
            }
            

            await this._context.SaveChangesAsync();
            return clienteId;
        }

        public async Task<Guid> CrearClienteAsync(CrearClienteRequest request, string userId, Guid cuentaId)
        {
            // Validar duplicado
            if (await _context.Clientes.AnyAsync(c => c.Rfc == request.Rfc && c.CuentaId == cuentaId))
                throw new Exception("Ya existe un cliente con ese RFC.");

            var clienteId = Guid.NewGuid();

            var codigoPostal = request.CodigoPostal;

            var cliente = new Cliente
            {
                Id = clienteId,
                Rfc = request.Rfc,
                RazonSocial = request.RazonSocial,
                RegimenFiscalId = request.RegimenFiscal,
                Email = request.Email,
                Telefono = request.Telefono,
                Calle = request.Calle,
                Pais = request.Pais,
                CodigoPostal = codigoPostal,
                UsuarioCreacionId = userId,
                Activo = true,
                Colonia = request.Colonia,
                Estado = request.Estado,
                Municipio = request.Municipio,
                NumeroExterior = request.NoExterior,
                NumeroInterior = request.NoInterior,
                CuentaId = cuentaId,
                FechaCreacion = DateTime.UtcNow
            };

            _context.Clientes.Add(cliente);

            var config = new ClienteConfiguracion
            {
                Id = Guid.NewGuid(),
                ClienteId = clienteId,
                MetodoPago = request.MetodoPago,
                FormaPago = request.FormaPago,
                Activo = true,
                Moneda = request.Moneda ?? "MXN",
                Exportacion = request.Exportacion ?? "01",
                UsoCfdiDefault = request.UsoCfdi,
                UsuarioCreacionId = userId,
                FechaCreacion = DateTime.Now,
            };

            _context.ClienteConfiguracions.Add(config);
            await _context.SaveChangesAsync();


            return clienteId;
        }

        public async Task<ClienteDetalleDto?> GetByIdAsync(Guid id)
        {
            return await _context.Clientes
            .AsNoTracking()
            .Include(x => x.ClienteConfiguracions)
            .Where(x => x.Id == id)
            .Select(x => new ClienteDetalleDto
            {
                id = x.Id,
                rfc = x.Rfc,
                razonSocial = x.RazonSocial,
                regimenFiscalId = x.RegimenFiscalId,
                email = x.Email,
                telefono = x.Telefono,
                pais = x.Pais,
                codigoPostal = x.CodigoPostal,
                estado = x.Estado,
                calle = x.Calle,
                noInterior = x.NumeroInterior ?? "",
                noExterior = x.NumeroExterior,
                colonia = x.Colonia ?? "",
                metodoPago = x.ClienteConfiguracions.FirstOrDefault()!.MetodoPago ?? "",
                formaPago = x.ClienteConfiguracions.FirstOrDefault()!.FormaPago ?? "",
                moneda = x.ClienteConfiguracions.FirstOrDefault()!.Moneda ?? "",
                exportacion = x.ClienteConfiguracions.FirstOrDefault()!.Exportacion ?? "",
                usoCfdi = x.ClienteConfiguracions.FirstOrDefault()!.UsoCfdiDefault ?? ""
            })
            .FirstOrDefaultAsync();
        }

        public async Task<List<ClienteListadoDto>> ObtenerClientesAsync(Guid cuentaId)
        {
            return await (
                from c in _context.Clientes
                where c.CuentaId == cuentaId
                join cfg in _context.ClienteConfiguracions
                    on c.Id equals cfg.ClienteId into cfgJoin
                from cfg in cfgJoin.DefaultIfEmpty()
                orderby c.RazonSocial
                select new ClienteListadoDto
                {
                    ClienteId = c.Id,
                    Rfc = c.Rfc,
                    RazonSocial = c.RazonSocial,
                    UsoCfdi = cfg != null ? cfg.UsoCfdiDefault : null,
                    Moneda = cfg != null ? cfg.Moneda : null,
                    Activo = c.Activo
                }
            ).ToListAsync();
        }

    }
}
