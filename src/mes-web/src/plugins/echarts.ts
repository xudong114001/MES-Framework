import { type App } from "vue";
import { use } from "echarts/core";
import { CanvasRenderer } from "echarts/renderers";
import {
  BarChart,
  LineChart,
  PieChart,
  GaugeChart,
  RadarChart,
} from "echarts/charts";
import {
  TitleComponent,
  TooltipComponent,
  LegendComponent,
  GridComponent,
  DatasetComponent,
  TransformComponent,
  ToolboxComponent,
} from "echarts/components";

// 按需注册 ECharts 组件，减小打包体积
use([
  CanvasRenderer,
  BarChart,
  LineChart,
  PieChart,
  GaugeChart,
  RadarChart,
  TitleComponent,
  TooltipComponent,
  LegendComponent,
  GridComponent,
  DatasetComponent,
  TransformComponent,
  ToolboxComponent,
]);

/**
 * ECharts 插件：全局注册 vue-echarts 组件
 */
export const echartsPlugin = {
  install(app: App) {
    // vue-echarts 需要在 use() 注册后才能按需渲染
    // 这里异步导入 VChart 组件以避免 SSR 问题
    import("vue-echarts").then((module) => {
      app.component("VChart", module.default);
    });
  },
};
