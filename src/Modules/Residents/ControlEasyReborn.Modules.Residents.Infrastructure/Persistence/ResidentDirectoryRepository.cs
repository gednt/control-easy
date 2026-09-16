using ControlEasyReborn.Infrastructure.MultiTenancy;
using ControlEasyReborn.Modules.Residents.Application.Abstractions;
using ControlEasyReborn.Modules.Residents.Domain.Entities;
using ControlEasyReborn.SharedKernel.MultiTenancy;
using DBTools.Abstractions;
using System.Data;

namespace ControlEasyReborn.Modules.Residents.Infrastructure.Persistence;

public sealed class ResidentDirectoryRepository : IResidentDirectory
{
    private const string ResidentsTable = "Residents";
    private const string DocumentsTable = "ResidentIdentityDocuments";
    private const string ResidentFields = "Id, TenantId, Name, Cpf, Email, Phone, ApartmentId, Active, CreatedAtUtc, UpdatedAtUtc";
    private const string DocumentFields = "Id, TenantId, ResidentId, DocumentType, NormalizedValue, Active, CreatedAtUtc, UpdatedAtUtc";

    private readonly ITenantContext _ctx;
    private readonly ITenantAwareLinqFactory _factory;

    public ResidentDirectoryRepository(ITenantContext ctx, ITenantAwareLinqFactory factory)
    {
        _ctx = ctx;
        _factory = factory;
    }

    public async Task<Resident?> FindActiveByCpfAsync(Guid tenantId, string normalizedCpf, CancellationToken ct)
    {
        var db = _factory.Create(_ctx);
        var rows = await db.SelectAsync(
            fields: ResidentFields,
            table: ResidentsTable,
            whereClause: "Cpf = @param0 AND Active = 1",
            parameters: new object[] { normalizedCpf },
            ct: ct);
        return MapResident(rows);
    }

    public async Task<Resident?> FindByIdAsync(Guid tenantId, Guid residentId, CancellationToken ct)
    {
        var db = _factory.Create(_ctx);
        var rows = await db.SelectAsync(
            fields: ResidentFields,
            table: ResidentsTable,
            whereClause: "Id = @param0",
            parameters: new object[] { residentId },
            ct: ct);
        return MapResident(rows);
    }

    public async Task<IReadOnlyList<Resident>> SearchByNameAsync(Guid tenantId, string nameTerm, int skip, int take, CancellationToken ct)
    {
        var db = _factory.Create(_ctx);
        var rows = await db.SelectAsync(
            fields: ResidentFields,
            table: ResidentsTable,
            whereClause: "Name LIKE @param0 AND Active = 1",
            parameters: new object[] { nameTerm + "%" },
            ct: ct);
        return MapResidentList(rows).Skip(skip).Take(take).ToList();
    }

    public async Task<IReadOnlyList<Resident>> SearchByApartmentAsync(Guid tenantId, Guid apartmentId, int skip, int take, CancellationToken ct)
    {
        var db = _factory.Create(_ctx);
        var rows = await db.SelectAsync(
            fields: ResidentFields,
            table: ResidentsTable,
            whereClause: "ApartmentId = @param0 AND Active = 1",
            parameters: new object[] { apartmentId },
            ct: ct);
        return MapResidentList(rows).Skip(skip).Take(take).ToList();
    }

    public async Task<IReadOnlyList<Resident>> SearchByDocumentAsync(Guid tenantId, string documentType, string normalizedValue, CancellationToken ct)
    {
        var db = _factory.Create(_ctx);
        var rows = await db.SelectAsync(
            fields: ResidentFields,
            table: ResidentsTable,
            whereClause: "Id IN (SELECT ResidentId FROM " + DocumentsTable + " WHERE DocumentType = @param0 AND NormalizedValue = @param1 AND Active = 1)",
            parameters: new object[] { documentType, normalizedValue },
            ct: ct);
        return MapResidentList(rows).ToList();
    }

    public async Task<IReadOnlyList<ResidentIdentityDocument>> GetActiveIdentityDocumentsAsync(Guid tenantId, Guid residentId, CancellationToken ct)
    {
        var db = _factory.Create(_ctx);
        var rows = await db.SelectAsync(
            fields: DocumentFields,
            table: DocumentsTable,
            whereClause: "ResidentId = @param0 AND Active = 1",
            parameters: new object[] { residentId },
            ct: ct);
        return MapDocumentList(rows).ToList();
    }

    private static Resident? MapResident(DataTable rows)
    {
        if (rows is null || rows.Rows.Count == 0) return null;
        return MapResidentRow(rows.Rows[0]);
    }

    private static IReadOnlyList<Resident> MapResidentList(DataTable rows)
    {
        if (rows is null || rows.Rows.Count == 0) return Array.Empty<Resident>();
        var list = new List<Resident>(rows.Rows.Count);
        foreach (DataRow r in rows.Rows)
        {
            var mapped = MapResidentRow(r);
            if (mapped is not null) list.Add(mapped);
        }
        return list;
    }

    private static Resident? MapResidentRow(DataRow r)
    {
        var apartmentIdStr = r["ApartmentId"]?.ToString();
        var updatedAtStr = r["UpdatedAtUtc"]?.ToString();
        return new Resident(
            id: Guid.Parse(r["Id"].ToString() ?? string.Empty),
            tenantId: Guid.Parse(r["TenantId"].ToString() ?? string.Empty),
            name: r["Name"]?.ToString() ?? string.Empty,
            cpf: r["Cpf"]?.ToString() ?? string.Empty,
            email: string.IsNullOrEmpty(r["Email"]?.ToString()) ? null : r["Email"].ToString(),
            phone: string.IsNullOrEmpty(r["Phone"]?.ToString()) ? null : r["Phone"].ToString(),
            apartmentId: string.IsNullOrEmpty(apartmentIdStr) ? null : Guid.Parse(apartmentIdStr),
            active: Convert.ToBoolean(r["Active"]),
            createdAtUtc: Convert.ToDateTime(r["CreatedAtUtc"]),
            updatedAtUtc: string.IsNullOrEmpty(updatedAtStr) ? null : DateTime.Parse(updatedAtStr, null, System.Globalization.DateTimeStyles.RoundtripKind));
    }

    private static IReadOnlyList<ResidentIdentityDocument> MapDocumentList(DataTable rows)
    {
        if (rows is null || rows.Rows.Count == 0) return Array.Empty<ResidentIdentityDocument>();
        var list = new List<ResidentIdentityDocument>(rows.Rows.Count);
        foreach (DataRow r in rows.Rows)
        {
            var updatedAtStr = r["UpdatedAtUtc"]?.ToString();
            list.Add(new ResidentIdentityDocument(
                id: Guid.Parse(r["Id"].ToString() ?? string.Empty),
                tenantId: Guid.Parse(r["TenantId"].ToString() ?? string.Empty),
                residentId: Guid.Parse(r["ResidentId"].ToString() ?? string.Empty),
                documentType: r["DocumentType"]?.ToString() ?? string.Empty,
                normalizedValue: r["NormalizedValue"]?.ToString() ?? string.Empty,
                active: Convert.ToBoolean(r["Active"]),
                createdAtUtc: Convert.ToDateTime(r["CreatedAtUtc"]),
                updatedAtUtc: string.IsNullOrEmpty(updatedAtStr) ? null : DateTime.Parse(updatedAtStr, null, System.Globalization.DateTimeStyles.RoundtripKind)));
        }
        return list;
    }
}
