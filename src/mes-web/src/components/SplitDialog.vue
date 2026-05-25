<template>
  <el-dialog v-model="visible" title="拆分工单" width="400px" @close="handleClose">
    <el-form ref="formRef" :model="form" :rules="rules" label-width="100px">
      <el-form-item label="原工单号">
        <span>{{ workOrder?.orderNo }}</span>
      </el-form-item>
      <el-form-item label="原数量">
        <span>{{ workOrder?.plannedQty }}</span>
      </el-form-item>
      <el-form-item label="拆分数量" prop="splitQty">
        <el-input-number v-model="form.splitQty" :min="1" :max="(workOrder?.plannedQty || 1) - 1" style="width: 100%" />
      </el-form-item>
      <el-form-item label="拆分后余量">
        <span>{{ (workOrder?.plannedQty || 0) - form.splitQty }}</span>
      </el-form-item>
    </el-form>
    <template #footer>
      <el-button @click="visible = false">取消</el-button>
      <el-button type="primary" @click="handleSplit" :loading="submitting">确认拆分</el-button>
    </template>
  </el-dialog>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import { ElMessage } from 'element-plus'
import { workOrderApi } from '../api/work-order'
import type { WorkOrder } from '../api/work-order'

const props = defineProps<{
  visible: boolean
  workOrder: WorkOrder | null
}>()

const emit = defineEmits<{
  (e: 'update:visible', val: boolean): void
  (e: 'done'): void
}>()

const visible = ref(false)
const submitting = ref(false)
const formRef = ref()
const form = ref({ splitQty: 1 })

watch(() => props.visible, (v) => { visible.value = v })
watch(visible, (v) => emit('update:visible', v))

const rules = {
  splitQty: [
    { required: true, message: '请输入拆分数量', trigger: 'blur' },
    {
      validator: (_: any, value: number, callback: Function) => {
        if (!props.workOrder) return callback()
        if (value >= (props.workOrder.plannedQty || 1)) {
          callback(new Error('拆分数量必须小于原数量'))
        } else {
          callback()
        }
      },
      trigger: 'blur'
    }
  ]
}

function handleClose() {
  form.value.splitQty = 1
  visible.value = false
}

async function handleSplit() {
  const valid = await formRef.value?.validate().catch(() => false)
  if (!valid || !props.workOrder?.id) return
  submitting.value = true
  try {
    await workOrderApi.split(props.workOrder.id, form.value.splitQty)
    ElMessage.success('拆分成功')
    visible.value = false
    emit('done')
  } catch {
    // error already handled by interceptor
  } finally {
    submitting.value = false
  }
}
</script>
