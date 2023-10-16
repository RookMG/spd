using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Text;

class Program
{
    static void Main(string[] args)
    {
        ProcessMultipleWriteAsync().Wait();
    }

    static async Task ProcessMultipleWriteAsync()
    {
        IList<FileStream> sourceStreams = new List<FileStream>();

        try
        {
            string folder = Directory.CreateDirectory("tempfolder").Name;
            IList<Task> writeTaskList = new List<Task>();

            for(int index = 1; index <= 10; ++index)
            {
                string fileName = $"file-{index:00}.txt";
                string filePath = $"{folder}/{fileName}";
                string text = $"In file {index}{Environment.NewLine}";
                byte[] encodedText = Encoding.Unicode.GetBytes(text);

                var sourceStream = new FileStream(
                    filePath,
                    FileMode.Create, FileAccess.Write, FileShare.None,
                    bufferSize: 4096, useAsync: true);

                Task writeTask = sourceStream.WriteAsync(encodedText, 0, encodedText.Length);
                sourceStreams.Add(sourceStream);
                writeTaskList.Add(writeTask);
            }
            await Task.WhenAll(writeTaskList);
        }
        finally
        {
            foreach(var sourceStream in sourceStreams)
            {
                sourceStream.Close();
            }
        }
    }
}

