using Microsoft.EntityFrameworkCore;

namespace AgriB2B.Api.Tests.Data;

[Collection(nameof(ApiCollection))]
[Trait("Spec", "phase-2-data-model")]
public class SchemaTests(ApiFactory factory)
{
    // 每一欄寫成「資料表.欄位 型別」，選填的欄位後面加 null，和規格的表格逐一對照
    private static readonly string[] ExpectedColumns =
    [
        .. Table("users",
            "account varchar(50)",
            "display_name varchar(100)",
            "password_hash varchar(200)",
            "role varchar(20)",
            "is_active boolean",
            "must_change_password boolean",
            "security_stamp varchar(64)",
            "failed_login_count integer",
            "lockout_until timestamptz null",
            "last_login_at timestamptz null"),
        .. Table("distributors",
            "user_id integer",
            "company_name varchar(100)",
            "tax_id varchar(8) null",
            "contact_name varchar(50)",
            "phone varchar(30)",
            "email varchar(254) null",
            "address varchar(200)",
            "note varchar(500) null"),
        .. Table("categories",
            "name varchar(50)",
            "sort_order integer",
            "is_active boolean"),
        .. Table("products",
            "category_id integer",
            "name varchar(100)",
            "license_no varchar(50)",
            "active_ingredient varchar(200)",
            "formulation varchar(50)",
            "manufacturer varchar(100)",
            "description text null",
            "image_path varchar(300) null",
            "is_active boolean"),
        .. Table("product_variants",
            "product_id integer",
            "name varchar(50)",
            "sku varchar(50)",
            "unit_price bigint",
            "sort_order integer",
            "is_active boolean"),
        .. Table("orders",
            "order_no varchar(20)",
            "distributor_id integer",
            "status varchar(20)",
            "recipient_name varchar(50)",
            "recipient_phone varchar(30)",
            "shipping_address varchar(200)",
            "note varchar(500) null",
            "total_amount bigint"),
        .. Table("order_items",
            "order_id integer",
            "product_variant_id integer",
            "product_name varchar(100)",
            "variant_name varchar(50)",
            "unit_price bigint",
            "quantity integer",
            "subtotal bigint"),
        .. Table("order_status_histories",
            "order_id integer",
            "from_status varchar(20) null",
            "to_status varchar(20)",
            "changed_by integer",
            "note varchar(500) null"),
    ];

    [Fact(DisplayName = "AC-3：所有資料表的欄位名稱、型別、長度、是否選填都和規格一致")]
    public async Task Columns_MatchSpec()
    {
        // format() 遇到空值會當成空字串，沒設長度的欄位會顯示成 varchar()，比對時看得出來
        var columns = await factory.QueryAsync(db => db.Database.SqlQuery<string>($"""
            SELECT format('%s.%s %s%s',
                table_name,
                column_name,
                CASE data_type
                    WHEN 'character varying' THEN format('varchar(%s)', character_maximum_length)
                    WHEN 'numeric' THEN format('numeric(%s,%s)', numeric_precision, numeric_scale)
                    WHEN 'timestamp with time zone' THEN 'timestamptz'
                    ELSE data_type
                END,
                CASE is_nullable WHEN 'YES' THEN ' null' ELSE '' END) AS "Value"
            FROM information_schema.columns
            WHERE table_schema = 'public' AND table_name <> '__EFMigrationsHistory'
            """).ToListAsync());

        Assert.Equal(ExpectedColumns.Order(StringComparer.Ordinal), columns.Order(StringComparer.Ordinal));
    }

    [Fact(DisplayName = "AC-8：所有外鍵都設成還有資料參照時不能刪除")]
    public async Task ForeignKeys_RestrictDelete()
    {
        var foreignKeys = await factory.QueryAsync(db => db.Database.SqlQuery<string>($"""
            SELECT format('%s.%s -> %s %s', kcu.table_name, kcu.column_name, ccu.table_name, rc.delete_rule) AS "Value"
            FROM information_schema.referential_constraints rc
            JOIN information_schema.key_column_usage kcu
                ON kcu.constraint_schema = rc.constraint_schema AND kcu.constraint_name = rc.constraint_name
            JOIN information_schema.constraint_column_usage ccu
                ON ccu.constraint_schema = rc.constraint_schema AND ccu.constraint_name = rc.constraint_name
            WHERE rc.constraint_schema = 'public'
            """).ToListAsync());

        Assert.Equal(
            [
                "distributors.user_id -> users RESTRICT",
                "order_items.order_id -> orders RESTRICT",
                "order_items.product_variant_id -> product_variants RESTRICT",
                "order_status_histories.changed_by -> users RESTRICT",
                "order_status_histories.order_id -> orders RESTRICT",
                "orders.distributor_id -> distributors RESTRICT",
                "product_variants.product_id -> products RESTRICT",
                "products.category_id -> categories RESTRICT",
            ],
            foreignKeys.Order(StringComparer.Ordinal));
    }

    // 每張表都有 id、created_at、updated_at，規格的表格沒有列出，這裡自動補上
    private static IEnumerable<string> Table(string name, params string[] columns)
    {
        string[] all = ["id integer", .. columns, "created_at timestamptz", "updated_at timestamptz"];
        return all.Select(column => $"{name}.{column}");
    }
}
