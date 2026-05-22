using Aimbys.Domain.Entities;

namespace Aimbys.Application.Storage;

/// <summary>
/// Returned by <see cref="IFileStorageService.OpenReadAsync"/>. The
/// <see cref="Stream"/> is owned by the caller and MUST be disposed (the
/// controller normally does this by passing it to a <c>FileStreamResult</c>).
/// </summary>
public sealed record FileOpenResult(Stream Content, FileAsset Asset);
