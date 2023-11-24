using System.IO;

namespace SnaPDataTransfer
{
    public class ReadFileToStream
    {
        private readonly string _fn;
        private readonly StreamString _ss;

        public ReadFileToStream(StreamString str, string filename)
        {
            _fn = filename;
            _ss = str;
        }

        public async void Start()
        {
            string contents = await File.ReadAllTextAsync(_fn);
            await _ss.WriteStringAsync(contents);
        }
    }
}