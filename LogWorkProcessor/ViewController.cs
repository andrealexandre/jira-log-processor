using System;
using System.Threading.Tasks;
using AppKit;
using Foundation;

namespace LogWorkProcessor
{
    public partial class ViewController : NSViewController
    {
        private int count = 0;

        public ViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Do any additional setup after loading the view.
        }

        public override NSObject RepresentedObject
        {
            get
            {
                return base.RepresentedObject;
            }
            set
            {
                base.RepresentedObject = value;
                // Update the view, if already loaded.
            }
        }

        partial void Button_Action(NSObject sender)
        {
            ++count;
            Console.WriteLine($"Button Pressed {count} times");
            Label.StringValue = "";

            var openPanel = new NSOpenPanel();
            openPanel.CanChooseFiles = true;
            openPanel.CanChooseDirectories = false;
            openPanel.CanCreateDirectories = false;
            openPanel.RunModal(new string[] { "csv" });

            //busy work to strip url
            var url = openPanel.Url;
            var fileUrlEncoded = url.FilePathUrl.AbsoluteString.Replace("file://", "");
            var fileUrl = Uri.UnescapeDataString(fileUrlEncoded);

            //UI updates
            Console.WriteLine($"Selected file {fileUrl}");
            Label.StringValue += $"Selected file {fileUrl}";

            //Call csv processor (this should be delegated to worker thread)
            Task.Factory
                .StartNew(() => new CsvProcessor().Process(fileUrl))
                .ContinueWith((resultFile) => 
                {
                    Console.WriteLine($"Total log work sheet by user created in file {resultFile.Result}");

                    //Update UI
                    InvokeOnMainThread(() =>
                    {
                        Label.StringValue += $"\n\nTotal log work sheet by user created in file {resultFile.Result}";
                    });
                });
        }

    }
}
