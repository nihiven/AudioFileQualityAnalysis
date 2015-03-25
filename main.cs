using System;
using System.Linq;
using System.IO;
using NAudio.Wave;
using System.Text;

class Program
{
    static void Main()
    {
		string path = "c:\\music\\flac\\";
    	path = "C:\\google\\Apps\\musicanalysis\\AudioFileQualityAnalysis\\music\\";
        path = "E:\\google\\Google Drive\\Apps\\musicanalysis\\AudioFileQualityAnalysis\\";
		
		if(File.Exists(path)) 
			ProcessFile(path); // This path is a file
		else if(Directory.Exists(path)) 
			ProcessDirectory(path); // This path is a directory
		else 
            Console.WriteLine("{0} is not a valid file or directory.", path);
   
    }
	
    // process given directory and all subdirectories
    public static void ProcessDirectory(string targetDirectory) 
    {
        // process all files found in given directory
        string [] fileEntries = Directory.GetFiles(targetDirectory);
        foreach(string fileName in fileEntries)
            ProcessFile(fileName);

        // recurse into subdirectories of this directory
        string [] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
        foreach(string subdirectory in subdirectoryEntries)
            ProcessDirectory(subdirectory);
    }

    // process individual files
    public static void ProcessFile(string path) 
    {
		// reader
		BinaryReader b;
        string fileName = Path.GetFileName(path);
	
		// define file header matching data
		byte[] flac = new byte[4] { 0x66, 0x4c, 0x61, 0x43 };
		byte[] wav = new byte[4] { 0x52, 0x49, 0x46, 0x46 };
		byte[] mpx = new byte[4] { 0x49, 0x44, 0x33, 0x03 };
	
        try
		{
			 b = new BinaryReader(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read));
		}
		catch (System.IO.IOException)
		{
			Console.WriteLine("some read error, oh god: {0}", fileName);
			return;
		}

		// read header
		byte[] d = new byte[4];
		int v = b.Read(d,0,4);
			
		if (v > 0)
		{
			if (d.SequenceEqual(flac)) 
			{
                Console.WriteLine("Detected FLAC file: {0}", fileName);
			}
			else if (d.SequenceEqual(wav))
			{
                Console.WriteLine("Detected WAV file: {0}", fileName);
			}
			else if (d.SequenceEqual(mpx))
			{
    			// what kind of mpx??
                try
                {
                    // Mp3
                    Console.WriteLine(DescribeMp3(path));
                }
                catch
                {
                    Console.WriteLine("MpX, unknown type: {0}", fileName);
                    return;
                }
			}
			else
			{
              //  Console.WriteLine("Unknown File {0}", Path.GetFileName(path));				
			}
		}
    }

    public static string DescribeMp3(string fileName)
    {
        StringBuilder stringBuilder = new StringBuilder();
        using (Mp3FileReader reader = new Mp3FileReader(fileName))
        {
            Mp3WaveFormat wf = reader.Mp3WaveFormat;
         /*
            stringBuilder.AppendFormat("MP3 File WaveFormat: {0} {1}Hz {2} channels {3} bits per sample\r\n",
                wf.Encoding, wf.SampleRate,
                wf.Channels, wf.BitsPerSample);
            stringBuilder.AppendFormat("Extra Size: {0} Block Align: {1} Average Bytes Per Second: {2}\r\n",
                wf.ExtraSize, wf.BlockAlign,
                wf.AverageBytesPerSecond);
            stringBuilder.AppendFormat("ID: {0} Flags: {1} Block Size: {2} Frames per Block: {3}\r\n",
                wf.id, wf.flags, wf.blockSize, wf.framesPerBlock
                );

            stringBuilder.AppendFormat("Bytes: {0} Time: {1} \r\n", reader.Length, reader.TotalTime);
            stringBuilder.AppendFormat("ID3v1 Tag: {0}\r\n", reader.Id3v1Tag == null ? "None" : reader.Id3v1Tag.ToString());
            stringBuilder.AppendFormat("ID3v2 Tag: {0}\r\n", reader.Id3v2Tag == null ? "None" : reader.Id3v2Tag.ToString());
            */

            Mp3Frame frame;
            int kbps = 0;
            int frames = 0;
            while ((frame = reader.ReadNextFrame()) != null)
            {
                /*stringBuilder.AppendFormat("{0},{1},{2}Hz,{3},{4}bps, length {5}\r\n",
                    frame.MpegVersion, frame.MpegLayer,
                    frame.SampleRate, frame.ChannelMode,
                    frame.BitRate, frame.FrameLength);
                 */
                kbps += frame.BitRate / 1000;
                frames++;
            }
            stringBuilder.AppendFormat("Mp3 Avg Kbps: {0} ({1})", kbps / frames, Path.GetFileName(fileName));
        }
        return stringBuilder.ToString();
    }
}