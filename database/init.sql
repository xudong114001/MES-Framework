-- ==================== 基础数据 ====================

CREATE TABLE mes_factory (
    id BIGSERIAL PRIMARY KEY,
    code VARCHAR(50) NOT NULL UNIQUE,
    name VARCHAR(200) NOT NULL,
    address VARCHAR(500),
    status SMALLINT DEFAULT 1,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    created_by BIGINT,
    updated_at TIMESTAMPTZ DEFAULT NOW(),
    updated_by BIGINT,
    is_deleted SMALLINT DEFAULT 0
);

CREATE TABLE mes_workshop (
    id BIGSERIAL PRIMARY KEY,
    factory_id BIGINT NOT NULL REFERENCES mes_factory(id),
    code VARCHAR(50) NOT NULL,
    name VARCHAR(200) NOT NULL,
    status SMALLINT DEFAULT 1,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    created_by BIGINT,
    updated_at TIMESTAMPTZ DEFAULT NOW(),
    updated_by BIGINT,
    is_deleted SMALLINT DEFAULT 0,
    UNIQUE(factory_id, code)
);

CREATE TABLE mes_production_line (
    id BIGSERIAL PRIMARY KEY,
    workshop_id BIGINT NOT NULL REFERENCES mes_workshop(id),
    code VARCHAR(50) NOT NULL,
    name VARCHAR(200) NOT NULL,
    line_type VARCHAR(50) DEFAULT 'FLOW',
    status SMALLINT DEFAULT 1,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    created_by BIGINT,
    updated_at TIMESTAMPTZ DEFAULT NOW(),
    updated_by BIGINT,
    is_deleted SMALLINT DEFAULT 0,
    UNIQUE(workshop_id, code)
);

CREATE TABLE mes_workstation (
    id BIGSERIAL PRIMARY KEY,
    line_id BIGINT NOT NULL REFERENCES mes_production_line(id),
    code VARCHAR(50) NOT NULL,
    name VARCHAR(200) NOT NULL,
    seq_no INT DEFAULT 0,
    status SMALLINT DEFAULT 1,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    created_by BIGINT,
    updated_at TIMESTAMPTZ DEFAULT NOW(),
    updated_by BIGINT,
    is_deleted SMALLINT DEFAULT 0,
    UNIQUE(line_id, code)
);

-- ==================== 物料与BOM ====================

CREATE TABLE mes_material (
    id BIGSERIAL PRIMARY KEY,
    code VARCHAR(100) NOT NULL UNIQUE,
    name VARCHAR(300) NOT NULL,
    spec VARCHAR(500),
    unit VARCHAR(20) NOT NULL,
    category VARCHAR(100),
    bom_level INT DEFAULT 0,
    status SMALLINT DEFAULT 1,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    created_by BIGINT,
    updated_at TIMESTAMPTZ DEFAULT NOW(),
    updated_by BIGINT,
    is_deleted SMALLINT DEFAULT 0
);

CREATE TABLE mes_bom (
    id BIGSERIAL PRIMARY KEY,
    product_id BIGINT NOT NULL REFERENCES mes_material(id),
    material_id BIGINT NOT NULL REFERENCES mes_material(id),
    quantity DECIMAL(18,6) NOT NULL,
    scrap_rate DECIMAL(5,2) DEFAULT 0,
    seq_no INT DEFAULT 0,
    valid_from DATE,
    valid_to DATE,
    status SMALLINT DEFAULT 1,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    created_by BIGINT,
    updated_at TIMESTAMPTZ DEFAULT NOW(),
    updated_by BIGINT,
    is_deleted SMALLINT DEFAULT 0
);

-- ==================== 工艺路线 ====================

CREATE TABLE mes_routing (
    id BIGSERIAL PRIMARY KEY,
    material_id BIGINT NOT NULL REFERENCES mes_material(id),
    routing_code VARCHAR(50) NOT NULL,
    routing_name VARCHAR(200),
    version INT DEFAULT 1,
    status SMALLINT DEFAULT 1,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    created_by BIGINT,
    updated_at TIMESTAMPTZ DEFAULT NOW(),
    updated_by BIGINT,
    is_deleted SMALLINT DEFAULT 0
);

