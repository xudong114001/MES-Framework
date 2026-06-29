<template>
  <div class="pda-container">
    <div class="pda-header">
      <h1>📦 PDA 扫码报工</h1>
    </div>

    <div class="pda-body">
      <!-- 扫码输入区 -->
      <div class="scan-section">
        <div class="input-card">
          <div class="input-icon">📋</div>
          <div class="input-label">工单号</div>
          <input
            id="scanCodeInput"
            :ref="(el: any) => { scanCodeRef = el }"
            v-model="form.scanCode"
            class="pda-input"
            placeholder="扫描或输入工单号"
            @keydown.enter="focusById('stepSelectInput')"
          />
        </div>

        <div class="input-card">
          <div class="input-icon">⚙️</div>
          <div class="input-label">工序</div>
          <select
            id="stepSelectInput"
            :ref="(el: any) => { stepSelectRef = el }"
            v-model="form.stepName"
            class="pda-input pda-select"
            @change="focusById('workstationInput')"
          >
            <option value="" disabled>选择工序</option>
            <option v-for="s in stepOptions" :key="s" :value="s">{{ s }}</option>
          </select>
        </div>

        <div class="input-card">
          <div class="input-icon">📍</div>
          <div class="input-label">工位码</div>
          <input
            id="workstationInput"
            :ref="(el: any) => { workstationRef = el }"
            v-model="form.workstationCode"
            class="pda-input"
            placeholder="扫描工位编码"
            @keydown.enter="focusById('operatorInput')"
          />
        </div>

        <div class="input-card">
          <div class="input-icon">👤</div>
          <div class="input-label">操作工</div>
          <input
            id="operatorInput"
            :ref="(el: any) => { operatorRef = el }"
            v-model="form.operatorCode"
            class="pda-input"
            placeholder="扫描操作工编码"
            @keydown.enter="focusById('goodQtyInput')"
          />
        </div>
      </div>

      <!-- 数量输入区 -->
      <div class="qty-section">
        <div class="qty-card qty-good">
          <div class="qty-icon">✅</div>
          <div class="qty-label">良品数</div>
          <div class="qty-controls">
            <button class="qty-btn qty-btn-minus" @click="adjust('goodQty', -1)">−</button>
            <input
              id="goodQtyInput"
              :ref="(el: any) => { goodQtyRef = el }"
              v-model.number="form.goodQty"
              class="qty-value"
              type="number"
              min="0"
              @keydown.enter="focusById('scrapQtyInput')"
            />
            <button class="qty-btn qty-btn-plus" @click="adjust('goodQty', 1)">+</button>
          </div>
        </div>

        <div class="qty-card qty-scrap">
          <div class="qty-icon">❌</div>
          <div class="qty-label">报废数</div>
          <div class="qty-controls">
            <button class="qty-btn qty-btn-minus" @click="adjust('scrapQty', -1)">−</button>
            <input
              id="scrapQtyInput"
              :ref="(el: any) => { scrapQtyRef = el }"
              v-model.number="form.scrapQty"
              class="qty-value"
              type="number"
              min="0"
              @keydown.enter="focusById('reworkQtyInput')"
            />
            <button class="qty-btn qty-btn-plus" @click="adjust('scrapQty', 1)">+</button>
          </div>
        </div>

        <div class="qty-card qty-rework">
          <div class="qty-icon">🔄</div>
          <div class="qty-label">返工数</div>
          <div class="qty-controls">
            <button class="qty-btn qty-btn-minus" @click="adjust('reworkQty', -1)">−</button>
            <input
              id="reworkQtyInput"
              :ref="(el: any) => { reworkQtyRef = el }"
              v-model.number="form.reworkQty"
              class="qty-value"
              type="number"
              min="0"
              @keydown.enter="handleSubmit"
            />
            <button class="qty-btn qty-btn-plus" @click="adjust('reworkQty', 1)">+</button>
          </div>
        </div>
      </div>

      <!-- 提交按钮 -->
      <button
        class="submit-btn"
        :disabled="submitting"
        @click="handleSubmit"
      >
        {{ submitting ? '提交中...' : '提交报工' }}
      </button>

      <!-- 错误提示 -->
      <div v-if="errorMsg" class="error-toast">{{ errorMsg }}</div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { workReportApi } from '../../api/work-report'
