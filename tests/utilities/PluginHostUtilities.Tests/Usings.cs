global using Xunit;
using System.Collections;

public abstract class TheoryData : IEnumerable<object[]>  
{
	readonly List<object[]> data = new();

	protected void AddRow(params object[] values)
	{
		data.Add(values);
	}

	public IEnumerator<object[]> GetEnumerator()
	{
		return data.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}