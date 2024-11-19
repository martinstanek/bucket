using System.Text.Json.Serialization;
using Bucket.Service.Model;

namespace Bucket.Service.Serialization;

[JsonSerializable(typeof(BundleManifest), GenerationMode = JsonSourceGenerationMode.Default)]
internal partial class SourceGenerationContext : JsonSerializerContext;