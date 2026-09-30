using BecaNet.Api.Data;
using BecaNet.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BecaNet.Api.Repositories;

public interface IUsuarioRepository
{
    Task<bool> ExisteCorreoAsync(string correo);
    Task<Estudiante> CrearEstudianteAsync(Estudiante estudiante);
    Task<Usuario?> BuscarPorCorreoAsync(string correo);
    Task<Usuario?> BuscarPorIdAsync(int id, string tipoUsuario);
    Task ActualizarAsync(Usuario usuario);
}

public class UsuarioRepository : IUsuarioRepository
{
    private readonly BecaNetDbContext _context;

    public UsuarioRepository(BecaNetDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ExisteCorreoAsync(string correo)
    {
        var enEstudiantes = await _context.Estudiantes.AnyAsync(e => e.Correo == correo);
        var enCoordinadores = await _context.Coordinadores.AnyAsync(c => c.Correo == correo);
        var enEvaluadores = await _context.Evaluadores.AnyAsync(e => e.Correo == correo);
        return enEstudiantes || enCoordinadores || enEvaluadores;
    }

    public async Task<Estudiante> CrearEstudianteAsync(Estudiante estudiante)
    {
        _context.Estudiantes.Add(estudiante);
        await _context.SaveChangesAsync();
        return estudiante;
    }

    /// <summary>Busca en las 3 tablas de usuarios hasta encontrar el correo (US-002: login unificado).</summary>
    public async Task<Usuario?> BuscarPorCorreoAsync(string correo)
    {
        Usuario? usuario = await _context.Estudiantes.FirstOrDefaultAsync(e => e.Correo == correo);
        usuario ??= await _context.Coordinadores.FirstOrDefaultAsync(c => c.Correo == correo);
        usuario ??= await _context.Evaluadores.FirstOrDefaultAsync(e => e.Correo == correo);
        return usuario;
    }

    public async Task<Usuario?> BuscarPorIdAsync(int id, string tipoUsuario)
    {
        return tipoUsuario switch
        {
            TiposUsuario.Estudiante => await _context.Estudiantes.FindAsync(id),
            TiposUsuario.Coordinador => await _context.Coordinadores.FindAsync(id),
            TiposUsuario.Evaluador => await _context.Evaluadores.FindAsync(id),
            _ => null
        };
    }

    public async Task ActualizarAsync(Usuario usuario)
    {
        _context.Entry(usuario).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }
}