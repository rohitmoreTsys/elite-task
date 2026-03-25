using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Elite.Common.Utilities.FileUpload
{
  public  class FileHelpers
    {
        public async static Task<byte[]> ConvertFileToBytes(AttachFile file, Action action=null)
        {
            using (var reader =
                       new StreamReader(file.FilePath, detectEncodingFromByteOrderMarks: true))
            {             

                var bytes = default(byte[]);
                using (var memstream = new MemoryStream())
                {
                    await reader.BaseStream.CopyToAsync(memstream);
                    bytes = memstream.ToArray();
                }
                file.FileSize = bytes.Length;

                if (action != null)
                    action();

                return await Task.FromResult(bytes);
            }
        }
        public async static Task<byte[]> ConvertFileToBytes(AttachFileAV file, Action action = null)
        {
            using (var reader =
                       new StreamReader(file.FilePath, detectEncodingFromByteOrderMarks: true))
            {

                var bytes = default(byte[]);
                using (var memstream = new MemoryStream())
                {
                    await reader.BaseStream.CopyToAsync(memstream);
                    bytes = memstream.ToArray();
                }
                file.FileSize = bytes.Length;

                if (action != null)
                    action();

                return await Task.FromResult(bytes);
            }
        }

        public async static Task<byte[]> ConvertFileToBytes(string filePath)
        {
            using (var reader =
                       new StreamReader(filePath, detectEncodingFromByteOrderMarks: true))
            {

                var bytes = default(byte[]);
                using (var memstream = new MemoryStream())
                {
                    await reader.BaseStream.CopyToAsync(memstream);
                    bytes = memstream.ToArray();
                }

                return await Task.FromResult(bytes);
            }
        }

    }
}
