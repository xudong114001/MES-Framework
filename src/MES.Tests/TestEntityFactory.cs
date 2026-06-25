using MES.Domain.Entities;
using MES.Domain.Enums;
using MES.Domain.ValueObjects;
using MES.AI.Domain.Entities;
using MES.AI.Domain.Enums;
using System.Reflection;

namespace MES.Tests;

/// <summary>
/// 测试辅助类，提供使用反射创建充血模型实体的方法
/// </summary>
public static class TestEntityFactory
{
    #region WorkOrder

    /// <summary>
    /// 创建 WorkOrder（使用工厂方法，状态为 PENDING）
    /// </summary>
    public static WorkOrder CreateWorkOrder(
        string orderNo = "WO-TEST-001",
        long materialId = 1,
        decimal plannedQty = 100,
        SourceType sourceType = SourceType.MANUAL,
        long? routingId = null,
        Priority priority = Priority.NORMAL)
    {
        return WorkOrder.Create(
            orderNo: orderNo,
            sourceType: sourceType,
            materialId: materialId,
            plannedQty: new Quantity(plannedQty),
            priority: priority,
            routingId: routingId
        );
    }

    /// <summary>
    /// 创建 WorkOrder 并设置特定状态（使用反射绕过验证）
    /// </summary>
    public static WorkOrder CreateWorkOrderWithStatus(
        string orderNo,
        long materialId,
        decimal plannedQty,
        WorkOrderStatus status,
        decimal completedQty = 0,
        decimal scrapQty = 0,
        long? routingId = null,
        long? lineId = null,
        Priority priority = Priority.NORMAL)
    {
        var wo = WorkOrder.Create(
            orderNo: orderNo,
            sourceType: SourceType.MANUAL,
            materialId: materialId,
            plannedQty: new Quantity(plannedQty),
            priority: priority,
            routingId: routingId
        );
        SetProperty(wo, "Status", status);
        SetProperty(wo, "CompletedQty", new Quantity(completedQty));
        SetProperty(wo, "ScrapQty", new Quantity(scrapQty));
        if (lineId.HasValue)
            SetProperty(wo, "LineId", lineId.Value);
        return wo;
    }

    /// <summary>
    /// 使用反射直接创建 WorkOrder（绕过所有验证，用于集成测试）
    /// </summary>
    public static WorkOrder CreateWorkOrderDirect(
        long id = 1,
        string orderNo = "WO-TEST-001",
        long materialId = 1,
        decimal plannedQty = 100,
        decimal completedQty = 0,
        decimal scrapQty = 0,
        WorkOrderStatus status = WorkOrderStatus.PENDING,
        Priority priority = Priority.NORMAL,
        long? routingId = null,
        long? lineId = null,
        long? factoryId = null,
        long? workshopId = null,
        string? remark = null,
        long? reworkFromId = null)
    {
        // 使用私有无参构造函数
        var ctor = typeof(WorkOrder).GetConstructor(
            BindingFlags.Instance | BindingFlags.NonPublic,
            null,
            Type.EmptyTypes,
            null)!;

        var wo = (WorkOrder)ctor.Invoke(null);

        // 设置 Id (BaseEntity)
        SetProperty(wo, "Id", id);

        // 设置所有属性
        SetProperty(wo, "OrderNo", orderNo);
        SetProperty(wo, "SourceType", SourceType.MANUAL);
        SetProperty(wo, "SourceRef", null);
        SetProperty(wo, "MaterialId", materialId);
        SetProperty(wo, "RoutingId", routingId);
        SetProperty(wo, "PlannedQty", new Quantity(plannedQty));
        SetProperty(wo, "CompletedQty", new Quantity(completedQty));
        SetProperty(wo, "ScrapQty", new Quantity(scrapQty));
        SetProperty(wo, "Status", status);
        SetProperty(wo, "Priority", priority);
        SetProperty(wo, "PlanStartTime", null);
        SetProperty(wo, "PlanEndTime", null);
        SetProperty(wo, "ActualStartTime", null);
        SetProperty(wo, "ActualEndTime", null);
        SetProperty(wo, "FactoryId", factoryId);
        SetProperty(wo, "WorkshopId", workshopId);
        SetProperty(wo, "LineId", lineId);
        SetProperty(wo, "Assignee", null);
        SetProperty(wo, "Remark", remark);
        SetProperty(wo, "ReworkFromId", reworkFromId);
        SetProperty(wo, "CreatedAt", DateTime.UtcNow);
        SetProperty(wo, "UpdatedAt", DateTime.UtcNow);

        return wo;
    }