CREATE TABLE mes_routing_step (
    id BIGSERIAL PRIMARY KEY,
    routing_id BIGINT NOT NULL REFERENCES mes_routing(id),
    step_no INT NOT NULL,
    step_name VARCHAR(200) NOT NULL,
    workstation_type VARCHAR(100),
    standard_time DECIMAL(10,2),
    is_qc_point SMALLINT DEFAULT 0,
    is_scrap_point SMALLINT DEFAULT 0,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    UNIQUE(routing_id, step_no)
);

-- ==================== 工单 ====================

CREATE TABLE mes_work_order (
    id BIGSERIAL PRIMARY KEY,
    order_no VARCHAR(50) NOT NULL UNIQUE,
    source_type VARCHAR(20) DEFAULT 'MANUAL',
    source_ref VARCHAR(100),
    material_id BIGINT NOT NULL REFERENCES mes_material(id),
    routing_id BIGINT REFERENCES mes_routing(id),
    planned_qty DECIMAL(18,2) NOT NULL,
    completed_qty DECIMAL(18,2) DEFAULT 0,
    scrap_qty DECIMAL(18,2) DEFAULT 0,
    status VARCHAR(30) NOT NULL DEFAULT 'PENDING',
    plan_start_time TIMESTAMPTZ,
    plan_end_time TIMESTAMPTZ,
    actual_start_time TIMESTAMPTZ,
    actual_end_time TIMESTAMPTZ,
    priority INT DEFAULT 0,
    factory_id BIGINT,
    workshop_id BIGINT,
    line_id BIGINT,
    assignee VARCHAR(100),
    remark TEXT,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    created_by BIGINT,
    updated_at TIMESTAMPTZ DEFAULT NOW(),
    updated_by BIGINT,
    is_deleted SMALLINT DEFAULT 0
);

CREATE TABLE mes_work_order_step (
    id BIGSERIAL PRIMARY KEY,
    work_order_id BIGINT NOT NULL REFERENCES mes_work_order(id),
    step_no INT NOT NULL,
    step_name VARCHAR(200) NOT NULL,
    workstation_id BIGINT REFERENCES mes_workstation(id),
    planned_qty DECIMAL(18,2) NOT NULL,
    completed_qty DECIMAL(18,2) DEFAULT 0,
    scrap_qty DECIMAL(18,2) DEFAULT 0,
    status VARCHAR(30) DEFAULT 'PENDING',
    plan_start_time TIMESTAMPTZ,
    plan_end_time TIMESTAMPTZ,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    UNIQUE(work_order_id, step_no)
);

-- ==================== 报工 ====================

CREATE TABLE mes_work_report (
    id BIGSERIAL PRIMARY KEY,
    report_no VARCHAR(50) NOT NULL UNIQUE,
    work_order_id BIGINT NOT NULL REFERENCES mes_work_order(id),
    step_id BIGINT REFERENCES mes_work_order_step(id),
    workstation_id BIGINT REFERENCES mes_workstation(id),
    operator_id VARCHAR(100),
    report_type VARCHAR(20) NOT NULL DEFAULT 'COMPLETE',
    good_qty DECIMAL(18,2) DEFAULT 0,
    scrap_qty DECIMAL(18,2) DEFAULT 0,
    rework_qty DECIMAL(18,2) DEFAULT 0,
    duration_min INT,
    report_time TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    remark TEXT,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    created_by BIGINT
);

-- ==================== 质检 ====================

CREATE TABLE mes_qc_inspection (
    id BIGSERIAL PRIMARY KEY,
    inspect_no VARCHAR(50) NOT NULL UNIQUE,
    source_type VARCHAR(30) NOT NULL,
    source_ref VARCHAR(100),
    work_order_id BIGINT REFERENCES mes_work_order(id),
    material_id BIGINT REFERENCES mes_material(id),
    inspector VARCHAR(100),
    inspect_result VARCHAR(20) DEFAULT 'PENDING',
    inspect_time TIMESTAMPTZ,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    created_by BIGINT
);

CREATE TABLE mes_qc_inspection_item (
    id BIGSERIAL PRIMARY KEY,
    inspection_id BIGINT NOT NULL REFERENCES mes_qc_inspection(id),
    item_name VARCHAR(200) NOT NULL,
    spec_value VARCHAR(200),
    actual_value VARCHAR(200),
    result VARCHAR(20) DEFAULT 'PENDING',
    created_at TIMESTAMPTZ DEFAULT NOW()
);

