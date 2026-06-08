using DesafioTecnico.Infrastructure.Repositories;
using DesafioTecnico.Infrastructure.Repositories.Interfaces;

namespace DesafioTecnico.Infrastructure.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        private IClienteRepository? _clientes;
        private IApartamentoRepository? _apartamentos;
        private IReservaRepository? _reservas;
        private IVendaRepository? _vendas;
        private IUsuarioRepository? _usuarios;

        public UnitOfWork(AppDbContext context) => _context = context;

        public IClienteRepository Clientes => _clientes ??= new ClienteRepository(_context);
        public IApartamentoRepository Apartamentos => _apartamentos ??= new ApartamentoRepository(_context);
        public IReservaRepository Reservas => _reservas ??= new ReservaRepository(_context);
        public IVendaRepository Vendas => _vendas ??= new VendaRepository(_context);
        public IUsuarioRepository Usuarios => _usuarios ??= new UsuarioRepository(_context);

        public Task<int> CommitAsync(CancellationToken ct = default)
            => _context.SaveChangesAsync(ct);
    }
}