    #endregion

    #region WorkOrderStep

    /// <summary>
    /// 创建 WorkOrderStep
    /// </summary>
    public static WorkOrderStep CreateWorkOrderStep(
        long workOrderId = 1,
        int stepNo = 1,
        string stepName = "Test Step",
        decimal plannedQty = 100)
    {
        return WorkOrderStep.Create(
            workOrderId: workOrderId,
            stepNo: stepNo,
            stepName: stepName,
            plannedQty: plannedQty
        );
    }

    /// <summary>
    /// 使用反射直接创建 WorkOrderStep
    /// </summary>
    public static WorkOrderStep CreateWorkOrderStepDirect(
        long id = 1,
        long workOrderId = 1,
        int stepNo = 1,
        string stepName = "Test Step",
        decimal plannedQty = 100,
        decimal completedQty = 0,
        decimal scrapQty = 0,
        WorkOrderStatus status = WorkOrderStatus.PENDING,
        long? workstationId = null)
    {
        var ctor = typeof(WorkOrderStep).GetConstructor(
            BindingFlags.Instance | BindingFlags.NonPublic,
            null,
            Type.EmptyTypes,
            null)!;

        var step = (WorkOrderStep)ctor.Invoke(null);

        SetProperty(step, "Id", id);
        SetProperty(step, "WorkOrderId", workOrderId);
        SetProperty(step, "StepNo", stepNo);
        SetProperty(step, "StepName", stepName);
        SetProperty(step, "WorkstationId", workstationId);
        SetProperty(step, "PlannedQty", plannedQty);
        SetProperty(step, "CompletedQty", completedQty);
        SetProperty(step, "ScrapQty", scrapQty);
        SetProperty(step, "Status", status);
        SetProperty(step, "PlanStartTime", null);
        SetProperty(step, "PlanEndTime", null);
        SetProperty(step, "CreatedAt", DateTime.UtcNow);
        SetProperty(step, "UpdatedAt", DateTime.UtcNow);

        return step;
    }

    #endregion

    #region Material

    /// <summary>
    /// 创建 Material
    /// </summary>
    public static Material CreateMaterial(
        long id = 1,
        string code = "MAT-001",
        string name = "Test Material",
        decimal stockQty = 1000,
        bool status = true)
    {
        var material = Material.Create(code, name);
        SetProperty(material, "Id", id);
        SetProperty(material, "StockQty", stockQty);
        SetProperty(material, "Status", status);
        return material;
    }

    #endregion

    #region Bom

    /// <summary>
    /// 创建 Bom
    /// </summary>
    public static Bom CreateBom(
        long productId = 1,
        long materialId = 2,
        decimal quantity = 1,
        bool status = true)
    {
        var bom = Bom.Create(productId, materialId, new Quantity(quantity), 0, 1);
        SetProperty(bom, "Id", 1);
        SetProperty(bom, "Status", status);
        return bom;
    }

    #endregion

    #region Routing

    /// <summary>
    /// 创建 Routing
    /// </summary>
    public static Routing CreateRouting(
        long materialId = 1,
        string routingCode = "R-TEST-001",
        string routingName = "Test Routing")
    {
        return Routing.Create(
            materialId: materialId,
            routingCode: routingCode,
            routingName: routingName
        );
    }

    /// <summary>
    /// 创建带工序的 Routing
    /// </summary>
    public static Routing CreateRoutingWithSteps(
        long materialId,
        string routingCode,
        string routingName,
        params (int stepNo, string stepName)[] steps)
    {
        var routing = CreateRouting(materialId, routingCode, routingName);

        foreach (var (stepNo, stepName) in steps)
        {
            var routingStep = RoutingStep.Create(
                routingId: 0, // 会被 AddStep 设置
                stepName: stepName,
                stepNo: stepNo,
                standardTime: 60
            );
            routing.AddStep(routingStep);
        }

        return routing;
    }

