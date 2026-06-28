using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MES.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "mes_alert_record",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    rule_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    level = table.Column<int>(type: "integer", nullable: false),
                    related_entity_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    related_entity_id = table.Column<long>(type: "bigint", nullable: true),
                    is_processed = table.Column<bool>(type: "boolean", nullable: false),
                    processed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    processed_by = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mes_alert_record", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "mes_alert_rule",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    condition = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    level = table.Column<int>(type: "integer", nullable: false),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mes_alert_rule", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "mes_andon_events",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    event_type = table.Column<int>(type: "integer", nullable: false),
                    level = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    workstation_id = table.Column<long>(type: "bigint", nullable: true),
                    workstation_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    work_order_id = table.Column<long>(type: "bigint", nullable: true),
                    work_order_no = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    triggered_by_id = table.Column<long>(type: "bigint", nullable: true),
                    triggered_by_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    triggered_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    resolved_by_id = table.Column<long>(type: "bigint", nullable: true),
                    resolved_by_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    resolved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mes_andon_events", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "mes_equipment",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    factory_id = table.Column<long>(type: "bigint", nullable: true),
                    workshop_id = table.Column<long>(type: "bigint", nullable: true),
                    line_id = table.Column<long>(type: "bigint", nullable: true),
                    install_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    last_maintain_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    next_maintain_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    maintain_cycle = table.Column<int>(type: "integer", nullable: true),
                    theoretical_cycle_time = table.Column<double>(type: "double precision", nullable: true),
                    planned_run_time = table.Column<double>(type: "double precision", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mes_equipment", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "mes_factory",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    status = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mes_factory", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "mes_knowledge_entry",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    category = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    keywords = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    material_id = table.Column<long>(type: "bigint", nullable: true),
                    equipment_id = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mes_knowledge_entry", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "mes_material",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    spec = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    unit = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    bom_level = table.Column<int>(type: "integer", nullable: true),
                    stock_qty = table.Column<decimal>(type: "numeric(18,4)", nullable: false, defaultValue: 0m),
                    status = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mes_material", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "mes_qc_checkpoint",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    step_id = table.Column<long>(type: "bigint", nullable: false),
                    check_type = table.Column<int>(type: "integer", nullable: false),
                    is_mandatory = table.Column<bool>(type: "boolean", nullable: false),
                    remark = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mes_qc_checkpoint", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "mes_qc_inspection",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    inspect_no = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    source_type = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    source_ref = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    work_order_id = table.Column<long>(type: "bigint", nullable: true),
                    material_id = table.Column<long>(type: "bigint", nullable: true),
                    inspector = table.Column<long>(type: "bigint", nullable: true),
                    inspect_result = table.Column<int>(type: "integer", nullable: false, defaultValue: 2),
                    inspect_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    remark = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    handling_action = table.Column<int>(type: "integer", nullable: true),
                    handling_remark = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    handled_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mes_qc_inspection", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "mes_role",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mes_role", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "mes_user",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    username = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    display_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    status = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    last_login_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mes_user", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "mes_maintenance_plan",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    equipment_id = table.Column<long>(type: "bigint", nullable: false),
                    plan_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    cycle_days = table.Column<int>(type: "integer", nullable: false),
                    last_completed_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    next_due_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    equipment_id1 = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mes_maintenance_plan", x => x.id);
                    table.ForeignKey(
                        name: "fk_mes_maintenance_plan_mes_equipment_equipment_id",
                        column: x => x.equipment_id,
                        principalTable: "mes_equipment",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_mes_maintenance_plan_mes_equipment_equipment_id1",
                        column: x => x.equipment_id1,
                        principalTable: "mes_equipment",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "mes_workshop",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    factory_id = table.Column<long>(type: "bigint", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    status = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mes_workshop", x => x.id);
                    table.ForeignKey(
                        name: "fk_mes_workshop_mes_factory_factory_id",
                        column: x => x.factory_id,
                        principalTable: "mes_factory",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "mes_bom",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    product_id = table.Column<long>(type: "bigint", nullable: false),
                    material_id = table.Column<long>(type: "bigint", nullable: false),
                    quantity = table.Column<decimal>(type: "numeric(18,6)", nullable: false),
                    scrap_rate = table.Column<decimal>(type: "numeric(5,2)", nullable: false, defaultValue: 0m),
                    seq_no = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    valid_from = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    valid_to = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mes_bom", x => x.id);
                    table.ForeignKey(
                        name: "fk_mes_bom_materials_material_id",
                        column: x => x.material_id,
                        principalTable: "mes_material",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_mes_bom_materials_product_id",
                        column: x => x.product_id,
                        principalTable: "mes_material",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "mes_material_trace",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    material_id = table.Column<long>(type: "bigint", nullable: false),
                    batch_no = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    serial_no = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    work_order_id = table.Column<long>(type: "bigint", nullable: true),
                    src_work_order_id = table.Column<long>(type: "bigint", nullable: true),
                    direction = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    qty = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    @operator = table.Column<long>(name: "operator", type: "bigint", nullable: true),
                    operate_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    remark = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mes_material_trace", x => x.id);
                    table.ForeignKey(
                        name: "fk_mes_material_trace_mes_material_material_id",
                        column: x => x.material_id,
                        principalTable: "mes_material",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "mes_routing",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    material_id = table.Column<long>(type: "bigint", nullable: false),
                    routing_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    routing_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    version = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "V1.0"),
                    status = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mes_routing", x => x.id);
                    table.ForeignKey(
                        name: "fk_mes_routing_mes_material_material_id",
                        column: x => x.material_id,
                        principalTable: "mes_material",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "mes_qc_inspection_item",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    inspection_id = table.Column<long>(type: "bigint", nullable: false),
                    item_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    spec_value = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    actual_value = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    result = table.Column<int>(type: "integer", nullable: false, defaultValue: 2),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mes_qc_inspection_item", x => x.id);
                    table.ForeignKey(
                        name: "fk_mes_qc_inspection_item_mes_qc_inspection_inspection_id",
                        column: x => x.inspection_id,
                        principalTable: "mes_qc_inspection",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "mes_role_permission",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    role_id = table.Column<long>(type: "bigint", nullable: false),
                    permission = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mes_role_permission", x => x.id);
                    table.ForeignKey(
                        name: "fk_mes_role_permission_mes_role_role_id",
                        column: x => x.role_id,
                        principalTable: "mes_role",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "mes_user_role",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    role_id = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mes_user_role", x => x.id);
                    table.ForeignKey(
                        name: "fk_mes_user_role_mes_role_role_id",
                        column: x => x.role_id,
                        principalTable: "mes_role",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_mes_user_role_mes_user_user_id",
                        column: x => x.user_id,
                        principalTable: "mes_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "mes_production_line",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    workshop_id = table.Column<long>(type: "bigint", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    line_type = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    status = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mes_production_line", x => x.id);
                    table.ForeignKey(
                        name: "fk_mes_production_line_workshops_workshop_id",
                        column: x => x.workshop_id,
                        principalTable: "mes_workshop",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "mes_routing_step",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    routing_id = table.Column<long>(type: "bigint", nullable: false),
                    step_no = table.Column<int>(type: "integer", nullable: false),
                    step_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    workstation_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    standard_time = table.Column<decimal>(type: "numeric(10,2)", nullable: false, defaultValue: 0m),
                    is_qc_point = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_scrap_point = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mes_routing_step", x => x.id);
                    table.ForeignKey(
                        name: "fk_mes_routing_step_mes_routing_routing_id",
                        column: x => x.routing_id,
                        principalTable: "mes_routing",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "mes_work_order",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    order_no = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    source_type = table.Column<int>(type: "integer", nullable: false),
                    source_ref = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    material_id = table.Column<long>(type: "bigint", nullable: false),
                    routing_id = table.Column<long>(type: "bigint", nullable: true),
                    planned_qty = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    completed_qty = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    scrap_qty = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    status = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    plan_start_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    plan_end_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    actual_start_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    actual_end_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    priority = table.Column<int>(type: "integer", nullable: false, defaultValue: 50),
                    factory_id = table.Column<long>(type: "bigint", nullable: true),
                    workshop_id = table.Column<long>(type: "bigint", nullable: true),
                    line_id = table.Column<long>(type: "bigint", nullable: true),
                    assignee = table.Column<long>(type: "bigint", nullable: true),
                    remark = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    rework_from_id = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mes_work_order", x => x.id);
                    table.ForeignKey(
                        name: "fk_mes_work_order_mes_material_material_id",
                        column: x => x.material_id,
                        principalTable: "mes_material",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_mes_work_order_mes_routing_routing_id",
                        column: x => x.routing_id,
                        principalTable: "mes_routing",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "mes_workstation",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    line_id = table.Column<long>(type: "bigint", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    seq_no = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    status = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mes_workstation", x => x.id);
                    table.ForeignKey(
                        name: "fk_mes_workstation_mes_production_line_line_id",
                        column: x => x.line_id,
                        principalTable: "mes_production_line",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "mes_work_order_step",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    work_order_id = table.Column<long>(type: "bigint", nullable: false),
                    step_no = table.Column<int>(type: "integer", nullable: false),
                    step_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    workstation_id = table.Column<long>(type: "bigint", nullable: true),
                    planned_qty = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    completed_qty = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    scrap_qty = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    status = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    plan_start_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    plan_end_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mes_work_order_step", x => x.id);
                    table.ForeignKey(
                        name: "fk_mes_work_order_step_mes_work_order_work_order_id",
                        column: x => x.work_order_id,
                        principalTable: "mes_work_order",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_mes_work_order_step_workstations_workstation_id",
                        column: x => x.workstation_id,
                        principalTable: "mes_workstation",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "mes_work_report",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    report_no = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    work_order_id = table.Column<long>(type: "bigint", nullable: false),
                    step_id = table.Column<long>(type: "bigint", nullable: true),
                    workstation_id = table.Column<long>(type: "bigint", nullable: true),
                    operator_id = table.Column<long>(type: "bigint", nullable: true),
                    report_type = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    good_qty = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    scrap_qty = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    rework_qty = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    duration_min = table.Column<int>(type: "integer", nullable: false),
                    report_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    remark = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    batch_no = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mes_work_report", x => x.id);
                    table.ForeignKey(
                        name: "fk_mes_work_report_mes_work_order_step_step_id",
                        column: x => x.step_id,
                        principalTable: "mes_work_order_step",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_mes_work_report_mes_work_order_work_order_id",
                        column: x => x.work_order_id,
                        principalTable: "mes_work_order",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_mes_work_report_workstations_workstation_id",
                        column: x => x.workstation_id,
                        principalTable: "mes_workstation",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.InsertData(
                table: "mes_alert_rule",
                columns: new[] { "id", "condition", "created_at", "created_by", "description", "is_deleted", "is_enabled", "level", "name", "updated_at", "updated_by" },
                values: new object[,]
                {
                    { 1L, "consecutive_scrap_rate > 0.05", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0L, "连续3个工单报废率 > 5%", false, true, 2, "产线连续不良率飙升", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 2L, "batch_defect_rate > 0.03", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0L, "某批次物料在多个工单中不良率 > 3%", false, true, 3, "物料批次异常", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 3L, "consecutive_rework >= 5", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0L, "某工位连续5次报工返工", false, true, 1, "工位连续返工", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null }
                });

            migrationBuilder.InsertData(
                table: "mes_factory",
                columns: new[] { "id", "address", "code", "created_at", "created_by", "name", "status", "updated_at", "updated_by" },
                values: new object[] { 1L, "中国上海市嘉定区", "FACTORY-001", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0L, "MES制造基地", true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null });

            migrationBuilder.InsertData(
                table: "mes_material",
                columns: new[] { "id", "bom_level", "category", "code", "created_at", "created_by", "name", "spec", "status", "stock_qty", "unit", "updated_at", "updated_by" },
                values: new object[,]
                {
                    { 1L, null, "成品", "MAT-001", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0L, "产品 A", "100*50*30", true, 1000m, "PCS", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 2L, null, "半成品", "MAT-002", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0L, "部件 B", "50*30*10", true, 5000m, "PCS", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 3L, null, "配件", "MAT-003", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0L, "螺丝 M6", "M6*20", true, 10000m, "PCS", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null }
                });

            migrationBuilder.InsertData(
                table: "mes_role",
                columns: new[] { "id", "created_at", "created_by", "description", "name", "updated_at", "updated_by" },
                values: new object[,]
                {
                    { 1L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0L, "系统管理员", "Admin", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 2L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0L, "生产经理", "ProductionManager", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 3L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0L, "质量工程师", "QualityEngineer", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 4L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0L, "设备工程师", "EquipmentEngineer", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 5L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0L, "操作员", "Operator", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null }
                });

            migrationBuilder.InsertData(
                table: "mes_user",
                columns: new[] { "id", "created_at", "created_by", "display_name", "email", "last_login_time", "password_hash", "phone", "status", "updated_at", "updated_by", "username" },
                values: new object[] { 1L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0L, "系统管理员", "admin@mes.local", null, "F0CE0E86206541C60BC47BE815F83EBA98004F63C883E6D71FF5CC929CB5F9CA", null, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "admin" });

            migrationBuilder.InsertData(
                table: "mes_user_role",
                columns: new[] { "id", "created_at", "created_by", "role_id", "updated_at", "updated_by", "user_id" },
                values: new object[] { 1L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0L, 1L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 1L });

            migrationBuilder.InsertData(
                table: "mes_workshop",
                columns: new[] { "id", "code", "created_at", "created_by", "factory_id", "name", "status", "updated_at", "updated_by" },
                values: new object[] { 1L, "WS-001", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0L, 1L, "总装车间", true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null });

            migrationBuilder.InsertData(
                table: "mes_production_line",
                columns: new[] { "id", "code", "created_at", "created_by", "name", "status", "updated_at", "updated_by", "workshop_id" },
                values: new object[] { 1L, "LINE-001", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0L, "生产线 A", true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 1L });

            migrationBuilder.InsertData(
                table: "mes_workstation",
                columns: new[] { "id", "code", "created_at", "created_by", "line_id", "name", "seq_no", "status", "updated_at", "updated_by" },
                values: new object[,]
                {
                    { 1L, "WS-001-01", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0L, 1L, "工位 1", 1, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 2L, "WS-001-02", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0L, 1L, "工位 2", 2, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 3L, "WS-001-03", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0L, 1L, "工位 3", 3, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null }
                });

            migrationBuilder.CreateIndex(
                name: "ix_mes_andon_events_triggered_at",
                table: "mes_andon_events",
                column: "triggered_at");

            migrationBuilder.CreateIndex(
                name: "ix_mes_andon_events_work_order_id",
                table: "mes_andon_events",
                column: "work_order_id");

            migrationBuilder.CreateIndex(
                name: "ix_mes_bom_material_id",
                table: "mes_bom",
                column: "material_id");

            migrationBuilder.CreateIndex(
                name: "ix_mes_bom_product_id",
                table: "mes_bom",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_mes_equipment_code",
                table: "mes_equipment",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_mes_factory_code",
                table: "mes_factory",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_mes_maintenance_plan_equipment_id",
                table: "mes_maintenance_plan",
                column: "equipment_id");

            migrationBuilder.CreateIndex(
                name: "ix_mes_maintenance_plan_equipment_id1",
                table: "mes_maintenance_plan",
                column: "equipment_id1");

            migrationBuilder.CreateIndex(
                name: "ix_mes_material_code",
                table: "mes_material",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_mes_material_trace_batch_no",
                table: "mes_material_trace",
                column: "batch_no");

            migrationBuilder.CreateIndex(
                name: "ix_mes_material_trace_material_id",
                table: "mes_material_trace",
                column: "material_id");

            migrationBuilder.CreateIndex(
                name: "ix_mes_material_trace_serial_no",
                table: "mes_material_trace",
                column: "serial_no");

            migrationBuilder.CreateIndex(
                name: "ix_mes_production_line_workshop_id_code",
                table: "mes_production_line",
                columns: new[] { "workshop_id", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_mes_qc_checkpoint_step_id_check_type",
                table: "mes_qc_checkpoint",
                columns: new[] { "step_id", "check_type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_mes_qc_inspection_inspect_no",
                table: "mes_qc_inspection",
                column: "inspect_no",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_mes_qc_inspection_item_inspection_id",
                table: "mes_qc_inspection_item",
                column: "inspection_id");

            migrationBuilder.CreateIndex(
                name: "ix_mes_role_name",
                table: "mes_role",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_mes_role_permission_role_id_permission",
                table: "mes_role_permission",
                columns: new[] { "role_id", "permission" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_mes_routing_material_id",
                table: "mes_routing",
                column: "material_id");

            migrationBuilder.CreateIndex(
                name: "ix_mes_routing_step_routing_id_step_no",
                table: "mes_routing_step",
                columns: new[] { "routing_id", "step_no" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_mes_user_username",
                table: "mes_user",
                column: "username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_mes_user_role_role_id",
                table: "mes_user_role",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "ix_mes_user_role_user_id_role_id",
                table: "mes_user_role",
                columns: new[] { "user_id", "role_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_mes_work_order_material_id",
                table: "mes_work_order",
                column: "material_id");

            migrationBuilder.CreateIndex(
                name: "ix_mes_work_order_order_no",
                table: "mes_work_order",
                column: "order_no",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_mes_work_order_routing_id",
                table: "mes_work_order",
                column: "routing_id");

            migrationBuilder.CreateIndex(
                name: "ix_mes_work_order_status",
                table: "mes_work_order",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_mes_work_order_step_work_order_id_step_no",
                table: "mes_work_order_step",
                columns: new[] { "work_order_id", "step_no" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_mes_work_order_step_workstation_id",
                table: "mes_work_order_step",
                column: "workstation_id");

            migrationBuilder.CreateIndex(
                name: "ix_mes_work_report_report_no",
                table: "mes_work_report",
                column: "report_no",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_mes_work_report_step_id",
                table: "mes_work_report",
                column: "step_id");

            migrationBuilder.CreateIndex(
                name: "ix_mes_work_report_work_order_id",
                table: "mes_work_report",
                column: "work_order_id");

            migrationBuilder.CreateIndex(
                name: "ix_mes_work_report_workstation_id",
                table: "mes_work_report",
                column: "workstation_id");

            migrationBuilder.CreateIndex(
                name: "ix_mes_workshop_factory_id_code",
                table: "mes_workshop",
                columns: new[] { "factory_id", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_mes_workstation_line_id_code",
                table: "mes_workstation",
                columns: new[] { "line_id", "code" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "mes_alert_record");

            migrationBuilder.DropTable(
                name: "mes_alert_rule");

            migrationBuilder.DropTable(
                name: "mes_andon_events");

            migrationBuilder.DropTable(
                name: "mes_bom");

            migrationBuilder.DropTable(
                name: "mes_knowledge_entry");

            migrationBuilder.DropTable(
                name: "mes_maintenance_plan");

            migrationBuilder.DropTable(
                name: "mes_material_trace");

            migrationBuilder.DropTable(
                name: "mes_qc_checkpoint");

            migrationBuilder.DropTable(
                name: "mes_qc_inspection_item");

            migrationBuilder.DropTable(
                name: "mes_role_permission");

            migrationBuilder.DropTable(
                name: "mes_routing_step");

            migrationBuilder.DropTable(
                name: "mes_user_role");

            migrationBuilder.DropTable(
                name: "mes_work_report");

            migrationBuilder.DropTable(
                name: "mes_equipment");

            migrationBuilder.DropTable(
                name: "mes_qc_inspection");

            migrationBuilder.DropTable(
                name: "mes_role");

            migrationBuilder.DropTable(
                name: "mes_user");

            migrationBuilder.DropTable(
                name: "mes_work_order_step");

            migrationBuilder.DropTable(
                name: "mes_work_order");

            migrationBuilder.DropTable(
                name: "mes_workstation");

            migrationBuilder.DropTable(
                name: "mes_routing");

            migrationBuilder.DropTable(
                name: "mes_production_line");

            migrationBuilder.DropTable(
                name: "mes_material");

            migrationBuilder.DropTable(
                name: "mes_workshop");

            migrationBuilder.DropTable(
                name: "mes_factory");
        }
    }
}
