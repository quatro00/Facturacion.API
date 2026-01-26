using Facturacion.API.Data;
using Facturacion.API.Models.Domain;
using Facturacion.API.Models.Dto.Cliente.Cliente;
using Facturacion.API.Services.Interface;
using Microsoft.EntityFrameworkCore;
using System;

namespace Facturacion.API.Services.Implementation
{
    public class ClienteService : IClienteService
    {
        private readonly FacturacionContext _context;

        public ClienteService(FacturacionContext context)
        {
            _context = context;
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
                    Moneda = cfg != null ? cfg.Moneda : null
                }
            ).ToListAsync();
        }
    }
}
