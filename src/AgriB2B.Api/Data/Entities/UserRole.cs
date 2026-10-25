namespace AgriB2B.Api.Data.Entities;

// 刻意不用 0：enum 沒有設定時是 0，忘了指定角色的帳號存檔時會被資料庫拒絕，不會變成管理員
public enum UserRole
{
    Admin = 1,
    Distributor = 2,
}