    #endregion

    #region RoutingStep

    /// <summary>
    /// 创建 RoutingStep
    /// </summary>
    public static RoutingStep CreateRoutingStep(
        long routingId = 1,
        int stepNo = 1,
        string stepName = "Test Step",
        decimal standardTime = 60)
    {
        return RoutingStep.Create(
            routingId: routingId,
            stepName: stepName,
            stepNo: stepNo,
            standardTime: standardTime
        );
    }

    #endregion

    #region Equipment

    /// <summary>
    /// 创建 Equipment
    /// </summary>
    public static Equipment CreateEquipment(
        string code = "EQ-001",
        string name = "Test Equipment",
        string? model = null,
        EquipmentStatus status = EquipmentStatus.IDLE)
    {
        var equipment = Equipment.Create(code, name, model);
        equipment.SetStatus(status);
        return equipment;
    }

    /// <summary>
    /// 使用反射直接创建 Equipment
    /// </summary>
    public static Equipment CreateEquipmentDirect(
        long id = 1,
        string code = "EQ-001",
        string name = "Test Equipment",
        string? model = null,
        EquipmentStatus status = EquipmentStatus.IDLE,
        long? lineId = null,
        double? theoreticalCycleTime = null,
        double? plannedRunTime = null)
    {
        var ctor = typeof(Equipment).GetConstructor(
            BindingFlags.Instance | BindingFlags.NonPublic,
            null,
            Type.EmptyTypes,
            null)!;

        var equipment = (Equipment)ctor.Invoke(null);

        SetProperty(equipment, "Id", id);
        SetProperty(equipment, "Code", code);
        SetProperty(equipment, "Name", name);
        SetProperty(equipment, "Model", model);
        SetProperty(equipment, "FactoryId", null);
        SetProperty(equipment, "WorkshopId", null);
        SetProperty(equipment, "LineId", lineId);
        SetProperty(equipment, "InstallDate", null);
        SetProperty(equipment, "Status", status);
        SetProperty(equipment, "LastMaintainDate", null);
        SetProperty(equipment, "NextMaintainDate", null);
        SetProperty(equipment, "MaintainCycle", null);
        SetProperty(equipment, "TheoreticalCycleTime", theoreticalCycleTime);
        SetProperty(equipment, "PlannedRunTime", plannedRunTime);
        SetProperty(equipment, "CreatedAt", DateTime.UtcNow);
        SetProperty(equipment, "UpdatedAt", DateTime.UtcNow);

        return equipment;
    }

    #endregion

    #region MaintenancePlan

    /// <summary>
    /// 创建 MaintenancePlan（检查是否有公共构造函数）
    /// </summary>
    public static MaintenancePlan CreateMaintenancePlan(
        long equipmentId = 1,
        string planName = "Test Plan",
        int cycleDays = 30)
    {
        return new MaintenancePlan(equipmentId, planName, cycleDays);
    }

    #endregion

    #region WorkReport

    /// <summary>
    /// 使用反射直接创建 WorkReport
    /// </summary>
    public static WorkReport CreateWorkReportDirect(
        long id = 1,
        string reportNo = "WR-TEST-001",
        long workOrderId = 1,
        long? stepId = null,
        long? workstationId = null,
        ReportType reportType = ReportType.COMPLETE,
        decimal goodQty = 0,
        decimal scrapQty = 0,
        decimal reworkQty = 0,
        int durationMin = 0,
        DateTime? reportTime = null)
    {
        var ctor = typeof(WorkReport).GetConstructor(
            BindingFlags.Instance | BindingFlags.NonPublic,
            null,
            Type.EmptyTypes,
            null)!;

        var report = (WorkReport)ctor.Invoke(null);

        SetProperty(report, "Id", id);
        SetProperty(report, "ReportNo", reportNo);
        SetProperty(report, "WorkOrderId", workOrderId);
        SetProperty(report, "StepId", stepId);
        SetProperty(report, "WorkstationId", workstationId);
        SetProperty(report, "ReportType", reportType);
        SetProperty(report, "GoodQty", new Quantity(goodQty));
        SetProperty(report, "ScrapQty", new Quantity(scrapQty));
        SetProperty(report, "ReworkQty", new Quantity(reworkQty));
        SetProperty(report, "DurationMin", durationMin);
        SetProperty(report, "ReportTime", reportTime ?? DateTime.UtcNow);
        SetProperty(report, "OperatorId", null);
        SetProperty(report, "BatchNo", null);
        SetProperty(report, "CreatedAt", DateTime.UtcNow);
        SetProperty(report, "UpdatedAt", DateTime.UtcNow);

        return report;
    }

