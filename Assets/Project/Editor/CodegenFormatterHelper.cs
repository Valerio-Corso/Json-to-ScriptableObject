namespace Project.Editor
{
	public static class CodegenFormatterHelper
	{
		public static string ToPascalCase(string str)
		{
			return char.ToUpper(str[0]) + str[1..];
		}
		
		public static string GetBraceParentheses(bool isOpen)
		{
			return isOpen ? "{" : "}";
		}
	}
}