import { routingApi } from '../../api/routing'

const scanCodeRef = ref<any>(null)
const stepSelectRef = ref<any>(null)
const workstationRef = ref<any>(null)
const operatorRef = ref<any>(null)
const goodQtyRef = ref<any>(null)
const scrapQtyRef = ref<any>(null)
const reworkQtyRef = ref<any>(null)

const submitting = ref(false)
const errorMsg = ref('')

const stepOptions = ref<string[]>([])

async function loadStepOptions() {
  try {
    const res: any = await routingApi.list()
    if (res.data && Array.isArray(res.data)) {
      // 从所有工艺路线中提取步骤名称
      const steps = new Set<string>()
      res.data.forEach((r: any) => {
        if (r.steps && Array.isArray(r.steps)) {
          r.steps.forEach((s: any) => steps.add(s.stepName))
        }
      })
      stepOptions.value = Array.from(steps)
    }
  } catch {
    // 如果获取失败，使用默认选项
    stepOptions.value = ['SMT贴片', 'DIP插件', '波峰焊', '组装', '测试', '包装']
  }
}

loadStepOptions()

const form = reactive({
  scanCode: '',
  stepName: '',
  workstationCode: '',
  operatorCode: '',
  goodQty: 0,
  scrapQty: 0,
  reworkQty: 0
})

function focusById(id: string) {
  setTimeout(() => {
    const el = document.getElementById(id)
    el?.focus()
  }, 50)
}

function adjust(field: 'goodQty' | 'scrapQty' | 'reworkQty', delta: number) {
  const newVal = form[field] + delta
  form[field] = Math.max(0, newVal)
}

function resetForm() {
  form.scanCode = ''
  form.stepName = ''
  form.workstationCode = ''
  form.operatorCode = ''
  form.goodQty = 0
  form.scrapQty = 0
  form.reworkQty = 0
  errorMsg.value = ''
}

async function handleSubmit() {
  // 基础校验
  if (!form.scanCode.trim()) {
    errorMsg.value = '请扫描工单号'
    document.getElementById('scanCodeInput')?.focus()
    return
  }
  if (!form.stepName) {
    errorMsg.value = '请选择工序'
    document.getElementById('stepSelectInput')?.focus()
    return
  }
  if (!form.workstationCode.trim()) {
    errorMsg.value = '请扫描工位编码'
    document.getElementById('operatorInput')?.focus()
    return
  }
  if (!form.operatorCode.trim()) {
    errorMsg.value = '请扫描操作工编码'
    document.getElementById('goodQtyInput')?.focus()
    return
  }
  if (form.goodQty <= 0 && form.scrapQty <= 0 && form.reworkQty <= 0) {
    errorMsg.value = '请填写至少一个数量'
    document.getElementById('goodQtyInput')?.focus()
    return
  }

  submitting.value = true
  errorMsg.value = ''

  try {
    await workReportApi.pdaScan({
      scanCode: form.scanCode.trim(),
      stepName: form.stepName,
      workstationCode: form.workstationCode.trim(),
      operatorCode: form.operatorCode.trim(),
      goodQty: form.goodQty,
      scrapQty: form.scrapQty,
      reworkQty: form.reworkQty
    })
    ElMessage.success('✅ 报工成功')
    resetForm()
    setTimeout(() => document.getElementById('scanCodeInput')?.focus(), 100)
  } catch (err: any) {
    const msg = err?.response?.data?.message || err?.message || '报工提交失败'
    errorMsg.value = msg
    ElMessage.error(msg)
  } finally {
    submitting.value = false
  }
}

onMounted(() => {
  setTimeout(() => document.getElementById('scanCodeInput')?.focus(), 100)
})
</script>