    #endregion

    #region QcInspection

    /// <summary>
    /// 使用反射直接创建 QcInspection
    /// </summary>
    public static QcInspection CreateQcInspectionDirect(
        long id = 1,
        string inspectNo = "QC-TEST-001",
        QcInspectionType sourceType = QcInspectionType.FINAL,
        long? workOrderId = null,
        long? materialId = null,
        long? inspector = null,
        QcResult inspectResult = QcResult.PENDING)
    {
        var ctor = typeof(QcInspection).GetConstructor(
            BindingFlags.Instance | BindingFlags.NonPublic,
            null,
            Type.EmptyTypes,
            null)!;

        var inspection = (QcInspection)ctor.Invoke(null);

        SetProperty(inspection, "Id", id);
        SetProperty(inspection, "InspectNo", inspectNo);
        SetProperty(inspection, "SourceType", sourceType);
        SetProperty(inspection, "SourceRef", null);
        SetProperty(inspection, "WorkOrderId", workOrderId);
        SetProperty(inspection, "MaterialId", materialId);
        SetProperty(inspection, "Inspector", inspector);
        SetProperty(inspection, "InspectResult", inspectResult);
        SetProperty(inspection, "InspectTime", default(DateTime));
        SetProperty(inspection, "Remark", null);
        SetProperty(inspection, "HandlingAction", null);
        SetProperty(inspection, "HandlingRemark", null);
        SetProperty(inspection, "HandledAt", null);
        SetProperty(inspection, "CreatedAt", DateTime.UtcNow);
        SetProperty(inspection, "UpdatedAt", DateTime.UtcNow);

        return inspection;
    }

    #endregion

    #region AndonEvent

    /// <summary>
    /// 使用反射直接创建 AndonEvent
    /// </summary>
    public static AndonEvent CreateAndonEventDirect(
        long id = 1,
        AndonEventType eventType = AndonEventType.EQUIPMENT_FAULT,
        AndonEventLevel level = AndonEventLevel.Warning,
        string title = "Test Event",
        string? description = null,
        long? workstationId = null,
        string? workstationName = null,
        long? workOrderId = null,
        string? workOrderNo = null,
        long? triggeredById = null,
        string? triggeredByName = null,
        DateTime? triggeredAt = null,
        DateTime? resolvedAt = null,
        long? resolvedById = null,
        string? resolvedByName = null)
    {
        var ctor = typeof(AndonEvent).GetConstructor(
            BindingFlags.Instance | BindingFlags.NonPublic,
            null,
            Type.EmptyTypes,
            null)!;

        var evt = (AndonEvent)ctor.Invoke(null);

        SetProperty(evt, "Id", id);
        SetProperty(evt, "EventType", eventType);
        SetProperty(evt, "Level", level);
        SetProperty(evt, "Title", title);
        SetProperty(evt, "Description", description);
        SetProperty(evt, "WorkstationId", workstationId);
        SetProperty(evt, "WorkstationName", workstationName);
        SetProperty(evt, "WorkOrderId", workOrderId);
        SetProperty(evt, "WorkOrderNo", workOrderNo);
        SetProperty(evt, "TriggeredById", triggeredById);
        SetProperty(evt, "TriggeredByName", triggeredByName);
        SetProperty(evt, "TriggeredAt", triggeredAt ?? DateTime.UtcNow);
        SetProperty(evt, "ResolvedAt", resolvedAt);
        SetProperty(evt, "ResolvedById", resolvedById);
        SetProperty(evt, "ResolvedByName", resolvedByName);
        SetProperty(evt, "CreatedAt", DateTime.UtcNow);
        SetProperty(evt, "UpdatedAt", DateTime.UtcNow);

        return evt;
    }

