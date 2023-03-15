using System.Globalization;
using System.Xml.Linq;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Packaging;
using NuGet.Packaging.PackageExtraction;
using NuGet.Packaging.Signing;
using NuGet.ProjectManagement;
using ExecutionContext = NuGet.ProjectManagement.ExecutionContext;

namespace VespionSoftworks.Athenaeum.Utilities.PluginHostUtilities;

public class ProjectContext : INuGetProjectContext
{
	private readonly ILogger _logger;

	public ProjectContext(ILogger logger)
	{
		_logger = logger;
		var clientPolicyContext = ClientPolicyContext.GetClientPolicy(new NullSettings(), _logger);
		PackageExtractionContext = new PackageExtractionContext(
			PackageSaveMode.Defaultv3,
			PackageExtractionBehavior.XmlDocFileSaveMode,
			clientPolicyContext,
			_logger
		);
		OperationId = Guid.NewGuid();
	}

	public ExecutionContext ExecutionContext => null;
	public PackageExtractionContext PackageExtractionContext { get; set; }

	public XDocument OriginalPackagesConfig { get; set; } = null;

	public ISourceControlManagerProvider SourceControlManagerProvider => null;

	public void Log(MessageLevel level, string message, params object[] args)
	{
		if (args.Length > 0)
		{
			message = string.Format(CultureInfo.CurrentCulture, message, args);
		}

		switch (level)
		{
			case MessageLevel.Debug:
				_logger.LogDebug(message);
				break;

			case MessageLevel.Info:
				_logger.LogMinimal(message);
				break;

			case MessageLevel.Warning:
				_logger.LogWarning(message);
				break;

			case MessageLevel.Error:
				_logger.LogError(message);
				break;
		}
	}

	public void Log(ILogMessage message)
	{
		_logger.Log(message);
	}

	public void ReportError(string message)
	{
		_logger.LogError(message);
	}

	public void ReportError(ILogMessage message)
	{
		_logger.Log(message);
	}

	public virtual FileConflictAction ResolveFileConflict(string message)
	{
		return FileConflictAction.IgnoreAll;
	}

	public NuGetActionType ActionType { get; set; }

	public Guid OperationId { get; set; }
}