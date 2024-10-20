﻿using System.Text.Json.Serialization;
using Kompozer.Service.Model;

namespace Kompozer.Service.Serialization;

[JsonSerializable(typeof(BundleManifest), GenerationMode = JsonSourceGenerationMode.Default)]
internal partial class SourceGenerationContext : JsonSerializerContext;