    #endregion

    #region QcCheckpoint

    /// <summary>
    /// 使用反射直接创建 QcCheckpoint
    /// </summary>
    public static QcCheckpoint CreateQcCheckpointDirect(
        long id = 1,
        long stepId = 1,
        QcInspectionType checkType = QcInspectionType.FIRST,
        bool isMandatory = true,
        string? remark = null)
    {
        var ctor = typeof(QcCheckpoint).GetConstructor(
            BindingFlags.Instance | BindingFlags.NonPublic,
            null,
            Type.EmptyTypes,
            null)!;

        var checkpoint = (QcCheckpoint)ctor.Invoke(null);

        SetProperty(checkpoint, "Id", id);
        SetProperty(checkpoint, "StepId", stepId);
        SetProperty(checkpoint, "CheckType", checkType);
        SetProperty(checkpoint, "IsMandatory", isMandatory);
        SetProperty(checkpoint, "Remark", remark);
        SetProperty(checkpoint, "CreatedAt", DateTime.UtcNow);
        SetProperty(checkpoint, "UpdatedAt", DateTime.UtcNow);

        return checkpoint;
    }

    #endregion

    #region User

    /// <summary>
    /// 创建 User
    /// </summary>
    public static User CreateUser(
        string username = "testuser",
        string displayName = "Test User",
        string? passwordHash = null,
        string? email = null,
        bool status = true)
    {
        return User.Create(
            username: username,
            displayName: displayName,
            passwordHash: passwordHash,
            email: email,
            status: status
        );
    }

    /// <summary>
    /// 使用反射直接创建 User
    /// </summary>
    public static User CreateUserDirect(
        long id = 1,
        string username = "testuser",
        string displayName = "Test User",
        string? passwordHash = null,
        string? email = null,
        bool status = true)
    {
        var ctor = typeof(User).GetConstructor(
            BindingFlags.Instance | BindingFlags.NonPublic,
            null,
            Type.EmptyTypes,
            null)!;

        var user = (User)ctor.Invoke(null);

        SetProperty(user, "Id", id);
        SetProperty(user, "Username", username);
        SetProperty(user, "DisplayName", displayName);
        SetProperty(user, "PasswordHash", passwordHash ?? string.Empty);
        SetProperty(user, "Email", email);
        SetProperty(user, "Phone", null);
        SetProperty(user, "Status", status);
        SetProperty(user, "LastLoginTime", null);
        SetProperty(user, "IsDeleted", false);
        SetProperty(user, "CreatedAt", DateTime.UtcNow);
        SetProperty(user, "UpdatedAt", DateTime.UtcNow);

        return user;
    }

    #endregion

    #region Workstation

    /// <summary>
    /// 创建 Workstation
    /// </summary>
    public static Workstation CreateWorkstation(
        long lineId = 1,
        string code = "WS-001",
        string name = "Test Workstation",
        int seqNo = 1)
    {
        return Workstation.Create(lineId, code, name, seqNo);
    }

    /// <summary>
    /// 使用反射直接创建 Workstation
    /// </summary>
    public static Workstation CreateWorkstationDirect(
        long id = 1,
        long lineId = 1,
        string code = "WS-001",
        string name = "Test Workstation",
        int seqNo = 1,
        bool status = true)
    {
        var ctor = typeof(Workstation).GetConstructor(
            BindingFlags.Instance | BindingFlags.NonPublic,
            null,
            Type.EmptyTypes,
            null)!;

        var workstation = (Workstation)ctor.Invoke(null);

        SetProperty(workstation, "Id", id);
        SetProperty(workstation, "LineId", lineId);
        SetProperty(workstation, "Code", code);
        SetProperty(workstation, "Name", name);
        SetProperty(workstation, "SeqNo", seqNo);
        SetProperty(workstation, "Status", status);
        SetProperty(workstation, "CreatedAt", DateTime.UtcNow);
        SetProperty(workstation, "UpdatedAt", DateTime.UtcNow);

        return workstation;
    }

