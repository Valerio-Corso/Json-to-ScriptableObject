namespace Project.Editor
{
	public static class CodegenStringHelper
	{
		public static string ToPascalCase(string str)
		{
			return char.ToUpper(str[0]) + str[1..];
		}
	}
}