-- ==================== 追溯 ====================

CREATE TABLE mes_material_trace (
    id BIGSERIAL PRIMARY KEY,
    material_id BIGINT NOT NULL REFERENCES mes_material(id),
    batch_no VARCHAR(100) NOT NULL,
    serial_no VARCHAR(100),
    work_order_id BIGINT REFERENCES mes_work_order(id),
    src_work_order_id BIGINT,
    direction VARCHAR(10) NOT NULL,
    qty DECIMAL(18,2) NOT NULL,
    operator VARCHAR(100),
    operate_time TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    remark TEXT,
    created_at TIMESTAMPTZ DEFAULT NOW()
);

-- ==================== 设备 ====================

CREATE TABLE mes_equipment (
    id BIGSERIAL PRIMARY KEY,
    code VARCHAR(50) NOT NULL UNIQUE,
    name VARCHAR(200) NOT NULL,
    model VARCHAR(200),
    factory_id BIGINT REFERENCES mes_factory(id),
    workshop_id BIGINT,
    line_id BIGINT,
    install_date DATE,
    status VARCHAR(30) DEFAULT 'IDLE',
    last_maintain_date TIMESTAMPTZ,
    next_maintain_date TIMESTAMPTZ,
    maintain_cycle INT,
    theoretical_cycle_time DOUBLE PRECISION,    -- 理论节拍（秒/件）
    planned_run_time DOUBLE PRECISION,          -- 日计划运行时间（小时）
    created_at TIMESTAMPTZ DEFAULT NOW(),
    created_by BIGINT,
    updated_at TIMESTAMPTZ DEFAULT NOW(),
    updated_by BIGINT,
    is_deleted SMALLINT DEFAULT 0
);

-- ==================== 保养计划 ====================

CREATE TABLE mes_maintenance_plan (
    id BIGSERIAL PRIMARY KEY,
    equipment_id BIGINT NOT NULL REFERENCES mes_equipment(id),
    plan_name VARCHAR(200) NOT NULL,
    cycle_days INT NOT NULL,
    last_completed_date TIMESTAMPTZ,
    next_due_date TIMESTAMPTZ NOT NULL,
    description TEXT,
    status INT DEFAULT 0,                       -- 0=PENDING 1=COMPLETED 2=OVERDUE
    created_at TIMESTAMPTZ DEFAULT NOW(),
    created_by BIGINT,
    updated_at TIMESTAMPTZ DEFAULT NOW(),
    updated_by BIGINT,
    is_deleted SMALLINT DEFAULT 0
);

CREATE INDEX idx_maintenance_plan_equipment ON mes_maintenance_plan(equipment_id);

-- ==================== 用户 ====================

CREATE TABLE mes_user (
    id BIGSERIAL PRIMARY KEY,
    username VARCHAR(100) NOT NULL UNIQUE,
    password_hash VARCHAR(500) NOT NULL,
    display_name VARCHAR(200),
    email VARCHAR(200),
    phone VARCHAR(50),
    status SMALLINT DEFAULT 1,
    last_login_time TIMESTAMPTZ,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    created_by BIGINT,
    updated_at TIMESTAMPTZ DEFAULT NOW(),
    updated_by BIGINT,
    is_deleted SMALLINT DEFAULT 0
);

-- ==================== 索引 ====================

CREATE INDEX idx_workshop_factory ON mes_workshop(factory_id);
CREATE INDEX idx_line_workshop ON mes_production_line(workshop_id);
CREATE INDEX idx_ws_line ON mes_workstation(line_id);
CREATE INDEX idx_bom_product ON mes_bom(product_id);
CREATE INDEX idx_bom_material ON mes_bom(material_id);
CREATE INDEX idx_routing_material ON mes_routing(material_id);
CREATE INDEX idx_routing_step_routing ON mes_routing_step(routing_id);
CREATE INDEX idx_wo_material ON mes_work_order(material_id);
CREATE INDEX idx_wo_status ON mes_work_order(status);
CREATE INDEX idx_wo_created ON mes_work_order(created_at);
CREATE INDEX idx_wos_work_order ON mes_work_order_step(work_order_id);
CREATE INDEX idx_wos_status ON mes_work_order_step(status);
CREATE INDEX idx_wr_work_order ON mes_work_report(work_order_id);
CREATE INDEX idx_wr_report_time ON mes_work_report(report_time);
CREATE INDEX idx_qc_work_order ON mes_qc_inspection(work_order_id);
CREATE INDEX idx_qc_result ON mes_qc_inspection(inspect_result);
CREATE INDEX idx_qcitem_inspection ON mes_qc_inspection_item(inspection_id);
CREATE INDEX idx_trace_batch ON mes_material_trace(batch_no);
CREATE INDEX idx_trace_serial ON mes_material_trace(serial_no);
CREATE INDEX idx_trace_material ON mes_material_trace(material_id);
CREATE INDEX idx_equipment_code ON mes_equipment(code);
CREATE INDEX idx_user_username ON mes_user(username);