    #endregion

    #region MaterialTrace

    /// <summary>
    /// 使用反射直接创建 MaterialTrace
    /// </summary>
    public static MaterialTrace CreateMaterialTraceDirect(
        long id = 1,
        long materialId = 1,
        string? batchNo = null,
        long? workOrderId = null,
        string? direction = null,
        decimal qty = 0)
    {
        var ctor = typeof(MaterialTrace).GetConstructor(
            BindingFlags.Instance | BindingFlags.NonPublic,
            null,
            Type.EmptyTypes,
            null)!;

        var trace = (MaterialTrace)ctor.Invoke(null);

        SetProperty(trace, "Id", id);
        SetProperty(trace, "MaterialId", materialId);
        SetProperty(trace, "BatchNo", batchNo);
        SetProperty(trace, "WorkOrderId", workOrderId);
        SetProperty(trace, "Direction", direction);
        SetProperty(trace, "Qty", qty);
        SetProperty(trace, "OperateTime", DateTime.UtcNow);

        return trace;
    }

    #endregion

    #region WorkReport (using factory method)

    /// <summary>
    /// 使用工厂方法创建 WorkReport
    /// </summary>
    public static WorkReport CreateWorkReport(
        long workOrderId = 1,
        ReportType reportType = ReportType.COMPLETE,
        decimal goodQty = 10,
        decimal scrapQty = 0,
        decimal reworkQty = 0,
        long? stepId = null,
        long? workstationId = null)
    {
        return WorkReport.Create(
            workOrderId: workOrderId,
            reportType: reportType,
            goodQty: new Quantity(goodQty),
            scrapQty: new Quantity(scrapQty),
            reworkQty: new Quantity(reworkQty),
            stepId: stepId,
            workstationId: workstationId
        );
    }

    #endregion

    #region AlertRecord

    /// <summary>
    /// 使用反射直接创建 AlertRecord
    /// </summary>
    public static AlertRecord CreateAlertRecordDirect(
        long id = 1,
        string title = "Test Alert",
        string message = "Test message",
        AlertLevel level = AlertLevel.Low,
        bool isProcessed = false)
    {
        var ctor = typeof(AlertRecord).GetConstructor(
            BindingFlags.Instance | BindingFlags.NonPublic,
            null,
            Type.EmptyTypes,
            null)!;

        var alert = (AlertRecord)ctor.Invoke(null);

        SetProperty(alert, "Id", id);
        SetProperty(alert, "Title", title);
        SetProperty(alert, "Message", message);
        SetProperty(alert, "Level", level);
        SetProperty(alert, "IsProcessed", isProcessed);

        return alert;
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// 使用反射设置属性值（即使是 private set）
    /// </summary>
    public static void SetProperty(object obj, string propertyName, object? value)
    {
        var type = obj.GetType();
        var property = type.GetProperty(propertyName,
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException($"Property {propertyName} not found on {type.Name}");

        var backingField = type.GetField($"<{propertyName}>k__BackingField",
            BindingFlags.NonPublic | BindingFlags.Instance);

        if (backingField != null)
        {
            backingField.SetValue(obj, value);
        }
        else
        {
            var setMethod = property.GetSetMethod(true); // true = non-public
            if (setMethod == null)
                throw new InvalidOperationException($"No setter found for property {propertyName}");
            setMethod.Invoke(obj, new[] { value });
        }
    }

    /// <summary>
    /// 使用反射获��属性值
    /// </summary>
    public static T? GetProperty<T>(object obj, string propertyName)
    {
        var type = obj.GetType();
        var property = type.GetProperty(propertyName,
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException($"Property {propertyName} not found on {type.Name}");

        var getMethod = property.GetGetMethod(true);
        if (getMethod == null)
            throw new InvalidOperationException($"No getter found for property {propertyName}");

        return (T?)getMethod.Invoke(obj, null);
    }

    #endregion
}
