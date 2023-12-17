using System;
using System.Net;
using System.Threading.Tasks;

namespace tdic.InternetConnectionayer.Downloader
{
    public class AudioDownloadManager
    {
        public static async Task<bool> DownloadAudio(string address, string audioPath)
        {
            using (var webClient = new WebClient())
            {
                try
                {
                    await Task.Run(() =>
                    {
                        using (var webClient = new WebClient())
                        {
                            webClient.DownloadFile(address, audioPath);
                        }
                    });

                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }
    }
}
