using Microsoft.EntityFrameworkCore;
using ProdutosAPI.Shared.Data;
using ProdutosAPI.Shared.Common;
using ProdutosAPI.Pedidos.Common;
using ProdutosAPI.Pedidos.Domain;

namespace ProdutosAPI.Pedidos.ListPedidos;

public record ListPedidosQuery(int Page = 1, int PageSize = 20, string? Status = null);

public record ListPedidosResponse(List<PedidoResponse> Data, int Total, int Page, int PageSize);

public class ListPedidosHandler(AppDbContext db)
{
    public async Task<Result<ListPedidosResponse>> HandleAsync(
        ListPedidosQuery query, CancellationToken ct = default)
    {
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var page = Math.Max(query.Page, 1);

        var q = db.Pedidos.Include(p => p.Itens).AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Status)
            && Enum.TryParse<StatusPedido>(query.Status, true, out var status))
        {
            q = q.Where(p => p.Status == status);
        }

        var total = await q.CountAsync(ct);
        var pedidos = await q
            .OrderByDescending(p => p.CriadoEm)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var response = new ListPedidosResponse(
            pedidos.Select(PedidoResponse.From).ToList(),
            total, page, pageSize);

        return Result<ListPedidosResponse>.Ok(response);
    }
}
