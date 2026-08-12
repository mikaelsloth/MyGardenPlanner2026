namespace MyGardenPlanner2026.Infrastructure.Data;

using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Text.Json;

public partial class PlannerDbContext
{
    private static readonly ValueComparer<Dictionary<string, string>> FeatureLimitsComparer =
        new(
            (left, right) => (left ?? new()).SequenceEqual(right ?? new()),
                dict => dict.Aggregate(0, (hash, kv) => HashCode.Combine(hash, kv.Key, kv.Value)),
                dict => new Dictionary<string, string>(dict)
        );

    private static readonly ValueConverter<Dictionary<string, string>, string> JsonContentConverter =
        new(
            v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
            v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions?)null) ?? new()
        );
}