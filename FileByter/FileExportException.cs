using System;
using System.Runtime.Serialization;

namespace FileByter
{
	[Serializable]
	public class FileExportException : Exception
	{
		public FileExportException()
		{
		}

		public FileExportException(string message)
			: base(message)
		{
		}

		public FileExportException(string message, Exception inner)
			: base(message, inner)
		{
		}

		protected FileExportException(
			SerializationInfo info,
			StreamingContext context)
			: base(info, context)
		{
		}
	}
}