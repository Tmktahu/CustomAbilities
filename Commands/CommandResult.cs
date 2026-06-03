namespace CustomAbilities.Commands;

#nullable enable

public record struct CommandResult(
  bool Success,
  string Message,
  object? Data = null  // Add when needed
);
