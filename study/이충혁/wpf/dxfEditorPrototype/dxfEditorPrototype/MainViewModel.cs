using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;


namespace dxfEditorPrototype
{
    class MainViewModel : ObservableCollection<Message>
    {
        public MainViewModel()
        {
            this.Add(new Message() { Sender = "Adventure Works", Content = "Hi, what can we do for you?" });
            this.Add(new Message() { Sender = "Client", Content = "Did you receive the GR268 KZ bike?" });
            this.Add(new Message() { Sender = "Adventure Works", Content = "Not yet, but we have a similar model available." });
            this.Add(new Message() { Sender = "Client", Content = "What is it like?" });
            this.Add(new Message() { Sender = "Adventure Works", Content = "It boasts a carbon frame, hydraulic brakes and suspension, and a gear hub." });
            this.Add(new Message() { Sender = "Client", Content = "How much does it cost?" });
            this.Add(new Message() { Sender = "Adventure Works", Content = "Same as the GR268 KZ model you requested. You can get it from our online shop." });
            this.Add(new Message() { Sender = "Client", Content = "Thanks." });
            this.Add(new Message() { Sender = "Adventure Works", Content = "Thank you, have a nice ride." });
        }
    }

    public class Message
    {
        public string Sender { get; set; }
        public string Content { get; set; }
    }
}
