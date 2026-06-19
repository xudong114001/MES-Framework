import { createApp } from "vue";
import ElementPlus from "element-plus";
import "element-plus/dist/index.css";
import App from "./App.vue";
import router from "./router";
import { createPinia } from "pinia";
import * as ElementPlusIconsVue from "@element-plus/icons-vue";
import { echartsPlugin } from "./plugins/echarts";
import { vRole, vPermission } from "./directives/role";

const app = createApp(App);
app.use(ElementPlus);
app.use(createPinia());
app.use(router);
app.use(echartsPlugin);

// 注册所有图标
for (const [key, component] of Object.entries(ElementPlusIconsVue)) {
  app.component(key, component);
}

// 注册自定义指令
app.directive("role", vRole);
app.directive("permission", vPermission);

app.mount("#app");
