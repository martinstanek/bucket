using System;

namespace Bucket.Service.Exceptions;

public sealed class BucketException(string message) : Exception(message) { }