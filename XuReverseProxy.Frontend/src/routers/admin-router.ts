import { RouteRecordRaw, createRouter, createWebHashHistory } from 'vue-router'
import Admin404Page from '@pages/admin/Admin404Page.vue';
import AdminOverviewPage from '@pages/admin/AdminOverviewPage.vue';
import ProxyConfigsPage from '@pages/admin/ProxyConfigsPage.vue';
import ProxyConfigPage from '@pages/admin/ProxyConfigPage.vue';

const routes: Readonly<RouteRecordRaw[]> = [
    { name: "/", path: "/", component: AdminOverviewPage },
    { name: "proxyconfigs", path: "/proxyconfigs", component: ProxyConfigsPage },
    { name: "proxyconfig", path: "/proxyconfigs/:configId", component: ProxyConfigPage },
    { name: "404", path: "/:catchAll(.*)", component: Admin404Page },
]

const adminRouter = createRouter({
    history: createWebHashHistory(),
    routes: routes
  });
export default adminRouter;
