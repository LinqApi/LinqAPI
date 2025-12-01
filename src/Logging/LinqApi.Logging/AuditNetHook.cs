using Audit.EntityFramework;
using LinqApi.Core;
using LinqApi.Core.Log;
using LinqApi.Core.Module;
using LinqApi.Correlation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;

namespace LinqApi.Logging;
public sealed class AuditNetHook : IAuditHook
{
    public Task OnSavingAsync (DbContext ctx, CancellationToken ct)
    {
        var outboxSet = ctx.Set<OutboxMessage>();
        var now = DateTime.UtcNow;
        var correlationIdGenerator = new DefaultCorrelationIdGenerator();
        var corr = CorrelationContext.GetNextCorrelationId(correlationIdGenerator);

        var entries = ctx.ChangeTracker.Entries()
            .Where(e =>
                e.Entity is not OutboxMessage &&                               // outbox'ı loglama
                e.Entity is not LinqLogEntity &&                               // kendi log tablolarını atla
                e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToList();

        foreach (var e in entries)
        {
            var entityType = e.Metadata.ClrType;
            if (typeof(LinqLogEntity).IsAssignableFrom(entityType))
                continue;

            var (op, payload, idStr) = BuildPayload(e);
            if (string.IsNullOrWhiteSpace(op))
                continue;

            var evtType = $"{entityType.Name}:{op}";
            var partition = idStr ?? entityType.Name;

            outboxSet.Add(new OutboxMessage
            {
                AggregateType = entityType.Name,
                AggregateId = idStr ?? string.Empty,
                Operation = op,                 // Created | Updated | Deleted
                EventType = evtType,
                DataJson = payload,            // snapshot/diff (maskelemesiz)
                OccurredAtUtc = now,
                PartitionKey = partition,
                Sequence = 0,                  // opsiyonel
                Status = OutboxStatus.Pending,
                NextAttemptAtUtc = now,
                CorrelationId = corr
            });
        }

        return Task.CompletedTask;
    }

    public Task OnSavedAsync (DbContext ctx, int rows, CancellationToken ct)
        => Task.CompletedTask;

    private static (string op, string payload, string? idStr) BuildPayload (EntityEntry e)
    {
        string op;
        string? idStr = null;

        // Tek kolon PK varsayımı; çoklu PK varsa pipe ile birleştir.
        var keyProps = e.Properties.Where(p => p.Metadata.IsPrimaryKey()).ToArray();
        if (keyProps.Length == 1)
        {
            idStr = (e.State == EntityState.Deleted ? keyProps[0].OriginalValue : keyProps[0].CurrentValue)?.ToString();
        }
        else if (keyProps.Length > 1)
        {
            var parts = keyProps.Select(p =>
                (e.State == EntityState.Deleted ? p.OriginalValue : p.CurrentValue)?.ToString() ?? "");
            idStr = string.Join("|", parts);
        }

        switch (e.State)
        {
            case EntityState.Added:
                op = "Created";
                var after = e.CurrentValues.Properties.ToDictionary(
                    p => p.Name,
                    p => e.CurrentValues[p]);
                return (op, JsonSerializer.Serialize(new { After = after }), idStr);

            case EntityState.Modified:
                op = "Updated";
                var diffs = e.Properties
                    .Where(p => p.IsModified || !Equals(p.OriginalValue, p.CurrentValue))
                    .Select(p => new
                    {
                        Field = p.Metadata.Name,
                        Old = e.OriginalValues[p.Metadata.Name],
                        New = e.CurrentValues[p.Metadata.Name]
                    })
                    .ToList();
                return (op, JsonSerializer.Serialize(new { Changes = diffs }), idStr);

            case EntityState.Deleted:
                op = "Deleted";
                var before = e.OriginalValues.Properties.ToDictionary(
                    p => p.Name,
                    p => e.OriginalValues[p]);
                return (op, JsonSerializer.Serialize(new { Before = before }), idStr);

            default:
                throw new NotSupportedException($"Unsupported state: {e.State}");
        }
    }
}

[AuditIgnore]
public class OutboxMessage : BaseEntity<long>
{
    // Kimliklendirme
    public string AggregateType { get; set; } = default!; // "Eventigg", "TicketListing"...
    public string AggregateId { get; set; } = default!; // "123" (stringleşmiş PK)
    public string Operation { get; set; } = default!; // "Created" | "Updated" | "Deleted"

    // Olay tipi (routing için kullanışlı)
    public string EventType { get; set; } = default!; // "Eventigg:Created" vb.

    // Veri
    public string DataJson { get; set; } = default!; // masked snapshot/diff
    public DateTime OccurredAtUtc { get; set; }

    // Sıralama / idempotency
    public string? PartitionKey { get; set; } // AggregateId veya tenant
    public long Sequence { get; set; } // monotonik Id yeterli ama istersen Aggregate-based sequence

    // Yayınlama durumu
    public OutboxStatus Status { get; set; } = OutboxStatus.Pending;
    public int AttemptCount { get; set; }
    public DateTime? NextAttemptAtUtc { get; set; }
    public DateTime? PublishedAtUtc { get; set; }
    public string? LastError { get; set; }

    // İzlek
    public string? CorrelationId { get; set; }
}

public enum OutboxStatus { Pending = 0, Processing = 1, Succeeded = 2, Failed = 3, DeadLetter = 4 }