using Microsoft.EntityFrameworkCore;
using QuanLyLinhKienMayTinh.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

namespace QuanLyLinhKienMayTinh.ViewModels
{
    // Class phụ trợ để hiển thị danh sách kịch bản trong ComboBox
    public class KichBanModel
    {
        public int Id { get; set; }
        public string TenKichBan { get; set; }
        public string MoTa { get; set; }
    }

    public class StringContainsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string text && parameter is string keyword)
                return text.Contains(keyword, StringComparison.OrdinalIgnoreCase);
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class BaoMatViewModel : BaseViewModel
    {
        // =============================================
        // PROPERTIES
        // =============================================

        private ObservableCollection<LinhKien> _dataUserA;
        public ObservableCollection<LinhKien> DataUserA
        {
            get => _dataUserA;
            set { _dataUserA = value; OnPropertyChanged(nameof(DataUserA)); }
        }

        private ObservableCollection<LinhKien> _dataUserB;
        public ObservableCollection<LinhKien> DataUserB
        {
            get => _dataUserB;
            set { _dataUserB = value; OnPropertyChanged(nameof(DataUserB)); }
        }

        private ObservableCollection<string> _logs = new ObservableCollection<string>();
        public ObservableCollection<string> Logs
        {
            get => _logs;
            set { _logs = value; OnPropertyChanged(nameof(Logs)); }
        }

        private int _selectedScenarioIndex;
        public int SelectedScenarioIndex
        {
            get => _selectedScenarioIndex;
            set
            {
                _selectedScenarioIndex = value;
                OnPropertyChanged(nameof(SelectedScenarioIndex));
                _ = LoadDataAsync(); // Reload từ DB thực tế mỗi khi đổi kịch bản
            }
        }

        private ObservableCollection<KichBanModel> _danhSachKichBan;
        public ObservableCollection<KichBanModel> DanhSachKichBan
        {
            get => _danhSachKichBan;
            set { _danhSachKichBan = value; OnPropertyChanged(); }
        }

        private KichBanModel _kichBanChon;
        public KichBanModel KichBanChon
        {
            get => _kichBanChon;
            set
            {
                _kichBanChon = value;
                OnPropertyChanged();
                if (_kichBanChon != null)
                {
                    _selectedScenarioIndex = _kichBanChon.Id;
                    OnPropertyChanged(nameof(SelectedScenarioIndex));
                    _ = LoadDataAsync(); // Reload từ DB thực tế mỗi khi đổi kịch bản
                }
            }
        }

        private bool _isFixMode;
        public bool IsFixMode
        {
            get => _isFixMode;
            set { _isFixMode = value; OnPropertyChanged(nameof(IsFixMode)); }
        }

        private bool _isUserABusy;
        public bool IsUserABusy
        {
            get => _isUserABusy;
            set
            {
                _isUserABusy = value;
                OnPropertyChanged(nameof(IsUserABusy));
                OnPropertyChanged(nameof(IsUserAIdle));
            }
        }

        private bool _isUserBBusy;
        public bool IsUserBBusy
        {
            get => _isUserBBusy;
            set
            {
                _isUserBBusy = value;
                OnPropertyChanged(nameof(IsUserBBusy));
                OnPropertyChanged(nameof(IsUserBIdle));
            }
        }

        public bool IsUserAIdle => !_isUserABusy;
        public bool IsUserBIdle => !_isUserBBusy;

        // =============================================
        // COMMANDS
        // =============================================

        public ICommand RunUserACommand { get; }
        public ICommand RunUserBCommand { get; }
        public ICommand ResetDataCommand { get; }

        // =============================================
        // CONSTRUCTOR
        // =============================================

        public BaoMatViewModel()
        {
            RunUserACommand = new RelayCommand<object>(p => IsUserAIdle, p => RunUserAAsync());
            RunUserBCommand = new RelayCommand<object>(p => IsUserBIdle, p => RunUserBAsync());
            ResetDataCommand = new RelayCommand<object>(p => true, p => ResetDataAsync());

            KhoiTaoDanhSachKichBan();
            _ = LoadDataAsync();
        }

        // =============================================
        // LOGIC HỖ TRỢ
        // =============================================

        private QL_LinhKien_PC_Context CreateNewContext()
        {
            string connString = DataProvider.Ins.GetContext().Database.GetConnectionString();
            var optionsBuilder = new DbContextOptionsBuilder<QL_LinhKien_PC_Context>();
            optionsBuilder.UseSqlServer(connString);
            return new QL_LinhKien_PC_Context(optionsBuilder.Options);
        }

        private void WriteLog(string message, string user)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                string time = DateTime.Now.ToString("HH:mm:ss");
                Logs.Insert(0, $"[{time}] [{user}] - {message}");
            });
        }

        private void KhoiTaoDanhSachKichBan()
        {
            DanhSachKichBan = new ObservableCollection<KichBanModel>
            {
                new KichBanModel { Id = 0, TenKichBan = "Kịch bản 1: Mất dữ liệu cập nhật (Lost Update)", MoTa = "Tác động: MOU001" },
                new KichBanModel { Id = 1, TenKichBan = "Kịch bản 2: Đọc dữ liệu rác (Dirty Read)", MoTa = "Tác động: MOU001" },
                new KichBanModel { Id = 2, TenKichBan = "Kịch bản 3: Không thể đọc lại (Unrepeatable Read)", MoTa = "Tác động: MOU001" },
                new KichBanModel { Id = 3, TenKichBan = "Kịch bản 4: Bóng ma dữ liệu (Phantom Read)", MoTa = "Tác động: Nhóm loại MOU" },
                new KichBanModel { Id = 4, TenKichBan = "Kịch bản 5: Tắc nghẽn giao dịch (Deadlock)", MoTa = "Tác động: MOU001 & MOU002" }
            };
            KichBanChon = DanhSachKichBan[0];
        }

        /// <summary>
        /// Load dữ liệu mới nhất từ DB và lọc theo kịch bản đang chọn.
        /// Gọi lúc khởi động, sau khi đổi kịch bản, sau khi reset.
        /// </summary>
        private async Task LoadDataAsync()
        {
            try
            {
                using var context = CreateNewContext();
                var allData = await context.LinhKiens.AsNoTracking().ToListAsync();

                List<LinhKien> filtered;
                switch (SelectedScenarioIndex)
                {
                    case 0:
                    case 1:
                    case 2:
                        filtered = allData.Where(x => x.MaLk == "MOU001").ToList();
                        break;
                    case 3:
                        filtered = allData.Where(x => x.MaLoai == "MOU").ToList();
                        break;
                    case 4:
                        filtered = allData.Where(x => x.MaLk == "MOU001" || x.MaLk == "MOU002").ToList();
                        break;
                    default:
                        filtered = allData;
                        break;
                }

                App.Current.Dispatcher.Invoke(() =>
                {
                    DataUserA = new ObservableCollection<LinhKien>(filtered);
                    DataUserB = new ObservableCollection<LinhKien>(filtered);
                });
            }
            catch (Exception ex)
            {
                WriteLog($"Lỗi tải dữ liệu: {ex.Message}", "HỆ THỐNG");
            }
        }

        // =============================================
        // THỰC THI GIAO TÁC & RESET
        // =============================================

        private async Task ResetDataAsync()
        {
            try
            {
                using var context = CreateNewContext();
                var lk001 = await context.LinhKiens.FirstOrDefaultAsync(x => x.MaLk == "MOU001");
                if (lk001 != null) { lk001.SoLuongTon = 50; lk001.DonGiaBan = 150000; }

                var lk002 = await context.LinhKiens.FirstOrDefaultAsync(x => x.MaLk == "MOU002");
                if (lk002 != null) { lk002.DonGiaBan = 200000; }

                await context.Database.ExecuteSqlRawAsync("DELETE FROM LinhKien WHERE MaLk = 'MOU099'");
                await context.SaveChangesAsync();

                WriteLog("Đã khôi phục dữ liệu hệ thống về trạng thái ban đầu.", "HỆ THỐNG");
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                WriteLog($"Lỗi reset: {ex.Message}", "HỆ THỐNG");
            }
        }

        private async Task RunUserAAsync()
        {
            IsUserABusy = true;
            WriteLog("-----------------------------------", "HỆ THỐNG");
            try
            {
                using var context = CreateNewContext();
                WriteLog($"Bắt đầu {KichBanChon.TenKichBan}...", "USER A");

                switch (SelectedScenarioIndex)
                {
                    case 0: // Lost Update
                        await context.Procedures.sp_kichban1_giaotacaAsync(IsFixMode);
                        WriteLog(IsFixMode ? "A dùng UPDLOCK, chờ B xong mới ghi"
                                           : "A ghi đè lên B (mất cập nhật của B)", "USER A");
                        var lk0 = await context.LinhKiens.AsNoTracking()
                                               .FirstOrDefaultAsync(x => x.MaLk == "MOU001");
                        App.Current.Dispatcher.Invoke(() =>
                            DataUserA = new ObservableCollection<LinhKien>(new[] { lk0 }));
                        break;

                    case 1: // Dirty Read - A ghi rác rồi rollback
                        await context.Procedures.sp_kichban2_giaotacaAsync();
                        WriteLog("A đã ghi SoLuongTon = 1000 rồi rollback", "USER A");
                        var lk1 = await context.LinhKiens.AsNoTracking()
                                               .FirstOrDefaultAsync(x => x.MaLk == "MOU001");
                        App.Current.Dispatcher.Invoke(() =>
                            DataUserA = new ObservableCollection<LinhKien>(new[] { lk1 }));
                        break;

                    case 2: // Unrepeatable Read - A đọc 2 lần
                        WriteLog($"Bắt đầu {KichBanChon.TenKichBan}...", "USER A");
                        using (var tran = await context.Database.BeginTransactionAsync(
                            IsFixMode ? System.Data.IsolationLevel.RepeatableRead
                                      : System.Data.IsolationLevel.ReadCommitted))
                        {
                            // Đọc lần 1
                            var lan1 = await context.LinhKiens.AsNoTracking()
                                                    .FirstOrDefaultAsync(x => x.MaLk == "MOU001");
                            WriteLog($"A đọc LẦN 1: SoLuongTon = {lan1.SoLuongTon}", "USER A");
                            App.Current.Dispatcher.Invoke(() =>
                                DataUserA = new ObservableCollection<LinhKien>(new[] { lan1 }));

                            await Task.Delay(10000); // B sửa trong lúc này

                            // Đọc lần 2
                            var lan2 = await context.LinhKiens.AsNoTracking()
                                                    .FirstOrDefaultAsync(x => x.MaLk == "MOU001");
                            WriteLog($"A đọc LẦN 2: SoLuongTon = {lan2.SoLuongTon}", "USER A");
                            App.Current.Dispatcher.Invoke(() =>
                                DataUserA = new ObservableCollection<LinhKien>(new[] { lan2 }));

                            await tran.CommitAsync();
                        }
                        break;

                    case 3: // Phantom Read - A đếm 2 lần
                        WriteLog($"Bắt đầu {KichBanChon.TenKichBan}...", "USER A");
                        using (var tran = await context.Database.BeginTransactionAsync(
                            IsFixMode ? System.Data.IsolationLevel.Serializable
                                      : System.Data.IsolationLevel.RepeatableRead))
                        {
                            // Đếm lần 1
                            var ds1 = await context.LinhKiens.AsNoTracking()
                                                   .Where(x => x.MaLoai == "MOU").ToListAsync();
                            WriteLog($"A đếm LẦN 1: Tổng MOU = {ds1.Count}", "USER A");
                            App.Current.Dispatcher.Invoke(() =>
                                DataUserA = new ObservableCollection<LinhKien>(ds1));

                            await Task.Delay(10000); // B chèn dòng mới trong lúc này

                            // Đếm lần 2
                            var ds2 = await context.LinhKiens.AsNoTracking()
                                                   .Where(x => x.MaLoai == "MOU").ToListAsync();
                            WriteLog($"A đếm LẦN 2: Tổng MOU = {ds2.Count}", "USER A");
                            App.Current.Dispatcher.Invoke(() =>
                                DataUserA = new ObservableCollection<LinhKien>(ds2));

                            await tran.CommitAsync();
                        }
                        break;

                    case 4: // Deadlock
                        await context.Procedures.sp_kichban5_giaotacaAsync();
                        WriteLog("A đã khóa MOU001 → chờ MOU002", "USER A");
                        var lk4 = await context.LinhKiens.AsNoTracking()
                                               .Where(x => x.MaLk == "MOU001" || x.MaLk == "MOU002")
                                               .ToListAsync();
                        App.Current.Dispatcher.Invoke(() =>
                            DataUserA = new ObservableCollection<LinhKien>(lk4));
                        break;
                }

                WriteLog("GIAO TÁC A HOÀN THÀNH!", "USER A");
            }
            catch (Exception ex)
            {
                WriteLog($"Lỗi Giao tác A: {ex.Message}", "USER A");
            }
            finally
            {
                IsUserABusy = false;
            }
        }

        private async Task RunUserBAsync()
        {
            IsUserBBusy = true;
            try
            {
                using var context = CreateNewContext();
                WriteLog($"Bắt đầu {KichBanChon.TenKichBan}...", "USER B");

                switch (SelectedScenarioIndex)
                {
                    case 0: // Lost Update
                        await context.Procedures.sp_kichban1_giaotacbAsync(IsFixMode);
                        WriteLog("B đã ghi xong SoLuongTon", "USER B");
                        var lk0 = await context.LinhKiens.AsNoTracking()
                                               .FirstOrDefaultAsync(x => x.MaLk == "MOU001");
                        App.Current.Dispatcher.Invoke(() =>
                            DataUserB = new ObservableCollection<LinhKien>(new[] { lk0 }));
                        break;

                    case 1: // Dirty Read - B phải dùng LINQ để set isolation level ReadUncommitted
                        using (var tran = await context.Database.BeginTransactionAsync(
                            IsFixMode ? System.Data.IsolationLevel.ReadCommitted
                                      : System.Data.IsolationLevel.ReadUncommitted))
                        {
                            var lk = await context.LinhKiens.AsNoTracking()
                                                  .FirstOrDefaultAsync(x => x.MaLk == "MOU001");
                            WriteLog($"B đọc SoLuongTon = {lk.SoLuongTon} " +
                                     $"({(IsFixMode ? "đã chờ A commit" : "đọc dữ liệu rác của A")})",
                                     "USER B");
                            App.Current.Dispatcher.Invoke(() =>
                                DataUserB = new ObservableCollection<LinhKien>(new[] { lk }));
                            await tran.CommitAsync();
                        }
                        break;

                    case 2: // Unrepeatable Read - B sửa
                        await context.Procedures.sp_kichban3_giaotacbAsync();
                        WriteLog("B đã sửa SoLuongTon = 9999", "USER B");
                        var lk2 = await context.LinhKiens.AsNoTracking()
                                               .FirstOrDefaultAsync(x => x.MaLk == "MOU001");
                        App.Current.Dispatcher.Invoke(() =>
                            DataUserB = new ObservableCollection<LinhKien>(new[] { lk2 }));
                        break;

                    case 3: // Phantom Read - B chèn dòng mới
                        await context.Procedures.sp_kichban4_giaotacbAsync();
                        WriteLog("B đã chèn MOU099 vào nhóm MOU", "USER B");
                        var ds3 = await context.LinhKiens.AsNoTracking()
                                               .Where(x => x.MaLoai == "MOU").ToListAsync();
                        App.Current.Dispatcher.Invoke(() =>
                            DataUserB = new ObservableCollection<LinhKien>(ds3));
                        break;

                    case 4: // Deadlock
                        await context.Procedures.sp_kichban5_giaotacbAsync(IsFixMode);
                        WriteLog(IsFixMode ? "B chờ MOU001 cùng thứ tự A → không deadlock"
                                           : "B khóa MOU002 → deadlock với A", "USER B");
                        var lk4 = await context.LinhKiens.AsNoTracking()
                                               .Where(x => x.MaLk == "MOU001" || x.MaLk == "MOU002")
                                               .ToListAsync();
                        App.Current.Dispatcher.Invoke(() =>
                            DataUserB = new ObservableCollection<LinhKien>(lk4));
                        break;
                }

                WriteLog("GIAO TÁC B HOÀN THÀNH!", "USER B");
            }
            catch (Exception ex)
            {
                WriteLog($"Lỗi Giao tác B: {ex.Message}", "USER B");
            }
            finally
            {
                IsUserBBusy = false;
            }
        }
    }
}