using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgriB2B.Api.Data.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "categories",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                sort_order = table.Column<int>(type: "integer", nullable: false),
                is_active = table.Column<bool>(type: "boolean", nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_categories", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "users",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                account = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                display_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                password_hash = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                is_active = table.Column<bool>(type: "boolean", nullable: false),
                must_change_password = table.Column<bool>(type: "boolean", nullable: false),
                security_stamp = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                failed_login_count = table.Column<int>(type: "integer", nullable: false),
                lockout_until = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                last_login_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_users", x => x.id);
                table.CheckConstraint("ck_users_account_lowercase", "account = lower(account)");
                table.CheckConstraint("ck_users_account_trimmed", "account ~ '^[^\\u0009\\u000a\\u000b\\u000c\\u000d\\u0020\\u0085\\u00a0\\u1680\\u2000\\u2001\\u2002\\u2003\\u2004\\u2005\\u2006\\u2007\\u2008\\u2009\\u200a\\u2028\\u2029\\u202f\\u205f\\u3000](.*[^\\u0009\\u000a\\u000b\\u000c\\u000d\\u0020\\u0085\\u00a0\\u1680\\u2000\\u2001\\u2002\\u2003\\u2004\\u2005\\u2006\\u2007\\u2008\\u2009\\u200a\\u2028\\u2029\\u202f\\u205f\\u3000])?$'");
                table.CheckConstraint("ck_users_role", "role IN ('Admin', 'Distributor')");
            });

        migrationBuilder.CreateTable(
            name: "products",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                category_id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                license_no = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                active_ingredient = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                formulation = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                manufacturer = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                description = table.Column<string>(type: "text", nullable: true),
                image_path = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                is_active = table.Column<bool>(type: "boolean", nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_products", x => x.id);
                table.ForeignKey(
                    name: "fk_products_categories_category_id",
                    column: x => x.category_id,
                    principalTable: "categories",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "distributors",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                company_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                tax_id = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                contact_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: true),
                address = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_distributors", x => x.id);
                table.CheckConstraint("ck_distributors_tax_id", "tax_id ~ '^[0-9]{8}$'");
                table.ForeignKey(
                    name: "fk_distributors_users_user_id",
                    column: x => x.user_id,
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "product_variants",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                product_id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                sku = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                unit_price = table.Column<long>(type: "bigint", nullable: false),
                sort_order = table.Column<int>(type: "integer", nullable: false),
                is_active = table.Column<bool>(type: "boolean", nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_product_variants", x => x.id);
                table.CheckConstraint("ck_product_variants_unit_price", "unit_price >= 0");
                table.ForeignKey(
                    name: "fk_product_variants_products_product_id",
                    column: x => x.product_id,
                    principalTable: "products",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "orders",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                order_no = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                distributor_id = table.Column<Guid>(type: "uuid", nullable: false),
                status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                recipient_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                recipient_phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                shipping_address = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                total_amount = table.Column<long>(type: "bigint", nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_orders", x => x.id);
                table.CheckConstraint("ck_orders_status", "status IN ('Pending', 'Confirmed', 'Shipped', 'Completed', 'Cancelled')");
                table.CheckConstraint("ck_orders_total_amount", "total_amount >= 0");
                table.ForeignKey(
                    name: "fk_orders_distributors_distributor_id",
                    column: x => x.distributor_id,
                    principalTable: "distributors",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "order_items",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                order_id = table.Column<Guid>(type: "uuid", nullable: false),
                product_variant_id = table.Column<Guid>(type: "uuid", nullable: false),
                product_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                variant_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                unit_price = table.Column<long>(type: "bigint", nullable: false),
                quantity = table.Column<int>(type: "integer", nullable: false),
                subtotal = table.Column<long>(type: "bigint", nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_order_items", x => x.id);
                table.CheckConstraint("ck_order_items_quantity", "quantity > 0");
                table.CheckConstraint("ck_order_items_subtotal", "subtotal >= 0");
                table.CheckConstraint("ck_order_items_unit_price", "unit_price >= 0");
                table.ForeignKey(
                    name: "fk_order_items_orders_order_id",
                    column: x => x.order_id,
                    principalTable: "orders",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_order_items_product_variants_product_variant_id",
                    column: x => x.product_variant_id,
                    principalTable: "product_variants",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "order_status_histories",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                order_id = table.Column<Guid>(type: "uuid", nullable: false),
                from_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                to_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                changed_by = table.Column<Guid>(type: "uuid", nullable: false),
                note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_order_status_histories", x => x.id);
                table.CheckConstraint("ck_order_status_histories_from_status", "from_status IN ('Pending', 'Confirmed', 'Shipped', 'Completed', 'Cancelled')");
                table.CheckConstraint("ck_order_status_histories_to_status", "to_status IN ('Pending', 'Confirmed', 'Shipped', 'Completed', 'Cancelled')");
                table.ForeignKey(
                    name: "fk_order_status_histories_orders_order_id",
                    column: x => x.order_id,
                    principalTable: "orders",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_order_status_histories_users_changed_by",
                    column: x => x.changed_by,
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "ix_distributors_tax_id",
            table: "distributors",
            column: "tax_id",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_distributors_user_id",
            table: "distributors",
            column: "user_id",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_order_items_order_id",
            table: "order_items",
            column: "order_id");

        migrationBuilder.CreateIndex(
            name: "ix_order_items_product_variant_id",
            table: "order_items",
            column: "product_variant_id");

        migrationBuilder.CreateIndex(
            name: "ix_order_status_histories_changed_by",
            table: "order_status_histories",
            column: "changed_by");

        migrationBuilder.CreateIndex(
            name: "ix_order_status_histories_order_id",
            table: "order_status_histories",
            column: "order_id");

        migrationBuilder.CreateIndex(
            name: "ix_orders_distributor_id",
            table: "orders",
            column: "distributor_id");

        migrationBuilder.CreateIndex(
            name: "ix_orders_order_no",
            table: "orders",
            column: "order_no",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_product_variants_product_id",
            table: "product_variants",
            column: "product_id");

        migrationBuilder.CreateIndex(
            name: "ix_product_variants_sku",
            table: "product_variants",
            column: "sku",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_products_category_id",
            table: "products",
            column: "category_id");

        migrationBuilder.CreateIndex(
            name: "ix_users_account",
            table: "users",
            column: "account",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "order_items");

        migrationBuilder.DropTable(
            name: "order_status_histories");

        migrationBuilder.DropTable(
            name: "product_variants");

        migrationBuilder.DropTable(
            name: "orders");

        migrationBuilder.DropTable(
            name: "products");

        migrationBuilder.DropTable(
            name: "distributors");

        migrationBuilder.DropTable(
            name: "categories");

        migrationBuilder.DropTable(
            name: "users");
    }
}
