// See https://aka.ms/new-console-template for more information
using System.Text;

var arguments = Environment.GetCommandLineArgs();
if (arguments == null || arguments.Length < 3)
{
    return;
}
var sourceName = arguments[1];
var destName = arguments[2];
var size = arguments[3];
int maxSplitSize = 10;
int.TryParse(size, out maxSplitSize);


Console.WriteLine("Splitting mbox file into smaller files of {0} MB", maxSplitSize);

static int indexOf(byte[] array, byte[] value)
{
    int found = value.Length - 1;
    for (int i = array.Length - 1; i >= 0; i--)
    {
        if (array[i] == value[found])
        {
            if (--found == 0)
            {
                return i - found + 1;
            }
        }
        else
        {
            found = value.Length - 1;
        }
    }
    return -1;
}


try
{
    using (FileStream fsSource = new FileStream(sourceName, FileMode.Open, FileAccess.Read))
    {
        byte[] searchText = Encoding.UTF8.GetBytes("\n\nFrom ");
        var totalLength = fsSource.Length;
        // Read the source file into a byte array.
        int maxLength = maxSplitSize * 1024 * 1024;
        byte[] bytes = new byte[maxLength];
        int numBytesToRead = maxLength;
        int numBytesRead = 0;
        int fileCounter = 1;
        var dir = Path.GetDirectoryName(destName) ?? "";
        Directory.CreateDirectory(dir);
        while (totalLength > 0)
        {
            // Read may return anything from 0 to numBytesToRead.
            int n = fsSource.Read(bytes, numBytesRead, numBytesToRead);

            // Break when the end of the file is reached.
            if (n == 0)
                break;

            numBytesRead = numBytesToRead - n;
            totalLength -= n;
            if (numBytesRead > 0)
                numBytesToRead -= n;
            else
                numBytesToRead = maxLength;

            int v = indexOf(bytes, searchText);
            fsSource.Seek(v - numBytesToRead, SeekOrigin.Current);

            var pathNew = string.Format(destName, fileCounter++);
            // Write the byte array to the other FileStream.
            using (FileStream fsNew = new FileStream(pathNew, FileMode.Create, FileAccess.Write))
            {
                fsNew.Write(bytes, 0, v);
            }
            Console.Write(".");
        }

    }
}
catch (FileNotFoundException ioEx)
{
    Console.WriteLine(ioEx.Message);
}
Console.WriteLine("");
Console.WriteLine("Done!");