-- Work Report composite indexes for step-level reporting
CREATE INDEX idx_work_report_work_order_step ON mes_work_report(work_order_id, step_id);
CREATE INDEX idx_work_report_operator ON mes_work_report(operator_id);
CREATE INDEX idx_work_report_batch ON mes_work_report(batch_no);

-- Material Trace for work order traceability
CREATE INDEX idx_material_trace_work_order ON mes_material_trace(work_order_id);

-- Equipment status filtering
CREATE INDEX idx_equipment_status ON mes_equipment(status);

-- Maintenance plan overdue queries
CREATE INDEX idx_maintenance_plan_status_due ON mes_maintenance_plan(status, next_due_date);

-- Dashboard queries: status + line + priority
CREATE INDEX idx_work_order_status_line_priority ON mes_work_order(status, line_id, priority);

-- ==================== 种子数据 ====================

INSERT INTO mes_user (username, password_hash, display_name, status) VALUES
('admin', '$2a$11$K4YfGqJ1e4YHIYHIYHIYHuDummyHashForAdmin2026', '系统管理员', 1);

INSERT INTO mes_factory (code, name, address) VALUES
('FACTORY-001', 'Demo Factory', '中国广东省深圳市南山区科技园');

INSERT INTO mes_workshop (factory_id, code, name) VALUES
(1, 'WS-SMT', 'SMT车间'),
(1, 'WS-ASSEMBLY', '组装车间');

INSERT INTO mes_production_line (workshop_id, code, name, line_type) VALUES
(1, 'SMT-LINE-01', 'SMT生产线1号', 'FLOW'),
(1, 'SMT-LINE-02', 'SMT生产线2号', 'FLOW'),
(2, 'ASSY-LINE-01', '组装线1号', 'FLOW'),
(2, 'ASSY-LINE-02', '组装线2号', 'FLOW');

INSERT INTO mes_workstation (line_id, code, name, seq_no) VALUES
(1, 'SMT-01-01', '印刷工位', 10),
(1, 'SMT-01-02', '贴片工位', 20),
(1, 'SMT-01-03', '回流焊工位', 30),
(2, 'SMT-02-01', '印刷工位', 10),
(2, 'SMT-02-02', '贴片工位', 20),
(2, 'SMT-02-03', '回流焊工位', 30),
(3, 'ASSY-01-01', '总装工位', 10),
(3, 'ASSY-01-02', '测试工位', 20),
(3, 'ASSY-01-03', '包装工位', 30),
(4, 'ASSY-02-01', '总装工位', 10),
(4, 'ASSY-02-02', '测试工位', 20),
(4, 'ASSY-02-03', '包装工位', 30);

INSERT INTO mes_material (code, name, spec, unit, category, bom_level) VALUES
('FG-A001', '智能控制器成品', 'IC-100 v2.0', 'PCS', '成品', 0),
('RM-B001', 'PCB主板', 'PCB-200x150mm', 'PCS', '电子物料', 1),
('RM-B002', '芯片组', 'CHIP-SET-A1', 'PCS', '电子物料', 1),
('RM-B003', '外壳套件', 'CASE-KIT-A1', '套', '结构件', 1);

