using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace UassetReader.WpfUtil
{
    public class MultiOutTextWriter : TextWriter
    {
        private IEnumerable<TextWriter> writers;


        public MultiOutTextWriter(IEnumerable<TextWriter> writers)
        {
            this.writers = writers.ToList();
        }

        public MultiOutTextWriter(params TextWriter[] writers)
        {
            this.writers = writers;
        }

        public override void Write(char value)
        {
            foreach(var writer in writers)
                writer.Write(value);
        }

        public override void Write(string value)
        {
            foreach (var writer in writers)
                writer.Write(value);
        }

        public override void WriteLine(string value)
        {
            foreach (var writer in writers)
                writer.WriteLine(value);
        }

        public override void Flush()
        {
            foreach (var writer in writers)
                writer.Flush();
        }

        public override void Close()
        {
            foreach (var writer in writers)
                writer.Close();
        }

        public override Encoding Encoding => Encoding.ASCII;
    }
}
