using System;
using System.Collections.Generic;
using System.Text;

namespace Elite.Common.Utilities.ResponseHandling
{
    public class EncodedResponse<T>
    {
        public string Data { get; set; } // Base64 encoded data
        public bool IsEncoded { get; set; } = true;
    }
}