<style scoped>
.pda-container {
  max-width: 520px;
  margin: 0 auto;
  padding: 16px;
  min-height: 100vh;
  background: #f5f7fa;
  font-size: 18px;
}

.pda-header {
  text-align: center;
  padding: 16px 0 8px;
}

.pda-header h1 {
  font-size: 26px;
  font-weight: 700;
  margin: 0;
  color: #303133;
}

.pda-body {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.scan-section {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.input-card {
  background: #fff;
  border-radius: 14px;
  padding: 14px 18px;
  display: flex;
  align-items: center;
  gap: 12px;
  box-shadow: 0 2px 8px rgba(0,0,0,0.06);
}

.input-icon {
  font-size: 28px;
  flex-shrink: 0;
}

.input-label {
  font-size: 17px;
  font-weight: 600;
  color: #606266;
  width: 64px;
  flex-shrink: 0;
}

.pda-input {
  flex: 1;
  height: 48px;
  border: 2px solid #dcdfe6;
  border-radius: 10px;
  padding: 0 14px;
  font-size: 18px;
  outline: none;
  transition: border-color 0.2s;
  background: #fafafa;
}

.pda-input:focus {
  border-color: #409eff;
  background: #fff;
}

.pda-select {
  appearance: auto;
  cursor: pointer;
  background: #fafafa;
}

/* 数量区域 */
.qty-section {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.qty-card {
  background: #fff;
  border-radius: 14px;
  padding: 12px 18px;
  display: flex;
  align-items: center;
  gap: 12px;
  box-shadow: 0 2px 8px rgba(0,0,0,0.06);
}

.qty-icon {
  font-size: 26px;
  flex-shrink: 0;
}

.qty-label {
  font-size: 17px;
  font-weight: 600;
  width: 64px;
  flex-shrink: 0;
}

.qty-controls {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 10px;
}

.qty-btn {
  width: 52px;
  height: 52px;
  border-radius: 12px;
  border: none;
  font-size: 28px;
  font-weight: 700;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: opacity 0.15s;
  -webkit-tap-highlight-color: transparent;
  user-select: none;
}

.qty-btn:active {
  opacity: 0.7;
}

.qty-btn-minus {
  background: #f56c6c;
  color: #fff;
}

.qty-btn-plus {
  background: #67c23a;
  color: #fff;
}

.qty-value {
  width: 90px;
  height: 52px;
  text-align: center;
  font-size: 26px;
  font-weight: 700;
  border: 2px solid #dcdfe6;
  border-radius: 10px;
  outline: none;
  background: #fafafa;
  -moz-appearance: textfield;
}

.qty-value::-webkit-inner-spin-button,
.qty-value::-webkit-outer-spin-button {
  -webkit-appearance: none;
  margin: 0;
}

.qty-value:focus {
  border-color: #409eff;
  background: #fff;
}

.qty-good .qty-label { color: #67c23a; }
.qty-scrap .qty-label { color: #f56c6c; }
.qty-rework .qty-label { color: #e6a23c; }
.qty-good .qty-value:focus { border-color: #67c23a; }
.qty-scrap .qty-value:focus { border-color: #f56c6c; }
.qty-rework .qty-value:focus { border-color: #e6a23c; }

/* 提交按钮 */
.submit-btn {
  width: 100%;
  height: 58px;
  border: none;
  border-radius: 14px;
  background: linear-gradient(135deg, #67c23a, #85ce61);
  color: #fff;
  font-size: 22px;
  font-weight: 700;
  cursor: pointer;
  transition: opacity 0.2s;
  letter-spacing: 2px;
  -webkit-tap-highlight-color: transparent;
}

.submit-btn:active {
  opacity: 0.85;
}

.submit-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

/* 错误提示 */
.error-toast {
  background: #fef0f0;
  color: #f56c6c;
  padding: 12px 16px;
  border-radius: 10px;
  font-size: 16px;
  text-align: center;
  border: 1px solid #fbc4c4;
}
</style>
