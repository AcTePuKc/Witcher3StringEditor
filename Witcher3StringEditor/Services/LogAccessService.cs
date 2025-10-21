using System.Collections.ObjectModel;
using Serilog.Events;
using Witcher3StringEditor.Common.Abstractions;

namespace Witcher3StringEditor.Services;

public class LogAccessService : ILogAccessService
{
    public ObservableCollection<LogEvent> Logs { get; } = [];
}