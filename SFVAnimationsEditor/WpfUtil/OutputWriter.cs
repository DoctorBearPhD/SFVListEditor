using System.IO;
using System.Text;
using System.Windows.Controls;

namespace SFVAnimationsEditor.WpfUtil
{
    // TODO NEXT: Change how we write to the output.
    //   Have the TextWriter update a string, and have the TextBox be bound to that string.
    //   See if that fixes it.
    public class OutputWriter : TextWriter
    {
        private TextBox textbox;
        ////private StringBuilder sb;
        ////private SynchronizationContext Synchronization = SynchronizationContext.Current;

        
        public OutputWriter(TextBox textbox)
        {
            this.textbox = textbox;
            ////sb = new StringBuilder();
        }

        public override void Write(char value)
        {
            App.Current.Dispatcher.BeginInvoke(new System.Action(
                () => {
                    textbox.Text += value;
                    ////if (value == '\n') textbox.ScrollToEnd();
                }
            ));
        }

        public override void Write(string value)
        {
            App.Current.Dispatcher.BeginInvoke(new System.Action(
                () => {
                    textbox.AppendText(value);
                    //textbox.ScrollToEnd();
                }
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
                    ////textbox.ScrollToEnd();
                }
            ));

            ////Synchronization.Post(
            ////    o => {
            ////        textbox.AppendText(value + "\n");
            ////        textbox.ScrollToEnd();
            ////    },
            ////    null);
        }

        public override Encoding Encoding => Encoding.ASCII;
    }

}
