using System.Windows;
using System.Windows.Documents;

namespace QuanLyLinhKienMayTinh.Views
{
    public partial class BaoCaoPreviewWindow : Window
    {
        public BaoCaoPreviewWindow(FlowDocument document)
        {
            InitializeComponent();
            ReportViewer.Document = document;
        }
    }
}
