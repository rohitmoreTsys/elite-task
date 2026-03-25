using Elite.AntiVirus.Lib;
using nClam;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Elite.AntiVirus.Services
{
    public class ClamAV
    {
        
        public async Task<ScanResult> ScanFileForViruses(byte[] fileBytes,string host, int port, long maxStreamSize)
        {

            ScanResult result = new ScanResult();
            try
            {
                var clam = new ClamClient(host,port);
                clam.MaxStreamSize = maxStreamSize;
                var scanResult = await clam.SendAndScanFileAsync(fileBytes);
                switch (scanResult.Result)
                {
                    //value false determines no virus detected.
                    case ClamScanResults.Clean:
                        result.value = false;
                        result.message=scanResult.RawResult;
                        break;
                    case ClamScanResults.VirusDetected:
                        result.value = true;
                        result.message = scanResult.InfectedFiles.FirstOrDefault().VirusName;
                        break;
                    case ClamScanResults.Error:
                        result.value = true;
                        result.message = scanResult.RawResult;
                        break;
                    case ClamScanResults.Unknown:
                        result.value = true;
                        result.message = scanResult.RawResult;
                        break;
                }
            }
            catch (Exception ex)
            {
                result.value = true;
                result.message = ex.Message.ToString();
                return result;
            }

            return result;
        }
    }

}
