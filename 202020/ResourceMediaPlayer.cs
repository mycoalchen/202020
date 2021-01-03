using System;
using System.IO;
using System.Windows.Media;

namespace _202020
{
    class ResourceMediaPlayer : MediaPlayer, IDisposable
    {
        protected internal string TempDirectory;

        public ResourceMediaPlayer() {}

        public void Dispose()
        {
            if (TempDirectory != null)
            {
                try
                {
                    Close();
                    //Delete all files first
                    foreach (var __f in Directory.GetFiles(TempDirectory))
                        File.Delete(__f);
                    Directory.Delete(TempDirectory, true);
                }
                catch (Exception) { }
            }
        }

        protected internal virtual void PlayResourceFile(Uri uri)
        {
            Uri _uri = WriteResourceFileToTemp(uri);
            Open(_uri);
            Play();
        }
        protected internal Uri WriteResourceFileToTemp(Uri uri)
        {
            try
            {
                #region Create a new temp folder if there isn't one for this session of the application
                if (string.IsNullOrEmpty(TempDirectory))
                {
                    TempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                    Directory.CreateDirectory(TempDirectory);
                }
                #endregion

                #region Write the ressource file to the file system if it isn't there already
                var __file = Path.Combine(TempDirectory, Path.GetFileName(uri.ToString()));
                if (!File.Exists(__file))
                {
                    var __stream = System.Windows.Application.GetResourceStream(uri).Stream;
                    var __fileStream = File.Create(__file);
                    __stream.CopyTo(__fileStream);
                    __fileStream.Close();
                }
                #endregion

                return new Uri(__file, UriKind.Absolute);
            }
            catch (Exception) { return null; }
        }

        private void SoundPlayer_MediaEnded(object sender, EventArgs e) {}
    }
}
