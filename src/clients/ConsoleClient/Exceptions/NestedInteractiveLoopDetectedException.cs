namespace VespionSoftworks.Athenaeum.Clients.ConsoleClient.Exceptions;

public interface ISupplyExitCode
{
	public int ExitCode { get; }
}

public class NestedInteractiveLoopDetectedException: InvalidOperationException, ISupplyExitCode
{
	/// <inheritdoc />
	public int ExitCode => 1;
}