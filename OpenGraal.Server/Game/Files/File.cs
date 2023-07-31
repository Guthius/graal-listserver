namespace OpenGraal.Server.Game.Files;

public sealed record File(FileCategory Category, string Path, DateTimeOffset LastModified);