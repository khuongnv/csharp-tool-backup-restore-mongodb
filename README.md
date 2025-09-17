# MongoDB Tools

Ứng dụng WinForms C# để backup và restore MongoDB database thành các file JSON.

## 📁 Repository

**GitHub**: [https://github.com/khuongnv/csharp-tool-backup-restore-mongodb.git](https://github.com/khuongnv/csharp-tool-backup-restore-mongodb.git)

## ✨ Tính năng

- 🔗 **Connection Management**: Lưu trữ và quản lý connection strings riêng biệt cho Backup và Restore
- 🧪 **Test Connection**: Kiểm tra kết nối MongoDB trước khi thực hiện backup/restore
- 💾 **Backup Database**: Export toàn bộ database thành các file JSON
- 🔄 **Restore Database**: Import database từ các file JSON đã backup
- 📊 **Progress Tracking**: Hiển thị tiến trình backup/restore real-time
- 📋 **Activity Logging**: Log chi tiết quá trình backup/restore
- 🎨 **Modern UI**: Giao diện Light theme hiện đại với TabControl
- ⚙️ **Settings Persistence**: Tự động lưu và load connection strings, database names

## 🚀 Cài đặt

### Yêu cầu hệ thống
- Windows 10/11
- .NET 6.0 Runtime hoặc cao hơn
- Visual Studio 2022 (khuyến nghị)

### Cách 1: Clone từ GitHub
```bash
git clone https://github.com/khuongnv/csharp-tool-backup-restore-mongodb.git
cd csharp-tool-backup-restore-mongodb
```

### Cách 2: Build và chạy
```bash
# Restore packages
dotnet restore

# Build project
dotnet build

# Chạy ứng dụng
dotnet run
```

### Cách 3: Visual Studio
1. Mở file `MongoDbTools.sln` trong Visual Studio 2022
2. Build solution (Ctrl+Shift+B)
3. Chạy project (F5)

## 📖 Hướng dẫn sử dụng

### Backup Database
1. Chọn tab **"Backup Database"**
2. Nhập **Connection String** (ví dụ: `mongodb://username:password@host:port/database`)
3. Nhập **Database Name** (ví dụ: `myapp_db`)
4. Click **"Test Connection"** để kiểm tra kết nối
5. Chọn **thư mục backup** để lưu file JSON
6. Click **"Start Backup"** để bắt đầu
7. Theo dõi tiến trình qua progress bar và log

### Restore Database
1. Chọn tab **"Restore Database"**
2. Nhập **Connection String** đích
3. Nhập **Database Name** đích
4. Click **"Test Connection"** để kiểm tra
5. Chọn **thư mục chứa file JSON** đã backup
6. Tick **"Drop existing collections"** nếu muốn xóa dữ liệu cũ
7. Click **"Start Restore"** để bắt đầu

## Cấu trúc backup

Sau khi backup, thư mục sẽ chứa:
- `collection1.json` - Dữ liệu collection 1
- `collection2.json` - Dữ liệu collection 2
- ...
- `backup_info.json` - Thông tin về backup (tên DB, ngày backup, danh sách collections)

## Cấu trúc restore

Để restore, chọn thư mục chứa các file JSON đã backup:
- Ứng dụng sẽ tự động tìm tất cả file `.json`
- Bỏ qua file `backup_info.json`
- Restore từng collection vào database

## 🔧 Cấu hình

### Connection String Examples
```
# Local MongoDB
mongodb://localhost:27017

# With Authentication
mongodb://username:password@host:port/database

# MongoDB Atlas
mongodb+srv://username:password@cluster.mongodb.net/database

# With Options
mongodb://host:port/database?retryWrites=true&w=majority
```

### Database Name Examples
- `myapp_db`
- `production_data`
- `user_management`
- `jobpxa`

## ⚠️ Lưu ý quan trọng

- 🔒 **Bảo mật**: Không chia sẻ connection strings chứa thông tin nhạy cảm
- 💾 **Backup**: Ứng dụng tạo thư mục backup với timestamp để tránh ghi đè
- 📁 **File Structure**: Mỗi collection được lưu thành 1 file JSON riêng biệt
- 🎨 **Format**: Dữ liệu được format JSON đẹp để dễ đọc
- ⏱️ **Performance**: Database lớn sẽ mất thời gian backup/restore
- 🗑️ **Restore**: Có thể chọn "Drop existing collections" để xóa dữ liệu cũ
- 💾 **Settings**: Connection strings và database names được lưu tự động

## 📄 License

MIT License - Xem file [LICENSE](LICENSE) để biết thêm chi tiết.

## 🤝 Contributing

Mọi đóng góp đều được chào đón! Vui lòng tạo Pull Request hoặc Issue trên GitHub.

## 📞 Support

Nếu gặp vấn đề, vui lòng tạo Issue trên [GitHub Repository](https://github.com/khuongnv/csharp-tool-backup-restore-mongodb.git).