-- ==================== BOM 种子数据 ====================
-- FG-A001（智能控制器成品）的 BOM：关联子件 RM-B001, RM-B002, RM-B003
-- product_id=1 (FG-A001), material_id 引用子件物料
INSERT INTO mes_bom (product_id, material_id, quantity, scrap_rate, seq_no, valid_from, valid_to, status) VALUES
(1, 2, 1.000000, 1.00, 10, '2025-01-01', '2025-12-31', 1),  -- FG-A001 包含 1块 PCB主板
(1, 3, 2.000000, 0.50, 20, '2025-01-01', '2025-12-31', 1),  -- FG-A001 包含 2个 芯片组
(1, 4, 1.000000, 2.00, 30, '2025-01-01', '2025-12-31', 1);  -- FG-A001 包含 1套 外壳套件

-- ==================== 工艺路线 种子数据 ====================
-- FG-A001（智能控制器成品）的工艺路线
INSERT INTO mes_routing (id, material_id, routing_code, routing_name, version, status) VALUES
(1, 1, 'RT-FG-A001-01', '智能控制器标准工艺路线', 1, 1);

-- 工艺路线工序步骤
INSERT INTO mes_routing_step (routing_id, step_no, step_name, workstation_type, standard_time, is_qc_point, is_scrap_point) VALUES
(1, 10, 'SMT印刷',  'SMT-PRINT',     5.00,  0, 0),
(1, 20, 'SMT贴片',  'SMT-PICK',     12.00,  0, 0),
(1, 30, '回流焊',   'REFLOW',        8.00,  0, 0),
(1, 40, 'AOI检测',  'AOI',           6.00,  1, 0),   -- 质检点
(1, 50, '总装',     'ASSEMBLY',     15.00,  0, 0),
(1, 60, '功能测试', 'FUNC-TEST',     10.00,  1, 0),   -- 质检点
(1, 70, '包装',     'PACKAGING',      5.00,  0, 1);   -- 报废点

-- ==================== 设备 种子数据 ====================
INSERT INTO mes_equipment (code, name, model, factory_id, workshop_id, line_id, install_date, status, theoretical_cycle_time, planned_run_time, maintain_cycle) VALUES
('EQ-SMT-01', 'SMT印刷机1号',   'DEK-03X',      1, 1, 1, '2024-03-15', 'RUNNING', 5.0,  20.0, 90),
('EQ-SMT-02', 'SMT贴片机1号',   'SM481+Venus',  1, 1, 1, '2024-03-15', 'RUNNING', 3.0,  20.0, 60),
('EQ-SMT-03', '回流焊炉1号',    'BTU-PYRAX',    1, 1, 1, '2024-03-15', 'RUNNING', 8.0,  20.0, 120),
('EQ-SMT-04', 'AOI检测仪1号',   'OMRON-VT',     1, 1, 1, '2024-06-01', 'IDLE',    6.0,  20.0, 90),
('EQ-ASSY-01', '组装工装台1号',  'CUSTOM-ASSY',  1, 2, 3, '2024-03-15', 'RUNNING', 15.0, 20.0, 180),
('EQ-ASSY-02', '功能测试台1号',  'CUSTOM-FT',    1, 2, 3, '2024-03-15', 'RUNNING', 10.0, 20.0, 180),
('EQ-PKG-01',  '包装机1号',      'SEMI-AUTO-PK', 1, 2, 3, '2024-03-15', 'IDLE',    5.0,  20.0, 120);

-- ==================== 工单 种子数据 ====================
-- 工单1：PENDING 状态，计划生产 100 件
INSERT INTO mes_work_order (order_no, source_type, material_id, routing_id, planned_qty, completed_qty, scrap_qty, status, plan_start_time, plan_end_time, priority, factory_id, workshop_id, line_id, assignee, remark) VALUES
('WO-2025-0001', 'MANUAL', 1, 1, 100, 0, 0, 'PENDING',
 '2025-07-01 08:00:00+08', '2025-07-03 18:00:00+08',
 5, 1, 1, 1, 'zhangsan', '智能控制器首批试产');

-- 工单2：IN_PROGRESS 状态，计划生产 500 件，已完成 200 件
INSERT INTO mes_work_order (order_no, source_type, material_id, routing_id, planned_qty, completed_qty, scrap_qty, status, plan_start_time, plan_end_time, actual_start_time, priority, factory_id, workshop_id, line_id, assignee, remark) VALUES
('WO-2025-0002', 'MANUAL', 1, 1, 500, 200, 5, 'IN_PROGRESS',
 '2025-06-25 08:00:00+08', '2025-06-30 18:00:00+08', '2025-06-25 08:15:00+08',
 10, 1, 2, 3, 'lisi', '智能控制器量产订单');

