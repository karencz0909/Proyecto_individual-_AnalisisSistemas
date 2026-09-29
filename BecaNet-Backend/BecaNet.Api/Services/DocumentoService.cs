using BecaNet.Api.Data;
using BecaNet.Api.DTOs;
using BecaNet.Api.Models;
using BecaNet.Api.Repositories;

namespace BecaNet.Api.Services;

public interface IDocumentoService
{
    Task<DocumentoDTO> CargarDocumentoAsync(int idSolicitud, IFormFile archivo);
    Task<List<DocumentoDTO>> ObtenerPorSolicitudAsync(int idSolicitud);
}

public class DocumentoService : IDocumentoService
{
    private readonly IDocumentoRepository _documentoRepo;
    private readonly BecaNetDbContext _context;
    private readonly IWebHostEnvironment _env;

    public DocumentoService(IDocumentoRepository documentoRepo, BecaNetDbContext context, IWebHostEnvironment env)
    {
        _documentoRepo = documentoRepo;
        _context = context;
        _env = env;
    }

    public async Task<DocumentoDTO> CargarDocumentoAsync(int idSolicitud, IFormFile archivo)
    {
        var solicitud = await _context.Solicitudes.FindAsync(idSolicitud)
            ?? throw new InvalidOperationException("La solicitud no existe.");

        if (archivo is null || archivo.Length == 0)
            throw new InvalidOperationException("Debe adjuntar un archivo.");

        var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
        if (!TiposArchivoPermitidos.Extensiones.Contains(extension))
            throw new InvalidOperationException("Formato de archivo no permitido. Solo se aceptan PDF, JPG y PNG.");

        if (archivo.Length > TiposArchivoPermitidos.TamanoMaximoBytes)
            throw new InvalidOperationException("El archivo supera el tamaño máximo permitido (5 MB).");

        var carpetaDestino = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads", idSolicitud.ToString());
        Directory.CreateDirectory(carpetaDestino);

        var nombreUnico = $"{Guid.NewGuid()}{extension}";
        var rutaCompleta = Path.Combine(carpetaDestino, nombreUnico);

        using (var stream = new FileStream(rutaCompleta, FileMode.Create))
        {
            await archivo.CopyToAsync(stream);
        }

        var tipoArchivo = extension.Replace(".", "").ToUpperInvariant();
        if (tipoArchivo == "JPEG") tipoArchivo = "JPG";

        var documento = new Documento
        {
            IdSolicitud = idSolicitud,
            NombreArchivo = archivo.FileName,
            TipoArchivo = tipoArchivo,
            UrlArchivo = $"/uploads/{idSolicitud}/{nombreUnico}",
            FechaCarga = DateTime.Now
        };

        var creado = await _documentoRepo.CrearAsync(documento);

        return new DocumentoDTO
        {
            Id = creado.Id,
            NombreArchivo = creado.NombreArchivo,
            TipoArchivo = creado.TipoArchivo,
            UrlArchivo = creado.UrlArchivo,
            FechaCarga = creado.FechaCarga,
            IdSolicitud = creado.IdSolicitud
        };
    }

    public async Task<List<DocumentoDTO>> ObtenerPorSolicitudAsync(int idSolicitud)
    {
        var documentos = await _documentoRepo.ObtenerPorSolicitudAsync(idSolicitud);
        return documentos.Select(d => new DocumentoDTO
        {
            Id = d.Id,
            NombreArchivo = d.NombreArchivo,
            TipoArchivo = d.TipoArchivo,
            UrlArchivo = d.UrlArchivo,
            FechaCarga = d.FechaCarga,
            IdSolicitud = d.IdSolicitud
        }).ToList();
    }
}