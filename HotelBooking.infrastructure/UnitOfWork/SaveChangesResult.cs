// FR-ONB-019: expose persistence conflicts without leaking EF exception types to application services.
public sealed record SaveChangesResult(int AffectedRows, bool HasConcurrencyConflict);
