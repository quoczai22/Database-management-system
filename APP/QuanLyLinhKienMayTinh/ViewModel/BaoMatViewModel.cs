using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using QuanLyLinhKienMayTinh.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
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
        // các thuộc tính

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

        private bool _isBackupRestoreBusy;
        public bool IsBackupRestoreBusy
        {
            get => _isBackupRestoreBusy;
            set
            {
                _isBackupRestoreBusy = value;
                OnPropertyChanged(nameof(IsBackupRestoreBusy));
                OnPropertyChanged(nameof(IsBackupRestoreIdle));
            }
        }

        public bool IsBackupRestoreIdle => !_isBackupRestoreBusy;

        // commands

        public ICommand RunUserACommand { get; }
        public ICommand RunUserBCommand { get; }
        public ICommand ResetDataCommand { get; }
        public ICommand BackupDatabaseCommand { get; }
        public ICommand RestoreDatabaseCommand { get; }
        public ICommand RestoreBackupChainCommand { get; }

        // constructor

        public BaoMatViewModel()
        {
            RunUserACommand = new RelayCommand<object>(p => IsUserAIdle, p => RunUserAAsync());
            RunUserBCommand = new RelayCommand<object>(p => IsUserBIdle, p => RunUserBAsync());
            ResetDataCommand = new RelayCommand<object>(p => true, p => ResetDataAsync());
            BackupDatabaseCommand = new RelayCommand<object>(p => IsBackupRestoreIdle, p => BackupDatabaseAsync(p?.ToString() ?? "FULL"));
            RestoreDatabaseCommand = new RelayCommand<object>(p => IsBackupRestoreIdle, p => RestoreDatabaseAsync());
            RestoreBackupChainCommand = new RelayCommand<object>(p => IsBackupRestoreIdle, p => RestoreBackupChainAsync());

            KhoiTaoDanhSachKichBan();
            _ = LoadDataAsync();
        }

        // logic hỗ trợ

        private QL_LinhKien_PC_Context CreateNewContext()
        {
            string connString = DataProvider.Ins.GetContext().Database.GetConnectionString();
            var optionsBuilder = new DbContextOptionsBuilder<QL_LinhKien_PC_Context>();
            optionsBuilder.UseSqlServer(connString);
            return new QL_LinhKien_PC_Context(optionsBuilder.Options);
        }

        private static bool CoQuyenBackupRestore()
        {
            return LuuTrangThai.QuyenDangNhap == "Quản lý toàn bộ"
                || LuuTrangThai.QuyenDangNhap == "Bảo mật";
        }

        private static string GetCurrentConnectionString()
        {
            using var context = DataProvider.Ins.GetContext();
            return context.Database.GetConnectionString();
        }

        private static string QuoteSqlIdentifier(string value)
        {
            return "[" + value.Replace("]", "]]") + "]";
        }

        private static string GetMasterConnectionString(string connString)
        {
            var builder = new SqlConnectionStringBuilder(connString)
            {
                InitialCatalog = "master"
            };
            return builder.ConnectionString;
        }

        private static async Task ExecuteMasterCommandAsync(string sql, string? backupPath = null)
        {
            var connString = GetCurrentConnectionString();
            await using var connection = new SqlConnection(GetMasterConnectionString(connString));
            await connection.OpenAsync();

            await using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.CommandTimeout = 0;

            if (!string.IsNullOrWhiteSpace(backupPath))
            {
                command.Parameters.Add(new SqlParameter("@BackupPath", backupPath));
            }

            await command.ExecuteNonQueryAsync();
        }

        private async Task BackupDatabaseAsync(string backupType)
        {
            if (!CoQuyenBackupRestore())
            {
                MessageBox.Show("Chỉ tài khoản quản lý mới được sao lưu cơ sở dữ liệu.", "Từ chối truy cập", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var normalizedType = (backupType ?? "FULL").ToUpperInvariant();
            var backupLabel = normalizedType switch
            {
                "DIFFERENTIAL" => "Differential",
                "LOG" => "Log",
                _ => "Full"
            };
            var extension = normalizedType == "LOG" ? ".trn" : ".bak";

            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Title = "Chọn nơi lưu file backup",
                FileName = $"QL_LinhKien_PC_{backupLabel}_{DateTime.Now:yyyyMMdd_HHmmss}{extension}",
                Filter = normalizedType == "LOG"
                    ? "SQL Server Transaction Log (*.trn)|*.trn|SQL Server Backup (*.bak)|*.bak|All files (*.*)|*.*"
                    : "SQL Server Backup (*.bak)|*.bak|All files (*.*)|*.*",
                AddExtension = true,
                DefaultExt = extension
            };

            if (dialog.ShowDialog() != true) return;

            try
            {
                IsBackupRestoreBusy = true;
                var connString = GetCurrentConnectionString();
                var dbName = new SqlConnectionStringBuilder(connString).InitialCatalog;
                var db = QuoteSqlIdentifier(dbName);
                var sql = normalizedType switch
                {
                    "DIFFERENTIAL" => $"BACKUP DATABASE {db} TO DISK = @BackupPath WITH DIFFERENTIAL, INIT, STATS = 10;",
                    "LOG" => $"BACKUP LOG {db} TO DISK = @BackupPath WITH INIT, STATS = 10;",
                    _ => $"ALTER DATABASE {db} SET RECOVERY FULL;" + Environment.NewLine +
                         $"BACKUP DATABASE {db} TO DISK = @BackupPath WITH INIT, STATS = 10;"
                };

                await ExecuteMasterCommandAsync(sql, dialog.FileName);

                WriteLog($"Đã backup {backupLabel} CSDL thành công: {dialog.FileName}", "HỆ THỐNG");
                MessageBox.Show($"Backup {backupLabel} cơ sở dữ liệu thành công!", "Backup", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                WriteLog($"Lỗi backup {backupLabel} CSDL: {ex.Message}", "HỆ THỐNG");
                MessageBox.Show($"Lỗi backup {backupLabel} cơ sở dữ liệu: " + ex.Message, "Backup", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBackupRestoreBusy = false;
            }
        }

        private async Task RestoreDatabaseAsync()
        {
            if (!CoQuyenBackupRestore())
            {
                MessageBox.Show("Chỉ tài khoản quản lý mới được phục hồi cơ sở dữ liệu.", "Từ chối truy cập", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Chọn file backup để restore",
                Filter = "SQL Server Backup (*.bak)|*.bak|All files (*.*)|*.*"
            };

            if (dialog.ShowDialog() != true) return;

            if (!File.Exists(dialog.FileName))
            {
                MessageBox.Show("File backup không tồn tại.", "Restore", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var confirm = MessageBox.Show(
                "Restore sẽ ghi đè cơ sở dữ liệu hiện tại bằng file backup đã chọn. Bạn có chắc muốn tiếp tục?",
                "Xác nhận restore",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes) return;

            var connString = GetCurrentConnectionString();
            var dbName = new SqlConnectionStringBuilder(connString).InitialCatalog;
            var db = QuoteSqlIdentifier(dbName);

            try
            {
                IsBackupRestoreBusy = true;
                var sql =
                    $"ALTER DATABASE {db} SET SINGLE_USER WITH ROLLBACK IMMEDIATE;" + Environment.NewLine +
                    $"RESTORE DATABASE {db} FROM DISK = @BackupPath WITH REPLACE, RECOVERY, STATS = 10;" + Environment.NewLine +
                    $"ALTER DATABASE {db} SET MULTI_USER;";

                await ExecuteMasterCommandAsync(sql, dialog.FileName);
                await LoadDataAsync();

                WriteLog($"Đã restore CSDL từ file: {dialog.FileName}", "HỆ THỐNG");
                MessageBox.Show("Restore cơ sở dữ liệu thành công!", "Restore", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                try
                {
                    await ExecuteMasterCommandAsync($"ALTER DATABASE {db} SET MULTI_USER;");
                }
                catch
                {
                }

                WriteLog($"Lỗi restore CSDL: {ex.Message}", "HỆ THỐNG");
                MessageBox.Show("Lỗi restore cơ sở dữ liệu: " + ex.Message, "Restore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBackupRestoreBusy = false;
            }
        }

        private static string? ChonFileRestore(string title, string filter)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = title,
                Filter = filter
            };

            if (dialog.ShowDialog() != true) return null;

            if (!File.Exists(dialog.FileName))
            {
                MessageBox.Show("File backup không tồn tại.", "Restore", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;
            }

            return dialog.FileName;
        }

        private async Task RestoreBackupChainAsync()
        {
            if (!CoQuyenBackupRestore())
            {
                MessageBox.Show("Chỉ tài khoản quản lý mới được phục hồi cơ sở dữ liệu.", "Từ chối truy cập", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var fullPath = ChonFileRestore("Chọn file full backup", "SQL Server Backup (*.bak)|*.bak|All files (*.*)|*.*");
            if (fullPath == null) return;

            string? diffPath = null;
            if (MessageBox.Show("Có restore thêm differential backup không?", "Restore differential", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                diffPath = ChonFileRestore("Chọn file differential backup", "SQL Server Backup (*.bak)|*.bak|All files (*.*)|*.*");
                if (diffPath == null) return;
            }

            string? logPath = null;
            if (MessageBox.Show("Có restore thêm log backup không?", "Restore log", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                logPath = ChonFileRestore("Chọn file log backup", "SQL Server Transaction Log (*.trn)|*.trn|SQL Server Backup (*.bak)|*.bak|All files (*.*)|*.*");
                if (logPath == null) return;
            }

            var confirm = MessageBox.Show(
                "Restore chuỗi backup sẽ ghi đè cơ sở dữ liệu hiện tại. Bạn có chắc muốn tiếp tục?",
                "Xác nhận restore",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes) return;

            var connString = GetCurrentConnectionString();
            var dbName = new SqlConnectionStringBuilder(connString).InitialCatalog;
            var db = QuoteSqlIdentifier(dbName);

            try
            {
                IsBackupRestoreBusy = true;

                await ExecuteMasterCommandAsync($"ALTER DATABASE {db} SET SINGLE_USER WITH ROLLBACK IMMEDIATE;");
                await ExecuteMasterCommandAsync($"RESTORE DATABASE {db} FROM DISK = @BackupPath WITH REPLACE, NORECOVERY, STATS = 10;", fullPath);

                if (diffPath != null)
                {
                    await ExecuteMasterCommandAsync($"RESTORE DATABASE {db} FROM DISK = @BackupPath WITH NORECOVERY, STATS = 10;", diffPath);
                }

                if (logPath != null)
                {
                    await ExecuteMasterCommandAsync($"RESTORE LOG {db} FROM DISK = @BackupPath WITH RECOVERY, STATS = 10;", logPath);
                }
                else
                {
                    await ExecuteMasterCommandAsync($"RESTORE DATABASE {db} WITH RECOVERY;");
                }

                await ExecuteMasterCommandAsync($"ALTER DATABASE {db} SET MULTI_USER;");
                await LoadDataAsync();

                WriteLog($"Đã restore chuỗi backup. Full: {fullPath}; Diff: {diffPath ?? "không dùng"}; Log: {logPath ?? "không dùng"}", "HỆ THỐNG");
                MessageBox.Show("Restore chuỗi backup thành công!", "Restore", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                try
                {
                    await ExecuteMasterCommandAsync($"RESTORE DATABASE {db} WITH RECOVERY;");
                    await ExecuteMasterCommandAsync($"ALTER DATABASE {db} SET MULTI_USER;");
                }
                catch
                {
                }

                WriteLog($"Lỗi restore chuỗi backup: {ex.Message}", "HỆ THỐNG");
                MessageBox.Show("Lỗi restore chuỗi backup: " + ex.Message, "Restore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBackupRestoreBusy = false;
            }
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
                new KichBanModel { Id = 4, TenKichBan = "Kịch bản 5: Tắc nghẽn giao dịch (Deadlock)", MoTa = "Tác động: MOU001 & MOU002" },
                new KichBanModel { Id = 5, TenKichBan = "Kịch bản 6: Giao tác đồng thời có rollback", MoTa = "Tác động: MOU001" }
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
                    case 5:
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

        // thực thi giao tác và reset

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

                    case 5: // Concurrent transaction with rollback
                        WriteLog("A cập nhật tạm MOU001, giữ khóa 10 giây rồi phát sinh lỗi để rollback", "USER A");
                        var kq6a = await context.Procedures.sp_kichban6_giaotaca_rollbackAsync();
                        foreach (var row in kq6a)
                        {
                            WriteLog($"{row.ThongBao} | Tồn ban đầu: {row.TonKhoBanDau} | Tồn sau rollback: {row.TonKhoTamThoi}", "USER A");
                        }

                        var lk6a = await context.LinhKiens.AsNoTracking()
                                               .FirstOrDefaultAsync(x => x.MaLk == "MOU001");
                        App.Current.Dispatcher.Invoke(() =>
                            DataUserA = new ObservableCollection<LinhKien>(new[] { lk6a }));
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

                    case 5: // Concurrent transaction with rollback
                        WriteLog("B đọc MOU001 ở READ COMMITTED, nếu A còn giữ khóa thì B phải chờ", "USER B");
                        var kq6b = await context.Procedures.sp_kichban6_giaotacb_docsaorollbackAsync();
                        foreach (var row in kq6b)
                        {
                            WriteLog($"{row.ThongBao} | Tồn kho đọc được: {row.TonKhoDocDuoc}", "USER B");
                        }

                        var lk6b = await context.LinhKiens.AsNoTracking()
                                               .FirstOrDefaultAsync(x => x.MaLk == "MOU001");
                        App.Current.Dispatcher.Invoke(() =>
                            DataUserB = new ObservableCollection<LinhKien>(new[] { lk6b }));
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
