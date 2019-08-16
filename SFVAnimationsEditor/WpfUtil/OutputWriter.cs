using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Controls;

namespace SFVAnimationsEditor.WpfUtil
{
    public class OutputWriter : TextWriter
    {
        private TextBox textbox;
        //private StringBuilder sb;
        //private SynchronizationContext Synchronization = SynchronizationContext.Current;


        public OutputWriter(TextBox textbox)
        {
            this.textbox = textbox;
            ////sb = new StringBuilder();
        }

        public override void Write(char value)
        {
            ////sb.Append(value);
            ////if (value == '\n')
            ////{
            ////    Write(sb.ToString());
            ////    sb.Clear();
            ////}
            ////return;
            App.Current.Dispatcher.BeginInvoke(new System.Action(
                () => {
                    textbox.Text += value;
                    //if (value == '\n') textbox.ScrollToEnd();
                }
            ));
        }

        public override void Write(string value)
        {
            App.Current.Dispatcher.BeginInvoke(new System.Action(
                () => { textbox.AppendText(value); textbox.ScrollToEnd(); }
            ));

            ////Synchronization.Post(
            ////    o => {
            ////        textbox.AppendText(value);
            ////        textbox.ScrollToEnd();
            ////    }, 
            ////    null);
        }

        public override void WriteLine(string value)
        {
            App.Current.Dispatcher.BeginInvoke(new System.Action(
                () =>
                {
                    textbox.AppendText(value + "\n");
                    textbox.ScrollToEnd();
                }
            ));

            ////Synchronization.Post(
            ////    o => {
            ////        textbox.AppendText(value + "\n");
            ////        textbox.ScrollToEnd();
            ////    },
            ////    null);
        }

        public override Encoding Encoding
        {
            get { return Encoding.ASCII; }
        }
    }

}