-- 工单2 的工序进度（已完成前4道工序，第5道进行中）
INSERT INTO mes_work_order_step (work_order_id, step_no, step_name, workstation_id, planned_qty, completed_qty, scrap_qty, status, plan_start_time, plan_end_time) VALUES
(2, 10, 'SMT印刷',  1, 500, 500, 0,  'COMPLETED', '2025-06-25 08:00:00+08', '2025-06-25 12:00:00+08'),
(2, 20, 'SMT贴片',  2, 500, 500, 2,  'COMPLETED', '2025-06-25 13:00:00+08', '2025-06-26 10:00:00+08'),
(2, 30, '回流焊',   3, 500, 500, 1,  'COMPLETED', '2025-06-26 11:00:00+08', '2025-06-26 18:00:00+08'),
(2, 40, 'AOI检测',  4, 500, 500, 2,  'COMPLETED', '2025-06-27 08:00:00+08', '2025-06-27 17:00:00+08'),
(2, 50, '总装',     7, 500, 200, 0,  'IN_PROGRESS', '2025-06-28 08:00:00+08', '2025-06-29 18:00:00+08'),
(2, 60, '功能测试', 8, 500, 0,   0,  'PENDING', '2025-06-30 08:00:00+08', '2025-06-30 17:00:00+08'),
(2, 70, '包装',     9, 500, 0,   0,  'PENDING', '2025-07-01 08:00:00+08', '2025-07-01 17:00:00+08');

-- ==================== AI 预警记录表 ====================
CREATE TABLE IF NOT EXISTS mes_ai_alerts (
    id                      BIGINT PRIMARY KEY,
    rule_name               VARCHAR(100) NOT NULL,
    title                   VARCHAR(200) NOT NULL,
    message                 TEXT,
    level                   INT NOT NULL DEFAULT 0,
    related_entity_type     VARCHAR(50),
    related_entity_id       BIGINT,
    is_processed            BOOLEAN DEFAULT FALSE,
    processed_by            VARCHAR(50),
    processed_at            TIMESTAMPTZ,
    created_at              TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_ai_alerts_created_at ON mes_ai_alerts(created_at DESC);
CREATE INDEX IF NOT EXISTS idx_ai_alerts_is_processed ON mes_ai_alerts(is_processed);
CREATE INDEX IF NOT EXISTS idx_ai_alerts_level ON mes_ai_alerts(level);

-- ==================== RBAC 权限管理 ====================

CREATE TABLE mes_role (
    id BIGSERIAL PRIMARY KEY,
    name VARCHAR(50) NOT NULL UNIQUE,
    description VARCHAR(200),
    created_at TIMESTAMPTZ DEFAULT NOW(),
    created_by BIGINT,
    updated_at TIMESTAMPTZ DEFAULT NOW(),
    updated_by BIGINT,
    is_deleted SMALLINT DEFAULT 0
);

CREATE TABLE mes_user_role (
    id BIGSERIAL PRIMARY KEY,
    user_id BIGINT NOT NULL REFERENCES mes_user(id),
    role_id BIGINT NOT NULL REFERENCES mes_role(id),
    created_at TIMESTAMPTZ DEFAULT NOW(),
    created_by BIGINT,
    updated_at TIMESTAMPTZ DEFAULT NOW(),
    updated_by BIGINT,
    is_deleted SMALLINT DEFAULT 0,
    UNIQUE(user_id, role_id)
);

CREATE INDEX IF NOT EXISTS idx_user_role_user ON mes_user_role(user_id);
CREATE INDEX IF NOT EXISTS idx_user_role_role ON mes_user_role(role_id);

-- ==================== 知识库 ====================

CREATE TABLE mes_knowledge_entry (
    id BIGSERIAL PRIMARY KEY,
    category INT NOT NULL DEFAULT 0,
    title VARCHAR(300) NOT NULL,
    content TEXT NOT NULL,
    keywords VARCHAR(500),
    material_id BIGINT,
    equipment_id BIGINT,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    created_by BIGINT,
    updated_at TIMESTAMPTZ DEFAULT NOW(),
    updated_by BIGINT,
    is_deleted SMALLINT DEFAULT 0
);
