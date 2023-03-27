using System.Collections.Generic;
// ReSharper disable CheckNamespace

public readonly record struct ConventionalCommit(string Sha, string Type, string Scope, bool IsBreaking, string Subject, string Body, IDictionary<string, string> Footers);