using System.Buffers;

namespace AD.Api.Buffers;

public static class CharCollections
{
	/// <summary>
	/// A <see cref="SearchValues{T}"/> instance containing all numeric characters.
	/// </summary>
	/// <remarks>
	/// Character range: <c>0-9</c>.
	/// </remarks>
	public static readonly SearchValues<char> Numbers;
	/// <summary>
	/// A <see cref="SearchValues{T}"/> instance containing all whitespace characters.
	/// </summary>
	public static readonly SearchValues<char> Whitespace;

	static CharCollections()
	{
		Numbers = CharRange.CreateSearchValues('0', '9');
		Whitespace = SearchValues.Create(
			'\t',       // horizontal tab
			'\n',       // line feed
			(char)11,   // vertical tab
			(char)12,   // form feed
			'\r',       // carriage return
			' ',        // space
			(char)133,  // next line
			(char)160,  // non-breaking space
			(char)5760, // ogham space mark
			(char)8203, // zero width space
			(char)8192, // en quad
			(char)8193, // em quad
			(char)8194, // en space
			(char)8195, // em space
			(char)8196, // three-per-em space
			(char)8197, // four-per-em space
			(char)8198, // six-per-em space
			(char)8199, // figure space
			(char)8200, // punctuation space
			(char)8201, // thin space
			(char)8202, // hair space
			(char)8232, // line separator
			(char)8233, // paragraph separator
			(char)8239, // narrow no-break space
			(char)8287, // medium mathematical space
			(char)12288,// fullwidth space
			(char)65279 // zero width no-break space
		);
	}
}