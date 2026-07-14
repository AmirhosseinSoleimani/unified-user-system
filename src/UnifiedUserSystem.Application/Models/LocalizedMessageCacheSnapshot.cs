
using UnifiedUserSystem.src.Domain.Common;

namespace UnifiedUserSystem.src.Application.Models;

public sealed record LocalizedMessageCacheSnapshot(
    string Version,
    IReadOnlyDictionary<string, LocalizedMessage> Messages);
